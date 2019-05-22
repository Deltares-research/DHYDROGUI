using System;
using System.Threading;
using DelftTools.Controls;

namespace DeltaShell.Plugins.DeveloperTools.Commands
{
    public class ThrowExceptionOnNewThreadCommand : Command
    {
        protected override void OnExecute(params object[] arguments)
        {
            var method = new ThreadStart(ThrowException);
            new Thread(method).Start();
        }

        private void ThrowException()
        {
            throw new InvalidOperationException("test exception on new thread");
        }

        public override bool Enabled
        {
            get { return true; }
        }
    }
}