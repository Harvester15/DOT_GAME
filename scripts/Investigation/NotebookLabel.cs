using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace dot_battle_signal_bus.scripts.Investigation
{
    public partial class NotebookLabel : Label
    {
        private Tween _flashTween;

        public override void _Ready()
        {
            SignalBus.Instance.ClueMarked += OnClicked;
        }


        public void OnClicked(string clueId)
        {
            this.Text = "";
            this.Text = $"{clueId} saved to notebook";
            // Safety check to kill any existing tween before starting a new one
            _flashTween?.Kill();
            _flashTween = CreateTween();

            // Fade back in to fully visible over 0.5 seconds
            _flashTween.TweenProperty(this, "modulate:a", 1.0f, 0.5);
            // Fade out to transparent over 0.5 seconds
            _flashTween.TweenProperty(this, "modulate:a", 0.0f, 0.5);

        }
    }
}
