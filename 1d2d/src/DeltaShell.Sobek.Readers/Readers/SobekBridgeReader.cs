using System.Text.RegularExpressions;
using DelftTools.Utils.RegularExpressions;
using DeltaShell.Sobek.Readers.SobekDataObjects;

namespace DeltaShell.Sobek.Readers.Readers
{
    public class SobekBridgeReader : ISobekStructureReader
    {
        public int Type
        {
            get { return 12; }
        }

        /// <summary>
        /// SOBEK Urban/Rural Bridge:
        /// STDS id 'bridge1' nm 'bridge' ty 12 tb 1 si 'trapezoidal1' 
        ///   pw 0.5 vf 1.15 li 0.63 lo 0.63 dl 10.0 rl -1.0 stds
        /// where:
        /// ty = type of structure
        /// 12 = bridge  
        /// tb = type of bridge
        /// 2 = pillar bridge
        /// 3 = abutment bridge
        /// 4 = fixed bed bridge
        /// 5 = soil bed bridge
        /// si = id of cross section definition (profile.def), only open profiles (if tb =3,4, or 5)
        /// pw = total width of pillars in direction of flow (if tb=2) 
        /// vf = form factor (if tb=2)
        /// li = inlet loss coefficient
        /// lo = outlet loss coefficient
        /// dl = length of bridge in flow direction.
        /// rl = bed level 
        /// rt = possible flow direction (0 : flow in both directions,1 : flow from begin node to end node (positive),2 : flow from end node to begin node (negative),3 : no flow
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public ISobekStructureDefinition GetStructure(string text)
        {
            // WHEN PROBLEMS PARSING STRUCTURE DEFINITION DUE TO SEQUENCE OF FIELDS 
            // SEE : SobekCulvertReader.GetStructure
            var pattern =
                RegularExpression.GetInteger("tb") + "|" +
                RegularExpression.GetExtendedCharacters("si") + "|" +
                RegularExpression.GetScientific("pw") + "|" +
                RegularExpression.GetScientific("vf") + "|" +
                RegularExpression.GetScientific("li") + "|" +
                RegularExpression.GetScientific("lo") + "|" +
                RegularExpression.GetScientific("dl") + "|" +
                RegularExpression.GetScientific("rl") + "|" +
                RegularExpression.GetInteger("rt");

            var bridge = new SobekBridge();

            foreach (Match match in RegularExpression.GetMatches(pattern, text))
            {
                bridge.BridgeType = (BridgeType)RegularExpression.ParseInt(match, "tb", (int)bridge.BridgeType);
                bridge.CrossSectionId = RegularExpression.ParseString(match, "si", bridge.CrossSectionId);
                bridge.TotalPillarWidth = RegularExpression.ParseSingle(match, "pw", bridge.TotalPillarWidth);
                bridge.FormFactor = RegularExpression.ParseSingle(match, "vf", bridge.FormFactor);
                bridge.InletLossCoefficient = RegularExpression.ParseSingle(match, "li", bridge.InletLossCoefficient);
                bridge.OutletLossCoefficient = RegularExpression.ParseSingle(match, "lo", bridge.OutletLossCoefficient);
                bridge.Length = RegularExpression.ParseSingle(match, "dl", bridge.Length);
                bridge.BedLevel = RegularExpression.ParseSingle(match, "rl", bridge.BedLevel);
                bridge.Direction = RegularExpression.ParseInt(match, "rt", bridge.Direction);
            }
            return bridge;
        }
    }
}

