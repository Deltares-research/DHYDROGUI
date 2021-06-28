using DelftTools.Utils.Guards;
using DeltaShell.NGHS.Common.Utils;
using SharpMap.Api.SpatialOperations;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Spatial
{
    /// <summary>
    /// Helper for spatial operations.
    /// </summary>
    public static class SpatialOperationHelper
    {
        /// <summary>
        /// Recursively makes the spatial operations names unique per spatial operation set
        /// within the specified <paramref name="operationSet"/> 
        /// </summary>
        /// <param name="operationSet"> The parent spatial operation set. </param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when <paramref name="operationSet"/> is <c>null</c>.
        /// </exception>
        public static void MakeNamesUniquePerSet(ISpatialOperationSet operationSet)
        {
            Ensure.NotNull(operationSet, nameof(operationSet));
            
            var uniqueStringProvider = new UniqueStringProvider();
            foreach (ISpatialOperation operation in operationSet.Operations)
            {
                if (operation is ISpatialOperationSet subOperationSet)
                {
                    operation.Name = uniqueStringProvider.GetUniqueStringFor("set");
                    MakeNamesUniquePerSet(subOperationSet);
                    continue;
                }

                operation.Name = uniqueStringProvider.GetUniqueStringFor(operation.Name);
            }
        } 
    }
}