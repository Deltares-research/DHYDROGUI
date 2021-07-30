using System.Collections.Generic;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO.FouFile
{
    internal class FouFileHeader
    {
        public FouFileHeader()
        {
            Headers = new List<string>()
            {
                @"*var",
                @"tsrts",
                @"sstop",
                @"numcyc",
                @"knfac",
                @"v0plu",
                @"layno",
                @"elp"
            };
        }

        public List<string> Headers { get; }
    }
}