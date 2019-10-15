using System;
using System.IO;
using System.Linq;
using DelftTools.Functions;
using DelftTools.Hydro;
using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.Helpers;
using DelftTools.Hydro.Structures;
using DelftTools.Shell.Core.Extensions;
using DelftTools.Shell.Core.Workflow;
using DelftTools.Shell.Core.Workflow.DataItems;
using DelftTools.TestUtils;
using DelftTools.TestUtils.TestReferenceHelper;
using DelftTools.Utils.IO;
using DelftTools.Utils.Reflection;
using DeltaShell.Core;
using DeltaShell.Core.Services;
using DeltaShell.Gui;
using DeltaShell.Plugins.CommonTools;
using DeltaShell.Plugins.Data.NHibernate;
using DeltaShell.Plugins.DelftModels.HydroModel;
using DeltaShell.Plugins.DelftModels.WaterFlowModel;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.DataObjects;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.Gui;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.ModelApiControllers.ModelApi;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.TestUtils;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.Validation;
using DeltaShell.Plugins.ImportExport.Sobek;
using DeltaShell.Plugins.ImportExport.Sobek.Tests;
using DeltaShell.Plugins.NetCDF;
using DeltaShell.Plugins.NetworkEditor;
using DeltaShell.Plugins.NetworkEditor.Gui;
using DeltaShell.Plugins.NetworkEditor.Gui.Forms.NetworkSideView;
using DeltaShell.Plugins.NetworkEditor.Gui.Helpers;
using DeltaShell.Plugins.Scripting;
using DeltaShell.Plugins.SharpMapGis;
using DeltaShell.Plugins.SharpMapGis.Gui;
using DeltaShell.Plugins.SharpMapGis.Gui.Forms;
using GeoAPI.Extensions.Coverages;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using log4net.Core;
using NetTopologySuite.Extensions.Coverages;
using NUnit.Framework;
using SharpMap.Converters.WellKnownText;
using SharpMap.Editors.Interactors.Network;
using SharpTestsEx;
using Point = NetTopologySuite.Geometries.Point;

namespace Sobek.IntegrationTests
{
    [TestFixture]
    public class WaterFlowModel1DIntegrationTest
    {
        [SetUp]
        public void SetUp()
        {
            LogHelper.ResetLogging(); // NOTE: disable logging before commit to avoid huge log files 
            // LogHelper.ConfigureLogging();

            TestHelper.SetDeltaresLicenseToEnvironmentVariable();
        }

        [Test]
        [Category(TestCategory.Jira)] //TOOLS-4407
        [Category(TestCategory.Integration)]
        [Category("DIMR_Introduction")]
        [Category(TestCategory.WorkInProgress)]

        public void ExtractTimeSliceFromOutputCoverageIntoInputCoverage()
        {
            using (var waterFlowModel1D = WaterFlowModel1DDemoModelTestHelper.CreateModelWithDemoNetwork())
            {

                RunModel(waterFlowModel1D, true);

                var numTimeSteps = waterFlowModel1D.OutputDepth.Components[0].Values.Count/
                                   waterFlowModel1D.OutputDepth.Time.Values.Count;

                Assert.AreNotEqual(0, numTimeSteps);
                

                NetworkCoverageHelper.ExtractTimeSlice(waterFlowModel1D.OutputDepth, waterFlowModel1D.InitialConditions,
                                                       waterFlowModel1D.OutputDepth.Time.Values.Last(), true);

                Assert.AreEqual(numTimeSteps, waterFlowModel1D.InitialConditions.Components[0].Values.Count);
            }
        }

