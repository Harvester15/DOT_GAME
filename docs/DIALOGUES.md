## Confrontation Lines (Bribe / Threaten / Fight)

> Uncooperative witnesses expose evidence only if confronted. Choices key off the
> witness `Disposition` and the player's **Marked** leverage (see `MECHANICS.md`).

### Approach Tags


**Scientist's Assistant (uncooperative):**
> "I don't talk to inquisitors."
> Player → **[BRIBE:200]**:
>   > "...Fine. The array wasn't passive. [CLUE:item outbound logs] [DISPO:+]"
> Player → **[THREATEN:needs=outbound_logs]** (if already marked):
>   > "You *have* the logs?! [TELL: panic] Ask the Mage, I just ran the dials. [BRANCH:signal]"
> Player → **[THREATEN:needs=outbound_logs]** (NOT marked = no leverage):
>   > "Threaten me with *what*? Guards! [FIGHT]"

**On Fight resolution:**
- **Win** → witness is compelled: unlocks the withheld [CLUE].
- **Lose** → witness flees; that evidence path closes (solve via convergence instead).