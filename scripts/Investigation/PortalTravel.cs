using Godot;
using System.Collections.Generic;
using System.Linq;

namespace dot_battle_signal_bus.scripts.Investigation
{
    /// <summary>
    /// Fast-travel through portals. Only MARKED coordinate clues are valid destinations,
    /// so the player must deliberately keep a coord before they can jump to it.
    /// Emits a request signal; the world/scene performs the actual teleport (decoupled).
    /// </summary>
    public partial class PortalTravel : Node
    {
        private readonly Logbook _logbook;

        public PortalTravel(Logbook logbook) => _logbook = logbook;

        /// <summary>Destinations the player has unlocked by marking coord clues.</summary>
        public IEnumerable<ClueEntry> AvailablePortals() => _logbook.KnownPortals();

        /// <summary>
        /// Attempt to travel to a marked coord clue. Returns false if unknown/unmarked.
        /// </summary>
        public bool TravelTo(string coordClueId)
        {
            var clue = _logbook.KnownPortals()
                                .FirstOrDefault(c => c.ClueId == coordClueId);
            if (clue == null)
            {
                SignalBus.Instance.EmitSignal(
                    SignalBus.SignalName.DialogueLineShown,
                    "You haven't recorded that location.");
                return false;
            }

            SignalBus.Instance.EmitSignal(
                SignalBus.SignalName.PortalTravelRequested, clue.ClueId, clue.Coord);
            return true;
        }
    }
}