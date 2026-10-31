using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dot_battle_signal_bus.scripts.Core.Factories
{
    public class ItemFactory : ISaveableFactory
    {
        public string Category => "item";

        private readonly Dictionary<string, PackedScene> _registry = new();

        public ItemFactory(string itemFolder)
        {
            using var dir = DirAccess.Open(itemFolder);
            if (dir == null)
            {
                GD.PrintErr($"ItemFactory: cannot open '{itemFolder}'.");
                return;
            }

            foreach (var fileName in dir.GetFiles())
            {
                var name = fileName.Replace(".remap", "");
                if (!name.EndsWith(".tscn"))
                    continue;

                _registry[name.GetBaseName()] = GD.Load<PackedScene>(itemFolder.PathJoin(name));
            }
        }

        public Node Create(string saveId)
        {
            if (!_registry.TryGetValue(saveId, out var scene))
            {
                GD.PrintErr($"ItemFactory: unknown saveId '{saveId}'.");
                return null;
            }
            return scene.Instantiate();
        }
    }
}
