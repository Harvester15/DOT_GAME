# MECHANICS — The Inquisitor's Loop

> The investigation is PLAYABLE, not just read. Three verbs drive it:
> **Travel (Portals)**, **Confront (Bribe / Threaten / Fight)**, and **Relive (Archive of Echoes)**.
> This file is the bridge between the narrative docs and the combat code.

## 1. Portal Travel (the "coords")
The Inquisitor moves between locations through **portals**, each keyed by a coordinate.
- A portal coordinate is a **[CLUE:coord]** — it is NOT auto-saved.
- The player must **Mark** a coord to add it to the Logbook and unlock fast-travel there.
- Unmarked coords can still be visited if the player *remembers/notes them manually* (Mean Streets style).
- Some portals are **locked** until a prerequisite clue is marked (a name, a key item).

## 2. Confronting Witnesses
When a witness is **uncooperative**, the player chooses an approach. Outcome depends on the
witness's `Disposition` and what the player has already **Marked**.

| Approach | Succeeds when | Risk | Yields |
|----------|---------------|------|--------|
| **Talk** | Witness is cooperative | none | base testimony |
| **Bribe** | Witness is greedy / low loyalty | wastes resource, can insult | extra [CLUE] |
| **Threaten** | Player holds leverage (a marked incriminating clue) | may trigger **Fight** | pressured confession |
| **Fight** | Witness refuses & is armed | lose = soft fail (retreat) | forced testimony OR flees |

- **Threaten without leverage** → witness resists → may escalate to **Fight** (enters HOMM mode).
- **Fight** reuses the existing combat stack (`CombatManager.StartCombat`). A loss is a *setback*,
  not a game-over: the witness flees and that evidence path closes (other paths remain — see
  convergence in `TESTIMONIES.md`).

### Confrontation → Combat handoff (code seam)


## 3. Archive of Echoes (Rashomon battles)
Witness accounts aren't just text — selected ones are **playable HOMM battles** that recreate
the *same* event from that witness's biased viewpoint. The same battle feels different because
the **CombatContext differs per witness**.

| What differs per echo | How it's expressed in `CombatContext` |
|-----------------------|----------------------------------------|
| Who is "the hero" | swap `_attacker` / `_defender` sides |
| Army size inflation/denial | add/remove `BaseUnit`s in the lists |
| Where it happened | `Terrain` (array site vs. temple vs. pen) |
| Exaggerated odds / miracles | `CombatSettings` (buffs, forced outcomes) |

### The deduction payoff
Playing the **same** battle through 2–3 witnesses exposes contradictions the player then Marks:
- Scientist's echo: the array is *passive scenery*.
- Mage's echo: the array is an *active combatant* throwing planar damage.
- Comparing the two ⇒ **[CLUE:contradict]** → convergence fact (see `TESTIMONIES.md`).

### Echo data model (per witness recording)

## 3. Archive of Echoes (Rashomon battles)
Witness accounts aren't just text — selected ones are **playable HOMM battles** that recreate
the *same* event from that witness's biased viewpoint. The same battle feels different because
the **CombatContext differs per witness**.

| What differs per echo | How it's expressed in `CombatContext` |
|-----------------------|----------------------------------------|
| Who is "the hero" | swap `_attacker` / `_defender` sides |
| Army size inflation/denial | add/remove `BaseUnit`s in the lists |
| Where it happened | `Terrain` (array site vs. temple vs. pen) |
| Exaggerated odds / miracles | `CombatSettings` (buffs, forced outcomes) |

### The deduction payoff
Playing the **same** battle through 2–3 witnesses exposes contradictions the player then Marks:
- Scientist's echo: the array is *passive scenery*.
- Mage's echo: the array is an *active combatant* throwing planar damage.
- Comparing the two ⇒ **[CLUE:contradict]** → convergence fact (see `TESTIMONIES.md`).

### Echo data model (per witness recording)