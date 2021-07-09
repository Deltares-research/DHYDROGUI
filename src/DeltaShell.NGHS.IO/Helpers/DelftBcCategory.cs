using System.Collections.Generic;

namespace DeltaShell.NGHS.IO.Helpers
{
    public class DelftBcCategory : DelftIniCategory, IDelftBcCategory
    {
        public IList<IDelftBcQuantityData> Table { get; set; }

        public DelftBcCategory(string categoryName) : base(categoryName)
        {
            Table = new List<IDelftBcQuantityData>();
        }
    }
}