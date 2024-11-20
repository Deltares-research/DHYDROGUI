using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;
using DelftTools.Utils;
using DelftTools.Utils.RegularExpressions;
using DeltaShell.Sobek.Readers.SobekDataObjects;
using GeoAPI.Geometries;
using log4net;

namespace DeltaShell.Sobek.Readers.Readers
{
    public class CrossSectionDefinitionReader : SobekReader<SobekCrossSectionDefinition>
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(CrossSectionDefinitionReader));

        public override IEnumerable<SobekCrossSectionDefinition> Parse(string text)
        {
            return RegularExpression.GetMatches(@"CRDS\s+?" + IdAndOptionalNamePattern + "(?'text'.*?)crds", text)
                .Cast<Match>()
                .Select(structureMatch => GetCrossSectionDefinition(structureMatch.Value))
                .Where(definition => definition != null);
        }

        /*
            id  = cross section definition id
            nm  = cross section definition name
            ty  = type cross section (0=table)
            wm  = width main channel
            w1  = width floodplain 1 (used in River profile only, else value = 0)
            w2  = width floodplain 2 (used in River profile only, else value = 0)
            sw  = sediment transport width (not in SOBEK Urban/Rural) 
            Default 0. Only important for module sediment/morfology 
            lt lw  = table for table profile between keywords TBLE and tble; the table contains height, total width en flowing width.
            dk  = summer dike (1 = active, 0 = not active) (in River profile only)
            dc  = dike crest level in River profile only()
            db  = floodplain base level behind dike (in River profile only)
            df  = flow area behind dike (in River profile only)
            dt  = total area behind dike (in River profile only)
            gl    = ground layer depth (meter relative to bed level)
            gu   = ground layer to be used within hydraulics calculation (1) or not (0).
            rw, bo, ew, aw, cw = width (maximum flow width for trapezium)
            rh, eh, ah, sh = height
            rd = diameter
            aa = archeight (arch)
            // steel cunette-specific
            sr = radius r
            sr1 = radius r1
            sr2 = radius r2
            sr3 = radius r3
            sa = angle a
            sa1 = angle a1
         */

        public SobekCrossSectionDefinition GetCrossSectionDefinition(string text)
        {
            const string pattern = @"id\s+'(?<Id>" + RegularExpression.Characters + @")'\s+nm\s+'(?<Name>" +
                                   RegularExpression.ExtendedCharacters + @")'\s+ty\s+" + @"(?<Type>" +
                                   RegularExpression.Integer + @")" + @"\s+(?<Structure>" +
                                   RegularExpression.CharactersAndQuote + @"?)\s+crds";

            var match = RegularExpression.GetFirstMatch(pattern, text);
            if (match == null)
            {
                Log.WarnFormat("Could not read cross-section definition with specification: \"{0}\"",text);
                return null;
            }

            var sobekCrossSectionDefinitionType = (SobekCrossSectionDefinitionType)Convert.ToInt32(match.Groups["Type"].Value);
            var sobekCrossSectionDefinition = new SobekCrossSectionDefinition
                                                  {
                                                      ID = match.Groups["Id"].Value,
                                                      Name = match.Groups["Name"].Value,
                                                      Type = sobekCrossSectionDefinitionType
                                                  };

            bool successfullParse;
            switch (sobekCrossSectionDefinition.Type)
            {
                case SobekCrossSectionDefinitionType.Tabulated: // 0, 
                    successfullParse = ParseTabulatedTable(sobekCrossSectionDefinition, match.Groups["Structure"].Value);
                    break;
                case SobekCrossSectionDefinitionType.Trapezoidal: // 1,
                    successfullParse = ParseTrapezoidal(sobekCrossSectionDefinition, match.Groups["Structure"].Value);
                    break;
                case SobekCrossSectionDefinitionType.OpenCircle: // 2,
                    return null;
                case SobekCrossSectionDefinitionType.Sedredge: // 3, // 2d morfology
                    return null;
                case SobekCrossSectionDefinitionType.ClosedCircle: // 4,
                    successfullParse = ParseClosedCircle(sobekCrossSectionDefinition, match.Groups["Structure"].Value);
                    break;
                case SobekCrossSectionDefinitionType.EggShapedWidth: // 6,
                    successfullParse = ParseEggShapeWidth(sobekCrossSectionDefinition, match.Groups["Structure"].Value);
                    break;
                    //EggShapedRadius = 7, according to Sobek Help not implemented
                    //ClosedRectangular = 8, according to Sobek Help not implemented
                case SobekCrossSectionDefinitionType.Yztable: // 10,
                    successfullParse = ParseYZTable(sobekCrossSectionDefinition, match.Groups["Structure"].Value);
                    break;
                case SobekCrossSectionDefinitionType.AsymmetricalTrapezoidal: // 11
                    successfullParse = ParseYZTable(sobekCrossSectionDefinition, match.Groups["Structure"].Value);
                    break;
                default:
                    // return null? or return 'empty' definition
                    return null;
            }
            
            return successfullParse ? sobekCrossSectionDefinition : null;
        }

        /// <summary>
        /// example
        /// CRDS id '21' nm 'TrapProf01' ty 1 bl 0 bw 6 bs 1 aw 16 sw 0  gl 0 gu 0 crds
        /// bw = bottomwidth b
        /// bs = slope
        /// aw = maximum flow width
        /// </summary>
        /// <param name="definition"></param>
        /// <param name="trapeziumSource"></param>
        private bool ParseTrapezoidal(SobekCrossSectionDefinition definition, string trapeziumSource)
        {
            string pattern = RegularExpression.GetScientific("bw") +
                             RegularExpression.GetScientific("bs") +
                             RegularExpression.GetScientific("aw");
            
            var smatch = RegularExpression.GetFirstMatch(pattern, trapeziumSource);
            if (smatch == null)
            {
                Log.WarnFormat("Could not read trapezoidal cross-section definition with specification: \"{0}\"",trapeziumSource);
                return false;
            }
            definition.BedWidth = ConversionHelper.ToSingle(smatch.Groups["bw"].Value);
            definition.Slope = ConversionHelper.ToSingle(smatch.Groups["bs"].Value);
            definition.MaxFlowWidth = ConversionHelper.ToSingle(smatch.Groups["aw"].Value);
            return true;
        }

        /// <summary>
        /// example 
        /// CRDS id 'Round 100 mm' nm 'Round 100 mm' ty 4 bl 0 rd  .05 crds
        /// bl  = bed level
        /// rd  = radius
        /// </summary>
        /// <param name="definition"></param>
        /// <param name="circleSource"></param>
        private static bool ParseClosedCircle(SobekCrossSectionDefinition definition, string circleSource)
        {
            string pattern = RegularExpression.GetScientific("bl") +
                             RegularExpression.GetScientific("rd");
            var smatch = RegularExpression.GetFirstMatch(pattern, circleSource);
            if (smatch == null)
            {
                Log.WarnFormat("Could not parse closed circle for cross-section {0} (\"{1}\").",definition.ID,circleSource);
                return false;
            }
            definition.BedLevel = ConversionHelper.ToSingle(smatch.Groups["bl"].Value);
            definition.Radius = ConversionHelper.ToSingle(smatch.Groups["rd"].Value);
            return true;
        }

        /// <summary>
        /// example 
        /// CRDS id 'Round 100 mm' nm 'Round 100 mm' ty 4 bl 0 rd  .05 crds
        /// bl  = bed level
        /// rd  = radius
        /// </summary>
        /// <param name="definition"></param>
        /// <param name="circleSource"></param>
        private static bool ParseEggShapeWidth(SobekCrossSectionDefinition definition, string circleSource)
        {
            string pattern = RegularExpression.GetScientific("bl") +
                             RegularExpression.GetScientific("bo");
            var smatch = RegularExpression.GetFirstMatch(pattern, circleSource);
            if (smatch == null)
            {
                Log.WarnFormat("Could not pase egg shaped width for cross-section {0} (\"{1}\")",definition.ID,circleSource);
                return false;
            }
            definition.BedLevel = ConversionHelper.ToSingle(smatch.Groups["bl"].Value);
            definition.Width = ConversionHelper.ToSingle(smatch.Groups["bo"].Value);
            return true;
        }


        private static bool ParseYZTable(SobekCrossSectionDefinition sobekCrossSectionDefinition, string tableSource)
        {
            const string tablepattern = @"lt\s+yz(?<Structure>" + RegularExpression.CharactersAndQuote + @"?)tble";

            var tableMatch = RegularExpression.GetFirstMatch(tablepattern, tableSource);
            if (tableMatch == null)
            {
                Log.WarnFormat("Could not parse YZTable of cross-section {0} (\"{1}\")",sobekCrossSectionDefinition.ID,tableSource);
                return false;
            }

            var yzTable = SobekDataTableReader.GetTable(tableMatch.Groups["Structure"].Value + "tble",
                                                        new Dictionary<string, Type>
                                                            {
                                                                {"x", typeof (double)},
                                                                {"y", typeof (double)}
                                                            });

            var valuesY = new List<double>();
            foreach (DataRow row in yzTable.Rows)
            {
                var y = (double) row[0];
                valuesY.Add(y);
            }

            var uniqueY = MakeValuesUniqueInwards(valuesY);
            for (var i = 0; i < yzTable.Rows.Count; i++)
            {
                var row = yzTable.Rows[i];
                sobekCrossSectionDefinition.YZ.Add(new Coordinate(uniqueY[i], (double)row[1]));
            }

            return true;
        }

        private static List<double> MakeValuesUniqueInwards(IList<double> uniqueY)
        {
            const double yDelta = 1e-5;
            var valueCount = uniqueY.Count;
            var index = (int)Math.Ceiling(0.5 * valueCount);

            var correctedDelta = yDelta;
            var deltaCorrection = new double[valueCount];
            var previous = uniqueY[0];

            deltaCorrection[0] = correctedDelta;
            for (var i = 1; i < valueCount; i++)
            {
                var areEqual = Math.Abs(uniqueY[i] - previous) < 1e-10;
                previous = uniqueY[i];

                if (i == index)
                {
                    correctedDelta = -yDelta;
                    deltaCorrection[i - 1] = correctedDelta; //Reset the correction if we are in the middle.
                }
                if (areEqual) uniqueY[i] += deltaCorrection[i - 1];

                deltaCorrection[i] = areEqual ? deltaCorrection[i - 1] + correctedDelta : correctedDelta;
            }

            return uniqueY.OrderBy(d => d).ToList();
        }

        /// <summary>
        /// if w1 (width floodplain 1) or w2 width floodplain 2) are not 0 the type of the profile in the
        /// Sobek UI is 'River Profile'; type stored in profile.def is still 0.
        /// River profiles may have different roughness for main channel, floodplain 1 of floodplain 2 where
        /// tabulated profiles have contant roughness.
        /// </summary>
        /// <param name="sobekCrossSectionDefinition"></param>
        /// <param name="tableSource"></param>
        private static bool ParseTabulatedTable(SobekCrossSectionDefinition sobekCrossSectionDefinition,
                                                string tableSource)
        {
            const string tablepattern = @"wm\s*(?<MainWidth>" + RegularExpression.CharactersAndQuote + @"?) " +
                                        @"(([acer]w|bo)\s*(?<Width>" + RegularExpression.Scientific + @"\s*))?" +
                                        @"([aers]h\s*(?<Height>" + RegularExpression.Scientific + @"\s*))?" +
                                        @"(aa\s*(?<ArcHeight>" + RegularExpression.Scientific + @"\s*))?" +
                                        @"(bs\s*(?<Slope>" + RegularExpression.Scientific + @"\s*))?" +
                                        @"(bw\s*(?<BottomwidthB>" + RegularExpression.Scientific + @"\s*))?" +
                                        @"(sr\s*(?<RadiusR>" + RegularExpression.Scientific + @"\s*))?" +
                                        @"(sr1\s*(?<RadiusR1>" + RegularExpression.Scientific + @"\s*))?" +
                                        @"(sr2\s*(?<RadiusR2>" + RegularExpression.Scientific + @"\s*))?" +
                                        @"(sr3\s*(?<RadiusR3>" + RegularExpression.Scientific + @"\s*))?" +
                                        @"(sa\s*(?<AngleA>" + RegularExpression.Scientific + @"\s*))?" +
                                        @"(sa1\s*(?<AngleA1>" + RegularExpression.Scientific + @"\s*))?" +
                                        @"w1\s*(?<floodplain1>" + RegularExpression.CharactersAndQuote + @"?) " +
                                        @"w2\s*(?<floodplain2>" + RegularExpression.CharactersAndQuote + @"?) " +
                                        RegularExpression.CharactersAndQuote + @"lt lw(?<TabTable>" +
                                        RegularExpression.CharactersAndQuote + @"?)tble";

            var tableMatch = RegularExpression.GetFirstMatch(tablepattern, tableSource);
            if (tableMatch == null)
            {
                Log.WarnFormat("Could not parse tabulated table of cross-section {0} (\"{1}\")",sobekCrossSectionDefinition.ID,tableSource);
                return false;
            }

            if (tableMatch.Groups["MainWidth"].Success)
            {
                sobekCrossSectionDefinition.MainChannelWidth =
                    ConversionHelper.ToDouble(tableMatch.Groups["MainWidth"].Value);
            }
            if (tableMatch.Groups["floodplain1"].Success)
            {
                sobekCrossSectionDefinition.FloodPlain1Width =
                    ConversionHelper.ToDouble(tableMatch.Groups["floodplain1"].Value);
            }
            if (tableMatch.Groups["floodplain2"].Success)
            {
                sobekCrossSectionDefinition.FloodPlain2Width =
                    ConversionHelper.ToDouble(tableMatch.Groups["floodplain2"].Value);
            }
            if (tableMatch.Groups["Width"].Success)
            {
                sobekCrossSectionDefinition.Width =
                    ConversionHelper.ToDouble(tableMatch.Groups["Width"].Value);
            }
            if (tableMatch.Groups["Height"].Success)
            {
                sobekCrossSectionDefinition.Height =
                    ConversionHelper.ToDouble(tableMatch.Groups["Height"].Value);
            }
            if (tableMatch.Groups["ArcHeight"].Success)
            {
                sobekCrossSectionDefinition.ArcHeight =
                    ConversionHelper.ToDouble(tableMatch.Groups["ArcHeight"].Value);
                sobekCrossSectionDefinition.InferStandardType = true;
            }
            if (tableMatch.Groups["Slope"].Success)
            {
                sobekCrossSectionDefinition.Slope =
                    ConversionHelper.ToDouble(tableMatch.Groups["Slope"].Value);
                sobekCrossSectionDefinition.InferStandardType = true;
            }
            if (tableMatch.Groups["BottomwidthB"].Success)
            {
                sobekCrossSectionDefinition.MaxFlowWidth =
                    ConversionHelper.ToDouble(tableMatch.Groups["BottomwidthB"].Value);
                sobekCrossSectionDefinition.InferStandardType = true;
            }
            if (tableMatch.Groups["RadiusR"].Success)
            {
                sobekCrossSectionDefinition.RadiusR =
                    ConversionHelper.ToDouble(tableMatch.Groups["RadiusR"].Value);
                sobekCrossSectionDefinition.InferStandardType = true;
            }
            if (tableMatch.Groups["RadiusR1"].Success)
            {
                sobekCrossSectionDefinition.RadiusR1 =
                    ConversionHelper.ToDouble(tableMatch.Groups["RadiusR1"].Value);
                sobekCrossSectionDefinition.InferStandardType = true;
            }
            if (tableMatch.Groups["RadiusR2"].Success)
            {
                sobekCrossSectionDefinition.RadiusR2 =
                    ConversionHelper.ToDouble(tableMatch.Groups["RadiusR2"].Value);
                sobekCrossSectionDefinition.InferStandardType = true;
            }
            if (tableMatch.Groups["RadiusR3"].Success)
            {
                sobekCrossSectionDefinition.RadiusR3 =
                    ConversionHelper.ToDouble(tableMatch.Groups["RadiusR3"].Value);
                sobekCrossSectionDefinition.InferStandardType = true;
            }
            if (tableMatch.Groups["AngleA"].Success)
            {
                sobekCrossSectionDefinition.AngleA =
                    ConversionHelper.ToDouble(tableMatch.Groups["AngleA"].Value);
                sobekCrossSectionDefinition.InferStandardType = true;
            }
            if (tableMatch.Groups["AngleA1"].Success)
            {
                sobekCrossSectionDefinition.AngleA1 =
                    ConversionHelper.ToDouble(tableMatch.Groups["AngleA1"].Value);
                sobekCrossSectionDefinition.InferStandardType = true;
            }
            // TabTable is required
            var yzTable = SobekDataTableReader.GetTable(tableMatch.Groups["TabTable"].Value + "tble",
                                                        new Dictionary<string, Type>
                                                            {
                                                                {"height", typeof (double)},
                                                                {"totalwidth", typeof (double)},
                                                                {"flowwidth", typeof (double)}
                                                            });
            for (int i = 0; i < yzTable.Rows.Count; i++)
            {
                sobekCrossSectionDefinition.TabulatedProfile.Add(new SobekTabulatedProfileRow
                                                                     {
                                                                         Height = (double) yzTable.Rows[i][0],
                                                                         TotalWidth = (double) yzTable.Rows[i][1],
                                                                         FlowWidth = (double) yzTable.Rows[i][2]
                                                                     });
            }

            //check if is closed rectangle profile
            if (sobekCrossSectionDefinition.IsTabulatedProfileClosedRectangularShape)
            {
                var b2 = sobekCrossSectionDefinition.TabulatedProfile.ElementAt(1);
                sobekCrossSectionDefinition.Width = b2.TotalWidth;
                sobekCrossSectionDefinition.Height = b2.Height;
            }

            RemoveUnneccessaryLowestsPoints(sobekCrossSectionDefinition);

            const string dikePattern = @"dk\s*" + @"(?<SummerDike>" + RegularExpression.Integer + @")"
                                       + @" dc\s*" + @"(?<DikeCrestLevel>" + RegularExpression.Scientific + @")"
                                       + @" db\s*" + @"(?<BaseLevel>" + RegularExpression.Scientific + @")"
                                       + @" df\s*" + @"(?<FlowArea>" + RegularExpression.Scientific + @")"
                                       + @" dt\s*" + @"(?<TotalArea>" + RegularExpression.Scientific + @")";

            var dikeMatch = RegularExpression.GetFirstMatch(dikePattern, tableSource);

            // if a CRDS record of type tabulated (0) has the field dk defined it is a riverprofile (Jaap Zeekant).
            // Apart form the summerdike a river profile also can have different roughness for main channel,
            // floodplain1 and floodplain2. These roughness values are found in the BDFR (friction.dat)
            // record.
            if (dikeMatch != null)
            {
                sobekCrossSectionDefinition.IsRiverProfile = true;
                if (dikeMatch.Groups["SummerDike"].Success)
                {
                    sobekCrossSectionDefinition.SummerDikeActive = dikeMatch.Groups["SummerDike"].Value == "1";
                }
                sobekCrossSectionDefinition.CrestLevel = ExtraDoubleVar(dikeMatch, "DikeCrestLevel", 0.0);
                sobekCrossSectionDefinition.FloodPlainLevel = ExtraDoubleVar(dikeMatch, "BaseLevel", 0.0);
                sobekCrossSectionDefinition.FlowArea = ExtraDoubleVar(dikeMatch, "FlowArea", 0.0);
                sobekCrossSectionDefinition.TotalArea = ExtraDoubleVar(dikeMatch, "TotalArea", 0.0);
            }
            const string groundPattern = @"gl\s*" + @"(?<GroundLayerDepth>" + RegularExpression.Scientific + @")"
                                         + @" gu\s*" + @"(?<GroundLayerUse>" + RegularExpression.Integer + @")";

            var groundMatch = RegularExpression.GetFirstMatch(groundPattern, tableSource);
            if (groundMatch == null)
            {
                return true;
            }
            
            sobekCrossSectionDefinition.GroundLayerDepth = ExtraDoubleVar(groundMatch, "GroundLayerDepth", 0.0);
            if (groundMatch.Groups["GroundLayerUse"].Success)
            {
                sobekCrossSectionDefinition.UseGroundLayer = groundMatch.Groups["GroundLayerUse"].Value == "1";
            }
            return true;
        }

        private static void RemoveUnneccessaryLowestsPoints(SobekCrossSectionDefinition crossSectionDefinition)
        {
            IList<SobekTabulatedProfileRow> tabulatedProfile = crossSectionDefinition.TabulatedProfile;
            //in some import files profiles are define like this
            // Z W
            // 0 0 
            // 10 0 
            // 20 10
            // so the 0,0 point is not needed. We remove it here
            var orderedRows = tabulatedProfile.OrderBy(r => r.Height).ToList();
            for (int i = 0; i < orderedRows.Count - 1; i++)
            {
                SobekTabulatedProfileRow thisRow = orderedRows[i];
                SobekTabulatedProfileRow nextRow = orderedRows[i + 1];
                //if the current and next row have zero width we can remove the current
                if ((thisRow.TotalWidth == 0) && nextRow.TotalWidth == 0)
                {
                    Log.WarnFormat("Removing profile row with z={0} from crossection {1} it is redundant because a higher point also has width 0",
                        thisRow.Height, crossSectionDefinition.Name);
                    tabulatedProfile.Remove(thisRow);
                }

                //break out as soon as we are non-zero
                if (thisRow.TotalWidth != 0)
                {
                    break;
                }
            }
        }

        private static double ExtraDoubleVar(Match dikeMatch, string varName, double defaultValue)
        {
            return dikeMatch.Groups[varName].Success
                       ? ConversionHelper.ToDouble(dikeMatch.Groups[varName].Value)
                       : defaultValue;
        }

        public override IEnumerable<string> GetTags()
        {
            yield return "crds";
        }
    }
}