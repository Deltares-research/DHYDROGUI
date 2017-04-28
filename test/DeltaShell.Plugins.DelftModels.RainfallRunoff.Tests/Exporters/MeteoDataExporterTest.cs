using System.Linq;
using DelftTools.TestUtils;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Meteo;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Exporters;
using NUnit.Framework;
using System;
using System.IO;


namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Tests.Exporters
{
    [TestFixture]
    [Category(TestCategory.DataAccess)]
    public class MeteoDataExporterTest
    {
        private MeteoDataExporter exporter;
        private RainfallRunoffModel rainfallRunoffModel;

        [TestFixtureSetUp]
        public void setup()
        {
            exporter = new MeteoDataExporter();
            rainfallRunoffModel = new RainfallRunoffModel();
        }

        [Test]
        public void ExportEvaporation()
        {
            rainfallRunoffModel.Evaporation.DataDistributionType = MeteoDataDistributionType.Global;
            var data = rainfallRunoffModel.Evaporation.MeteoDataDistributed.Data;
            data.Arguments[0].SetValues(new[] { new DateTime(2014,1,1), new DateTime(2014,1,2), new DateTime(2014,1,3) });
            data.Components[0].SetValues(new[] {1.0, 2.0, 3.0});

            bool exportSuccessful = exporter.Export(rainfallRunoffModel.Evaporation, "test.evp");
            Assert.IsTrue(exportSuccessful);

            var lines = File.ReadLines("test.evp");
            var lines2 = lines.Select(x => x.Trim()).Where(x => ! x.StartsWith("*")).ToArray();
            Assert.AreEqual(3, lines2.Count());
            Assert.That(lines2[0].Contains("2014 01 01 1"));
            Assert.That(lines2[1].Contains("2014 01 02 2"));
            Assert.That(lines2[2].Contains("2014 01 03 3"));
        }

        [Test]
        public void ExportEvaporationMultipleStations()
        {
            rainfallRunoffModel.Evaporation.DataDistributionType = MeteoDataDistributionType.PerStation;
            var data = rainfallRunoffModel.Evaporation.MeteoDataDistributed.Data;
            
            data.Arguments[0].SetValues(new[] { new DateTime(2014, 1, 1), new DateTime(2014, 1, 2), new DateTime(2014, 1, 3) });
            data.Arguments[1].SetValues(new[] {"station1", "station2"}); 
            data.Components[0].SetValues(new[] { 1.0, 2.0, 3.0, 4.0, 5.0, 6.0} );
            
            bool exportSuccessful = exporter.Export(rainfallRunoffModel.Evaporation, "test.evp");
            Assert.IsTrue(exportSuccessful);

            var lines = File.ReadLines("test.evp");
            var lines2 = lines.Select(x => x.Trim()).Where(x => !x.StartsWith("*")).ToArray();
            Assert.AreEqual(3, lines2.Count());
            Assert.That(lines2[0].Contains("2014 01 01 1.00 2.00"));
            Assert.That(lines2[1].Contains("2014 01 02 3.00 4.00"));
            Assert.That(lines2[2].Contains("2014 01 03 5.00 6.00"));
        }

        [Test]
        public void ExportTemperatureData()
        {
            rainfallRunoffModel.Temperature.DataDistributionType = MeteoDataDistributionType.Global;
            var data = rainfallRunoffModel.Temperature.MeteoDataDistributed.Data;
            data.Arguments[0].SetValues(new[] { 
                new DateTime(2014, 1, 1, 0, 0, 0), 
                new DateTime(2014, 1, 1, 2, 10, 0), 
                new DateTime(2014, 1, 1, 4, 20, 0)});
            data.Components[0].SetValues(new[] { 1.0, 2.0, 3.0 });

            bool exportSuccessful = exporter.Export(rainfallRunoffModel.Temperature, "test.tmp");
            Assert.IsTrue(exportSuccessful);

            var lines = File.ReadLines("test.tmp");
            var lines2 = lines.Select(x => x.Trim()).Where(x => !x.StartsWith("*") && x != "").ToArray();
            Assert.AreEqual(5 + 3, lines2.Count());

            Assert.That(lines2[3].Contains("1 7800"));
            Assert.That(lines2[4].Contains("2014 01 01 00 00 00 00 04 20 00"));
            Assert.That(lines2[5].Contains("1.00"));
            Assert.That(lines2[6].Contains("2.00"));
            Assert.That(lines2[7].Contains("3.00"));
        }

        [Test]
        public void ExportPrecipitationData()
        {
            rainfallRunoffModel.Precipitation.DataDistributionType = MeteoDataDistributionType.Global;
            var data = rainfallRunoffModel.Precipitation.MeteoDataDistributed.Data;
            data.Arguments[0].SetValues(new[] { 
                new DateTime(2014, 1, 1, 0, 0, 0), 
                new DateTime(2014, 1, 1, 2, 10, 0), 
                new DateTime(2014, 1, 1, 4, 20, 0)});
            data.Components[0].SetValues(new[] { 1.0, 2.0, 3.0 });

            bool exportSuccessful = exporter.Export(rainfallRunoffModel.Precipitation, "test.bui");
            Assert.IsTrue(exportSuccessful);

            var lines = File.ReadLines("test.bui");
            var lines2 = lines.Select(x => x.Trim()).Where(x => !x.StartsWith("*") && x != "").ToArray();
            Assert.AreEqual(5 + 3, lines2.Count());

            Assert.That(lines2[3].Contains("1 7800"));
            Assert.That(lines2[4].Contains("2014 01 01 00 00 00 00 04 20 00"));
            Assert.That(lines2[5].Contains("1.00"));
            Assert.That(lines2[6].Contains("2.00"));
            Assert.That(lines2[7].Contains("3.00"));
        }
    }
}