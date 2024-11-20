using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Deltares.Infrastructure.IO.Ini;

namespace DeltaShell.NGHS.IO.Helpers
{
    public class BcQuantityData : IBcQuantityData
    {
        public IniProperty Quantity { get; set; }
        public IniProperty Unit { get; set; }
        
        /// <summary>
        /// The line where this property was read in the file.
        /// </summary>
        public int LineNumber { get; set; }

        public IList<string> Values { get; set; }

        public BcQuantityData(IniProperty quantity)
        {
            Quantity = quantity;
            Values = new List<string>();
        }

        public BcQuantityData(IniProperty quantity, IniProperty unit, IEnumerable<double> values)
        {
            Quantity = quantity;
            Unit = unit;
            Values = values.Select(v => v.ToString(CultureInfo.InvariantCulture)).ToList();
        }
    }
}