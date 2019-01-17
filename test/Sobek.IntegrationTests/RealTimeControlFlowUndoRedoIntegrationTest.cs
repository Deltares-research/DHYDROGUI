using System;
using System.Linq;
using System.Windows;
using DelftTools.Hydro;
using DelftTools.Hydro.Helpers;
using DelftTools.Hydro.Structures;
using DelftTools.Shell.Core;
using DelftTools.Shell.Core.Workflow.DataItems;
using DelftTools.TestUtils;
using DelftTools.Utils.Editing;
using DelftTools.Utils.Reflection;
using DeltaShell.Gui;
using DeltaShell.IntegrationTestUtils;
using DeltaShell.Plugins.CommonTools;
using DeltaShell.Plugins.CommonTools.Gui;
using DeltaShell.Plugins.Data.NHibernate;
using DeltaShell.Plugins.DelftModels.HydroModel;
using DeltaShell.Plugins.DelftModels.HydroModel.Gui;
using DeltaShell.Plugins.DelftModels.RealTimeControl;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Domain;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Gui;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Gui.Forms;
using DeltaShell.Plugins.DelftModels.RTCShapes.Shapes;
using DeltaShell.Plugins.DelftModels.WaterFlowModel;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.Gui;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.ModelApiControllers.ModelApi;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.TestUtils;
using DeltaShell.Plugins.NetCDF;
using DeltaShell.Plugins.NetworkEditor;
using DeltaShell.Plugins.NetworkEditor.Gui;
using DeltaShell.Plugins.SharpMapGis;
using DeltaShell.Plugins.SharpMapGis.Gui;
using DeltaShell.Plugins.SharpMapGis.Gui.Forms;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using Netron.GraphLib;
using NUnit.Framework;
using Point = NetTopologySuite.Geometries.Point;

namespace Sobek.IntegrationTests
{
    [TestFixture]
    [Category(TestCategory.WindowsForms)]
    [Category(TestCategory.UndoRedo)]
    [Category(TestCategory.WorkInProgress)] // Remove from work in progress when re-adding UndoRedo support
    public class RealTimeControlFlowUndoRedoIntegrationTest
    {
        private DeltaShellGui gui;
        private Project project;
        private Window mainWindow;
        private RealTimeControlModel rtcModel;
        private WaterFlowModel1D model;
        private HydroModel hydroModel;

        private Action onBeforeUndoRedoOn;
        private Action onMainWindowShown;

        [SetUp]
        public void SetUp()
        {
            gui = new DeltaShellGui();
            var app = gui.Application;

            app.Plugins.Add(new CommonToolsApplicationPlugin());
            app.Plugins.Add(new SharpMapGisApplicationPlugin());
            app.Plugins.Add(new NHibernateDaoApplicationPlugin());
            app.Plugins.Add(new NetworkEditorApplicationPlugin());
            app.Plugins.Add(new WaterFlowModel1DApplicationPlugin());
            app.Plugins.Add(new RealTimeControlApplicationPlugin());
            app.Plugins.Add(new NetCdfApplicationPlugin());
            app.Plugins.Add(new HydroModelApplicationPlugin());

            gui.Plugins.Add(new CommonToolsGuiPlugin());
            gui.Plugins.Add(new SharpMapGisGuiPlugin());
            gui.Plugins.Add(new NetworkEditorGuiPlugin());
            gui.Plugins.Add(new WaterFlowModel1DGuiPlugin());
            gui.Plugins.Add(new RealTimeControlGuiPlugin());
            gui.Plugins.Add(new HydroModelGuiPlugin());
            
            gui.Run();

            project = app.Project;

            // add data
            var builder = new HydroModelBuilder();
            hydroModel = builder.BuildModel(ModelGroup.SobekModels);

            // remove anything but flow and RTC
            var activitiesToRemove = hydroModel.Activities.Where(a => !(a is WaterFlowModel1D) && !(a is RealTimeControlModel)).ToList();
            foreach(var activity in activitiesToRemove)
            {
                hydroModel.Activities.Remove(activity);
            }

            rtcModel = hydroModel.Activities.OfType<RealTimeControlModel>().First();
            model = hydroModel.Activities.OfType<WaterFlowModel1D>().First();

            project.RootFolder.Add(hydroModel);
            // show gui main window
            mainWindow = (Window)gui.MainWindow;

            onBeforeUndoRedoOn = () => { };

            // wait until gui starts
            mainWindow.IsVisibleChanged += delegate
            {
                if (!mainWindow.IsVisible)
                {
                    return;
                }

                onBeforeUndoRedoOn();

                gui.UndoRedoManager.TrackChanges = true;

                onMainWindowShown();
            };
        }

