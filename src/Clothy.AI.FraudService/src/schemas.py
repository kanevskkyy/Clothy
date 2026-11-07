"""Pydantic-схеми запитів і відповідей HTTP API (сторінка /docs).

Схеми лише описують і перевіряють дані: обчислення робить Scorer (src/scoring.py).
Приклади відповідей отримано справжнім викликом сервісу на прикладі запиту нижче.
"""

from typing import Any

from pydantic import BaseModel, ConfigDict, Field

TX_EXAMPLE = {
    "txid": "a1", "ts": 1_000_000, "TransactionAmt": 49.0, "card1": 15000.0, "ProductCD": "W",
    "card4": "visa", "card6": "debit", "addr1": 299.0, "addr2": 87.0, "P_emaildomain": "gmail.com",
}

# ---------- запити ----------


class Tx(BaseModel):
    """Транзакція на оцінку. Поля мають назви й значення схеми IEEE-CIS Fraud Detection."""

    model_config = ConfigDict(json_schema_extra={"examples": [TX_EXAMPLE]})

    txid: str = Field(description="Унікальний ідентифікатор транзакції. Повторний txid дає 409.")
    ts: int = Field(description="Час транзакції в секундах (аналог TransactionDT). Транзакції мають надходити "
                                "в неспадному порядку: раніша за останню збережену дає 409. "
                                "З ts також рахуються ознаки hour і weekday.")
    TransactionAmt: float = Field(description="Сума транзакції (у IEEE-CIS у доларах США).")
    card1: float = Field(description="Ідентифікатор картки. Ключ історії: вікна 1/7/30 днів, пари з адресою й email, "
                                     "мітки сусідів.")
    ProductCD: str | None = Field(None, description="Код типу товару (W, C, R, H, S).")
    card2: float | None = Field(None, description="Числовий атрибут картки (анонімізований).")
    card3: float | None = Field(None, description="Числовий атрибут картки (анонімізований).")
    card4: str | None = Field(None, description="Платіжна система картки (visa, mastercard, ...).")
    card5: float | None = Field(None, description="Числовий атрибут картки (анонімізований).")
    card6: str | None = Field(None, description="Тип картки (debit, credit, ...).")
    addr1: float | None = Field(None, description="Регіон платіжної адреси. Разом із card1 утворює ключ u2.")
    addr2: float | None = Field(None, description="Країна платіжної адреси.")
    dist1: float | None = Field(None, description="Відстань (анонімізована величина IEEE-CIS).")
    dist2: float | None = Field(None, description="Відстань (анонімізована величина IEEE-CIS).")
    P_emaildomain: str | None = Field(None, description="Домен email покупця. Разом із card1 і addr1 "
                                                           "утворює ключ u3.")
    R_emaildomain: str | None = Field(None, description="Домен email отримувача.")
    DeviceType: str | None = Field(None, description="Тип пристрою (desktop, mobile).")
    DeviceInfo: str | None = Field(None, description="Модель пристрою або ОС. Частина відбитка пристрою.")
    id_30: str | None = Field(None, description="Операційна система. Частина відбитка пристрою.")
    id_31: str | None = Field(None, description="Браузер. Частина відбитка пристрою.")
    id_33: str | None = Field(None, description="Роздільна здатність екрана. Частина відбитка пристрою. "
                                                   "Відбиток (і сигнал кільця) є лише тоді, коли задано всі "
                                                   "чотири поля: DeviceInfo, id_30, id_31, id_33.")


class Label(BaseModel):
    """Мітка за результатом спору (аналог dispute-вебхука платіжної системи)."""

    model_config = ConfigDict(json_schema_extra={"examples": [{"txid": "a1", "fraud": True, "at": 1_604_800}]})

    txid: str = Field(description="Транзакція, якої стосується спір. Невідомий txid дає 404.")
    fraud: bool = Field(True, description="true: шахрайство підтверджено; false: зняти мітку.")
    at: int | None = Field(None, description="Час отримання мітки в секундах (тій самій шкалі, що й ts). "
                                                "Мітка стає видимою для ознак сусідів, лише коли at <= ts нової "
                                                "транзакції. Якщо не задано, береться ts останньої збереженої "
                                                "транзакції (а не поточний годинник). У реальній інтеграції "
                                                "передавайте час створення спору зі Stripe (dispute.created).")


# ---------- відповіді ----------


class TopFeature(BaseModel):
    """Один із трьох найбільших за модулем внесків ознак у рахунок (SHAP-внески XGBoost)."""

    feature: str = Field(description="Назва ознаки.")
    value: float | None = Field(description="Значення ознаки; null, якщо пропущене.")
    contribution: float = Field(description="Внесок у логіт: додатний підвищує ризик, від'ємний знижує.")


class RingSignal(BaseModel):
    """Прозорий сигнал кільця: багато інших карток на тому самому відбитку пристрою, переважно нових."""

    suspect: bool = Field(description="true, якщо other_cards_30d >= k і share_new >= s, а сигнал активний.")
    active: bool = Field(description="Чи накопичено в сховищі не менше warmup_days днів історії.")
    other_cards_30d: int | None = Field(description="Скільки транзакцій інших карток на цьому відбитку за 30 днів; "
                                                       "null, якщо відбитка немає.")
    share_new: float | None = Field(description="Частка серед них перших появ картки; null, якщо їх немає.")
    k: int = Field(description="Поріг other_cards_30d.")
    s: float = Field(description="Поріг share_new.")


