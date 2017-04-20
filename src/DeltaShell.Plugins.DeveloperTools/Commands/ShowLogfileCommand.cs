using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using DelftTools.Controls;

namespace DeltaShell.Plugins.DeveloperTools.Commands
{
    class ShowLogfileCommand:Command
    {
        private bool enabled=true;

        protected override void OnExecute(params object[] arguments)
        {
            var logPath = Path.Combine(Path.GetDirectoryName(Application.ExecutablePath),"DeltaShell.log");
            Process.Start(logPath);
        }

        public override bool Enabled
        {
            get { return enabled; }
        }
    }
}