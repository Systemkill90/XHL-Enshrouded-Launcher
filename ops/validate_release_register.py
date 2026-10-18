#!/usr/bin/env python3
"""Offline policy checks for the public Velaris change register and handoff.

No network, game files, user data, Steam or server processes accessed.
Success means documentation is internally consistent, not that anything is safe to install.
"""
import json
import pathlib
import re
import sys

ROOT = pathlib.Path(__file__).resolve().parent.parent
PHASES = ("research", "source-ready", "offline-validated", "trial-candidate",
          "public-trial-approved", "live-validated", "release-approved", "hold")
RISKS = {"low", "medium", "high", "critical"}

class RegisterError(ValueError):
    pass

def validate(data):
    if data.get("format") != "shroudforge-change-register-v1" or data.get("serverClass") != "live-public" or data.get("serverName") != "Velaris":
        raise RegisterError("Register must identify live public Velaris")
    r = data.get("rules", {})
    for key, expected in (("defaultDeployment", "none"), ("autoDeploy", False),
                          ("autoRestart", False), ("ownerApprovalRequired", True),
                          ("unknownPlayerCountBlocksDisruptiveWork", True),
                          ("sourceCiEqualsGameplay", False), ("preventSilentWorldRollback", True)):
        if r.get(key) != expected or type(r.get(key)) is not type(expected):
            raise RegisterError("Unsafe or missing policy value: " + key)
    if r.get("allowedPhases") != list(PHASES):
        raise RegisterError("Allowed phases differ from operating model")
    if data.get("hostInventoryStatus") != "pending-owner-desktop-connection":
        raise RegisterError("Do not claim a live host inventory without actual desktop access")
    entries = data.get("changeSets")
    if not isinstance(entries, list) or not entries:
        raise RegisterError("Empty change register")
    ids, prs = set(), set()
    for entry in entries:
        key, pr = entry.get("id"), entry.get("pr")
        if not isinstance(key, str) or not re.fullmatch(r"[a-z0-9-]{3,80}", key) or key in ids:
            raise RegisterError("Missing, duplicated or invalid change ID")
        ids.add(key)
        if type(pr) is not int or pr < 1 or pr in prs:
            raise RegisterError("Missing or duplicated PR number")
        prs.add(pr)
        if not isinstance(entry.get("name"), str) or not entry["name"].strip():
            raise RegisterError("Change entry requires name")
        if entry.get("phase") not in PHASES or entry.get("risk") not in RISKS:
            raise RegisterError("Invalid change phase or risk")
        if type(entry.get("requiresRestart")) is not bool or type(entry.get("playerVisible")) is not bool:
            raise RegisterError("Restart and player impact must be explicit")
        deps = entry.get("dependencies")
        if not isinstance(deps, list) or len(set(deps)) != len(deps):
            raise RegisterError("Invalid or duplicate dependency list")
        if entry.get("phase") in ("public-trial-approved", "live-validated", "release-approved"):
            raise RegisterError("Public release approval must be recorded separately after real owner action")
        if not isinstance(entry.get("decision"), str) or not entry["decision"].strip():
            raise RegisterError("Explicit disposition is required")
    for entry in entries:
        if any(d not in ids or d == entry["id"] for d in entry["dependencies"]):
            raise RegisterError("Unknown/self reference in dependency list")
    if "browser-quest-guide-22" not in ids:
        raise RegisterError("Rejected browser quest alternative must remain visible as HOLD")
    if any(e["pr"] == 22 and (e["phase"] != "hold" or e["decision"] != "rejected-closed") for e in entries):
        raise RegisterError("Closed, rejected feature cannot be in an installation queue")
    if any(e["pr"] in (21, 24) and e["phase"] not in ("hold", "research") for e in entries):
        raise RegisterError("Unverified in-game GPS/tracker research cannot be marked deployable")
    return {"changeCount": len(entries), "blockedFromAutoDeploy": len(entries), "hostInventoryConfirmed": False}

def main():
    try:
        data = json.loads((ROOT / "ops" / "change-register.json").read_text(encoding="utf-8"))
        result = validate(data)
        docs = ("docs/PUBLIC-SERVER-OPERATING-MODEL.md",
                "docs/DESKTOP-HANDOFF-2026-10-18.md",
                "docs/CHANGE-REGISTER.md")
        for filename in docs:
            if not (ROOT / filename).is_file():
                raise RegisterError("Missing required operating document: " + filename)
        print(json.dumps({"result": "PASS (documentation only)", **result}, indent=2))
        return 0
    except (RegisterError, OSError, ValueError, KeyError, TypeError) as exc:
        print("PUBLIC SERVER RELEASE GATE FAILED: " + str(exc), file=sys.stderr)
        return 2

if __name__ == "__main__":
    raise SystemExit(main())
