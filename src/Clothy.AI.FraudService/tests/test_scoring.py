import heapq
import os

import numpy as np
import pytest

from src.scoring import Scorer

MODELS = "models"
pytestmark = pytest.mark.skipif(not os.path.exists(os.path.join(MODELS, "service_config.yaml")),
                                reason="models/ is missing")
REQ = dict(txid="a1", ts=1_000_000, TransactionAmt=49.0, card1=15000.0, ProductCD="W", card4="visa",
           card6="debit", addr1=299.0, addr2=87.0, P_emaildomain="gmail.com")


@pytest.mark.parametrize("variant", ["main", "device"])
def test_fixture_reproduces(variant):
    sc = Scorer(MODELS, variant)
    fx = np.load(os.path.join(MODELS, "fixture_synthetic.npz"))
    p = sc.booster.inplace_predict(fx[f"X_{variant}"])
    assert np.abs(p - fx[f"p_{variant}"]).max() < 1e-6


def test_real_fixture_if_present():
    path = os.path.join("private", "fixture_real_test.npz")
    if not os.path.exists(path):
        pytest.skip("private/fixture_real_test.npz is missing")
    fx = np.load(path)
    for variant in ("main", "device"):
        sc = Scorer(MODELS, variant)
        assert np.abs(sc.booster.inplace_predict(fx[f"X_{variant}"].astype("float32")) - fx[f"p_{variant}"]).max() < 1e-5


@pytest.mark.parametrize("backend", ["memory", "sqlite"])
@pytest.mark.parametrize("variant,n", [("main", 34), ("device", 41)])
def test_score_roundtrip(variant, n, backend):
    sc = Scorer(MODELS, variant, 0.01, backend)
    assert len(sc.order) == n
    out = sc.score("a1", REQ, debug=True)
    assert 0.0 <= out["score"] <= 1.0 and isinstance(out["flag"], bool)
    assert len(out["top_features"]) == 3 and len(out["features"]) == n
    assert sc.store.n_transactions == 1
    sc.score("a2", {**REQ, "ts": REQ["ts"] + 1}, store=False)
    assert sc.store.n_transactions == 1                        # store=False stores nothing
    assert sc.score("a3", {**REQ, "ts": REQ["ts"] + 2}, explain=False)["top_features"] == []
    with pytest.raises(ValueError):
        sc.score("a1", REQ)                                    # duplicate


@pytest.mark.parametrize("variant", ["main", "device"])
def test_sqlite_scores_equal_memory(variant):
    from src.synth import make_stream, public_fields
    evs = make_stream(MODELS, seed=1)[:700]
    a, b = Scorer(MODELS, variant, 0.01, "memory"), Scorer(MODELS, variant, 0.01, "sqlite")
    pend = []
    for e in evs:
        while pend and pend[0][0] <= e["ts"]:
            at, txid = heapq.heappop(pend)
            a.set_label(txid, True, at); b.set_label(txid, True, at)
        ra = a.score(e["txid"], public_fields(e), explain=False)
        rb = b.score(e["txid"], public_fields(e), explain=False)
        assert abs(ra["score"] - rb["score"]) < 1e-6 and ra["flag"] == rb["flag"]
        if e["_fraud"] and e["_dispute_at"] is not None:
            heapq.heappush(pend, (e["_dispute_at"], e["txid"]))
    assert b.store.n_decisions == len(evs)


def test_http_flow():
    pytest.importorskip("fastapi"); pytest.importorskip("httpx")
    from fastapi.testclient import TestClient
    from src.service import create_app
    c = TestClient(create_app(MODELS, "main", 0.01, backend="memory"))
    assert c.get("/health").json()["n_features"] == 34
    assert c.post("/score", json=REQ).status_code == 200
    assert c.post("/score", json=REQ).status_code == 409
    assert c.post("/score", json={**REQ, "txid": "x", "ts": 5}).status_code == 409   # back in time
    assert c.post("/score", json={"txid": "y"}).status_code == 422
    assert c.post("/labels", json={"txid": "a1", "fraud": True}).status_code == 200
    assert c.post("/labels", json={"txid": "zzz"}).status_code == 404
    assert c.get("/tx/a1").status_code == 501                  # memory store has no database


def test_http_sqlite_persists(tmp_path):
    pytest.importorskip("fastapi"); pytest.importorskip("httpx")
    from fastapi.testclient import TestClient
    from src.service import create_app
    path = str(tmp_path / "h.db")
    c = TestClient(create_app(MODELS, "main", 0.01, backend="sqlite", db_path=path))
    assert c.post("/score", json=REQ).status_code == 200
    rec = c.get("/tx/a1").json()
    assert rec["txid"] == "a1" and rec["variant"] == "main" and rec["flag"] in (True, False)
    assert c.post("/labels", json={"txid": "a1", "fraud": True, "at": REQ["ts"] + 100}).status_code == 200
    assert c.get("/tx/a1").json()["fraud_label_at"] == REQ["ts"] + 100
    assert c.get("/tx/nope").status_code == 404
    c.app.state.scorer.store.close()
    c2 = TestClient(create_app(MODELS, "main", 0.01, backend="sqlite", db_path=path))
    assert c2.get("/health").json()["n_transactions"] == 1


def test_synthetic_ring_mechanics():
    from src.features import FeatureStore
    from src.synth import make_stream, public_fields
    evs = make_stream(MODELS, seed=0)
    ring = [e for e in evs if e["_ring"] == 0]
    assert 5 <= len(ring) <= 7 and len({e["card1"] for e in ring}) == len(ring)
    assert all(sum(1 for x in evs if x["card1"] == e["card1"]) == 1 for e in ring)    # new cards, one transaction each
    last = max(ring, key=lambda e: (e["ts"], e["txid"]))
    st, seen = FeatureStore(), None
    for e in evs:
        if e["txid"] == last["txid"]:
            seen = st.device_features(public_fields(e))
        st.add(e["txid"], public_fields(e))
    assert seen["fp_other30"] >= 1 and seen["fp_share_new"] == 1.0