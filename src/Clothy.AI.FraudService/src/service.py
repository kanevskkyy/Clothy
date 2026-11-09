"""FastAPI service. Run: uvicorn src.service:create_app --factory --port 8000
Settings come from environment variables or from the .env file in the repository root (see .env.example):
MODELS_DIR, MODEL_VARIANT (main|device), FLAG_RATE, STORE_BACKEND (memory|sqlite), DB_PATH, RING_SIGNAL (on|off),
LOG_LEVEL. Variables set in the terminal take precedence over .env. Relative paths start at the repository root.
Values are validated at startup (src/config.py). Transaction fields (card, email, address, device)
never reach the logs: only txid, error codes and error messages."""
from collections.abc import Mapping
from typing import Any

from fastapi import FastAPI, HTTPException, Query, Request
from fastapi.exception_handlers import request_validation_exception_handler
from fastapi.exceptions import RequestValidationError

try:
    from dotenv import load_dotenv
except ImportError:             # python-dotenv is not installed: use environment variables only
    load_dotenv = None

from src.config import ROOT, ConfigError, load_settings, setup_logging
from src.config import abs_path as _abs  # noqa: F401  (older tests import _abs from here)
from src.features import DuplicateTransactionError, OutOfOrderError
from src.schemas import (
    VALIDATION_422,
    DecisionsResponse,
    HealthResponse,
    Label,
    LabelResponse,
    RingAlertsResponse,
    ScoreResponse,
    Tx,
    TxRecord,
    error,
)
from src.scoring import Scorer

API_DESCRIPTION = """
Demo service from the graph-antifraud coursework: fraud risk scoring for e-commerce transactions.

Flow: `POST /score` computes features from history (card, card+address/email pairs, neighbour labels with a
7-day delay in a 30-day window, optionally the device fingerprint) → XGBoost → validation threshold → decision.
A dispute (the payment provider's dispute webhook) is sent to `POST /labels` and affects features of later transactions.

Demo data is synthetic; the model is trained on IEEE-CIS and must be retrained for a real store.
"""

TAGS = [
    {"name": "scoring", "description": "Transaction scoring."},
    {"name": "labels", "description": "Labels from dispute outcomes (like a dispute webhook)."},
    {"name": "history", "description": "Stored decisions and ring signal alerts (STORE_BACKEND=sqlite only)."},
    {"name": "service", "description": "Service status."},
]

E404 = error("Unknown txid: the transaction was not scored with store=true.", "unknown transaction")
E501 = error("This endpoint requires STORE_BACKEND=sqlite (decisions are not stored in memory).",
             "STORE_BACKEND=sqlite required")


def _dump(m: Any) -> dict[str, Any]:
    """Pydantic model -> dict (v2 and v1)."""
    return m.model_dump() if hasattr(m, "model_dump") else m.dict()


