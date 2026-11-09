import os

import pytest

needs_models = pytest.mark.skipif(not os.path.exists(os.path.join("models", "service_config.yaml")),
                                  reason="models/ is missing")


def test_abs_paths():
    from src.service import _abs
    assert _abs(":memory:") == ":memory:"
    assert os.path.isabs(_abs("models")) and _abs("models").endswith("models")


@needs_models
def test_env_variables_are_used(monkeypatch, tmp_path):
    pytest.importorskip("fastapi"); pytest.importorskip("httpx")
    from fastapi.testclient import TestClient
    from src.service import create_app
    monkeypatch.setenv("STORE_BACKEND", "sqlite")
    monkeypatch.setenv("DB_PATH", str(tmp_path / "e.db"))
    monkeypatch.setenv("FLAG_RATE", "0.02")
    c = TestClient(create_app("models", "main"))
    h = c.get("/health").json()
    assert h["backend"] == "sqlite" and h["flag_rate_val"] == 0.02
    c.app.state.scorer.store.close()