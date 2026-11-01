# MVP ROADMAP — Planelords Investigation Slice

> Goal: VERIFY the idea is fun/playable at small scale before scaling toward a full
> HOMM-sized game (many races, large rosters). Prove the three verbs:
> **Portal travel → Confront (bribe/threaten/fight) → Relive (Archive of Echoes)**.

## Vertical Slice (the one thing that must work)
**One event, two witnesses, two playable echoes of the SAME battle, a manual Logbook,
one confrontation that can escalate to combat, and a final accusation.**
- Event: **"Array Incursion"** (`PLOTLINE.md` Phase T-3).
- Witnesses: **Scientist** + **Mage** (cleanest contradiction; exonerates the Mage).
- The hook to prove: *replaying the same fight from two viewpoints produces a markable contradiction.*

## Core Systems Needed for the Slice
| System | Purpose | Reuse / Notes |
|--------|---------|---------------|
| **Dialogue runner** | Play tagged lines; Mark/Read/Press; Bribe/Threaten | parses `[CLUE:*]`,`[TELL]`,`[BRANCH]` |
| **Logbook** | Store ONLY player-marked clues (coords, names, items) | searchable; no auto-entries |
| **Portal travel** | Move between locations by marked coordinate | reuse grid: `GridManager`, `GridPathfinding` |
| **Confrontation** | Bribe/Threaten/Fight resolution vs. `Disposition` | Fight hands off to combat |
| **Archive of Echoes** | Replay same event as per-witness HOMM battle | build `CombatContext` from `BattleEcho` |
| **Contradiction check** | Compare two marked clues → unlock line/fact | data-driven convergence table |
| **Accusation** | Resolve case from Logbook state | multiple valid evidence paths |

## Combat Reuse (important — do NOT rebuild)
- Echoes and confrontation fights both call `CombatManager.StartCombat(player, enemy)`.
- Per-witness bias is injected via `CombatContext` (`Terrain`, `CombatSettings`, side swap).
- The pluggable `ICombatSystem` stays untouched; the slice only *feeds* it different contexts.

## Milestones
1. **M0 — Spike:** dialogue tag parser + Mark/Read on one hard-coded scene.
2. **M1 — Logbook:** persist marked clues (coords/names/items); search; no auto-save.
3. **M2 — Portals:** coord-gated travel between array site & the Mage's refuge.
4. **M3 — Confront:** Bribe/Threaten vs. `Disposition`; Threaten-without-leverage → Fight handoff.
5. **M4 — Echoes:** `BattleEcho` → `CombatContext`; play Array Incursion as Scientist, then Mage.
6. **M5 — Deduction:** comparing the two echoes unlocks `[CLUE:contradict]` → convergence fact.
7. **M6 — Accusation:** evaluate Logbook → win/lose with the reasoning shown.
8. **M7 — Polish:** noir UI, clue-marked sting, save/load.

## Dependencies
- Dialogue runner **blocks** Logbook, confrontation, contradiction, accusation.
- Logbook **blocks** portals (fast-travel), confrontation leverage, accusation.
- Portals & Confront-Fight & Echoes all **depend on** existing `CombatManager`/grid code.
- Accusation **depends on** the convergence table in `TESTIMONIES.md`.

## Technical Priorities (ordered)
1. **Data-driven content** (dialogue + `BattleEcho` as assets, not hard-coded) — scales to many races later.
2. **Logbook as single source of truth** — all reactions, leverage, and fast-travel key off marked clues.
3. **CombatContext as the bias layer** — one combat engine, many "truths."
4. **Save/Load Logbook + world state early** — investigations are long sessions.
5. **Reuse combat/grid before building anything new.**
6. **Author tooling last** — ship the slice on hand-authored data.

## Explicitly Out of Scope for MVP
- The other 5 arcs (Priest, Paladin, Necromancer, Conqueror, Tamer).
- Large rosters / many races (that's the full HOMM-scale game).
- More than one escalating confrontation and one multi-witness echo event.
- Branching optional dialogue beyond one `[BRANCH]`.

## Path to the Full Game (post-slice)
- Scale witnesses 2 → 7 (full `STORY_ARCS.md`).
- Scale echoes: more shared `EventId`s, 3+ viewpoints each.
- Scale rosters/races toward HOMM depth (feeds existing `UnitRace`/`UnitData`).
- Add portal network as an overworld map of coordinates.

## Cross-References
- Verbs & combat seam → `MECHANICS.md`
- Content to author → `DIALOGUES.md`, `TESTIMONIES.md`
- Narrative truth to encode → `PLOTLINE.md`, `STORY_ARCS.md`