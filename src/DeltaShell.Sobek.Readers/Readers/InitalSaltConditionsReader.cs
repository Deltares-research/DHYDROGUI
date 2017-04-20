using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using DelftTools.Utils;
using DelftTools.Utils.RegularExpressions;
using DeltaShell.Sobek.Readers.SobekDataObjects;
using log4net;

namespace DeltaShell.Sobek.Readers.Readers
{
    public class InitalSaltConditionsReader : SobekReader<SaltInitialCondition>
    {
        // isa-file (initial conditions layer)
        // This file contains the initial conditions for the salt module (SA). Global WQ initial conditions start with GLIN, have a carrier id of -1, and end with glin.
        // STIN id '1' nm 'Init1' ci '5' ty 0 co co 0 90 stin
        // or
        // GLIN STIN id '0' nm 'Initglobal' ci '-1' ty 1 co co 0 50 stin glin
        // 
        // Where:
        // id  = id of the initial condition
        // nm  = name of the initial condition
        // ci  = carrier id
        // ty  = type concentration: 
        // 0 = salt
        // 1 = chloride
        // co co  = table with initial salt conditions 
        // co co 0 = constant
        // co co 2 = table as a function of the location on branch
        // column 1 = location
        // column 2 = load

        private static readonly ILog Log = LogManager.GetLogger(typeof(InitalSaltConditionsReader));

        public override IEnumerable<SaltInitialCondition> Parse(string text)
        {
            const string structurePattern = @"(GLIN (?'text'.*?) glin)|(STIN (?'text'.*?)\sstin)";

            foreach (Match structureMatch in RegularExpression.GetMatches(structurePattern, text))
            {
                var flowInitialCondition = GetFlowInitialCondition(structureMatch.Value);
                if (flowInitialCondition != null)
                {
                    yield return flowInitialCondition;
                }
            }
        }

        public static SaltInitialCondition GetFlowInitialCondition(string record)
        {
            var flowInitialCondition = new SaltInitialCondition
                                           {
                                               Id = RegularExpression.ParseFieldAsString("id", record),
                                               Name = RegularExpression.ParseFieldAsString("nm", record),
                                               BranchId = RegularExpression.ParseFieldAsString("ci", record)
                                           };

            if (flowInitialCondition.BranchId == "-1")
            {
                flowInitialCondition.IsGlobalDefinition = true;
            }

            if (ParseInitialCondition(flowInitialCondition,flowInitialCondition.Salt, record))
            {
                return flowInitialCondition;
            }
            return null;
        }

        /// <summary>
        /// parses the part following co co of the STIN record. According to online Sobek help 
        /// is co co 0 {constant} or lv ll 2 {.. table}
        /// </summary>
        /// <param name="initialCondition"></param>
        /// <param name="text"></param>
        private static bool ParseInitialCondition(SaltInitialCondition condition, InitialCondition initialCondition, string text)
        {
            const string pattern = @"co\sco\s(0 (?<const>" + RegularExpression.Scientific + @"))|(1(?<table>" + 
                                        RegularExpression.CharactersAndQuote + @")tble{1}" + @")";
            
            var match = RegularExpression.GetFirstMatch(pattern, text);
            if (match != null && match.Groups["const"].Success)
            {
                initialCondition.IsConstant = true;
                initialCondition.Constant = ConversionHelper.ToDouble(match.Groups["const"].Value);
            }
            else
            {
                var newmatch = RegularExpression.GetFirstMatch(@"TBLE[^(tble)]+", text);
                if (newmatch == null)
                {
                    Log.WarnFormat("Could not read initial salt condition with ID {0} (\"{1}\")",condition.Id,text);
                    return false;
                }
                string table = newmatch.Value + " tble"; // <- adding of "tble" is hack
                initialCondition.Data = SobekDataTableReader.GetTable(table,
                                                                 new Dictionary<string, Type>
                                                                     {
                                                                         {"first", typeof (double)},
                                                                         {"second", typeof (double)}
                                                                     });
            }
            return true;
        }

        public override IEnumerable<string> GetTags()
        {
            yield return "glin";
            yield return "stin";
        }
    }
}
