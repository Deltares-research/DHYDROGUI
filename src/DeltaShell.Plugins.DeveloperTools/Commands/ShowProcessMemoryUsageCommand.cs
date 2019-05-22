using System;
using System.Diagnostics;
using DelftTools.Controls;
using log4net;

namespace DeltaShell.Plugins.DeveloperTools.Commands
{
    public class ShowProcessMemoryUsageCommand : Command
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(ShowProcessMemoryUsageCommand));

        protected override void OnExecute(params object[] arguments)
        {
            var process = Process.GetCurrentProcess();
            log.InfoFormat("Physical memory: {0:# ### ### ###} Kbytes", process.WorkingSet64 / 1000);
            log.InfoFormat("Private memory (heap): {0:# ### ### ###} Kbytes", process.PrivateMemorySize64 / 1000);
            log.InfoFormat("Virtual memory: {0:# ### ### ###} Kbytes", process.VirtualMemorySize64 / 1000);
            log.InfoFormat("Managed memory: {0:# ### ### ###} Kbytes", GC.GetTotalMemory(true) / 1000);
        }

        public override bool Enabled
        {
            get { return true; }
        }
    }
}