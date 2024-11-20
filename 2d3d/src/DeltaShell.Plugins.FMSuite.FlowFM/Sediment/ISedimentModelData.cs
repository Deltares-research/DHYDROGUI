using DelftTools.Utils.Collections.Generic;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Sediment
{
    public interface ISedimentModelData
    {
        IEventedList<ISedimentProperty> SedimentOverallProperties { get; }
        IEventedList<ISedimentFraction> SedimentFractions { get; }
        string MduFilePath { get; }
        SedimentModelDataItem GetSedimentDataItem();
    }
}