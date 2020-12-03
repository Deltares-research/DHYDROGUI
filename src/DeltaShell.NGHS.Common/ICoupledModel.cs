using System.Collections.Generic;
using DelftTools.Shell.Core.Workflow.DataItems;

namespace DeltaShell.NGHS.Common
{
    /// <summary>
    /// Interface for coupling DataItems of different origins.
    /// </summary>
    /// <remarks>
    /// Currently, only used in addition to
    /// <see cref="DelftTools.Shell.Core.Workflow.IModel"/>
    /// </remarks>
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