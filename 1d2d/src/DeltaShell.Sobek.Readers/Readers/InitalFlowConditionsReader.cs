using System.Collections.Generic;
using System.Text.RegularExpressions;
using DelftTools.Functions.Generic;
using DelftTools.Utils;
using DelftTools.Utils.RegularExpressions;
using DeltaShell.Sobek.Readers.SobekDataObjects;
using log4net;

namespace DeltaShell.Sobek.Readers.Readers
{
    public class InitalFlowConditionsReader : SobekReader<FlowInitialCondition>
    {
        #region example data
        // from Sobek online help
        // ifl-file (initial conditions layer)

        // This file contains the description of initial conditions of the flow module. For global flow conditions the additional keywords GLIN glin (and carrier id -1) are used.
        // For Channel Flow, Sewer Flow and River Flow:

        // FLIN id '1' nm 'Initcond1' ci '1' q_ lq 0 0.01 ty 1 lv ll 1 
        // TBLE .. 
        // tble 
        // flin
        // or
        // GLIN FLIN id '1' nm 'Initcond1' ci '-1' q_ lq 0 0.01 ty 0 lv ll 0 0.05 flin glin

        // where: 
        // id   = id 
        // nm   = name
        // ci   = carrier id (branch id)
        // q_ lq  = initial discharge 
        // q_ lq 0 = as a constant, 
        // q_ lq 2 = as a function of the location on the branch
        // column 1 = location
        // column 2 = discharge
        // ty   = type water level/depth 
        // 1 = water level
        // 0 = water depth
        // lv ll   = value for depth or water level 
        // lv ll 0 = constant 
        // lv ll 2 = table as function of the location op de branch
        // column 1 = location, 
        // column 2 = water depth or water level
        #endregion

        private static readonly ILog Log = LogManager.GetLogger(typeof(InitalFlowConditionsReader));

        public override IEnumerable<FlowInitialCondition> Parse(string text)
        {
            const string structurePattern = @"(GLIN (?'text'.*?) glin)|(FLIN (?'text'.*?)\sflin)";

            foreach (Match structureMatch in RegularExpression.GetMatches(structurePattern, text))
            {
                FlowInitialCondition flowInitialCondition = GetFlowInitialCondition(structureMatch.Value);
                if (flowInitialCondition != null)
                {
                    yield return flowInitialCondition;
                }
            }
        }

        public FlowInitialCondition GetFlowInitialCondition(string record)
        {
            FlowInitialCondition flowInitialCondition = new FlowInitialCondition();

            flowInitialCondition.ID = RegularExpression.ParseFieldAsString("id", record);
            flowInitialCondition.Name = RegularExpression.ParseFieldAsString("nm", record);
            flowInitialCondition.BranchID = RegularExpression.ParseFieldAsString("ci", record);
                
            if (flowInitialCondition.BranchID == "-1")
            {
                flowInitialCondition.IsGlobalDefinition = true;
            }

            const string patternQ = @"q_ lq" + @"\s(?<q>" + RegularExpression.CharactersAndQuote + @")\s*" + "(ty)?";
            var match = RegularExpression.GetFirstMatch(patternQ, record);
            if (match != null && match.Groups["q"].Success)
            {
                flowInitialCondition.IsQBoundary = true;
                ParseInitialCondition(flowInitialCondition,flowInitialCondition.Discharge, match.Groups["q"].Value);
            }

            const string patternT = @"(ty\s(?<ty>" + RegularExpression.Integer + @")\s?)lv ll";
            match = RegularExpression.GetFirstMatch(patternT, record);
            if (match != null)
            {
                flowInitialCondition.IsLevelBoundary = true;
                flowInitialCondition.WaterLevelType = (FlowInitialCondition.FlowConditionType) int.Parse(match.Groups["ty"].Value);
                const string patternL = @"lv ll" + @"\s(?<l>" + RegularExpression.CharactersAndQuote + @")\s" + "(tble)*";
                
                match = RegularExpression.GetFirstMatch(patternL, record);
                if (match != null && match.Groups["l"].Success)
                {
                    ParseInitialCondition(flowInitialCondition,flowInitialCondition.Level, match.Groups["l"].Value);
                }
            }
            return flowInitialCondition;
        }

        /// <summary>
        /// parses the part following lv ll of the FLIN record. According to online Sobek help and also found in some 
        /// Sobek river files and SobekRe files this is lv ll 0 {constant} or lv ll 2 {.. table}
        /// Sobek river also writes files lv ll 1 {.. table}
        /// </summary>
        /// <param name="initialCondition"></param>
        /// <param name="text"></param>
        private static void ParseInitialCondition(FlowInitialCondition condition, InitialCondition initialCondition, string text)
        {
            const string pattern = @"(0\s+(?<const>" + RegularExpression.OptionalFloat + @"))|((2|1)" +
                                   "(?<table>" + RegularExpression.CharactersAndQuote + @")tble{1}" + @")";
            
            var match = RegularExpression.GetFirstMatch(pattern, text);
            if (match == null)
            {
                Log.WarnFormat("Could not parse initial condition with ID \"{0}\" (\"{1}\")",condition.ID,text);
                return;
            }

            if (match.Groups["const"].Success)
            {
                initialCondition.IsConstant = true;
                if (match.Groups["const"].Value != "")
                {
                    initialCondition.Constant = ConversionHelper.ToDouble(match.Groups["const"].Value);
                }
            }
            else
            {
                var newmatch = RegularExpression.GetFirstMatch(@"TBLE[^(tble)]+", text);
                if (newmatch == null)
                {
                    Log.WarnFormat("Could not parse initial condition with ID {0} (\"{1}\")",condition.ID,text);
                }
                else
                {
                    string table = newmatch.Value + " tble"; // <- adding of "tble" is hack
                    initialCondition.Data = SobekDataTableReader.GetTable(table,
                                                                     new Dictionary<string, System.Type>
                                                                     {
                                                                         {"first", typeof (double)},
                                                                         {"second", typeof (double)}
                                                                     });
                }
                
                //interpolation
                newmatch = RegularExpression.GetFirstMatch(@"PDIN\s*(?<interpolation>" + RegularExpression.Integer + @")", text);
                if (newmatch != null && newmatch.Groups["interpolation"].Success)
                {
                    initialCondition.Interpolation = newmatch.Groups["interpolation"].Value == "1"
                                                         ? InterpolationType.Linear
                                                         : InterpolationType.Constant;
                }
            }
        }

        public override IEnumerable<string> GetTags()
        {
            yield return "glin";
            yield return "flin";
        }
    }
}
