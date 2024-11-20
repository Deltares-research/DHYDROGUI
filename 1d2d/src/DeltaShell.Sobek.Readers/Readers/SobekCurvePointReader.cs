using System.Collections.Generic;
using System.Data;
using System.Text.RegularExpressions;
using DelftTools.Utils.RegularExpressions;
using DeltaShell.Sobek.Readers.SobekDataObjects;

namespace DeltaShell.Sobek.Readers.Readers
{
    public class SobekCurvePointReader : SobekReader<BranchGeometry>
    {
        public override IEnumerable<BranchGeometry> Parse(string text)
        {
            const string curvePattern = @"(BRCH(?'text'.*?)brch)";

            foreach (Match curveMatch in RegularExpression.GetMatches(curvePattern, text))
            {
                BranchGeometry branchGeometry = GetBranchGeometry(curveMatch.Value);
                if (branchGeometry != null)
                {
                    yield return branchGeometry;
                }
            }
        }

        private static DataTable CurvePointTable
        {
            get
            {
                DataTable dataTable = new DataTable();
                dataTable.BeginLoadData(); 
                dataTable.Columns.Add("offset", typeof(double));
                dataTable.Columns.Add("angle", typeof(double));
                return dataTable;
            }
        }


        public static BranchGeometry GetBranchGeometry(string record)
        {
            BranchGeometry branchGeometry = new BranchGeometry();
            const string pattern = @"ct bc\s*(?<table>" + RegularExpression.CharactersAndQuote + @")";
            var matches = RegularExpression.GetMatches(pattern, record);
            var match = matches[0];

            branchGeometry.BranchID = RegularExpression.ParseFieldAsString("id", record);
            string tableString = match.Groups["table"].Value;
            if (!tableString.Contains("TBLE"))
            {
                // no table given; geometry has no additional curvepoints.
                return branchGeometry;
            }
            var table = SobekDataTableReader.GetTable(tableString, CurvePointTable);
            foreach (DataRow row in table.Rows)
            {
                double x = (double) row[0];
                double y = (double) row[1];

                branchGeometry.CurvingPoints.Add(new CurvingPoint(x, y));
            }
            return branchGeometry;
        }

        public override IEnumerable<string> GetTags()
        {
            yield return "brch";
        }
    }
}
