using System.Collections.Generic;
using System.Data;
using System.Text.RegularExpressions;
using DelftTools.Functions.Generic;
using DelftTools.Utils;
using DelftTools.Utils.RegularExpressions;
using DeltaShell.Sobek.Readers.SobekDataObjects;
using log4net;

namespace DeltaShell.Sobek.Readers.Readers
{
    public class SobekBoundaryConditionReader : SobekReader<SobekFlowBoundaryCondition>
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(SobekBoundaryConditionReader));

        public override IEnumerable<SobekFlowBoundaryCondition> Parse(string text)
        {
            const string structurePattern = @"(FLBO (?'text'.*?) flbo)";

            foreach (Match structureMatch in RegularExpression.GetMatches(structurePattern, text))
            {
                SobekFlowBoundaryCondition flowBoundaryCondition = GetFlowBoundaryCondition(structureMatch.Value);
                if (flowBoundaryCondition != null)
                {
                    yield return flowBoundaryCondition;
                }
            }
        }


        /// <summary>
        /// please note the Sobek help does ot match the values found in BOUNDARY.DAT files
        /// 
        /// FLBO id '1' ty 0 h_ wd 0 1.2 0 flbo  (constant H)
        /// or
        /// FLBO id '1' ty 1 q_ wd 0 1.2 0 flbo  (constant Q)
        /// or (variable discharge)
        /// FLBO id '1' ty 1 q_ dt 1 TBLE .. tble flbo  (variable Q)
        /// or
        /// FLBO id '1' ty 0 h_ wt 1 TBLE .. tble flbo  (variable H)
        /// 
        /// h boundaries
        /// h_ wd 0 = constant water level (only for ty 0)
        /// h_ wd 1 =  h_ wd 4
        /// h_ wd 4 =  water level as a function of Q 
        /// column 1 = Q
        /// column 2 = h 
        /// h_ wt 1 = variable water level as a function of time (TBLE ... tble)
        /// q boundaries
        /// q_ dw 0 = constant discharge 
        /// q_ dw 1 = q_ dw 4
        /// q_ dw 4 = Q = Q(H) according to Q-H table 
        /// column 1 = h
        /// column 2 =Q
        /// 
        /// q_ dt 1 = variable discharge as function of time (TBLE ... tble)
        /// 
        /// @"FLBO id '1' st 0 ty 0 h_ wt 1 0 0 PDIN 1 0  pdin"
        /// PDIN ..pdin = period and interpolation method
        /// 0 0 ' '     =  interpolation continuous, no period 
        /// 1 0 ' '     =  interpolation block, no period 
        /// 0 1 '3600'  =  interpolation continuous,  period in seconds
        /// 1 1 '86400' =  interpolation block, period  in seconds


        /// </summary>
        /// <param name="record"></param>
        /// <returns></returns>
        public SobekFlowBoundaryCondition GetFlowBoundaryCondition(string record)
        {
            var flowBoundaryCondition = new SobekFlowBoundaryCondition
                                            {
                                                ID = RegularExpression.ParseFieldAsString("id", record),
                                                InterpolationType = InterpolationType.Linear,
                                                ExtrapolationType = ExtrapolationType.Constant
                                            };

            // SobeRe files typically have both h and q boundary where ty defines the active boundary
            var typeOfBoundary = RegularExpression.ParseFieldAsInt("ty", record);
            string pattern = typeOfBoundary == 0
                                 ? @"h_ wd 0\s*(?<hconst>" + RegularExpression.Scientific + @")|" +
                                   @"h_ wt 1\s*(?<httable>" + RegularExpression.CharactersAndQuote + @")|" +
                                   @"h_ wd (1|4)\s*(?<hqhtable>" + RegularExpression.CharactersAndQuote + @")"

                                 : @"q_ dw 0\s*(?<qconst>" + RegularExpression.Scientific + @")|" +
                                   @"q_ dt 1\s*(?<qttable>" + RegularExpression.CharactersAndQuote + @")|" +
                                   @"q_ dw (1|4)\s*(?<qhqtable>" + RegularExpression.CharactersAndQuote + @")";


            const string pdinPattern = @"PDIN\s(?<pdin>" + RegularExpression.CharactersAndQuote + @") pdin" + RegularExpression.CharactersAndQuote + @"TBLE\s(?<tble>" + RegularExpression.CharactersAndQuote + @")tble";

            var pdinMatch = RegularExpression.GetFirstMatch(pdinPattern, record);
            if (pdinMatch != null)
            {
                const string pdinSubPattern =
                    @"(?<pdin1>" + RegularExpression.Integer + @")\s(?<pdin2>" + RegularExpression.Integer + @")" +
                    @"(?<period>" + RegularExpression.CharactersAndQuote + @")";

                var pdin = pdinMatch.Groups["pdin"].ToString();
                var pdinSubMatch = RegularExpression.GetFirstMatch(pdinSubPattern, pdin);

                if (pdinSubMatch != null)
                {
                    var pdin1 = pdinSubMatch.Groups["pdin1"].ToString();
                    var pdin2 = pdinSubMatch.Groups["pdin2"].ToString();
                    var period = pdinSubMatch.Groups["period"].ToString();

                    if (pdin1 == "1")
                    {
                        flowBoundaryCondition.InterpolationType = InterpolationType.Constant;
                    }

                    if (pdin2 == "1")
                    {
                        if (string.IsNullOrEmpty(period))
                        {
                            flowBoundaryCondition.ExtrapolationType = ExtrapolationType.Constant;
                        }
                        else
                        {
                            flowBoundaryCondition.ExtrapolationType = ExtrapolationType.Periodic;
                            flowBoundaryCondition.ExtrapolationPeriod = period;
                        }
                    }
                    else
                    {
                        flowBoundaryCondition.ExtrapolationType = ExtrapolationType.None;
                    }
                }
            }

            var matches = RegularExpression.GetMatches(pattern, record);
            if (matches.Count > 0)
            {
                var match = matches[0];
                if (match.Groups["hconst"].Success)
                {
                    flowBoundaryCondition.BoundaryType = SobekFlowBoundaryConditionType.Level;
                    flowBoundaryCondition.StorageType = SobekFlowBoundaryStorageType.Constant;
                    flowBoundaryCondition.LevelConstant = ConversionHelper.ToDouble(match.Groups["hconst"].Value);
                }
                else if (match.Groups["qconst"].Success)
                {
                    flowBoundaryCondition.BoundaryType = SobekFlowBoundaryConditionType.Flow;
                    flowBoundaryCondition.StorageType = SobekFlowBoundaryStorageType.Constant;
                    flowBoundaryCondition.FlowConstant = ConversionHelper.ToDouble(match.Groups["qconst"].Value);
                }
                else if (match.Groups["httable"].Success)
                {
                    flowBoundaryCondition.BoundaryType = SobekFlowBoundaryConditionType.Level;
                    flowBoundaryCondition.StorageType = SobekFlowBoundaryStorageType.Variable;
                    DataTable table = null;
                    if (!match.ToString().ToUpper().StartsWith("H_ WT 11"))
                    {
                        table = SobekDataTableReader.GetTable(match.Groups["httable"].Value, SobekFlowBoundaryCondition.TimeTableStructure);
                        flowBoundaryCondition.LevelTimeTable = table;
                    }
                    else
                    {
                        log.WarnFormat("Variable water level as a function of time from a separate table library " +
                            "are not supported, regarding boundary condition: {0}",flowBoundaryCondition.ID);
                        flowBoundaryCondition.StorageType = SobekFlowBoundaryStorageType.Constant;
                    }
                }
                else if (match.Groups["qttable"].Success)
                {
                    flowBoundaryCondition.BoundaryType = SobekFlowBoundaryConditionType.Flow;
                    flowBoundaryCondition.StorageType = SobekFlowBoundaryStorageType.Variable;
                    flowBoundaryCondition.FlowTimeTable = SobekDataTableReader.GetTable(match.Groups["qttable"].Value, SobekFlowBoundaryCondition.TimeTableStructure);
                }
                else if (match.Groups["qhqtable"].Success)
                {
                    flowBoundaryCondition.BoundaryType = SobekFlowBoundaryConditionType.Flow;
                    flowBoundaryCondition.StorageType = SobekFlowBoundaryStorageType.Qh;
                    flowBoundaryCondition.FlowHqTable = SobekDataTableReader.GetTable(match.Groups["qhqtable"].Value, SobekFlowBoundaryCondition.HqTableStructure);
                    if (flowBoundaryCondition.InterpolationType != InterpolationType.Linear)
                    {
                        log.WarnFormat("Interpolation of type {0} for {1} not supported; set to {2}", flowBoundaryCondition.InterpolationType,
                            flowBoundaryCondition.ID, InterpolationType.Linear);
                        flowBoundaryCondition.InterpolationType = InterpolationType.Linear;
                    }
                }
                else if (match.Groups["hqhtable"].Success)
                {
                    flowBoundaryCondition.BoundaryType = SobekFlowBoundaryConditionType.Level;
                    flowBoundaryCondition.StorageType = SobekFlowBoundaryStorageType.Qh;
                    flowBoundaryCondition.LevelQhTable = SobekDataTableReader.GetTable(match.Groups["hqhtable"].Value, SobekFlowBoundaryCondition.QhTableStructure);
                    if (flowBoundaryCondition.InterpolationType != InterpolationType.Linear)
                    {
                        log.WarnFormat("Interpolation of type {0} for {1} not supported; set to {2}", flowBoundaryCondition.InterpolationType,
                            flowBoundaryCondition.ID, InterpolationType.Linear);
                        flowBoundaryCondition.InterpolationType = InterpolationType.Linear;
                    }
                }
            }
            return flowBoundaryCondition;
        }

        public override IEnumerable<string> GetTags()
        {
            yield return "flbo";
        }
    }
}
