using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Forms;
using DelftTools.Functions.Generic;
using DelftTools.Hydro;
using DelftTools.Hydro.Helpers;
using DelftTools.Hydro.Structures;
using DelftTools.Shell.Core;
using DelftTools.Shell.Core.Workflow.DataItems;
using DelftTools.Shell.Gui;
using DelftTools.TestUtils;
using DelftTools.Utils.Collections;
using DelftTools.Utils.Reflection;
using DeltaShell.Gui;
using DeltaShell.Plugins.CommonTools;
using DeltaShell.Plugins.CommonTools.Gui;
using DeltaShell.Plugins.CommonTools.Gui.Forms.Functions;
using DeltaShell.Plugins.Data.NHibernate;
using DeltaShell.Plugins.DelftModels.HydroModel;
using DeltaShell.Plugins.DelftModels.HydroModel.Export;
using DeltaShell.Plugins.DelftModels.HydroModel.Gui;
using DeltaShell.Plugins.DelftModels.RealTimeControl;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Domain;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Gui;
using DeltaShell.Plugins.DelftModels.RealTimeControl.ImportExport;
using DeltaShell.Plugins.DelftModels.WaterFlowModel;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.Gui;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.TestUtils;
using DeltaShell.Plugins.ImportExport.Sobek;
using DeltaShell.Plugins.ImportExport.Sobek.Tests;
using DeltaShell.Plugins.NetCDF;
using DeltaShell.Plugins.NetworkEditor;
using DeltaShell.Plugins.NetworkEditor.Gui;
using DeltaShell.Plugins.NetworkEditor.Gui.Helpers;
using DeltaShell.Plugins.ProjectExplorer;
using DeltaShell.Plugins.SharpMapGis;
using DeltaShell.Plugins.SharpMapGis.Gui;
using DeltaShell.Plugins.SharpMapGis.Gui.Forms;
using GeoAPI.Geometries;
using NUnit.Framework;
using SharpMap;
using SharpMap.Api.Layers;
using SharpMap.UI.Forms;
using Control = System.Windows.Controls.Control;
using Point = NetTopologySuite.Geometries.Point;

namespace Sobek.IntegrationTests
{
    [TestFixture]
    public class RealTimeControlModelGuiIntegrationTest
    {
        private IApplication app;

        private DeltaShellGui gui;

        [Test]
        [Category(TestCategory.DataAccess)]
        [Category(TestCategory.Jira)] //TOOLS-5080
        [Category(TestCategory.WindowsForms)] //TOOLS-5080
        [Category(TestCategory.Slow)]
        public void DeleteClonedCopyOfRtcWithSaltAfterSaveAndSaveAgainShouldNotGiveException()
        {
            string pathToSobekNetwork = TestHelper.GetTestDataPath(typeof(SobekWaterFlowModel1DImporterTest).Assembly, @"ReModels\JAMM2010.sbk\40\deftop.1");

            var modelImporter = new SobekHydroModelImporter(false);
            var hydroModel = (HydroModel)modelImporter.ImportItem(pathToSobekNetwork);
            
            var path = "dcc_nosalt.dsproj";

            gui.Application.Project.RootFolder.Add(hydroModel);

            var mainWindow = (Window)gui.MainWindow;

            Action mainWindowShown = delegate
                {
                    //MessageBox.Show("Continue: clone");
                    //clone model

                    var clonedModel = hydroModel.DeepClone();
                    clonedModel.Name = "Copy of " + clonedModel.Name;
                    gui.Application.Project.RootFolder.Add(clonedModel);

                    //MessageBox.Show("Continue: save");
                    //save

                    gui.Application.SaveProjectAs(path);

                    //MessageBox.Show("Continue: delete");
                    //now delete original model:

                    var originalModelItem = gui.Application.Project.RootFolder.Items.ElementAt(1);
                    gui.Application.Project.RootFolder.Items.Remove(originalModelItem);

                    //MessageBox.Show("Continue: save");
                    //save again

                    gui.Application.SaveProjectAs(path);
                };

            WpfTestHelper.ShowModal(mainWindow, mainWindowShown);
        }

