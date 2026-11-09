"""Synthetic history with the IEEE-CIS schema: regular cards, isolated fraud episodes, rings via a shared device.
This data demonstrates the pipeline and is not for quality evaluation. Categories come from models/encoders.json."""
import json
import os
from typing import Any

import numpy as np

DAY = 86400
DEV_COLS = ("DeviceInfo", "id_30", "id_31", "id_33")


def public_fields(e: dict[str, Any]) -> dict[str, Any]:
    """Event fields without the internal ones (_fraud, _ring, _dispute_at): what the service sees."""
    return {k: v for k, v in e.items() if not k.startswith("_")}


def make_stream(models_dir: str = "models", seed: int = 0, n_cards: int = 250, days: int = 60,
                n_rings: int = 3) -> list[dict[str, Any]]:
    """Generates a synthetic time-ordered transaction stream.

    Args:
        models_dir: folder with encoders.json (categories come from there).
        seed: random seed.
        n_cards: number of regular cards.
        days: stream length in days (at least 40).
        n_rings: number of rings (new cards on one device within 2 days).

    Returns:
        Events with IEEE-CIS fields, txid "t000000"... and the internal fields _fraud, _ring, _dispute_at.
    """
    assert days >= 40, "rings are placed on days 20..days-8"
    rng = np.random.default_rng(seed)
    with open(os.path.join(models_dir, "encoders.json"), encoding="utf-8") as f:
        enc = json.load(f)
    K = {c: sorted(v.keys()) for c, v in enc.items()}
    def pick(col: str) -> str:
        return K[col][int(rng.integers(len(K[col])))]

    def rand_dev():
        d = {c: pick(c) for c in DEV_COLS}
        d["DeviceType"] = pick("DeviceType")
        return d

    pool, seen = [], set()
    while len(pool) < 40:
        d = rand_dev()
        key = tuple(d[c] for c in DEV_COLS)
        if key not in seen:
            seen.add(key)
            pool.append(d)
    wts = 1.0 / np.arange(1, len(pool) + 1)
    wts /= wts.sum()

    def new_card(cid):
        return dict(card1=float(cid), card2=float(rng.integers(100, 600)), card3=150.0,
                    card5=float(rng.choice([102.0, 126.0, 137.0, 166.0, 195.0, 226.0])),
                    card4=pick("card4"), card6=pick("card6"),
                    addr1=float(rng.choice([204, 299, 325, 330, 441, 476, 512])), addr2=87.0,
                    P_emaildomain=pick("P_emaildomain"))

    events = []

    def emit(card, t, amt, dev, fraud, ring, dispute_at):
        e = {"txid": "", "ts": int(t), "TransactionAmt": round(float(amt), 2), "ProductCD": pick("ProductCD"),
             "card1": card["card1"], "card2": card["card2"], "card3": card["card3"], "card4": card["card4"],
             "card5": card["card5"], "card6": card["card6"], "addr1": card["addr1"], "addr2": card["addr2"],
             "dist1": None, "dist2": None, "P_emaildomain": card["P_emaildomain"], "R_emaildomain": None,
             "DeviceType": None, "DeviceInfo": None, "id_30": None, "id_31": None, "id_33": None,
             "_fraud": bool(fraud), "_ring": ring, "_dispute_at": None if dispute_at is None else int(dispute_at)}
        if dev is not None:
            e.update(dev)
        events.append(e)

    cards = [new_card(10000 + i) for i in range(n_cards)]
    cdev = [pool[int(rng.choice(len(pool), p=wts))] for _ in cards]
    for card, dev in zip(cards, cdev, strict=False):                                   # regular transactions
        t = float(rng.uniform(0, 5 * DAY))
        while t < days * DAY:
            emit(card, t, rng.lognormal(3.5, 0.9), dev if rng.random() < 0.35 else None, False, None, None)
            t += float(rng.exponential(2.5 * DAY))
    for ci in rng.choice(n_cards, size=max(1, int(0.08 * n_cards)), replace=False):   # isolated episodes
        card, dev = cards[int(ci)], cdev[int(ci)]
        tt = float(rng.uniform(10 * DAY, (days - 5) * DAY))
        for _ in range(int(rng.integers(3, 7))):
            tt += float(rng.uniform(0, 3 * 3600))
            emit(card, tt, rng.lognormal(5.5, 0.5), dev if rng.random() < 0.5 else None, True, None,
                 tt + float(rng.uniform(3, 12)) * DAY)
    for r in range(n_rings):                                             # rings: new cards, one device
        while True:
            dev = rand_dev()
            key = tuple(dev[c] for c in DEV_COLS)
            if key not in seen:
                seen.add(key)
                break
        t0 = float(rng.uniform(20 * DAY, (days - 8) * DAY))
        for j in range(int(rng.integers(5, 8))):
            t = t0 + float(rng.uniform(0, 2 * DAY))
            emit(new_card(20000 + r * 100 + j), t, rng.lognormal(5.0, 0.6), dev, True, r,
                 t + float(rng.uniform(5, 14)) * DAY)

    events.sort(key=lambda e: e["ts"])
    for i, e in enumerate(events):
        e["txid"] = f"t{i:06d}"
    return events
