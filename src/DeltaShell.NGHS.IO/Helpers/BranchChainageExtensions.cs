using System;
using GeoAPI.Extensions.Networks;

namespace DeltaShell.NGHS.IO.Helpers
{
    public static class BranchChainageExtensions
    {
        public static double CorrectlyRoundOffChainageIfChainageIsOnEndOfBranch(this IBranch branch, double readChainageProperty)
        {
            var branchLength = branch.IsLengthCustom ? branch.Length : Math.Round(branch.Length - 0.0000005, 6);
            var chainage = readChainageProperty <= branchLength ? readChainageProperty : branchLength;
            return chainage;
        }
    }
}