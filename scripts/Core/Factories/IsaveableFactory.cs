using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dot_battle_signal_bus.scripts.Core.Factories
{
        /// <summary>Rebuilds a saveable object from its id. One implementation per object category.</summary>
        public interface ISaveableFactory
        {
            /// <summary>The "type" key stored in scene state so SaveManager picks the right factory.</summary>
            string Category { get; }

            /// <summary>Creates a detached node for the given id. Caller adds it to the tree.</summary>
            Node Create(string saveId);
        }
    }
