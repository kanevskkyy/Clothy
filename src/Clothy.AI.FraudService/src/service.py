"""FastAPI-сервіс. Запуск: uvicorn src.service:create_app --factory --port 8000
Налаштування беруться зі змінних середовища або з файлу .env у корені репозиторію (див. .env.example):
MODELS_DIR, MODEL_VARIANT (main|device), FLAG_RATE, STORE_BACKEND (memory|sqlite), DB_PATH, RING_SIGNAL (on|off),
LOG_LEVEL. Змінні, задані в терміналі, мають пріоритет над .env. Відносні шляхи рахуються від кореня репозиторію.
Значення перевіряються при старті (src/config.py). У логи не потрапляють поля транзакцій (картка, email,
адреса, пристрій): лише txid, коди й тексти помилок."""
from collections.abc import Mapping
from typing import Any

from fastapi import FastAPI, HTTPException, Query, Request
from fastapi.exception_handlers import request_validation_exception_handler
from fastapi.exceptions import RequestValidationError

try:
    from dotenv import load_dotenv
except ImportError:             # python-dotenv не встановлено: працюємо лише зі змінними середовища
    load_dotenv = None

from src.config import ROOT, ConfigError, load_settings, setup_logging
from src.config import abs_path as _abs  # noqa: F401  (старі тести імпортують _abs звідси)
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
Демо-сервіс курсової роботи graph-antifraud: оцінка ризику шахрайства для e-commerce транзакцій.

Потік: `POST /score` рахує ознаки з історії (картка, пари картка+адреса/email, мітки сусідів із затримкою
7 днів у вікні 30 днів, за потреби відбиток пристрою) → XGBoost → поріг із валідації → рішення.
Спір (dispute-вебхук платіжної системи) передається в `POST /labels` і впливає на ознаки наступних транзакцій.

