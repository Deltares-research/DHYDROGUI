using System.Collections.Generic;

namespace DeltaShell.NGHS.IO.Helpers
{
    public interface IDelftBcQuantityData
    {
        DelftIniProperty Quantity { get; set; }
        DelftIniProperty Unit { get; set; }

        /// <summary>
        /// The line where this property was read in the file.
        /// </summary>
        int LineNumber { get; set; }
        IList<string> Values { get; set; }
    }
}