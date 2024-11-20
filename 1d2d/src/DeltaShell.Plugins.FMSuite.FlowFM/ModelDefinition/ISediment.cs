using DelftTools.Utils.Collections.Generic;

namespace DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition
{
    public interface ISediment
    {
        string Name { get; set; }
        IEventedList<ISedimentProperty> Properties { get; set; }
    }
}