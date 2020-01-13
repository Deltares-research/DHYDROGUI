using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DelftTools.Utils.Data;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Concepts.Nwrw
{
    public class NwrwRrData : Unique<long>, IUrbanRrData
    {
        public virtual IList<IUrbanRrDefinition> UrbanRrGlobalDefinitions { get; set; } = new List<IUrbanRrDefinition>();
        public virtual IList<IUrbanRrDefinition> UrbanRrFlowDefinitions { get; set; } = new List<IUrbanRrDefinition>();
        public virtual IList<IUrbanRrDefinition> UrbanRrDischargeDefinitions { get; set; } = new List<IUrbanRrDefinition>();
        public void Clear()
        {
            UrbanRrGlobalDefinitions.Clear();
            UrbanRrFlowDefinitions.Clear();
            UrbanRrDischargeDefinitions.Clear();
        }
    }
}
