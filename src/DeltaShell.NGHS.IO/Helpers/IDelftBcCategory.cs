using System.Collections.Generic;

namespace DeltaShell.NGHS.IO.Helpers
{
    public interface IDelftBcCategory:IDelftIniCategory
    {
        IList<IDelftBcQuantityData> Table { get; set; }
    }
}