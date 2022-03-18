using GeoAPI.Extensions.Networks;

namespace DeltaShell.NGHS.IO.Helpers
{
    public static class BranchChainageExtensions
    {
        /// <summary>
        /// Makes sure that the chainage is not passed the length of then branch
        /// </summary>
        /// <param name="branch">Branch to snap to</param>
        /// <param name="chainage"></param>
        /// <returns></returns>
        public static double GetBranchSnappedChainage(this IBranch branch, double chainage)
        {
            if (chainage > branch.Length)
            {
                return branch.Length;
            }

            return chainage > 0
                       ? chainage
                       : 0;
        }
    }
}