using DelftTools.Shell.Core.Workflow.DataItems;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;

namespace DeltaShell.Plugins.FMSuite.FlowFM
{
    public interface ISedimentModelData
    {
        IEventedList<ISedimentProperty> SedimentOverallProperties { get; }
        IEventedList<ISedimentFraction> SedimentFractions { get; }
        IEventedList<IDataItem> DataItems { get; }
        string MduFilePath { get; }
    }
}