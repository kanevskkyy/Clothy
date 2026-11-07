"""Сховище історії в SQLite. Ті самі визначення ознак, що в FeatureStore (src/features.py):
вікна лише з рядків СТРОГО раніше за час транзакції, мітка j відома для i, якщо t_j <= t_i - 7 днів,
t_j >= t_i - 30 днів і dispute надійшов не пізніше t_i (fraud_at <= t_i). Власна мітка не використовується."""
import sqlite3
from collections.abc import Iterable, Mapping
from typing import Any

from src.features import DAY, DEVICE_NAMES, NAN, DuplicateTransactionError, OutOfOrderError, normalize_request

SCHEMA = """
CREATE TABLE IF NOT EXISTS tx (
  txid TEXT PRIMARY KEY, card1 REAL NOT NULL, addr1 REAL, em TEXT, fp TEXT,
  ts INTEGER NOT NULL, amt REAL NOT NULL, is_new INTEGER NOT NULL, fraud_at REAL);
CREATE INDEX IF NOT EXISTS i_card ON tx(card1, ts);
CREATE INDEX IF NOT EXISTS i_pa ON tx(card1, addr1, ts);
CREATE INDEX IF NOT EXISTS i_pe ON tx(card1, em, ts);
CREATE INDEX IF NOT EXISTS i_tri ON tx(card1, addr1, em, ts);
CREATE INDEX IF NOT EXISTS i_fp ON tx(fp, ts);
CREATE TABLE IF NOT EXISTS decisions (
  txid TEXT PRIMARY KEY, ts INTEGER, variant TEXT, score REAL, flag INTEGER, threshold REAL);
CREATE TABLE IF NOT EXISTS ring_alerts (
  txid TEXT PRIMARY KEY, ts INTEGER, other_cards INTEGER, share_new REAL);
"""
_KNOWN_FRAUD = "CASE WHEN ts<=? AND fraud_at IS NOT NULL AND fraud_at<=? THEN 1 ELSE 0 END"