        [TearDown]
        public void TearDown()
        {
            gui.UndoRedoManager.TrackChanges = false;
            gui.Dispose();
            onMainWindowShown = null;
            LogHelper.ResetLogging();
        }

        [Test]
        [Category(TestCategory.Slow)]
        public void AddControlGroup()
        {
            onMainWindowShown = () =>
            {
                var numDataItemsBefore = rtcModel.DataItems.Count;

                rtcModel.ControlGroups.Add(new ControlGroup());
                Assert.AreEqual(numDataItemsBefore + 1, rtcModel.DataItems.Count);

                gui.UndoRedoManager.Undo();

                Assert.AreEqual(0, rtcModel.ControlGroups.Count());
                Assert.AreEqual(numDataItemsBefore, rtcModel.DataItems.Count);

                gui.UndoRedoManager.Redo();

                Assert.AreEqual(numDataItemsBefore+1, rtcModel.DataItems.Count);
                Assert.AreEqual(1, rtcModel.ControlGroups.Count());
            };

            WpfTestHelper.ShowModal(mainWindow);
        }

        [Test]
        public void LinkRtcOutputItemUsingGuiEditorAndLinkAgainTools7388()
        {
            onMainWindowShown = () =>
            {
                gui.UndoRedoManager.TrackChanges = false;

                // set up flow
                var branchPoints = new[] {new Point(0, 0), new Point(50, 0), new Point(50, 50)};
                HydroNetworkHelper.AddSnakeHydroNetwork(model.Network, branchPoints);

                var branch1 = model.Network.Branches[0];
                var lateral1 = new LateralSource { Name = "lat1", Branch = branch1, Chainage = 30, Geometry = new Point(30, 0) };
                branch1.BranchFeatures.Add(lateral1);

                var branch2 = model.Network.Branches[1];
                var lateral2 = new LateralSource {Name = "lat2", Branch = branch2, Chainage = 30, Geometry = new Point(50, 30)};
                branch2.BranchFeatures.Add(lateral2);
                
                // set up flow data items
                var networkDataItem = model.GetDataItemByValue(model.Network);
                var target1 = new DataItem
                {
                    Role = DataItemRole.Output,
                    ValueConverter = new WaterFlowModelBranchFeatureValueConverter(model, lateral1, "Discharge",
                        QuantityType.Discharge, ElementSet.Laterals, DataItemRole.Output, "m3/s"),
                    ValueType = typeof(double),
                    Parent = networkDataItem
                };
                
                var target2 = new DataItem
                {
                    Role = DataItemRole.Input,
                    ValueConverter = new WaterFlowModelBranchFeatureValueConverter(model, lateral2, "Discharge", 
                        QuantityType.Discharge, ElementSet.Laterals, DataItemRole.Input, "m3/s"),
                    ValueType = typeof(double),
                    Parent = networkDataItem
                };

                // set up rtc
                var input = new Input();
                var output = new Output();
                var invertorRule = new FactorRule { Inputs = { input }, Outputs = { output } };
                var controlGroup = new ControlGroup { Inputs = { input }, Outputs = { output }, Rules = { invertorRule } };
                rtcModel.ControlGroups.Add(controlGroup);

                // open view
                gui.CommandHandler.OpenView(controlGroup);

                var cgView = (ControlGroupGraphView)gui.DocumentViews.ActiveView;
                var editor = cgView.ControlGroupEditor;
                var inputShape = editor.GraphControl.GetShapes<InputItemShape>().First();
                var outputShape = editor.GraphControl.GetShapes<OutputItemShape>().First();

                // start tracking
                gui.UndoRedoManager.TrackChanges = true;

                // actions
                editor.Link(inputShape, target1); 
                editor.Link(outputShape, target2);

                Assert.AreEqual(2, gui.UndoRedoManager.UndoStack.Count(), "#undo");
                while (gui.UndoRedoManager.CanUndo)
                {
                    gui.UndoRedoManager.Undo();
                }

                Assert.AreEqual(2, gui.UndoRedoManager.RedoStack.Count(), "#redo");
                while (gui.UndoRedoManager.CanRedo)
                {
                    gui.UndoRedoManager.Redo();
                }
                Assert.AreEqual(2, gui.UndoRedoManager.UndoStack.Count(), "#undo end");
            };
        
            WpfTestHelper.ShowModal(mainWindow);
        }

