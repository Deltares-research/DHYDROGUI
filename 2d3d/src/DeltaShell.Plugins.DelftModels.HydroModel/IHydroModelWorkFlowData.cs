using System.Collections.Generic;
using DelftTools.Shell.Core.Workflow.DataItems;
using DelftTools.Utils.Data;

namespace DeltaShell.Plugins.DelftModels.HydroModel
{
    public interface IHydroModelWorkFlowData : IUnique<long>
    {
        /// <summary>
        /// Output data of the workflow
        /// </summary>
        IEnumerable<IDataItem> OutputDataItems { get; }
    }
}