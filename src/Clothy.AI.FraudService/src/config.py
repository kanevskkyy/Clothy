"""Service settings from environment variables (or .env), validated at startup.

Each error explains which variable is wrong and which values are allowed.
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
    """Invalid setting value; the message explains what to fix."""


@dataclass(frozen=True)
class Settings:
    """Validated service settings.

    Attributes:
        models_dir: absolute path to the models folder.
        variant: "main" or "device".
        flag_rate: flagged share on validation (a thresholds_from_validation key).
        backend: "memory" or "sqlite".
        db_path: absolute path to the SQLite database or ":memory:".
        ring_params: None = parameters from models/ring_signal.yaml, False = signal off, dict = explicit parameters.
        log_level: logging level.
    """

    models_dir: str
    variant: str
    flag_rate: float
    backend: str
    db_path: str
    ring_params: Mapping[str, Any] | bool | None
    log_level: str = "INFO"


def abs_path(p: str | os.PathLike) -> str:
    """Absolute path from the repository root (":memory:" and absolute paths are kept as is)."""
    p = str(p)
    return p if (p == ":memory:" or os.path.isabs(p)) else str(ROOT / p)


def _available_rates(models_dir: str, variant: str) -> list[str]:
    path = os.path.join(models_dir, "service_config.yaml")
    if not os.path.exists(path):
        raise ConfigError(f"MODELS_DIR={models_dir}: service_config.yaml not found")
    with open(path, encoding="utf-8") as f:
        cfg = yaml.safe_load(f)
    return sorted(cfg["thresholds_from_validation"][variant])


def load_settings(models_dir: str | None = None, variant: str | None = None, flag_rate: float | None = None,
                  backend: str | None = None, db_path: str | None = None,
                  ring_params: Mapping[str, Any] | bool | None = None,
                  env: Mapping[str, str] | None = None) -> Settings:
    """Collects and validates settings: explicit arguments take precedence over environment variables.

    Args:
        models_dir, variant, flag_rate, backend, db_path: explicit values or None (then MODELS_DIR, MODEL_VARIANT,
            FLAG_RATE, STORE_BACKEND, DB_PATH from environment variables or the defaults).
        ring_params: explicit ring signal parameters; None = use RING_SIGNAL.
        env: variable source (os.environ by default).

    Returns:
        Validated Settings.

    Raises:
        ConfigError: any value is invalid.
    """
    env = os.environ if env is None else env

    models_dir = abs_path(models_dir or env.get("MODELS_DIR", "models"))
    if not os.path.isdir(models_dir):
        raise ConfigError(f"MODELS_DIR={models_dir}: folder does not exist")

    variant = variant or env.get("MODEL_VARIANT", "main")
    if variant not in VARIANTS:
        raise ConfigError(f"MODEL_VARIANT={variant!r}: allowed {' or '.join(VARIANTS)}")

    raw_rate = flag_rate if flag_rate is not None else env.get("FLAG_RATE", "0.01")
    try:
        rate = float(raw_rate)
    except (TypeError, ValueError):
        raise ConfigError(f"FLAG_RATE={raw_rate!r}: must be a number, e.g. 0.01") from None
    rates = _available_rates(models_dir, variant)
    if str(rate) not in rates:
        raise ConfigError(f"FLAG_RATE={raw_rate!r}: for model {variant} validation thresholds exist only for "
                          f"{', '.join(rates)}")

    backend = backend or env.get("STORE_BACKEND", "memory")
    if backend not in BACKENDS:
        raise ConfigError(f"STORE_BACKEND={backend!r}: allowed {' or '.join(BACKENDS)}")

    db_path = abs_path(db_path or env.get("DB_PATH", "history.db"))
    if backend == "sqlite" and db_path != ":memory:":
        parent = os.path.dirname(db_path)
        if not os.path.isdir(parent):
            raise ConfigError(f"DB_PATH={db_path}: folder {parent} does not exist (create it or change the path)")
        if os.path.isdir(db_path):
            raise ConfigError(f"DB_PATH={db_path}: this is a folder, a database file path is required")

    if ring_params is None:
        ring = env.get("RING_SIGNAL", "on").lower()
        if ring in RING_OFF:
            ring_params = False
        elif ring not in RING_ON:
            raise ConfigError(f"RING_SIGNAL={ring!r}: allowed {'/'.join(RING_ON)} (on) "
                              f"or {'/'.join(RING_OFF)} (off)")

    log_level = env.get("LOG_LEVEL", "INFO").upper()
    if log_level not in LOG_LEVELS:
        raise ConfigError(f"LOG_LEVEL={log_level!r}: allowed {', '.join(LOG_LEVELS)}")

    return Settings(models_dir, variant, rate, backend, db_path, ring_params, log_level)


def setup_logging(level: str) -> logging.Logger:
    """Sets up the "antifraud" logger: its own stderr handler unless logging is already configured elsewhere.

    Returns:
        The service logger.
    """
    log = logging.getLogger("antifraud")
    log.setLevel(level)
    if not log.handlers and not logging.getLogger().handlers:
        h = logging.StreamHandler()
        h.setFormatter(logging.Formatter("%(asctime)s %(levelname)s %(name)s: %(message)s"))
        log.addHandler(h)
        log.propagate = False
    return log