Дані демо синтетичні; модель навчено на IEEE-CIS, для реального магазину її треба донавчити.
"""

TAGS = [
    {"name": "scoring", "description": "Оцінка транзакцій."},
    {"name": "labels", "description": "Мітки за результатами спорів (аналог dispute-вебхука)."},
    {"name": "history", "description": "Збережені рішення та спрацювання сигналу кільця (лише STORE_BACKEND=sqlite)."},
    {"name": "service", "description": "Стан сервісу."},
]

E404 = error("Невідомий txid: транзакцію не оцінювали із store=true.", "невідома транзакція")
E501 = error("Ендпоінт потребує STORE_BACKEND=sqlite (у пам'яті рішення не зберігаються).",
             "потрібен STORE_BACKEND=sqlite")


def _dump(m: Any) -> dict[str, Any]:
    """Модель pydantic -> dict (v2 і v1)."""
    return m.model_dump() if hasattr(m, "model_dump") else m.dict()


def create_app(models_dir: str | None = None, variant: str | None = None, flag_rate: float | None = None,
               backend: str | None = None, db_path: str | None = None,
               ring_params: Mapping[str, Any] | bool | None = None) -> FastAPI:
    """Створює застосунок FastAPI.

    Аргументи, не задані явно, беруться зі змінних середовища або .env
    (MODELS_DIR, MODEL_VARIANT, FLAG_RATE, STORE_BACKEND, DB_PATH, RING_SIGNAL, LOG_LEVEL).

    Returns:
        Застосунок; Scorer доступний як app.state.scorer.

    Raises:
        ConfigError: неправильне налаштування (текст пояснює, що виправити).
        ValueError: файли моделей змінено або не збігаються з кодом ознак.
    """
    if load_dotenv is not None:
        load_dotenv(ROOT / ".env")
    log = setup_logging("INFO")
    try:
        st = load_settings(models_dir, variant, flag_rate, backend, db_path, ring_params)
    except ConfigError as e:
        log.error("помилка конфігурації: %s", e)
        raise
    log.setLevel(st.log_level)
    try:
        scorer = Scorer(st.models_dir, st.variant, st.flag_rate, st.backend, st.db_path, st.ring_params)
    except Exception as e:
        log.error("не вдалося завантажити модель: %s", e)
        raise
    log.info("старт: variant=%s n_features=%d flag_rate=%s threshold=%.6f backend=%s db=%s ring=%s models=%s",
             scorer.variant, len(scorer.order), scorer.flag_rate, scorer.threshold, scorer.backend,
             st.db_path if st.backend == "sqlite" else "-", scorer.ring or "off", st.models_dir)
    if st.backend == "sqlite":
        log.info("історія в SQLite: %d транзакцій", scorer.store.n_transactions)
    app = FastAPI(title="graph-antifraud demo service", version="0.3", description=API_DESCRIPTION,
                  openapi_tags=TAGS)
    app.state.scorer = scorer

    @app.exception_handler(RequestValidationError)
    async def log_validation_error(request: Request, exc: RequestValidationError):
        # у лог лише шлях і місця помилок, без значень полів запиту
        locs = [".".join(str(x) for x in err.get("loc", ())) for err in exc.errors()]
        log.warning("422 %s: некоректний запит, поля: %s", request.url.path, ", ".join(locs))
        return await request_validation_exception_handler(request, exc)

    @app.get("/health", tags=["service"], response_model=HealthResponse,
             summary="Стан сервісу та конфігурація",
             description="Активна модель, сховище, поріг, параметри сигналу кільця і кількість транзакцій в історії.")
    def health():
        with scorer._lock:
            return {"status": "ok", "variant": scorer.variant, "backend": scorer.backend,
                    "n_features": len(scorer.order), "n_transactions": scorer.store.n_transactions,
                    "flag_rate_val": scorer.flag_rate, "threshold": scorer.threshold, "ring": scorer.ring}

    @app.post("/score", tags=["scoring"], response_model=ScoreResponse, response_model_exclude_unset=True,
              summary="Оцінити транзакцію",
              description="Рахує ознаки лише з транзакцій, що були СТРОГО раніше за `ts`, і мітки сусідів, "
                          "відомі на момент `ts`; повертає ймовірність, рішення за порогом, три найбільші внески "
                          "ознак і сигнал кільця. Обов'язкові поля: `txid`, `ts`, `TransactionAmt`, `card1`; "
                          "решта необов'язкові (пропущене або невідоме значення категорії = NaN). "
                          "При `store=true` транзакція зберігається в історії.",
              responses={409: error("Дублікат txid (транзакцію вже збережено) або ts раніший за останню "
                                    "збережену транзакцію (потік має йти в неспадному порядку часу). "
                                    "Лише при store=true.", "транзакція a1 уже збережена"),
                         422: VALIDATION_422})
    def score(tx: Tx,
              store: bool = Query(True, description="Зберегти транзакцію в історії (і рішення в SQLite)."),
              debug: bool = Query(False, description="Додати у відповідь усі ознаки (поле `features`)."),
              explain: bool = Query(True, description="Рахувати три найбільші внески (додає близько 15 мс).")):
        try:
            return scorer.score(tx.txid, _dump(tx), store=store, debug=debug, explain=explain)
        except (DuplicateTransactionError, OutOfOrderError) as e:
            log.warning("409 /score txid=%s: %s", tx.txid, e)
            raise HTTPException(status_code=409, detail=str(e)) from e
        except ValueError as e:
            log.warning("422 /score txid=%s: %s", tx.txid, e)
            raise HTTPException(status_code=422, detail=str(e)) from e

    @app.post("/labels", tags=["labels"], response_model=LabelResponse,
              summary="Записати мітку спору",
              description="Аналог dispute-вебхука: позначає транзакцію як шахрайську (`fraud=true`) або знімає "
                          "мітку (`fraud=false`). Мітка впливає на ознаки u2/u3/fp наступних транзакцій лише тоді, "
                          "коли від транзакції минуло щонайменше 7 днів (але не більше 30) і `at` не пізніше їхнього "
                          "`ts`. Без `at` береться ts останньої збереженої транзакції; у реальній інтеграції "
                          "передавайте час спору зі Stripe.",
              responses={404: E404})
    def labels(label: Label):
        try:
            scorer.set_label(label.txid, label.fraud, label.at)
        except KeyError as e:
            log.warning("404 /labels txid=%s: невідома транзакція", label.txid)
            raise HTTPException(status_code=404, detail="невідома транзакція") from e
        return {"ok": True}

    @app.get("/tx/{txid}", tags=["history"], response_model=TxRecord,
             summary="Транзакція, рішення і мітка",
             description="Повертає збережену транзакцію, рішення моделі, час мітки та спрацювання сигналу кільця.",
             responses={404: E404, 501: E501})
    def get_tx(txid: str):
        if not hasattr(scorer.store, "get_tx"):
            raise HTTPException(status_code=501, detail="потрібен STORE_BACKEND=sqlite")
        with scorer._lock:
            rec = scorer.store.get_tx(txid)
        if rec is None:
            log.warning("404 /tx txid=%s: невідома транзакція", txid)
            raise HTTPException(status_code=404, detail="невідома транзакція")
        return rec

    @app.get("/rings", tags=["history"], response_model=RingAlertsResponse,
             summary="Останні спрацювання сигналу кільця",
             description="Транзакції, на яких спрацював сигнал кільця, від новіших до старіших.",
             responses={501: E501})
    def rings(limit: int = Query(20, description="Скільки записів повернути.")):
        if not hasattr(scorer.store, "recent_ring_alerts"):
            raise HTTPException(status_code=501, detail="потрібен STORE_BACKEND=sqlite")
        with scorer._lock:
            return {"alerts": scorer.store.recent_ring_alerts(limit)}

    @app.get("/decisions", tags=["history"], response_model=DecisionsResponse,
             summary="Збережені рішення (Clothy)",
             description="Останні рішення моделі з трьома головними причинами й сигналом кільця, від новіших до "
                         "старіших. `flagged=true` повертає лише транзакції «на перевірку», `flagged=false` лише "
                         "решту; без параметра повертаються всі.",
             responses={501: E501})
    def decisions(flagged: bool | None = Query(None, description="Фільтр за рішенням (flag)."),
                  limit: int = Query(50, ge=1, le=500, description="Скільки записів повернути (1..500).")):
        if not hasattr(scorer.store, "recent_decisions"):
            raise HTTPException(status_code=501, detail="потрібен STORE_BACKEND=sqlite")
        with scorer._lock:
            return {"items": scorer.store.recent_decisions(flagged, limit)}

    return app
