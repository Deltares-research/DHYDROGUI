using System.Collections.Generic;
using System.Data;
using DelftTools.Hydro.CrossSections;

namespace DeltaShell.Plugins.NetworkEditor.ImportExportCsv
{
    public class CrossSectionZWToCsvFileExporter : CrossSectionToCsvExporterBase
    {
        public override string Name
        {
            get { return "ZW Cross sections to CSV"; }
        }

        public override CrossSectionType CrossSectionType
        {
            get { return CrossSectionType.ZW; }
        }

        protected override DataTable CreateDataTable()
        {
            var table = new DataTable();
            foreach (var requiredColumn in CrossSectionZWCsvImportExportSettings.ColumnsMapping.Keys)
            {
                table.Columns.Add(requiredColumn.Name, requiredColumn.ValueType);
            }
            return table;
        }
  
        protected override IEnumerable<object[]> CreateDataRows(ICrossSection crossSection)
        {
            var crossSectionDefinition =
                (CrossSectionDefinitionZW)
                (crossSection.Definition.IsProxy
                     ? ((CrossSectionDefinitionProxy) crossSection.Definition).InnerDefinition
                     : crossSection.Definition);

            var main = crossSectionDefinition.GetSectionWidth(CrossSectionZWCsvImportExportSettings.MainSectionName);
            var fp1 = crossSectionDefinition.GetSectionWidth(CrossSectionZWCsvImportExportSettings.FloodPlain1SectionName);
            var fp2 = crossSectionDefinition.GetSectionWidth(CrossSectionZWCsvImportExportSettings.FloodPlain2SectionName);
            var summerDike = crossSectionDefinition.SummerDike;

            // write the meta record: 
            yield return new object[]
                {
                    crossSection.Name, // Id
                    crossSection.LongName, // Name
                    CrossSectionZWCsvImportExportSettings.ToGeoMetaString(true),
                    null, // Level
                    null, // TotalWidth
                    null, // FlowWidth
                    "ZW",
                    crossSection.Branch.Name, // branch; ignored
                    crossSection.Chainage, // chainage; ignored
                    (object) main,
                    (object) fp1,
                    (object) fp2,
                    null, //crossSection.WidthSedimentTransport,
                    summerDike.Active ? 1 : 0,
                    summerDike.CrestLevel,
                    summerDike.FloodPlainLevel,
                    summerDike.FloodSurface,
                    summerDike.TotalSurface,
                    null //crossSection.UseGroundlayer
                };

            foreach (var heightFlowStorageWidth in crossSectionDefinition.ZWDataTable)
            {
                yield return new object[]
                    {
                        crossSection.Name, // Id
                        crossSection.LongName, // Name
                        CrossSectionZWCsvImportExportSettings.ToGeoMetaString(false),
                        heightFlowStorageWidth.Z,
                        heightFlowStorageWidth.Width,
                        heightFlowStorageWidth.Width - heightFlowStorageWidth.StorageWidth,
                        null, // Type,
                        null, // branch; ignored
                        null, // chainage; ignored
                        null, // crossSection.,
                        null, // crossSection.WidthFloodplain1,
                        null, // crossSection.WidthFloodplain2,
                        null, // crossSection.WidthSedimentTransport,
                        null, // crossSection.HasSummerdike,
                        null, // crossSection.SummerdikeCrestLevel,
                        null, // crossSection.SummerdikeFloodPlainLevel,
                        null, // crossSection.SummerdikeFloodSurface,
                        null, // crossSection.SummerdikeTotalSurface,
                        null // crossSection.UseGroundlayer
                    };
            }
        }
    }
}