using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.CrossSections;
using DelftTools.Utils.Csv.Importer;
using log4net;

namespace DeltaShell.Plugins.NetworkEditor.ImportExportCsv
{
    public class CrossSectionYZFromCsvFileImporter : CrossSectionFromCsvFileImporterBase
    {
        private class YZProfileData
        {
            public double YValue;
            public double ZValue;
            public double Storage;
        }

        private class CrossSectionYZCsvData: CrossSectionCsvData
        {
            public IList<YZProfileData> YzCoordinates;
        }

        private static readonly ILog log = LogManager.GetLogger(typeof(CrossSectionYZFromCsvFileImporter));

        private static readonly Dictionary<CsvRequiredField, CsvColumnInfo> columnMapping =
            new Dictionary<CsvRequiredField, CsvColumnInfo>
                {
                    {new CsvRequiredField("name", typeof (string)), new CsvColumnInfo(0, null)},
                    {new CsvRequiredField("channel", typeof (string)), new CsvColumnInfo(1, null)},
                    {new CsvRequiredField("offset", typeof (double)), new CsvColumnInfo(2, new NumberFormatInfo())},
                    {new CsvRequiredField("yValue", typeof (double)), new CsvColumnInfo(3, new NumberFormatInfo())},
                    {new CsvRequiredField("zValue", typeof (double)), new CsvColumnInfo(4, new NumberFormatInfo())},
                    {new CsvRequiredField("storageValue", typeof (double)), new CsvColumnInfo(5, new NumberFormatInfo())}
                };

        public override string Name
        {
            get { return "YZ Cross sections from CSV"; }
        }

        protected override CrossSectionType CrossSectionType
        {
            get { return CrossSectionType.YZ; }
        }

        protected override IDictionary<CsvRequiredField, CsvColumnInfo> CsvFieldToColumnMapping
        {
            get { return columnMapping; }
        }

        protected override IList<CrossSectionCsvData> ReadDataTable(DataTable dataTable)
        {
            var crossSectionData = new List<CrossSectionCsvData>();
           
            string oldName = null;
            var line = 0;
            foreach (DataRow row in dataTable.Rows)
            {
                YZProfileData profileData;
                var name = row["name"] as string;
                if (!Equals(name, oldName))
                {
                    string branchName;
                    double chainage;
                    try
                    {
                        branchName = row["channel"] as string;
                        chainage = row["offset"] is double ? (double) row["offset"] : double.NaN;
                        profileData = new YZProfileData
                            {
                                YValue = (double) row["yValue"],
                                ZValue = (double) row["zValue"],
                                Storage = (double) row["storageValue"]
                            };
                    }
                    catch (Exception e)
                    {
                        log.Warn("Reading data table row failed for line" + line + ", exception thrown: " + e.Message);
                        oldName = name;
                        line++;
                        continue;
                    }
                    crossSectionData.Add(
                        new CrossSectionYZCsvData
                            {
                                Name = name,
                                Branch = branchName,
                                Chainage = chainage,
                                YzCoordinates = new List<YZProfileData>(new[]{profileData})
                            });
                }
                else
                {
                    try
                    {
                        profileData = new YZProfileData
                        {
                            YValue = (double)row["yValue"],
                            ZValue = (double)row["zValue"],
                            Storage = (double)row["storageValue"]
                        };
                    }
                    catch (Exception e)
                    {
                        log.Warn("Reading data table row failed for line" + line + ", exception thrown: " + e.Message);
                        oldName = name;
                        line++;
                        continue;
                    }
                    var lastProfile = ((CrossSectionYZCsvData)crossSectionData.Last()).YzCoordinates;
                    if (lastProfile.Select(c => c.YValue).Contains(profileData.YValue))
                    {
                        oldName = name;
                        line++;
                        continue;
                    }
                    lastProfile.Add(profileData);
                }
                oldName = name;
                line++;
            }
            return crossSectionData;
        }

        protected override void UpdateCrossSectionDefinition(ICrossSectionDefinition crossSectionDefinition, CrossSectionCsvData crossSectionCsvData, IHydroNetwork target)
        {
            var yzDataTable = ((CrossSectionDefinitionYZ) crossSectionDefinition).YZDataTable;
            yzDataTable.Clear();
            foreach (var c in ((CrossSectionYZCsvData)crossSectionCsvData).YzCoordinates)
            {
                yzDataTable.AddCrossSectionYZRow(c.YValue, c.ZValue);
            }
        }
    }
}