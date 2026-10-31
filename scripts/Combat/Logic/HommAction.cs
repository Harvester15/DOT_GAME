using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Godot;

namespace dot_battle_signal_bus.scripts.Combat.Logic
{
        public enum HommActionType { Move, Attack, Defend, Wait, Cast }

        /// <summary>A single resolved decision for a unit's turn.</summary>
        public readonly struct HommAction
        {
            public HommActionType Type { get; }
            public Vector2I TargetCell { get; }
            public BaseUnit TargetUnit { get; }

            private HommAction(HommActionType type, Vector2I cell, BaseUnit unit)
            {
                Type = type; TargetCell = cell; TargetUnit = unit;
            }

            public static HommAction Move(Vector2I cell) => new(HommActionType.Move, cell, null);
            public static HommAction Attack(BaseUnit target) => new(HommActionType.Attack, Vector2I.Zero, target);
            public static HommAction Cast(BaseUnit target) => new(HommActionType.Cast, Vector2I.Zero, target);
            public static HommAction Defend() => new(HommActionType.Defend, Vector2I.Zero, null);
            public static HommAction Wait() => new(HommActionType.Wait, Vector2I.Zero, null);
        }
}
