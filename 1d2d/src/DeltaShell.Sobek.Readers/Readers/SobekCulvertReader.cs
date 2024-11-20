using System.Text.RegularExpressions;
using DelftTools.Utils.RegularExpressions;
using DeltaShell.Sobek.Readers.SobekDataObjects;

namespace DeltaShell.Sobek.Readers.Readers
{
    public class SobekCulvertReader : ISobekStructureReader
    {
        public int Type
        {
            get { return 10; }
        }

        /// <summary>
        /// SOBEK Urban Rural Culvert, Siphon and Inverted Siphon
        /// STDS id 'culvert1' nm 'culvert' ty 10 tc 1 ll -2.0 rl -1.0 si 'Crdef' li 0.63 lo 0.63 lb 0 
        ///        ov -2.2 tv 1 ‘Table1' rt 0 dl 10.0 hs 7.6 he 8.8 stds
        /// Where:
        /// ty = type of structure
        /// 10 = culvert or siphon or inverted siphon
        /// tc = type of culvert
        /// 1 = culvert
        /// 2 = siphon
        /// 3 = inverted siphon
        /// rl = bed level (right)
        /// ll = bed level (left)
        /// si = id of cross section definition (profile.def), only closed profiles
        /// li = inlet loss coefficient
        /// lo = outlet loss coefficient
        /// lb = bend loss coefficient
        /// ov = initial opening level of valve
        /// tv = table of loss coefficient
        /// 0 no table, no valve
        /// 1 valve present, reference to table in file valve.tab. See detailed decription of this file below.
        /// rt = possible flow direction (relative to the branch direction):
        /// 0 : flow in both directions
        /// 1 : flow from begin node to end node (positive)
        /// 2 : flow from end node to begin node (negative)
        /// 3 : no flow
        /// dl = length of culvert, siphon or inverted siphon
        /// hs = start level of operation of siphon
        /// he = end level of operation of siphon
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public ISobekStructureDefinition GetStructure(string text)
        {
            //tc 1 ll 5 rl 10 dl 10 si '1' li 0.7 lo 1 ov 0 tv 0 rt 0

            string pattern =
                RegularExpression.GetInteger("tc") + "|" +
                RegularExpression.GetScientific("ll") + "|" +
                RegularExpression.GetScientific("rl") + "|" +
                RegularExpression.GetScientific("dl") + "|" +
                RegularExpression.GetExtendedCharacters("si") + "|" +
                RegularExpression.GetScientific("li") + "|" +
                RegularExpression.GetScientific("lo") + "|" +
                RegularExpression.GetScientific("lb") + "|" +
                RegularExpression.GetScientific("ov") + "|" +
                RegularExpression.GetIntegerOptionallyExtendedCharacters("tv", "tableID") + "|" +
                RegularExpression.GetScientific("hs") + "|" +
                RegularExpression.GetScientific("he") + "|" +
                RegularExpression.GetInteger("rt");

            var culvert = new SobekCulvert();

            foreach (Match match in RegularExpression.GetMatches(pattern, text))
            {
                culvert.CulvertType = (CulvertType)RegularExpression.ParseInt(match, "tc", (int)culvert.CulvertType);
                culvert.BedLevelLeft = RegularExpression.ParseSingle(match, "ll", culvert.BedLevelLeft);
                culvert.BedLevelRight = RegularExpression.ParseSingle(match, "rl", culvert.BedLevelRight);
                culvert.Length = RegularExpression.ParseSingle(match, "dl", culvert.Length);
                culvert.CrossSectionId = RegularExpression.ParseString(match, "si", culvert.CrossSectionId);
                culvert.InletLossCoefficient = RegularExpression.ParseSingle(match, "li", culvert.InletLossCoefficient);
                culvert.OutletLossCoefficient = RegularExpression.ParseSingle(match, "lo", culvert.OutletLossCoefficient);
                culvert.ValveInitialOpeningLevel = RegularExpression.ParseSingle(match, "ov", culvert.ValveInitialOpeningLevel);
                culvert.UseTableOffLossCoefficient = RegularExpression.ParseInt(match, "tv", culvert.UseTableOffLossCoefficient);
                culvert.TableOfLossCoefficientId = RegularExpression.ParseString(match, "tableID", culvert.TableOfLossCoefficientId);
                culvert.BendLossCoefficient = RegularExpression.ParseSingle(match, "lb", culvert.BendLossCoefficient);
                culvert.Direction = RegularExpression.ParseInt(match, "rt", culvert.Direction);
            }
            return culvert;
        }
    }
}