using System.Collections.Generic;
using DelftTools.Shell.Core.Workflow.DataItems;

namespace DeltaShell.NGHS.Common
{
    public interface ICoupledModel
    {
        IEnumerable<IDataItem> GetDataItemsUsedForCouplingModel(DataItemRole role);
    }
}