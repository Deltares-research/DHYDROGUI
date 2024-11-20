using System;
using System.Collections.Generic;
using DelftTools.Utils;
using DelftTools.Utils.RegularExpressions;
using DeltaShell.Sobek.Readers.SobekDataObjects;
using log4net;

namespace DeltaShell.Sobek.Readers.Readers
{
    public class SobekRiverWeirReader : ISobekStructureReader
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(SobekRiverWeirReader));

        public int Type
        {
            get
            {
                return 0;
            }
        }
        
        public ISobekStructureDefinition GetStructure(string text)
        {
            // WHEN PROBLEMS PARSING STRUCTURE DEFINITION DUE TO SEQUENCE OF FIELDS 
            // SEE : SobekCulvertReader.GetStructure
            //cs might come before cw or after. Hence the optional cs.
            const string pattern =
                @"cl\s(?<cl>" + RegularExpression.Scientific + @")\s" +
                @"(cs\s(?<cs>" + RegularExpression.Scientific + @")\s)?" +
                @"cw\s(?<cw>" + RegularExpression.Scientific + @")\s" +
                @"(cs\s(?<cs>" + RegularExpression.Scientific + @")\s)?" +
                @"po\s(?<po>" + RegularExpression.Scientific + @")\s" +
                @"ps\s(?<ps>" + RegularExpression.Scientific + @")\s" +
                @"pt pr\s(?<postable>" + RegularExpression.CharactersAndQuote + @")\s" +
                @"no\s(?<no>" + RegularExpression.Scientific + @")\s" +
                @"ns\s(?<ns>" + RegularExpression.Scientific + @")\s" +
                @"nt nr\s(?<negtable>" + RegularExpression.CharactersAndQuote + @")";

            var match = RegularExpression.GetFirstMatch(pattern, text);
            if (match == null)
            {
                Log.WarnFormat("Could not parse weir definition (\"{0}\")",text);
                return null;
            }

            var positiveReductionTable = SobekDataTableReader.GetTable(
                match.Groups["postable"].Value,
                new Dictionary<string, Type>
                    {
                        {"first", typeof (float)},
                        {"second", typeof (float)}
                    });
            var negativeReductionTable = SobekDataTableReader.GetTable(
                match.Groups["negtable"].Value,
                new Dictionary<string, Type>
                    {
                        {"first", typeof (float)},
                        {"second", typeof (float)}
                    });
            return new SobekRiverWeir
                       {
                           CrestLevel = ConversionHelper.ToSingle(match.Groups["cl"].Value),
                           CrestWidth = ConversionHelper.ToSingle(match.Groups["cw"].Value),
                           CrestShape = Convert.ToInt32((string) match.Groups["cs"].Value),
                           CorrectionCoefficientPos = ConversionHelper.ToSingle(match.Groups["po"].Value),
                           SubmergeLimitPos = ConversionHelper.ToSingle(match.Groups["ps"].Value),
                           PositiveReductionTable = positiveReductionTable,
                           CorrectionCoefficientNeg = ConversionHelper.ToSingle(match.Groups["no"].Value),
                           SubmergeLimitNeg = ConversionHelper.ToSingle(match.Groups["ns"].Value),
                           NegativeReductionTable = negativeReductionTable
                       };
        }

        
    }
}