"""Онлайн-ознаки сервісної моделі.

Відтворюють визначення з ноутбуків 20-22 (models/service_config.yaml):
  * усі вікна беруть лише рядки зі СТРОГО раннім часом (рівний час виключено);
  * мітка сусіда j «відома» для транзакції i, якщо t_j <= t_i - 7 днів (затримка),
    t_j >= t_i - 30 днів (вікно) і мітку про шахрайство отримано не пізніше t_i;
  * мітку не отримано = «чесна»: рахується в nk, але не в fk;
  * власна мітка рядка не використовується.
Транзакції мають надходити в неспадному порядку часу ts (секунди).
"""
import math
from bisect import bisect_left, bisect_right
from collections.abc import Mapping
from typing import Any

DAY = 86400
NAN = float("nan")
INF = float("inf")
FP_COLS = ("DeviceInfo", "id_30", "id_31", "id_33")
HISTORY_NAMES = ["card_n_30d", "card_n_7d", "card_n_1d", "card_days_since_prev", "card_amt_ratio_30d",
                 "pair_addr_n30", "pair_email_n30", "u2_nk", "u2_fk", "u2_rate", "u3_nk", "u3_fk", "u3_rate"]
DEVICE_NAMES = ["fp_other30", "fp_other7", "fp_nk", "fp_fk", "fp_rate", "fp_new30", "fp_share_new"]


class OutOfOrderError(ValueError):
    """Транзакція має ts, раніший за останню збережену (потік мусить іти в неспадному порядку часу)."""


class DuplicateTransactionError(ValueError):
    """Транзакцію з таким txid уже збережено."""


def _missing(v: Any) -> bool:
    return v is None or (isinstance(v, float) and math.isnan(v))


def normalize_request(req: Mapping[str, Any]) -> dict[str, Any]:
    """Витягає з запиту ключі історії у зручному вигляді.

    Args:
        req: поля транзакції (назви IEEE-CIS); card1, ts і TransactionAmt обов'язкові.

    Returns:
        Словник: c1 (card1), ad (addr1 або None), em (P_emaildomain або None),
        fp (відбиток DeviceInfo|id_30|id_31|id_33 або None, якщо бракує хоч одного поля),
        amt (сума), ts (час, int).

    Raises:
        ValueError: обов'язкове поле відсутнє або NaN.
    """
    for k in ("card1", "ts", "TransactionAmt"):
        if _missing(req.get(k)):
            raise ValueError(f"{k} обов'язкове")
    fp = [None if _missing(req.get(c)) else str(req[c]) for c in FP_COLS]
    return {
        "c1": float(req["card1"]),
        "ad": None if _missing(req.get("addr1")) else float(req["addr1"]),
        "em": None if _missing(req.get("P_emaildomain")) else str(req["P_emaildomain"]),
        "fp": "|".join(fp) if all(v is not None for v in fp) else None,
        "amt": float(req["TransactionAmt"]),
        "ts": int(req["ts"]),
    }


class _Group:
    """Рядки однієї групи ключа (картка, пара, трійка чи відбиток), упорядковані за часом."""

    __slots__ = ("t", "amt", "new", "fraud_at", "card")

    def __init__(self) -> None:
        self.t, self.amt, self.new, self.fraud_at, self.card = [], [], [], [], []

    def append(self, t: int, amt: float, new: int, card: float) -> int:
        self.t.append(t)
        self.amt.append(amt)
        self.new.append(new)
        self.fraud_at.append(INF)
        self.card.append(card)
        return len(self.t) - 1


def _window(g: _Group | None, now: int, w_days: float, delay_days: float) -> tuple[int, int]:
    """позиції [lo, hi): рядки з часом у [now - w_days, now - delay_days] і строго раніше за now"""
    if g is None:
        return 0, 0
    a = bisect_left(g.t, now)
    hi = a if delay_days == 0 else min(bisect_right(g.t, now - delay_days * DAY), a)
    lo = min(bisect_left(g.t, now - w_days * DAY), hi)
    return lo, hi


def _count(g: _Group | None, now: int, w_days: float) -> int:
    lo, hi = _window(g, now, w_days, 0)
    return hi - lo


def _known(g: _Group | None, now: int, w_days: float, delay_days: float) -> tuple[int, int]:
    """(скільки відомих, з них шахрайських) для групи з однією ознакою ключа"""
    if g is None:
        return 0, 0
    lo, hi = _window(g, now, w_days, delay_days)
    return hi - lo, sum(1 for k in range(lo, hi) if g.fraud_at[k] <= now)


