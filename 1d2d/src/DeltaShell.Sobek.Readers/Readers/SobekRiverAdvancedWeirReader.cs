using System;
using DelftTools.Utils;
using DelftTools.Utils.RegularExpressions;
using DeltaShell.Sobek.Readers.SobekDataObjects;
using log4net;

namespace DeltaShell.Sobek.Readers.Readers
{
    public class SobekRiverAdvancedWeirReader : ISobekStructureReader
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(SobekRiverAdvancedWeirReader));

        public int Type
        {
            get { return 1; }
        }

        public ISobekStructureDefinition GetStructure(string text)
        {
            // WHEN PROBLEMS PARSING STRUCTURE DEFINITION DUE TO SEQUENCE OF FIELDS 
            // SEE : SobekCulvertReader.GetStructure
            string pattern =
                RegularExpression.GetScientific("cl") + RegularExpression.GetScientific("sw") +
                RegularExpression.GetInteger("ni") + RegularExpression.GetScientific("ph") +
                RegularExpression.GetScientific("nh") + RegularExpression.GetScientific("pw") +
                RegularExpression.GetScientific("nw") + RegularExpression.GetScientific("pp") +
                RegularExpression.GetScientific("np") + RegularExpression.GetScientific("pa") +
                RegularExpression.GetScientific("na");

            var match = RegularExpression.GetFirstMatch(pattern, text);
            if (match == null)
            {
                Log.WarnFormat("Could not read weir structure definition (\"{0}\")",text);
                return null;
            }

            return new SobekRiverAdvancedWeir
                       {
                           CrestLevel = ConversionHelper.ToSingle(match.Groups["cl"].Value),
                           SillWidth = ConversionHelper.ToSingle(match.Groups["sw"].Value),
                           NumberOfPiers = Convert.ToInt32((string) match.Groups["ni"].Value),
                           PositiveUpstreamFaceHeight = ConversionHelper.ToSingle(match.Groups["ph"].Value),
                           NegativeUpstreamHeight = ConversionHelper.ToSingle(match.Groups["nh"].Value),
                           PositiveWeirDesignHead = ConversionHelper.ToSingle(match.Groups["pw"].Value),
                           NegativeWeirDesignHead = ConversionHelper.ToSingle(match.Groups["nw"].Value),
                           PositivePierContractionCoefficient = ConversionHelper.ToSingle(match.Groups["pp"].Value),
                           NegativePierContractionCoefficient = ConversionHelper.ToSingle(match.Groups["np"].Value),
                           PositiveAbutmentContractionCoefficient = ConversionHelper.ToSingle(match.Groups["pa"].Value),
                           NegativeAbutmentContractionCoefficient = ConversionHelper.ToSingle(match.Groups["na"].Value)
                       };
        }
    }
}