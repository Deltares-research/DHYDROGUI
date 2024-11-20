using System;
using DelftTools.Utils;
using DelftTools.Utils.RegularExpressions;
using DeltaShell.Sobek.Readers.SobekDataObjects;
using log4net;

namespace DeltaShell.Sobek.Readers.Readers
{
    public class SobekUniversalWeirReader : ISobekStructureReader
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(SobekUniversalWeirReader));

        public int Type
        {
            get
            {
                return 11;    
            }
        }
        
        public ISobekStructureDefinition GetStructure(string text)
        {
            //string source = @"cl 1 si '1' ce 1 sv 0.667 rt 0";
            // WHEN PROBLEMS PARSING STRUCTURE DEFINITION DUE TO SEQUENCE OF FIELDS 
            // SEE : SobekCulvertReader.GetStructure
            string pattern = RegularExpression.GetScientific("cl") + RegularExpression.GetCharacters("si") + RegularExpression.GetScientific("ce") +
                             RegularExpression.Characters + RegularExpression.GetInteger("rt");

            var match = RegularExpression.GetFirstMatch(pattern, text);
            if (match == null)
            {
                Log.WarnFormat("Could not parse weir definition (\"{0}\")",text);
                return null;
            }

            return new SobekUniversalWeir
                       {
                           CrestLevelShift = ConversionHelper.ToSingle(match.Groups["cl"].Value),
                           CrossSectionId = match.Groups["si"].Value,
                           DischargeCoefficient = ConversionHelper.ToSingle(match.Groups["ce"].Value),
                           FlowDirection = Convert.ToInt32(match.Groups["rt"].Value)
                       };
        }
    }
}