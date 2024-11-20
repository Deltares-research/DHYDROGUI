using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.CrossSections;
using DelftTools.Utils.Csv.Importer;
using GeoAPI.Geometries;
using log4net;
using NetTopologySuite.Geometries;

namespace DeltaShell.Plugins.NetworkEditor.ImportExportCsv
{
    public class CrossSectionXYZFromCsvFileImporter: CrossSectionFromCsvFileImporterBase
    {
        private class XYZGeometryData
        {
            public double XValue;
            public double YValue;
            public double ZValue;
            public double Storage;
        }

        private class CrossSectionXYZCsvData: CrossSectionCsvData
        {
            public IList<XYZGeometryData> GeometryData;
        }

        private static readonly ILog log = LogManager.GetLogger(typeof(CrossSectionXYZFromCsvFileImporter));

        private static readonly Dictionary<CsvRequiredField, CsvColumnInfo> columnMapping =
            new Dictionary<CsvRequiredField, CsvColumnInfo>
                {
                    {new CsvRequiredField("name", typeof (string)), new CsvColumnInfo(0, null)},
                    {new CsvRequiredField("branch", typeof (string)), new CsvColumnInfo(1, null)},
                    {new CsvRequiredField("chainage", typeof (double)), new CsvColumnInfo(2, new NumberFormatInfo())},
                    {new CsvRequiredField("x", typeof (double)), new CsvColumnInfo(3, new NumberFormatInfo())},
                    {new CsvRequiredField("y", typeof (double)), new CsvColumnInfo(4, new NumberFormatInfo())},
                    {new CsvRequiredField("z", typeof (double)), new CsvColumnInfo(5, new NumberFormatInfo())},
                    {new CsvRequiredField("storage", typeof (double)), new CsvColumnInfo(6, new NumberFormatInfo())}
                };

        public override string Name
        {
            get { return "XYZ Cross sections from CSV"; }
        }

        protected override CrossSectionType CrossSectionType
        {
            get { return CrossSectionType.GeometryBased; }
        }

        protected override IDictionary<CsvRequiredField, CsvColumnInfo> CsvFieldToColumnMapping
        {
            get { return columnMapping; }
        }

        protected override IList<CrossSectionCsvData> ReadDataTable(DataTable dataTable)
        {
            var crossSectionData = new List<CrossSectionCsvData>();

            string oldName = null;
            int line = 0;
            foreach (DataRow row in dataTable.Rows)
            {
                XYZGeometryData geometryData;
                var name = row["name"] as string;
                if (!Equals(name, oldName))
                {
                    string branchName;
                    double chainage;
                    try
                    {
                        branchName = row["branch"] as string;
                        chainage = row["chainage"] is double ? (double) row["chainage"] : double.NaN;
                        geometryData = new XYZGeometryData
                        {
                            XValue = (double)row["x"],
                            YValue = (double)row["y"],
                            ZValue = (double)row["z"],
                            Storage = (double)row["storage"]
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
                        new CrossSectionXYZCsvData
                            {
                                Name = name,
                                Branch = branchName,
                                Chainage = chainage,
                                GeometryData = new List<XYZGeometryData>(new[] {geometryData})
                            });
                }
                else
                {
                    try
                    {
                        geometryData = new XYZGeometryData
                        {
                            XValue = (double)row["x"],
                            YValue = (double)row["y"],
                            ZValue = (double)row["z"],
                            Storage = (double)row["storage"]
                        };
                    }
                    catch (Exception e)
                    {
                        log.Warn("Reading data table row failed for line" + line + ", exception thrown: " + e.Message);
                        oldName = name;
                        line++;
                        continue;
                    }
                    var lastProfile = ((CrossSectionXYZCsvData)crossSectionData.Last()).GeometryData;
                    if (lastProfile.Select(c => new Tuple<double, double>(c.XValue, c.YValue))
                                   .Contains(new Tuple<double, double>(geometryData.XValue, geometryData.YValue)))
                    {
                        oldName = name;
                        line++;
                        continue;
                    }
                    lastProfile.Add(geometryData);
                }
                oldName = name;
                line++;
            }
            return crossSectionData;
        }

        protected override void UpdateCrossSectionDefinition(ICrossSectionDefinition crossSectionDefinition, CrossSectionCsvData crossSectionCsvData, IHydroNetwork target)
        {
            var crossSectionDefinitionXYZ = (CrossSectionDefinitionXYZ) crossSectionDefinition;
            var crossSectionXYZCsvData = (CrossSectionXYZCsvData) crossSectionCsvData;

            crossSectionDefinitionXYZ.Geometry =
                new LineString(
                    crossSectionXYZCsvData.GeometryData.Select(
                        g => new Coordinate(g.XValue, g.YValue, g.ZValue)).ToArray());

            var profileEnumerator = crossSectionXYZCsvData.GeometryData.GetEnumerator();

            foreach (var xyzDataRow in crossSectionDefinitionXYZ.XYZDataTable.Rows)
            {
                if (profileEnumerator.MoveNext() && xyzDataRow != null)
                {
                    xyzDataRow.DeltaZStorage = profileEnumerator.Current.Storage;
                }
            }
        }
    }
}
