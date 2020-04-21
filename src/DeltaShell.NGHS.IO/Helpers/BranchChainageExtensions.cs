using System;
using DeltaShell.NGHS.IO.Grid;
using GeoAPI.Extensions.Networks;

namespace DeltaShell.NGHS.IO.Helpers
{
    public static class BranchChainageExtensions
    {
        public const int DIGITS = (int)1E6;

        public static double CorrectlyRoundOffChainageIfChainageIsOnEndOfBranch(this IBranch branch, double readChainageProperty)
        {
            var branchLength = branch.IsLengthCustom ? branch.Length : Math.Floor(branch.Length * DIGITS) / DIGITS; //Math.Round(branch.Length, 6, MidpointRounding.ToEven);
            var chainage = readChainageProperty <= branchLength ? readChainageProperty : branchLength;
            return chainage;
        }
    }
}