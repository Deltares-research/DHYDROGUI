using System;
using Deltares.Infrastructure.API.Guards;
using GeoAPI.Extensions.Networks;

namespace DeltaShell.NGHS.Utils
{
    /// <summary>
    /// Extensions for <see cref="IBranchFeature"/>s
    /// </summary>
    public static class BranchFeatureExtensions
    {
        private const double tolerance = 1e-10;

        /// <summary>
        /// Test if <paramref name="branchFeature"/> is on the end of the branch
        /// </summary>
        /// <param name="branchFeature">Branch feature to test for</param>
        /// <returns>True if the <paramref name="branchFeature"/> is on the end of the branch</returns>
        public static bool IsOnEndOfBranch(this IBranchFeature branchFeature)
        {
            return branchFeature.TryGetNode(out var node) 
                   && branchFeature.Branch.Target == node;
        }

        /// <summary>
        /// Tries to find the node for the <paramref name="branchFeature"/> (branch, chainage).
        /// </summary>
        /// <param name="branchFeature">Feature to search for</param>
        /// <param name="node">Node found for the <paramref name="branchFeature"/></param>
        /// <returns>True if the node was found</returns>
        public static bool TryGetNode(this IBranchFeature branchFeature, out INode node)
        {
            Ensure.NotNull(branchFeature, nameof(branchFeature));
            Ensure.NotNull(branchFeature.Branch, nameof(branchFeature.Branch)); 

            node = null;

            if (branchFeature.Chainage <= tolerance)
            {
                node = branchFeature.Branch.Source;
                return true;
            }

            if (Math.Abs(branchFeature.Branch.Length - branchFeature.Chainage) < tolerance)
            {
                node = branchFeature.Branch.Target;
                return true;
            }

            return false;
        }
    }
}