        [Test]
        public void ConnectInputWithRule()
        {
            onMainWindowShown = () =>
            {
                gui.UndoRedoManager.TrackChanges = false;

                // set up flow
                HydroNetworkHelper.AddSnakeHydroNetwork(model.Network, new[] { new Point(0, 0), new Point(50, 0) });

                var branch = model.Network.Branches[0];
                var lateral = new LateralSource { Name = "lat1", Branch = branch, Chainage = 30, Geometry = new Point(30, 0) };
                branch.BranchFeatures.Add(lateral);
                
                // set up flow data items
                var networkDataItem = model.GetDataItemByValue(model.Network);
                var target1 = new DataItem
                {
                    Role = DataItemRole.Output,
                    ValueConverter = new WaterFlowModelBranchFeatureValueConverter(model, lateral, "Discharge",
                        QuantityType.Discharge, ElementSet.Laterals, DataItemRole.Output, "m3/s"),
                    ValueType = typeof(double),
                    Parent = networkDataItem
                };
                
                // set up rtc
                var input = new Input();
                var invertorRule = new FactorRule();
                var controlGroup = new ControlGroup { Inputs = { input }, Rules = { invertorRule } };
                rtcModel.ControlGroups.Add(controlGroup);

                // open view
                gui.CommandHandler.OpenView(controlGroup);

                var cgView = (ControlGroupGraphView)gui.DocumentViews.ActiveView;
                var editor = cgView.ControlGroupEditor;
                var inputShape = editor.GraphControl.GetShapes<InputItemShape>().First();
                
                // link
                editor.Link(inputShape, target1);

                gui.UndoRedoManager.TrackChanges = true;

                invertorRule.Inputs.Add(input);

                Assert.AreEqual(1, GetNumberOfPaintedConnections(editor), "#links before");

                gui.UndoRedoManager.Undo();

                Assert.AreEqual(0, GetNumberOfPaintedConnections(editor), "#links after undo");

                gui.UndoRedoManager.Redo();

                Assert.AreEqual(1, GetNumberOfPaintedConnections(editor), "#links after redo");
            };

            WpfTestHelper.ShowModal(mainWindow);
        }

        /// <summary>
        /// Hack into NetronGraphLib to make sure we get the number of painted Connections, not those reachable through api
        /// because they are not always in sync
        /// </summary>
        /// <param name="editor"></param>
        /// <returns></returns>
        private static int GetNumberOfPaintedConnections(ControlGroupEditor editor)
        {
            var paintables = TypeUtils.GetField<GraphAbstract, EntityCollection>(editor.GraphControl.NetronGraph.Abstract, "paintables");
            return paintables.Count - editor.GraphControl.GetShapes<ShapeBase>().Count();
        }

