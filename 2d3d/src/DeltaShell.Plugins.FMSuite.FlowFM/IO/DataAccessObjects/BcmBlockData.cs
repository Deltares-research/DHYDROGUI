using System.Collections.Generic;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO.DataAccessObjects
{
    public class BcmBlockData : BcBlockData
    {
        public BcmBlockData()
        {
            Quantities = new List<BcQuantityData>();
        }

        public string Location { get; set; }
    }
}