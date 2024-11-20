using System;
using DelftTools.Utils;
using DelftTools.Utils.RegularExpressions;
using DeltaShell.Sobek.Readers.SobekDataObjects;
using log4net;

namespace DeltaShell.Sobek.Readers.Readers
{
    public class SobekWeirReader : ISobekStructureReader
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(SobekWeirReader));

        public int Type
        {
            get
            {
                return 6;    
            }
        }
        
        public ISobekStructureDefinition GetStructure(string text)
        {
            // WHEN PROBLEMS PARSING STRUCTURE DEFINITION DUE TO SEQUENCE OF FIELDS 
            // SEE : SobekCulvertReader.GetStructure
            const string sobekWeirPattern =
                @"cl\s(?<cl>" + RegularExpression.Scientific + @")\s" +
                @"cw\s(?<cw>" + RegularExpression.Scientific + @")\s" +
                @"ce\s(?<ce>" + RegularExpression.Scientific + @")\s" +
                @"sc\s(?<sc>" + RegularExpression.Scientific + @")\s" +
                @"rt\s*(?<rt>" + RegularExpression.Integer + @")";

            var match = RegularExpression.GetFirstMatch(sobekWeirPattern,text);
            if (match == null)
            {
                Log.WarnFormat("Could not read weir definition (\"{0}\")",text);
                return null;
            }

            return new SobekWeir
                       {
                           CrestLevel = ConversionHelper.ToSingle(match.Groups["cl"].Value),
                           CrestWidth = ConversionHelper.ToSingle(match.Groups["cw"].Value),
                           DischargeCoefficient = ConversionHelper.ToSingle(match.Groups["ce"].Value),
                           LateralContractionCoefficient = ConversionHelper.ToSingle(match.Groups["sc"].Value),
                           FlowDirection = Convert.ToInt32((string) match.Groups["rt"].Value)
                       };
        }
    }
}