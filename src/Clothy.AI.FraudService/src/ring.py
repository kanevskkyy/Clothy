"""Прозорий сигнал кільця: багато ІНШИХ карток на тій самій конфігурації пристрою за 30 днів,
і серед них переважно нові картки. Параметри (k, s) підібрано на реальній валідації (ноутбук 23)."""
import math
from collections.abc import Mapping
from typing import Any

DAY = 86400


def _nan(v: Any) -> bool:
    return v is None or (isinstance(v, float) and math.isnan(v))


def ring_signal(dev: Mapping[str, float], params: Mapping[str, float], history_days: float) -> dict[str, Any]:
    """Рахує сигнал кільця для однієї транзакції.

    Args:
        dev: результат device_features (потрібні fp_other30 і fp_share_new).
        params: {"k", "s", "warmup_days"} з models/ring_signal.yaml.
        history_days: скільки днів історії накопичено в сховищі.

    Returns:
        {"suspect", "active", "other_cards_30d", "share_new", "k", "s"}: suspect = True, якщо сигнал активний
        (history_days >= warmup_days), відбиток є і fp_other30 >= k та fp_share_new >= s.
    """
    other, share = dev["fp_other30"], dev["fp_share_new"]
    has = not _nan(other)
    active = history_days >= params["warmup_days"]
    suspect = bool(active and has and not _nan(share) and other >= params["k"] and share >= params["s"])
    return {"suspect": suspect, "active": bool(active),
            "other_cards_30d": int(other) if has else None,
            "share_new": None if _nan(share) else float(share),
            "k": params["k"], "s": params["s"]}
