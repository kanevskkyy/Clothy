"""Response schemas (response_model) do not change the JSON, and /openapi.json documents endpoints and error codes."""
import heapq
import json
import os

import pytest

pytest.importorskip("fastapi")
pytest.importorskip("httpx")
from fastapi.testclient import TestClient  # noqa: E402

from src.scoring import Scorer  # noqa: E402
from src.service import create_app  # noqa: E402
from src.synth import make_stream, public_fields  # noqa: E402

MODELS = "models"
pytestmark = pytest.mark.skipif(not os.path.exists(os.path.join(MODELS, "service_config.yaml")),
                                reason="models/ is missing")
RING = {"k": 2, "s": 0.5, "warmup_days": 1}   # short warmup so the ring signal fires on synthetic data


def _plain(obj):
    """What FastAPI would return without response_model (dict -> JSON -> dict)."""
    return json.loads(json.dumps(obj))


@pytest.mark.parametrize("backend", ["memory", "sqlite"])
@pytest.mark.parametrize("variant", ["main", "device"])
def test_http_json_equals_scorer_output(variant, backend):
    evs = make_stream(MODELS, seed=2)[:400]
    c = TestClient(create_app(MODELS, variant, 0.01, backend=backend, db_path=":memory:", ring_params=RING))
    ref = Scorer(MODELS, variant, 0.01, backend, ":memory:", ring_params=RING)
    pend, n_suspect = [], 0
    for i, e in enumerate(evs):
        while pend and pend[0][0] <= e["ts"]:
            at, txid = heapq.heappop(pend)
            assert c.post("/labels", json={"txid": txid, "fraud": True, "at": at}).json() == {"ok": True}
            ref.set_label(txid, True, at)
        req = public_fields(e)
        params = {"debug": i % 3 == 0, "explain": i % 2 == 0}
        got = c.post("/score", json=req, params=params)
        assert got.status_code == 200
        want = _plain(ref.score(e["txid"], req, **params))
        assert got.json() == want
        assert ("features" in got.json()) == params["debug"]
        n_suspect += bool(want["ring"] and want["ring"]["suspect"])
        if e["_fraud"] and e["_dispute_at"] is not None:
            heapq.heappush(pend, (e["_dispute_at"], e["txid"]))
    h = c.get("/health").json()
    assert h == _plain({"status": "ok", "variant": ref.variant, "backend": ref.backend, "n_features": len(ref.order),
                        "n_transactions": ref.store.n_transactions, "flag_rate_val": ref.flag_rate,
                        "threshold": ref.threshold, "ring": ref.ring})
    if backend == "sqlite":
        for e in evs[::37]:
            assert c.get(f"/tx/{e['txid']}").json() == _plain(ref.store.get_tx(e["txid"]))
        assert c.get("/rings", params={"limit": 50}).json() == _plain({"alerts": ref.store.recent_ring_alerts(50)})
        assert n_suspect > 0   # the /tx and /rings check actually saw ring alerts
        ref.store.close()
        c.app.state.scorer.store.close()


def test_ring_off_health_and_score():
    c = TestClient(create_app(MODELS, "main", 0.01, backend="memory", ring_params=False))
    assert c.get("/health").json()["ring"] is None
    out = c.post("/score", json={"txid": "a", "ts": 10, "TransactionAmt": 1.0, "card1": 1.0}).json()
    assert out["ring"] is None and "features" not in out


def test_openapi_documents_endpoints_and_errors():
    c = TestClient(create_app(MODELS, "main", 0.01, backend="memory"))
    spec = c.get("/openapi.json").json()
    paths = spec["paths"]
    expected = {("/score", "post"): {"200", "409", "422"}, ("/labels", "post"): {"200", "404", "422"},
                ("/tx/{txid}", "get"): {"200", "404", "501"}, ("/rings", "get"): {"200", "501"},
                ("/health", "get"): {"200"}}
    for (path, method), codes in expected.items():
        op = paths[path][method]
        assert op["tags"] and op["summary"] and op["description"], path
        assert codes <= set(op["responses"]), (path, set(op["responses"]))
        assert "$ref" in op["responses"]["200"]["content"]["application/json"]["schema"], path
    schemas = spec["components"]["schemas"]
    for name in ("Tx", "Label", "ScoreResponse", "LabelResponse", "TxRecord", "HealthResponse", "RingAlertsResponse"):
        assert name in schemas, name
    tx = schemas["Tx"]
    assert set(tx["required"]) == {"txid", "ts", "TransactionAmt", "card1"}
    assert all(p.get("description") for p in tx["properties"].values())
    assert tx["examples"] and schemas["ScoreResponse"]["examples"]
