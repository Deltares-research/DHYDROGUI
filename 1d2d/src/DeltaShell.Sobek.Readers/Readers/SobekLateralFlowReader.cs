using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using DelftTools.Functions.Generic;
using DelftTools.Utils;
using DelftTools.Utils.RegularExpressions;
using DeltaShell.NGHS.Utils.Extensions;
using DeltaShell.Sobek.Readers.SobekDataObjects;
using log4net;

namespace DeltaShell.Sobek.Readers.Readers
{
    public class SobekLateralFlowReader
    {
        // Lateral discharge on branch:
        // FLBR id '3' sc 0 lt 0 dc lt 0 ir 0 ms 'station 1' ii 0.005 ar 600000 flbr
        // or
        // FLBR id 'Intensity from Meteostation' sc 0 lt 0 dc lt 7 ir 0.003 ms 'meteostation' ii 0.002 ar 1000 flbr
        // or
        // FLBR id 'Constant intensity' sc 0 lt 0 dc lt 6 ir 0.003 ms 'meteostation' ii 0.002 ar 1000 flbr
        // or
        // FLBR id '1' ci '1' sc 0 lt 0 dc lt 1 0 0  TBLE .. tble flbr
        // or
        // FLBR id '11' sc 0 lt 0 dc lt 0 1 0  flbr
        // or
        // FLBR id '107' sc 0 lt 0 dc lt 11 '107' flbr

        // lateral structures:
        // FLBR id '1' sc 0 lt 0 dc lt 4 0 0 sd 'S1' wl ow 0 -1 0 flbr
        // or
        // FLBR id '1' sc 0 lt 0 dc lt 4 0 0 sd 'S1' wl ow 1 TBLE .. tble flbr

        // Where:
        // id  = id
        // sc = section (for 2D morphology !)
        //     0 = left (=main section; default)
        //     1 = right
        // lt = length of discharge
        //     0 = point discharge (m3/s) 
        //     >0 = discharge over a certain length (m2/s) (not in SOBEK-Urban/Rural)
        //     -1 = discharge over the entire length of the branch (m3/s) (new in SOBEK Urban/Rural)
        // dc lt = table: dc lt 0 = constant value, 
        // dc lt 1 =  'real' table (first column=time, second column=discharge)
        // dc lw 2 =  as a function of the waterlevel (not in SOBEK Urban/Rural)
        //     column 1 = h 
        //     column 2 =Q
        // dc lt 3 = linked to another lateral discharge ('2nd station') (not in SOBEK Urban/Rural)
        // dc lt 4  =  indicates lateral structure on a branch
        // dc lt 5  =  retention
        // dc lt 6  = rational method with constant intensity
        // dc lt 7  = with intensity from the rainfall station
        // dc lt 11 = from a table library
        // ir = constant intensity (mm/s)
        // ms = meteo-station
        // ii = seepage/infiltration intensity (mm/s)
        // ar = runoff area (m2)
        // sd = id of structure definition (see STRUCT.DEF; only in case dc lt 4)
        // ci = id of second station (only in case of dc lt 3; not in SOBEK Urban/Rural)
        // wl ow = table with water levels outside the lateral structure
        // wl ow 0= constant as a table, 
        // wl ow 1= 'real' table

        private static readonly ILog Log = LogManager.GetLogger(typeof(SobekLateralFlowReader));

        public IEnumerable<SobekLateralFlow> ReadLateralBoundaries(string filePath)
        {
            var warningList = new Dictionary<string, IList<string>>();
            var sb = new StringBuilder();

            using (var fileStream = File.OpenText(filePath))
            {
                do
                {
                    var line = fileStream.ReadLine();

                    if(line != null)
                    {
                        line = line.TrimEnd();
                        sb.AppendLine(line);

                        if (line.EndsWith("flbr") || line.EndsWith("fldi") || line.EndsWith("flno"))
                        {
                            foreach (var sobekLateralFlow in ParseBoundaryConditions(sb.ToString(), warningList))
                            {
                                yield return sobekLateralFlow;
                            }
                            sb.Remove(0, sb.Length);
                        }
                    }
    
                    
                } 
                while (!fileStream.EndOfStream); 

            }

            if (warningList.Any())
            {
                foreach (var kvp in warningList)
                {
                    Log.Warn(kvp.Key + Environment.NewLine + string.Join(Environment.NewLine, kvp.Value));
                }
            }
        }

