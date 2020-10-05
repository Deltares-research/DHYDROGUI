using System.Collections.Generic;
using DelftTools.Shell.Core.Workflow.DataItems;

namespace DeltaShell.NGHS.Common
{
    /// <summary>
    /// Interface for models, which can be coupled
    /// to other models.
    /// </summary>
    public interface ICoupledModel
    {
        /// <summary>
        /// Method for retrieving the specific data items,
        /// which can be used for model coupling.
        /// </summary>
        /// <param name="role"> Role of the retrieved data items.</param>
        /// <returns>
        /// The data items for coupling the model based on their role.
        /// </returns>
        IEnumerable<IDataItem> GetDataItemsUsedForCouplingModel(DataItemRole role);
    }
}