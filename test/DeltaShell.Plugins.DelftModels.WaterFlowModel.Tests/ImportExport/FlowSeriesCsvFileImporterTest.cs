using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using DelftTools.Functions;
using DelftTools.Functions.Generic;
using DelftTools.Hydro;
using DelftTools.Shell.Core.Workflow.DataItems;
using DelftTools.TestUtils;
using DelftTools.Utils.Csv.Importer;
using DeltaShell.NGHS.IO.DataObjects;
using DeltaShell.Plugins.CommonTools.Functions;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport;
using GeoAPI.Extensions.Feature;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.Tests.ImportExport
{
    [TestFixture]
    public class FlowSeriesCsvFileImporterTest
    {
        [Test]
        [Category(TestCategory.DataAccess)]
        [Category(TestCategory.Slow)]
        public void TestImport()
        {
            var filePath = TestHelper.GetTestFilePath("timeseries.csv");
            var importer = new FlowTimeSeriesCsvFileImporter();
            IFeatureData featureData = new Model1DLateralSourceData()
            {
                Data = HydroTimeSeriesFactory.CreateFlowTimeSeries()
            };

            //configure the importer...this is done by a dialog in the application
            var dateTimeInfo = (DateTimeFormatInfo)CultureInfo.InvariantCulture.DateTimeFormat.Clone();
            dateTimeInfo.FullDateTimePattern = "yyyy-MM-dd HH:mm:ss";
             var fieldToColumnMapping = new Dictionary<CsvRequiredField, CsvColumnInfo>
                {
                    {new CsvRequiredField("time", typeof (DateTime)), new CsvColumnInfo(0, dateTimeInfo)},
                    {new CsvRequiredField("value", typeof (double)), new CsvColumnInfo(1, new NumberFormatInfo())}
                };
            importer.FilePath = filePath;
            importer.CsvMappingData = new CsvMappingData
                {
                    FieldToColumnMapping = fieldToColumnMapping,
                    Filters = new List<CsvFilter>(),
                    Settings = new CsvSettings
                        {
                            Delimiter = ',',
                            FirstRowIsHeader = true,
                            SkipEmptyLines = true
                        }
                };
            importer.BoundaryRelationType = BoundaryRelationType.QT;
            importer.CsvImporterMode = CsvImporterMode.OneFunction;

            //action! import
            var importedStuff = importer.ImportItem(null, featureData);
            var series = (TimeSeries)((IDataItem)importedStuff).Value;

            //check first and last time step and number of values
            Assert.AreEqual(15, series.Time.Values.Count);
            Assert.AreEqual(new DateTime(2003, 03, 01, 01, 00, 0), series.Time.Values.First());
            Assert.AreEqual(new DateTime(2003, 03, 01, 04, 30, 0), series.Time.Values.Last());

            //check values
            var values = (IMultiDimensionalArray<double>) series.Components[0].Values;
            Assert.AreEqual(15.0, values.Count);
            Assert.AreEqual(-999.0, values.First());
            Assert.AreEqual(14.0,values.Last());
        }
    }
}
