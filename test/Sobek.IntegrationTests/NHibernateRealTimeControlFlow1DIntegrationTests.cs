using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.Helpers;
using DelftTools.Hydro.Structures;
using DelftTools.Shell.Core;
using DelftTools.Shell.Core.Workflow;
using DelftTools.Shell.Core.Workflow.DataItems;
using DelftTools.TestUtils;
using DelftTools.TestUtils.TestReferenceHelper;
using DelftTools.Utils;
using DelftTools.Utils.Collections;
using DelftTools.Utils.IO;
using DelftTools.Utils.Reflection;
using DeltaShell.Gui;
using DeltaShell.IntegrationTestUtils;
using DeltaShell.Plugins.CommonTools;
using DeltaShell.Plugins.Data.NHibernate;
using DeltaShell.Plugins.DelftModels.HydroModel;
using DeltaShell.Plugins.DelftModels.HydroModel.Export;
using DeltaShell.Plugins.DelftModels.RainfallRunoff;
using DeltaShell.Plugins.DelftModels.RealTimeControl;
using DeltaShell.Plugins.DelftModels.WaterFlowModel;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.DataObjects;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.TestUtils;
using DeltaShell.Plugins.DelftModels.WaterQualityModel;
using DeltaShell.Plugins.ImportExport.Sobek;
using DeltaShell.Plugins.ImportExport.Sobek.Tests;
using DeltaShell.Plugins.NetCDF;
using DeltaShell.Plugins.NetworkEditor;
using DeltaShell.Plugins.ProjectExplorer;
using DeltaShell.Plugins.SharpMapGis;
using DeltaShell.Plugins.SharpMapGis.Gui;
using NetTopologySuite.Extensions.Coverages;
using NetTopologySuite.Geometries;
using NUnit.Framework;
using ElementSet = DeltaShell.Plugins.DelftModels.WaterFlowModel.ModelApiControllers.ModelApi.ElementSet;
using QuantityType = DeltaShell.Plugins.DelftModels.WaterFlowModel.ModelApiControllers.ModelApi.QuantityType;

namespace Sobek.IntegrationTests
{
    [TestFixture]
    public class NHibernateRealTimeControlFlow1DIntegrationTests : NHibernateIntegrationTestBase
    {
        [TestFixtureSetUp]
        public override void TestFixtureSetUp()
        {
            base.TestFixtureSetUp();
            factory.AddPlugin(new WaterFlowModel1DApplicationPlugin());
            factory.AddPlugin(new NetworkEditorApplicationPlugin());
            factory.AddPlugin(new RealTimeControlApplicationPlugin());
            factory.AddPlugin(new HydroModelApplicationPlugin());
            factory.AddPlugin(new RainfallRunoffApplicationPlugin());
            factory.AddPlugin(new CommonToolsApplicationPlugin());
            factory.AddPlugin(new SharpMapGisApplicationPlugin());
            factory.AddPlugin(new NetCdfApplicationPlugin());
            factory.AddPlugin(new SharpMapGisGuiPlugin());
            InitializeSobekLicense();
        }

        private void InitializeSobekLicense()
        {
            TestHelper.SetDeltaresLicenseToEnvironmentVariable();
        }

        [Test]
        public void SimuluteBackwardCompatibilityFlowModelMissingDataItem()
        {
            var flowModel = new WaterFlowModel1D();

            // remove data item (as in legacy model)
            flowModel.DataItems.Remove(flowModel.GetDataItemByValue(flowModel.Inflows));

            var clone = (WaterFlowModel1D) flowModel.DeepClone();

            //stack overflows if events go wrong

            Assert.IsNotNull(clone.Inflows);
        }

