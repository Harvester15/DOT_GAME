using Godot;
using static BaseGameObject;

namespace dot_battle_signal_bus.scripts.Combat
{
    public partial class CombatSlot : PanelContainer
    {
        public TextureRect Portrait { get; set; }
        public Team UnitTeam { get; set; }
        public string UnitName { get; set; }
        public ulong UnitId { get; set; }

        public override void _Ready()
        {
            if (Portrait != null)
            {
                AddChild(Portrait);
                // Set some default sizing so it doesn't collapse to 0x0
                Portrait.CustomMinimumSize = new Vector2(64, 64);
                Portrait.ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize;
                Portrait.StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered;
            }

            CustomMinimumSize = new Vector2(64, 64);
        }
    }
}
