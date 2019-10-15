using System;
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
    [Category(TestCategory.DataAccess)]
    public class WaterQualityModelIntegrationTest
    {
        [Test]
        public void ImportSobekHydFileAndRun()
        {
            var dataDir = TestHelper.GetTestDataDirectory();
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
            var oxygen = (UnstructuredGridCellCoverage) oxygenDataItem.Value;
            var firstFeature = oxygen.GetTimeSeries(oxygen.GetCoordinatesForGrid(oxygen.Grid).First());
            Assert.NotNull(firstFeature, "First feature in oxygen data item not found.");
            var firstComponent = firstFeature.Components.FirstOrDefault();
            Assert.NotNull(firstComponent, "first feature component invalid.");
            for (int i = 1; i < firstComponent.Values.Count; i++)
            {
                Assert.IsTrue((double) firstComponent.Values[i] > 0d);
            }
        }

        [Test]
        public void ImportFMHydFileAndRun()
        {
            var dataDir = TestHelper.GetTestDataDirectory();
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
            var dataDir = TestHelper.GetTestDataDirectory();
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
            var oxygen = (UnstructuredGridCellCoverage) oxygenDataItem.Value;
            var firstFeature = oxygen.GetTimeSeries(oxygen.GetCoordinatesForGrid(oxygen.Grid).First());
            Assert.NotNull(firstFeature, "First feature in oxygen data item not found.");
            var firstComponent = firstFeature.Components.FirstOrDefault();
            Assert.NotNull(firstComponent, "first feature component invalid.");
            for (int i = 1; i < firstComponent.Values.Count; i++)
            {
                Assert.IsTrue((double) firstComponent.Values[i] > 0d);
            }
        }

        [Test]
        [Category(TestCategory.Slow)]
        public void GivenValidWaqModel_WhenRunningWithInvalidData_ThenOutputDataItemsAreNotRemovedFromModel()
        {
            var testDir = FileUtils.CreateTempDirectory();
            var originalDir = TestHelper.GetTestFilePath("WaterQualityDataFiles");
            FileUtils.CopyAll(new DirectoryInfo(originalDir), new DirectoryInfo(testDir), string.Empty);

            var hydFilePath = Path.Combine(testDir, "flow-model", "westernscheldt01.hyd");
            var subFilePath = Path.Combine(testDir, "waq", "sub-files", "bacteria.sub");
            var boundaryConditionsFilePath = Path.Combine(testDir, "waq", "boundary-conditions", "bacteria.csv");

            Func<IDataItem, bool> isWaqOutputFileDataItem = di => di.Role == DataItemRole.Output &&
                                                                  di.ValueType == typeof(TextDocumentFromFile) &&
                                                                  di.Tag != WaterQualityModel.ListFileDataItemMetaData.Tag;

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
                // are not removed from the model.
                ActivityRunner.RunActivity(model);
                Assert.That(model.DataItems.Count(di => isWaqOutputFileDataItem(di)), Is.EqualTo(2));
                model.Dispose();
            }
            finally
            {
                FileUtils.DeleteIfExists(testDir); // cleanup of created files
            }
        }

        [Test]
        [Category(TestCategory.Slow)]
        public void GivenValidWaqModel_WhenClearingOutput_ThenOutputDataItemsAndFilesAreNotRemovedFromModel()
        {
            var testDir = FileUtils.CreateTempDirectory();
            var projectFolder = Path.Combine(testDir, "BasicWaqProject.dsproj_data");

            var dataDir = TestHelper.GetTestDataDirectory();
            var hydFilePath = Path.Combine(dataDir, "IntegrationTests", "FM", "FlowFM.hyd");
            var substanceFilePath = Path.Combine(dataDir, "IntegrationTests", "coli_04.sub");

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
                    foreach (var tag in outputFeatureCoveragesTags) CheckFeatureCoverageFunctionStore(outputDataItemValues, tag);

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
                    var outputFolderFiles = Directory.GetFileSystemEntries(projectFolder, "*", SearchOption.AllDirectories).Select(path => FileUtils.GetRelativePath(projectFolder, path)).ToArray();
                    Assert.That(outputFolderFiles.Length, Is.EqualTo(relativePathsThatShouldExistAfterWaqModelRun.Length));
                    foreach (var filePath in outputFolderFiles)
                    {
                        Assert.That(relativePathsThatShouldExistAfterWaqModelRun.Contains(filePath),
                            $"File at location '{filePath}' should not exist after Water Quality Model run.");
                    }
                    
                    waqModel.ClearOutput(); // Clearing output

                    // Check if output file data items are still existent AND that feature coverage data items
                    // are not connected to data
                    foreach (var tag in outputTextDocumentsTags)
                        Assert.IsTrue(outputDataItemValues.Any(di => di.Tag == tag));
                    foreach (var tag in outputFeatureCoveragesTags)
                        CheckFeatureCoverageFunctionStore(outputDataItemValues, tag, false);

                    // Check folder structure after model cleanup
                    var outputFolderFilesAfterCleanup = Directory.GetFileSystemEntries(projectFolder, "*", SearchOption.AllDirectories).Select(path => FileUtils.GetRelativePath(projectFolder, path)).ToArray();
                    Assert.That(outputFolderFilesAfterCleanup.Length, Is.EqualTo(relativePathsThatShouldExistAfterWaqModelCleanup.Length));
                    foreach (var filePath in outputFolderFilesAfterCleanup)
                    {
                        Assert.That(relativePathsThatShouldExistAfterWaqModelCleanup.Contains(filePath), 
                            $"File at location '{filePath}' should not exist after Water Quality Model cleanup.");
                    }
                }
            }
            finally
            {
                FileUtils.DeleteIfExists(testDir);
            }
        }

        private readonly string[] relativePathsThatShouldExistAfterWaqModelRun =
        {
            @"Water_Quality",
            @"Water_Quality_output",
            @"Water_Quality\output",
            @"Water_Quality\output\deltashell.lsp",
            @"Water_Quality\output\deltashell.lst",
            @"Water_Quality\output\deltashell.map",
            @"Water_Quality\output\deltashell.mon",
            @"Water_Quality\output\deltashell_res.map",
            @"Water_Quality_output\deltashell-timers.out",
            @"Water_Quality_output\deltashell.inp",
            @"Water_Quality_output\includes_deltashell",
            @"Water_Quality_output\memory_map.out",
            @"Water_Quality_output\includes_deltashell\B1_sublist.inc",
            @"Water_Quality_output\includes_deltashell\B1_t0.inc",
            @"Water_Quality_output\includes_deltashell\B2_numsettings.inc",
            @"Water_Quality_output\includes_deltashell\B2_outlocs.inc",
            @"Water_Quality_output\includes_deltashell\B2_outputtimers.inc",
            @"Water_Quality_output\includes_deltashell\B2_simtimers.inc",
            @"Water_Quality_output\includes_deltashell\B3_attributes.inc",
            @"Water_Quality_output\includes_deltashell\B3_nrofseg.inc",
            @"Water_Quality_output\includes_deltashell\B3_volumes.inc",
            @"Water_Quality_output\includes_deltashell\B4_area.inc",
            @"Water_Quality_output\includes_deltashell\B4_cdispersion.inc",
            @"Water_Quality_output\includes_deltashell\B4_flows.inc",
            @"Water_Quality_output\includes_deltashell\B4_length.inc",
            @"Water_Quality_output\includes_deltashell\B4_nrofexch.inc",
            @"Water_Quality_output\includes_deltashell\B4_pointers.inc",
            @"Water_Quality_output\includes_deltashell\B5_boundaliases.inc",
            @"Water_Quality_output\includes_deltashell\B5_bounddata.inc",
            @"Water_Quality_output\includes_deltashell\B5_boundlist.inc",
            @"Water_Quality_output\includes_deltashell\B6_loads.inc",
            @"Water_Quality_output\includes_deltashell\B6_loads_aliases.inc",
            @"Water_Quality_output\includes_deltashell\B6_loads_data.inc",
            @"Water_Quality_output\includes_deltashell\B7_constants.inc",
            @"Water_Quality_output\includes_deltashell\B7_dispersion.inc",
            @"Water_Quality_output\includes_deltashell\B7_functions.inc",
            @"Water_Quality_output\includes_deltashell\B7_numerical_options.inc",
            @"Water_Quality_output\includes_deltashell\B7_parameters.inc",
            @"Water_Quality_output\includes_deltashell\B7_processes.inc",
            @"Water_Quality_output\includes_deltashell\B7_segfunctions.inc",
            @"Water_Quality_output\includes_deltashell\B7_vdiffusion.inc",
            @"Water_Quality_output\includes_deltashell\B8_initials.inc",
            @"Water_Quality_output\includes_deltashell\B9_Hisvar.inc",
            @"Water_Quality_output\includes_deltashell\B9_Mapvar.inc"
        };

        private readonly string[] relativePathsThatShouldExistAfterWaqModelCleanup =
        {
            @"Water_Quality",
            @"Water_Quality_output",
            @"Water_Quality\output",
            @"Water_Quality_output\deltashell.inp",
            @"Water_Quality_output\includes_deltashell",
            @"Water_Quality_output\includes_deltashell\B1_sublist.inc",
            @"Water_Quality_output\includes_deltashell\B1_t0.inc",
            @"Water_Quality_output\includes_deltashell\B2_numsettings.inc",
            @"Water_Quality_output\includes_deltashell\B2_outlocs.inc",
            @"Water_Quality_output\includes_deltashell\B2_outputtimers.inc",
            @"Water_Quality_output\includes_deltashell\B2_simtimers.inc",
            @"Water_Quality_output\includes_deltashell\B3_attributes.inc",
            @"Water_Quality_output\includes_deltashell\B3_nrofseg.inc",
            @"Water_Quality_output\includes_deltashell\B3_volumes.inc",
            @"Water_Quality_output\includes_deltashell\B4_area.inc",
            @"Water_Quality_output\includes_deltashell\B4_cdispersion.inc",
            @"Water_Quality_output\includes_deltashell\B4_flows.inc",
            @"Water_Quality_output\includes_deltashell\B4_length.inc",
            @"Water_Quality_output\includes_deltashell\B4_nrofexch.inc",
            @"Water_Quality_output\includes_deltashell\B4_pointers.inc",
            @"Water_Quality_output\includes_deltashell\B5_boundaliases.inc",
            @"Water_Quality_output\includes_deltashell\B5_bounddata.inc",
            @"Water_Quality_output\includes_deltashell\B5_boundlist.inc",
            @"Water_Quality_output\includes_deltashell\B6_loads.inc",
            @"Water_Quality_output\includes_deltashell\B6_loads_aliases.inc",
            @"Water_Quality_output\includes_deltashell\B6_loads_data.inc",
            @"Water_Quality_output\includes_deltashell\B7_constants.inc",
            @"Water_Quality_output\includes_deltashell\B7_dispersion.inc",
            @"Water_Quality_output\includes_deltashell\B7_functions.inc",
            @"Water_Quality_output\includes_deltashell\B7_numerical_options.inc",
            @"Water_Quality_output\includes_deltashell\B7_parameters.inc",
            @"Water_Quality_output\includes_deltashell\B7_processes.inc",
            @"Water_Quality_output\includes_deltashell\B7_segfunctions.inc",
            @"Water_Quality_output\includes_deltashell\B7_vdiffusion.inc",
            @"Water_Quality_output\includes_deltashell\B8_initials.inc",
            @"Water_Quality_output\includes_deltashell\B9_Hisvar.inc",
            @"Water_Quality_output\includes_deltashell\B9_Mapvar.inc"
        };

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