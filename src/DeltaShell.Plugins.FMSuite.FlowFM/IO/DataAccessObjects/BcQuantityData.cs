using System.Collections.Generic;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO.DataAccessObjects
{
    public class BcQuantityData
    {
        public IList<string> Values;

        public BcQuantityData()
        {
            Values = new List<string>();
        }

        public string QuantityName { get; set; }
        public string Unit { get; set; }
        public string VerticalPosition { get; set; }
        public string TracerName { get; set; }
    }
}