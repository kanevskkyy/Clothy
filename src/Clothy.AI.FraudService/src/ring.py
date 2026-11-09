"""Transparent ring signal: many OTHER cards on the same device configuration within 30 days,
mostly new cards among them. Parameters (k, s) were tuned on real validation data (notebook 23)."""
import math
from collections.abc import Mapping
from typing import Any

DAY = 86400


def _nan(v: Any) -> bool:
    return v is None or (isinstance(v, float) and math.isnan(v))


def ring_signal(dev: Mapping[str, float], params: Mapping[str, float], history_days: float) -> dict[str, Any]:
    """Computes the ring signal for one transaction.

    Args:
        dev: device_features output (needs fp_other30 and fp_share_new).
        params: {"k", "s", "warmup_days"} from models/ring_signal.yaml.
        history_days: how many days of history the store holds.

    Returns:
        {"suspect", "active", "other_cards_30d", "share_new", "k", "s"}: suspect = True if the signal is active
        (history_days >= warmup_days), a fingerprint exists and fp_other30 >= k and fp_share_new >= s.
    """
    other, share = dev["fp_other30"], dev["fp_share_new"]
    has = not _nan(other)
    active = history_days >= params["warmup_days"]
    suspect = bool(active and has and not _nan(share) and other >= params["k"] and share >= params["s"])
    return {"suspect": suspect, "active": bool(active),
            "other_cards_30d": int(other) if has else None,
            "share_new": None if _nan(share) else float(share),
            "k": params["k"], "s": params["s"]}
