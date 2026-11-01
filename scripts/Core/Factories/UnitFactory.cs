using dot_battle_signal_bus.scripts.Actors.Units;
using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dot_battle_signal_bus.scripts.Core.Factories
{
    /// <summary>
    /// Stateless service that rebuilds units from a saveId. Not a Node — factories
    /// don't belong in the scene tree. Instantiated/owned by SaveManager.
    /// </summary>
    public class UnitFactory : ISaveableFactory
    {
        public string Category => "unit";

        private readonly PackedScene _baseUnitScene;
        private readonly Dictionary<string, UnitData> _registry = new();

        /// <param name="baseUnitScenePath">Path to the BaseUnit.tscn template.</param>
        /// <param name="unitDataFolder">Folder containing UnitData .tres resources.</param>
        public UnitFactory(string baseUnitScenePath, string unitDataFolder)
        {
            _baseUnitScene = GD.Load<PackedScene>(baseUnitScenePath);
            LoadRegistry(unitDataFolder);
        }

        private void LoadRegistry(string unitDataFolder)
        {
            using var dir = DirAccess.Open(unitDataFolder);
            if (dir == null)
            {
                GD.PrintErr($"UnitFactory: cannot open '{unitDataFolder}'.");
                return;
            }

            foreach (var fileName in dir.GetFiles())
            {
                var name = fileName.Replace(".remap", "");
                if (!name.EndsWith(".tres"))
                    continue;

                var data = GD.Load<UnitData>(unitDataFolder.PathJoin(name));
                if (data != null && !string.IsNullOrEmpty(data.UnitId))
                    _registry[data.UnitId] = data;
            }
        }

        public Node Create(string saveId)
        {
            if (!_registry.TryGetValue(saveId, out var data))
            {
                GD.PrintErr($"UnitFactory: unknown saveId '{saveId}'.");
                return null;
            }

            var unit = _baseUnitScene.Instantiate<BaseUnit>();
            unit.Data = data;
            return unit;
        }
    }
}