        [Test]
        [Category(TestCategory.Integration)]
        [Category(TestCategory.BackwardCompatibility)]
        [Category(TestCategory.VerySlow)]
        public void OpenRolfCorruptRoughnessCoverageProjectAndVerifyItsFixedTools8916()
        {
            using (var projectRepository = factory.CreateNew())
            {
                var legacyPath = TestHelper.GetTestFilePath(@"j95_20952_v012_newrgh.dsproj");
                var localLegacyPath = TestHelper.CopyProjectToLocalDirectory(legacyPath);
                var project = projectRepository.Open(localLegacyPath);
                var hydroModel = (HydroModel) project.RootFolder.Models.First();
                var flow = hydroModel.Activities.OfType<WaterFlowModel1D>().First();

                foreach (var section in flow.RoughnessSections)
                {
                    foreach (var networkLocation in section.RoughnessNetworkCoverage.Locations.Values)
                    {
                        if (networkLocation.Chainage - networkLocation.Branch.Length > 1.0e-7)
                        {
                            Assert.Fail("Network location with chainage {0} is not on branch {1} (with length {2})",
                                        networkLocation.Chainage, networkLocation.Branch.Name,
                                        networkLocation.Branch.Length);
                        }
                    }
                }
            }
        }

        [Test]
        [Category(TestCategory.Slow)]
        [Category(TestCategory.Jira)] //TOOLS-9483
        [Category(TestCategory.Integration)]
        [Category(TestCategory.BackwardCompatibility)]
        public void OpenLegacy300ProjectAndCheckNetCdfData()
        {
            var projectRepository = factory.CreateNew();
            var legacyPath = TestHelper.GetTestFilePath(@"RTMJZ_Import.dsproj");
            var localLegacyPath = TestHelper.CopyProjectToLocalDirectory(legacyPath);
            var project = projectRepository.Open(localLegacyPath);

            var hydroModel = (HydroModel) project.RootFolder.Models.First();
            var flow = hydroModel.Activities.OfType<WaterFlowModel1D>().First();
            Assert.AreEqual(26, flow.OutputWaterLevel.Time.Values.Count); //the netcdf contains 26 timesteps. If the upgrade fails, it returns 0.
            Assert.AreEqual(700, flow.OutputWaterLevel.Locations.Values.Count); //700 locations
        }

        [Test]
        [Category(TestCategory.Integration)]
        [Category(TestCategory.BackwardCompatibility)]
        [Category(TestCategory.VerySlow)]
        public void Open300CrebasRijnTakkenProjectAndRun()
        {
            var projectRepository = factory.CreateNew();
            var legacyPath = TestHelper.GetTestFilePath(@"Crebas3.0RijnTakken\R-95-5-01.dsproj");
            var localLegacyPath = TestHelper.CopyProjectToLocalDirectory(legacyPath);
            var project = projectRepository.Open(localLegacyPath);

            Assert.IsTrue(project.IsTemporary); //make sure it's temporary (and this will prompt user to re-save)

            var dataItems = project.GetAllItemsRecursive().OfType<IDataItem>().ToList();
            Assert.GreaterOrEqual(dataItems.Count, 101);

            var hydroModel = (HydroModel)project.RootFolder.Models.First();
            hydroModel.ExplicitWorkingDirectory = Path.GetFullPath(Path.Combine(".", TestHelper.GetCurrentMethodName()));
            
            Assert.AreEqual(2, hydroModel.Activities.Count);
            Assert.AreEqual(1, hydroModel.Region.SubRegions.Count);
            var rtc = hydroModel.Activities.OfType<RealTimeControlModel>().First();
            var flow = hydroModel.Activities.OfType<WaterFlowModel1D>().First();
            var zwDef = (CrossSectionDefinitionZW)flow.Network.CrossSections.First().Definition;

            Assert.AreEqual(new DateTime(1994, 12, 01), flow.StartTime, "flow starttime");
            Assert.AreEqual(new DateTime(1994, 12, 01), hydroModel.StartTime, "hydromodel starttime");
            
            Assert.IsNotNull(zwDef.SummerDike);
            Assert.IsTrue(flow.LateralSourceData.Any(lsd => lsd.DataType != WaterFlowModel1DLateralDataType.FlowTimeSeries));
            Assert.AreEqual(5, rtc.ControlGroups.Count, "#control groups");
            Assert.IsNotNull(rtc.ControlGroups[0].Inputs[0].Feature, "linking not correct1");

            var interpolationParameter = flow.ParameterSettings.FirstOrDefault(ps => ps.Name == "InterpolationType");
            Assert.IsNotNull(interpolationParameter, "interpolation engine parameter missing");

            Assert.IsTrue(flow.OutputSettings.GetEngineParameter(QuantityType.WaterLevel, ElementSet.Laterals) != null);

            hydroModel.Initialize();
            Assert.AreEqual(ActivityStatus.Initialized, hydroModel.Status);
            Assert.AreEqual(ActivityStatus.Initialized, flow.Status);
            Assert.AreEqual(ActivityStatus.Initialized, rtc.Status);

            hydroModel.Execute();

            Assert.AreEqual(rtc.RunsInIntegratedModel ? 9.365 : 7.90, rtc.ControlGroups[0].Inputs[0].Value, 0.01);

            Assert.AreEqual(ActivityStatus.Executed, rtc.Status);
            Assert.AreEqual(ActivityStatus.Executed, flow.Status);

            hydroModel.Finish();

            try
            {
                // TODO : Fix cf dll => Finalize
                hydroModel.Cleanup();   
            }
            catch{}

            LogHelper.ResetLogging();

            projectRepository.Close();
        }

