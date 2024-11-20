using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Functions;
using DelftTools.Shell.Core;
using DelftTools.Shell.Core.Workflow;
using DelftTools.Shell.Core.Workflow.DataItems;
using DelftTools.TestUtils;
using DelftTools.Utils;
using DelftTools.Utils.IO;
using DeltaShell.NGHS.TestUtils.Builders;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.IO;
using GeoAPI.Geometries;
using NetTopologySuite.Extensions.Coverages;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.WaterQualityModel.Tests
{
    [TestFixture]
    [Category(TestCategory.Integration)]
    public class WaterQualityModelIntegrationTest
    {
        [Test]
        public void ImportFMHydFileAndRun()
        {
            string dataDir = TestHelper.GetTestDataDirectory();
            string hydFile = Path.Combine(dataDir, "ValidWaqModels", "FM", "FlowFM.hyd");

            using (var model = new WaterQualityModel())
            {
                EditInputFileToCreateBinaryFiles(model);
                new HydFileImporter().ImportItem(hydFile, model);

                string subFilePath = Path.Combine(dataDir, "ValidWaqModels", "coli_04.sub");
                new SubFileImporter().Import(model.SubstanceProcessLibrary, subFilePath);

                // Send the model to delwaq
                ActivityRunner.RunActivity(model);

                Assert.IsTrue(model.Status == ActivityStatus.Cleaned);
                List<IDataItem> dataItems = model.OutputSubstancesDataItemSet.DataItems.ToList();
                Assert.IsTrue(dataItems.Any());
                IDataItem substanceDataItem = dataItems.FirstOrDefault(d => d.Name.Equals("Salinity"));
                Assert.NotNull(substanceDataItem, "Substance data item for Salinity not found.");
                var substance = substanceDataItem.Value as UnstructuredGridCellCoverage;
                Assert.NotNull(substance, "Substance not of type UnstructuredGridCellCoverage.");
                Assert.NotNull(substance.Grid, "Substance.Grid undefined.");
                Coordinate coordinate = substance.GetCoordinatesForGrid(substance.Grid).ToList().FirstOrDefault();
                Assert.NotNull(coordinate, "Coordinate not found.");
                IFunction firstFeature = substance.GetTimeSeries(coordinate);
                Assert.NotNull(firstFeature, "First feature in substance data item not found.");
                IVariable firstComponent = firstFeature.Components.FirstOrDefault();
                Assert.AreEqual(181, firstComponent.Values.Count);
            }
        }

        [Test]
        [Category(TestCategory.Slow)]
        public void ImportUgridHydFileAndRun()
        {
            string dataDir = TestHelper.GetTestDataDirectory();
            string hydFile = Path.Combine(dataDir, "ValidWaqModels", "UGrid", "f34.hyd");

            using (var model = new WaterQualityModel())
            {
                new HydFileImporter().ImportItem(hydFile, model);

                string subFilePath = Path.Combine(dataDir, "ValidWaqModels", "coli_04.sub");
                new SubFileImporter().Import(model.SubstanceProcessLibrary, subFilePath);

                // Send the model to delwaq
                ActivityRunner.RunActivity(model);

                Assert.IsTrue(model.Status == ActivityStatus.Cleaned, $"Actual ActivityStatus was: {model.Status}");
                Assert.IsTrue(model.OutputSubstancesDataItemSet.DataItems.Any());
                IDataItem inorganicMatterDataItem = model.OutputSubstancesDataItemSet.DataItems.FirstOrDefault(d => d.Name.Equals("IM1"));
                Assert.NotNull(inorganicMatterDataItem, "IM1 dataitem not found.");
                var inorganicMatter = (UnstructuredGridCellCoverage) inorganicMatterDataItem.Value;
                IFunction firstFeature = inorganicMatter.GetTimeSeries(inorganicMatter.GetCoordinatesForGrid(inorganicMatter.Grid).First());
                Assert.NotNull(firstFeature, "First feature in IM1 data item not found.");
                IVariable firstComponent = firstFeature.Components.FirstOrDefault();
                Assert.NotNull(firstComponent, "first feature component invalid.");
                Assert.True(firstComponent.Values.OfType<Double>().Any());
            }
        }
        
        [Test]
        [Category(TestCategory.Slow)]
        public void GivenValidWaqModel_WhenClearingOutput_ThenOutputDataItemsAndFilesAreNotRemovedFromModel()
        {
            string testDir = FileUtils.CreateTempDirectory();
            string projectFolder = Path.Combine(testDir, "BasicWaqProject.dsproj_data");

            string dataDir = TestHelper.GetTestDataDirectory();
            string hydFilePath = Path.Combine(dataDir, "ValidWaqModels", "UGrid", "f34.hyd");
            string substanceFilePath = Path.Combine(dataDir, "ValidWaqModels", "coli_04.sub");

            string[] outputTextDocumentsTags =
            {
                "ListFileTag",
                "ProcessFileTag",
                "MonitoringFileTag",
                "lastRunLogFileDataItem"
            };
            string[] outputFeatureCoveragesTags =
            {
                "IM1",
                "Salinity",
                "EColi",
                "ExtVL",
                "MrtToEColi"
            };

            try
            {
                using (var app = CreateApplication())
                {
                    app.Run();
                    IProjectService projectService = app.ProjectService;
                    Project project = projectService.CreateProject();
                    projectService.SaveProjectAs(Path.Combine(testDir, "BasicWaqProject.dsproj"));

                    var waqModel = new WaterQualityModel();
                    project.RootFolder.Add(waqModel);
                    projectService.SaveProject();

                    //Import hydroDynamics file
                    HydFileImporter hydFileImporter = app.FileImporters.OfType<HydFileImporter>().FirstOrDefault();
                    Assert.IsNotNull(hydFileImporter);
                    var hydFileImportActivity = new FileImportActivity(hydFileImporter, waqModel)
                    {
                        Files = new[]
                        {
                            hydFilePath
                        }
                    };
                    app.RunActivity(hydFileImportActivity);

                    // Import substance library
                    SubFileImporter subFileImporter = app.FileImporters.OfType<SubFileImporter>().FirstOrDefault();
                    Assert.IsNotNull(subFileImporter);
                    var subFileImportActivity = new FileImportActivity(subFileImporter, waqModel.SubstanceProcessLibrary)
                    {
                        Files = new[]
                        {
                            substanceFilePath
                        }
                    };
                    app.RunActivity(subFileImportActivity);

                    // Check if output file data items are non-existent AND that feature coverage data items
                    // are not connected to data
                    IEnumerable<IDataItem> outputDataItemValues = waqModel.AllDataItems.Where(di => di.Role.HasFlag(DataItemRole.Output));
                    foreach (string tag in outputTextDocumentsTags)
                    {
                        Assert.IsFalse(outputDataItemValues.Any(di => di.Tag == tag));
                    }

                    foreach (string tag in outputFeatureCoveragesTags)
                    {
                        CheckFeatureCoverageFunctionStore(outputDataItemValues, tag, false);
                    }

                    app.RunActivity(waqModel);

                    // Add a custom Run Report data item that represents the Run Report 
                    // that is created normally after a WAQ model run in DeltaShell
                    var runReport = new TextDocument(true)
                    {
                        Name = "Run report",
                        Content = "This is content for a run report."
                    };
                    var runReportDataItem = new DataItem(runReport, DataItemRole.Output, "lastRunLogFileDataItem");
                    waqModel.DataItems.Add(runReportDataItem);

                    // Check that necessary data items exist after model run AND that
                    foreach (string tag in outputTextDocumentsTags)
                    {
                        Assert.That(outputDataItemValues.Count(di => di.Tag == tag), Is.EqualTo(1));
                    }

                    foreach (string tag in outputFeatureCoveragesTags)
                    {
                        CheckFeatureCoverageFunctionStore(outputDataItemValues, tag);
                    }

                    // Check folder structure after model run
                    IEnumerable<string> filesAfterModelRun = GetAllFilesPaths(projectFolder);

                    waqModel.ClearOutput(); // Clearing output

                    IEnumerable<string> filesAfterClearOutput = GetAllFilesPaths(projectFolder);

                    // Check that feature coverage data items are not connected to data
                    foreach (string tag in outputFeatureCoveragesTags)
                    {
                        CheckFeatureCoverageFunctionStore(outputDataItemValues, tag, false);
                    }

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
        
        [Test]
        [Category(TestCategory.Slow)]
        public void ImportCorrectSubFileAndThenCorruptItAndExpectErrorMessageInListFile()
        {
            using (var tempDirectory = new TemporaryDirectory())
            using (WaterQualityModel model = CreateWesternScheldtModel(tempDirectory.Path))
            {
                string boundaryDataTableFilePath = Path.Combine(model.BoundaryDataManager.FolderPath, "bacteria.tbl");
                Assert.True(File.Exists(boundaryDataTableFilePath));

                // Simulate corruption of boundary table data
                using (StreamWriter sw = File.AppendText(boundaryDataTableFilePath))
                {
                    sw.WriteLine("The Corruption");
                    sw.WriteLine("Spreads in this file");
                }

                ActivityRunner.RunActivity(model);
                Assert.AreEqual(model.Status, ActivityStatus.Cleaned);

                string content = RetrieveListFileContents(model);
                Assert.That(content, Is.Not.Empty);
                Assert.That(content, Does.Contain("ERROR: token found on input file: The "));
            }
        }
        
        [Test]
        [Category(TestCategory.DataAccess)]
        [Category(TestCategory.Slow)]
        public void Check_When_RunningTwice_WaqModel_OutputFiles_And_Saving_TheFilesArePersisted()
        {
            using (var tempDirectory = new TemporaryDirectory())
            using (var app = CreateApplication())
            using (WaterQualityModel model = CreateWesternScheldtModel(tempDirectory.Path))
            {
                app.Run();
                IProjectService projectService = app.ProjectService;
                projectService.CreateProject();
                projectService.SaveProjectAs(Path.Combine(tempDirectory.Path, "WAQ_proj.dsproj"));
                
                //First run
                ActivityRunner.RunActivity(model);
                Assert.AreEqual(model.Status, ActivityStatus.Cleaned);

                //save the project
                projectService.SaveProject();

                //Second run
                ActivityRunner.RunActivity(model);
                CheckDataItems(model);
            }
        }

        [Test]
        [Category(TestCategory.Slow)]
        public void Check_RunningWaterQualityModelTwice_OutputFilesAreNotDuplicated()
        {
            using (var tempDirectory = new TemporaryDirectory())
            using (WaterQualityModel model = CreateWesternScheldtModel(tempDirectory.Path))
            {
                //First run
                ActivityRunner.RunActivity(model);
                Assert.AreEqual(model.Status, ActivityStatus.Cleaned);
                CheckDataItems(model);

                //Second run
                ActivityRunner.RunActivity(model);
                Assert.AreEqual(model.Status, ActivityStatus.Cleaned);
                CheckDataItems(model);
            }
        }

        private static void EditInputFileToCreateBinaryFiles(WaterQualityModel model)
        {
            string inputFile = model.InputFile.Content;

            string editedInputFile = inputFile.Replace(
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

        private static WaterQualityModel CreateWesternScheldtModel(string tempDirectory)
        {
            var waqModel = new WaterQualityModel();

            string originalDir = TestHelper.GetTestFilePath("WaterQualityDataFiles");
            FileUtils.CopyAll(new DirectoryInfo(originalDir), new DirectoryInfo(tempDirectory), string.Empty);

            string hydFilePath = Path.Combine(tempDirectory, "flow-model", "westernscheldt01.hyd");
            string subFilePath = Path.Combine(tempDirectory, "waq", "sub-files", "bacteria.sub");
            string boundaryConditionsFilePath = Path.Combine(tempDirectory, "waq", "boundary-conditions", "bacteria.csv");

            new HydFileImporter().ImportItem(hydFilePath, waqModel);
            new SubFileImporter().Import(waqModel.SubstanceProcessLibrary, subFilePath);
            new DataTableImporter().ImportItem(boundaryConditionsFilePath, waqModel.BoundaryDataManager);

            return waqModel;
        }

        private static void CheckDataItems(WaterQualityModel waqModel)
        {
            //Check data items
            IList<string> dataItemTags = GetDataItemTags(waqModel);

            string[] expectedDataItemTags =
            {
                WaterQualityModel.ListFileDataItemMetaData.Tag,
                WaterQualityModel.ProcessFileDataItemMetaData.Tag,
                WaterQualityModel.MonitoringFileDataItemMetaData.Tag
            };

            foreach (string expectedTag in expectedDataItemTags)
            {
                Assert.IsTrue(dataItemTags.Any(t => t == expectedTag),
                              $"DataItem with tag {expectedTag} not found in dataItems {string.Join(", ", dataItemTags)}");
            }
        }

        private static IList<string> GetDataItemTags(WaterQualityModel waqModel)
        {
            List<IDataItem> dataItems = waqModel.DataItems.Where(di => di.Role == DataItemRole.Output
                                                                       && di.ValueType == typeof(TextDocument)).ToList();
            Assert.IsTrue(dataItems.Any());
            Assert.AreEqual(3, dataItems.Count);
            return dataItems.Select(di => di.Tag).ToList();
        }

        private static string RetrieveListFileContents(WaterQualityModel model)
            => RetrieveTextDocumentContents(model, WaterQualityModel.ListFileDataItemMetaData.Tag);
        
        private static string RetrieveTextDocumentContents(WaterQualityModel model, string tag)
            => ((TextDocument)model.DataItems.Single(di => di.Tag == tag).Value).Content;

        private IEnumerable<string> GetAllFilesPaths(string directory)
        {
            return new DirectoryInfo(directory).GetFiles().Select(f => f.FullName);
        }

        private static void CheckFeatureCoverageFunctionStore(IEnumerable<IDataItem> outputDataItemValues, string tag,
                                                              bool connectedToData = true)
        {
            IDataItem[] dataItemValues = outputDataItemValues.ToArray();
            Assert.That(dataItemValues.Count(di => di.Tag == tag), Is.EqualTo(1));
            IDataItem dataItem = dataItemValues.FirstOrDefault(di => di.Tag == tag);
            Assert.IsNotNull(dataItem);
            var outputCoverage = dataItem.Value as UnstructuredGridCellCoverage;
            Assert.IsNotNull(outputCoverage);
            var lazyMapFileFunctionStore = outputCoverage.Store as LazyMapFileFunctionStore;
            Assert.IsNotNull(lazyMapFileFunctionStore);

            if (connectedToData)
            {
                Assert.IsNotNull(lazyMapFileFunctionStore.Path);
            }
            else
            {
                Assert.IsNull(lazyMapFileFunctionStore.Path);
            }
        }
        
        private static IApplication CreateApplication()
        {
            return new DHYDROApplicationBuilder().WithWaterQuality().Build();
        }
    }
}