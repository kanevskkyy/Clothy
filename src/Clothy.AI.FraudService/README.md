# Clothy.AI.FraudService

Антифрод-сервіс Clothy: оцінює оплату картою за її полями та історією (картка, пари картка+адреса й
картка+email, мітки спорів сусідніх транзакцій, відбиток пристрою), повертає ймовірність шахрайства, рішення
за порогом, три головні причини і сигнал кільця. Мітки спорів надходять окремим запитом `POST /labels`.

## Звідки код

| | |
|---|---|
| Джерело | `D:\graph-antifraud` (курсова робота, приватний репозиторій) |
| Коміт | `7476e4f9dec7a1fc1d2179ed12f616c37b1974e4` |
| Скопійовано | `src/`, `tests/`, `requirements.txt`, `pytest.ini`, `pyproject.toml`, `.env.example` |
| Моделі | `models/service_main.json`, `service_device.json`, `encoders.json`, `service_config.yaml`, `ring_signal.yaml`, `fixture_synthetic.npz`: у git **не** зберігаються, копіюються вручну (див. [models/README.md](models/README.md)) |
| Не скопійовано | `notebooks/`, `data/`, `results/`, `private/`, `scripts/`, `.venv/`, `history.db*`, `demo_db/` |

Повна документація моделі, ознак і експериментів лишається в README graph-antifraud.

## Відмінності від graph-antifraud

Логіка ознак, моделі й пороги не змінені.

Поки відмінностей у коді немає (див. історію комітів цієї теки).

## Запуск локально

```powershell
cd src\Clothy.AI.FraudService
# 1. моделі (див. models/README.md)
# 2. оточення
python -m venv .venv
.venv\Scripts\Activate.ps1
pip install -r requirements.txt
Copy-Item .env.example .env
# 3. сервіс на порту 8097 (у dev PaymentService звертається саме сюди)
uvicorn src.service:create_app --factory --port 8097
```

Документація API: <http://127.0.0.1:8097/docs>.

Тести (з цієї теки):

```powershell
pytest -q
```

З моделями: 57 passed, 1 skipped (`private/fixture_real_test.npz` не копіюється). Без моделей тести, яким вони
потрібні, пропускаються (`skipif` за `models/service_config.yaml`).

## Docker

Образ збирається з цієї теки (`docker-compose.yml`, сервіс `fraud-service`). Перед збіркою в `models/` мають
лежати файли моделей, бо `COPY models` бере їх із локальної теки.

| Змінна | У образі | Опис |
|---|---|---|
| `MODELS_DIR` | `models` | тека моделей |
| `MODEL_VARIANT` | `main` | `main` (34 ознаки) або `device` (41) |
| `FLAG_RATE` | `0.01` | частка позначених на валідації: 0.005, 0.01, 0.02, 0.03, 0.05 |
| `STORE_BACKEND` | `sqlite` | `/tx`, `/rings` працюють лише з SQLite |
| `DB_PATH` | `/data/history.db` | `/data` монтується як volume, історія переживає перезапуск |
| `RING_SIGNAL` | `on` | сигнал кільця |
| `LOG_LEVEL` | `INFO` | рівень логування |

Порт 8097 у docker-compose назовні не публікується: сервіс доступний лише в мережі `clothy-network`.
`/score` і `/labels` викликає тільки PaymentService; адмінка читає дані через Gateway з роллю Admin.

## Обмеження

- Модель навчена на IEEE-CIS, а не на даних магазину. Поля Stripe відображаються в поля IEEE-CIS наближено
  (`card1` = хеш відбитка картки, `addr1` = поштовий індекс тощо), тому оцінки демонстраційні; для реального
  використання модель треба донавчити.
- Сигнал кільця вмикається лише після 14 днів історії в сховищі (`warmup_days`).
- Транзакції мають надходити в неспадному порядку `ts`; запізнілий вебхук отримує 409 і лишається «не оцінено».
- SQLite розрахований на один екземпляр сервісу; масштабувати горизонтально не можна.
- `requirements.txt` без зафіксованих версій (як в оригіналі); моделі збережені XGBoost 3.4.1.
- Логи не містять полів транзакцій (картка, email, адреса, пристрій): лише txid і тексти помилок.
