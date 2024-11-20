using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.CrossSections;
using DelftTools.Utils.Csv.Importer;
using log4net;

namespace DeltaShell.Plugins.NetworkEditor.ImportExportCsv
{
    public class CrossSectionZWFromCsvFileImporter : CrossSectionFromCsvFileImporterBase
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(CrossSectionZWFromCsvFileImporter));

        private class CrossSectionZWCsvData : CrossSectionCsvData
        {
            public double WidthMainChannel { get; set; }
            public double WidthFloodplain1 { get; set; }
            public double WidthFloodplain2 { get; set; }
            public double WidthSedimentTransport { get; set; }
            public bool HasSummerdike { get; set; }
            public double SummerdikeCrestLevel { get; set; }
            public double SummerdikeFloodPlainLevel { get; set; }
            public double SummerdikeFloodSurface { get; set; }
            public double SummerdikeTotalSurface { get; set; }
            public bool UseGroundlayer { get; set; }
            public double GroundLayerDepth { get; set; }

            public List<HeightFlowStorageWidth> HeightFlowStorageWidthData;
            public IEnumerable<CrossSectionSection> CrossSectionSections
            {
                get
                {
                    var main = WidthMainChannel;
                    var fp1 = WidthFloodplain1;
                    var fp2 = WidthFloodplain2;
                    double from = 0, to = 0;

                    if (!double.IsNaN(main))
                    {
                        to = main / 2.0;

                        yield return new CrossSectionSection
                            {
                                MinY = from,
                                MaxY = to,
                                SectionType =
                                    new CrossSectionSectionType
                                        {
                                            Name = CrossSectionZWCsvImportExportSettings.MainSectionName
                                        }
                            };
                    }

                    if (!double.IsNaN(fp1))
                    {
                        from = to;
                        to = from + fp1 / 2.0;

                        yield return new CrossSectionSection
                            {
                                MinY = from,
                                MaxY = to,
                                SectionType =
                                    new CrossSectionSectionType
                                        {
                                            Name = CrossSectionZWCsvImportExportSettings.FloodPlain1SectionName
                                        }
                            };
                    }

                    if (!double.IsNaN(fp2))
                    {
                        from = to;
                        to = from + fp2 / 2.0;

                        yield return new CrossSectionSection
                            {
                                MinY = from,
                                MaxY = to,
                                SectionType =
                                    new CrossSectionSectionType
                                        {
                                            Name = CrossSectionZWCsvImportExportSettings.FloodPlain2SectionName
                                        }
                            };
                    }
                }
            }
        }

        public override string Name
        {
            get { return "ZW Cross sections from CSV"; }
        }

        protected override CrossSectionType CrossSectionType
        {
            get { return CrossSectionType.ZW; }
        }

        protected override IDictionary<CsvRequiredField, CsvColumnInfo> CsvFieldToColumnMapping
        {
            get { return CrossSectionZWCsvImportExportSettings.ColumnsMapping; }
        }

        private static double SafelyParseDouble(object o)
        {
            return o is double ? (double) o : 0;
        }

        protected override IList<CrossSectionCsvData> ReadDataTable(DataTable dataTable)
        {
            var crossSectionData = new List<CrossSectionCsvData>();
            var line = 0;
            foreach (DataRow row in dataTable.Rows)
            {
                if (CrossSectionZWCsvImportExportSettings.IsMeta(row))
                {
                    var useGroundLayer =
                        CrossSectionZWCsvImportExportSettings.ParseToBool(row[CrossSectionZWCsvImportExportSettings.UseGroundlayerHeader]);
                    var useSummerDike =
                        CrossSectionZWCsvImportExportSettings.ParseToBool(row[CrossSectionZWCsvImportExportSettings.UseSummerdikeHeader]);
                    var sedimentTransportWidthEntry = row[CrossSectionZWCsvImportExportSettings.SedimentWidthHeader];
                    try
                    {
                        var newData = new CrossSectionZWCsvData
                            {
                                Name = (string) row[CrossSectionZWCsvImportExportSettings.IdHeader],
                                LongName = row[CrossSectionZWCsvImportExportSettings.NameHeader] as string,
                                Branch = row[CrossSectionZWCsvImportExportSettings.BranchHeader] as string,
                                Chainage =
                                    row[CrossSectionZWCsvImportExportSettings.ChainageHeader] is double
                                        ? (double) row[CrossSectionZWCsvImportExportSettings.ChainageHeader]
                                        : double.NaN,
                                CrossSectionType = CrossSectionType.ZW,
                                UseGroundlayer = useGroundLayer,
                                GroundLayerDepth =
                                    useGroundLayer
                                        ? (double) row[CrossSectionZWCsvImportExportSettings.GroundLayerDepthHeader]
                                        : 0,
                                HasSummerdike = useSummerDike,
                                WidthMainChannel =
                                    SafelyParseDouble(row[CrossSectionZWCsvImportExportSettings.MainWidthHeader]),
                                WidthFloodplain1 =
                                    SafelyParseDouble(row[CrossSectionZWCsvImportExportSettings.FloodPlain1Header]),
                                WidthFloodplain2 =
                                    SafelyParseDouble(row[CrossSectionZWCsvImportExportSettings.FloodPlain2Header]),
                                WidthSedimentTransport =
                                    sedimentTransportWidthEntry is double ? (double) sedimentTransportWidthEntry : 0,
                                SummerdikeCrestLevel =
                                    useSummerDike
                                        ? (double) row[CrossSectionZWCsvImportExportSettings.CrestLevelSummerdikeHeader]
                                        : 0,
                                SummerdikeFloodPlainLevel =
                                    useSummerDike
                                        ? (double)
                                          row[CrossSectionZWCsvImportExportSettings.FloodPlainLevelSummerdikeHeader]
                                        : 0,
                                SummerdikeFloodSurface =
                                    useSummerDike
                                        ? (double) row[CrossSectionZWCsvImportExportSettings.FlowAreaSummerdikeHeader]
                                        : 0,
                                SummerdikeTotalSurface =
                                    useSummerDike
                                        ? (double) row[CrossSectionZWCsvImportExportSettings.TotalAreaSummerdikeHeader]
                                        : 0,
                                HeightFlowStorageWidthData = new List<HeightFlowStorageWidth>()
                            };
                        crossSectionData.Add(newData);
                    }
                    catch (Exception e)
                    {
                        log.Warn("Reading data table row failed for line" + line + ", exception thrown: " + e.Message);
                        line++;
                        continue;
                    }  
                }
                else if (CrossSectionZWCsvImportExportSettings.IsGeom(row))
                {
                    if (!crossSectionData.Any())
                    {
                        line++;
                        continue;
                    }
                    var level = (double) row[CrossSectionZWCsvImportExportSettings.LevelHeader];
                    var totalWidth = (double) row[CrossSectionZWCsvImportExportSettings.TotalWidthHeader];
                    var flowWidth = (double) row[CrossSectionZWCsvImportExportSettings.FlowWidthHeader];
                    var lastCrossSectionData = (CrossSectionZWCsvData) crossSectionData.Last();
                    lastCrossSectionData.HeightFlowStorageWidthData.Add(new HeightFlowStorageWidth(level, totalWidth,
                                                                                                   flowWidth));
                }
                else
                {
                    log.Warn("record column Data_Type could not be parsed...skipping row");
                }
                line++;
            }
            return crossSectionData;
        }

        protected override void UpdateCrossSectionDefinition(ICrossSectionDefinition crossSectionDefinition, CrossSectionCsvData crossSectionCsvData, IHydroNetwork target)
        {
            var crossSectionDefinitionZW = (CrossSectionDefinitionZW) crossSectionDefinition;
            var crossSectionZWCvsData = (CrossSectionZWCsvData) crossSectionCsvData;

            FillSummerDike(crossSectionDefinitionZW.SummerDike, crossSectionZWCvsData);

            crossSectionDefinitionZW.ZWDataTable.Set(crossSectionZWCvsData.HeightFlowStorageWidthData);

            SetCrossSectionSections(crossSectionDefinition, crossSectionZWCvsData, target);
        }

        private void SetCrossSectionSections(ICrossSectionDefinition crossSectionDefinition, CrossSectionZWCsvData crossSectionZwCsvData, IHydroNetwork target)
        {
            crossSectionDefinition.Sections.Clear();
            foreach (var crossSectionSection in crossSectionZwCsvData.CrossSectionSections)
            {
                var networkSectionType = target.CrossSectionSectionTypes.FirstOrDefault(s => s.Name == crossSectionSection.SectionType.Name);

                if (networkSectionType == null)
                {
                    log.Warn("skipping import of cross section section type " + crossSectionSection.SectionType.Name +
                             " which was not found in network...skipping import");
                    continue;
                }
                crossSectionDefinition.Sections.Add(new CrossSectionSection
                    {
                        SectionType = networkSectionType,
                        MinY = crossSectionSection.MinY,
                        MaxY = crossSectionSection.MaxY
                    });
            }
        }

        private static void FillSummerDike(SummerDike summerDike, CrossSectionZWCsvData crossSectionCsvData)
        {
            summerDike.Active = crossSectionCsvData.HasSummerdike;
            summerDike.CrestLevel = crossSectionCsvData.SummerdikeCrestLevel;
            summerDike.FloodPlainLevel = crossSectionCsvData.SummerdikeFloodPlainLevel;
            summerDike.FloodSurface = crossSectionCsvData.SummerdikeFloodSurface;
            summerDike.TotalSurface = crossSectionCsvData.SummerdikeTotalSurface;
        }
    }
}