class FeatureStore:
    """Сховище історії в пам'яті з онлайн-ознаками (див. докстрінг модуля).

    Атрибути last_ts і first_ts: час останньої й першої збереженої транзакції (None, якщо порожньо).
    """

    def __init__(self) -> None:
        self.card, self.pa, self.pe, self.tri, self.fp = {}, {}, {}, {}, {}
        self.tx = {}
        self.last_ts = None
        self.first_ts = None

    @property
    def n_transactions(self) -> int:
        """Кількість збережених транзакцій."""
        return len(self.tx)

    def check(self, txid: str, req: Mapping[str, Any]) -> None:
        """Перевіряє, що транзакцію можна зберегти.

        Raises:
            ValueError: неповний запит.
            DuplicateTransactionError: txid уже збережено.
            OutOfOrderError: ts раніший за останній збережений.
        """
        r = normalize_request(req)
        if txid in self.tx:
            raise DuplicateTransactionError(f"транзакція {txid} уже збережена")
        if self.last_ts is not None and r["ts"] < self.last_ts:
            raise OutOfOrderError(f"ts {r['ts']} раніше за останній збережений {self.last_ts}")

    def history_features(self, req: Mapping[str, Any]) -> dict[str, float]:
        """Ознаки історії картки та міток сусідів (HISTORY_NAMES) для транзакції req.

        Враховуються лише транзакції строго раніше за req["ts"]; сама req не зберігається.
        Пропущені значення повертаються як NaN.
        """
        r = normalize_request(req)
        now, c1, ad, em, amt = r["ts"], r["c1"], r["ad"], r["em"], r["amt"]
        g = self.card.get(c1)
        lo, hi = _window(g, now, 30, 0)
        n30 = hi - lo
        s30 = sum(g.amt[lo:hi]) if (g is not None and n30 > 0) else 0.0
        mean = s30 / n30 if n30 > 0 else None
        prev = NAN
        if g is not None:
            a = bisect_left(g.t, now)
            if a > 0:
                prev = min((now - g.t[a - 1]) / DAY, 30.0)
        f = {"card_n_30d": float(n30), "card_n_7d": float(_count(g, now, 7)),
             "card_n_1d": float(_count(g, now, 1)), "card_days_since_prev": prev,
             "card_amt_ratio_30d": amt / mean if (mean is not None and mean > 0) else NAN}
        pa = self.pa.get((c1, ad)) if ad is not None else None
        pe = self.pe.get((c1, em)) if em is not None else None
        tri = self.tri.get((c1, ad, em)) if (ad is not None and em is not None) else None
        f["pair_addr_n30"] = float(_count(pa, now, 30)) if ad is not None else NAN
        f["pair_email_n30"] = float(_count(pe, now, 30)) if em is not None else NAN
        for name, ok, grp in (("u2", ad is not None, pa), ("u3", ad is not None and em is not None, tri)):
            if ok:
                nk, fk = _known(grp, now, 30, 7)
                f[f"{name}_nk"], f[f"{name}_fk"] = float(nk), float(fk)
                f[f"{name}_rate"] = fk / nk if nk > 0 else NAN
            else:
                f[f"{name}_nk"] = f[f"{name}_fk"] = f[f"{name}_rate"] = NAN
        return f

    def device_features(self, req: Mapping[str, Any]) -> dict[str, float]:
        """Ознаки відбитка пристрою (DEVICE_NAMES): інші картки на тому самому відбитку за 7/30 днів,
        їхні відомі мітки та частка нових карток. Без повного відбитка всі значення NaN.
        """
        r = normalize_request(req)
        if r["fp"] is None:
            return {k: NAN for k in DEVICE_NAMES}
        now, c1 = r["ts"], r["c1"]
        g = self.fp.get(r["fp"])
        lo, hi = _window(g, now, 30, 0)
        o30 = n_new = 0
        for k in range(lo, hi):
            if g.card[k] != c1:
                o30 += 1
                n_new += g.new[k]
        lo7, hi7 = _window(g, now, 7, 0)
        o7 = sum(1 for k in range(lo7, hi7) if g.card[k] != c1)
        lok, hik = _window(g, now, 30, 7)
        nk = fk = 0
        for k in range(lok, hik):
            if g.card[k] != c1:
                nk += 1
                fk += 1 if g.fraud_at[k] <= now else 0
        return {"fp_other30": float(o30), "fp_other7": float(o7), "fp_nk": float(nk), "fp_fk": float(fk),
                "fp_rate": fk / nk if nk > 0 else NAN, "fp_new30": float(n_new),
                "fp_share_new": n_new / o30 if o30 > 0 else NAN}

    def add(self, txid: str, req: Mapping[str, Any]) -> None:
        """Зберігає транзакцію в історії (після тих самих перевірок, що й check)."""
        self.check(txid, req)
        r = normalize_request(req)
        now, c1, ad, em = r["ts"], r["c1"], r["ad"], r["em"]
        g0 = self.card.get(c1)
        new = 1 if (g0 is None or bisect_left(g0.t, now) == 0) else 0   # перша поява картки в цей момент
        refs = []

        def put(d, key):
            g = d.get(key)
            if g is None:
                g = d[key] = _Group()
            refs.append((g, g.append(now, r["amt"], new, c1)))

        put(self.card, c1)
        if ad is not None:
            put(self.pa, (c1, ad))
        if em is not None:
            put(self.pe, (c1, em))
        if ad is not None and em is not None:
            put(self.tri, (c1, ad, em))
        if r["fp"] is not None:
            put(self.fp, r["fp"])
        self.tx[txid] = refs
        self.last_ts = now
        if self.first_ts is None:
            self.first_ts = now

    def set_label(self, txid: str, fraud: bool, at: float) -> None:
        """Записує мітку спору.

        Args:
            txid: збережена транзакція.
            fraud: True ставить мітку шахрайства з часом at, False знімає мітку.
            at: час отримання мітки (секунди, та сама шкала, що й ts).

        Raises:
            KeyError: txid невідомий.
        """
        refs = self.tx.get(txid)
        if refs is None:
            raise KeyError(txid)
        for g, k in refs:
            g.fraud_at[k] = float(at) if fraud else INF
