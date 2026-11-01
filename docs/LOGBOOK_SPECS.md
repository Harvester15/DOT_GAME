# LOGBOOK_SPEC — Data & Interaction Contract

> One shared contract for the Dialogue Runner, Confrontation, Portal Travel, and
> Archive of Echoes. Everything keys off PLAYER-MARKED data (Mean Streets style).
> Nothing is auto-saved; the player curates the Logbook.

## Design Rules (non-negotiable)
1. **No preset questions.** Players type a **topic** or **present a marked clue**.
2. **No auto-logging.** A surfaced clue offers Mark / Read; only Mark persists it.
3. **Bounded openness.** The parser only resolves topics the player has *encountered*
   (seen in dialogue) or *Marked*. Unknown input → "You don't know enough to ask that."
4. **Logbook is the single source of truth** for leverage, fast-travel, and accusation.

---

## 1. Clue Model