        [Test]
        [Category(TestCategory.Integration)]
        [Category(TestCategory.DataAccess)]
        [Category(TestCategory.Jira)] //TOOLS-5080
        [Category(TestCategory.Slow)]
        public void DeleteClonedCopyOfRtcWithoutSaltAfterSaveAndSaveAgainShouldNotGiveException()
        {
            string pathToSobekNetwork = TestHelper.GetTestDataPath(typeof(SobekWaterFlowModel1DImporterTest).Assembly, @"ReModels\JAMM2010.sbk\40\deftop.1");

            var modelImporter = new SobekHydroModelImporter(false);
            var hydroModel = (HydroModel)modelImporter.ImportItem(pathToSobekNetwork);

            var wfmodel = hydroModel.Models.OfType<WaterFlowModel1D>().First();
            wfmodel.UseSalt = false;

            var path = "dcc_salt.dsproj";

            gui.Application.Project.RootFolder.Add(hydroModel);

            //clone model

            var clonedModel = hydroModel.DeepClone();
            clonedModel.Name = "Copy of " + clonedModel.Name;
            gui.Application.Project.RootFolder.Add(clonedModel);

            //MessageBox.Show("Continue: save");
            //save

            gui.Application.SaveProjectAs(path);

            //MessageBox.Show("Continue: delete");
            //now delete original model:

            var originalModelItem = gui.Application.Project.RootFolder.Items.ElementAt(1);
            gui.Application.Project.RootFolder.Items.Remove(originalModelItem);

            //MessageBox.Show("Continue: save");
            //save again

            gui.Application.SaveProjectAs(path);
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void TestRtcOutput_LoadRunSaveAsLoad()
        {
            // TestSetup is not sufficient for this test... so we do things a little differently
            gui.Dispose();
            app.Dispose();

            using (gui = new DeltaShellGui())
            {
                app = gui.Application;

                app.Plugins.Add(new SharpMapGisApplicationPlugin());
                app.Plugins.Add(new CommonToolsApplicationPlugin());
                app.Plugins.Add(new NetworkEditorApplicationPlugin());
                app.Plugins.Add(new WaterFlowModel1DApplicationPlugin());
                app.Plugins.Add(new RealTimeControlApplicationPlugin());
                app.Plugins.Add(new NHibernateDaoApplicationPlugin());
                app.Plugins.Add(new NetCdfApplicationPlugin());
                app.Plugins.Add(new HydroModelApplicationPlugin());

                gui.Plugins.Add(new ProjectExplorerGuiPlugin());
                gui.Plugins.Add(new SharpMapGisGuiPlugin());
                gui.Plugins.Add(new CommonToolsGuiPlugin());
                gui.Plugins.Add(new NetworkEditorGuiPlugin());
                gui.Plugins.Add(new WaterFlowModel1DGuiPlugin());
                gui.Plugins.Add(new RealTimeControlGuiPlugin());
                gui.Plugins.Add(new HydroModelGuiPlugin());

                gui.Run();
                
                var legacyPath = TestHelper.GetTestFilePath(@"RtcOutput\Flow1D_WithRtcOutput.dsproj");
                var localLegacyPath = TestHelper.CopyProjectToLocalDirectory(legacyPath);
                app.OpenProject(localLegacyPath);

                var originalHydroModel = app.Project.RootFolder.Models.OfType<HydroModel>().FirstOrDefault();
                Assert.NotNull(originalHydroModel);

                var originalRtcModel = originalHydroModel.Activities.OfType<RealTimeControlModel>().FirstOrDefault();
                Assert.NotNull(originalRtcModel);

                var originalOutputFileFunctionStore = originalRtcModel.OutputFileFunctionStore;
                Assert.NotNull(originalOutputFileFunctionStore);
                Assert.Greater(originalOutputFileFunctionStore.Functions.Count, 0);

                var originalFunction = originalOutputFileFunctionStore.Functions.First();

                gui.MainWindow.Show();
                app.RunActivity(originalHydroModel);

                var resavedPath = "resaved_" + localLegacyPath;
                app.SaveProjectAs(resavedPath);
                app.OpenProject(resavedPath);

                var resavedHydroModel = app.Project.RootFolder.Models.OfType<HydroModel>().FirstOrDefault();
                Assert.NotNull(resavedHydroModel);

                var resavedRtcModel = resavedHydroModel.Activities.OfType<RealTimeControlModel>().FirstOrDefault();
                Assert.NotNull(resavedRtcModel);

                var resavedOutputFileFunctionStore = resavedRtcModel.OutputFileFunctionStore;
                Assert.NotNull(resavedOutputFileFunctionStore);
                Assert.Greater(resavedOutputFileFunctionStore.Functions.Count, 0);

                var resavedFunction = resavedOutputFileFunctionStore.Functions.First();

                Assert.AreEqual(originalFunction.Name, resavedFunction.Name);

                var originalVariables = originalFunction.Arguments.Concat(originalFunction.Components).ToList();
                var resavedVariables = resavedFunction.Arguments.Concat(resavedFunction.Components).ToList();
                Assert.AreEqual(originalVariables.Count, resavedVariables.Count);

                var originalDateTimeVariables = originalVariables.OfType<IVariable<DateTime>>().ToList();
                var resavedDateTimeVariables = resavedVariables.OfType<IVariable<DateTime>>().ToList();
                Assert.AreEqual(originalDateTimeVariables.Count, resavedDateTimeVariables.Count);
                Assert.IsTrue(VariablesAreEqual(originalDateTimeVariables, resavedDateTimeVariables));

                var originalDoubleVariables = originalVariables.OfType<IVariable<double>>().ToList();
                var resavedDoubleVariables = resavedVariables.OfType<IVariable<double>>().ToList();
                Assert.AreEqual(originalDoubleVariables.Count, resavedDoubleVariables.Count);
                Assert.IsTrue(VariablesAreEqual(originalDoubleVariables, resavedDoubleVariables));
            }
        }

        private static bool VariablesAreEqual<T>(IList<IVariable<T>> firstVariableList, IList<IVariable<T>> secondVariableList)
        {
            for (var i = 0; i < firstVariableList.Count; i++)
            {
                var firstVariableValues = firstVariableList[i].Values;
                var secondVariableValues = secondVariableList[i].Values;

                if (!VariablesAreEqual(firstVariableValues, secondVariableValues)) return false;
            }

            return true;
        }

        private static bool VariablesAreEqual<T>(IEnumerable<T> firstVariable, IEnumerable<T> secondVariable)
        {
            var firstValues = firstVariable.ToList();
            var secondValues = secondVariable.ToList();
            if (firstValues.Count != secondValues.Count) return false;

            for (var j = 0; j < firstValues.Count; j++)
            {
                if (!firstValues[j].Equals(secondValues[j])) return false;
            }
            

            return true;
        }

        [Test]
        [Category(TestCategory.WindowsForms)]
        [Category(TestCategory.Slow)]
        public void TestConnectToRtcOutputFileAndShowInFunctionView()
        {
            RealTimeControlModel rtcModel;
            RealTimeControlOutputFileFunctionStore outputFunctionStore;
            GetSimpleRealTimeControlModelWithOutputFileFunctionStore(out outputFunctionStore, out rtcModel);

            // Show in functionView
            var function = outputFunctionStore.Functions.First();
            var view = new FunctionView() { Data = function };
            WindowsFormsTestHelper.ShowModal(view);
        }

        [Test]
        [Category(TestCategory.WindowsForms)]
        [Category(TestCategory.Slow)]
        public void TestConnectToRtcOutputFileAndShowInProjectTree()
        {
            RealTimeControlModel rtcModel;
            RealTimeControlOutputFileFunctionStore outputFunctionStore;
            GetSimpleRealTimeControlModelWithOutputFileFunctionStore(out outputFunctionStore, out rtcModel);

            // Show in ProjectTree
            WpfTestHelper.ShowModal((Control)gui.MainWindow);
        }

        [Test]
        [Category(TestCategory.WindowsForms)]
        [Category(TestCategory.Slow)]
        public void TestConnectToRtcOutputFileAndShowInMapView()
        {
            RealTimeControlModel rtcModel;
            RealTimeControlOutputFileFunctionStore outputFunctionStore;
            GetSimpleRealTimeControlModelWithOutputFileFunctionStore(out outputFunctionStore, out rtcModel);

            var providers = new IMapLayerProvider[]{ new RealTimeControlMapLayerProvider() };
            var layer = (IGroupLayer)MapLayerProviderHelper.CreateLayersRecursive(rtcModel, null, providers);
            layer.Layers.ForEach(l => l.Visible = true);

            var map = new Map { Layers = { layer }, Size = new System.Drawing.Size { Width = 800, Height = 800 } };
            map.ZoomToExtents();

            var mapControl = new MapControl { Map = map, Dock = DockStyle.Fill };

            // Nothing to see really, but should not crash!
            WindowsFormsTestHelper.ShowModal(mapControl);
        }
        
        [Test]
        [Category(TestCategory.WindowsForms)]
        public void LinkRtcRuleOutputToWeirCrestLevel()
        {
            // create flow1d model
            var weir = new Weir() {Geometry = new Point(new Coordinate(10, 0))};
            var from = new HydroNode() { Geometry = new Point(new Coordinate(0, 0)) };
            var to = new HydroNode() { Geometry = new Point(new Coordinate(100, 0)) };
            var network = new HydroNetwork { Branches = { new Channel { BranchFeatures = { weir }, Source = from, Target = to } }, Nodes = { from, to } };
            var flowModel = new WaterFlowModel1D { Network = network };

            // create RTC model
            var output = new Output();
            var rtcModel = new RealTimeControlModel { ControlGroups = { new ControlGroup { Outputs = { output } } } };
            var hydroModel = new HydroModel { Activities = { rtcModel, flowModel } };

            gui.Application.Project.RootFolder.Add(hydroModel);
            
            // attach models to eacht other
            var source = rtcModel.GetDataItemByValue(output);
            var target = flowModel.GetChildDataItems(weir).First();

            // link
            target.LinkTo(source);
        }
        
        [Test]
        [Category(TestCategory.Integration)]
        public void DisconnectingItemsDoesNotUnlinkDataItemTools7412()
        {
            // create flow1d model
            var weir = new Weir() {Geometry = new Point(new Coordinate(10,0))};
            var from = new HydroNode() { Geometry = new Point(new Coordinate(0, 0)) };
            var to = new HydroNode() { Geometry = new Point(new Coordinate(100, 0)) };
            var network = new HydroNetwork { Branches = { new Channel { BranchFeatures = { weir }, Source = from, Target = to } }, Nodes = { from, to } };
            var flowModel = new WaterFlowModel1D { Network = network };

            // create RTC model
            var input = new Input();
            var rule = new FactorRule {Inputs = {input}};
            var rtcModel = new RealTimeControlModel {ControlGroups = {new ControlGroup {Inputs = {input}, Rules = {rule}}}};
            var hydroModel = new HydroModel { Activities = { rtcModel, flowModel } };

            gui.Application.Project.RootFolder.Add(hydroModel);

            // attach models to eacht other
            var target = rtcModel.GetDataItemByValue(input);
            var source = flowModel.GetChildDataItems(weir).First();

            // link
            target.LinkTo(source);

            // action
            rule.Inputs.Remove(input);

            // assert not unlinked
            Assert.IsTrue(target.LinkedTo != null);
        }

        [Test]
        [Category(TestCategory.Integration)]
        [Category(TestCategory.Slow)]
        public void MovingFeatureDoesNotUnlinkInputTools7415()
        {
            // create flow1d model
            var network = HydroNetworkHelper.GetSnakeHydroNetwork(new Point(0, 0), new Point(100, 0));
            var branch = network.Branches.First();
            var lateral = new LateralSource {Branch = branch, Chainage = 30, Geometry = new Point(30, 0)};
            branch.BranchFeatures.Add(lateral);
            var flowModel = new WaterFlowModel1D { Network = network };

            // create RTC model
            var input = new Input();
            var rule = new FactorRule { Inputs = { input } };
            var rtcModel = new RealTimeControlModel { ControlGroups = { new ControlGroup { Inputs = { input }, Rules = { rule } } } };
            var hydroModel = new HydroModel { Activities = { rtcModel, flowModel } };

            gui.Application.Project.RootFolder.Add(hydroModel);

            // attach models to eacht other
            var target = rtcModel.GetDataItemByValue(input);
            var source = flowModel.GetChildDataItems(lateral).First();

            // link
            target.LinkTo(source);

            // action
            HydroRegionEditorHelper.MoveBranchFeatureTo(lateral, 15, false);

            // assert not unlinked
            Assert.IsTrue(target.LinkedTo != null);
        }

        [Test]
        [Category(TestCategory.WindowsForms)]
        [Category(TestCategory.Jira)]
        [Category(TestCategory.Slow)]
        public void RunRealTimeControlModelWithFlowModelShouldNotMarkFlowOutputOutOfSync_Issue7002()
        {
            var path = TestHelper.GetTestDataPath(typeof(SobekWaterFlowModel1DImporterTest).Assembly, @"HKTG.lit\1\NETWORK.TP");
            var modelImporter = new SobekHydroModelImporter(false);
            var hydroModel = (HydroModel)modelImporter.ImportItem(path);
            gui.Application.Project.RootFolder.Add(hydroModel);

            var flowModel = hydroModel.Models.OfType<WaterFlowModel1D>().First();
            flowModel.StopTime = flowModel.StartTime.Add(flowModel.TimeStep); // 1 time step

            LogHelper.ConfigureLogging();

            // run
            hydroModel.Initialize();
            hydroModel.Execute();

            // checks
            Assert.IsFalse(flowModel.OutputOutOfSync);

            hydroModel.Cleanup();
        }

        [Test]
        [Category(TestCategory.WindowsForms)]
        [Category(TestCategory.Slow)]
        [Category(TestCategory.WorkInProgress)] //hydro model branch
        public void RunAndCopyPasteRealTimeControlModelWithFlowModelShouldNotMarkClonedModelFlowOutputOutOfSync()
        {
            var path = TestHelper.GetTestDataPath(typeof(SobekWaterFlowModel1DImporterTest).Assembly, @"HKTG.lit\1\NETWORK.TP");
            var modelImporter = new SobekHydroModelImporter(false);
            var hydroModel = (HydroModel)modelImporter.ImportItem(path);

            gui.Application.Project.RootFolder.Add(hydroModel);

            var flowModel = hydroModel.Models.OfType<WaterFlowModel1D>().First();
            flowModel.StopTime = flowModel.StartTime.Add(flowModel.TimeStep); // 1 time step

            LogHelper.ConfigureLogging();

            // run
            hydroModel.Initialize();
            hydroModel.Execute();
            hydroModel.Cleanup();

            // copy/paste
            gui.CopyPasteHandler.Copy(hydroModel);
            gui.CopyPasteHandler.Paste(gui.Application.Project, gui.Application.Project.RootFolder);

            // checks
            var flowModelClone =
                gui.Application.Project.RootFolder.Models.OfType<HydroModel>().Last().Models.OfType<WaterFlowModel1D>().
                    First();
            Assert.IsFalse(flowModelClone.OutputOutOfSync);

            gui.Application.CloseProject();
        }

        [Test]
        [Category(TestCategory.WindowsForms)]
        [Category(TestCategory.Jira)]
        [Category(TestCategory.Slow)]
        public void CopyPasteImportedRtcAndFlowModelAfterRunShouldNotCrash_Issue7010()
        {
            var path = TestHelper.GetTestDataPath(typeof(SobekWaterFlowModel1DImporterTest).Assembly, @"HKTG.lit\1\NETWORK.TP");
            var modelImporter = new SobekHydroModelImporter(false);
            var hydroModel = (HydroModel)modelImporter.ImportItem(path);
            gui.Application.Project.RootFolder.Add(hydroModel);

            var flowModel = hydroModel.Models.OfType<WaterFlowModel1D>().First();
            flowModel.StopTime = flowModel.StartTime.Add(flowModel.TimeStep); // 1 time step

            LogHelper.ConfigureLogging();

            hydroModel.Initialize();
            hydroModel.Execute();
            hydroModel.Cleanup();

            // copy/paste
            gui.CopyPasteHandler.Copy(hydroModel);
            gui.CopyPasteHandler.Paste(gui.Application.Project, gui.Application.Project.RootFolder);
        }

        private IEnumerable<IFileExporter> GetApplicationFileExportersForDimr()
        {
            return app.Plugins.SelectMany(p => p.GetFileExporters()).Plus(new Iterative1D2DCouplerExporter());
        }

        [Test]
        [Category(TestCategory.WindowsForms)]
        public void CopyPastePasteRtcModelTwiceWithFlowInApplication()
        {
            // domain objects necessary for testing
            var flowModel = WaterFlowModel1DDemoModelTestHelper.CreateModelWithDemoNetwork();
            var rtcModel = new RealTimeControlModel();
            var hydroModel = new HydroModel { Activities = { rtcModel } };

            hydroModel.Region.SubRegions.Add(flowModel.Region);
            hydroModel.Activities.Add(flowModel);
            var network = hydroModel.Region.SubRegions.OfType<HydroNetwork>().FirstOrDefault();

            var observationPoint = ObservationPoint.CreateDefault(network.Branches[0]);
            network.Branches[0].BranchFeatures.Add(observationPoint);
            
            var weir = new Weir("Weir1") {Geometry = new Point(network.Branches[0].Geometry.Coordinates[0])};
            network.Branches[0].BranchFeatures.Add(weir);

            rtcModel.ControlGroups.Add(RealTimeControlModelHelper.CreateGroupHydraulicRule(true));

            gui.Application.Project.RootFolder.Add(hydroModel);
            // attach models to eacht other
            var rtcInputdataItem = rtcModel.AllDataItems
                .First(di => di.ValueConverter != null && ReferenceEquals(di.ValueConverter.OriginalValue, rtcModel.ControlGroups[0].Inputs[0]));

            var rtcOutputDataItem = rtcModel.AllDataItems
                .First(di => di.ValueConverter != null && ReferenceEquals(di.ValueConverter.OriginalValue, rtcModel.ControlGroups[0].Outputs[0]));
            
            var flowObservationOutputDataItem = rtcModel.GetChildDataItemsFromControlledModelsForLocation(observationPoint).First();
            var flowWeirInputDataItem = rtcModel.GetChildDataItemsFromControlledModelsForLocation(weir).First();

            // link
            rtcInputdataItem.LinkTo(flowObservationOutputDataItem);
            flowWeirInputDataItem.LinkTo(rtcOutputDataItem);
            
            // copy paste paste should not throw exception
            gui.CopyPasteHandler.Copy(hydroModel);
            gui.CopyPasteHandler.Paste(gui.Application.Project, gui.Application.Project.RootFolder);
            gui.CopyPasteHandler.Paste(gui.Application.Project, gui.Application.Project.RootFolder);
            
            // 3 models should be in the final project
            Assert.AreEqual(3, gui.Application.Project.RootFolder.Items.Count);
        }

        [Test]
        [Category(TestCategory.Integration)]
        [Category(TestCategory.VerySlow)]
        public void CloneModelLinkedToExternalNetworkInProjectKeepsNetworkLinked()
        {
            var project = gui.Application.Project;

            // add network to the project
            var network = new HydroNetwork();
            var networkDataItem = new DataItem { Value = network, ValueType = typeof(HydroNetwork) };
            project.RootFolder.Add(networkDataItem);

            // add hydro model (flow + rtc) to the project
            var flowModel = new WaterFlowModel1D();
            var rtcModel = new RealTimeControlModel();
            var hydroModel = new HydroModel {Activities = {flowModel, rtcModel}};
            project.RootFolder.Add(hydroModel);

            // link flow to the network in project
            var flowModelNetworkDataItem = flowModel.GetDataItemByValue(flowModel.Network);
            flowModelNetworkDataItem.LinkTo(networkDataItem);

            // clone hydro model
            var hydroModelClone =(HydroModel) hydroModel.DeepClone();
            var flowModelClone = hydroModelClone.Activities.OfType<WaterFlowModel1D>().First();

            // asserts
            Assert.AreEqual(network, flowModelClone.Network);
            
            var flowModelNetworkDataItemClone = flowModelClone.GetDataItemByValue(flowModelClone.Network);
            Assert.AreEqual(networkDataItem, flowModelNetworkDataItemClone.LinkedTo);
            Assert.AreEqual(2, networkDataItem.LinkedBy.Count); // Original model & cloned model
            Assert.AreEqual(flowModelNetworkDataItem, networkDataItem.LinkedBy[0]);
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void CopyPasteModelLinkedToExternalNetworkInProject()
        {
            var network = new HydroNetwork();
            var networkDataItem = new DataItem { Value = network, ValueType = typeof(HydroNetwork) };
            var flowModel = new WaterFlowModel1D();
            var rtcModel = new RealTimeControlModel();
            var hydroModel = new HydroModel { Activities = { flowModel, rtcModel } };

            var project = gui.Application.Project;

            project.RootFolder.Add(networkDataItem);
            project.RootFolder.Add(hydroModel);

            var flowModelNetworkDataItem = flowModel.GetDataItemByValue(flowModel.Network);
            flowModelNetworkDataItem.LinkTo(networkDataItem);

            // copy/paste
            gui.CopyPasteHandler.Copy(hydroModel);
            gui.CopyPasteHandler.Paste(project, project.RootFolder);

            var hydroModelCopy = (HydroModel)project.RootFolder.Models.ToList()[1];

            // asserts
            var flowModelCopy = hydroModelCopy.Models.OfType<WaterFlowModel1D>().First();
            Assert.AreEqual(network, flowModelCopy.Network);

            var flowModelNetworkDataItemCopy = flowModelCopy.GetDataItemByValue(flowModelCopy.Network);
            Assert.AreEqual(networkDataItem, flowModelNetworkDataItemCopy.LinkedTo);
            Assert.AreEqual(2, networkDataItem.LinkedBy.Count);
            Assert.AreEqual(flowModelNetworkDataItem, networkDataItem.LinkedBy[0]);
            Assert.AreEqual(flowModelNetworkDataItemCopy, networkDataItem.LinkedBy[1]);

        }

        [Test]
        [Category(TestCategory.Integration)]
        [Category(TestCategory.Slow)]
        public void CopyPasteFlowModelFromHydroModelToProjectBreaksLinks()
        {
            // create flow1d model
            var weir = new Weir() {Geometry = new Point(new Coordinate(10,0)) };
            var from = new HydroNode() { Geometry = new Point(new Coordinate(0, 0)) }; ;
            var to = new HydroNode() { Geometry = new Point(new Coordinate(100, 0)) }; ;
            var network = new HydroNetwork { Branches = { new Channel { BranchFeatures = { weir }, Source = from, Target = to } }, Nodes = { from, to } };
            var flowModel = new WaterFlowModel1D { Network = network };

            var rtcModel = new RealTimeControlModel();
            var hydroModel = new HydroModel { Activities = { rtcModel } };

            hydroModel.Region.SubRegions.Add(flowModel.Region);
            hydroModel.Activities.Add(flowModel);

            var controlGroup = new ControlGroup();
            rtcModel.ControlGroups.Add(controlGroup);

            var output = new Output();
            controlGroup.Outputs.Add(output);

            // add to project
            var project = gui.Application.Project;
            project.RootFolder.Add(hydroModel);

            // link
            var outputDataItem = rtcModel.GetDataItemByValue(output);
            var inputDataItem = flowModel.GetChildDataItems(weir).First();
            inputDataItem.LinkTo(outputDataItem);

            // ACTION: copy/paste flow model
            gui.CopyPasteHandler.Copy(flowModel);
            gui.CopyPasteHandler.Paste(project, project.RootFolder);

            // asserts (check that copy/pasted flow model links are broken
            var flowModelClone = (WaterFlowModel1D)project.RootFolder.Models.Last();
            
            // get cloned weir child data item which was linked to RTC
            var networkCloneDataItem = flowModelClone.GetDataItemByValue(flowModelClone.Network);

            Assert.AreEqual(1, networkCloneDataItem.Children.Count, "child weir data item in copy/pasted flow model is not deleted");
            Assert.IsNull(networkCloneDataItem.Children[0].LinkedTo, "child weir data item in copy/pasted flow model is unlinked");
        }
        
        [Test]
        [Category(TestCategory.DataAccess)]
        [Category(TestCategory.Slow)]
        [Category(TestCategory.BackwardCompatibility)]
        [Category(TestCategory.WorkInProgress)]
        public void OpenRolfModelWithUndoRedoAndMoveNode()
        {
            Action onMainWindowShown = null;
            var mainWindow = (Window)gui.MainWindow;

            onMainWindowShown =
                () =>
                {
                    gui.UndoRedoManager.TrackChanges = true;

                    // open rolf's model
                    var projectPath = TestHelper.GetTestFilePath("j03_18008_run_v064.dsproj");
                    var projectFileName = TestHelper.CopyProjectToLocalDirectory(projectPath);

                    gui.Application.CloseProject();
                    gui.CommandHandler.OpenProject(projectFileName);
                    
                    // get models
                    var rtcModel = (RealTimeControlModel)gui.Application.Project.RootFolder.Models.First();
                    var flowModel = (WaterFlowModel1D)rtcModel.ControlledModels.First();
                    var network = flowModel.Network;

                    // open network editor
                    gui.CommandHandler.OpenView(network);
                    var networkEditor = (ProjectItemMapView)gui.DocumentViews.ActiveView;
                    var mapControl = networkEditor.MapView.MapControl;

                    // move node
                    var node = network.Nodes[0];
                    mapControl.SelectTool.Select(node);
                    var args = new MouseEventArgs(MouseButtons.Left, 1, 0, 0, 0);
                    var fromCoordinate = node.Geometry.Coordinate;
                    var toCoordinate = (Coordinate)fromCoordinate.Clone();
                    toCoordinate.X += 5;
                    mapControl.MoveTool.OnMouseDown(fromCoordinate, args);
                    mapControl.MoveTool.OnMouseMove(toCoordinate, args);
                    mapControl.MoveTool.OnMouseUp(toCoordinate, args);

                    // undo & redo (expect no exceptions)
                    gui.UndoRedoManager.Undo();
                    gui.UndoRedoManager.Redo();
                };

            mainWindow.ContentRendered += (s, e) => onMainWindowShown();
            WpfTestHelper.ShowModal(mainWindow);
        }
        
        [Test]
        [Category(TestCategory.DataAccess)]
        [Category(TestCategory.VerySlow)]
        [Category(TestCategory.BackwardCompatibility)]
        [Category(TestCategory.WorkInProgress)]
        public void OpenRolfModelModifyAndExpectOutputToBeCleared()
        {
            Action onMainWindowShown = null;
            var mainWindow = (Window)gui.MainWindow;

            onMainWindowShown =
                () =>
                {
                    // open rolf's model
                    var projectPath = TestHelper.GetTestFilePath("j03_18008_run_v064.dsproj");
                    var projectFileName = TestHelper.CopyProjectToLocalDirectory(projectPath);

                    gui.Application.CloseProject();
                    gui.CommandHandler.OpenProject(projectFileName);

                    // get models
                    var rtcModel = (RealTimeControlModel)gui.Application.Project.RootFolder.Models.First();
                    var flowModel = (WaterFlowModel1D)rtcModel.ControlledModels.First();
                    var network = flowModel.Network;

                    //assert we have coverages
                    var coverage = flowModel.OutputFunctions.First();
                    Assert.Greater(coverage.Arguments[0].Values.Count, 0);
                    Assert.Greater(rtcModel.OutputFeatureCoverages.Count(), 0);

                    // open network editor
                    gui.CommandHandler.OpenView(network);
                    var networkEditor = (ProjectItemMapView)gui.DocumentViews.ActiveView;
                    var mapControl = networkEditor.MapView.MapControl;

                    // move node
                    var node = network.Nodes[0];
                    mapControl.SelectTool.Select(node);
                    var args = new MouseEventArgs(MouseButtons.Left, 1, 0, 0, 0);
                    var fromCoordinate = node.Geometry.Coordinate;
                    var toCoordinate = (Coordinate)fromCoordinate.Clone();
                    toCoordinate.X += 5;
                    mapControl.MoveTool.OnMouseDown(fromCoordinate, args);
                    mapControl.MoveTool.OnMouseMove(toCoordinate, args);
                    mapControl.MoveTool.OnMouseUp(toCoordinate, args);

                    // assert coverages have been removed
                    Assert.AreEqual(0, coverage.Arguments[0].Values.Count);
                    Assert.AreEqual(0, rtcModel.OutputFeatureCoverages.Count());
                };

            WpfTestHelper.ShowModal(mainWindow, onMainWindowShown);
        }

        [SetUp]
        public void SetUp()
        {
            gui = new DeltaShellGui();
            app = gui.Application;

            app.Plugins.Add(new SharpMapGisApplicationPlugin());
            app.Plugins.Add(new CommonToolsApplicationPlugin());
            app.Plugins.Add(new NetworkEditorApplicationPlugin());
            app.Plugins.Add(new WaterFlowModel1DApplicationPlugin());
            app.Plugins.Add(new RealTimeControlApplicationPlugin());
            app.Plugins.Add(new NHibernateDaoApplicationPlugin());
            app.Plugins.Add(new NetCdfApplicationPlugin());

            gui.Plugins.Add(new ProjectExplorerGuiPlugin());
            gui.Plugins.Add(new SharpMapGisGuiPlugin());
            gui.Plugins.Add(new CommonToolsGuiPlugin());
            gui.Plugins.Add(new NetworkEditorGuiPlugin());
            gui.Plugins.Add(new WaterFlowModel1DGuiPlugin());
            gui.Plugins.Add(new RealTimeControlGuiPlugin());

            
            gui.Run();
        }

        [TearDown]
        public void TearDown()
        {
            if( gui != null )
                gui.Dispose();
        }

        private void GetSimpleRealTimeControlModelWithOutputFileFunctionStore(out RealTimeControlOutputFileFunctionStore outputFunctionStore, out RealTimeControlModel rtcModel)
        {
            var testFilePath = TestHelper.GetTestFilePath(@"RtcOutput\" + RealTimeControlModel.OutputFileName);

            // create flow1d model
            var observationPoint = new ObservationPoint() { Name = "Near pipe", Geometry = new Point(new Coordinate(10, 0)) };
            var from = new HydroNode() { Geometry = new Point(new Coordinate(0, 0)) };
            var to = new HydroNode() { Geometry = new Point(new Coordinate(100, 0)) };
            var network = new HydroNetwork
            {
                Branches = { new Channel { BranchFeatures = { observationPoint }, Source = @from, Target = to } },
                Nodes = { @from, to }
            };
            var flowModel = new WaterFlowModel1D { Network = network };

            // create RTC model
            var input = new Input();
            rtcModel = new RealTimeControlModel { ControlGroups = { new ControlGroup { Inputs = { input } } } };

            // create and add to HydroModel
            var hydroModel = new HydroModel { Activities = { rtcModel } };
            hydroModel.Region.SubRegions.Add(flowModel.Region);
            hydroModel.Activities.Add(flowModel);

            gui.Application.Project.RootFolder.Add(hydroModel);

            // attach models to each other
            var source = rtcModel.GetDataItemByValue(input);
            var target = flowModel.GetChildDataItems(observationPoint).First();

            // link
            target.LinkTo(source);

            // Connect output
            TypeUtils.CallPrivateMethod(rtcModel, "ReconnectOutputFiles", new[] { testFilePath });

            outputFunctionStore = rtcModel.OutputFileFunctionStore;
            Assert.NotNull(outputFunctionStore);
            Assert.IsTrue(outputFunctionStore.Functions.Any());
        }
    }
}