"""Pydantic request and response schemas for the HTTP API (the /docs page).

Schemas only describe and validate data: the Scorer does the computation (src/scoring.py).
Response examples come from a real call to the service with the request example below.
"""

from typing import Any

from pydantic import BaseModel, ConfigDict, Field

TX_EXAMPLE = {
    "txid": "a1", "ts": 1_000_000, "TransactionAmt": 49.0, "card1": 15000.0, "ProductCD": "W",
    "card4": "visa", "card6": "debit", "addr1": 299.0, "addr2": 87.0, "P_emaildomain": "gmail.com",
}

# ---------- requests ----------


class Tx(BaseModel):
    """Transaction to score. Field names and values follow the IEEE-CIS Fraud Detection schema."""

    model_config = ConfigDict(json_schema_extra={"examples": [TX_EXAMPLE]})

    txid: str = Field(description="Unique transaction ID. A repeated txid returns 409.")
    ts: int = Field(description="Transaction time in seconds (like TransactionDT). Transactions must arrive "
                                "in non-decreasing order: one earlier than the last stored returns 409. "
                                "The hour and weekday features are also computed from ts.")
    TransactionAmt: float = Field(description="Transaction amount (in US dollars in IEEE-CIS).")
    card1: float = Field(description="Card ID. History key: 1/7/30-day windows, pairs with address and email, "
                                     "neighbour labels.")
    ProductCD: str | None = Field(None, description="Product type code (W, C, R, H, S).")
    card2: float | None = Field(None, description="Numeric card attribute (anonymized).")
    card3: float | None = Field(None, description="Numeric card attribute (anonymized).")
    card4: str | None = Field(None, description="Card network (visa, mastercard, ...).")
    card5: float | None = Field(None, description="Numeric card attribute (anonymized).")
    card6: str | None = Field(None, description="Card type (debit, credit, ...).")
    addr1: float | None = Field(None, description="Billing address region. Together with card1 forms the u2 key.")
    addr2: float | None = Field(None, description="Billing address country.")
    dist1: float | None = Field(None, description="Distance (anonymized IEEE-CIS value).")
    dist2: float | None = Field(None, description="Distance (anonymized IEEE-CIS value).")
    P_emaildomain: str | None = Field(None, description="Buyer email domain. Together with card1 and addr1 "
                                                           "forms the u3 key.")
    R_emaildomain: str | None = Field(None, description="Recipient email domain.")
    DeviceType: str | None = Field(None, description="Device type (desktop, mobile).")
    DeviceInfo: str | None = Field(None, description="Device model or OS. Part of the device fingerprint.")
    id_30: str | None = Field(None, description="Operating system. Part of the device fingerprint.")
    id_31: str | None = Field(None, description="Browser. Part of the device fingerprint.")
    id_33: str | None = Field(None, description="Screen resolution. Part of the device fingerprint. "
                                                   "The fingerprint (and the ring signal) exists only when all "
                                                   "four fields are set: DeviceInfo, id_30, id_31, id_33.")


class Label(BaseModel):
    """Label from a dispute outcome (like the payment provider's dispute webhook)."""

    model_config = ConfigDict(json_schema_extra={"examples": [{"txid": "a1", "fraud": True, "at": 1_604_800}]})

    txid: str = Field(description="The disputed transaction. An unknown txid returns 404.")
    fraud: bool = Field(True, description="true: fraud confirmed; false: remove the label.")
    at: int | None = Field(None, description="Time the label was received, in seconds (same scale as ts). "
                                                "Neighbour features see the label only when at <= ts of the new "
                                                "transaction. If not set, the ts of the last stored "
                                                "transaction is used (not the wall clock). In a real integration "
                                                "pass the dispute creation time from Stripe (dispute.created).")


# ---------- responses ----------


class TopFeature(BaseModel):
    """One of the three largest (by absolute value) feature contributions to the score (XGBoost SHAP contributions)."""

    feature: str = Field(description="Feature name.")
    value: float | None = Field(description="Feature value; null if missing.")
    contribution: float = Field(description="Contribution to the logit: positive raises risk, negative lowers it.")


class RingSignal(BaseModel):
    """Transparent ring signal: many other cards on the same device fingerprint, mostly new ones."""

    suspect: bool = Field(description="true if other_cards_30d >= k and share_new >= s and the signal is active.")
    active: bool = Field(description="Whether the store has at least warmup_days days of history.")
    other_cards_30d: int | None = Field(description="Transactions by other cards on this fingerprint in 30 days; "
                                                       "null if there is no fingerprint.")
    share_new: float | None = Field(description="Share of first card appearances among them; null if there are none.")
    k: int = Field(description="other_cards_30d threshold.")
    s: float = Field(description="share_new threshold.")


class ScoreResponse(BaseModel):
    """Decision for a transaction."""

    model_config = ConfigDict(json_schema_extra={"examples": [{
        "txid": "a1", "score": 0.01439689565449953, "flag": False, "threshold": 0.4008151590824127,
        "flag_rate_val": 0.01, "variant": "main",
        "ring": {"suspect": False, "active": False, "other_cards_30d": None, "share_new": None, "k": 2, "s": 0.5},
        "top_features": [
            {"feature": "card5", "value": None, "contribution": 0.2589011788368225},
            {"feature": "pair_addr_n30", "value": 0.0, "contribution": -0.24073068797588348},
            {"feature": "u2_fk", "value": 0.0, "contribution": -0.23515567183494568}],
    }]})

    txid: str
    score: float = Field(description="Fraud probability from the XGBoost model (0..1).")
    flag: bool = Field(description="score >= threshold: the transaction should be reviewed.")
    threshold: float = Field(description="Threshold tuned on validation for the flag_rate_val share.")
    flag_rate_val: float = Field(description="Share of flagged transactions the threshold was tuned for "
                                             "(on validation; on a live stream the share drifts).")
    variant: str = Field(description="Model: main (34 features) or device (41 features).")
    ring: RingSignal | None = Field(description="Ring signal; null if RING_SIGNAL=off.")
    top_features: list[TopFeature] = Field(description="Three largest contributions; empty list when explain=false.")
    features: dict[str, float | None] | None = Field(
        None, description="All features in model order; only with debug=true (otherwise the field is absent).")


