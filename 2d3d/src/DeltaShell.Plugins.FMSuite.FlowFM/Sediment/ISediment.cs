using DelftTools.Utils.Collections.Generic;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Sediment
{
    public interface ISediment
    {
        string Name { get; set; }
        IEventedList<ISedimentProperty> Properties { get; set; }
    }
}