        public IEnumerable<SobekLateralFlow> ParseBoundaryConditions(string text,
            Dictionary<string, IList<string>> warningList)
        {
            const string lateralConditionPattern = @"(FLBR\s+(?'text'.*?)\s+flbr)|(FLDI\s+(?'text'.*?)\s+fldi)|(FLNO\s+(?'text'.*?)\s+flno)";

            var matches = RegularExpression.GetMatches(lateralConditionPattern, text);

            foreach (Match match in matches)
            {
                SobekLateralFlow sobekLateralFlow = GetLateralFlow(match.Value, warningList);
                if (sobekLateralFlow != null)
                {
                    yield return sobekLateralFlow;
                }
            }
        }

        public SobekLateralFlow GetLateralFlow(string record, Dictionary<string, IList<string>> warningList)
        {
            var sobekLateralFlow = new SobekLateralFlow
            {
                Id = RegularExpression.ParseFieldAsString("id", record)
            };

            var pattern = RegularExpression.GetScientific("lt") + "|" +
                             RegularExpression.GetScientific("lc") + "|" +
                             @"dc\s+'?(?<dc>" + RegularExpression.ExtendedCharacters + @")'?\s?";

            void LogWarning(string key, string value)
            {
                warningList.AddToList(key, value);
            }

            foreach (Match match in RegularExpression.GetMatches(pattern, record))
            {
                ExtractLength(match, sobekLateralFlow, "lt");
                ExtractLength(match, sobekLateralFlow, "lc");
                if (match.Value.StartsWith("dc lt"))
                {
                    var match2 = RegularExpression.GetFirstMatch(@"dc lt (?<ltype>" + RegularExpression.Integer + @")", match.Value);
                    if (match2 == null)
                    {
                        LogWarning("Could not parse lateral flow specification for id's", $"{sobekLateralFlow.Id} (\"{record}\")");
                        continue;
                    }
                    var type = int.Parse(match2.Groups["ltype"].Value);
                    switch (type)
                    {
                        case 0: // const
                            match2 = RegularExpression.GetFirstMatch(@"dc lt 0\s*(?<const>" + RegularExpression.Scientific + @")", record);
                            if (match2 == null)
                            {
                                LogWarning("Could not parse lateral flow specification for id's", $"{sobekLateralFlow.Id} (\"{record}\")");
                                break;
                            }
                            sobekLateralFlow.IsConstantDischarge = true;
                            sobekLateralFlow.ConstantDischarge = ConversionHelper.ToDouble(match2.Groups["const"].Value);
                            break;
                        case 1: // time 
                            //@"(ty 0 h_ wt 1\s*(?<httable>" + RegularExpression.CharactersAndQuote + @"))|" +
                            var dcmatch = RegularExpression.GetFirstMatch(@"dc lt 1\s*(?<table>" + RegularExpression.CharactersAndQuote + @")", record);
                            if (dcmatch == null)
                            {
                                LogWarning("Could not parse lateral flow specification for id's", $"{sobekLateralFlow.Id} (\"{record}\")");
                                break;
                            }
                            sobekLateralFlow.IsConstantDischarge = false;
                            sobekLateralFlow.FlowTimeTable = SobekDataTableReader.GetTable((string) dcmatch.Groups["table"].Value, (DataTable) SobekLateralFlow.TimeTableStructure);
                            break;
                        case 3:
                            LogWarning("Unable to import Lateral Flow record; 2nd station is not supported: consider creating a RTC control group with Invertor Rule; currently set to default", $"record {sobekLateralFlow.Id} (dc lt {type})");
                            break;
                        case 5: 
                            // ignore
                            break;
                        case 6:
                            var intensity = RegularExpression.ParseFieldAsDouble("ir", record);
                            var infiltrationOrSeepage = RegularExpression.ParseFieldAsDouble("ii", record);
                            var area = RegularExpression.ParseFieldAsInt("ar", record);

                            sobekLateralFlow.IsConstantDischarge = true;
                            sobekLateralFlow.ConstantDischarge = 0.001 * (intensity + infiltrationOrSeepage)*area;
                            break;
                        case 7:
                            LogWarning("Unable to import Lateral Flow record; rational method from meteo station not yet supported; currently set to default", $"{sobekLateralFlow.Id} (dc lt {type})");
                            break;
                        default:
                            LogWarning("Unable to import Lateral Flow record; type not supported; set to default", $"record {record} (dc lt {type})");
                            break;
                    }
                }
                //diffuse lateral source
                if (match.Value.StartsWith("sc 0 lt -1"))
                {
                    sobekLateralFlow.IsPointDischarge = false;
                }
                if (match.Value.StartsWith("dc lw"))
                {
                    var match2 = RegularExpression.GetFirstMatch(@"dc lw (?<ltype>" + RegularExpression.Integer + @")", match.Value);
                    if (match2 == null)
                    {
                        LogWarning("Could not parse lateral flow specification for the following id's", $"id {sobekLateralFlow.Id} (\"{record}\")");
                        continue;
                    }
                    var type = int.Parse(match2.Groups["ltype"].Value);
                    switch (type)
                    {
                        case 2: // function of the waterlevel 
                            var dcmatch = RegularExpression.GetFirstMatch(@"dc lw 2\s*(?<table>" + RegularExpression.CharactersAndQuote + @")", record);
                            if (dcmatch == null)
                            {
                                LogWarning("Could not parse lateral flow specification for the following id's", $"id {sobekLateralFlow.Id} (\"{record}\")");
                                break;
                            }
                            sobekLateralFlow.IsConstantDischarge = false;
                            sobekLateralFlow.LevelQhTable = SobekDataTableReader.GetTable(dcmatch.Groups["table"].Value,
                                                                                  SobekFlowBoundaryCondition.QhTableStructure);
                            break;
                        default:
                            LogWarning("Unable to import Lateral Flow record; type not supported; set to default", $"record {record} (dc lt {type})");
                            break;
                    }
                }

            }

            const string pdinPattern = @"PDIN\s(?<pdin>" + RegularExpression.CharactersAndQuote + @") pdin" + RegularExpression.CharactersAndQuote + @"TBLE\s(?<tble>" + RegularExpression.CharactersAndQuote + @")tble";

            var pdinMatches = RegularExpression.GetMatches(pdinPattern, record);
            if (pdinMatches.Count > 0)
            {
                const string pdinSubPattern =
                    @"(?<pdin1>" + RegularExpression.Integer + @")\s(?<pdin2>" + RegularExpression.Integer + @")" +
                    @"(?<period>" + RegularExpression.CharactersAndQuote + @")";

                var pdin = pdinMatches[0].Groups["pdin"].ToString();
                var pdinSubMatches = RegularExpression.GetMatches(pdinSubPattern, pdin);

                if (pdinSubMatches.Count > 0)
                {

                    string pdin1 = pdinSubMatches[0].Groups["pdin1"].ToString();
                    string pdin2 = pdinSubMatches[0].Groups["pdin2"].ToString();
                    string period = pdinSubMatches[0].Groups["period"].ToString();
                    if (pdin1 == "0")
                    {
                        sobekLateralFlow.InterpolationType = InterpolationType.Linear;
                    }
                    else
                    {
                        sobekLateralFlow.InterpolationType = InterpolationType.Constant;
                    }

                    if (pdin2 == "1")
                    {
                        if (string.IsNullOrEmpty(period))
                        {
                            sobekLateralFlow.ExtrapolationType = ExtrapolationType.Constant;
                        }
                        else
                        {
                            sobekLateralFlow.ExtrapolationType = ExtrapolationType.Periodic;
                            sobekLateralFlow.ExtrapolationPeriod = period;
                        }

                    }
                    else
                    {
                        sobekLateralFlow.ExtrapolationType = ExtrapolationType.None;
                    }
                }
            }

            if ((sobekLateralFlow.LevelQhTable != null) && (sobekLateralFlow.InterpolationType != InterpolationType.Linear))
            {
                Log.WarnFormat("Interpolation of type {0} for {1} not supported; set to {2}", sobekLateralFlow.InterpolationType,
                    sobekLateralFlow.Id, InterpolationType.Linear);
                sobekLateralFlow.InterpolationType = InterpolationType.Linear;
            }

            return sobekLateralFlow;
        }

        private void ExtractLength(Match match, SobekLateralFlow sobekLateralFlow, string lengthCode)
        {
            if (match.Value.StartsWith(lengthCode))
            {
                if (match.Value.Length == 2)
                {
                    return;
                }
                var match2 = RegularExpression.GetFirstMatch(lengthCode + @" (?<length>" + RegularExpression.Integer + @")", match.Value);
                if (match2 == null)
                {
                    Log.WarnFormat("Could not parse length code of lateral flow specification with ID {0} (\"{1}\")", sobekLateralFlow.Id, lengthCode);
                    return;
                }
                var length = ConversionHelper.ToDouble(match2.Groups["length"].Value);
                if (length > 1.0e-6)
                {
                    sobekLateralFlow.IsPointDischarge = false;
                    sobekLateralFlow.Length = length;
                }
                else if (length == -1)
                {
                    sobekLateralFlow.IsPointDischarge = false;                    
                }
                else if (length == 0)
                {
                    sobekLateralFlow.IsPointDischarge = true;
                }
            }
        }
    }
}
