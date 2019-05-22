using System.Collections.Generic;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO.DataAccessObjects
{
    public class BcmBlockData : BcBlockData
    {
        public string Location { get; set; }

        public BcmBlockData()
        {
            Quantities = new List<BcQuantityData>();
        }
    }
}
