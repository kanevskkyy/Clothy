"""Скорер: ознаки -> XGBoost -> ймовірність, прапорець за порогом із валідації, топ-3 внески, сигнал кільця."""
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
    """False = вимкнено; dict = явні параметри; None = із models/ring_signal.yaml (якщо є й прийнятий)"""
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
    """Ознаки зі сховища -> XGBoost -> ймовірність, рішення за порогом із валідації, топ-3 внески, сигнал кільця.

    Під час створення перевіряє sha256 моделі й кодувань, відповідність набору ознак коду features.py
    і кількість ознак у моделі. Методи score і set_label потокобезпечні (спільний RLock).

    Args:
        models_dir: тека з service_config.yaml, моделями й encoders.json.
        variant: "main" (34 ознаки) або "device" (41 ознака, з блоком пристрою).
        flag_rate: частка позначених на валідації; має бути серед thresholds_from_validation.
        backend: "memory" (FeatureStore) або "sqlite" (SqlFeatureStore).
        db_path: шлях до бази SQLite або ":memory:" (лише для backend="sqlite").
        ring_params: None = з models/ring_signal.yaml (якщо є й прийнятий), False = вимкнено,
            dict {"k", "s", "warmup_days"} = явні параметри.

    Raises:
        ValueError: невідомий variant/backend/flag_rate, змінені файли моделей або розбіжність ознак.
    """

    def __init__(self, models_dir: str = "models", variant: str = "main", flag_rate: float = 0.01,
                 backend: str = "memory", db_path: str = ":memory:",
                 ring_params: Mapping[str, Any] | bool | None = None) -> None:
        if variant not in ("main", "device"):
            raise ValueError("variant має бути 'main' або 'device'")
        if backend not in ("memory", "sqlite"):
            raise ValueError("backend має бути 'memory' або 'sqlite'")
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
            raise ValueError("набір ознак у конфігурації не збігається з кодом features.py")

        for name in (os.path.basename(cfg["models"][variant]["file"]), "encoders.json"):
            if cfg["sha256"][f"models/{name}"] != _sha(os.path.join(models_dir, name)):
                raise ValueError(f"sha256 не збігається для {name}: файл змінено")
        with open(os.path.join(models_dir, "encoders.json"), encoding="utf-8") as f:
            self.encoders = json.load(f)

        thr_map = cfg["thresholds_from_validation"][variant]
        key = str(float(flag_rate))
        if key not in thr_map:
            raise ValueError(f"flag_rate {flag_rate} недоступний, є: {sorted(thr_map)}")
        self.threshold = float(thr_map[key])

        self.booster = xgb.Booster()
        self.booster.load_model(os.path.join(models_dir, os.path.basename(cfg["models"][variant]["file"])))
        try:
            self.booster.set_param({"device": "cpu"})
        except Exception:
            pass
        if self.booster.num_features() != len(self.order):
            raise ValueError(f"модель має {self.booster.num_features()} ознак, конфігурація {len(self.order)}")

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
                else float(self.encoders[c].get(str(v), NAN))   # невідоме значення = пропущене
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
        """Оцінює транзакцію.

        Args:
            txid: ідентифікатор транзакції.
            req: поля транзакції (назви IEEE-CIS).
            store: зберегти транзакцію в історії (і рішення та спрацювання кільця в SQLite).
            debug: додати до відповіді всі ознаки (ключ "features").
            explain: рахувати три найбільші внески ознак (інакше top_features = []).

        Returns:
            {"txid", "score", "flag", "threshold", "flag_rate_val", "variant", "ring", "top_features"}
            і "features" при debug=True; ring = None, якщо сигнал кільця вимкнено.

        Raises:
            ValueError: неповний запит.
            DuplicateTransactionError, OutOfOrderError: лише при store=True.
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
        """Записує мітку спору; без at береться ts останньої збереженої транзакції.

        Raises:
            KeyError: txid невідомий.
        """
        with self._lock:
            self.store.set_label(txid, fraud, self.store.last_ts if at is None else at)
