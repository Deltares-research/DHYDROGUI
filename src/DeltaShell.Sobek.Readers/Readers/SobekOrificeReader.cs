using System;
using System.Text.RegularExpressions;
using DelftTools.Utils;
using DelftTools.Utils.RegularExpressions;
using DeltaShell.NGHS.Common.Extensions;
using DeltaShell.Sobek.Readers.SobekDataObjects;
using log4net;

namespace DeltaShell.Sobek.Readers.Readers
{
    public class SobekOrificeReader : ISobekStructureReader
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(SobekOrificeReader));

        public int Type
        {
            get
            {
                return 7;    
            }
        }
        
        public ISobekStructureDefinition GetStructure(string text)
        {
            // WHEN PROBLEMS PARSING STRUCTURE DEFINITION DUE TO SEQUENCE OF FIELDS 
            // SEE : SobekCulvertReader.GetStructure
            //string source = @"cl 0.7 cw 2 gh 0.9 mu 0.63 sc 1 rt 0";
            string pattern = RegularExpression.GetScientific("cl") +
                             RegularExpression.GetScientific("cw") +
                             RegularExpression.GetScientific("gh") +
                             RegularExpression.GetScientific("mu") +
                             RegularExpression.GetScientific("sc") +
                             RegularExpression.GetInteger("rt") +
                             //optional mp 
                             @"(mp\s(?<mp>" + RegularExpression.Integer + @"\s" + RegularExpression.Scientific + @")\s?)?" +
                             @"(mn\s(?<mn>" + RegularExpression.Integer + @"\s" + RegularExpression.Scientific + @")\s?)?";

            var match = RegularExpression.GetFirstMatch(pattern, text);
            if (match == null)
            {
                Log.WarnFormat("Could not read structure definition (\"{0}\")",text);
                return null;
            }

            return new SobekOrifice
                       {
                           CrestLevel = ConversionHelper.ToSingle(match.Groups["cl"].Value),
                           CrestWidth = ConversionHelper.ToSingle(match.Groups["cw"].Value),
                           GateHeight = ConversionHelper.ToSingle(match.Groups["gh"].Value),
                           ContractionCoefficient = ConversionHelper.ToSingle(match.Groups["mu"].Value),
                           LateralContractionCoefficient = ConversionHelper.ToSingle(match.Groups["sc"].Value),
                           FlowDirection = Convert.ToInt32((string) match.Groups["rt"].Value),
                           UseMaximumFlowPos = GetEnabled(match.Groups["mp"]),
                           MaximumFlowPos = GetValue(match.Groups["mp"]),
                           UseMaximumFlowNeg = GetEnabled(match.Groups["mn"]),
                           MaximumFlowNeg = GetValue(match.Groups["mn"])
                       };
        }
        private static bool GetEnabled(Group group)
        {
            return (group.Success) && group.Value.StartsWith("1");
        }

        private static double GetValue(Group group)
        {
            if (!group.Success)
                return 0;
            string value = group.Value.SplitOnEmptySpace()[1];
            return ConversionHelper.ToDouble(value);
        }
    }
}