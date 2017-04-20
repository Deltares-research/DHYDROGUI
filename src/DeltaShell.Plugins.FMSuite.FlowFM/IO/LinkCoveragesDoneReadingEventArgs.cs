using System;
using System.Collections.Generic;
using DelftTools.Functions;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO
{
    public class LinkCoveragesDoneReadingEventArgs : EventArgs
    {
        public LinkCoveragesDoneReadingEventArgs(IList<ITimeSeries> linkCoverageValues)
        {
            LinkCoverageValues = linkCoverageValues;
        }

        public IList<ITimeSeries> LinkCoverageValues { get; private set; }
    }
}