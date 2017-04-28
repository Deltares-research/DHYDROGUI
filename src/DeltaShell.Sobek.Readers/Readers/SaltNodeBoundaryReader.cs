using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Text.RegularExpressions;
using DelftTools.Utils.RegularExpressions;
using DeltaShell.Sobek.Readers.SobekDataObjects;

namespace DeltaShell.Sobek.Readers.Readers
{
    public class SaltNodeBoundaryReader : SobekReader<SobekSaltNodeBoundary>
    {
        // STBO id '1' ty 1 co co 1 
        // TBLE .. 
        // tble
        // tl 0 tu 0 stbo
        //
        // Where:
        // id  = id
        // ty  = type of boundary
        // 1= zero flux
        // 0= concentration
        //
        // co co  = concentrations (only for type 0)
        // co co 0 = as constant
        // co co 1 = as table 
        // column 1 = time
        // column 2 =concentrations
        //  tl = Thatcher-Harleman time lag (only for SOBEK River, concentration boundary)
        //  tu = time lag used (only for SOBEK River model, concentration boundary)

        public override IEnumerable<SobekSaltNodeBoundary> Parse(string text)
        {
            const string compoundstructuresPattern = @"(STBO(?'text'.*?)stbo)";

            foreach (Match match in RegularExpression.GetMatches(compoundstructuresPattern, text))
            {
                var sobekBoundaryLocation = GetSobekSaltBoundary(match.Value);
                if (sobekBoundaryLocation != null)
                {
                    yield return sobekBoundaryLocation;
                }
            }
        }

        public static SobekSaltNodeBoundary GetSobekSaltBoundary(string record)
        {
            var sobekSaltBoundary = new SobekSaltNodeBoundary();

            sobekSaltBoundary.Id = RegularExpression.ParseFieldAsString("id", record);
            sobekSaltBoundary.SaltBoundaryNodeType = (SaltBoundaryNodeType) RegularExpression.ParseFieldAsInt("ty", record);

            if (sobekSaltBoundary.SaltBoundaryNodeType == SaltBoundaryNodeType.ZeroFlux)
            {
                sobekSaltBoundary.SaltBoundaryNodeType = SaltBoundaryNodeType.ZeroFlux;
            }
            else
            {
                sobekSaltBoundary.SaltBoundaryNodeType = SaltBoundaryNodeType.Concentration;
                // SaltBoundaryNodeType.Concentration
                // parse co ct
                const string pattern = @"co co\s((0\s(?<bound>" + RegularExpression.Scientific + @"))|1\s(?<table>" + RegularExpression.CharactersAndQuote + "))";
                var matches = RegularExpression.GetMatches(pattern, record);
                if (matches.Count != 1)
                {
                    return null;
                }
                if (matches[0].Groups["bound"].Success)
                {
                    sobekSaltBoundary.SaltStorageType = SaltStorageType.Constant;
                    sobekSaltBoundary.ConcentrationConst = Convert.ToDouble(matches[0].Groups["bound"].Value, CultureInfo.InvariantCulture);
                }
                else if (matches[0].Groups["table"].Success)
                {
                    sobekSaltBoundary.SaltStorageType = SaltStorageType.FunctionOfTime;
                    sobekSaltBoundary.ConcentrationTable = SobekDataTableReader.GetTable(matches[0].Groups["table"].Value, ConcentrationTable);
                }

                //TimeLag
                var label = "tl";
                var pat = RegularExpression.GetScientific(label);
                matches = RegularExpression.GetMatches(pat, record);
                if (matches.Count == 1)
                {
                    sobekSaltBoundary.TimeLag = Convert.ToDouble(matches[0].Groups[label].Value, CultureInfo.InvariantCulture);
                }
            }
            return sobekSaltBoundary;
        }

        private static DataTable ConcentrationTable
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
            yield return "stbo";
        }
    }
}