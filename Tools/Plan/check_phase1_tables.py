"""Phase 1 plan-table consistency checker (stdlib only).

Run: python -I Tools/Plan/check_phase1_tables.py
Prints `FAIL <check>: <detail>` lines, then PHASE1_TABLES_OK (exit 0) or exits 1.
"""
import os
import re
import sys

ROOT = os.path.abspath(os.path.join(os.path.dirname(os.path.abspath(__file__)), "..", ".."))
PLAN = os.path.join(ROOT, "PROJECT_RECOMENDATIONS.md")
REGIONS = os.path.join(ROOT, "Assets", "_Game", "Scripts", "Domain", "Travel", "RegionPlan.cs")
APPX = {
    3: os.path.join(ROOT, "Docs", "Art", "Tier03", "tier3-complete-asset-batch-plan-2026-11-08.md"),
    4: os.path.join(ROOT, "Docs", "Art", "Tier04", "tier4-complete-asset-batch-plan-2026-11-08.md"),
}
EXPECTED_MISSIONS = {2: 69, 3: 69, 4: 68}
DENSITY_MIN, DENSITY_MAX = 0.5, 1.1


def read(path):
    with open(path, encoding="utf-8") as f:
        return f.read().splitlines()


def section(lines, start_prefix, end_pred):
    """Lines after the heading starting with start_prefix up to the next line matching end_pred."""
    out, on = [], False
    for ln in lines:
        if not on:
            on = ln.startswith(start_prefix)
            continue
        if end_pred(ln):
            break
        out.append(ln)
    return out


def parse_missions(plan):
    """{tier: {code: (region, reward_text)}} from 11.6.2-11.6.4."""
    res = {}
    row = re.compile(r"^\|\s*(T(\d)-[A-Z]\d+)\s*\|\s*(\d+)\s*\|(.*)$")
    for tier, head in ((2, "#### 11.6.2"), (3, "#### 11.6.3"), (4, "#### 11.6.4")):
        sec = section(plan, head, lambda l: l.startswith("#### "))
        res[tier] = {}
        for ln in sec:
            m = row.match(ln)
            if m and int(m.group(2)) == tier:
                cells = [c.strip() for c in m.group(4).split("|")]
                res[tier][m.group(1)] = (int(m.group(3)), cells[1] if len(cells) > 1 else "")
    return res


def parse_sets(plan):
    """List of (tier, set, slot, name, id, region) from 9.6.1."""
    sec = section(plan, "#### 9.6.1", lambda l: l.startswith("#### "))
    row = re.compile(r"^\|\s*([234]) ([AB])\s*\|\s*(\w+)\s*\|\s*(.+?) \(`(\w+)`\).*\|\s*(\d+)\s*\|\s*$")
    out = []
    for ln in sec:
        m = row.match(ln)
        if m:
            out.append((int(m.group(1)), m.group(2), m.group(3).lower(), m.group(4), m.group(5), int(m.group(6))))
    return out


def parse_camps():
    txt = "\n".join(read(REGIONS))
    return {int(n): int(c) for n, c in re.findall(r'new RegionPlanEntry\((\d+), "[^"]+", (\d+),', txt)}


def parse_merchants(plan):
    """{tier: [display names]} from 14.1 rows `| `special.tN.slot` | Name | ...`."""
    sec = section(plan, "### 14.1", lambda l: l.startswith("### "))
    out = {}
    for ln in sec:
        m = re.match(r"^\|\s*`special\.t(\d)\.\w+`\s*\|\s*([^|]+?)\s*\|", ln)
        if m:
            out.setdefault(int(m.group(1)), []).append(m.group(2))
    return out


def head_word(name):
    tok = re.split(r"[ \-]", name.strip())[0].lower()
    return re.sub(r"'s$", "", tok)


def reward_key(reward):
    m = re.search(r"\((\d[AB]) (\w+)\)\s*\**\s*$", reward)
    return (m.group(1), m.group(2).lower()) if m else None


def parse_appendix(tier):
    sec = section(read(APPX[tier]), "## Appendix A", lambda l: l.startswith("## "))
    row = re.compile(r"^\|\s*(T%d-[A-Z]\d+)\s*\|\s*(\d+)\s*\|" % tier)
    return {m.group(1): int(m.group(2)) for m in (row.match(l) for l in sec) if m}


