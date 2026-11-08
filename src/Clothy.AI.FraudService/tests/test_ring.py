import os

import pytest

from src.ring import ring_signal
from src.scoring import Scorer

MODELS = "models"
needs_models = pytest.mark.skipif(not os.path.exists(os.path.join(MODELS, "service_config.yaml")),
                                  reason="models/ відсутня")
P = {"k": 3, "s": 0.8, "warmup_days": 14}
DAY = 86400


def test_ring_rule_unit():
    nan = float("nan")
    dev = lambda o, s: {"fp_other30": o, "fp_share_new": s}
    assert ring_signal(dev(3.0, 1.0), P, 20)["suspect"] is True
    assert ring_signal(dev(2.0, 1.0), P, 20)["suspect"] is False      # мало карток
    assert ring_signal(dev(5.0, 0.5), P, 20)["suspect"] is False      # мало нових
    assert ring_signal(dev(nan, nan), P, 20)["suspect"] is False      # немає відбитка
    assert ring_signal(dev(0.0, nan), P, 20)["suspect"] is False
    r = ring_signal(dev(9.0, 1.0), P, 5)                              # історія ще не прогріта
    assert r["suspect"] is False and r["active"] is False


@needs_models
@pytest.mark.parametrize("backend", ["memory", "sqlite"])
def test_synthetic_ring_is_flagged(backend):
    from src.synth import make_stream, public_fields
    evs = make_stream(MODELS, seed=0)
    sc = Scorer(MODELS, "main", 0.01, backend, ":memory:", ring_params=P)
    ring_events = [e for e in evs if e["_ring"] == 0]
    res = {}
    for e in evs:
        out = sc.score(e["txid"], public_fields(e), explain=False)
        if e["_ring"] == 0:
            res[e["txid"]] = out["ring"]
    first, last = ring_events[0], ring_events[-1]
    assert res[first["txid"]]["suspect"] is False and res[first["txid"]]["other_cards_30d"] == 0
    assert res[last["txid"]]["suspect"] is True and res[last["txid"]]["share_new"] == 1.0


@needs_models
def test_http_ring_flow(tmp_path):
    pytest.importorskip("fastapi"); pytest.importorskip("httpx")
    from fastapi.testclient import TestClient
    from src.service import create_app
    c = TestClient(create_app(MODELS, "main", 0.01, backend="sqlite", db_path=str(tmp_path / "r.db"), ring_params=P))
    base = dict(TransactionAmt=10.0, addr1=299.0, P_emaildomain="gmail.com",
                DeviceInfo="ringdev", id_30="o", id_31="b", id_33="s")
    assert c.post("/score", json={"txid": "w", "ts": 0, "card1": 1.0, "TransactionAmt": 10.0}).status_code == 200
    flags = []
    for i in range(4):
        r = c.post("/score", json={**base, "txid": f"t{i}", "ts": 20 * DAY + i * 60, "card1": 100.0 + i})
        assert r.status_code == 200
        flags.append(r.json()["ring"]["suspect"])
    assert flags == [False, False, False, True]                         # k = 3: з четвертої картки
    assert c.get("/tx/t3").json()["ring_alert"]["other_cards_30d"] == 3
    assert c.get("/tx/t0").json()["ring_alert"] is None
    alerts = c.get("/rings").json()["alerts"]
    assert [a["txid"] for a in alerts] == ["t3"]
    c.app.state.scorer.store.close()