        [Test]
        [Category(TestCategory.WindowsForms)]
        [Category(TestCategory.Slow)]
        public void ShowSideViewForProblematicRoute()
        {
            var modelImporter = new SobekWaterFlowModel1DImporter();
            string pathToSobekNetwork =
                TestHelper.GetTestDataDirectoryPathForAssembly(typeof(SobekWaterFlowModel1DImporterTest).Assembly,
                                           @"SW_max_1.lit\3\NETWORK.TP");
            using (var waterFlowModel1D = (WaterFlowModel1D) modelImporter.ImportItem(pathToSobekNetwork))
            {
                //AddCrossSectionToBranch4(importedModel);
                //reduce stoptime to make test faster.
                waterFlowModel1D.StopTime = waterFlowModel1D.StartTime.AddHours(1);
                ModelTestHelper.RefreshCrossSectionDefinitionSectionWidths(waterFlowModel1D.Network);
                var report = waterFlowModel1D.Validate();
                RunModel(waterFlowModel1D);
                Assert.AreEqual(ActivityStatus.Cleaned, waterFlowModel1D.Status);

                var waterLevel = waterFlowModel1D.OutputWaterLevel;
                /*var filteredWaterLevel =
                    (INetworkCoverage)
                    waterLevel.Filter(new VariableValueFilter<DateTime>(waterLevel.Time, waterLevel.Time.Values[0]));*/
                //create a route on the waterlevel
                var startBranch =
                    waterFlowModel1D.Network.Branches.FirstOrDefault(br => br.Name == "Bo_Overijsselsch Kanaal");
                var endBranch = waterFlowModel1D.Network.Branches.FirstOrDefault(br => br.Name == "Bo_1");
                var route = RouteHelper.CreateRoute(new NetworkLocation(startBranch, 763),
                                                    new NetworkLocation(endBranch, 4730));

                var controller = new NetworkSideViewDataController(route,
                                                                   new NetworkSideViewCoverageManager(route, null,
                                                                                                      new[] {waterLevel}));

                var view = new NetworkSideView {Data = route, DataController = controller};
                WindowsFormsTestHelper.ShowModal(view);
            }
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        [Category(TestCategory.Slow)]
        public void ImportModelAssertNetworkEventsDoNoLeak()
        {
            var path =
                TestHelper.GetTestDataDirectoryPathForAssembly(typeof(SobekWaterFlowModel1DImporterTest).Assembly,
                                           @"HolandseNoorderkwartier\AP.lit\9\NETWORK.TP");

            var importer = new SobekHydroModelImporter(false, false);
            var flow = (WaterFlowModel1D) ((HydroModel) importer.ImportItem(path)).Models.First();

            var numSubscriptions = TestReferenceHelper.FindEventSubscriptions(flow.Network);

            Assert.Less(numSubscriptions, 4*40); //was: 4896
        }

        [Test]
        [Category(TestCategory.Performance)]
        [Category(TestCategory.Slow)]
        public void HHNKModelWithoutGuiShouldRunFast()
        {
            LogHelper.ConfigureLogging(Level.Debug);

            var path =
                TestHelper.GetTestDataDirectoryPathForAssembly(typeof(SobekWaterFlowModel1DImporterTest).Assembly,
                                           @"HolandseNoorderkwartier\AP.lit\9\NETWORK.TP");

            var hydroModelImporter = new SobekHydroModelImporter(true, false);

            var hydroModel = (HydroModel) hydroModelImporter.ImportItem(path);
        }

        [Test]
        [Category(TestCategory.Integration)]
        [Category(TestCategory.Performance)]
        [Category(TestCategory.Slow)]
        public void ValidateLargeModelShouldBeFast()
        {
            var modelImporter = new SobekWaterFlowModel1DImporter();

            var pathToSobekNetwork =
                TestHelper.GetTestDataDirectoryPathForAssembly(typeof(SobekWaterFlowModel1DImporterTest).Assembly,
                                           @"FHM2011F.lit\1\network.tp");
            using (var waterFlowModel1D = (WaterFlowModel1D) modelImporter.ImportItem(pathToSobekNetwork))
            {
                var validator = new WaterFlowModel1DModelValidator();
                validator.Validate(waterFlowModel1D); //hit caches

                TestHelper.AssertIsFasterThan(800, () =>
                    {
                        var report1 = validator.Validate(waterFlowModel1D);
                        var report2 = validator.Validate(waterFlowModel1D);
                        Assert.IsTrue(report1.Equals(report2)); //equals performance
                    });
            }
        }

        [Test]
        [Category(TestCategory.Integration)]
        [Category(TestCategory.VerySlow)]
        public void RunImportedAndCopiedModel()
        {
            var modelImporter = new SobekWaterFlowModel1DImporter();
            
            var pathToSobekNetwork =
                TestHelper.GetTestDataDirectoryPathForAssembly(typeof(SobekWaterFlowModel1DImporterTest).Assembly,
                                           @"SW_max_1.lit\3\network.tp");

            using (var waterFlowModel1D = (WaterFlowModel1D) modelImporter.ImportItem(pathToSobekNetwork))
            {
                waterFlowModel1D.StopTime = waterFlowModel1D.StartTime.AddHours(1);

                ModelTestHelper.RefreshCrossSectionDefinitionSectionWidths(waterFlowModel1D.Network);

                // cloned model failed if source contains later sources
                // importedModel.HydroNetwork.Branches.ForEach(b => b.BranchFeatures.RemoveAllWhere(bf => bf is LateralSource));

                var clonedModel = (WaterFlowModel1D) waterFlowModel1D.Clone();
                RunModel(clonedModel);
                Assert.AreEqual(ActivityStatus.Cleaned, clonedModel.Status);
            }
        }

        [Test]
        [Category(TestCategory.Integration)]
        [Category(TestCategory.VerySlow)]
        public void Run105Model()
        {

            var modelImporter = new SobekWaterFlowModel1DImporter();
            
            var pathToSobekNetwork =
                TestHelper.GetTestDataDirectoryPathForAssembly(typeof(SobekWaterFlowModel1DImporterTest).Assembly,
                                           @"105_20m.lit\5\network.tp");
            using (var waterFlowModel1D = (WaterFlowModel1D) modelImporter.ImportItem(pathToSobekNetwork))
            {
                waterFlowModel1D.StopTime = waterFlowModel1D.StartTime.AddHours(1);

                // cloned model failed if source contains lateral sources
                // importedModel.HydroNetwork.Branches.ForEach(b => b.BranchFeatures.RemoveAllWhere(bf => bf is LateralSource));
                RunModel(waterFlowModel1D);
                Assert.AreEqual(ActivityStatus.Cleaned, waterFlowModel1D.Status);
            }
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void RunModelWithAllOutputCoveragesOnTools7529()
        {
            using (var waterFlowModel1D = WaterFlowModel1DDemoModelTestHelper.CreateModelWithDemoNetwork())
            {
                var exceptions = 0;
                AppDomain.CurrentDomain.UnhandledException += (s, e) => exceptions++;

                // enable all output
                foreach (var engineParam in waterFlowModel1D.OutputSettings.EngineParameters)
                {
                    if (engineParam.QuantityType != QuantityType.FiniteGridType)
                        engineParam.AggregationOptions = AggregationOptions.Current;
                }

                waterFlowModel1D.Initialize();
                waterFlowModel1D.Execute();

                Assert.AreEqual(ActivityStatus.Executed, waterFlowModel1D.Status);
                Assert.AreEqual(0, exceptions, "#model exceptions");
                waterFlowModel1D.Cleanup();
            }
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void Test_LegacyLoader350_UpdatesDispersionCoverages() // Issue#: SOBEK3-791
        {
            // Make copy of legacy project
            var legacyProjectDir = Path.Combine(TestHelper.GetTestDataDirectory(), @"Dispersion\LegacyProject");
            const string workingDirectory = "MigrateLegacyProjectWithDispersion";
            var dsProjDir = Path.Combine(workingDirectory, "ThatcherHarlemanEnabled.dsproj");

            FileUtils.DeleteIfExists(workingDirectory);
            FileUtils.CreateDirectoryIfNotExists(workingDirectory);
            FileUtils.CopyDirectory(legacyProjectDir, workingDirectory);
            
            using (var app = new DeltaShellApplication())
            {
                app.Plugins.Add(new NHibernateDaoApplicationPlugin());
                app.Plugins.Add(new CommonToolsApplicationPlugin());
                app.Plugins.Add(new SharpMapGisApplicationPlugin());
                app.Plugins.Add(new NetworkEditorApplicationPlugin());
                app.Plugins.Add(new WaterFlowModel1DApplicationPlugin());
                app.Plugins.Add(new NetCdfApplicationPlugin());
                
                app.Run();

                // open project
                app.OpenProject(dsProjDir);
                var waterFlowModel1D = app.Project.RootFolder.Models.OfType<WaterFlowModel1D>().First();
                
                Assert.IsTrue(waterFlowModel1D.DispersionFormulationType == DispersionFormulationType.Constant);

                // Legacy F1 coverage and component names should be updated
                var f1CoverageDataItem = waterFlowModel1D.DataItems.FirstOrDefault(di => di.Tag == WaterFlowModel1DDataSet.InputDispersionCoverageTag);
                Assert.NotNull(f1CoverageDataItem);

                var f1Coverage = f1CoverageDataItem.Value as INetworkCoverage;
                Assert.NotNull(f1Coverage);

                Assert.AreEqual("Dispersion F1 coefficient", f1Coverage.Name);

                var f1Component = f1Coverage.Components.FirstOrDefault(c => c.Name == "Dispersion F1 coefficient");
                Assert.NotNull(f1Component);
                Assert.AreEqual("Dispersion F1 coefficient", f1Component.Name);
                
                // Legacy F3 component should be removed from F1 coverage
                var f3Component = f1Coverage.Components.FirstOrDefault(c => c.Name == "F3");
                Assert.IsNull(f3Component);

                // F3 and F4 coverages and dataitems should not exist => it should be set to constant
                var f3CoverageDataItem = waterFlowModel1D.DataItems.FirstOrDefault(di => di.Tag == WaterFlowModel1DDataSet.InputDispersionF3CoverageTag);
                Assert.IsNull(f3CoverageDataItem);

                var f4CoverageDataItem = waterFlowModel1D.DataItems.FirstOrDefault(di => di.Tag == WaterFlowModel1DDataSet.InputDispersionF4CoverageTag);
                Assert.IsNull(f4CoverageDataItem);

                app.CloseProject();
            }
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void Test_ToggleSalinityCoverages_AfterProjectSaveAndLoad() // Issue#: SOBEK3-621
        {
            const string workingDirectory = "RunLegacyModelAndSave";
            var dsProjDir = Path.Combine(workingDirectory, "TestModel.dsproj");

            FileUtils.DeleteIfExists(workingDirectory);
            FileUtils.CreateDirectoryIfNotExists(workingDirectory);

            using (var app = new DeltaShellApplication())
            {
                app.Plugins.Add(new NHibernateDaoApplicationPlugin());
                app.Plugins.Add(new CommonToolsApplicationPlugin());
                app.Plugins.Add(new SharpMapGisApplicationPlugin());
                app.Plugins.Add(new NetworkEditorApplicationPlugin());
                app.Plugins.Add(new WaterFlowModel1DApplicationPlugin());
                app.Plugins.Add(new NetCdfApplicationPlugin());

                app.Run();
                app.CreateNewProject();
                app.SaveProjectAs(dsProjDir);

                const double f3Value = 0.5;
                const double f4Value = 0.7;
                
                // Setup WaterFlowModel1D with thatcher harleman enabled and F3 & F4 values
                using (var waterFlowModel1D = new WaterFlowModel1D{ Network = HydroNetworkHelper.GetSnakeHydroNetwork(new Point(0, 0), new Point(100, 0)) })
                {
                    app.Project.RootFolder.Add(waterFlowModel1D);

                    waterFlowModel1D.UseSalt = true;
                    waterFlowModel1D.DispersionFormulationType = DispersionFormulationType.KuijperVanRijnPrismatic;

                    var networkLocation = new NetworkLocation(waterFlowModel1D.Network.Branches[0], 0.0);

                    var dispersionF3CoverageDataItem =
                        waterFlowModel1D.DataItems.FirstOrDefault(di => di.Name == "Dispersion F3 coefficient");
                    Assert.NotNull(dispersionF3CoverageDataItem);

                    var dispersionF3Coverage = dispersionF3CoverageDataItem.Value as INetworkCoverage;
                    Assert.NotNull(dispersionF3Coverage);

                    dispersionF3Coverage.Arguments[0].Values.Clear();
                    dispersionF3Coverage.Arguments[0].Values.Add(networkLocation);
                    dispersionF3Coverage.Components[0].Values.Clear();
                    dispersionF3Coverage.Components[0].Values.Add(f3Value);

                    var dispersionF4CoverageDataItem =
                        waterFlowModel1D.DataItems.FirstOrDefault(di => di.Name == "Dispersion F4 coefficient");
                    Assert.NotNull(dispersionF4CoverageDataItem);

                    var dispersionF4Coverage = dispersionF4CoverageDataItem.Value as INetworkCoverage;
                    Assert.NotNull(dispersionF4Coverage);

                    dispersionF4Coverage.Arguments[0].Values.Clear();
                    dispersionF4Coverage.Arguments[0].Values.Add(networkLocation);
                    dispersionF4Coverage.Components[0].Values.Clear();
                    dispersionF4Coverage.Components[0].Values.Add(f4Value);

                    // Save and close project
                    app.SaveProject();
                    app.CloseProject();
                }

                // Reopen project (now the cached values are null)
                app.OpenProject(dsProjDir);
                
                var reopenedWaterFlowModel1D = app.Project.RootFolder.Models.OfType<WaterFlowModel1D>().First();

                // Toggle ThatcherHarleman (this should cache the existing F3 and F4 values)
                reopenedWaterFlowModel1D.DispersionFormulationType = DispersionFormulationType.Constant;
                reopenedWaterFlowModel1D.DispersionFormulationType = DispersionFormulationType.KuijperVanRijnPrismatic;

                var reopenedNetworkLocation = new NetworkLocation(reopenedWaterFlowModel1D.Network.Branches[0], 0.0);

                // Assert F3 values have been restored from cache
                var restoredDispersionF3CoverageDataItem = reopenedWaterFlowModel1D.DataItems.FirstOrDefault(di => di.Name == "Dispersion F3 coefficient");
                Assert.NotNull(restoredDispersionF3CoverageDataItem);

                var restoredDispersionF3Coverage = restoredDispersionF3CoverageDataItem.Value as INetworkCoverage;
                Assert.NotNull(restoredDispersionF3Coverage);

                Assert.AreEqual(reopenedNetworkLocation, restoredDispersionF3Coverage.Arguments[0].Values[0]);
                Assert.AreEqual(f3Value, (double)restoredDispersionF3Coverage.Components[0].Values[0], 0.0001);

                // Assert F4 values have been restored from cache
                var restoredDispersionF4CoverageDataItem = reopenedWaterFlowModel1D.DataItems.FirstOrDefault(di => di.Name == "Dispersion F4 coefficient");
                Assert.NotNull(restoredDispersionF4CoverageDataItem);

                var restoredDispersionF4Coverage = restoredDispersionF4CoverageDataItem.Value as INetworkCoverage;
                Assert.NotNull(restoredDispersionF4Coverage);

                Assert.AreEqual(reopenedNetworkLocation, restoredDispersionF4Coverage.Arguments[0].Values[0]);
                Assert.AreEqual(f4Value, (double)restoredDispersionF4Coverage.Components[0].Values[0], 0.0001);

                app.CloseProject();
            }
        }

        [Test]
        [Category(TestCategory.Integration)]
        [Category(TestCategory.Slow)]
        public void SobekOutput_RunLegacyModelAndSave_CoveragesUseNewOutputFilesAndArePersisted()
        {
            // Make copy of legacy project
            var legacyProjectDir = Path.Combine(TestHelper.GetTestDataDirectory(), @"SobekOutput\LegacyProject");
            const string workingDirectory = "RunLegacyModelAndSave";
            var dsProjDir = Path.Combine(workingDirectory, "TestModel.dsproj");

            FileUtils.DeleteIfExists(workingDirectory);
            FileUtils.CreateDirectoryIfNotExists(workingDirectory);
            FileUtils.CopyDirectory(legacyProjectDir, workingDirectory);
            
            using (var app = new DeltaShellApplication())
            {
                app.Plugins.Add(new NHibernateDaoApplicationPlugin());
                app.Plugins.Add(new CommonToolsApplicationPlugin());
                app.Plugins.Add(new SharpMapGisApplicationPlugin());
                app.Plugins.Add(new NetworkEditorApplicationPlugin());
                app.Plugins.Add(new WaterFlowModel1DApplicationPlugin());
                app.Plugins.Add(new NetCdfApplicationPlugin());
                
                app.Run();

                // open project
                app.OpenProject(dsProjDir);
                var waterFlowModel1D = app.Project.RootFolder.Models.OfType<WaterFlowModel1D>().First();
                
                // check pre-run conditions
                var dsProjDataDir = new DirectoryInfo(Path.Combine(workingDirectory, "TestModel.dsproj_data"));
                var numOriginalNetFilesBeforeRun = dsProjDataDir.EnumerateFiles("*.nc").Count();
                Assert.AreEqual(4, numOriginalNetFilesBeforeRun);

                var numNewFunctionStoresBeforeRun = waterFlowModel1D.DataItems.Count(
                    di => (di.Role & DataItemRole.Output) == DataItemRole.Output
                    && di.Value is IFunction
                    && ((IFunction)di.Value).Store is WaterFlowModel1DNetCdfFunctionStore);
                Assert.AreEqual(0, numNewFunctionStoresBeforeRun);

                var numOriginalFunctionStoresBeforeRun = waterFlowModel1D.DataItems.Count(
                di => (di.Role & DataItemRole.Output) == DataItemRole.Output
                    && di.Value is IFunction
                    && ((IFunction)di.Value).Store is NetCdfFunctionStore);
                Assert.AreEqual(4, numOriginalFunctionStoresBeforeRun);
                
                // run project
                var exceptions = 0;
                AppDomain.CurrentDomain.UnhandledException += (s, e) => exceptions++;

                waterFlowModel1D.Initialize();
                waterFlowModel1D.Execute();

                Assert.AreEqual(ActivityStatus.Executed, waterFlowModel1D.Status);
                Assert.AreEqual(0, exceptions, "#model exceptions");
                   
                waterFlowModel1D.Finish();
                waterFlowModel1D.Cleanup();
                app.SaveProjectAs(dsProjDir);

                // check new files exist
                var outputDirectory = Path.Combine(waterFlowModel1D.ExplicitWorkingDirectory, waterFlowModel1D.DirectoryName, @"output");
                Assert.IsTrue(File.Exists(Path.Combine(outputDirectory, "gridpoints.nc")));
                Assert.IsTrue(File.Exists(Path.Combine(outputDirectory, "observations.nc")));
                Assert.IsTrue(File.Exists(Path.Combine(outputDirectory, "reachsegments.nc")));
                
                // close project and re-open to check persistance
                app.CloseProject();
                app.OpenProject(dsProjDir);
                waterFlowModel1D = app.Project.RootFolder.Models.OfType<WaterFlowModel1D>().First();

                // check post-run conditions
                var dataItemsWithNewFileBasedStores = waterFlowModel1D.DataItems.Where(
                    di => (di.Role & DataItemRole.Output) == DataItemRole.Output
                          && di.Value is IFunction
                          && ((IFunction) di.Value).Store is WaterFlowModel1DNetCdfFunctionStore);

                var numNewFunctionStoresAfterRun =
                    dataItemsWithNewFileBasedStores.Select(di => ((IFunction)di.Value).Store)
                    .Distinct()
                    .Count();

                Assert.AreEqual(3, numNewFunctionStoresAfterRun);
                
                var numOriginalFunctionStoresAfterRun = waterFlowModel1D.DataItems.Count(
                    di => (di.Role & DataItemRole.Output) == DataItemRole.Output
                    && di.Value is IFunction
                    && ((IFunction)di.Value).Store is NetCdfFunctionStore);
                Assert.AreEqual(0, numOriginalFunctionStoresAfterRun);
                
                app.CloseProject();
            }
        }

        [Test]
        [Category(TestCategory.Integration)]
        [Category(TestCategory.Slow)]
        public void SobekOutput_RunAndSaveLegacyModelTwice_DoesNotThrowException()
        {
            // Make copy of legacy project
            var legacyProjectDir = Path.Combine(TestHelper.GetTestDataDirectory(), @"SobekOutput\LegacyProject");
            const string workingDirectory = "RunLegacyModelAndSave";

            FileUtils.DeleteIfExists(workingDirectory);
            FileUtils.CreateDirectoryIfNotExists(workingDirectory);
            FileUtils.CopyDirectory(legacyProjectDir, workingDirectory);

            using (var app = new DeltaShellApplication())
            {
                app.Plugins.Add(new NHibernateDaoApplicationPlugin());
                app.Plugins.Add(new CommonToolsApplicationPlugin());
                app.Plugins.Add(new SharpMapGisApplicationPlugin());
                app.Plugins.Add(new NetworkEditorApplicationPlugin());
                app.Plugins.Add(new WaterFlowModel1DApplicationPlugin());
                app.Plugins.Add(new NetCdfApplicationPlugin());

                app.Run();

                // open project
                app.OpenProject(Path.Combine(workingDirectory, "TestModel.dsproj"));
                var waterFlowModel1D = app.Project.RootFolder.Models.OfType<WaterFlowModel1D>().First();
                
                // run project
                var exceptions = 0;
                AppDomain.CurrentDomain.UnhandledException += (s, e) => exceptions++;

                waterFlowModel1D.Initialize();
                Assert.AreNotEqual(ActivityStatus.Failed, waterFlowModel1D.Status);
                waterFlowModel1D.Execute();

                Assert.AreEqual(ActivityStatus.Executed, waterFlowModel1D.Status);
                Assert.AreEqual(0, exceptions, "#model exceptions");

                waterFlowModel1D.Finish();
                Assert.AreNotEqual(ActivityStatus.Failed, waterFlowModel1D.Status);
                waterFlowModel1D.Cleanup();
                Assert.AreNotEqual(ActivityStatus.Failed, waterFlowModel1D.Status);
                app.SaveProject();

                // remove branch
                var firstBranch = waterFlowModel1D.Network.Branches.First();
                var startNode = firstBranch.Source;
                var endNode = firstBranch.Target;

                waterFlowModel1D.Network.Branches.Remove(firstBranch);
                waterFlowModel1D.Network.Nodes.Remove(startNode);
                waterFlowModel1D.Network.Nodes.Remove(endNode);

                // run again
                waterFlowModel1D.Initialize();
                waterFlowModel1D.Execute();

                Assert.AreEqual(ActivityStatus.Executed, waterFlowModel1D.Status);
                Assert.AreEqual(0, exceptions, "#model exceptions");

                waterFlowModel1D.Finish();
                waterFlowModel1D.Cleanup();
                app.SaveProject();
                
                app.CloseProject();
            }
        }

        private static void RunModel(WaterFlowModel1D flowModel1D)
        {
            RunModel(flowModel1D, false);
        }

        private static void RunModel(WaterFlowModel1D flowModel1D, bool useNetCdfForStorage)
        {
            if (useNetCdfForStorage)
            {
                //make unique branch ids...
                var id = 0;
                foreach (var b in flowModel1D.Network.Branches)
                {
                    b.Id = id++;
                }
            }

            if (useNetCdfForStorage)
            {
                ModelTestHelper.ReplaceStoreForOutputCoverages(flowModel1D);
            }

            flowModel1D.Initialize();

            Assert.AreEqual(ActivityStatus.Initialized, flowModel1D.Status,
                            "Model should be in initialized state after it is created.");

            while (flowModel1D.Status != ActivityStatus.Done)
            {
                LogHelper.ConfigureLogging();
                flowModel1D.Execute();

                if (flowModel1D.Status == ActivityStatus.Failed)
                {
                    Assert.Fail("Model run has failed");
                }
            }

            flowModel1D.Finish();
            flowModel1D.Cleanup();
        }

        [Test]
        [Category(TestCategory.Jira)] //TOOLS-4343
        [Category(TestCategory.Integration)]
        public void RemoveAllLinksWhenModelHasOutput()
        {
            var modelService = new ModelService(null);

            using (var sourceModel = new WaterFlowModel1D())
            {
                var sourceItem = new DataItem("source", DataItemRole.Input);
                sourceModel.DataItems.Add(sourceItem);

                using (var targetModel = new WaterFlowModel1D())
                {
                    var targetItem = new DataItem("target", DataItemRole.Input);
                    targetModel.DataItems.Add(targetItem);

                    // link "source" -> "target"
                    targetItem.LinkTo(sourceItem);

                    targetModel.DataItems.Add(new DataItem(new NetworkCoverage(), DataItemRole.Output, "Test"));

                    TypeUtils.SetField(targetModel, "outputIsEmpty", false); //hack

                    //remove links for all other models that link to this model.

                    //this triggered an enumeration was edited because output coverages were removed 
                    //from dataitems during unlink + property changed.  
                    targetModel.DisconnectExternalDataItems();

                    Assert.IsNull(targetItem.LinkedTo);
                }
            }
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void MovingANodeShouldNotChangeTheNumberOfSegments()
        {
            using (var waterFlowModel1D = WaterFlowModel1DDemoModelTestHelper.CreateModelWithDemoNetwork())
            {
                var network = waterFlowModel1D.Network;

                var computationalGrid = waterFlowModel1D.NetworkDiscretization;

                //var values = computationalGrid.Locations.Values;

                var nodeEditor = new NodeInteractor(null, network.Nodes[2], null, network);
                nodeEditor.Start();
                nodeEditor.TargetFeature.Geometry = new Point(999.0, 100.0);
                nodeEditor.Stop();

                //Assert.AreEqual(locationsCountBefore, computationalGrid.Locations.Values.Count);
                Assert.AreEqual(25, computationalGrid.Segments.Values.Count);
            }
        }
        
        [Test]
        [Category(TestCategory.Integration)]
        public void ReverseBranchShouldKeepBoundaryData()
        {
            // setup a simple model with 1 dummy branch and 2 sources
            using (var waterFlowModel1D = new WaterFlowModel1D())
            {
                using (var gui = new DeltaShellGui())
                {
                    var app = gui.Application;
                    app.Plugins.Add(new WaterFlowModel1DApplicationPlugin());
                    app.Plugins.Add(new NetworkEditorApplicationPlugin());

                    gui.Plugins.Add(new NetworkEditorGuiPlugin());
                    gui.Plugins.Add(new WaterFlowModel1DGuiPlugin());
                    gui.Plugins.Add(new SharpMapGisGuiPlugin());

                    gui.Run();

                    // create a project containing a single model
                    app.Project.RootFolder.Add(waterFlowModel1D);

                    using (
                        var centralMapView =
                            gui.DocumentViewsResolver.CreateViewForData(waterFlowModel1D,
                                                                        (vi) =>
                                                                        vi.ViewType == typeof (ProjectItemMapView)) as
                            ProjectItemMapView)
                    {
                        var channelLayer = centralMapView.MapView.GetLayerForData(waterFlowModel1D.Network.Channels);
                        Assert.IsNotNull(channelLayer);

                        var networkMapTool =
                            HydroRegionEditorHelper.AddHydroRegionEditorMapTool(centralMapView.MapView.MapControl);

                        channelLayer.DataSource.Add(GeometryFromWKT.Parse("LINESTRING (0 0, 100 0)"));
                        // nodes added by topology rule
                        Assert.AreEqual(2, waterFlowModel1D.Network.Nodes.Count);
                        var channel = waterFlowModel1D.Network.Branches[0];

                        var boundarySource =
                            waterFlowModel1D.BoundaryConditions.FirstOrDefault(bc => bc.Feature == channel.Source);
                        var boundaryTarget =
                            waterFlowModel1D.BoundaryConditions.FirstOrDefault(bc => bc.Feature == channel.Target);

                        boundarySource.DataType = WaterFlowModel1DBoundaryNodeDataType.FlowConstant;
                        boundarySource.Flow = 1.0;
                        boundaryTarget.DataType = WaterFlowModel1DBoundaryNodeDataType.FlowConstant;
                        boundaryTarget.Flow = 2.0;

                        networkMapTool.ReverseBranch((IChannel) waterFlowModel1D.Network.Branches[0]);

                        var newBoundarySource =
                            waterFlowModel1D.BoundaryConditions.FirstOrDefault(bc => bc.Feature == channel.Source);
                        var newBoundaryTarget =
                            waterFlowModel1D.BoundaryConditions.FirstOrDefault(bc => bc.Feature == channel.Target);
                        Assert.AreEqual(newBoundarySource, boundaryTarget);
                        Assert.AreEqual(newBoundaryTarget, boundarySource);

                        Assert.AreEqual(WaterFlowModel1DBoundaryNodeDataType.FlowConstant, newBoundarySource.DataType);
                        Assert.AreEqual(2.0, newBoundarySource.Flow, 1.0e-6);

                        Assert.AreEqual(WaterFlowModel1DBoundaryNodeDataType.FlowConstant, newBoundaryTarget.DataType);
                        Assert.AreEqual(1.0, newBoundaryTarget.Flow, 1.0e-6);
                    }
                }
            }
        }
        [Test]
        [Category(TestCategory.Performance)]
        public void ImportSloterplasSobekIntoNetworkExportToSobekFilebasedWithOutGUI()
        {
            using (var app = new DeltaShellApplication())
            {
                app.Plugins.Add(new WaterFlowModel1DApplicationPlugin());
                app.Plugins.Add(new NetworkEditorApplicationPlugin());

                app.Run();

                var modelPath = TestHelper.GetTestDataDirectoryPathForAssembly(typeof (SobekWaterFlowModel1DImporterTest).Assembly,
                    @"ExpSBI.lit\1\NETWORK.TP");

                var modelImporter = new SobekWaterFlowModel1DImporter {TargetItem = new WaterFlowModel1D()};
                var model = (WaterFlowModel1D) modelImporter.ImportItem(modelPath);
                app.Project.RootFolder.Add(model);
                var network = model.Network;

                var toNetworkImported = new SobekNetworkToNetworkImporter {TargetObject = network};
                toNetworkImported.ImportItem(modelPath);
                
                // add cross sections which were not imported correctly so that our model is valid
                AddNewDefaultCrossSectionZWWithDefaultSectionToBranch(network, "2");
                AddNewDefaultCrossSectionZWWithDefaultSectionToBranch(network, "CH120");
                AddNewDefaultCrossSectionZWWithDefaultSectionToBranch(network, "CH410");
                AddNewDefaultCrossSectionZWWithDefaultSectionToBranch(network, "CH479");

                var validation = model.Validate();
                Assert.AreEqual(0, validation.ErrorCount);

                var exporter = new WaterFlowModel1DExporter();

                string filepath = TestHelper.GetTestDataDirectoryPathForAssembly(typeof (WaterFlowModel1DGuiIntegrationTest).Assembly,
                    "BridgeExport_SOBEK3-54.md1d");
                TestHelper.AssertIsFasterThan(5000, () => exporter.Export(model, filepath));

                Assert.IsTrue(File.Exists(filepath));

            }
        }

        private static void AddNewDefaultCrossSectionZWWithDefaultSectionToBranch(IHydroNetwork network, string channelName)
        {
            var branch = network.Channels.FirstOrDefault(channel => channel.Name == channelName);
            Assert.NotNull(branch);
            var crossSection = CrossSection.CreateDefault(CrossSectionType.ZW, branch, branch.Length / 2);
            crossSection.Definition.AddSection(new CrossSectionSectionType(), 100);
            branch.BranchFeatures.Add(crossSection);
        }

        [Test]
        [Category(TestCategory.Integration)]
        [Ignore("Wait for copy folders of dependecies in ms-buildscript")]
        public void RunModelUsingScript()
        {
            var app = new DeltaShellApplication();
            app.Plugins.Add(new SharpMapGisApplicationPlugin());
            app.Plugins.Add(new ScriptingApplicationPlugin());
            app.Plugins.Add(new WaterFlowModel1DApplicationPlugin());
            app.Run();

            var script = new Script
                {
                    Name = "script1",
                    Code = File.ReadAllText(@"..\..\Scripts\CreateWaterFlowModel1D.py")
                };

            app.Project.RootFolder.Add(script);

            app.ScriptRunner.RunScript(script.Code);

        }

        private WaterFlowModel1D ImportSobekModel_105_010_case7()
        {
            var modelImporter = new SobekWaterFlowModel1DImporter();
            var pathToSobekNetwork =
                TestHelper.GetTestDataDirectoryPathForAssembly(typeof(SobekWaterFlowModel1DImporterTest).Assembly,
                                           @"105_010.lit\7\network.tp");

            return (WaterFlowModel1D) modelImporter.ImportItem(pathToSobekNetwork);
        }

        private WaterFlowModel1D ImportSobekModel_105_010_case9()
        {
            var modelImporter = new SobekWaterFlowModel1DImporter();
            var pathToSobekNetwork =
                TestHelper.GetTestDataDirectoryPathForAssembly(typeof(SobekWaterFlowModel1DImporterTest).Assembly,
                                           @"105_010.lit\9\network.tp");

            return (WaterFlowModel1D) modelImporter.ImportItem(pathToSobekNetwork);
        }

        private HydroModel ImportSobekModel_245_000_case3()
        {
            var modelImporter = new SobekWaterFlowModel1DImporter();
            var pathToSobekNetwork =
                TestHelper.GetTestDataDirectoryPathForAssembly(typeof(SobekWaterFlowModel1DImporterTest).Assembly,
                                           @"245_000.lit\3\network.tp");

            return (HydroModel) modelImporter.ImportItem(pathToSobekNetwork);
        }

        private WaterFlowModel1D ImportSobekModel105_20m()
        {
            var modelImporter = new SobekWaterFlowModel1DImporter();
            var pathToSobekNetwork =
                TestHelper.GetTestDataDirectoryPathForAssembly(typeof(SobekWaterFlowModel1DImporterTest).Assembly,
                                           @"105_20m.lit\5\network.tp");

            return (WaterFlowModel1D) modelImporter.ImportItem(pathToSobekNetwork);
        }

        [Test]
        [Category(TestCategory.Performance)]
        public void ImportSobekModel105Grid20mPerformance()
        {
            ImportSobekModel105_20m(); // avoid reflection overhead

            TestHelper.AssertIsFasterThan(8200, () => ImportSobekModel105_20m());
            //original 105: 4750ms
            //fixes1 105_20m: 210000ms
            //fixes2 105_20m:   4500ms (50x faster)
        }

        [Test]
        [Category(TestCategory.WorkInProgress)]
        [Category(TestCategory.Performance)]
        public void RunSobekModel105_20m_case5_12hours_Performance()
        {
            using (var waterFlowModel1D = ImportSobekModel105_20m())
            {
                waterFlowModel1D.StopTime = waterFlowModel1D.StartTime.AddHours(12);

                //todo: figure out why initialization is taking so bloody long!!

                TestHelper.AssertIsFasterThan(5400, () => RunModel(waterFlowModel1D, true));
                //original: 5400 (for 12 hours)

                Assert.AreEqual(ActivityStatus.Cleaned, waterFlowModel1D.Status);
            }
        }

        [Test]
        [Category(TestCategory.Performance)]
        [Category(TestCategory.Slow)]
        public void RunSobekModel105_010_case9_Performance()
        {
            using (var waterFlowModel1D = ImportSobekModel_105_010_case9())
            {
                TestHelper.AssertIsFasterThan(180000, () => RunModel(waterFlowModel1D, true));

                Assert.AreEqual(ActivityStatus.Cleaned, waterFlowModel1D.Status);
            }
        }

        [Test]
        [Category(TestCategory.Performance)]
        [Category(TestCategory.Slow)]
        public void RunSobekModel_245_000_case3_Performance()
        {
            var hydroModel = ImportSobekModel_245_000_case3();

            var waterFlowModel1D = hydroModel.Models.OfType<WaterFlowModel1D>().First();
            TestHelper.AssertIsFasterThan(140000, () => RunModel(waterFlowModel1D, true));

            Assert.AreEqual(ActivityStatus.Cleaned, waterFlowModel1D.Status);
        }

        [Test]
        [Category(TestCategory.Performance)]
        [Category(TestCategory.WorkInProgress)]
        public void RunSobekModel105_010_case7_Performance()
        {
            //sobek:  98s
            using (var waterFlowModel1D = ImportSobekModel_105_010_case7())
            {
                TestHelper.AssertIsFasterThan(125000, () => RunModel(waterFlowModel1D, true));
                // (Gijs) Running of model way off aimed performance...

                Assert.AreEqual(ActivityStatus.Cleaned, waterFlowModel1D.Status);
            }
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void ImportModel243AndCheckInitialConditionIsLevel()
        {
            var modelImporter = new SobekWaterFlowModel1DImporter();
            
            var pathToSobekNetwork =
                TestHelper.GetTestDataDirectoryPathForAssembly(typeof(SobekWaterFlowModel1DImporterTest).Assembly,
                                           @"243_const.lit\4\network.tp");

            using (var waterFlowModel1D = (WaterFlowModel1D) modelImporter.ImportItem(pathToSobekNetwork))
            {
                Assert.AreEqual(InitialConditionsType.WaterLevel, waterFlowModel1D.InitialConditionsType);
                Assert.AreEqual(0, waterFlowModel1D.InitialConditions.Locations.Values.Count);
                Assert.AreEqual(2.0, waterFlowModel1D.InitialConditions.DefaultValue);
            }
        }

        [Test]
        [Category(TestCategory.Integration)]
        [Category(TestCategory.Jira)]
        [Category(TestCategory.Slow)]
        [Category(TestCategory.WorkInProgress)]
        // fails, see test history: https://build.deltares.nl/project.html?projectId=project4&testNameId=1018460238771856335&tab=testDetails
        public void RoughnessOfCulvertShouldAffectTheResult_Jira_TOOLS_4209()
        {
            var modelImporter = new SobekWaterFlowModel1DImporter();
            
            var pathToSobekNetwork =
                TestHelper.GetTestDataDirectoryPathForAssembly(typeof(SobekWaterFlowModel1DImporterTest).Assembly,
                                           @"243_const.lit\4\network.tp");

            using (var waterFlowModel1D = (WaterFlowModel1D) modelImporter.ImportItem(pathToSobekNetwork))
            {
                var culvert = waterFlowModel1D.Network.Culverts.First();
                culvert.Friction = 45;

                RunModel(waterFlowModel1D);
                Assert.AreEqual(ActivityStatus.Finished, waterFlowModel1D.Status);

                var outputLevel = waterFlowModel1D.OutputWaterLevel.Components[0].GetValues().Clone();

                //set roughness culvert to another value
                culvert.Friction = 60;

                RunModel(waterFlowModel1D);
                Assert.AreEqual(ActivityStatus.Cleaned, waterFlowModel1D.Status);

                var outputLevel2 = waterFlowModel1D.OutputWaterLevel.Components[0].GetValues().Clone();

                Assert.AreNotEqual(outputLevel, outputLevel2);
            }
        }

        [Test]
        [Category(TestCategory.Integration)]
        [Category(TestCategory.Jira)]
        [Category(TestCategory.Slow)]
        public void ChangesInSummerdikeDataShouldChangesResult()
        {
            var modelImporter = new SobekWaterFlowModel1DImporter();
            
            var pathToSobekNetwork =
                TestHelper.GetTestDataDirectoryPathForAssembly(typeof(SobekWaterFlowModel1DImporterTest).Assembly,
                                           @"Summerdike.lit\1\network.tp");

            using (var waterFlowModel1D = (WaterFlowModel1D) modelImporter.ImportItem(pathToSobekNetwork))
            {
                var summerdikeCrossSection =
                    waterFlowModel1D.Network.CrossSections.FirstOrDefault(
                        cs => cs.Definition is CrossSectionDefinitionZW);
                if (summerdikeCrossSection == null)
                {
                    Assert.Fail("No Cross section definition ZW imported.");
                }

                var summerdikeCrossSectionDefinition = (CrossSectionDefinitionZW) (summerdikeCrossSection.Definition);

                summerdikeCrossSectionDefinition.SummerDike.CrestLevel = 3.35;
                summerdikeCrossSectionDefinition.SummerDike.FloodPlainLevel = 2.75;
                summerdikeCrossSectionDefinition.SummerDike.FloodSurface = 100.0;
                summerdikeCrossSectionDefinition.SummerDike.TotalSurface = 220.0;

                RunModel(waterFlowModel1D, true);
                Assert.AreEqual(ActivityStatus.Cleaned, waterFlowModel1D.Status);

                var outputLevel = waterFlowModel1D.OutputWaterLevel.Components[0].GetValues().Clone();

                //set summerdike values another value
                summerdikeCrossSectionDefinition.SummerDike.CrestLevel = 0;
                summerdikeCrossSectionDefinition.SummerDike.FloodPlainLevel = 0;
                summerdikeCrossSectionDefinition.SummerDike.FloodSurface = 0;
                summerdikeCrossSectionDefinition.SummerDike.TotalSurface = 0;

                RunModel(waterFlowModel1D);
                Assert.AreEqual(ActivityStatus.Cleaned, waterFlowModel1D.Status);

                var outputLevel2 = waterFlowModel1D.OutputWaterLevel.Components[0].GetValues().Clone();

                Assert.AreNotEqual(outputLevel, outputLevel2);
            }
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void AddCompoundStructureToFeatureCoverage()
        {
            using (var waterFlowModel1D = WaterFlowModel1DDemoModelTestHelper.CreateModelWithDemoNetwork())
            {
                var network = waterFlowModel1D.Network;

                var branch = network.Branches[0];

                HydroNetworkHelper.AddStructureToExistingCompositeStructureOrToANewOne(
                    new Weir {Name = "weir1", Chainage = branch.Length/2}, branch);
                HydroNetworkHelper.AddStructureToExistingCompositeStructureOrToANewOne(
                    new Weir {Name = "weir2", Chainage = branch.Length/2}, branch);
                HydroNetworkHelper.AddStructureToExistingCompositeStructureOrToANewOne(
                    new Weir {Name = "weir3", Chainage = branch.Length/2}, branch);

                var crestLevelOutput =
                    waterFlowModel1D.OutputSettings.EngineParameters.FirstOrDefault(
                        ep => ep.QuantityType == QuantityType.CrestLevel && ep.ElementSet == ElementSet.Structures);
                Assert.IsNotNull(crestLevelOutput);
                crestLevelOutput.AggregationOptions = AggregationOptions.Current;

                RunModel(waterFlowModel1D, false);
                Assert.AreEqual(ActivityStatus.Cleaned, waterFlowModel1D.Status);
            }
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void CopyPasteModelWithLinkedNetworkShouldNotDamageBoundaryConditionTypes()
        {
            using (var gui = new DeltaShellGui())
            {
                var app = gui.Application;
                app.Plugins.Add(new WaterFlowModel1DApplicationPlugin());
                app.Plugins.Add(new NetworkEditorApplicationPlugin());

                gui.Run();

                var project = gui.Application.Project;

                // link network
                var node1 = new HydroNode {Name = "node1", Geometry = new Point(0, 0)};
                var node2 = new HydroNode {Name = "node2", Geometry = new Point(100, 0)};
                var branch1 = new Channel("branch1", node1, node2)
                    {
                        Geometry = new LineString(new [] {new Coordinate(0, 0), new Coordinate(100, 0)})
                    };
                var network = new HydroNetwork {Nodes = {node1, node2}, Branches = {branch1}};
                var networkDataItem = new DataItem(network);

                // create model, link network and change bc type
                using (var waterFlowModel1D = new WaterFlowModel1D())
                {
                    waterFlowModel1D.GetDataItemByValue(waterFlowModel1D.Network).LinkTo(networkDataItem);

                    // add network and model to project
                    project.RootFolder.Add(waterFlowModel1D);
                    project.RootFolder.Add(networkDataItem);

                    // change 1st bc type to flow time series
                    waterFlowModel1D.BoundaryConditions[0].DataType =
                        WaterFlowModel1DBoundaryNodeDataType.FlowTimeSeries;

                    // add model clone to project
                    var modelClone = (WaterFlowModel1D) waterFlowModel1D.DeepClone();
                    project.RootFolder.Add(modelClone);

                    // asserts
                    modelClone.BoundaryConditions[0].DataType.Should().Be.EqualTo(
                        WaterFlowModel1DBoundaryNodeDataType.FlowTimeSeries);
                }
            }
        }
    }
    
}