        [Test]
        public void ConnectConditionWithRuleIsTrackedByUndoRedo()
        {
            onMainWindowShown = () =>
            {
                gui.UndoRedoManager.TrackChanges = false;
                
                // set up rtc
                var condition = new StandardCondition();
                var rule = new FactorRule();
                var controlGroup = new ControlGroup {Conditions = {condition}, Rules = {rule}};
                rtcModel.ControlGroups.Add(controlGroup);

                // open view
                gui.CommandHandler.OpenView(controlGroup);
                
                gui.UndoRedoManager.TrackChanges = true;

                condition.TrueOutputs.Add(rule);
                
                Assert.AreEqual(1, gui.UndoRedoManager.UndoStack.Count());
                
                gui.UndoRedoManager.Undo();
                Assert.AreEqual(0, condition.TrueOutputs.Count, "#after undo");

                gui.UndoRedoManager.Redo();
                Assert.AreEqual(1, condition.TrueOutputs.Count, "#after redo");
            };

            WpfTestHelper.ShowModal(mainWindow);
        }

        [Test]
        public void ModifyTimeSeriesOfTimeRuleIsTrackedByUndoRedo()
        {
            onMainWindowShown = () =>
            {
                gui.UndoRedoManager.TrackChanges = false;

                // set up rtc
                var rule = new TimeRule();
                var controlGroup = new ControlGroup { Rules = { rule } };
                rtcModel.ControlGroups.Add(controlGroup);

                var series = rule.TimeSeries; //trigger lazyness

                gui.UndoRedoManager.TrackChanges = true;

                series.BeginEdit(new DefaultEditAction("Setting value"));
                series[new DateTime(2001, 1, 1)] = 5.0;
                series.EndEdit();

                Assert.AreEqual(1, gui.UndoRedoManager.UndoStack.Count());

                gui.UndoRedoManager.Undo();

                Assert.AreEqual(0, series.Time.Values.Count);

                gui.UndoRedoManager.Redo();

                Assert.AreEqual(1, series.Time.Values.Count);
            };

            WpfTestHelper.ShowModal(mainWindow);
        }

        [Test]
        public void UndoConnectInputToConditionRefreshesPaintedConnections()
        {
            onMainWindowShown = () =>
            {
                gui.UndoRedoManager.TrackChanges = false;

                // set up rtc
                var input = new Input();
                var condition = new StandardCondition();
                var controlGroup = new ControlGroup { Conditions = { condition }, Inputs = { input } };
                rtcModel.ControlGroups.Add(controlGroup);

                // open view
                gui.CommandHandler.OpenView(controlGroup);
                var cgView = (ControlGroupGraphView)gui.DocumentViews.ActiveView;
                var editor = cgView.ControlGroupEditor;
                
                gui.UndoRedoManager.TrackChanges = true;

                condition.Input = input;

                Assert.AreEqual(1, gui.UndoRedoManager.UndoStack.Count());

                gui.UndoRedoManager.Undo();
                Assert.IsNull(condition.Input, "#after undo");
                Assert.AreEqual(0, GetNumberOfPaintedConnections(editor));

                gui.UndoRedoManager.Redo();
                Assert.IsNotNull(condition.Input, "#after undo");
                Assert.AreEqual(1, GetNumberOfPaintedConnections(editor));
            };

            WpfTestHelper.ShowModal(mainWindow);
        }

