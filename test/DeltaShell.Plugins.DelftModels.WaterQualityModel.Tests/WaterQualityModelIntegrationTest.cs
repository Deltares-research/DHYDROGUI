using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Functions;
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
        public void ImportSobekHydFileAndRun()
        {
            string dataDir = TestHelper.GetTestDataDirectory();
            string hydFile = Path.Combine(dataDir, "ValidWaqModels", "Flow1D", "sobek.hyd");

            using (var model = new WaterQualityModel())
            {
                EditInputFileToCreateBinaryFiles(model);
                new HydFileImporter().ImportItem(hydFile, model);

                string subFilePath = Path.Combine(dataDir, "ValidWaqModels", "Eutrof_simple_sobek.sub");
                new SubFileImporter().Import(model.SubstanceProcessLibrary, subFilePath);

                // Send the model to delwaq
                ActivityRunner.RunActivity(model);

                Assert.IsTrue(model.Status == ActivityStatus.Cleaned);
                Assert.IsTrue(model.OutputSubstancesDataItemSet.DataItems.Any());
                IDataItem oxygenDataItem = model.OutputSubstancesDataItemSet.DataItems.FirstOrDefault(d => d.Name.Equals("OXY"));
                Assert.NotNull(oxygenDataItem, "OXY dataitem not found.");
                var oxygen = (UnstructuredGridCellCoverage) oxygenDataItem.Value;
                IFunction firstFeature = oxygen.GetTimeSeries(oxygen.GetCoordinatesForGrid(oxygen.Grid).First());
                Assert.NotNull(firstFeature, "First feature in oxygen data item not found.");
                IVariable firstComponent = firstFeature.Components.FirstOrDefault();
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

        [Test]
        [Category(TestCategory.Slow)]
        public void Check_RunningWaterQualityModelTwice_ThenSeparateOutputFileContentsReferToTheirOwnRun()
        {
            using (var tempDirectory = new TemporaryDirectory())
            using (WaterQualityModel model = CreateWesternScheldtModel(tempDirectory.Path))
            {
                //First run
                ActivityRunner.RunActivity(model);
                Assert.AreEqual(model.Status, ActivityStatus.Cleaned);

                List<string> contentFirstRun = RetrieveRunContent(model);

                //Second run
                ActivityRunner.RunActivity(model);
                Assert.AreEqual(model.Status, ActivityStatus.Cleaned);

                List<string> contentSecondRun = RetrieveRunContent(model);

                // Assert
                ExecutionStartTimeDifferent(contentFirstRun[0], contentSecondRun[0]);
                ExecutionStartTimeDifferent(contentFirstRun[1], contentSecondRun[1]);
                ExecutionStartTimeDifferent(contentFirstRun[2], contentSecondRun[2]);
            }
        }

        private static void ExecutionStartTimeDifferent(string entryA, string entryB)
        {
            const string lookFor = "Execution start";
            int startIndex = entryA.IndexOf( lookFor, StringComparison.OrdinalIgnoreCase);
            if (startIndex == -1)
            {
                Assert.Fail($"Could not find string '{lookFor}' in content of the file: {entryA}.");
            }
            string content1 = entryA.Substring(startIndex + 17, 19);
            string content2 = entryB.Substring(startIndex + 17, 19);
            
            Assert.AreNotEqual(content1, content2);
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

        private static List<string> RetrieveRunContent(WaterQualityModel model)
        {
            var content = new List<string>
            {
                ((TextDocument) model
                                .DataItems.Single(di => di.Tag == WaterQualityModel.ListFileDataItemMetaData.Tag)
                                .Value).Content,
                ((TextDocument) model
                                .DataItems.Single(di => di.Tag == WaterQualityModel.ProcessFileDataItemMetaData.Tag)
                                .Value).Content,
                ((TextDocument) model
                                .DataItems.Single(di => di.Tag == WaterQualityModel.MonitoringFileDataItemMetaData.Tag)
                                .Value).Content
            };
            return content;
        }

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
    }
}