def check_counts(missions, sets, camps, fails):
    for t, n in EXPECTED_MISSIONS.items():
        if len(missions[t]) != n:
            fails.append("FAIL counts: tier %d missions %d != %d" % (t, len(missions[t]), n))
    if len(sets) != 30:
        fails.append("FAIL counts: set rows %d != 30" % len(sets))
    if len(camps) != 24 or sorted(camps) != list(range(1, 25)):
        fails.append("FAIL counts: regions parsed %d != 24" % len(camps))


def check_set_region(missions, sets, fails):
    by_key = {}
    for t in missions:
        for code, (region, reward) in missions[t].items():
            k = reward_key(reward)
            if k:
                if k in by_key:
                    fails.append("FAIL set_region: duplicate reward %s%s %s (%s, %s)" % (k[0], "", k[1], by_key[k][0], code))
                by_key[k] = (code, region)
    for t, s, slot, name, _id, region in sets:
        k = ("%d%s" % (t, s), "backpack" if slot == "backpack" else slot)
        hit = by_key.get(k)
        if not hit:
            fails.append("FAIL set_region: no mission rewards %s %s (%s)" % (k[0], slot, name))
        elif hit[1] != region:
            fails.append("FAIL set_region: %s %s (%s) set table region %d, mission %s region %d"
                         % (k[0], slot, name, region, hit[0], hit[1]))


def check_density(missions, camps, fails):
    per = {}
    for t in missions:
        for region, _ in missions[t].values():
            per[region] = per.get(region, 0) + 1
    for r in range(7, 25):
        if r not in camps:
            continue
        d = per.get(r, 0) / camps[r]
        if not (DENSITY_MIN <= d <= DENSITY_MAX):
            fails.append("FAIL density: region %d has %d missions / %d camps = %.2f, outside [%.1f, %.1f]"
                         % (r, per.get(r, 0), camps[r], d, DENSITY_MIN, DENSITY_MAX))


def check_name_clash(sets, merchants, fails):
    if sum(len(merchants.get(t, [])) for t in (2, 3, 4)) != 15:
        fails.append("FAIL name_clash: parsed %d tier 2-4 merchant names != 15"
                     % sum(len(merchants.get(t, [])) for t in (2, 3, 4)))
    for t, s, slot, name, _id, _r in sets:
        for m in merchants.get(t, []):
            if head_word(name) == head_word(m):
                fails.append("FAIL name_clash: tier %d set %s %s '%s' vs merchant '%s' share '%s'"
                             % (t, s, slot, name, m, head_word(name)))


def check_appendix(missions, fails):
    for t, expected in ((4, 68), (3, 69)):
        app = parse_appendix(t)
        if len(app) != expected:
            fails.append("FAIL appendix: tier %d appendix rows %d != %d" % (t, len(app), expected))
        for code, region in app.items():
            src = missions[t].get(code)
            if src is None:
                fails.append("FAIL appendix: %s not in 11.6.x" % code)
            elif src[0] != region:
                fails.append("FAIL appendix: %s appendix region %d, 11.6.x region %d" % (code, region, src[0]))
        for code in missions[t]:
            if code not in app:
                fails.append("FAIL appendix: %s missing from tier %d appendix" % (code, t))


def check_skins(sets, fails):
    path = os.path.join(ROOT, "Assets", "_Game", "Scripts", "Unity", "InventoryStorageGear.cs")
    if not os.path.exists(path):
        fails.append("FAIL skins: InventoryStorageGear.cs missing")
        return
    text = "\n".join(read(path))
    have = set(re.findall(r'"(item\.[a-z_0-9]+|special\.t[1-4]\.(?:shirt|pack))"', text))
    need = {"item." + sid for _t, _s, slot, _n, sid, _r in sets if slot in ("shirt", "backpack")}
    need |= {"special.t%d.%s" % (t, k) for t in (1, 2, 3, 4) for k in ("shirt", "pack")}
    for missing in sorted(need - have):
        fails.append("FAIL skins: %s has no entry in InventoryStorageGear.cs" % missing)


def main():
    plan = read(PLAN)
    missions, sets, camps, merchants = parse_missions(plan), parse_sets(plan), parse_camps(), parse_merchants(plan)
    fails = []
    check_counts(missions, sets, camps, fails)
    check_set_region(missions, sets, fails)
    check_density(missions, camps, fails)
    check_name_clash(sets, merchants, fails)
    check_appendix(missions, fails)
    check_skins(sets, fails)
    for f in fails:
        print(f)
    if fails:
        return 1
    print("PHASE1_TABLES_OK")
    return 0


if __name__ == "__main__":
    sys.exit(main())
