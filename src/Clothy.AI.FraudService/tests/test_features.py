import math
import random

import pytest

from src.features import (DuplicateTransactionError, FeatureStore, OutOfOrderError, normalize_request)
from src.sqlstore import SqlFeatureStore

DAY = 86400
FP = ("DeviceInfo", "id_30", "id_31", "id_33")
DEVS = [("d1", "o1", "b1", "s1"), ("d1", "o1", "b2", "s1"), ("d2", "o2", "b1", "s2")]


def new_store(kind):
    return FeatureStore() if kind == "memory" else SqlFeatureStore(":memory:")


def make_events(seed, n=400):
    rnd = random.Random(seed)
    times = sorted(rnd.randint(0, 45 * DAY) for _ in range(n))
    for k in range(0, n, 17):
        times[k] = times[max(k - 1, 0)]                      # identical times: an equal time must be excluded
    evs = []
    for i, t in enumerate(times):
        ev = {"txid": f"t{i}", "ts": t, "TransactionAmt": round(rnd.uniform(1, 300), 2),
              "card1": rnd.choice([1.0, 2.0, 3.0, 4.0, 5.0, 6.0]),
              "addr1": rnd.choice([None, 10.0, 20.0]), "P_emaildomain": rnd.choice([None, "a.com", "b.com"])}
        if rnd.random() < 0.6:
            ev.update(dict(zip(FP, rnd.choice(DEVS))))
            if rnd.random() < 0.1:
                ev[rnd.choice(FP)] = None                    # incomplete fingerprint = no fingerprint
        ev["_fraud"] = rnd.random() < 0.15
        ev["_at"] = t + rnd.randint(0, 20 * DAY) if ev["_fraud"] else None
        evs.append(ev)
    return evs


def same(a, b):
    if isinstance(a, float) and math.isnan(a):
        return isinstance(b, float) and math.isnan(b)
    return math.isclose(a, b, rel_tol=1e-9, abs_tol=1e-9)


def brute(evs, norm, newflag, i):
    e, now = norm[i], norm[i]["ts"]
    P = [(j, norm[j]) for j in range(i)]
    kf = lambda j: bool(evs[j]["_fraud"] and evs[j]["_at"] <= now)
    nan = float("nan")
    same_card = [(j, r) for j, r in P if r["c1"] == e["c1"] and r["ts"] < now]
    w30 = [(j, r) for j, r in same_card if r["ts"] >= now - 30 * DAY]
    out = {"card_n_30d": len(w30),
           "card_n_7d": sum(1 for _, r in same_card if r["ts"] >= now - 7 * DAY),
           "card_n_1d": sum(1 for _, r in same_card if r["ts"] >= now - DAY),
           "card_days_since_prev": min((now - max(r["ts"] for _, r in same_card)) / DAY, 30.0) if same_card else nan}
    mean = sum(r["amt"] for _, r in w30) / len(w30) if w30 else None
    out["card_amt_ratio_30d"] = e["amt"] / mean if (mean is not None and mean > 0) else nan
    out["pair_addr_n30"] = sum(1 for _, r in w30 if r["ad"] == e["ad"]) if e["ad"] is not None else nan
    out["pair_email_n30"] = sum(1 for _, r in w30 if r["em"] == e["em"]) if e["em"] is not None else nan
    for name, ok, cond in (("u2", e["ad"] is not None, lambda r: r["ad"] == e["ad"]),
                           ("u3", e["ad"] is not None and e["em"] is not None,
                            lambda r: r["ad"] == e["ad"] and r["em"] == e["em"])):
        if ok:
            kn = [(j, r) for j, r in same_card if cond(r) and now - 30 * DAY <= r["ts"] <= now - 7 * DAY]
            nk, fk = len(kn), sum(kf(j) for j, _ in kn)
            out[f"{name}_nk"], out[f"{name}_fk"] = nk, fk
            out[f"{name}_rate"] = fk / nk if nk else nan
        else:
            out[f"{name}_nk"] = out[f"{name}_fk"] = out[f"{name}_rate"] = nan
    if e["fp"] is not None:
        other = [(j, r) for j, r in P if r["fp"] == e["fp"] and r["c1"] != e["c1"] and r["ts"] < now]
        o30 = [(j, r) for j, r in other if r["ts"] >= now - 30 * DAY]
        kn = [(j, r) for j, r in other if now - 30 * DAY <= r["ts"] <= now - 7 * DAY]
        fk, nw = sum(kf(j) for j, _ in kn), sum(newflag[j] for j, _ in o30)
        out.update({"fp_other30": len(o30), "fp_other7": sum(1 for _, r in other if r["ts"] >= now - 7 * DAY),
                    "fp_nk": len(kn), "fp_fk": fk, "fp_rate": fk / len(kn) if kn else nan,
                    "fp_new30": nw, "fp_share_new": nw / len(o30) if o30 else nan})
    else:
        out.update({k: nan for k in ("fp_other30", "fp_other7", "fp_nk", "fp_fk", "fp_rate", "fp_new30", "fp_share_new")})
    return out


