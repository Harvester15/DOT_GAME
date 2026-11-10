using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dot_battle_signal_bus.scripts.Investigation
{
    public partial class WitnessButton : TextureButton
    {
        [Export]
        public WitnessData WitnessData;


        public override void _Ready()
        {
            Pressed += () =>
                SignalBus.Instance.EmitSignal(
                    SignalBus.SignalName.DisplayWitness,
                    WitnessData);           
        }

       
    }
}
