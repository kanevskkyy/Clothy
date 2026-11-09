"""Scorer: features -> XGBoost -> probability, flag by the validation threshold, top-3 contributions, ring signal."""
import hashlib
import json
import math
import os
import threading
from collections.abc import Mapping
from typing import Any

import numpy as np
import xgboost as xgb
import yaml

from src.features import DAY, DEVICE_NAMES, HISTORY_NAMES, NAN, FeatureStore, normalize_request
from src.ring import ring_signal
from src.sqlstore import SqlFeatureStore


def _sha(path: str) -> str:
    with open(path, "rb") as f:
        return hashlib.sha256(f.read()).hexdigest()


def _clean(v: Any) -> float | None:
    return None if (v is None or (isinstance(v, float) and math.isnan(v))) else float(v)


def _load_ring(models_dir: str, ring_params: Mapping[str, Any] | bool | None) -> dict[str, float] | None:
    """False = off; dict = explicit parameters; None = from models/ring_signal.yaml (if present and accepted)"""
    if ring_params is False:
        return None
    if ring_params is not None:
        return {"k": int(ring_params["k"]), "s": float(ring_params["s"]),
                "warmup_days": float(ring_params["warmup_days"])}
    path = os.path.join(models_dir, "ring_signal.yaml")
    if not os.path.exists(path):
        return None
    with open(path, encoding="utf-8") as f:
        c = yaml.safe_load(f)
    if not c.get("accepted", True):
        return None
    return {"k": int(c["k"]), "s": float(c["s"]), "warmup_days": float(c["warmup_days"])}