@pytest.mark.parametrize("kind", ["memory", "sqlite"])
@pytest.mark.parametrize("seed", [0, 1, 2])
def test_online_equals_brute_force(kind, seed):
    evs = make_events(seed)
    norm = [normalize_request(e) for e in evs]
    newflag = [0 if any(norm[k]["c1"] == norm[j]["c1"] and norm[k]["ts"] < norm[j]["ts"] for k in range(j)) else 1
               for j in range(len(evs))]
    st = new_store(kind)
    pending = sorted((e["_at"], j) for j, e in enumerate(evs) if e["_fraud"])
    for i, ev in enumerate(evs):
        for at, j in list(pending):                          # labels received no later than the current time
            if j < i and at <= ev["ts"]:
                st.set_label(evs[j]["txid"], True, at); pending.remove((at, j))
        got = {**st.history_features(ev), **st.device_features(ev)}
        exp = brute(evs, norm, newflag, i)
        assert set(got) == set(exp)
        for k in exp:
            assert same(float(got[k]), float(exp[k])), f"{kind}, event {i}, feature {k}: {got[k]} != {exp[k]}"
        st.add(ev["txid"], ev)


@pytest.mark.parametrize("kind", ["memory", "sqlite"])
def test_order_duplicates_and_labels(kind):
    st = new_store(kind)
    base = {"txid": "a", "ts": 1000, "TransactionAmt": 10.0, "card1": 1.0}
    st.add("a", base)
    with pytest.raises(DuplicateTransactionError):
        st.add("a", base)
    with pytest.raises(OutOfOrderError):
        st.add("b", {**base, "ts": 999})
    st.add("c", {**base, "ts": 1000})                         # equal time is allowed
    with pytest.raises(KeyError):
        st.set_label("zzz", True, 1)
    with pytest.raises(ValueError):
        st.history_features({"ts": 1, "TransactionAmt": 1.0})   # no card1


def test_sqlite_persistence(tmp_path):
    evs = make_events(5, n=150)
    path = str(tmp_path / "h.db")
    st, mem, half = SqlFeatureStore(path), FeatureStore(), 100
    for ev in evs[:half]:
        st.add(ev["txid"], ev); mem.add(ev["txid"], ev)
    st.set_label(evs[3]["txid"], True, evs[3]["ts"] + 8 * DAY); mem.set_label(evs[3]["txid"], True, evs[3]["ts"] + 8 * DAY)
    st.close()
    st2 = SqlFeatureStore(path)                              # a new "process": history and labels must persist
    assert st2.n_transactions == half
    with pytest.raises(OutOfOrderError):
        st2.check("zz", {**evs[0], "ts": evs[half - 1]["ts"] - 1})
    for ev in evs[half:]:
        a = {**st2.history_features(ev), **st2.device_features(ev)}
        b = {**mem.history_features(ev), **mem.device_features(ev)}
        for k in b:
            assert same(float(a[k]), float(b[k])), k
        st2.add(ev["txid"], ev); mem.add(ev["txid"], ev)
    st2.close()