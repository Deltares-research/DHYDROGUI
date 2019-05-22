using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using DelftTools.Controls;
using DelftTools.Controls.Swf;
using log4net;

namespace DeltaShell.Plugins.DeveloperTools.Commands
{
    public class AllocateTooMuchMemoryCommand:Command
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(AllocateTooMuchMemoryCommand));

        protected override void OnExecute(params object[] arguments)
        {
            var dialog = new InputTextDialog {Text = "Enter chunk size in Mbytes",EnteredText = "50"};

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                var pointerList = new List<IntPtr>();
                //ask chunksize 
                int size = Convert.ToInt32(dialog.EnteredText) * 1000000;
                var allocateCount = 0;
                try
                {
                    while (true)
                    {
                        var intPtr = Marshal.AllocHGlobal(size);
                        allocateCount++;
                        pointerList.Add(intPtr);
                    }
                }
                catch
                {
                    Log.Debug(string.Format("Ran out of memory after allocating {0} Mbytes in {1} chunks of {2} Mb", (allocateCount * size) / 1000000, allocateCount, size / 1000000));
                }
                finally
                {
                    //clean up our stuff
                    foreach (var ptr in pointerList)
                    {
                        Marshal.FreeHGlobal(ptr);
                    }
                }
            }
            
        }

        public override bool Enabled
        {
            get { return true; }
        }
    }
}
