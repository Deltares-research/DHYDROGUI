
using System.Collections.Generic;
using DelftTools.Utils.Data;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Concepts.Nwrw
{
    public interface IUrbanRrData : IUnique<long>
    {
        IList<IUrbanRrDefinition> UrbanRrGlobalDefinitions { get; set; }
        IList<IUrbanRrDefinition> UrbanRrFlowDefinitions { get; set; }
        IList<IUrbanRrDefinition> UrbanRrDischargeDefinitions { get; set; }
        void Clear();
    }
}
