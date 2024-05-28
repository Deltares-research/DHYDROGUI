using System;
using System.IO;
using DelftTools.Hydro;
using DelftTools.TestUtils;
using DeltaShell.NGHS.IO.Helpers;
using DeltaShell.Plugins.DelftModels.RainfallRunoff;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Concepts;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Exporters;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Importers;
using NUnit.Framework;

namespace DeltaShell.Plugins.ImportExport.Sobek.Tests
{
    [TestFixture]
    public class SobekRainfallRunoffModelImporterTest
    {
        [TearDown]
        public void TearDown()
        {
            Sobek2ModelImporters.ClearRegisteredImporters();
        }
        
        [Test]
        [Category(TestCategory.Integration)]
        public void GivenRRModelWithCatchmentsLinkedToRunoffBoundary_WhenSavingAndImporting_ThenLinksAreCorrectlyRestored()
        {
            // Given
            RainfallRunoffModel rrModel = CreateRrModelWithCatchmentsAndWWTPLinkedToRunoffBoundary();

            var exporter = new RainfallRunoffModelExporter();
            var importer = new RainfallRunoffModelImporter();
            SetupRrImporter();

            // When
            using (var tempDir = new TemporaryDirectory())
            {
                bool exported = exporter.Export(rrModel, tempDir.Path);
                Assert.That(exported, Is.True);

                string fnmPath = Path.Combine(tempDir.Path, "Sobek_3b.fnm");
                object importedModel = importer.ImportItem(fnmPath);

                // Then
                Assert.That(importedModel, Is.TypeOf<RainfallRunoffModel>());

                var importedRrModel = (RainfallRunoffModel)importedModel;
                Assert.That(importedRrModel.Basin.Links.Count, Is.EqualTo(6)); // 5 catchments and 1 WWTP linked to boundary
            }
        }

        private static RainfallRunoffModel CreateRrModelWithCatchmentsAndWWTPLinkedToRunoffBoundary()
        {
            var rrModel = new RainfallRunoffModel();
            IDrainageBasin basin = rrModel.Basin;

            var runoffBoundary = new RunoffBoundary();
            basin.Boundaries.Add(runoffBoundary);

            AddNewCatchmentLinkedToRunoffBoundary<PavedData>("pavedCatchment", rrModel, runoffBoundary);
            AddNewCatchmentLinkedToRunoffBoundary<UnpavedData>("unpavedCatchment", rrModel, runoffBoundary);
            AddNewCatchmentLinkedToRunoffBoundary<HbvData>("hbvCatchment", rrModel, runoffBoundary);
            AddNewCatchmentLinkedToRunoffBoundary<SacramentoData>("sacramentoCatchment", rrModel, runoffBoundary);
            AddNewCatchmentLinkedToRunoffBoundary<OpenWaterData>("openWaterCatchment", rrModel, runoffBoundary);

            AddNewWWTPLinkedToRunoffBoundary(basin, runoffBoundary);

            return rrModel;
        }

        private static void SetupRrImporter()
        {
            Sobek2ModelImporters.RegisterSobek2Importer(() => new SobekModelToRainfallRunoffModelImporter());
        }

        private static void AddNewCatchmentLinkedToRunoffBoundary<T>(
            string catchmentName,
            RainfallRunoffModel rrModel,
            RunoffBoundary runoffBoundary) where T : CatchmentModelData
        {
            var catchment = new Catchment() { Name = catchmentName };
            var data = (T)Activator.CreateInstance(typeof(T), catchment);

            rrModel.Basin.Catchments.Add(catchment);
            rrModel.ModelData.Add(data);

            catchment.LinkTo(runoffBoundary);
        }

        private static void AddNewWWTPLinkedToRunoffBoundary(IDrainageBasin basin, RunoffBoundary runoffBoundary)
        {
            var wwtp = new WasteWaterTreatmentPlant();
            basin.WasteWaterTreatmentPlants.Add(wwtp);

            wwtp.LinkTo(runoffBoundary);
        }
    }
}