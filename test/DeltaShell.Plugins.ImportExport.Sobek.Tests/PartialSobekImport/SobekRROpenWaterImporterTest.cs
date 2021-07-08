using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.TestUtils;
using DeltaShell.Plugins.DelftModels.RainfallRunoff;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Concepts;
using DeltaShell.Plugins.ImportExport.Sobek.PartialSobekImporter;
using log4net.Core;
using NUnit.Framework;

namespace DeltaShell.Plugins.ImportExport.Sobek.Tests.PartialSobekImport
{
    [TestFixture]
    public class SobekRROpenWaterImporterTest
    {
        [Test]
        public void Constructor_InitializesInstanceCorrectly()
        {
            // Call
            var importer = new SobekRROpenWaterImporter();

            // Assert
            Assert.That(importer.DisplayName, Is.EqualTo("Rainfall Runoff open water data"));
            Assert.That(importer.Category, Is.EqualTo(SobekImporterCategories.RainfallRunoff));
            Assert.That(importer, Is.AssignableTo<PartialSobekImporterBase>());
        }

        [Test]
        public void Import_SetsCorrectDataOnOpenWaterData()
        {
            using (var temp = new TemporaryDirectory())
            using (var model = new RainfallRunoffModel())
            {
                // Setup
                var openWaterData = new OpenWaterData(new Catchment()) {Name = "some_id"};

                model.ModelData.Add(openWaterData);

                var importer = new SobekRROpenWaterImporter
                {
                    TargetObject = model,
                    PathSobek = Path.Combine(temp.Path, "Sobek_3b.fnm")
                };

                const string fileContent = "OWRR id 'some_id' ar 123.456 ms 'some_meteo_station' owrr";
                temp.CreateFile("OpenWate.3b", fileContent);

                // Call
                importer.Import();

                // Assert
                Assert.That(openWaterData.CalculationArea, Is.EqualTo(123.456));
                Assert.That(openWaterData.MeteoStationName, Is.EqualTo("some_meteo_station"));
            }
        }

        [Test]
        public void Import_IdNotFoundInModelData_LogsError()
        {
            using (var temp = new TemporaryDirectory())
            using (var model = new RainfallRunoffModel())
            {
                // Setup
                var openWaterData = new OpenWaterData(new Catchment()) {Name = "some_id"};

                model.ModelData.Add(openWaterData);

                var importer = new SobekRROpenWaterImporter
                {
                    TargetObject = model,
                    PathSobek = Path.Combine(temp.Path, "Sobek_3b.fnm")
                };
                
                const string fileContent = "OWRR id 'some_other_id' ar 123.456 ms 'some_meteo_station' owrr";
                temp.CreateFile("OpenWate.3b", fileContent);

                // Call
                void Call() => importer.Import();

                // Assert
                IEnumerable<string> errors = TestHelper.GetAllRenderedMessages(Call, Level.Error);
                Assert.That(errors.Single(), Is.EqualTo("No open paved data to be present in model for catchment with id some_other_id"));
                Assert.That(openWaterData.CalculationArea, Is.EqualTo(0));
                Assert.That(openWaterData.MeteoStationName, Is.Empty);
            }
        }
    }
}