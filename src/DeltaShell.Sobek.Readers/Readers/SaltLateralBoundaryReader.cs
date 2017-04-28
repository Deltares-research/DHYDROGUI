using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Text.RegularExpressions;
using DelftTools.Utils.RegularExpressions;
using DeltaShell.Sobek.Readers.SobekDataObjects;

namespace DeltaShell.Sobek.Readers.Readers
{
    public class SaltLateralBoundaryReader : SobekReader<SobekSaltBoundary>
    {
        // STBR id 0 ty 0 le 100 lo lt 0 10 0 co ct 0 
        //
        //  TBLE.. 
        //  tble 
        //  stbr
        //
        //  STBR id 0 ty 1 le 100 co ct 1 
        //  TBLE.. 
        //  tble 
        //  stbr
        //
        //  Where: 
        //
        //  id  =  id of the lateral discharge of salt
        //  ty  =  type condition
        //   0 = dry substance
        //   1 = concentration
        //  le   = length
        //  lo lt  = load table (for ty 0)
        //  lo lt 0 = constant
        //  lo lt 1 = table as function of time
        //  di  = id of accompanying lateral discharge (when ty 1: concentration)
        //  co ct = concentration table (only for ty 1)
        //  co ct 0 = constant
        //  co ct 1 = table as function of the time

        public override IEnumerable<SobekSaltBoundary> Parse(string text)
        {
            const string compoundstructuresPattern = @"(STBR(?'text'.*?)stbr)";

            foreach (Match match in RegularExpression.GetMatches(compoundstructuresPattern, text))
            {
                var sobekBoundaryLocation = GetSobekSaltBoundary(match.Value);
                if (sobekBoundaryLocation != null)
                {
                    yield return sobekBoundaryLocation;
                }
            }
        }

        public SobekSaltBoundary GetSobekSaltBoundary(string record)
        {
            var sobekSaltBoundary = new SobekSaltBoundary();

            sobekSaltBoundary.Id = RegularExpression.ParseFieldAsString("id", record);
            sobekSaltBoundary.Length = RegularExpression.ParseFieldAsDouble("le", record);
            sobekSaltBoundary.LateralId = RegularExpression.ParseFieldAsString("di", record);
            sobekSaltBoundary.SaltBoundaryType = (SaltBoundaryType) RegularExpression.ParseFieldAsInt("ty", record);

            if (sobekSaltBoundary.SaltBoundaryType == SaltBoundaryType.DrySubstance)
            {
                // parse lo lt
                const string pattern = @"lo lt\s((0\s(?<bound>" + RegularExpression.Scientific + @"))|1\s(?<table>" + RegularExpression.CharactersAndQuote + "))";
                var matches = RegularExpression.GetMatches(pattern, record);
                if (matches.Count != 1)
                {
                    return null;
                }
                if (matches[0].Groups["bound"].Success)
                {
                    sobekSaltBoundary.SaltStorageType = SaltStorageType.Constant;
                    sobekSaltBoundary.DryLoadConst = Convert.ToDouble(matches[0].Groups["bound"].Value,CultureInfo.InvariantCulture);
                }
                else if (matches[0].Groups["table"].Success)
                {
                    sobekSaltBoundary.SaltStorageType = SaltStorageType.FunctionOfTime;
                    sobekSaltBoundary.DryLoadTable = SobekDataTableReader.GetTable(matches[0].Groups["table"].Value, ConcentrationTable);
                }
            }
            else
            {
                // SaltBoundaryType.Concentration
                // parse co ct
                const string pattern = @"co ct\s((0\s(?<bound>" + RegularExpression.Scientific + @"))|1\s(?<table>" + RegularExpression.CharactersAndQuote + "))";
                var matches = RegularExpression.GetMatches(pattern, record);
                if (matches.Count != 1)
                {
                    return null;
                }
                if (matches[0].Groups["bound"].Success)
                {
                    sobekSaltBoundary.SaltStorageType = SaltStorageType.Constant;
                    sobekSaltBoundary.ConcentrationConst = Convert.ToDouble(matches[0].Groups["bound"].Value,CultureInfo.InvariantCulture);
                }
                else if (matches[0].Groups["table"].Success)
                {
                    sobekSaltBoundary.SaltStorageType = SaltStorageType.FunctionOfTime;
                    sobekSaltBoundary.ConcentrationTable = SobekDataTableReader.GetTable(matches[0].Groups["table"].Value, ConcentrationTable);
                }
            }
            return sobekSaltBoundary;
        }

        private DataTable ConcentrationTable
        {
            get
            {
                var dataTable = new DataTable();
                dataTable.BeginLoadData();
                dataTable.Columns.Add("Time", typeof(DateTime));
                dataTable.Columns.Add("concentration", typeof(double));
                return dataTable;
            }
        }

        public override IEnumerable<string> GetTags()
        {
            yield return "stbr";
        }
    }
}
