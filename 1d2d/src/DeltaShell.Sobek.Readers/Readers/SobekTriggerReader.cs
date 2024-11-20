using System;
using System.Collections.Generic;
using System.Data;
using System.Text.RegularExpressions;
using DelftTools.Utils.RegularExpressions;
using DeltaShell.Sobek.Readers.SobekDataObjects;

namespace DeltaShell.Sobek.Readers.Readers
{
    public class SobekTriggerReader : SobekReader<SobekTrigger>
    {
        public override IEnumerable<SobekTrigger> Parse(string datFileText)
        {
            const string pattern = @"(TRGR\s(?<row>(?'text'.*?))\strgr)";

            var triggerMatches = RegularExpression.GetMatches(pattern, datFileText);
            foreach (Match triggerMatch in triggerMatches)
            {
                var trigger = GetSobekTrigger(triggerMatch.Groups["row"].Value);
                
                if (trigger != null)
                {
                    RemoveInputLocationSobekTrigger(trigger);
                    yield return trigger;
                }
            }
        }

        private static SobekTrigger GetSobekTrigger(string rowText)
        {
            //id  = id of the trigger
            //nm  = name of the trigger 
            //ty  = trigger type
            //tp  = trigger parameter (for hydraulic /combined triggers only)
            //Note: These number differ from the ones used in the Nefis file, which uses 1,2,3,4,5,6,7,8. 

            //0 = water level at branch location
           // 1 = head difference over structure
            //2 = discharge at branch location
            //3 = gate height of structure
           // 4 = crest level of structure
            //5 = crest width of structure
            //6 = waterlevel in retention area
           // 7 = pressure difference over structure



            //A trigger refers to exactly 1 location (branch-location or structure)
            //ml = measurement station id
            //ts = structure id (for hydraulic/combined triggers only)
            //ch = check on (only relevant if trigger parameter=3,4,5)
            //0 = value (default)
            //1 = direction
            //tt tr  = trigger table containing 5 columns:

            //column 1 = time (all types of triggers, including the hydraulic trigger; in this case, the setpoint can de defined in time)
            //column 2 = on/off (for time trigger and combined trigger),
            //column 3 = and/or (for combined trigger), 
            //column 4 = operation (hydraulic trigger: < or >)
            //column 5 = trigger parameter (for hydraulic and combined trigger)

            //if the user in sobek changed the input, double data has been stored. This confuses the pattern -> 3 separate checks
            string inputPattern = "";
            string measurestationid = "";
            string structureid = "";
            string branchid = "";
            string chainage = "";

            inputPattern = @"(ml\s'(?<measurestationid>" + RegularExpression.ExtendedCharacters + @")'\s)";
            var inputMatches = RegularExpression.GetMatches(inputPattern, rowText);
            if (inputMatches.Count == 1)
            {
                measurestationid = inputMatches[0].Groups["measurestationid"].Value;
            }

            inputPattern = @"(tb\s'(?<branchid>" + RegularExpression.Characters + @")'\s*tl\s(?<chainage>" + RegularExpression.Scientific + @")\s)";
            inputMatches = RegularExpression.GetMatches(inputPattern, rowText);
            if (inputMatches.Count == 1)
            {
                branchid = inputMatches[0].Groups["branchid"].Value;
                chainage = inputMatches[0].Groups["chainage"].Value;
            }

            inputPattern = @"(ts\s'(?<structureid>" + RegularExpression.ExtendedCharacters + @")'\s)";
            inputMatches = RegularExpression.GetMatches(inputPattern, rowText);
            if (inputMatches.Count == 1)
            {
                structureid = inputMatches[0].Groups["structureid"].Value;
            }

            const string pattern = @"id\s'(?<id>" + RegularExpression.ExtendedCharacters + @")'\s*" +
                                   @"(nm\s'(?<name>" + RegularExpression.ExtendedCharacters + @")'\s)?" +
                                   @"ty\s(?<triggertype>" + RegularExpression.Integer + @")\s" +
                                   @"(t1\s(?<oncehydraulictrigger>" + RegularExpression.Integer + @")\s)?" +
                                   @"tp\s(?<triggerparametertype>" + RegularExpression.Integer + @")\s" +
                                   @"(tb\s'(?<tb>" + RegularExpression.ExtendedCharacters + @")'\s)?" +
                                   @"(tl\s(?<tl>" + RegularExpression.ExtendedCharacters + @"?)\s)?" +
                                   @"(ts\s'(?<ts>" + RegularExpression.ExtendedCharacters + @")'\s)?" +
                                   @"(ch\s(?<checkon>" + RegularExpression.Integer + @")\s)?" +
                                   RegularExpression.AnyNonGreedy +
                                   @"PDIN\s(?<pdin>" + RegularExpression.CharactersAndQuote + @") pdin" +
                                   RegularExpression.CharactersAndQuote +
                                   @"(?<table>TBLE(?'text'.*?)tble)";

            var matches = RegularExpression.GetMatches(pattern, rowText);

            if(matches.Count == 1)
            {
                var id = "TRG_" + matches[0].Groups["id"].Value;
                var name = matches[0].Groups["name"].Value;
                var triggertype = Convert.ToInt32((string) matches[0].Groups["triggertype"].Value);
                var triggerparametertype = Convert.ToInt32((string) matches[0].Groups["triggerparametertype"].Value);
                var checkon = (matches[0].Groups["checkon"].Value != "") ? Convert.ToInt32((string)matches[0].Groups["checkon"].Value) : 0;
                var oncehydraulictrigger = (matches[0].Groups["oncehydraulictrigger"].Value != "") ? Convert.ToInt32((string)matches[0].Groups["oncehydraulictrigger"].Value) : 0;
                var pdin = matches[0].Groups["pdin"].Value;
                var table = matches[0].Groups["table"].Value;

                var sobekTrigger = new SobekTrigger();

                sobekTrigger.Id = id;
                sobekTrigger.Name = name;
                sobekTrigger.TriggerType = (SobekTriggerType)triggertype;
                sobekTrigger.TriggerParameterType = (SobekTriggerParameterType)triggerparametertype;
                sobekTrigger.OnceHydraulicTrigger = (oncehydraulictrigger != 0);
                sobekTrigger.MeasurementStationId = measurestationid;
                if (!string.IsNullOrEmpty(branchid) && branchid != "-1" && (!string.IsNullOrEmpty(chainage)))
                {
                    sobekTrigger.MeasurementStationId = MeasurementLocationIdGenerator.GetMeasurementLocationId(branchid, Convert.ToDouble(chainage));
                }
                sobekTrigger.StructureId = structureid;
		        sobekTrigger.CheckOn = (SobekTriggerCheckOn)checkon;
                sobekTrigger.PeriodicExtrapolationPeriod = GiveExtrapolationPeriod(pdin);

                //table
                if(!string.IsNullOrEmpty(table))
                {
                    sobekTrigger.TriggerTable = SobekDataTableReader.GetTable((string) table, (DataTable) SobekTrigger.TriggerTableStructure);                    
                }
  
                return sobekTrigger;
            }

            return null;
        }

        private static string GiveExtrapolationPeriod(string pdin)
        {
            const string pattern = @"(?<pdin1>" + RegularExpression.Integer + @")\s*(?<pdin2>" + RegularExpression.Integer + @")\s*'" +
                    @"(?<period>" + RegularExpression.CharactersAndQuote + @")'";
            var matches = RegularExpression.GetMatches(pattern, pdin);

            if (matches.Count == 0) return"";

            return matches[0].Groups["period"].Value;
        }

        /// <summary>
        /// In Sobek files input locations can still be defined for controllers which don't have an input location (after changing type)
        /// </summary>
        /// <param name="sobekController"></param>
        private static void RemoveInputLocationSobekTrigger(SobekTrigger sobekTrigger)
        {
            if (sobekTrigger.TriggerType == SobekTriggerType.Time)
            {
                sobekTrigger.MeasurementStationId = "";
                sobekTrigger.StructureId = "";
            }
        }

        public override IEnumerable<string> GetTags()
        {
            yield return "trgr";
        }
    }
}
