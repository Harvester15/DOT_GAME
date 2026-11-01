# MVP VERTICAL SLICE — Prove It's Fun

> Goal: the SMALLEST playable proof of the core loop. Two pillars = two POCs,
> but the slice below is ONE build that proves the MYSTERY pillar first,
> reusing existing combat. HoMM-Lite map is the "testimony scenario" layer.

## The One Thing To Prove
> "Playing contradictory testimonies as biased battles, then reconstructing the
>  truth from clues the PLAYER chose to keep, is fun."

## Scope (hard limits)
- **3 witnesses:** Priest, Scientist, Mage (the experiment's triangle).
- **3 playable testimonies:** the three `SCENARIO_OUTLINES.md` scenarios.
- **3 convergence facts** → **one accusation** with **multiple interpretations** (`ENDINGS.md`).
- **1 hub:** Archive of Echoes.
- **No:** cities, economy loop, 7-faction roster, overworld travel network.

## The Loop (what the player does)
Archive of Echoes → pick a witness → play their testimony as a HoMM-Lite scenario → 
find Evidence Sites on the map (clues/relics/notes) → 
CHOOSE to Mark or just Read each clue (nothing auto-logs) → 
return to Archive → cross-check marked clues → unlock next witness/perspective → 
final accusation → interpretation-based ending


## Reuse (do NOT rebuild)
| Need | Already exists |
|---|---|
| Tactical battles | `WarlordsCombatSystem`, `ICombatSystem`, `CombatManager` |
| Per-witness bias | `CombatContext` + `CombatSettings` (now wired) |
| Grid + stacks | `GridManager`, `GridPathfinding`, `ArmyStack`, `Hero` |
| Content authoring | `[GlobalClass] Resource` (`UnitData`, `ClueEntry`, `BattleEcho`) |
| Investigation core | `Logbook`, `TopicParser`, `DialogueRunner`, `DeductionEngine` |
| Save/Load | `InvestigationSave` + `ScenarioSave` (in progress) |

## Build Order
1. **Archive hub scene** — list 3 witnesses, launch testimony, show Logbook.
2. **HoMM-Lite scenario map** (see code overview) — move a stack, visit Evidence Sites.
3. **Evidence Site → clue** — surfaces a `ClueEntry`; Mark/Read prompt.
4. **Testimony battle** — one biased `BattleEcho` per scenario via existing combat.
5. **Deduction** — 3 convergences unlock as clues are marked.
6. **Accusation + endings** — interpretation screen, not win/lose.
7. **Save/Load** — Logbook + scenario progress.

## Critique / Cut List
- **Cut** full economy: POC gold is a **gate/score**, not a sim.
- **Cut** recruitment trees to **3 one-click dwellings** (Refugee/Monastery/Crypt).
- **Cut** branching dialogue beyond one optional branch per witness.
- **Cut** the other 4 arcs entirely from the build.
- **If forced to cut more:** drop to **2 witnesses** (Mage + Priest) — still proves contradiction.

## Done = Fun Test
A playtester, with **no hand-holding**, plays 2–3 testimonies, marks clues themselves,
and forms a *theory* before accusing. If they argue about "what really happened" — it works.