class ScoreResponse(BaseModel):
    """Рішення щодо транзакції."""

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
    score: float = Field(description="Ймовірність шахрайства за моделлю XGBoost (0..1).")
    flag: bool = Field(description="score >= threshold: транзакцію варто перевірити.")
    threshold: float = Field(description="Поріг, підібраний на валідації для частки flag_rate_val.")
    flag_rate_val: float = Field(description="Частка позначених транзакцій, на яку розраховано поріг "
                                             "(на валідації; на живому потоці частка зміщується).")
    variant: str = Field(description="Модель: main (34 ознаки) або device (41 ознака).")
    ring: RingSignal | None = Field(description="Сигнал кільця; null, якщо RING_SIGNAL=off.")
    top_features: list[TopFeature] = Field(description="Три найбільші внески; порожній список при explain=false.")
    features: dict[str, float | None] | None = Field(
        None, description="Усі ознаки в порядку моделі; лише при debug=true (поле інакше відсутнє).")


class LabelResponse(BaseModel):
    """Підтвердження, що мітку записано."""

    model_config = ConfigDict(json_schema_extra={"examples": [{"ok": True}]})

    ok: bool


class RingAlert(BaseModel):
    """Збережене спрацювання сигналу кільця."""

    other_cards_30d: int | None
    share_new: float | None


class TxRecord(BaseModel):
    """Збережена транзакція разом із рішенням і міткою (лише STORE_BACKEND=sqlite)."""

    model_config = ConfigDict(json_schema_extra={"examples": [{
        "txid": "a1", "card1": 15000.0, "ts": 1000000, "amount": 49.0, "fraud_label_at": None, "variant": "main",
        "score": 0.01439689565449953, "flag": False, "threshold": 0.4008151590824127, "ring_alert": None}]})

    txid: str
    card1: float
    ts: int
    amount: float = Field(description="TransactionAmt.")
    fraud_label_at: float | None = Field(description="Час мітки про шахрайство; null, якщо мітки немає.")
    variant: str | None = Field(description="Модель, що ухвалила рішення; null, якщо рішення не записано.")
    score: float | None
    flag: bool | None
    threshold: float | None
    ring_alert: RingAlert | None = Field(description="Спрацювання сигналу кільця; null, якщо його не було.")


class RingAlertItem(BaseModel):
    """Рядок списку останніх спрацювань сигналу кільця."""

    txid: str
    ts: int
    other_cards_30d: int | None
    share_new: float | None


class RingAlertsResponse(BaseModel):
    """Останні спрацювання сигналу кільця, від новіших до старіших."""

    model_config = ConfigDict(json_schema_extra={"examples": [{"alerts": [
        {"txid": "t001234", "ts": 2_600_000, "other_cards_30d": 4, "share_new": 1.0}]}]})

    alerts: list[RingAlertItem]


class RingParams(BaseModel):
    """Параметри сигналу кільця (з models/ring_signal.yaml)."""

    k: int
    s: float
    warmup_days: float


class HealthResponse(BaseModel):
    """Стан сервісу та активна конфігурація."""

    model_config = ConfigDict(json_schema_extra={"examples": [{
        "status": "ok", "variant": "main", "backend": "sqlite", "n_features": 34, "n_transactions": 1,
        "flag_rate_val": 0.01, "threshold": 0.4008151590824127, "ring": {"k": 2, "s": 0.5, "warmup_days": 14.0}}]})

    status: str
    variant: str
    backend: str = Field(description="memory або sqlite.")
    n_features: int
    n_transactions: int = Field(description="Скільки транзакцій у сховищі історії.")
    flag_rate_val: float
    threshold: float
    ring: RingParams | None = Field(description="null, якщо сигнал кільця вимкнено.")


class ErrorDetail(BaseModel):
    """Помилка з текстовим поясненням."""

    detail: str


class ValidationErrorResponse(BaseModel):
    """422: або список помилок валідації pydantic, або текстове пояснення від сервісу."""

    detail: str | list[dict[str, Any]]


def error(description: str, example: str) -> dict[str, Any]:
    """Опис відповіді з помилкою для параметра `responses` у FastAPI.

    Args:
        description: коли виникає помилка.
        example: приклад тексту поля detail.

    Returns:
        Словник для `responses={код: ...}`.
    """
    return {"model": ErrorDetail, "description": description,
            "content": {"application/json": {"example": {"detail": example}}}}


VALIDATION_422 = {
    "model": ValidationErrorResponse,
    "description": "Неповний або некоректний запит. Тіло буває двох форматів: список помилок pydantic "
                   "(немає обов'язкового поля txid, ts, TransactionAmt чи card1, або тип не той) "
                   "чи рядок від сервісу (наприклад, обов'язкове поле передано як NaN).",
    "content": {"application/json": {"examples": {
        "pydantic": {"summary": "Немає обов'язкових полів",
                     "value": {"detail": [{"type": "missing", "loc": ["body", "ts"], "msg": "Field required",
                                           "input": {"txid": "y"}}]}},
        "service": {"summary": "Помилка від сервісу", "value": {"detail": "card1 обов'язкове"}},
    }}},
}