class SqlFeatureStore:
    """Сховище історії в SQLite з тим самим інтерфейсом і ознаками, що FeatureStore.

    Додатково зберігає рішення (таблиця decisions) і спрацювання сигналу кільця (ring_alerts).
    Файлова база переживає перезапуск сервісу.

    Args:
        path: шлях до файлу бази або ":memory:".
    """

    def __init__(self, path: str = ":memory:") -> None:
        self.path = path
        self.db = sqlite3.connect(path, check_same_thread=False)
        if path != ":memory:":
            self.db.execute("PRAGMA journal_mode=WAL")
            self.db.execute("PRAGMA synchronous=NORMAL")
        self.db.executescript(SCHEMA)
        self.db.commit()
        self.last_ts, self.first_ts = self.db.execute("SELECT MAX(ts), MIN(ts) FROM tx").fetchone()

    def close(self) -> None:
        """Закриває з'єднання з базою."""
        self.db.close()

    @property
    def n_transactions(self) -> int:
        """Кількість збережених транзакцій."""
        return self.db.execute("SELECT COUNT(*) FROM tx").fetchone()[0]

    @property
    def n_decisions(self) -> int:
        """Кількість записаних рішень."""
        return self.db.execute("SELECT COUNT(*) FROM decisions").fetchone()[0]

    def check(self, txid: str, req: Mapping[str, Any]) -> None:
        """Перевіряє, що транзакцію можна зберегти (див. FeatureStore.check)."""
        r = normalize_request(req)
        if self.db.execute("SELECT 1 FROM tx WHERE txid=?", (txid,)).fetchone():
            raise DuplicateTransactionError(f"транзакція {txid} уже збережена")
        if self.last_ts is not None and r["ts"] < self.last_ts:
            raise OutOfOrderError(f"ts {r['ts']} раніше за останній збережений {self.last_ts}")

    def history_features(self, req: Mapping[str, Any]) -> dict[str, float]:
        """Ознаки історії картки та міток сусідів (див. FeatureStore.history_features)."""
        r = normalize_request(req)
        now, c1, ad, em, amt = r["ts"], r["c1"], r["ad"], r["em"], r["amt"]
        q = self.db.execute
        d1, d7, d30 = now - DAY, now - 7 * DAY, now - 30 * DAY
        n30, s30, n7, n1 = q(
            "SELECT COUNT(*), COALESCE(SUM(amt),0.0), "
            "COALESCE(SUM(CASE WHEN ts>=? THEN 1 ELSE 0 END),0), COALESCE(SUM(CASE WHEN ts>=? THEN 1 ELSE 0 END),0) "
            "FROM tx WHERE card1=? AND ts>=? AND ts<?", (d7, d1, c1, d30, now)).fetchone()
        mean = s30 / n30 if n30 > 0 else None
        m = q("SELECT MAX(ts) FROM tx WHERE card1=? AND ts<?", (c1, now)).fetchone()[0]
        f = {"card_n_30d": float(n30), "card_n_7d": float(n7), "card_n_1d": float(n1),
             "card_days_since_prev": NAN if m is None else min((now - m) / DAY, 30.0),
             "card_amt_ratio_30d": amt / mean if (mean is not None and mean > 0) else NAN}
        if ad is not None:
            pa30, nk, fk = q(
                "SELECT COUNT(*), COALESCE(SUM(CASE WHEN ts<=? THEN 1 ELSE 0 END),0), "
                f"COALESCE(SUM({_KNOWN_FRAUD}),0) FROM tx WHERE card1=? AND addr1=? AND ts>=? AND ts<?",
                (d7, d7, now, c1, ad, d30, now)).fetchone()
            f["pair_addr_n30"] = float(pa30)
            f["u2_nk"], f["u2_fk"] = float(nk), float(fk)
            f["u2_rate"] = fk / nk if nk > 0 else NAN
        else:
            f["pair_addr_n30"] = f["u2_nk"] = f["u2_fk"] = f["u2_rate"] = NAN
        if em is not None:
            f["pair_email_n30"] = float(q("SELECT COUNT(*) FROM tx WHERE card1=? AND em=? AND ts>=? AND ts<?",
                                          (c1, em, d30, now)).fetchone()[0])
        else:
            f["pair_email_n30"] = NAN
        if ad is not None and em is not None:
            nk, fk = q(
                "SELECT COALESCE(SUM(CASE WHEN ts<=? THEN 1 ELSE 0 END),0), "
                f"COALESCE(SUM({_KNOWN_FRAUD}),0) FROM tx WHERE card1=? AND addr1=? AND em=? AND ts>=? AND ts<?",
                (d7, d7, now, c1, ad, em, d30, now)).fetchone()
            f["u3_nk"], f["u3_fk"] = float(nk), float(fk)
            f["u3_rate"] = fk / nk if nk > 0 else NAN
        else:
            f["u3_nk"] = f["u3_fk"] = f["u3_rate"] = NAN
        return f

    def device_features(self, req: Mapping[str, Any]) -> dict[str, float]:
        """Ознаки відбитка пристрою (див. FeatureStore.device_features)."""
        r = normalize_request(req)
        if r["fp"] is None:
            return {k: NAN for k in DEVICE_NAMES}
        now, c1 = r["ts"], r["c1"]
        d7, d30 = now - 7 * DAY, now - 30 * DAY
        n_new, o30, o7, nk, fk = self.db.execute(
            "SELECT COALESCE(SUM(is_new),0), COUNT(*), COALESCE(SUM(CASE WHEN ts>=? THEN 1 ELSE 0 END),0), "
            "COALESCE(SUM(CASE WHEN ts<=? THEN 1 ELSE 0 END),0), "
            f"COALESCE(SUM({_KNOWN_FRAUD}),0) FROM tx WHERE fp=? AND card1<>? AND ts>=? AND ts<?",
            (d7, d7, d7, now, r["fp"], c1, d30, now)).fetchone()
        return {"fp_other30": float(o30), "fp_other7": float(o7), "fp_nk": float(nk), "fp_fk": float(fk),
                "fp_rate": fk / nk if nk > 0 else NAN, "fp_new30": float(n_new),
                "fp_share_new": n_new / o30 if o30 > 0 else NAN}

    def add(self, txid: str, req: Mapping[str, Any]) -> None:
        """Зберігає транзакцію в таблиці tx (після перевірок check)."""
        self.check(txid, req)
        r = normalize_request(req)
        now = r["ts"]
        seen = self.db.execute("SELECT 1 FROM tx WHERE card1=? AND ts<? LIMIT 1", (r["c1"], now)).fetchone()
        is_new = 0 if seen else 1
        with self.db:
            self.db.execute("INSERT INTO tx (txid, card1, addr1, em, fp, ts, amt, is_new, fraud_at) "
                            "VALUES (?,?,?,?,?,?,?,?,NULL)",
                            (txid, r["c1"], r["ad"], r["em"], r["fp"], now, r["amt"], is_new))
        self.last_ts = now
        if self.first_ts is None:
            self.first_ts = now

    def bulk_load(self, rows: Iterable[tuple]) -> None:
        """Масове завантаження історії без перевірок (для бенчмарку).

        Args:
            rows: кортежі (txid, card1, addr1, em, fp, ts, amt, is_new, fraud_at).
        """
        with self.db:
            self.db.executemany("INSERT INTO tx (txid, card1, addr1, em, fp, ts, amt, is_new, fraud_at) "
                                "VALUES (?,?,?,?,?,?,?,?,?)", rows)
        self.db.execute("ANALYZE")
        self.last_ts, self.first_ts = self.db.execute("SELECT MAX(ts), MIN(ts) FROM tx").fetchone()

    def set_label(self, txid: str, fraud: bool, at: float) -> None:
        """Записує мітку спору (див. FeatureStore.set_label).

        Raises:
            KeyError: txid невідомий.
        """
        with self.db:
            cur = self.db.execute("UPDATE tx SET fraud_at=? WHERE txid=?", (float(at) if fraud else None, txid))
        if cur.rowcount == 0:
            raise KeyError(txid)

    def log_decision(self, txid: str, ts: int, variant: str, score: float, flag: bool, threshold: float) -> None:
        """Записує рішення моделі в таблицю decisions (повторний txid перезаписує запис)."""
        with self.db:
            self.db.execute("INSERT OR REPLACE INTO decisions VALUES (?,?,?,?,?,?)",
                            (txid, int(ts), variant, float(score), int(bool(flag)), float(threshold)))

    def log_ring(self, txid: str, ts: int, other_cards: int | None, share_new: float | None) -> None:
        """Записує спрацювання сигналу кільця в таблицю ring_alerts."""
        with self.db:
            self.db.execute("INSERT OR REPLACE INTO ring_alerts VALUES (?,?,?,?)",
                            (txid, int(ts), None if other_cards is None else int(other_cards),
                             None if share_new is None else float(share_new)))

    def recent_ring_alerts(self, limit: int = 20) -> list[dict[str, Any]]:
        """Останні limit спрацювань сигналу кільця, від новіших до старіших."""
        rows = self.db.execute("SELECT txid, ts, other_cards, share_new FROM ring_alerts "
                               "ORDER BY ts DESC, txid DESC LIMIT ?", (int(limit),)).fetchall()
        return [dict(zip(("txid", "ts", "other_cards_30d", "share_new"), r, strict=False)) for r in rows]

    def get_tx(self, txid: str) -> dict[str, Any] | None:
        """Транзакція разом із рішенням, часом мітки та спрацюванням кільця; None, якщо txid невідомий."""
        row = self.db.execute(
            "SELECT t.txid, t.card1, t.ts, t.amt, t.fraud_at, d.variant, d.score, d.flag, d.threshold "
            "FROM tx t LEFT JOIN decisions d ON d.txid = t.txid WHERE t.txid=?", (txid,)).fetchone()
        if row is None:
            return None
        cols = ("txid", "card1", "ts", "amount", "fraud_label_at", "variant", "score", "flag", "threshold")
        rec = dict(zip(cols, row, strict=False))
        rec["flag"] = None if rec["flag"] is None else bool(rec["flag"])
        al = self.db.execute("SELECT other_cards, share_new FROM ring_alerts WHERE txid=?", (txid,)).fetchone()
        rec["ring_alert"] = None if al is None else {"other_cards_30d": al[0], "share_new": al[1]}
        return rec
