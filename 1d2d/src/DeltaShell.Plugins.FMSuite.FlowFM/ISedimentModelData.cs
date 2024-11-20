using DelftTools.Utils.Collections.Generic;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;

namespace DeltaShell.Plugins.FMSuite.FlowFM
{
    public interface ISedimentModelData
    {
        IEventedList<ISedimentProperty> SedimentOverallProperties { get; }
        IEventedList<ISedimentFraction> SedimentFractions { get; }
        SedimentModelDataItem GetSedimentDataItem();
        string MduFilePath { get; }
    }
}