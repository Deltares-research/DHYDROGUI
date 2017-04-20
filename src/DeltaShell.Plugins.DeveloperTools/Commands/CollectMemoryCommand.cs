using System;
using DelftTools.Controls;

namespace DeltaShell.Plugins.DeveloperTools.Commands
{
    public class CollectMemoryCommand : Command
    {
        protected override void OnExecute(params object[] arguments)
        {
            GC.Collect();
        }

        public override bool Enabled
        {
            get { return true; }
        }
    }
}
