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
            SignalBus.Instance.InvestigationAdded +=
                OnAdded;

            SignalBus.Instance.InvestigationAlreadyKnown +=
                OnAlreadyKnown;

            SignalBus.Instance.InvestigationCorrected +=
                OnCorrected;
        }

        private void OnAdded(string token)
        {
            Flash($"Added: {token}");
        }

        private void OnAlreadyKnown(string token)
        {
            Flash($"Already known: {token}");
        }

        private void OnCorrected(
            string entered,
            string resolved)
        {
            Flash(
                $"Using {resolved} instead of {entered}");
        }

        private void Flash(string text)
        {
            Text = text;

            _flashTween?.Kill();

            Modulate =
                new Color(1, 1, 1, 1);

            _flashTween = CreateTween();

            _flashTween.TweenInterval(1.0);

            _flashTween.TweenProperty(
                this,
                "modulate:a",
                0.0f,
                1.0f);
        }
    }
}
