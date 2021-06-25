using System.Linq;
using DelftTools.Utils;
using DelftTools.Utils.Guards;
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
            
            foreach (ISpatialOperationSet operation in operationSet.Operations.OfType<ISpatialOperationSet>())
            {
                operation.Name = "set";
                MakeNamesUniquePerSet(operation);
            }

            NamingHelper.MakeNamesUnique(operationSet.Operations, suffixFormat: " {0}");
        } 
    }
}