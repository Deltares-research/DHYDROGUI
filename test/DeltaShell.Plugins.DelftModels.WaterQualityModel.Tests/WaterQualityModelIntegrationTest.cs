using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Shell.Core.Workflow;
using DelftTools.Shell.Core.Workflow.DataItems;
using DelftTools.TestUtils;
using DelftTools.Utils;
using DelftTools.Utils.IO;
using DeltaShell.Core;
using DeltaShell.Plugins.CommonTools;
using DeltaShell.Plugins.Data.NHibernate;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.IO;
using DeltaShell.Plugins.NetworkEditor;
using DeltaShell.Plugins.SharpMapGis;
using NetTopologySuite.Extensions.Coverages;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.WaterQualityModel.Tests
{
    [TestFixture]
    [Category(TestCategory.Integration)]
    public class WaterQualityModelIntegrationTest
    {
        [Test]
        public void ImportSobekHydFileAndRun()
        {
            var dataDir = TestHelper.GetTestDataDirectory();
            var hydFile = Path.Combine(dataDir, "ValidWaqModels", "Flow1D", "sobek.hyd");

            using (var model = new WaterQualityModel())
            {
                EditInputFileToCreateBinaryFiles(model);
                new HydFileImporter().ImportItem(hydFile, model);

                var subFilePath = Path.Combine(dataDir, "ValidWaqModels", "Eutrof_simple_sobek.sub");
                new SubFileImporter().Import(model.SubstanceProcessLibrary, subFilePath);

                // Send the model to delwaq
                ActivityRunner.RunActivity(model);

                Assert.IsTrue(model.Status == ActivityStatus.Cleaned);
                Assert.IsTrue(model.OutputSubstancesDataItemSet.DataItems.Any());
                var oxygenDataItem = model.OutputSubstancesDataItemSet.DataItems.FirstOrDefault(d => d.Name.Equals("OXY"));
                Assert.NotNull(oxygenDataItem, "OXY dataitem not found.");
                var oxygen = (UnstructuredGridCellCoverage) oxygenDataItem.Value;
                var firstFeature = oxygen.GetTimeSeries(oxygen.GetCoordinatesForGrid(oxygen.Grid).First());
                Assert.NotNull(firstFeature, "First feature in oxygen data item not found.");
                var firstComponent = firstFeature.Components.FirstOrDefault();
                Assert.NotNull(firstComponent, "first feature component invalid.");
                for (var i = 1; i < firstComponent.Values.Count; i++)
                {
                    Assert.IsTrue((double) firstComponent.Values[i] > 0d);
                }
            }
        }

        [Test]
        public void ImportFMHydFileAndRun()
        {
            var dataDir = TestHelper.GetTestDataDirectory();
            var hydFile = Path.Combine(dataDir, "ValidWaqModels", "FM", "FlowFM.hyd");

            using (var model = new WaterQualityModel())
            {
                EditInputFileToCreateBinaryFiles(model);
                new HydFileImporter().ImportItem(hydFile, model);

                var subFilePath = Path.Combine(dataDir, "ValidWaqModels", "coli_04.sub");
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
        }

        private static void EditInputFileToCreateBinaryFiles(WaterQualityModel model)
        {
            var inputFile = model.InputFile.Content;

            var editedInputFile = inputFile.Replace(
                                               "0                                                  ; Switch on binary Map file",
                                               "1                                                  ; Switch on binary Map file")
                                           .Replace(
                                               "0                                                  ; Switch on binary History file",
                                               "1                                                  ; Switch on binary History file")
                                           .Replace(
                                               "1                                                  ; Switch off Nefis History file",
                                               "0                                                  ; Switch off Nefis History file")
                                           .Replace(
                                               "1                                                  ; Switch off Nefis Map file",
                                               "0                                                  ; Switch off Nefis Map file");

            model.InputFile.Content = editedInputFile;
        }

        [Test]
        [Category(TestCategory.Slow)]
        public void ImportUgridHydFileAndRun()
        {
            var dataDir = TestHelper.GetTestDataDirectory();
            var hydFile = Path.Combine(dataDir, "ValidWaqModels", "UGrid", "f34.hyd");

            using (var model = new WaterQualityModel())
            {
                new HydFileImporter().ImportItem(hydFile, model);

                var subFilePath = Path.Combine(dataDir, "ValidWaqModels", "Eutrof_simple_fm.sub");
                new SubFileImporter().Import(model.SubstanceProcessLibrary, subFilePath);

                // Send the model to delwaq
                ActivityRunner.RunActivity(model);

                Assert.IsTrue(model.Status == ActivityStatus.Cleaned);
                Assert.IsTrue(model.OutputSubstancesDataItemSet.DataItems.Any());
                var oxygenDataItem = model.OutputSubstancesDataItemSet.DataItems.FirstOrDefault(d => d.Name.Equals("OXY"));
                Assert.NotNull(oxygenDataItem, "OXY dataitem not found.");
                var oxygen = (UnstructuredGridCellCoverage) oxygenDataItem.Value;
                var firstFeature = oxygen.GetTimeSeries(oxygen.GetCoordinatesForGrid(oxygen.Grid).First());
                Assert.NotNull(firstFeature, "First feature in oxygen data item not found.");
                var firstComponent = firstFeature.Components.FirstOrDefault();
                Assert.NotNull(firstComponent, "first feature component invalid.");
                for (int i = 1; i < firstComponent.Values.Count; i++)
                {
                    Assert.IsTrue((double)firstComponent.Values[i] > 0d);
                }
            }
        }

        [Test]
        [Category(TestCategory.Slow)]
        public void GivenValidWaqModel_WhenClearingOutput_ThenOutputDataItemsAndFilesAreNotRemovedFromModel()
        {
            var testDir = FileUtils.CreateTempDirectory();
            var projectFolder = Path.Combine(testDir, "BasicWaqProject.dsproj_data");

            var dataDir = TestHelper.GetTestDataDirectory();
            var hydFilePath = Path.Combine(dataDir, "ValidWaqModels", "UGrid", "f34.hyd");
            var substanceFilePath = Path.Combine(dataDir, "ValidWaqModels", "coli_04.sub");

            string[] outputTextDocumentsTags = { "ListFileTag", "ProcessFileTag", "MonitoringFileTag", "lastRunLogFileDataItem" };
            string[] outputFeatureCoveragesTags = { "IM1", "Salinity", "EColi", "ExtUv", "MrtToEColi" };

            try
            {
                using (var app = new DeltaShellApplication())
                {
                    app.Plugins.Add(new NHibernateDaoApplicationPlugin());
                    app.Plugins.Add(new CommonToolsApplicationPlugin());
                    app.Plugins.Add(new NetworkEditorApplicationPlugin());
                    app.Plugins.Add(new SharpMapGisApplicationPlugin());
                    app.Plugins.Add(new WaterQualityModelApplicationPlugin());

                    app.Run();
                    app.CreateNewProject();
                    app.SaveProjectAs(Path.Combine(testDir, "BasicWaqProject.dsproj"));

                    var waqModel = new WaterQualityModel();
                    app.Project.RootFolder.Add(waqModel);
                    app.SaveProject();

                    //Import hydroDynamics file
                    var hydFileImporter = app.FileImporters.OfType<HydFileImporter>().FirstOrDefault();
                    Assert.IsNotNull(hydFileImporter);
                    var hydFileImportActivity = new FileImportActivity(hydFileImporter, waqModel) { Files = new[] { hydFilePath } };
                    app.RunActivity(hydFileImportActivity);
                    
                    // Import substance library
                    var subFileImporter = app.FileImporters.OfType<SubFileImporter>().FirstOrDefault();
                    Assert.IsNotNull(subFileImporter);
                    var subFileImportActivity = new FileImportActivity(subFileImporter, waqModel.SubstanceProcessLibrary) { Files = new[] { substanceFilePath } };
                    app.RunActivity(subFileImportActivity);
                    
                    // Check if output file data items are non-existent AND that feature coverage data items
                    // are not connected to data
                    var outputDataItemValues = waqModel.AllDataItems.Where(di => di.Role.HasFlag(DataItemRole.Output));
                    foreach (var tag in outputTextDocumentsTags) Assert.IsFalse(outputDataItemValues.Any(di => di.Tag == tag));
                    foreach (var tag in outputFeatureCoveragesTags) CheckFeatureCoverageFunctionStore(outputDataItemValues, tag, false);

                    app.RunActivity(waqModel);

                    // Add a custom Run Report data item that represents the Run Report 
                    // that is created normally after a WAQ model run in DeltaShell
                    var runReport = new TextDocument(true) { Name = "Run report", Content = "This is content for a run report." };
                    var runReportDataItem = new DataItem(runReport, DataItemRole.Output, "lastRunLogFileDataItem");
                    waqModel.DataItems.Add(runReportDataItem);

                    // Check that necessary data items exist after model run AND that
                    foreach (var tag in outputTextDocumentsTags) Assert.That(outputDataItemValues.Count(di => di.Tag == tag), Is.EqualTo(1));
                    foreach (var tag in outputFeatureCoveragesTags) CheckFeatureCoverageFunctionStore(outputDataItemValues, tag);

                    // Check folder structure after model run
                    IEnumerable<string> filesAfterModelRun = GetAllFilesPaths(projectFolder);

                    waqModel.ClearOutput(); // Clearing output

                    IEnumerable<string> filesAfterClearOutput = GetAllFilesPaths(projectFolder);

                    // Check that feature coverage data items are not connected to data
                    foreach (var tag in outputFeatureCoveragesTags)
                        CheckFeatureCoverageFunctionStore(outputDataItemValues, tag, false);

                    // Check folder structure after model cleanup
                    IEnumerable<string> missingFilesAfterClearOutput = filesAfterModelRun.Except(filesAfterClearOutput);
                    Assert.That(missingFilesAfterClearOutput.Any(), Is.False,
                                "No files should be deleted at a clear output.");
                }
            }
            finally
            {
                FileUtils.DeleteIfExists(testDir);
            }
        }

        private IEnumerable<string> GetAllFilesPaths(string directory)
        {
            return new DirectoryInfo(directory).GetFiles().Select(f => f.FullName);
        }

        private static void CheckFeatureCoverageFunctionStore(IEnumerable<IDataItem> outputDataItemValues, string tag,
            bool connectedToData = true)
        {
            var dataItemValues = outputDataItemValues.ToArray();
            Assert.That(dataItemValues.Count(di => di.Tag == tag), Is.EqualTo(1));
            var dataItem = dataItemValues.FirstOrDefault(di => di.Tag == tag);
            Assert.IsNotNull(dataItem);
            var outputCoverage = dataItem.Value as UnstructuredGridCellCoverage;
            Assert.IsNotNull(outputCoverage);
            var lazyMapFileFunctionStore = outputCoverage.Store as LazyMapFileFunctionStore;
            Assert.IsNotNull(lazyMapFileFunctionStore);

            if (connectedToData) Assert.IsNotNull(lazyMapFileFunctionStore.Path);
            else Assert.IsNull(lazyMapFileFunctionStore.Path);
        }
    }
}