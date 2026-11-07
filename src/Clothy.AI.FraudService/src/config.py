"""Налаштування сервісу зі змінних середовища (або .env) з перевіркою при старті.

Кожна помилка пояснює, яка змінна неправильна і які значення допустимі.
"""

import logging
import os
from collections.abc import Mapping
from dataclasses import dataclass
from pathlib import Path
from typing import Any

import yaml

ROOT = Path(__file__).resolve().parent.parent
VARIANTS = ("main", "device")
BACKENDS = ("memory", "sqlite")
RING_ON = ("on", "1", "true", "yes")
RING_OFF = ("off", "0", "false", "no")
LOG_LEVELS = ("DEBUG", "INFO", "WARNING", "ERROR")


class ConfigError(ValueError):
    """Неправильне значення налаштування; текст пояснює, що виправити."""


@dataclass(frozen=True)
class Settings:
    """Перевірені налаштування сервісу.

    Attributes:
        models_dir: абсолютний шлях до теки моделей.
        variant: "main" або "device".
        flag_rate: частка позначених на валідації (ключ thresholds_from_validation).
        backend: "memory" або "sqlite".
        db_path: абсолютний шлях до бази SQLite або ":memory:".
        ring_params: None = параметри з models/ring_signal.yaml, False = сигнал вимкнено, dict = явні параметри.
        log_level: рівень логування.
    """

    models_dir: str
    variant: str
    flag_rate: float
    backend: str
    db_path: str
    ring_params: Mapping[str, Any] | bool | None
    log_level: str = "INFO"


def abs_path(p: str | os.PathLike) -> str:
    """Абсолютний шлях від кореня репозиторію (":memory:" і абсолютні шляхи без змін)."""
    p = str(p)
    return p if (p == ":memory:" or os.path.isabs(p)) else str(ROOT / p)


def _available_rates(models_dir: str, variant: str) -> list[str]:
    path = os.path.join(models_dir, "service_config.yaml")
    if not os.path.exists(path):
        raise ConfigError(f"MODELS_DIR={models_dir}: немає файлу service_config.yaml")
    with open(path, encoding="utf-8") as f:
        cfg = yaml.safe_load(f)
    return sorted(cfg["thresholds_from_validation"][variant])


def load_settings(models_dir: str | None = None, variant: str | None = None, flag_rate: float | None = None,
                  backend: str | None = None, db_path: str | None = None,
                  ring_params: Mapping[str, Any] | bool | None = None,
                  env: Mapping[str, str] | None = None) -> Settings:
    """Збирає й перевіряє налаштування: явні аргументи мають пріоритет над змінними середовища.

    Args:
        models_dir, variant, flag_rate, backend, db_path: явні значення або None (тоді MODELS_DIR, MODEL_VARIANT,
            FLAG_RATE, STORE_BACKEND, DB_PATH зі змінних середовища або значення за замовчуванням).
        ring_params: явні параметри сигналу кільця; None = за RING_SIGNAL.
        env: джерело змінних (за замовчуванням os.environ).

    Returns:
        Перевірені Settings.

    Raises:
        ConfigError: будь-яке значення неправильне.
    """
    env = os.environ if env is None else env

    models_dir = abs_path(models_dir or env.get("MODELS_DIR", "models"))
    if not os.path.isdir(models_dir):
        raise ConfigError(f"MODELS_DIR={models_dir}: теки не існує")

    variant = variant or env.get("MODEL_VARIANT", "main")
    if variant not in VARIANTS:
        raise ConfigError(f"MODEL_VARIANT={variant!r}: допустимо {' або '.join(VARIANTS)}")

    raw_rate = flag_rate if flag_rate is not None else env.get("FLAG_RATE", "0.01")
    try:
        rate = float(raw_rate)
    except (TypeError, ValueError):
        raise ConfigError(f"FLAG_RATE={raw_rate!r}: має бути числом, наприклад 0.01") from None
    rates = _available_rates(models_dir, variant)
    if str(rate) not in rates:
        raise ConfigError(f"FLAG_RATE={raw_rate!r}: для моделі {variant} пороги з валідації є лише для "
                          f"{', '.join(rates)}")

    backend = backend or env.get("STORE_BACKEND", "memory")
    if backend not in BACKENDS:
        raise ConfigError(f"STORE_BACKEND={backend!r}: допустимо {' або '.join(BACKENDS)}")

    db_path = abs_path(db_path or env.get("DB_PATH", "history.db"))
    if backend == "sqlite" and db_path != ":memory:":
        parent = os.path.dirname(db_path)
        if not os.path.isdir(parent):
            raise ConfigError(f"DB_PATH={db_path}: теки {parent} не існує (створіть її або змініть шлях)")
        if os.path.isdir(db_path):
            raise ConfigError(f"DB_PATH={db_path}: це тека, а потрібен шлях до файлу бази")

    if ring_params is None:
        ring = env.get("RING_SIGNAL", "on").lower()
        if ring in RING_OFF:
            ring_params = False
        elif ring not in RING_ON:
            raise ConfigError(f"RING_SIGNAL={ring!r}: допустимо {'/'.join(RING_ON)} (увімкнено) "
                              f"або {'/'.join(RING_OFF)} (вимкнено)")

    log_level = env.get("LOG_LEVEL", "INFO").upper()
    if log_level not in LOG_LEVELS:
        raise ConfigError(f"LOG_LEVEL={log_level!r}: допустимо {', '.join(LOG_LEVELS)}")

    return Settings(models_dir, variant, rate, backend, db_path, ring_params, log_level)


def setup_logging(level: str) -> logging.Logger:
    """Налаштовує логер "antifraud": власний обробник у stderr, якщо логування ще не налаштоване ззовні.

    Returns:
        Логер сервісу.
    """
    log = logging.getLogger("antifraud")
    log.setLevel(level)
    if not log.handlers and not logging.getLogger().handlers:
        h = logging.StreamHandler()
        h.setFormatter(logging.Formatter("%(asctime)s %(levelname)s %(name)s: %(message)s"))
        log.addHandler(h)
        log.propagate = False
    return log
