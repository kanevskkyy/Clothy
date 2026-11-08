"""Clothy: top_features у таблиці decisions і ендпоінт GET /decisions."""
import os
import sqlite3

import pytest

pytest.importorskip("fastapi")
pytest.importorskip("httpx")
from fastapi.testclient import TestClient  # noqa: E402

from src.service import create_app  # noqa: E402
from src.sqlstore import SqlFeatureStore  # noqa: E402
from src.synth import make_stream, public_fields  # noqa: E402

MODELS = "models"
needs_models = pytest.mark.skipif(not os.path.exists(os.path.join(MODELS, "service_config.yaml")),
                                  reason="models/ відсутня")


def _scored_client(n=300, flag_rate=0.05):
    """Клієнт із SQLite у пам'яті та n оцінених транзакцій; повертає (client, {txid: відповідь /score})."""
    c = TestClient(create_app(MODELS, "main", flag_rate, backend="sqlite", db_path=":memory:", ring_params=False))
    outs = {}
    for i, e in enumerate(make_stream(MODELS, seed=3)[:n]):
        r = c.post("/score", json=public_fields(e), params={"explain": i % 5 != 0})
        assert r.status_code == 200
        outs[e["txid"]] = (r.json(), i % 5 != 0)
    return c, outs


@needs_models
def test_tx_returns_stored_top_features():
    c, outs = _scored_client(60)
    for txid, (out, explained) in outs.items():
        rec = c.get(f"/tx/{txid}").json()
        if explained:
            assert rec["top_features"] == out["top_features"] and len(rec["top_features"]) == 3
        else:
            assert rec["top_features"] is None
        assert rec["score"] == out["score"] and rec["flag"] == out["flag"]


@needs_models
def test_decisions_filter_order_and_limit():
    c, outs = _scored_client()
    flagged = [t for t, (o, _) in outs.items() if o["flag"]]
    assert flagged, "на синтетиці з FLAG_RATE=0.05 мають бути позначені транзакції"

    items = c.get("/decisions", params={"flagged": True, "limit": 500}).json()["items"]
    assert {i["txid"] for i in items} == set(flagged)
    assert all(i["flag"] for i in items)
    keys = [(i["ts"], i["txid"]) for i in items]
    assert keys == sorted(keys, reverse=True)
    for i in items:
        out, explained = outs[i["txid"]]
        assert i["score"] == out["score"] and i["threshold"] == out["threshold"]
        assert i["top_features"] == (out["top_features"] if explained else None)

    rest = c.get("/decisions", params={"flagged": False, "limit": 500}).json()["items"]
    assert not any(i["flag"] for i in rest) and len(rest) + len(items) == len(outs)

    every = c.get("/decisions", params={"limit": 7}).json()["items"]
    assert len(every) == 7
    assert c.get("/decisions", params={"limit": 0}).status_code == 422


@needs_models
def test_decisions_shows_label():
    c, outs = _scored_client(40)
    txid = next(iter(outs))
    assert c.post("/labels", json={"txid": txid, "fraud": True, "at": 123}).json() == {"ok": True}
    items = c.get("/decisions", params={"limit": 500}).json()["items"]
    assert next(i for i in items if i["txid"] == txid)["fraud_label_at"] == 123


@needs_models
def test_decisions_needs_sqlite():
    c = TestClient(create_app(MODELS, "main", 0.01, backend="memory"))
    assert c.get("/decisions").status_code == 501


@needs_models
def test_openapi_documents_decisions():
    c = TestClient(create_app(MODELS, "main", 0.01, backend="memory"))
    op = c.get("/openapi.json").json()["paths"]["/decisions"]["get"]
    assert op["tags"] and op["summary"] and op["description"]
    assert {"200", "501"} <= set(op["responses"])


def test_old_database_gets_top_features_column(tmp_path):
    """База зі старою схемою decisions (без top_features) відкривається, дані не губляться."""
    path = str(tmp_path / "old.db")
    db = sqlite3.connect(path)
    db.executescript("""
        CREATE TABLE tx (txid TEXT PRIMARY KEY, card1 REAL NOT NULL, addr1 REAL, em TEXT, fp TEXT,
                         ts INTEGER NOT NULL, amt REAL NOT NULL, is_new INTEGER NOT NULL, fraud_at REAL);
        CREATE TABLE decisions (txid TEXT PRIMARY KEY, ts INTEGER, variant TEXT, score REAL, flag INTEGER,
                                threshold REAL);
        INSERT INTO tx VALUES ('old', 1.0, NULL, NULL, NULL, 10, 5.0, 1, NULL);
        INSERT INTO decisions VALUES ('old', 10, 'main', 0.7, 1, 0.4);
    """)
    db.commit()
    db.close()

    st = SqlFeatureStore(path)
    rec = st.get_tx("old")
    assert rec["score"] == 0.7 and rec["flag"] is True and rec["top_features"] is None
    st.log_decision("old", 10, "main", 0.8, True, 0.4, [{"feature": "card1", "value": 1.0, "contribution": 0.1}])
    assert st.get_tx("old")["top_features"] == [{"feature": "card1", "value": 1.0, "contribution": 0.1}]
    assert [d["txid"] for d in st.recent_decisions(flagged=True)] == ["old"]
    st.close()