def create_app(models_dir: str | None = None, variant: str | None = None, flag_rate: float | None = None,
               backend: str | None = None, db_path: str | None = None,
               ring_params: Mapping[str, Any] | bool | None = None) -> FastAPI:
    """Creates the FastAPI app.

    Arguments not given explicitly are taken from environment variables or .env
    (MODELS_DIR, MODEL_VARIANT, FLAG_RATE, STORE_BACKEND, DB_PATH, RING_SIGNAL, LOG_LEVEL).

    Returns:
        The app; the Scorer is available as app.state.scorer.

    Raises:
        ConfigError: invalid setting (the message explains what to fix).
        ValueError: model files were changed or do not match the feature code.
    """
    if load_dotenv is not None:
        load_dotenv(ROOT / ".env")
    log = setup_logging("INFO")
    try:
        st = load_settings(models_dir, variant, flag_rate, backend, db_path, ring_params)
    except ConfigError as e:
        log.error("configuration error: %s", e)
        raise
    log.setLevel(st.log_level)
    try:
        scorer = Scorer(st.models_dir, st.variant, st.flag_rate, st.backend, st.db_path, st.ring_params)
    except Exception as e:
        log.error("failed to load model: %s", e)
        raise
    log.info("startup: variant=%s n_features=%d flag_rate=%s threshold=%.6f backend=%s db=%s ring=%s models=%s",
             scorer.variant, len(scorer.order), scorer.flag_rate, scorer.threshold, scorer.backend,
             st.db_path if st.backend == "sqlite" else "-", scorer.ring or "off", st.models_dir)
    if st.backend == "sqlite":
        log.info("SQLite history: %d transactions", scorer.store.n_transactions)
    app = FastAPI(title="graph-antifraud demo service", version="0.3", description=API_DESCRIPTION,
                  openapi_tags=TAGS)
    app.state.scorer = scorer

    @app.exception_handler(RequestValidationError)
    async def log_validation_error(request: Request, exc: RequestValidationError):
        # log only the path and error locations, never request field values
        locs = [".".join(str(x) for x in err.get("loc", ())) for err in exc.errors()]
        log.warning("422 %s: invalid request, fields: %s", request.url.path, ", ".join(locs))
        return await request_validation_exception_handler(request, exc)

    @app.get("/health", tags=["service"], response_model=HealthResponse,
             summary="Service status and configuration",
             description="Active model, store, threshold, ring signal parameters and history size.")
    def health():
        with scorer._lock:
            return {"status": "ok", "variant": scorer.variant, "backend": scorer.backend,
                    "n_features": len(scorer.order), "n_transactions": scorer.store.n_transactions,
                    "flag_rate_val": scorer.flag_rate, "threshold": scorer.threshold, "ring": scorer.ring}

    @app.post("/score", tags=["scoring"], response_model=ScoreResponse, response_model_exclude_unset=True,
              summary="Score a transaction",
              description="Computes features only from transactions STRICTLY before `ts` and neighbour labels "
                          "known at `ts`; returns the probability, the threshold decision, the three largest feature "
                          "contributions and the ring signal. Required: `txid`, `ts`, `TransactionAmt`, `card1`; "
                          "the rest are optional (a missing or unknown category value = NaN). "
                          "With `store=true` the transaction is saved to history.",
              responses={409: error("Duplicate txid (the transaction is already stored) or ts earlier than the last "
                                    "stored transaction (the stream must be in non-decreasing time order). "
                                    "Only with store=true.", "transaction a1 is already stored"),
                         422: VALIDATION_422})
    def score(tx: Tx,
              store: bool = Query(True, description="Save the transaction to history (and the decision to SQLite)."),
              debug: bool = Query(False, description="Add all features to the response (the `features` field)."),
              explain: bool = Query(True, description="Compute the three largest contributions (adds about 15 ms).")):
        try:
            return scorer.score(tx.txid, _dump(tx), store=store, debug=debug, explain=explain)
        except (DuplicateTransactionError, OutOfOrderError) as e:
            log.warning("409 /score txid=%s: %s", tx.txid, e)
            raise HTTPException(status_code=409, detail=str(e)) from e
        except ValueError as e:
            log.warning("422 /score txid=%s: %s", tx.txid, e)
            raise HTTPException(status_code=422, detail=str(e)) from e

    @app.post("/labels", tags=["labels"], response_model=LabelResponse,
              summary="Record a dispute label",
              description="Like a dispute webhook: marks the transaction as fraud (`fraud=true`) or removes "
                          "the label (`fraud=false`). The label affects u2/u3/fp features of later transactions only "
                          "when 7 to 30 days have passed since the transaction and `at` is not later than their "
                          "`ts`. Without `at`, the ts of the last stored transaction is used; in a real integration "
                          "pass the dispute time from Stripe.",
              responses={404: E404})
    def labels(label: Label):
        try:
            scorer.set_label(label.txid, label.fraud, label.at)
        except KeyError as e:
            log.warning("404 /labels txid=%s: unknown transaction", label.txid)
            raise HTTPException(status_code=404, detail="unknown transaction") from e
        return {"ok": True}

    @app.get("/tx/{txid}", tags=["history"], response_model=TxRecord,
             summary="Transaction, decision and label",
             description="Returns the stored transaction, model decision, label time and ring signal alert.",
             responses={404: E404, 501: E501})
    def get_tx(txid: str):
        if not hasattr(scorer.store, "get_tx"):
            raise HTTPException(status_code=501, detail="STORE_BACKEND=sqlite required")
        with scorer._lock:
            rec = scorer.store.get_tx(txid)
        if rec is None:
            log.warning("404 /tx txid=%s: unknown transaction", txid)
            raise HTTPException(status_code=404, detail="unknown transaction")
        return rec

    @app.get("/rings", tags=["history"], response_model=RingAlertsResponse,
             summary="Recent ring signal alerts",
             description="Transactions that triggered the ring signal, newest first.",
             responses={501: E501})
    def rings(limit: int = Query(20, description="How many records to return.")):
        if not hasattr(scorer.store, "recent_ring_alerts"):
            raise HTTPException(status_code=501, detail="STORE_BACKEND=sqlite required")
        with scorer._lock:
            return {"alerts": scorer.store.recent_ring_alerts(limit)}

    @app.get("/decisions", tags=["history"], response_model=DecisionsResponse,
             summary="Stored decisions (Clothy)",
             description="Recent model decisions with the top three reasons and the ring signal, newest "
                         "first. `flagged=true` returns only transactions that need review, `flagged=false` only "
                         "the rest; without the parameter all are returned.",
             responses={501: E501})
    def decisions(flagged: bool | None = Query(None, description="Filter by decision (flag)."),
                  limit: int = Query(50, ge=1, le=500, description="How many records to return (1..500).")):
        if not hasattr(scorer.store, "recent_decisions"):
            raise HTTPException(status_code=501, detail="STORE_BACKEND=sqlite required")
        with scorer._lock:
            return {"items": scorer.store.recent_decisions(flagged, limit)}

    return app
