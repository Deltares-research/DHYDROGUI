using DelftTools.Utils;
using DelftTools.Utils.RegularExpressions;
using DeltaShell.Sobek.Readers.SobekDataObjects;
using log4net;

namespace DeltaShell.Sobek.Readers.Readers
{
    public class SobekGeneralStructureReader : ISobekStructureReader
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(SobekGeneralStructureReader));

        private SobekType sobekType = SobekType.Unknown;

        public SobekGeneralStructureReader(SobekType sobekType)
        {
            this.sobekType = sobekType;
        }

        public int Type
        {
            get
            {
                return 2;    
            }
        }

        public ISobekStructureDefinition GetStructure(string text)
        {
            // WHEN PROBLEMS PARSING STRUCTURE DEFINITION DUE TO SEQUENCE OF FIELDS 
            // SEE : SobekCulvertReader.GetStructure
            string pattern = RegularExpression.GetScientific("w1") +
                             RegularExpression.GetScientific("wl") +
                             RegularExpression.GetScientific("ws") +
                             RegularExpression.GetScientific("wr") +
                             RegularExpression.GetScientific("w2") +
                             RegularExpression.GetScientific("z1") +
                             RegularExpression.GetScientific("zl") +
                             RegularExpression.GetScientific("zs") +
                             RegularExpression.GetScientific("zr") +
                             RegularExpression.GetScientific("z2") +
                             RegularExpression.GetScientific("gh") +
                             RegularExpression.GetScientific("pg") +
                             RegularExpression.GetScientific("pd") +
                             RegularExpression.GetScientific("pi") +
                             RegularExpression.GetScientific("pr") +
                             RegularExpression.GetScientific("pc") +
                             RegularExpression.GetScientific("ng") +
                             RegularExpression.GetScientific("nd") +
                             RegularExpression.GetScientific("nf") +
                             RegularExpression.GetScientific("nr") +
                             RegularExpression.GetScientific("nc") + 
                             RegularExpression.GetFloatOptional("er");

            var match = RegularExpression.GetFirstMatch(pattern, text);
            if (match == null)
            {
                Log.WarnFormat("Could not read structure definition (\"{0}\")",text);
                return null;
            }

            var generalStructure =
                new SobekGeneralStructure
                    {
                        ImportFromRE = sobekType == SobekType.SobekRE,
                        WidthLeftSideOfStructure = ConversionHelper.ToSingle(match.Groups["w1"].Value),
                        WidthStructureLeftSide = ConversionHelper.ToSingle(match.Groups["wl"].Value),
                        WidthStructureCentre = ConversionHelper.ToSingle(match.Groups["ws"].Value),
                        WidthStructureRightSide = ConversionHelper.ToSingle(match.Groups["wr"].Value),
                        WidthRightSideOfStructure = ConversionHelper.ToSingle(match.Groups["w2"].Value),
                        BedLevelLeftSideOfStructure = ConversionHelper.ToSingle(match.Groups["z1"].Value),
                        BedLevelLeftSideStructure = ConversionHelper.ToSingle(match.Groups["zl"].Value),
                        BedLevelStructureCentre = ConversionHelper.ToSingle(match.Groups["zs"].Value),
                        BedLevelRightSideStructure = ConversionHelper.ToSingle(match.Groups["zr"].Value),
                        BedLevelRightSideOfStructure = ConversionHelper.ToSingle(match.Groups["z2"].Value),
                        GateHeight = ConversionHelper.ToSingle(match.Groups["gh"].Value),
                        PositiveFreeGateFlow = ConversionHelper.ToSingle(match.Groups["pg"].Value),
                        PositiveDrownedGateFlow = ConversionHelper.ToSingle(match.Groups["pd"].Value),
                        PositiveFreeWeirFlow = ConversionHelper.ToSingle(match.Groups["pi"].Value),
                        PositiveDrownedWeirFlow = ConversionHelper.ToSingle(match.Groups["pr"].Value),
                        PositiveContractionCoefficient = ConversionHelper.ToSingle(match.Groups["pc"].Value),
                        NegativeFreeGateFlow = ConversionHelper.ToSingle(match.Groups["ng"].Value),
                        NegativeDrownedGateFlow = ConversionHelper.ToSingle(match.Groups["nd"].Value),
                        NegativeFreeWeirFlow = ConversionHelper.ToSingle(match.Groups["nf"].Value),
                        NegativeDrownedWeirFlow = ConversionHelper.ToSingle(match.Groups["nr"].Value),
                        NegativeContractionCoefficient = ConversionHelper.ToSingle(match.Groups["nc"].Value)
                    };

            if (match.Groups["er"].Success)
                generalStructure.ExtraResistance = ConversionHelper.ToSingle(match.Groups["er"].Value);
            
            return generalStructure;
        }
    }
}