class LabelResponse(BaseModel):
    """Confirmation that the label was recorded."""

    model_config = ConfigDict(json_schema_extra={"examples": [{"ok": True}]})

    ok: bool


class RingAlert(BaseModel):
    """Stored ring signal alert."""

    other_cards_30d: int | None
    share_new: float | None


class TxRecord(BaseModel):
    """Stored transaction with its decision and label (STORE_BACKEND=sqlite only)."""

    model_config = ConfigDict(json_schema_extra={"examples": [{
        "txid": "a1", "card1": 15000.0, "ts": 1000000, "amount": 49.0, "fraud_label_at": None, "variant": "main",
        "score": 0.01439689565449953, "flag": False, "threshold": 0.4008151590824127, "ring_alert": None,
        "top_features": [{"feature": "card5", "value": None, "contribution": 0.259}]}]})

    txid: str
    card1: float
    ts: int
    amount: float = Field(description="TransactionAmt.")
    fraud_label_at: float | None = Field(description="Fraud label time; null if there is no label.")
    variant: str | None = Field(description="Model that made the decision; null if no decision was recorded.")
    score: float | None
    flag: bool | None
    threshold: float | None
    ring_alert: RingAlert | None = Field(description="Ring signal alert; null if there was none.")
    top_features: list[TopFeature] | None = Field(None, description="Clothy: the three largest feature contributions, "
                                                  "stored with the decision; null if not computed.")


class DecisionItem(BaseModel):
    """Clothy: a row in the stored decisions list."""

    txid: str
    ts: int
    score: float
    flag: bool
    threshold: float
    variant: str
    fraud_label_at: float | None = Field(description="Fraud label time; null if there is no label.")
    top_features: list[TopFeature] | None = Field(description="Three largest contributions; null if not computed.")
    ring_alert: RingAlert | None = Field(description="Ring signal alert; null if there was none.")


class DecisionsResponse(BaseModel):
    """Clothy: stored decisions, newest first."""

    model_config = ConfigDict(json_schema_extra={"examples": [{"items": [
        {"txid": "a1", "ts": 1000000, "score": 0.52, "flag": True, "threshold": 0.4008, "variant": "main",
         "fraud_label_at": None, "top_features": [{"feature": "card5", "value": None, "contribution": 0.259}],
         "ring_alert": None}]}]})

    items: list[DecisionItem]


class RingAlertItem(BaseModel):
    """A row in the recent ring signal alerts list."""

    txid: str
    ts: int
    other_cards_30d: int | None
    share_new: float | None


class RingAlertsResponse(BaseModel):
    """Recent ring signal alerts, newest first."""

    model_config = ConfigDict(json_schema_extra={"examples": [{"alerts": [
        {"txid": "t001234", "ts": 2_600_000, "other_cards_30d": 4, "share_new": 1.0}]}]})

    alerts: list[RingAlertItem]


class RingParams(BaseModel):
    """Ring signal parameters (from models/ring_signal.yaml)."""

    k: int
    s: float
    warmup_days: float


class HealthResponse(BaseModel):
    """Service status and active configuration."""

    model_config = ConfigDict(json_schema_extra={"examples": [{
        "status": "ok", "variant": "main", "backend": "sqlite", "n_features": 34, "n_transactions": 1,
        "flag_rate_val": 0.01, "threshold": 0.4008151590824127, "ring": {"k": 2, "s": 0.5, "warmup_days": 14.0}}]})

    status: str
    variant: str
    backend: str = Field(description="memory or sqlite.")
    n_features: int
    n_transactions: int = Field(description="Number of transactions in the history store.")
    flag_rate_val: float
    threshold: float
    ring: RingParams | None = Field(description="null if the ring signal is off.")


class ErrorDetail(BaseModel):
    """Error with a text explanation."""

    detail: str


class ValidationErrorResponse(BaseModel):
    """422: either a list of pydantic validation errors or a text explanation from the service."""

    detail: str | list[dict[str, Any]]


def error(description: str, example: str) -> dict[str, Any]:
    """Error response description for the FastAPI `responses` parameter.

    Args:
        description: when the error occurs.
        example: sample text of the detail field.

    Returns:
        A dict for `responses={code: ...}`.
    """
    return {"model": ErrorDetail, "description": description,
            "content": {"application/json": {"example": {"detail": example}}}}


VALIDATION_422 = {
    "model": ValidationErrorResponse,
    "description": "Incomplete or invalid request. The body comes in two formats: a list of pydantic errors "
                   "(a required field txid, ts, TransactionAmt or card1 is missing, or has the wrong type) "
                   "or a string from the service (for example, a required field was sent as NaN).",
    "content": {"application/json": {"examples": {
        "pydantic": {"summary": "Required fields missing",
                     "value": {"detail": [{"type": "missing", "loc": ["body", "ts"], "msg": "Field required",
                                           "input": {"txid": "y"}}]}},
        "service": {"summary": "Service error", "value": {"detail": "card1 is required"}},
    }}},
}
