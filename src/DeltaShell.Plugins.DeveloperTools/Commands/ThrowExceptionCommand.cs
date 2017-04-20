using System;
using DelftTools.Controls;

namespace DeltaShell.Plugins.DeveloperTools.Commands
{
    public class ThrowExceptionCommand : Command
    {
        protected override void OnExecute(params object[] arguments)
        {
            throw new InvalidOperationException("test exception");
        }

        public override bool Enabled
        {
            get { return true; }
        }
    }
}