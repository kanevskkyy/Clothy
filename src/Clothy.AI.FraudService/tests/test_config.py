"""Перевірка конфігу при старті та логування без персональних даних."""
import logging
import os

import pytest

from src.config import ConfigError, load_settings

MODELS = "models"
pytestmark = pytest.mark.skipif(not os.path.exists(os.path.join(MODELS, "service_config.yaml")),
                                reason="models/ відсутня")


def test_defaults():
    st = load_settings(env={})
    got = (st.variant, st.flag_rate, st.backend, st.ring_params, st.log_level)
    assert got == ("main", 0.01, "memory", None, "INFO")
    assert os.path.isabs(st.models_dir) and st.db_path.endswith("history.db")


def test_explicit_args_override_env():
    st = load_settings(MODELS, "device", 0.02, "sqlite", ":memory:",
                       env={"MODEL_VARIANT": "main", "FLAG_RATE": "0.05", "STORE_BACKEND": "memory"})
    assert (st.variant, st.flag_rate, st.backend, st.db_path) == ("device", 0.02, "sqlite", ":memory:")


@pytest.mark.parametrize("value,expected", [("on", None), ("ON", None), ("1", None), ("true", None), ("yes", None),
                                            ("off", False), ("0", False), ("False", False), ("no", False)])
def test_ring_signal_values(value, expected):
    assert load_settings(env={"RING_SIGNAL": value}).ring_params is expected


def test_explicit_ring_params_skip_env():
    assert load_settings(ring_params={"k": 3, "s": 0.8, "warmup_days": 1},
                         env={"RING_SIGNAL": "garbage"}).ring_params["k"] == 3


@pytest.mark.parametrize("env,needle", [
    ({"MODEL_VARIANT": "mian"}, "MODEL_VARIANT='mian': допустимо main або device"),
    ({"FLAG_RATE": "abc"}, "FLAG_RATE='abc': має бути числом"),
    ({"FLAG_RATE": "0.04"}, "0.005, 0.01, 0.02, 0.03, 0.05"),
    ({"STORE_BACKEND": "postgres"}, "STORE_BACKEND='postgres': допустимо memory або sqlite"),
    ({"STORE_BACKEND": "sqlite", "DB_PATH": "no_such_dir_xyz/h.db"}, "не існує"),
    ({"STORE_BACKEND": "sqlite", "DB_PATH": "models"}, "це тека"),
    ({"RING_SIGNAL": "of"}, "RING_SIGNAL='of': допустимо on/1/true/yes"),
    ({"MODELS_DIR": "no_such_models_xyz"}, "MODELS_DIR="),
    ({"LOG_LEVEL": "loud"}, "LOG_LEVEL='LOUD'"),
])
def test_bad_values_have_clear_messages(env, needle):
    with pytest.raises(ConfigError) as e:
        load_settings(env=env)
    assert needle in str(e.value)


def test_memory_backend_ignores_db_path():
    assert load_settings(env={"STORE_BACKEND": "memory", "DB_PATH": "no_such_dir_xyz/h.db"}).backend == "memory"


def test_config_error_is_value_error():
    assert issubclass(ConfigError, ValueError)


def test_create_app_rejects_bad_env(monkeypatch, caplog):
    pytest.importorskip("fastapi")
    from src.service import create_app
    monkeypatch.setenv("RING_SIGNAL", "maybe")
    with caplog.at_level(logging.ERROR, logger="antifraud"), pytest.raises(ConfigError):
        create_app(MODELS, "main", 0.01, backend="memory")
    assert "RING_SIGNAL" in caplog.text


def test_logs_have_no_personal_data(monkeypatch, caplog):
    pytest.importorskip("fastapi")
    pytest.importorskip("httpx")
    from fastapi.testclient import TestClient

    from src.service import create_app
    monkeypatch.setenv("RING_SIGNAL", "on")
    req = {"txid": "a1", "ts": 1000, "TransactionAmt": 49.0, "card1": 15123.0, "addr1": 299.0,
           "P_emaildomain": "secretmail.com", "DeviceInfo": "SecretPhone", "id_30": "OS", "id_31": "B", "id_33": "1x1"}
    with caplog.at_level(logging.INFO, logger="antifraud"):
        c = TestClient(create_app(MODELS, "main", 0.01, backend="memory"))
        assert c.post("/score", json=req).status_code == 200
        assert c.post("/score", json=req).status_code == 409
        assert c.post("/score", json={**req, "txid": "a2", "card1": "not-a-number"}).status_code == 422
        assert c.post("/labels", json={"txid": "zzz"}).status_code == 404
    text = " | ".join(r.getMessage() for r in caplog.records if r.name == "antifraud")
    assert "старт: variant=main" in text and "409 /score txid=a1" in text and "404 /labels txid=zzz" in text
    assert "422 /score" in text and "card1" in text          # назва поля є, значення немає
    for secret in ("15123", "secretmail", "SecretPhone", "299", "not-a-number"):
        assert secret not in text, secret
