#!/usr/bin/env python3
"""Validate and bundle a self-contained offline ShroudNav quest/journal guide.

No dependency installs, external network, server access, game files or player mods.
The output is a static browser page. Guides remain evidence-graded and manually tracked.
"""
import argparse
import html
import json
import pathlib
import re
import sys
from urllib.parse import urlparse

ROOT = pathlib.Path(__file__).resolve().parents[1]
SOURCE = ROOT / "quests" / "quest-guide.json"
TEMPLATE = ROOT / "quests" / "quest-guide.template.html"
OUTPUT = ROOT / "quests" / "quest-guide.html"

class QuestGuideError(ValueError): pass

def validate(data):
    if data.get("format") != "shroudnav-quest-guide-v1" or not data.get("noLiveTracking") or not data.get("noClientMod"):
        raise QuestGuideError("Unsupported guide format or safety declarations")
    qs=data.get("quests")
    if not isinstance(qs,list) or not qs or len(qs)>500:
        raise QuestGuideError("Quest guide dataset empty or oversized")
    ids=set()
    total_clues=0
    for quest in qs:
        qid=quest.get("id")
        if not isinstance(qid,str) or not re.fullmatch(r"[a-z0-9-]{3,90}",qid) or qid in ids:
            raise QuestGuideError("Quest ID duplicate or malformed")
        ids.add(qid)
        if not isinstance(quest.get("title"),str) or not quest["title"].strip() or len(quest["title"])>150:
            raise QuestGuideError("Quest title missing")
        for field in ("region","giver","summary","scope"):
            if not isinstance(quest.get(field),str) or not quest[field].strip():
                raise QuestGuideError("Missing quest field: "+field)
        if quest.get("liveTracking") is not False or quest.get("geometryVerified") is not False:
            raise QuestGuideError("Do not assert live tracking or verified GPS")
        steps=quest.get("steps")
        clues=quest.get("clues")
        if not isinstance(steps,list) or not steps or len(steps)>100:
            raise QuestGuideError("Invalid quest steps")
        if not isinstance(clues,list) or not clues or len(clues)>100:
            raise QuestGuideError("Invalid journal clue entries")
        step_ids=set()
        clue_ids=set()
        for step in steps:
            sid=step.get("id")
            if not isinstance(sid,str) or not re.fullmatch(r"[a-z0-9-]{3,90}",sid) or sid in step_ids:
                raise QuestGuideError("Duplicate/malformed step ID")
            step_ids.add(sid)
            for field in ("heading","direction","location","movement","completion"):
                if not isinstance(step.get(field),str) or not step[field].strip():
                    raise QuestGuideError("Step missing "+field)
            if step.get("confidence") not in ("source-backed","guide_only"):
                raise QuestGuideError("Unknown step evidence")
            if not isinstance(step.get("hazards"),list):
                raise QuestGuideError("Step hazards must be a list")
        for clue in clues:
            cid=clue.get("id")
            if not isinstance(cid,str) or not re.fullmatch(r"[a-z0-9-]{3,90}",cid) or cid in clue_ids:
                raise QuestGuideError("Duplicate/malformed clue ID")
            clue_ids.add(cid)
            for field in ("entry","category","foundAt","whatItMeans","nextAction","easyToMiss"):
                if not isinstance(clue.get(field),str) or not clue[field].strip():
                    raise QuestGuideError("Journal clue missing "+field)
            if clue.get("stepId") not in step_ids:
                raise QuestGuideError("Orphaned journal clue: "+cid)
            if clue.get("importance") not in ("required","optional","quest-chain"):
                raise QuestGuideError("Unclassified journal clue")
            if clue.get("confidence") not in ("source-backed","guide_only"):
                raise QuestGuideError("Unknown journal clue provenance")
        total_clues+=len(clue_ids)
        if not isinstance(quest.get("sources"),list) or not quest["sources"]:
            raise QuestGuideError("Every quest needs public source evidence")
        for url in quest["sources"]:
            u=urlparse(url)
            if u.scheme!="https" or not u.netloc or not u.path or u.username or u.password:
                raise QuestGuideError("Invalid source citation URL")
    return {"questCount":len(qs),"stepCount":sum(len(q["steps"]) for q in qs),"clueCount":total_clues}

def render(data, template):
    validate(data)
    anchor="__QUEST_DATA__"
    if template.count(anchor)!=1:
        raise QuestGuideError("HTML data marker must occur exactly once")
    packed=json.dumps(data,ensure_ascii=True,separators=(",",":"))
    # Escape script-closing sequences and HTML special chars inside embedded JSON.
    packed=packed.replace("&","\\u0026").replace("<","\\u003c").replace(">","\\u003e")
    return template.replace(anchor,packed)

def main():
    p=argparse.ArgumentParser(description=__doc__)
    p.add_argument("--check",action="store_true",help="validate data/template without touching files")
    p.add_argument("--output",default=str(OUTPUT))
    args=p.parse_args()
    try:
        data=json.loads(SOURCE.read_text(encoding="utf-8"))
        template=TEMPLATE.read_text(encoding="utf-8")
        result=render(data,template)
        summary=validate(data)
        print(json.dumps(summary,indent=2))
        if not args.check:
            target=pathlib.Path(args.output)
            target.parent.mkdir(parents=True,exist_ok=True)
            temp=target.with_suffix(target.suffix+".tmp")
            try:
                temp.write_text(result,encoding="utf-8")
                temp.replace(target)
            finally:
                if temp.exists(): temp.unlink()
            print("Wrote standalone no-install guide:",target)
        return 0
    except (QuestGuideError,ValueError,OSError,KeyError) as err:
        print("QUEST GUIDE NOT READY:",err,file=sys.stderr)
        return 2

if __name__=="__main__":
    sys.exit(main())
