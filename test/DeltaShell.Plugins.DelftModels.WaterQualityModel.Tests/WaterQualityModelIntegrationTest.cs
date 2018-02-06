using System;
using System.IO;
using System.Linq;
using DelftTools.Shell.Core.Workflow;
using DelftTools.Shell.Core.Workflow.DataItems;
using DelftTools.TestUtils;
using DelftTools.Utils;
using DelftTools.Utils.IO;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.IO;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.Model;
using NetTopologySuite.Extensions.Coverages;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.WaterQualityModel.Tests
{
    [TestFixture]
    [Category(TestCategory.Integration)]
    [Category(TestCategory.DataAccess)]
    public class WaterQualityModelIntegrationTest
    {
        [Test]
        public void ImportSobekHydFileAndRun()
        {
            var dataDir = TestHelper.GetDataDir();
            var hydFile = Path.Combine(dataDir, "IntegrationTests", "Flow1D", "sobek.hyd");

            var model = new WaterQualityModel();

            new HydFileImporter().ImportItem(hydFile, model);

            var subFilePath = Path.Combine(dataDir, "IntegrationTests", "Eutrof_simple.sub");
            new SubFileImporter().Import(model.SubstanceProcessLibrary, subFilePath);

            // Send the model to delwaq
            ActivityRunner.RunActivity(model);

            Assert.IsTrue(model.Status == ActivityStatus.Cleaned);
            Assert.IsTrue(model.OutputSubstancesDataItemSet.DataItems.Any());
            var oxygenDataItem = model.OutputSubstancesDataItemSet.DataItems.FirstOrDefault(d => d.Name.Equals("OXY"));
            Assert.NotNull(oxygenDataItem, "OXY dataitem not found.");
            var oxygen = (UnstructuredGridCellCoverage)oxygenDataItem.Value;
            var firstFeature = oxygen.GetTimeSeries(oxygen.GetCoordinatesForGrid(oxygen.Grid).First());
            Assert.NotNull(firstFeature, "First feature in oxygen data item not found.");
            var firstComponent = firstFeature.Components.FirstOrDefault();
            Assert.NotNull(firstComponent, "first feature component invalid.");
            for (int i = 1; i < firstComponent.Values.Count; i++)
            {
                Assert.IsTrue((double)firstComponent.Values[i] > 0d);
            }
        }

        [Test]
        public void ImportFMHydFileAndRun()
        {
            var dataDir = TestHelper.GetDataDir();
            var hydFile = Path.Combine(dataDir, "IntegrationTests", "FM", "FlowFM.hyd");

            var model = new WaterQualityModel();

            new HydFileImporter().ImportItem(hydFile, model);

            var subFilePath = Path.Combine(dataDir, "IntegrationTests", "coli_04.sub");
            new SubFileImporter().Import(model.SubstanceProcessLibrary, subFilePath);
            
            // Send the model to delwaq
            ActivityRunner.RunActivity(model);

            Assert.IsTrue(model.Status == ActivityStatus.Cleaned);
            var dataItems = model.OutputSubstancesDataItemSet.DataItems.ToList();
            Assert.IsTrue(dataItems.Any());
            var substanceDataItem = dataItems.FirstOrDefault(d => d.Name.Equals("Salinity"));
            Assert.NotNull(substanceDataItem, "Substance data item for Salinity not found.");
            var substance = substanceDataItem.Value as UnstructuredGridCellCoverage;
            Assert.NotNull(substance, "Substance not of type UnstructuredGridCellCoverage.");
            Assert.NotNull(substance.Grid, "Substance.Grid undefined.");
            var coordinate = substance.GetCoordinatesForGrid(substance.Grid).ToList().FirstOrDefault();
            Assert.NotNull(coordinate, "Coordinate not found.");
            var firstFeature = substance.GetTimeSeries(coordinate);
            Assert.NotNull(firstFeature, "First feature in substance data item not found.");
            var firstComponent = firstFeature.Components.FirstOrDefault();
            Assert.AreEqual(181, firstComponent.Values.Count);
        }

        [Test]
        public void ImportUgridHydFileAndRun()
        {
            var dataDir = TestHelper.GetDataDir();
            var hydFile = Path.Combine(dataDir, "IntegrationTests", "UGrid", "f34.hyd");

            var model = new WaterQualityModel();

            new HydFileImporter().ImportItem(hydFile, model);

            var subFilePath = Path.Combine(dataDir, "IntegrationTests", "Eutrof_simple.sub");
            new SubFileImporter().Import(model.SubstanceProcessLibrary, subFilePath);

            // Send the model to delwaq
            ActivityRunner.RunActivity(model);

            Assert.IsTrue(model.Status == ActivityStatus.Cleaned);
            Assert.IsTrue(model.OutputSubstancesDataItemSet.DataItems.Any());
            var oxygenDataItem = model.OutputSubstancesDataItemSet.DataItems.FirstOrDefault(d => d.Name.Equals("OXY"));
            Assert.NotNull(oxygenDataItem, "OXY dataitem not found.");
            var oxygen = (UnstructuredGridCellCoverage)oxygenDataItem.Value;
            var firstFeature = oxygen.GetTimeSeries(oxygen.GetCoordinatesForGrid(oxygen.Grid).First());
            Assert.NotNull(firstFeature, "First feature in oxygen data item not found.");
            var firstComponent = firstFeature.Components.FirstOrDefault();
            Assert.NotNull(firstComponent, "first feature component invalid.");
            for (int i = 1; i < firstComponent.Values.Count; i++)
            {
                Assert.IsTrue((double)firstComponent.Values[i] > 0d);
            }
        }

        [Test]
        [Category(TestCategory.Slow)]
        public void GivenValidWaqModel_WhenRunningWithInvalidData_ThenOutputDataItemsAreRemovedFromModel()
        {
            var testDir = FileUtils.CreateTempDirectory();
            var originalDir = TestHelper.GetTestFilePath("WaterQualityDataFiles");
            FileUtils.CopyAll(new DirectoryInfo(originalDir), new DirectoryInfo(testDir), string.Empty);

            var hydFilePath = Path.Combine(testDir, "flow-model", "westernscheldt01.hyd");
            var subFilePath = Path.Combine(testDir, "waq", "sub-files", "bacteria.sub");
            var boundaryConditionsFilePath = Path.Combine(testDir, "waq", "boundary-conditions", "bacteria.csv");

            Func<IDataItem, bool> isWaqOutputFileDataItem = di => di.Role == DataItemRole.Output &&
                                                                  di.ValueType == typeof(TextDocumentFromFile) &&
                                                                  di.Tag != WaqFileBasedPreProcessor.ListFileTag + "Tag";

            try
            {
                // model setup
                var model = new WaterQualityModel();
                new HydFileImporter().ImportItem(hydFilePath, model);
                new SubFileImporter().Import(model.SubstanceProcessLibrary, subFilePath);
                new DataTableImporter().ImportItem(boundaryConditionsFilePath, model.BoundaryDataManager);
                Assert.IsEmpty(model.DataItems.Where(di => isWaqOutputFileDataItem(di)));

                // Run the model successfully and check that the output data items connected to the .lsp & .mor-files
                // are added to the model.
                ActivityRunner.RunActivity(model);
                Assert.That(model.DataItems.Count(di => isWaqOutputFileDataItem(di)), Is.EqualTo(2));


                // Put incorrect data in the boundary conditions file
                var dataFile = model.BoundaryDataManager.DataTables.FirstOrDefault()?.DataFile;
                Assert.IsNotNull(dataFile);
                dataFile.Content = dataFile.Content.Replace("2014/01/01-00:00:00 0.1", "2014/01/01-00:00:00 wrongValue");

                // Run the model again (which will fail) and check that the output data items connected to the .lsp & .mor-files
                // are removed from the model.
                ActivityRunner.RunActivity(model);
                Assert.IsEmpty(model.DataItems.Where(di => isWaqOutputFileDataItem(di)));
                model.Dispose();
            }
            finally
            {
                FileUtils.DeleteIfExists(testDir); // cleanup of created files
            }
        }
    }
}