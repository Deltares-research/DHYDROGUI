using System.Collections.Generic;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO.DataAccessObjects
{
    public class BcBlockData
    {
        public BcBlockData()
        {
            Quantities = new List<BcQuantityData>();
        }

        public string FilePath { get; set; }
        public int LineNumber { get; set; }
        public string SupportPoint { get; set; }
        public string FunctionType { get; set; }
        public string SeriesIndex { get; set; }
        public string TimeInterpolationType { get; set; }
        public string VerticalPositionType { get; set; }
        public string VerticalPositionDefinition { get; set; }
        public string VerticalInterpolationType { get; set; }
        public string Offset { get; set; }
        public string Factor { get; set; }
        public IList<BcQuantityData> Quantities { get; set; }
    }
}