class Scorer:
    """Store features -> XGBoost -> probability, validation-threshold decision, top-3 contributions, ring signal.

    On creation it checks the sha256 of the model and encoders, that the feature set matches features.py
    and the number of features in the model. score and set_label are thread-safe (a shared RLock).

    Args:
        models_dir: folder with service_config.yaml, the models and encoders.json.
        variant: "main" (34 features) or "device" (41 features, with the device block).
        flag_rate: flagged share on validation; must be one of thresholds_from_validation.
        backend: "memory" (FeatureStore) or "sqlite" (SqlFeatureStore).
        db_path: path to the SQLite database or ":memory:" (backend="sqlite" only).
        ring_params: None = from models/ring_signal.yaml (if present and accepted), False = off,
            dict {"k", "s", "warmup_days"} = explicit parameters.

    Raises:
        ValueError: unknown variant/backend/flag_rate, changed model files or a feature mismatch.
    """

    def __init__(self, models_dir: str = "models", variant: str = "main", flag_rate: float = 0.01,
                 backend: str = "memory", db_path: str = ":memory:",
                 ring_params: Mapping[str, Any] | bool | None = None) -> None:
        if variant not in ("main", "device"):
            raise ValueError("variant must be 'main' or 'device'")
        if backend not in ("memory", "sqlite"):
            raise ValueError("backend must be 'memory' or 'sqlite'")
        with open(os.path.join(models_dir, "service_config.yaml"), encoding="utf-8") as f:
            cfg = yaml.safe_load(f)
        self.cfg, self.variant, self.flag_rate = cfg, variant, float(flag_rate)
        self.num_cols, self.cat_cols = cfg["numeric_columns"], cfg["categorical_columns"]
        base = list(cfg["features_main"])
        self.order = base if variant == "main" else base + list(cfg["features_device_extra"])

        expected = set(self.num_cols) | set(self.cat_cols) | {"hour", "weekday"} | set(HISTORY_NAMES)
        if variant == "device":
            expected |= set(DEVICE_NAMES)
        if set(self.order) != expected or len(self.order) != len(expected):
            raise ValueError("the feature set in the config does not match features.py")

        for name in (os.path.basename(cfg["models"][variant]["file"]), "encoders.json"):
            if cfg["sha256"][f"models/{name}"] != _sha(os.path.join(models_dir, name)):
                raise ValueError(f"sha256 mismatch for {name}: the file was changed")
        with open(os.path.join(models_dir, "encoders.json"), encoding="utf-8") as f:
            self.encoders = json.load(f)

        thr_map = cfg["thresholds_from_validation"][variant]
        key = str(float(flag_rate))
        if key not in thr_map:
            raise ValueError(f"flag_rate {flag_rate} is not available, options: {sorted(thr_map)}")
        self.threshold = float(thr_map[key])

        self.booster = xgb.Booster()
        self.booster.load_model(os.path.join(models_dir, os.path.basename(cfg["models"][variant]["file"])))
        try:
            self.booster.set_param({"device": "cpu"})
        except Exception:
            pass
        if self.booster.num_features() != len(self.order):
            raise ValueError(f"the model has {self.booster.num_features()} features, the config {len(self.order)}")

        self.ring = _load_ring(models_dir, ring_params)
        self.backend = backend
        self.store = FeatureStore() if backend == "memory" else SqlFeatureStore(db_path)
        self._lock = threading.RLock()

    def _request_part(self, req: Mapping[str, Any]) -> dict[str, float]:
        f = {}
        for c in self.num_cols:
            v = req.get(c)
            f[c] = NAN if (v is None or (isinstance(v, float) and math.isnan(v))) else float(v)
        for c in self.cat_cols:
            v = req.get(c)
            f[c] = NAN if (v is None or (isinstance(v, float) and math.isnan(v))) \
                else float(self.encoders[c].get(str(v), NAN))   # unknown value = missing
        ts = int(req["ts"])
        f["hour"] = float((ts // 3600) % 24)
        f["weekday"] = float((ts // 86400) % 7)
        return f

    def _predict(self, x: np.ndarray) -> float:
        try:
            return float(self.booster.inplace_predict(x)[0])
        except Exception:
            return float(self.booster.predict(xgb.DMatrix(x))[0])

    def _explain(self, x: np.ndarray, feats: Mapping[str, float]) -> list[dict[str, Any]]:
        c = self.booster.predict(xgb.DMatrix(x), pred_contribs=True)[0][:-1]
        idx = np.argsort(-np.abs(c))[:3]
        return [{"feature": self.order[i], "value": _clean(feats[self.order[i]]),
                 "contribution": float(c[i])} for i in idx]

    def score(self, txid: str, req: Mapping[str, Any], store: bool = True, debug: bool = False,
              explain: bool = True) -> dict[str, Any]:
        """Scores a transaction.

        Args:
            txid: transaction ID.
            req: transaction fields (IEEE-CIS names).
            store: save the transaction to history (and the decision and ring alert to SQLite).
            debug: add all features to the response (the "features" key).
            explain: compute the three largest feature contributions (otherwise top_features = []).

        Returns:
            {"txid", "score", "flag", "threshold", "flag_rate_val", "variant", "ring", "top_features"}
            and "features" with debug=True; ring = None if the ring signal is off.

        Raises:
            ValueError: incomplete request.
            DuplicateTransactionError, OutOfOrderError: only with store=True.
        """
        with self._lock:
            normalize_request(req)
            if store:
                self.store.check(txid, req)
            feats = self._request_part(req)
            feats.update(self.store.history_features(req))
            dfeats = self.store.device_features(req) if (self.variant == "device" or self.ring is not None) else None
            if self.variant == "device":
                feats.update(dfeats)
            x = np.array([[feats[k] for k in self.order]], dtype=np.float32)
            p = self._predict(x)
            flag = bool(p >= self.threshold)
            ring = None
            if self.ring is not None:
                first = self.store.first_ts
                hist_days = 0.0 if first is None else max(0.0, (int(req["ts"]) - first) / DAY)
                ring = ring_signal(dfeats, self.ring, hist_days)
            out = {"txid": txid, "score": p, "flag": flag, "threshold": self.threshold,
                   "flag_rate_val": self.flag_rate, "variant": self.variant, "ring": ring,
                   "top_features": self._explain(x, feats) if explain else []}
            if debug:
                out["features"] = {k: _clean(feats[k]) for k in self.order}
            if store:
                self.store.add(txid, req)
                if hasattr(self.store, "log_decision"):
                    self.store.log_decision(txid, int(req["ts"]), self.variant, p, flag, self.threshold,
                                            out["top_features"] if explain else None)
                    if ring is not None and ring["suspect"]:
                        self.store.log_ring(txid, int(req["ts"]), ring["other_cards_30d"], ring["share_new"])
            return out

    def set_label(self, txid: str, fraud: bool = True, at: float | None = None) -> None:
        """Records a dispute label; without at, the ts of the last stored transaction is used.

        Raises:
            KeyError: unknown txid.
        """
        with self._lock:
            self.store.set_label(txid, fraud, self.store.last_ts if at is None else at)
