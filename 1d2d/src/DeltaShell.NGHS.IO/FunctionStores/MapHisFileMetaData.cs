using System;
using System.Collections.Generic;

namespace DeltaShell.NGHS.IO.FunctionStores
{
    public class MapHisFileMetaData
    {
        public MapHisFileMetaData()
        {
            Parameters = new List<string>();
            Times = new List<DateTime>();
        }

        public IList<string> Parameters { get; set; }

        public IList<string> Locations { get; set; }

        public IList<DateTime> Times { get; set; }

        public int NumberOfParameters { get; set; }

        public int NumberOfLocations { get; set; }

        public int NumberOfTimeSteps { get; set; }

        public long DataBlockOffsetInBytes { get; set; }
    }
}
