using DeltaShell.NGHS.Utils;
using GeoAPI.Extensions.Networks;

namespace DeltaShell.NGHS.IO.Helpers
{
    public static class BranchChainageExtensions
    {
        public static double CorrectlyRoundOffChainageIfChainageIsOnEndOfBranch(this IBranch branch, double readChainageProperty)
        {
            var branchLength = branch.IsLengthCustom ? branch.Length : branch.Length.TruncateByDigits();
            var chainage = readChainageProperty <= branchLength ? readChainageProperty : branchLength;
            return chainage;
        }
    }
}