using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace DeltaShell.NGHS.IO.Helpers
{
    public class DelftBcQuantityData : IDelftBcQuantityData
    {
        public DelftIniProperty Quantity { get; set; }
        public DelftIniProperty Unit { get; set; }
        
        /// <summary>
        /// The line where this property was read in the file.
        /// </summary>
        public int LineNumber { get; set; }

        public IList<string> Values { get; set; }

        public DelftBcQuantityData(DelftIniProperty quantity)
        {
            Quantity = quantity;
            Values = new List<string>();
        }

        public DelftBcQuantityData(DelftIniProperty quantity, DelftIniProperty unit, IEnumerable<double> values)
        {
            Quantity = quantity;
            Unit = unit;
            Values = values.Select(v => v.ToString(CultureInfo.InvariantCulture)).ToList();
        }
    }
}