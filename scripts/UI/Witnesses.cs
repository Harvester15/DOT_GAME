using dot_battle_signal_bus.scripts.Investigation;
using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dot_battle_signal_bus.scripts.UI
{
    public partial class Witnesses : Control
    {

        public override void _Ready()
        {
            Visible = false;
            SignalBus.Instance.DisplayWitness += ShowWitness;
        }

        private void ShowWitness(WitnessData witnessData)
        {
            Toggle();
        }

        public override void _Input(InputEvent @event)
        {
            if (Input.IsActionJustPressed("witnesses"))
            {
                GD.Print("witness from witnesses");

                Toggle();
            }
            if (Input.IsActionJustPressed("archive"))
            {
                GD.Print("archive from witnesses");
            }
        }
        public void Toggle()
        {
            Visible = !Visible;
        }
    }
}
