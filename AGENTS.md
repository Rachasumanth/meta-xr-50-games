# XR_Games_04 campaign (you are inside the Unity project MetaXRSim)

Goal: implement all 50 XR games defined in Specs~/ (5 chapters x 10 specs),
parallel sub-agents, Unity + Meta XR SDK, production quality. NO cheats:
no placeholder classes, no TODO-only implementations, no fabricated results,
no status inflation. Take the time each game needs.

## Manager (main session) duties
1. Read Campaign~/BLUEPRINT.md and Campaign~/manifest.json first.
2. Fix first: games with status gate_fail (spawn fix sub-agents, then re-queue).
3. Otherwise pick at most 3 pending games; spawn at most 3 parallel general
   sub-agents, ONE game each.
4. After a batch: append per-game progress lines to Campaign~/manager-log.md,
   append new slugs to Campaign~/gate-queue.txt (one slug per line),
   set their manifest status to gate_queued with attempt notes, then END run.
5. A 'complete' status is set ONLY by the PowerShell gate (from Unity's own
   test XML). Never claim complete or gate_pass yourself.

## Worker (sub-agent) duties - ONE game
Implement fully per BLUEPRINT.md and the game spec. No Unity runs. End with
BUILD_LOG.md so the manager can gate-queue honestly.

## Anti-cheat spine (hard rules)
- The gate exclusively writes gate-results.jsonl and statuses gate_pass,
  gate_fail, complete by parsing Unity's NUnit XML (Campaign~/results/run_*.xml).
- Never edit: Campaign~/gate-results.jsonl, Campaign~/results/*, manifest fields
  gate _pass/_fail/complete of other games, or attempts the gate maintains.
- Static lint in the gate rejects: missing .cs files, NotImplementedException in
  Scripts, comment-only test bodies. Keep code real.
- If the free gateway rate-limits you mid-run: end the run cleanly with notes
  (relay will relaunch and you resume from the manifest). Do not queue games you
  did not actually finish implementing.

