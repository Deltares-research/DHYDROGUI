using System.Collections.Generic;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO.DataAccessObjects
{
    public class BcQuantityData
    {
        public BcQuantityData()
        {
            Values = new List<string>();
        }

        public string QuantityName { get; set; }
        public string Unit { get; set; }
        public string VerticalPosition { get; set; }
        public string TracerName { get; set; }

        public IList<string> Values { get; set; }
    }
}