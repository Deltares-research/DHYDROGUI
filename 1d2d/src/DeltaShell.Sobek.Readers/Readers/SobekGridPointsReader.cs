using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using DelftTools.Utils.RegularExpressions;
using DeltaShell.Sobek.Readers.SobekDataObjects;
using log4net;

namespace DeltaShell.Sobek.Readers.Readers
{
    public class SobekGridPointsReader : SobekReader<CalcGrid>
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(SobekGridPointsReader));

        public enum SobekGridPointsTypeEnum
        {
            Unknown,
            Gr10,
            Gr11,
            Gr12
        }

        public SobekGridPointsTypeEnum SobekGridPointsType { get; set; }

        public override IEnumerable<CalcGrid> Read(string filePath)
        {
            if (!File.Exists(filePath))
            {
                return Enumerable.Empty<CalcGrid>();
            }

            using (var sr = new StreamReader(filePath, Encoding.Default))
            {
                var header = sr.ReadLine();
                if (header == null)
                {
                    return Enumerable.Empty<CalcGrid>();
                }
                
                SobekGridPointsType = SobekGridPointsTypeEnum.Unknown;
                if (header.StartsWith("GR_1.0"))
                {
                    SobekGridPointsType = SobekGridPointsTypeEnum.Gr10;
                }
                else if (header.StartsWith("GR_1.1"))
                {
                    SobekGridPointsType = SobekGridPointsTypeEnum.Gr11;
                }
                else if (header.StartsWith("GR_1.2"))
                {
                    SobekGridPointsType = SobekGridPointsTypeEnum.Gr12;
                }
            }

            return base.Read(filePath);
        }

        public override IEnumerable<CalcGrid> Parse(string fileContent)
        {
            const string gridPointsPattern = @"(GRID (?'text'.*?)grid)";

            if (SobekGridPointsType == SobekGridPointsTypeEnum.Gr10)
            {
                return (from Match gridPointsPerBranch in RegularExpression.GetMatches(gridPointsPattern, fileContent)
                        select GetSobekGridPointsPerBranch10(gridPointsPerBranch.Value)).ToList();
            }

            if (SobekGridPointsType == SobekGridPointsTypeEnum.Gr11)
            {
                return (from Match gridPointsPerBranch in RegularExpression.GetMatches(gridPointsPattern, fileContent)
                        select GetSobekGridPointsPerBranch11(gridPointsPerBranch.Value)).ToList();
            }
            if (SobekGridPointsType == SobekGridPointsTypeEnum.Gr12)
            {
                return (from Match gridPointsPerBranch in RegularExpression.GetMatches(gridPointsPattern, fileContent)
                        select GetSobekGridPointsPerBranch12(gridPointsPerBranch.Value)).ToList();

            }
            Log.WarnFormat("Grid version definition not found, assuming version Re version (offset only)");
            return (from Match gridPointsPerBranch in RegularExpression.GetMatches(gridPointsPattern, fileContent)
                    select GetSobekGridPointsPerBranchRe(gridPointsPerBranch.Value)).ToList();
        }

        private static CalcGrid GetSobekGridPointsPerBranch11(string record)
        {
            //            GR_1.1
            //GRID id '1' ci '1' re 0 dc 0 gr gr 
            //'GridPoint Table' PDIN 0 0 '' pdin CLTT 'Location' '1/R' cltt CLID '' '' clid
            //TBLE
            //0 0 '' '1' '1_1' 0 0 <
            //127.015281637859 0 '1_1' 'C4' '1_2' 127.015281637859 0 <
            //3359.319312144 0 '1_4' '6' '1_5' 3359.319312144 0 <
            //5603.78133990958 0 '1_5' '7' '1_6' 5603.78133990958 0 <
            //9078.58095625083 0 '1_6' '5' '1_7' 9078.58095625083 0 <
            //9189.0100423178 0 '1_7' '1_1' '1_8' 9189.0100423178 0 <
            //9190.0100423178 0 '1_9' '1_2' '1_10' 9190.0100423178 0 <
            //16362.208604901 0 '1_10' '14' '1_11' 16362.208604901 0 <
            //16431.2984695909 0 '1_11' '1_3' '1_12' 16431.2984695909 0 <
            //16432.2984695909 0 '1_13' '1_4' '1_14' 16432.2984695909 0 <
            //19877.043946651 0 '1_14' 'C3' '1_15' 19877.043946651 0 <
            //20000 0 '1_16' '2' '' 20000 0 <
            //tble grid
            const string pattern = @"TBLE\s(" + RegularExpression.CharactersAndQuote + ")tble";
            
            var calcGrid = new CalcGrid
            {
                BranchID = RegularExpression.ParseFieldAsString("ci", record)
            };

            var match = RegularExpression.GetFirstMatch(pattern, record);
            if (match == null)
            {
                Log.WarnFormat("Something went wrong while parsing grid points at channel {0}. \"{1}\" was not recognized as a valid grid definition.", calcGrid.BranchID, record);
                return calcGrid;
            }

            var dataTable = new DataTable();
            dataTable.BeginLoadData(); 
            dataTable.Columns.Add("offset", typeof(double));
            dataTable.Columns.Add("angle", typeof(double));
            dataTable.Columns.Add("from", typeof(string));
            dataTable.Columns.Add("id", typeof(string));
            dataTable.Columns.Add("to", typeof(string));
            dataTable.Columns.Add("unknown1", typeof(double));
            dataTable.Columns.Add("unknown2", typeof(double));
            

            var table = SobekDataTableReader.GetTable(match.Value, dataTable);

            foreach (DataRow row in table.Rows)
            {

                calcGrid.GridPoints.Add(new SobekCalcGridPoint
                {
                    Offset = (double)row["offset"],
                    Id = row["id"].ToString().Replace("'", ""),
                    SegmentId = row["to"].ToString().Replace("'", ""),
                });
            }
            return calcGrid;
        }

        private static CalcGrid GetSobekGridPointsPerBranch12(string record)
        {
            // GRID_1.2
            // GRID id '3' ci '3' re 0 dc 0 gr gr 
            //'GridPoint Table' PDIN 0 0 '' pdin CLTT 'Location' '1/R' cltt CLID '' '' clid
            //TBLE
            //0 0 '' 'Up_Bnd_Cul_3' '4' -200500 500050 '' '' '' <
            //100 0 '8' '3_1' '3_1' -200400 500050 '' '' '' <
            //200 0 '3_1' 'Up_Cul_3' 'R_Cul3' -200300 500050 '' '' '' <
            //250 0 'R_Cul3' 'Culvert_3' '15' -200250 500050 '' '' '' <
            //300 0 '15' 'Dwn_Cul_3' '3_3' -200200 500050 '' '' '' <
            //400 0 '3_3' '3_4' '3_4' -200100 500050 '' '' '' <
            //500 0 '13' 'Dwn_Bnd_Cul_3' '' -200000 500050 '' '' '' <
            //tble grid

            var dataTable = new DataTable();
            dataTable.BeginLoadData(); 
            dataTable.Columns.Add("offset", typeof(double));
            dataTable.Columns.Add("angle", typeof(double));
            dataTable.Columns.Add("from", typeof(string));
            dataTable.Columns.Add("id", typeof(string));
            dataTable.Columns.Add("to", typeof(string));
            dataTable.Columns.Add("unknown1", typeof(double));
            dataTable.Columns.Add("unknown2", typeof(double));
            dataTable.Columns.Add("unknown3", typeof(string));
            dataTable.Columns.Add("name", typeof(string));

            var gridPoint = new CalcGrid
            {
                BranchID = RegularExpression.ParseFieldAsString("ci", record)
            };

            const string pattern = @"TBLE\s(" + RegularExpression.CharactersAndQuote + ")tble";
            var match = RegularExpression.GetFirstMatch(pattern, record);
            if (match == null)
            {
                Log.WarnFormat("Something went wrong while parsing grid points at channel {0}. \"{1}\" was not recognized as a valid grid definition.",gridPoint.BranchID,record);
                return gridPoint;
            }
            
            var table = SobekDataTableReader.GetTable(match.Value, dataTable);
            
            foreach (DataRow row in table.Rows)
            {
                gridPoint.GridPoints.Add(new SobekCalcGridPoint
                {
                    Offset = (double)row["offset"],
                    Id = row["id"].ToString().Replace("'", ""),
                    SegmentId = row["to"].ToString().Replace("'", ""),
                    Name = row["name"].ToString().Replace("'", "")
                });
            }
            return gridPoint;

        }

        private static CalcGrid GetSobekGridPointsPerBranch10(string record)
        {
            //GR_1.0
            //GRID id '2' ci '2' gr gr 
            //'GridPoint Table' PDIN 0 0 '' pdin CLTT 'Location' '1/R' cltt CLID '' '' clid
            //TBLE
            //0 0 '' '4' '2' <
            //83.1666666669771 0 '2' '2_1' '2_3' <
            //166.333333333023 0 '7' '2_2' '2_5' <
            //249.5 0 '2_5' '15' '2_4' <
            //250 0 '2_4' '8' '2_1' <
            //250.5 0 '2_1' '16' '2_2' <
            //333.666666666977 0 '2_2' '2_3' '2_6' <
            //416.833333333023 0 '5' '2_4' '2_7' <
            //500 0 '2_7' '1' '' <
            //tble grid
            var gridPoint = new CalcGrid
            {
                BranchID = RegularExpression.ParseFieldAsString("ci", record)
            };

            const string pattern = @"TBLE\s(" + RegularExpression.CharactersAndQuote + ")tble";
            var match = RegularExpression.GetFirstMatch(pattern, record);
            if (match == null)
            {
                Log.WarnFormat("Something went wrong while parsing grid points at channel {0}. \"{1}\" was not recognized as a valid grid definition.", gridPoint.BranchID, record);
                return gridPoint;
            }
            
            var dataTable = new DataTable();
            dataTable.Columns.Add("offset", typeof(double));
            dataTable.Columns.Add("angle", typeof(double));
            dataTable.Columns.Add("from", typeof(string));
            dataTable.Columns.Add("id", typeof(string));
            dataTable.Columns.Add("to", typeof(string));

            var table = SobekDataTableReader.GetTable(match.Value, dataTable);

            
            foreach (DataRow row in table.Rows)
            {

                gridPoint.GridPoints.Add(new SobekCalcGridPoint
                                             {
                                                 Offset = (double)row["offset"],
                                                 Id = row["id"].ToString().Replace("'", ""),
                                                 SegmentId = row["to"].ToString().Replace("'", "")
                                             });
            }
            return gridPoint;
        }

        public static CalcGrid GetSobekGridPointsPerBranchRe(string record)
        {
            // GRID id 'LA1_883' nm '(null)' ci 'LA1_14' lc 9.9999e+009 se 0 oc 0 gr gr 'GridPoints on Branch <La06UPLa-MdLa> with 
            //       -->    length: 1341.0' PDIN 0 0 '' pdin CLTT 'Location [m]' '1/R [1/m]' cltt CLID '(null)' '(null)' clid TBLE 
            // 0 9.9999e+009 < 
            // 670 9.9999e+009 < 
            // 1341 9.9999e+009 < 
            // tble
            //  grid
           
            var calcGrid = new CalcGrid
            {
                BranchID = RegularExpression.ParseFieldAsString("ci", record)
            };

            // do not import grid points when 'on cross sections' is specified:
            if (RegularExpression.ParseFieldAsInt("oc", record) == 1)
            {
                return calcGrid;
            }


            const string pattern = @"TBLE\s(" + RegularExpression.CharactersAndQuote + ")tble";
            var match = RegularExpression.GetFirstMatch(pattern, record);
            if (match == null)
            {
                Log.WarnFormat("No computational grid points given for channel {0}", calcGrid.BranchID);
                return calcGrid;
            }
            
            var dataTable = new DataTable();
            dataTable.Columns.Add("offset", typeof(double));
            dataTable.Columns.Add("dummy", typeof(double));

            var table = SobekDataTableReader.GetTable(match.Value, dataTable);


            foreach (DataRow row in table.Rows)
            {
                calcGrid.GridPoints.Add(new SobekCalcGridPoint
                {
                    Offset = (double)row["offset"]
                });
            }
            return calcGrid;
        }

        public override IEnumerable<string> GetTags()
        {
            yield return "grid";
        }
    }
}
