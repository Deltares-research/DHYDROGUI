using System.Data;
using System.Globalization;
using DelftTools.Utils;
using DelftTools.Utils.RegularExpressions;
using DeltaShell.Sobek.Readers.SobekDataObjects;
using log4net;

namespace DeltaShell.Sobek.Readers.Readers
{
    public class SobekPumpReader : ISobekStructureReader
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(SobekPumpReader));

        public SobekPumpReader(int type)
        {
            Type = type;
        }

        public SobekPumpReader()
        {
            Type = 9;
        }

        public int Type { get; private set; }

        /// <summary>
        /// STDS id '4' nm 'Pomp 4' ty 9 dn 2 rt cr 1
        ///     TBLE
        ///     2 0.5 <
        ///     5 0.7 <
        ///     8 0.9 <
        ///     tble ct lt 1
        ///     TBLE 10 0.05 -0.20 0.05 1.50 <
        ///     10 0.10 -0.10 0.10 1.25 <
        ///     5 0.20 -0.05 0.20 1.00 <
        ///     tble stds
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public ISobekStructureDefinition GetStructure(string text)
        {
            const string dnPattern = @"dn\s(?<dn>" + RegularExpression.Integer + @")";
            
            var match = RegularExpression.GetFirstMatch(dnPattern, text);
            if (match == null)
            {
                Log.WarnFormat("Could not read structure definition (\"{0}\").", text);
                return null;
            }
            var sobekPump = new SobekPump
                                {
                                    Direction = int.Parse(match.Groups["dn"].Value)
                                };

            // WHEN PROBLEMS PARSING STRUCTURE DEFINITION DUE TO SEQUENCE OF FIELDS 
            // SEE : SobekCulvertReader.GetStructure
            const string pattern = @"((rt cr 0 (?<const>" + RegularExpression.Scientific + @")\s?" +
                                   RegularExpression.Scientific + @")|(rt cr (1|2)(?<redtable>" +
                                   RegularExpression.CharactersAndQuote + @")))\s" + @"ct lt\s(?<captable>" + 
                                   RegularExpression.CharactersAndQuote + @")";
            
            match = RegularExpression.GetFirstMatch(pattern, text);
            if (match == null)
            {
                Log.WarnFormat("Could not read pump definition (\"{0}\").", text);
                return null;
            }

            if (match.Groups["const"].Success)
            {
                // Constant Reduction as One Row Table.
                double constantReduction = ConversionHelper.ToDouble(match.Groups["const"].Value);
                var tableForConstantReduction = "TBLE\r\n0 " + constantReduction.ToString(".0#######",CultureInfo.InvariantCulture) + " <\r\ntble";
             
                sobekPump.ReductionTable = SobekDataTableReader.GetTable((string) tableForConstantReduction,
                                                                    (DataTable) sobekPump.ReductionTable);
            }
            else
            {
                sobekPump.ReductionTable = SobekDataTableReader.GetTable((string) match.Groups["redtable"].Value,
                                                                    (DataTable) sobekPump.ReductionTable);
            }
            sobekPump.CapacityTable = SobekDataTableReader.GetTable((string) match.Groups["captable"].Value,
                                                               (DataTable) sobekPump.CapacityTable);

            // Check Pump Capacity Table
            var prevCap = -1.0;  // Pump capacity must always be 0 or positive. If prevCap is set to 0 here, a zero-capacity pump will not be accepted. 
            for (int i = 0; i < sobekPump.CapacityTable.Rows.Count; i++)
            {
                var row = sobekPump.CapacityTable.Rows[i];

                if ((double) row[0] <= prevCap)
                {
                    Log.WarnFormat("Decreasing pump capacities not allowed. Pump definition: (\"{0}\").", text);
                    return null;
                }
                prevCap = (double)row[0];
            }
            
            return sobekPump;
        }
    }
}
