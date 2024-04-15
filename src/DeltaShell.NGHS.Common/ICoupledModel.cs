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
        /// The name of the model.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Method for retrieving the specific data items,
        /// which can be used for model coupling.
        /// </summary>
        /// <param name="role"> Role of the retrieved data items.</param>
        /// <returns>
        /// The data items for coupling the model based on their role.
        /// </returns>
        IEnumerable<IDataItem> GetDataItemsUsedForCouplingModel(DataItemRole role);

        /// <summary>
        /// Gets the new data item name corresponding with the <paramref name="oldDataItemName"/>.
        /// </summary>
        /// <param name="oldDataItemName">The old data item name.</param>
        /// <returns>
        /// The up to date data item name corresponding with <paramref name="oldDataItemName"/>.
        /// </returns>
        /// <remarks>
        /// This function is necessary for backwards-compatibility reason. If the data item
        /// name has been changed since previous version, this method will ensure the data
        /// item is updated to the current expected name.
        /// If the provided <paramref name="oldDataItemName"/> is still correct, it will be
        /// returned unmodified.
        /// </remarks>
        string GetUpToDateDataItemName(string oldDataItemName);

        /// <summary>
        /// Gets the data items that match the given identifier.
        /// </summary>
        /// <param name="identifier">The identifier to get the data item for.</param>
        /// <returns>A collection of matching data items.</returns>
        IEnumerable<IDataItem> GetDataItemsByExchangeIdentifier(string identifier);

        /// <summary>
        /// Gets the item string representing the given <paramref name="dataItem"/>.
        /// </summary>
        /// <param name="dataItem">The data item.</param>
        /// <returns>The string representing the data item.</returns>
        string GetExchangeIdentifier(IDataItem dataItem);
    }
}