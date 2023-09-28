using System.Collections.Generic;
using DHYDRO.Common.IO.Ini;

namespace DeltaShell.NGHS.IO.Helpers
{
    public interface IBcQuantityData
    {
        IniProperty Quantity { get; set; }
        IniProperty Unit { get; set; }

        /// <summary>
        /// The line where this property was read in the file.
        /// </summary>
        int LineNumber { get; set; }

        IList<string> Values { get; set; }
    }
}