# SCENARIO OUTLINES — The Three Playable Testimonies

> Each = one HoMM-Lite map + one biased testimony battle. Same events, different truths.
> Map template (shared): Gold · Mercenary Camp · Evidence Site · Exit + 3 dwellings
> (Refugee Camp→Swordsmen, Monastery→Clerics, Crypt→Skeletons).

## Shared Map Beats
- **Start:** one stack, a Hero, minimal units.
- **Gold:** optional pickup (gates mercenaries; pure score in POC).
- **Mercenary Camp:** spend gold for extra units (risk/reward before the battle).
- **Evidence Site:** the point — surfaces a `ClueEntry` (doc/relic/note). Mark or Read.
- **Dwellings:** recruit Swordsmen / Clerics / Skeletons.
- **Exit:** triggers the testimony battle, then returns to Archive.

---

## 1. "Last Crusade" — the Paladin's testimony (Faith-dominant map)
- **Claim:** a holy victory that contained the Breach.
- **Bias (CombatSettings):** `AttackerDamageMultiplier` high, `ForcedMiracle = true`
  (his side "cannot lose"), Faith terrain buffs Clerics.
- **Evidence Site clue:** a torn honors-roll (dead-end: reads as glory).
- **Contradiction seed:** the roll's names match a missing-persons list found elsewhere.
- **Deadends** no victory just massacre.

## 2. "Forbidden Miracle" — the Priest's testimony (Essence-dominant map)
- **Claim:** pure salvation; he held death back.
- **Bias:** `ForcedMiracle = true` (no unit dies — "none were lost"),
  Essence terrain raises Skeletons unbidden mid-battle.
- **Evidence Site clue:** the liturgy-of-binding (breaking clue).
- **Contradiction seed:** the rising dead he "saved" never stop rising.
- **Dead-end:** a relic implying he was murdered.

## 3. "On The Nature Of Things" — the Scientist's testimony (Magic-dominant map)
- **Claim:** a measurement, not a summoning; and he died here.
- **Bias:** `DefenderDamageMultiplier` erratic, Magic terrain randomizes initiative
  (reality is unstable in his account).
- **Evidence Site clue:** measurement logs dated AFTER his "death" (breaking clue).
- **Contradiction seed:** his grave (found in another scenario) holds someone else.
- **Dead-end:** a sealed experiment that leads nowhere.

## Critique / Scope Discipline
- **One Evidence Site per scenario.** More sites = busywork, not depth.
- **Reuse ONE map layout** across all three; only dominance/terrain + the battle bias change. This is the cheapest way to sell "same event, different truth."
- Mercenary/Gold loop is **optional** — present to prove HoMM-Lite tension, not required to solve the mystery.

## Cross-References
- Biases → `TESTIMONIES.md`; Truths → `STORY_ARCS.md`; Combat bias fields → `CombatSettings`