using System.Collections.Generic;
using System.Linq;

namespace DeltaShell.NGHS.IO.Tests.Grid
{
    public static class TwoDimensionalArrayExtensions
    {
        public static IEnumerable<int>[] ConvertToTwoOneDimensionalArrays(this int[,] twoDimensionalArray)
        {
            return twoDimensionalArray.Cast<int>().Select((v, i) => new
            {
                i = i / twoDimensionalArray.GetLength(1),
                v
            }).GroupBy(e => e.i).Select(g => g.Select(e => e.v)).ToArray();
        }
    }
}