        [Test]
        [Category(TestCategory.Integration)]
        [Category(TestCategory.BackwardCompatibility)]
        [Category(TestCategory.VerySlow)]
        public void OpenAnkeBrPkProjectAndRun()
        {
            using (var projectRepository = factory.CreateNew())
            {
                var legacyPath = TestHelper.GetTestFilePath(@"AnkeBrPk\R-95-5-010f-BrPkNL_midden.dsproj");
                var localLegacyPath = TestHelper.CopyProjectToLocalDirectory(legacyPath);
                var project = projectRepository.Open(localLegacyPath);

                var hydroModel = (HydroModel) project.RootFolder.Models.First();
                hydroModel.ExplicitWorkingDirectory = Path.GetFullPath(Path.Combine(".", TestHelper.GetCurrentMethodName()));

                var flow = hydroModel.Activities.OfType<WaterFlowModel1D>().First();

                Assert.AreEqual(10, flow.AllDataItems.Count(di => di.ValueConverter is WaterFlowModelBranchFeatureValueConverter));

                try
                {
                    hydroModel.Initialize();
                    Assert.AreEqual(ActivityStatus.Initialized, flow.Status);

                    hydroModel.Execute();
                    Assert.AreEqual(ActivityStatus.Executed, flow.Status);
                }
                finally
                {
                    hydroModel.Cleanup();
                    LogHelper.ResetLogging();
                }
            }
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        [Category(TestCategory.Slow)]
        public void ViewContextBackwardCompatibilityWithSaveAndUnsupportedViewContext()
        {
            using (var gui = new DeltaShellGui())
            {
                var app = gui.Application;
                app.Plugins.Add(new SharpMapGisApplicationPlugin());
                app.Plugins.Add(new CommonToolsApplicationPlugin());
                app.Plugins.Add(new NetworkEditorApplicationPlugin());
                app.Plugins.Add(new HydroModelApplicationPlugin());
                app.Plugins.Add(new WaterFlowModel1DApplicationPlugin());
                app.Plugins.Add(new RealTimeControlApplicationPlugin());
                app.Plugins.Add(new WaterQualityModelApplicationPlugin());
                app.Plugins.Add(new RainfallRunoffApplicationPlugin());
                app.Plugins.Add(new NHibernateDaoApplicationPlugin());
                app.Plugins.Add(new NetCdfApplicationPlugin());

                gui.Plugins.Add(new ProjectExplorerGuiPlugin());
                gui.Run();

                var path = TestHelper.CopyProjectToLocalDirectory(TestHelper.GetTestFilePath(@"SmallRiver\SmallRiver.dsproj"));

                gui.Application.OpenProject(path);
                gui.Application.SaveProject();
            }
        }

        [Test]
        [Category(TestCategory.Integration)]
        [Category(TestCategory.BackwardCompatibility)]
        [Category(TestCategory.VerySlow)]
        [Category(TestCategory.WorkInProgress)]
        public void Open300CrebasRijnTakkenProjectAndAutoClone()
        {
            var projectRepository = factory.CreateNew();
            var legacyPath = TestHelper.GetTestFilePath(@"Crebas3.0RijnTakken\R-95-5-01.dsproj");
            var localLegacyPath = TestHelper.CopyProjectToLocalDirectory(legacyPath);
            var project = projectRepository.Open(localLegacyPath);
            
            var clonedProject = TypeUtils.DeepClone(project);
            
            // close old project.. a bit hackish this..yes
            projectRepository.Close();
            project.GetAllItemsRecursive().OfType<ITransactionalChangeAccess>().ForEach(ta => ta.CommitChanges());
            project.GetAllItemsRecursive().OfType<IFileBased>().ForEach(fb => fb.Close());
            
            // get cloned models
            var hydroModel = (HydroModel)clonedProject.RootFolder.Models.First();
            var rtc = hydroModel.Models.OfType<RealTimeControlModel>().First();
            var flow = hydroModel.Models.OfType<WaterFlowModel1D>().First();

            var origHydroModel = (HydroModel)project.RootFolder.Models.First();
            var origFlow = origHydroModel.Models.OfType<WaterFlowModel1D>().First();
            var origNetwork = origFlow.Network;

            var graph = TestReferenceHelper.BuildReferenceTree(hydroModel);

            var hits = TestReferenceHelper.SearchObjectInObjectGraph(origNetwork, graph);
            hits.ForEach(Console.WriteLine);
            Assert.AreEqual(0, hits.Count);

            var origItems = origHydroModel.GetAllItemsRecursive().ToList();
            var clonedItems = hydroModel.GetAllItemsRecursive().ToList();
            var diffs = clonedItems.Select(o => new Item { Name = o.ToString(), Value = o })
                                  .Except(origItems.Select(o => new Item { Name = o.ToString(), Value = o }), new ItemComparer());
            
            foreach (var diff in diffs)
            {
                Console.WriteLine("Diff: " + diff);
                var where = TestReferenceHelper.SearchObjectInObjectGraph(diff.Value, graph);
                where.ForEach(Console.WriteLine);
            }

            // we expect some difference (backward compatibility missing items are added by clone now), but how much?
            //Assert.AreEqual(origItems.Count, clonedItems.Count(), "getallitemsrecursive");

            hydroModel.Initialize();
            Assert.AreEqual(ActivityStatus.Initialized, rtc.Status);
            Assert.AreEqual(7.90, rtc.ControlGroups[0].Inputs[0].Value, 0.01);

            hydroModel.Execute();

            Assert.AreEqual(ActivityStatus.Executed, rtc.Status);
            Assert.AreEqual(ActivityStatus.Executed, flow.Status);

            hydroModel.Cleanup();

            LogHelper.ResetLogging();

            projectRepository.Close();
        }

        private class Item
        {
            private string name;
            public string Name
            {
                get { return name; }
                set { name = value ?? ""; }
            }

            public object Value;

            public override string ToString()
            {
                return Name;
            }
        }

        private class ItemComparer : IEqualityComparer<Item>
        {
            public bool Equals(Item x, Item y)
            {
                return x.Name == y.Name;
            }

            public int GetHashCode(Item obj)
            {
                return obj.Name.GetHashCode();
            }
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        [Category(TestCategory.Slow)]
        public void SaveAndRetrieveControlledModel()
        {
            var rtcModel = new RealTimeControlModel();
            var controlledModel = WaterFlowModel1DDemoModelTestHelper.CreateModelWithDemoNetwork();
            
            var compModel = new CompositeModel();
            compModel.Activities.Add(rtcModel);
            compModel.Activities.Add(controlledModel);
            
            var retrievedCompositeModel = SaveAndRetrieveObject(compModel);
            var retrievedRtc = retrievedCompositeModel.Models.First();
            Assert.IsNotNull(retrievedRtc);
            Assert.AreEqual("RTC Model", retrievedRtc.Name);
        }


        /// <summary>
        /// reproducing TOOLS-4119 Unhandled exception when saving, modifying, and re-saving imported JAMM2010.sbk model
        /// </summary>
        [Test]
        [Category(TestCategory.DataAccess)]
        [Category(TestCategory.Slow)]
        public void ResaveRtcModelAfterUnlinkingDueToDeletionOfObservationPoint()
        {
            // build rtc model and link it to demo waterflowmodel1d
            var rtcModel = new RealTimeControlModel("Test RTC Model");
            rtcModel.ControlGroups.Add(RealTimeControlModelHelper.CreateGroupHydraulicRule(true));

            var controlledModel = WaterFlowModel1DDemoModelTestHelper.CreateModelWithDemoNetwork();
            var observationPoint = ObservationPoint.CreateDefault(controlledModel.Network.Branches[0]); 

            controlledModel.Network.Branches[0].BranchFeatures.Add(observationPoint);
            var compModel = new CompositeModel();
            compModel.Activities.Add(rtcModel);
            compModel.Activities.Add(controlledModel);

            // Connect the observationpoint in the flow model with the first controlgroup; this is the simplest possible rtc model
            var dataItemsForObservationPoint = controlledModel.GetChildDataItems(observationPoint).Where(di => (di.Role & DataItemRole.Output) == DataItemRole.Output);
            Assert.AreNotEqual(0, dataItemsForObservationPoint.Count());
            
            var outputDataItem = dataItemsForObservationPoint.First();
            rtcModel.GetDataItemByValue(rtcModel.ControlGroups[0].Inputs[0]).LinkTo(outputDataItem);


            var path = TestHelper.GetCurrentMethodName() + "1.dsproj";
            var project = new Project();
            project.RootFolder.Add(rtcModel);

            //controlledModel.HydroNetwork.Branches[0].BranchFeatures.Remove(observationPoint);

            // save the model
            ProjectRepository.Create(path);
            ProjectRepository.SaveOrUpdate(project);

            // remove observation point

            // fails:
            // remove the observation point; data items should be cleared
            controlledModel.Network.Branches[0].BranchFeatures.Remove(observationPoint);
            
            // check if the connection is removed
            Assert.IsNull(rtcModel.ControlGroups[0].Inputs[0].Feature);

            // fails
            //rtcModel.ControlGroups.Clear();
            //controlledModel.HydroNetwork.Branches[0].BranchFeatures.Remove(observationPoint);

            // works
            // rtcModel.ControlGroups.Clear();

            // works
            // rtcModel.Models.Clear();

            // fails
            //rtcModel.Models.Clear();
            //project.RootFolder.Add(controlledModel);
            //controlledModel.HydroNetwork.Branches[0].BranchFeatures.Remove(observationPoint);


            // resave : tools 4119 -> 
            //          NHibernate.ObjectDeletedException : deleted object would be re-saved by cascade (remove deleted 
            //          object from associations)[DelftTools.Hydro.ObservationPoint#6]
            ProjectRepository.SaveOrUpdate(project);

            ProjectRepository.Close();
        }

        /// <summary>
        /// reproducing TOOLS-4119 Unhandled exception when saving, modifying, and re-saving imported JAMM2010.sbk model
        /// </summary>
        [Test]
        [Category(TestCategory.DataAccess)]
        [Category(TestCategory.Slow)]
        public void ReSaveRtcModelAfterDeletingBranchAndThusUnlinking()
        {
            const string testRtcModel = "Test RTC Model";

            // build rtc model and link it to demo waterflowmodel1d
            var rtcModel = new RealTimeControlModel(testRtcModel);
            rtcModel.ControlGroups.Add(RealTimeControlModelHelper.CreateGroupHydraulicRule(true));
            var controlledModel = WaterFlowModel1DDemoModelTestHelper.CreateModelWithDemoNetwork();
            var observationPoint = ObservationPoint.CreateDefault(controlledModel.Network.Branches[0]);
            controlledModel.Network.Branches[0].BranchFeatures.Add(observationPoint);
            var compModel = new CompositeModel();
            compModel.Activities.Add(rtcModel);
            compModel.Activities.Add(controlledModel);

            // Connect the observationpoint in the flow model with the first controlgroup; this is the simplest possible rtc model
            var itemsForObservationPoint = controlledModel.GetChildDataItems(observationPoint).Where(di => (di.Role & DataItemRole.Output) == DataItemRole.Output);
            Assert.AreNotEqual(0, itemsForObservationPoint.Count());
            var dataItem = itemsForObservationPoint.First();
            
            rtcModel.GetDataItemByValue(rtcModel.ControlGroups[0].Inputs[0]).LinkTo(dataItem);


            var path = "project.dsproj";
            var project = new Project();
            project.RootFolder.Add(rtcModel);

            // save the model
            ProjectRepository.Create(path);
            ProjectRepository.SaveOrUpdate(project);

            // remove the branch observation point; data items shouild be deleted / unlinked
            controlledModel.Network.Branches.Remove(controlledModel.Network.Branches[0]);

            // check if the connection is removed
            Assert.IsNull(rtcModel.ControlGroups[0].Inputs[0].Feature);

            // resave : tools 4119 -> 
            //          NHibernate.ObjectDeletedException : deleted object would be re-saved by cascade (remove deleted 
            //          object from associations)[DelftTools.Hydro.ObservationPoint#6]
            ProjectRepository.SaveOrUpdate(project);

            ProjectRepository.Close();
        }

        [Test]
        [Category(TestCategory.Jira)]
        [Category(TestCategory.DataAccess)]
        [Category(TestCategory.Slow)]
        public void ReloadRtcModelWithLazyLoadedFeaturesInitializesModelCorrectlyTools7140()
        {
            var project = new Project();

            // create simple rtc model 
            var rtcModel = new RealTimeControlModel();
            rtcModel.ControlGroups.Add(RealTimeControlModelHelper.CreateGroupHydraulicRule(true));
            project.RootFolder.Add(rtcModel);

            // create flow demo model
            var flowModel = WaterFlowModel1DDemoModelTestHelper.CreateModelWithDemoNetwork();
            var observationPoint = ObservationPoint.CreateDefault(flowModel.Network.Branches[0]);
            flowModel.Network.Branches[0].BranchFeatures.Add(observationPoint);
            
            // add flow to RTC
            var compModel = new CompositeModel();
            compModel.Activities.Add(rtcModel);
            compModel.Activities.Add(flowModel);

            // connect the observationpoint in the flow model with the first controlgroup
            var dataItem = flowModel.GetChildDataItems(observationPoint).First(di => (di.Role & DataItemRole.Output) == DataItemRole.Output);
            rtcModel.GetDataItemByValue(rtcModel.ControlGroups[0].Inputs[0]).LinkTo(dataItem);

            const string path = "proxies.dsproj";
            
            // save the model
            using (var projectRepository = factory.CreateNew())
            {
                projectRepository.SaveAs(project, path);
            }
            
            // reload the model
            using (var projectRepository = factory.CreateNew())
            {
                var retrievedProject = projectRepository.Open(path);

                var retrievedRtc = (RealTimeControlModel) retrievedProject.RootFolder.Models.First();

                var retrievedInputName = retrievedRtc.ControlGroups.First().Inputs[0].Name;
                Assert.AreEqual("ObservationPoint1_Water level (op)", retrievedInputName);
            }
        }
        
        [Test]
        [Category(TestCategory.Jira)]
        [Category(TestCategory.DataAccess)]
        [Category(TestCategory.VerySlow)]
        [Ignore("No such things as RTC outputs with DIMR (so far)")]
        public void ImportSaveAndReloadRtcModelWithLazyLoadedFeaturesShouldWorkTools7140()
        {
            const string path = "proxies2.dsproj";
            var project = new Project();
            
            // import sobek model and add to project
            var importPath = TestHelper.GetTestDataPath(typeof(SobekWaterFlowModel1DImporterTest).Assembly, @"ReModels\J_10BANK_v2.sbk\4\DEFTOP.1");
            var modelImporter = new SobekHydroModelImporter(useRR: false);
            var hydroModel = (HydroModel)modelImporter.ImportItem(importPath);
            project.RootFolder.Add(hydroModel);
            hydroModel.ExplicitWorkingDirectory = Path.GetFullPath(Path.Combine(".", TestHelper.GetCurrentMethodName()));
            hydroModel.Initialize();

            var rtc = hydroModel.Activities.OfType<RealTimeControlModel>().First();
            var crestLevelCoverage = rtc.OutputFeatureCoverages.First(fc => fc.Name == "Crest level (s)");
            var featureCount = crestLevelCoverage.Features.Count;

            // save the model
            using (var projectRepository = factory.CreateNew())
            {
                projectRepository.SaveAs(project, path);
            }

            // reload the model
            using (var projectRepository = factory.CreateNew())
            {
                var retrievedProject = projectRepository.Open(path);
                var retrievedHydro = (HydroModel)retrievedProject.RootFolder.Models.First();

                // initialize to fill coverages
                retrievedHydro.Initialize();
                var retrievedRtc = retrievedHydro.Activities.OfType<RealTimeControlModel>().First();

                var retrievedCrestLevelCoverage = retrievedRtc.OutputFeatureCoverages.First(fc => fc.Name == "Crest level (s)");
                
                // asserts
                Assert.AreEqual(featureCount, retrievedCrestLevelCoverage.Features.Count, "not all features included");
                Assert.AreEqual(featureCount, retrievedCrestLevelCoverage.FeatureVariable.Values.Count); //optional check
            }
        }

        [Test]
        [Category(TestCategory.Jira)]
        [Category(TestCategory.DataAccess)]
        [Category(TestCategory.Slow)]
        [Ignore("No such things as RTC outputs with DIMR (so far)")]
        public void SaveAndLoadRtcModelWithLazyLoadedFeaturesShouldWorkTools7140()
        {
            const string path = "rtc_with_proxied_features.dsproj";

            var project = new Project();

            // import sobek model and add to project
            var importPath = TestHelper.GetTestDataPath(typeof(SobekWaterFlowModel1DImporterTest).Assembly, @"HKTG.lit\1\NETWORK.TP");
            var modelImporter = new SobekHydroModelImporter(useRR: false);
            var hydroModel = (HydroModel)modelImporter.ImportItem(importPath);
            hydroModel.ExplicitWorkingDirectory = Path.GetFullPath(Path.Combine(".", TestHelper.GetCurrentMethodName()));

            project.RootFolder.Add(hydroModel);
            hydroModel.Initialize();

            var rtc = hydroModel.Activities.OfType<RealTimeControlModel>().First();

            var crestLevelCoverage = rtc.OutputFeatureCoverages.First(fc => fc.Name == "Crest level (s)");

            // save the model
            using (var projectRepository = factory.CreateNew())
            {
                projectRepository.SaveAs(project, path);
            }

            // reload the model
            using (var projectRepository = factory.CreateNew())
            {
                var retrievedProject = projectRepository.Open(path);
                var retrievedHydro = (HydroModel)retrievedProject.RootFolder.Models.First();

                var retrievedRtc = retrievedHydro.Activities.OfType<RealTimeControlModel>().First();
                // initialize to fill coverages
                retrievedRtc.Initialize();

                var retrievedCrestLevelCoverage = retrievedRtc.OutputFeatureCoverages.First(fc => fc.Name == "Crest level (s)");

                // asserts
                var expectedFeatureCount = crestLevelCoverage.Features.Count;
                Assert.AreEqual(expectedFeatureCount, retrievedCrestLevelCoverage.Features.Count, "not all features included");
                Assert.AreEqual(expectedFeatureCount, retrievedCrestLevelCoverage.FeatureVariable.Values.Count); //optional check
            }
        }

        private IEnumerable<IFileExporter> GetFactoryFileExportersForDimr()
        {
            return factory.SessionProvider.ConfigurationProvider.Plugins.OfType<ApplicationPlugin>().SelectMany(p => p.GetFileExporters()).Plus(new Iterative1D2DCouplerExporter());
        }

        [Test]
        [Category(TestCategory.Jira)]
        [Category(TestCategory.DataAccess)]
        [Category(TestCategory.Slow)]
        public void SaveAndLoadRtcModelWhileControllingAHydroNodeTools9625()
        {
            const string Path = "rtc_flow_hydro_node.dsproj";

            var project = new Project();

            // import small sobek model and add to project
            var importPath = TestHelper.GetTestDataPath(typeof(SobekWaterFlowModel1DImporterTest).Assembly, @"HKTG.lit\1\NETWORK.TP");
            var modelImporter = new SobekHydroModelImporter(useRR: false);
            var hydroModel = (HydroModel)modelImporter.ImportItem(importPath);
            project.RootFolder.Add(hydroModel);

            var flow = hydroModel.Activities.OfType<WaterFlowModel1D>().First();
            var rtc = hydroModel.Activities.OfType<RealTimeControlModel>().First();

            // link input to hydro node
            var hydroNode = flow.Network.HydroNodes.First();
            var dataItem = flow.GetChildDataItems(hydroNode).First(di => (di.Role & DataItemRole.Output) == DataItemRole.Output);
            rtc.GetDataItemByValue(rtc.ControlGroups[0].Inputs[0]).LinkTo(dataItem);

            // save & reload the model
            using (var projectRepository = factory.CreateNew())
            {
                projectRepository.SaveAs(project, Path);
                projectRepository.Close();
                var retrievedProject = projectRepository.Open(Path);
                var retrievedHydroModel = (HydroModel)retrievedProject.RootFolder.Models.First();
                var retrievedRtc = retrievedHydroModel.Activities.OfType<RealTimeControlModel>().First();
                Assert.IsTrue(retrievedRtc.ControlGroups[0].Inputs[0].Feature is IHydroNode);
            }
        }

        [Test]
        [Category(TestCategory.Jira)]
        [Category(TestCategory.DataAccess)]
        [Category(TestCategory.Slow)]
        public void GetChildDataItemsAfterLoad()
        {
            //relates to 5747
            var model = WaterFlowModel1DDemoModelTestHelper.CreateModelWithDemoNetwork();
            AddWeirToLocation(new NetworkLocation(model.Network.Branches.First(), 50));

            var project = new Project();
            project.RootFolder.Add(model);

            var path = TestHelper.GetCurrentMethodName() + ".dsproj";

            //before 
            var weir = model.Network.Weirs.First();
            var childDataItems = model.GetChildDataItems(weir);
            Assert.AreEqual(10, childDataItems.Count());
            Assert.AreEqual(10, childDataItems.Count(ei => (ei.Role & DataItemRole.Output)  == DataItemRole.Output));

            ProjectRepository.Create(path);
            ProjectRepository.SaveOrUpdate(project);
            ProjectRepository.Close();

            var retrievedProject = ProjectRepository.Open(path);
            var retrievedModel = retrievedProject.GetAllItemsRecursive().OfType<WaterFlowModel1D>().First();
            var retrievedWeir = retrievedProject.GetAllItemsRecursive().OfType<Weir>().First();

            var retrievedChildDataItems = retrievedModel.GetChildDataItems(retrievedWeir);
            Assert.AreEqual(10, retrievedChildDataItems.Count());
            Assert.AreEqual(10, retrievedChildDataItems.Count(ei => (ei.Role & DataItemRole.Output) == DataItemRole.Output));
        }

        private static void AddWeirToLocation(NetworkLocation networkLocation)
        {
            var weirBranch = networkLocation.Branch;
            var weirOfset = networkLocation.Chainage;
            var weir = new Weir();
            weir.Chainage = weirOfset;
            weir.CrestLevel = 0.1;
            weir.Geometry = new Point(weirBranch.Geometry.Coordinates[0]);
            HydroNetworkHelper.AddStructureToExistingCompositeStructureOrToANewOne(weir, weirBranch);
        }
    }
}