using System;
using DeltaShell.NGHS.IO.Grid;
using GeoAPI.Extensions.Networks;

namespace DeltaShell.NGHS.IO.Helpers
{
    public static class BranchChainageExtensions
    {
        public static double CorrectlyRoundOffChainageIfChainageIsOnEndOfBranch(this IBranch branch, double readChainageProperty)
        {
            var branchLength = branch.IsLengthCustom ? branch.Length : Math.Floor(branch.Length * UGridFileHelper.DIGITS) / UGridFileHelper.DIGITS;
            var chainage = readChainageProperty <= branchLength ? readChainageProperty : branchLength;
            return chainage;
        }
    }
}