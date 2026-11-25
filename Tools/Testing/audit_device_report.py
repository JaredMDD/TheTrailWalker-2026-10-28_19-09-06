"""Milestone 16 slice 2: turns the phone's audit-*.json files (AuditRecorder) into Docs/Testing/milestone-16-device.md.

Usage: python Tools/Testing/audit_device_report.py <folder with pulled audit-*.json> [--out Docs/Testing/milestone-16-device.md] [--build GUID-prefix]

--build keeps only one build's files (a recheck after fixes is reported on its own).

Budgets (spec section 7): median frame <= 33.3 ms (30 fps), p95 <= 50 ms, <= 300 MB allocated in any context, and memory
flat (+-5%) across repeated entries of the same context. Builds/Validation/apk-build.json maps the build GUID to a commit.
"""
import argparse
import glob
import json
import os
import sys

MEDIAN_MS, P95_MS, ALLOCATED_MB, FLAT = 1000 / 30, 50.0, 300.0, 0.05
ORDER = ["trail", "camp", "settlement street", "house interior", "backyard", "counters", "sheets"]


def load(folder, build=None):
    files = []
    for path in sorted(glob.glob(os.path.join(folder, "audit-" + (build or "") + "*.json"))):
        with open(path, encoding="utf-8") as f:
            data = json.load(f)
        data["_file"] = os.path.basename(path)
        files.append(data)
    return files


def merge(files):
    """Frame-weighted totals per context across every file (percentiles: the worst file's value, conservative)."""
    out = {}
    for data in files:
        for c in data.get("contexts", []):
            m = out.setdefault(c["context"], {"frames": 0, "seconds": 0.0, "median": 0.0, "p95": 0.0, "worst": 0.0, "alloc": 0.0, "reserved": 0.0, "gc": 0, "files": 0})
            if c["frames"] == 0:
                continue
            m["median"] = (m["median"] * m["frames"] + c["medianMs"] * c["frames"]) / (m["frames"] + c["frames"])
            m["frames"] += c["frames"]; m["seconds"] += c["seconds"]; m["files"] += 1
            m["p95"] = max(m["p95"], c["p95Ms"]); m["worst"] = max(m["worst"], c["worstMs"])
            m["alloc"] = max(m["alloc"], c["allocatedMaxMb"]); m["reserved"] = max(m["reserved"], c["reservedMaxMb"]); m["gc"] += c["gcCount"]
    return out


def trends(files):
    """Allocated memory at each entry of a context, per file: flat when the last entry is within 5% of the first."""
    rows = []
    for data in files:
        entries = {}
        for e in data.get("events", []):
            if e["kind"] == "enter" and e.get("context"):
                entries.setdefault(e["context"], []).append(e["allocatedMb"])
        for ctx, values in entries.items():
            if len(values) >= 3:
                growth = (values[-1] - values[0]) / values[0] if values[0] > 0 else 0
                rows.append((data["_file"], ctx, len(values), values[0], values[-1], growth))
    return rows


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument("folder")
    parser.add_argument("--out", default="Docs/Testing/milestone-16-device.md")
    parser.add_argument("--build", default=None)
    args = parser.parse_args()
    files = load(args.folder, args.build)
    if not files:
        sys.exit("No audit-*.json files in " + args.folder)
    build = {}
    if os.path.exists("Builds/Validation/apk-build.json"):
        with open("Builds/Validation/apk-build.json", encoding="utf-8") as f:
            build = json.load(f)
    first = files[0]
    guids = sorted({d.get("build", "") for d in files})
    def commit_of(guid):
        return build.get("commit", "unknown") if build.get("buildGuid") == guid else "unknown (not the last APK in apk-build.json)"
    merged = merge(files)
    failures = []
    md = ["# Milestone 16 device measurements", "",
          f"Device {first['device']} ({first['os']}, {first['gpu']}), screen {first['screenWidth']}x{first['screenHeight']}, target {first['targetFps']} fps.",
          "Builds: " + "; ".join(f"{g} = {commit_of(g)}" for g in guids) + f". Files: {len(files)} ({', '.join(d['_file'] for d in files)}).",
          "", "Budgets: median <= 33.3 ms, p95 <= 50 ms, <= 300 MB allocated, memory flat (+-5%) across repeated entries.", "",
          "| Context | Time | Frames | Median ms | p95 ms | Worst ms | Allocated max MB | Reserved max MB | GC | Result |",
          "| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |"]
    for ctx in sorted(merged, key=lambda k: ORDER.index(k) if k in ORDER else 99):
        m = merged[ctx]
        problems = []
        if m["median"] > MEDIAN_MS: problems.append("median")
        if m["p95"] > P95_MS: problems.append("p95")
        if m["alloc"] > ALLOCATED_MB: problems.append("memory")
        failures += [ctx + ": " + p for p in problems]
        md.append(f"| {ctx} | {m['seconds'] / 60:.1f} min | {m['frames']} | {m['median']:.1f} | {m['p95']:.1f} | {m['worst']:.1f} | {m['alloc']:.0f} | {m['reserved']:.0f} | {m['gc']} | {'within budget' if not problems else '**over: ' + ', '.join(problems) + '**'} |")
    missing = [c for c in ORDER if c not in merged]
    if missing:
        md += ["", "Not measured: " + ", ".join(missing) + "."]
    md += ["", "## Memory across repeated entries", "", "| File | Context | Entries | First MB | Last MB | Change | Result |", "| --- | --- | --- | --- | --- | --- | --- |"]
    for file, ctx, n, a, b, g in trends(files):
        flat = abs(g) <= FLAT or g < 0
        if not flat: failures.append(f"{ctx}: memory grew {g:+.1%} over {n} entries")
        md.append(f"| {file} | {ctx} | {n} | {a:.0f} | {b:.0f} | {g:+.1%} | {'flat' if flat else '**grows**'} |")
    md += ["", "## Events", ""]
    for data in files:
        kinds = {}
        for e in data.get("events", []):
            kinds[e["kind"]] = kinds.get(e["kind"], 0) + 1
        resumed = f", relaunch to first recorded frame {data['firstFrameSeconds']:.1f} s" if data.get("firstFrameSeconds", 0) > 0 else ""
        md.append(f"- {data['_file']}: " + ", ".join(f"{k} {v}" for k, v in sorted(kinds.items())) + resumed)
    md += ["", "## Result", ""] + (["All contexts within budget."] if not failures else ["- " + f for f in failures])
    os.makedirs(os.path.dirname(args.out), exist_ok=True)
    with open(args.out, "w", encoding="utf-8", newline="\n") as f:
        f.write("\n".join(md) + "\n")
    print("\n".join(md))


if __name__ == "__main__":
    main()