        [Test]
        [Category(TestCategory.Slow)]
        public void LinkInputCrashesWithPredefinedControlGroup()
        {
            onMainWindowShown = () =>
            {
                // open network editor
                gui.CommandHandler.OpenView(model, typeof(ProjectItemMapView));

                // add branch
                var networkEditor = (ProjectItemMapView)gui.DocumentViews.ActiveView;
                var weirLayer = networkEditor.MapView.GetLayerForData(model.Network.Weirs);
                var branchLayer = networkEditor.MapView.GetLayerForData(model.Network.Channels);
                var branchGeom = new LineString(new [] {new Coordinate(0, 0), new Coordinate(50, 0)});
                branchLayer.DataSource.Add(branchGeom);

                // add weir
                var weir = weirLayer.DataSource.Add(new Point(30, 0));
                
                // set up rtc (add prefab pid rule)
                var controlGroup = RealTimeControlModelHelper.CreateStandardControlGroup(
                    RealTimeControlModelHelper.StandardControlGroups.Skip(1).First());
                rtcModel.ControlGroups.Add(controlGroup);

                // open view
                gui.CommandHandler.OpenView(controlGroup);
                
                // do something in network and undo (add branch)
                branchLayer.DataSource.Add(new LineString(new [] { new Coordinate(50, 0), new Coordinate(100, 0) }));
                gui.UndoRedoManager.Undo(); // undo add branch
                
                // link
                var cgView = (ControlGroupGraphView)gui.DocumentViews.ActiveView;
                var editor = cgView.ControlGroupEditor;
                var inputShape = editor.GraphControl.GetShapes<InputItemShape>().Skip(1).First(); //input to condition
                editor.Link(inputShape, model.GetChildDataItems(weir).First());
            };

            WpfTestHelper.ShowModal(mainWindow);
        }

        [Test]
        [Category(TestCategory.Slow)]
        public void RunSaveLoadRtcModelAddOutputAndLink()
        {
            onMainWindowShown =
                () =>
                    {
                        // set up models
                        WaterFlowModel1DDemoModelTestHelper.ConfigureModelAsDemoModel(model);
                        
                        // add 2 weirs
                        var branch = model.Network.Branches[0];
                        var weir1 = new Weir {Name = "w1", Branch = branch, Chainage = 15, Geometry = new Point(15, 0) };
                        var weir2 = new Weir {Name = "w2", Branch = branch, Chainage = 35, Geometry = new Point(35, 0) };
                        HydroNetworkHelper.AddStructureToExistingCompositeStructureOrToANewOne(weir1, branch);
                        HydroNetworkHelper.AddStructureToExistingCompositeStructureOrToANewOne(weir2, branch);

                        // set up rtc
                        var cg = new ControlGroup();
                        rtcModel.ControlGroups.Add(cg);
                        var timeRule = new TimeRule();
                        timeRule.TimeSeries[new DateTime(2001, 1, 1)] = 5.0;
                        var output = new Output();
                        cg.Rules.Add(timeRule);
                        cg.Outputs.Add(output);
                        timeRule.Outputs.Add(output);

                        // link output
                        var outputDataItem = rtcModel.GetDataItemByValue(output);
                        var weirDataItem = model.GetChildDataItems(weir1).First(di => (di.Role & DataItemRole.Input)>0);
                        weirDataItem.LinkTo(outputDataItem);
                        
                        // run model
                        gui.Application.RunActivity(hydroModel);

                        // save & reload project
                        var path = "tt.dsproj";
                        gui.Application.SaveProjectAs(path);
                        gui.Application.CloseProject();
                        gui.Application.OpenProject(path);

                        gui.UndoRedoManager.TrackChanges = true;

                        // add another output
                        var output2 = new Output();
                        cg.Outputs.Add(output2);

                        // link output
                        var output2DataItem = rtcModel.GetDataItemByValue(output2);
                        var flowDataItem2 = model.GetChildDataItems(weir2).First(di => (di.Role & DataItemRole.Input) > 0);
                        flowDataItem2.LinkTo(output2DataItem); //expect no exception here

                        gui.Application.CloseProject();
                    };

            WpfTestHelper.ShowModal(mainWindow);
        }

        [TestFixture]
        [Category(TestCategory.Integration)]
        [Category(TestCategory.UndoRedo)]
        [Category(TestCategory.Slow)]
        public class ShotgunTest
        {
            [Test]
            public void TestRtcAssemblyForSideEffectsInSetters()
            {
                LogHelper.ConfigureLogging();
                var pointType = typeof(Point); //force type load
                UndoRedoSideEffectTester.TestAssembly(typeof(RealTimeControlModel).Assembly);
            }
        }
    }
}