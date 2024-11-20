using System;
using System.Collections.Generic;

namespace DeltaShell.Plugins.DelftModels.WaterQualityModel.IO
{
    /// <summary>
    /// Class for the meta data of a map file
    /// </summary>
    public class MapFileMetaData
    {
        public MapFileMetaData()
        {
            Substances = new List<string>();
            SubstancesMapping = new Dictionary<string, string>();
            Times = new List<DateTime>();
        }

        public IList<string> Substances { get; set; }

        public IDictionary<string, string> SubstancesMapping { get; set; }

        public IList<DateTime> Times { get; set; }

        public int NumberOfSubstances { get; set; }

        public int NumberOfSegments { get; set; }

        public int NumberOfTimeSteps { get; set; }

        public long DataBlockOffsetInBytes { get; set; }
    }
}