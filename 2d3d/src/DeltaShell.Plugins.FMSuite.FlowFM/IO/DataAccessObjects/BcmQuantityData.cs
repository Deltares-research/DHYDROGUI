using System.Collections.Generic;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO.DataAccessObjects
{
    public class BcmQuantityData : BcQuantityData
    {
        public BcmQuantityData()
        {
            Values = new List<string>();
        }

        public string ReferenceTime { get; set; }
    }
}