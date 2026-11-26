using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dot_battle_signal_bus.scripts.Investigation
{
    public partial class NotebookEntry : Node
    {
        public string Id;
        public string Title;
        public string Description;

        public string RelatedText;

        public string SourceText;

        /// <summary>
        /// Free-form notes that the player can write about this entry.
        /// </summary>
        public string Notes;

    }
}
