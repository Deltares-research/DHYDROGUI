using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using DelftTools.Functions.Generic;
using DelftTools.Utils.RegularExpressions;
using DeltaShell.Sobek.Readers.SobekDataObjects;
using log4net;

namespace DeltaShell.Sobek.Readers.Readers.SobekRrReaders
{
    public class SobekRRSeepageReader : SobekReader<SobekRRSeepage>
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(SobekRRSeepage));

        public override IEnumerable<SobekRRSeepage> Parse(string fileContent)
        {
            const string pattern = @"^\s*SEEP(?'text'.*?)seep" + RegularExpression.EndOfLine;

            return (from Match line in RegularExpression.GetMatches(pattern, fileContent)
                    select GetSobekRRSeepage(line.Value)).ToList();
        }

        private static SobekRRSeepage GetSobekRRSeepage(string line)
        {
            var sobekRRSeepage = new SobekRRSeepage();

            //id   =          seepage identification
            //nm  =          name 
            //co =          computation option seepage
            //                  1 = constant seepage (Default)
            //                  2 = variable seepage, using C and a table for H0
            //                  3 = variable seepage, using C and H0 from Modflow    
            //                  If the co field is missing, co 1 will be assumed. 
            //sp   =          Seepage or percolation  (mm/day)
            //                  Positive numbers represent seepage, negative numbers represent percolation.
            //                  Default 0.
            //ss   =          salt concentration seepage (mg/l). Default 500 mg/l. 
            //                  This value is only important for positive seepage values. 
            //cv   =          Resistance value C for aquitard 
            //h0  =          reference to a table with H0 values 


            //Id
            var label = "id";
            var pattern = RegularExpression.GetExtendedCharacters(label);
            var matches = RegularExpression.GetMatches(pattern, line);
            if (matches.Count == 1)
            {
                sobekRRSeepage.Id = matches[0].Groups[label].Value;
            }

            //Name
            label = "nm";
            pattern = RegularExpression.GetExtendedCharacters(label);
            matches = RegularExpression.GetMatches(pattern, line);
            if (matches.Count == 1)
            {
                sobekRRSeepage.Name = matches[0].Groups[label].Value;
            }

            //computation option
            label = "co";
            pattern = RegularExpression.GetInteger(label);
            matches = RegularExpression.GetMatches(pattern, line);
            if (matches.Count == 1)
            {
                var coAsInt = Convert.ToInt32(matches[0].Groups[label].Value);
                if (Enum.IsDefined(typeof (SeepageComputationOption), coAsInt))
                {
                    sobekRRSeepage.ComputationOption = (SeepageComputationOption) coAsInt;
                }
                else
                {
                    log.ErrorFormat("Computation option of {0} is unkown.", sobekRRSeepage.Id);
                }
            }

            //Seepage or percolation
            label = "sp";
            pattern = RegularExpression.GetScientific(label);
            matches = RegularExpression.GetMatches(pattern, line);
            if (matches.Count == 1)
            {
                sobekRRSeepage.Seepage = Convert.ToDouble(matches[0].Groups[label].Value, CultureInfo.InvariantCulture);
            }

            //Salt concentration table or constant

            sobekRRSeepage.SaltTableConcentration = GetSaltTable(sobekRRSeepage, line);

            if (sobekRRSeepage.SaltTableConcentration == null)
            {
                label = "ss";
                pattern = RegularExpression.GetScientific(label);
                matches = RegularExpression.GetMatches(pattern, line);
                if (matches.Count == 1)
                {
                    sobekRRSeepage.SaltConcentration = Convert.ToDouble(matches[0].Groups[label].Value,
                                                                        CultureInfo.InvariantCulture);
                }
            }

            //Resistance value
            label = "cv";
            pattern = RegularExpression.GetScientific(label);
            matches = RegularExpression.GetMatches(pattern, line);
            if (matches.Count == 1)
            {
                sobekRRSeepage.ResistanceValue = Convert.ToDouble(matches[0].Groups[label].Value, CultureInfo.InvariantCulture);
            }

            //H0 table
            label = "h0";
            pattern = RegularExpression.GetExtendedCharacters(label);
            matches = RegularExpression.GetMatches(pattern, line);
            if (matches.Count == 1)
            {
                sobekRRSeepage.H0TableName = matches[0].Groups[label].Value;
            }

            return sobekRRSeepage;
        }

        private static DataTable GetSaltTable(SobekRRSeepage seepage, string fileText)
        {
            const string propertiesPatternTable = @"ss\s*" + RegularExpression.CharactersAndQuote + @"\s*(?<table>TBLE(?'text'.*?)tble)";
            const string propertiesPatternPDIN = @"(PDIN (?<pdin>" + RegularExpression.CharactersAndQuote + @") pdin)";

            DataTable dataTable = null;
            MatchCollection matches;

            matches = RegularExpression.GetMatches(propertiesPatternTable, fileText);

            if (matches.Count == 1)
            {
                var tableSchema = new DataTable("salt");
                tableSchema.Columns.Add(new DataColumn("time", typeof (DateTime)));
                tableSchema.Columns.Add(new DataColumn("salt value", typeof(double)));

                var tableText = matches[0].Groups["table"].Value;
                dataTable = SobekDataTableReader.GetTable(tableText, tableSchema);
            }

            matches = RegularExpression.GetMatches(propertiesPatternPDIN, fileText);
            if (matches.Count == 1)
            {
                var pdinText = matches[0].Groups["pdin"].Value;
                SetInterAndExtrapolation(seepage, pdinText);
            }

            return dataTable;
        }

        private static void SetInterAndExtrapolation(SobekRRSeepage seepage, string pdin)
        {
            const string pdinSubPattern =
                @"(?<pdin1>" + RegularExpression.Integer + @")\s(?<pdin2>" + RegularExpression.Integer + @")" +
                @"(\s(?<period>" + RegularExpression.CharactersAndQuote + @"))?";

            var pdinSubMatches = RegularExpression.GetMatches(pdinSubPattern, pdin);

            if (pdinSubMatches.Count > 0)
            {

                string pdin1 = pdinSubMatches[0].Groups["pdin1"].Value;
                string pdin2 = pdinSubMatches[0].Groups["pdin2"].Value;
                string period = pdinSubMatches[0].Groups["period"].Value;
                if (pdin1 == "0")
                {
                    seepage.InterpolationType = InterpolationType.Linear;
                }
                else
                {
                    seepage.InterpolationType = InterpolationType.Constant;
                }

                if (pdin2 == "1")
                {
                    seepage.ExtrapolationType = ExtrapolationType.Periodic;
                    if (!string.IsNullOrEmpty(period))
                    {
                        seepage.ExtrapolationPeriod = period;
                    }
                }
                else
                {
                    seepage.ExtrapolationType = ExtrapolationType.Constant;
                }
            }
        }

        public override IEnumerable<string> GetTags()
        {
            yield return "seep";
        }
    }
}
