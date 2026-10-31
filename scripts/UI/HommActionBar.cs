using dot_battle_signal_bus.scripts.Combat.Systems;
using dot_battle_signal_bus.scripts.Combat.Logic;
using Godot;

namespace dot_battle_signal_bus.scripts.UI
{


    /// <summary>
    /// Button bar for HOMM combat. Add as "HommActionBar" (Control) under the Homm node,
    /// with four TextureButton/Button children named below.
    /// Enabled only during the active player unit's turn.
    /// </summary>
    public partial class HommActionBar : Control
    {
        [Export] public NodePath SystemPath;   // points at the HommCombatSystem node

        private HommCombatSystem _system;

        // Assign these in the editor (Button or TextureButton both work).
        private BaseButton _defendBtn => GetNode<BaseButton>("DefendBtn");
        private BaseButton _waitBtn => GetNode<BaseButton>("WaitBtn");
        private BaseButton _castBtn => GetNode<BaseButton>("CastBtn");

        public override void _Ready()
        {
            _system = GetNode<HommCombatSystem>(SystemPath);

            _defendBtn.Pressed += OnDefend;
            _waitBtn.Pressed += OnWait;
            _castBtn.Pressed += OnCast;

            // React to turn flow so buttons are only live on the player's turn.
            SignalBus.Instance.CombatTurnStarted += OnTurnStarted;
            SignalBus.Instance.CombatTurnEnded += OnTurnEnded;

            SetButtonsEnabled(false);   // start disabled until a player unit acts
        }

        public override void _ExitTree()
        {
            SignalBus.Instance.CombatTurnStarted -= OnTurnStarted;
            SignalBus.Instance.CombatTurnEnded -= OnTurnEnded;
        }

        private void OnTurnStarted(BaseUnit unit) => SetButtonsEnabled(_system.IsPlayerSide(unit));
        private void OnTurnEnded(BaseUnit unit) => SetButtonsEnabled(false);

        private void OnDefend()
        {
            SubmitAndLock(HommAction.Defend());
        }

        private void OnWait()
        {
            SubmitAndLock(HommAction.Wait());
        }

        private void OnCast()
        {
            // Don't lock/submit yet — enter targeting mode. Submit happens on target click.
            _system.BeginCastTargeting();
        }

        private void SubmitAndLock(HommAction action)
        {
            SetButtonsEnabled(false);          // prevent double-submit for one turn
            _system.SubmitPlayerAction(action);
        }

        private void SetButtonsEnabled(bool enabled)
        {
            _defendBtn.Disabled = !enabled;
            _waitBtn.Disabled = !enabled;
            _castBtn.Disabled = !enabled;
        }
    }
}
