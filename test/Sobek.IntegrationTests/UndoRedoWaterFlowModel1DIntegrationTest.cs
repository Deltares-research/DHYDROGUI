using System;
using System.Linq;
using System.Windows;
using System.Windows.Forms;
using DelftTools.Hydro;
using DelftTools.Hydro.CrossSections;
using DelftTools.Shell.Core;
using DelftTools.Shell.Core.Workflow;
using DelftTools.Shell.Core.Workflow.DataItems;
using DelftTools.TestUtils;
using DelftTools.Utils.Editing;
using DeltaShell.Gui;
using DeltaShell.NGHS.IO.DataObjects.Model1D;
using DeltaShell.Plugins.CommonTools;
using DeltaShell.Plugins.CommonTools.Gui;
using DeltaShell.Plugins.Data.NHibernate;
using DeltaShell.Plugins.DelftModels.RealTimeControl;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Gui;
using DeltaShell.Plugins.DelftModels.WaterFlowModel;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.Gui;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.TestUtils;
using DeltaShell.Plugins.NetCDF;
using DeltaShell.Plugins.NetworkEditor;
using DeltaShell.Plugins.NetworkEditor.Gui;
using DeltaShell.Plugins.ProjectExplorer;
using DeltaShell.Plugins.SharpMapGis;
using DeltaShell.Plugins.SharpMapGis.Gui;
using DeltaShell.Plugins.SharpMapGis.Gui.Forms;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using NetTopologySuite.Extensions.Coverages;
using NUnit.Framework;
using SharpTestsEx;
using Point = NetTopologySuite.Geometries.Point;

namespace Sobek.IntegrationTests
{
    [TestFixture]
    [Category(TestCategory.WindowsForms)]
    [Category(TestCategory.UndoRedo)]
    public class UndoRedoWaterFlowModel1DIntegrationTest
    {
        private DeltaShellGui gui;
        private Project project;
        private WaterFlowModel1D model;
        private Window mainWindow;

        private Action onBeforeUndoRedoOn;
        private Action onMainWindowShown;
        private Action mainWindowShown;

        [SetUp]
        public void SetUp()
        {
            gui = new DeltaShellGui();
            var app = gui.Application;

            app.Plugins.Add(new CommonToolsApplicationPlugin());
            app.Plugins.Add(new SharpMapGisApplicationPlugin());
            app.Plugins.Add(new NetworkEditorApplicationPlugin());
            app.Plugins.Add(new NHibernateDaoApplicationPlugin());
            app.Plugins.Add(new NetCdfApplicationPlugin());
            app.Plugins.Add(new WaterFlowModel1DApplicationPlugin());
            app.Plugins.Add(new RealTimeControlApplicationPlugin());

            gui.Plugins.Add(new CommonToolsGuiPlugin());
            gui.Plugins.Add(new ProjectExplorerGuiPlugin());
            gui.Plugins.Add(new SharpMapGisGuiPlugin());
            gui.Plugins.Add(new NetworkEditorGuiPlugin());
            gui.Plugins.Add(new WaterFlowModel1DGuiPlugin());
            gui.Plugins.Add(new RealTimeControlGuiPlugin());

            
            gui.Run();

            project = app.Project;

            // add data
            model = new WaterFlowModel1D();
            project.RootFolder.Add(model);

            // show gui main window
            mainWindow = (Window)gui.MainWindow;

            onBeforeUndoRedoOn = () => { };

            // wait until gui starts
            mainWindowShown = () =>
            {
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
            //onBeforeUndoRedoOn = () => { };
            LogHelper.ResetLogging();
        }
        
        [Test]
        public void UndoDeleteOfModelAndSave()
        {
            onMainWindowShown = () =>
            {
                // delete model
                gui.CommandHandler.DeleteProjectItem(model);

                // undo delete
                gui.UndoRedoManager.Undo();

                // save
                gui.Application.SaveProjectAs("undo_delete.dsproj");
            };
            WpfTestHelper.ShowModal(mainWindow, mainWindowShown);
        }


        [Test]
        [Category(TestCategory.DataAccess)]
        [Category(TestCategory.Slow)]
        public void UndoUseAsInitialStateWorksFineTools9040()
        {
            var flowModel = WaterFlowModel1DDemoModelTestHelper.CreateModelWithDemoNetwork();

            onBeforeUndoRedoOn =
                () =>
                    {
                        project.RootFolder.Add(flowModel);

                        // run flow & write restart
                        flowModel.WriteRestart = true;
                        flowModel.StopTime = flowModel.StartTime.AddHours(24.0);
                        ActivityRunner.RunActivity(flowModel);
                        Assert.AreEqual(ActivityStatus.Cleaned, flowModel.Status);
                    };

            onMainWindowShown =
                () =>
                    {
                        // mimic what 'use as initial state' does:
                        flowModel.RestartInput =
                            (FileBasedRestartState) flowModel.GetRestartOutputStates().Last().Clone();

                        Assert.IsFalse(flowModel.RestartInput.IsEmpty);
                        Assert.IsTrue(flowModel.RestartInput.Path.Contains("flow"));

                        // undo it
                        gui.UndoRedoManager.Undo();

                        Assert.IsTrue(flowModel.RestartInput.Path.Contains("empty"));
                        Assert.IsTrue(flowModel.RestartInput.IsEmpty);

                        // redo it
                        gui.UndoRedoManager.Redo();

                        Assert.IsFalse(flowModel.RestartInput.IsEmpty);
                        Assert.IsTrue(flowModel.RestartInput.Path.Contains("flow"));

                        // undo it
                        gui.UndoRedoManager.Undo();

                        Assert.IsTrue(flowModel.RestartInput.IsEmpty);
                        Assert.IsTrue(flowModel.RestartInput.Path.Contains("empty"));

                    };

            WpfTestHelper.ShowModal(mainWindow, mainWindowShown);
        }

        [Test]
        [Category(TestCategory.BackwardCompatibility)]
        [Category(TestCategory.WorkInProgress)]
        public void UndoMoveNodeAndDeleteBranchFromLoadedModel()
        {
            onMainWindowShown =
                () =>
                    {
                        // close current project and open rijntakken model
                        gui.Application.CloseProject();
                        var path = TestHelper.GetTestFilePath("RTMJZ_Import.dsproj");
                        path = TestHelper.CopyProjectToLocalDirectory(path);
                        gui.Application.OpenProject(path);

                        // get flow model
                        var rtc = (RealTimeControlModel)gui.Application.Project.RootFolder.Models.First();
                        var flow = (WaterFlowModel1D) rtc.ControlledModels.First();

                        // open view for network
                        gui.CommandHandler.OpenView(flow.Network, typeof(ProjectItemMapView));
                        var editor = (ProjectItemMapView)gui.DocumentViews.ActiveView;
                        editor.MapView.Map.ZoomToExtents();

                        // select & move node
                        var node = flow.Network.Nodes.First(n => n.Name == "N_002");
                        var mapControl = editor.MapView.MapControl;
                        mapControl.SelectTool.Select(node);
                        var mouseArgs = new MouseEventArgs(MouseButtons.Left, 0, 0, 0, 0);
                        var newPosition = new Coordinate(node.Geometry.Coordinate.X + 10, node.Geometry.Coordinate.Y);
                        var moveTool = mapControl.MoveTool;
                        moveTool.OnMouseDown(node.Geometry.Coordinate, mouseArgs);
                        moveTool.OnMouseMove(newPosition, mouseArgs);
                        moveTool.OnMouseUp(newPosition, mouseArgs);

                        // select & delete branch
                        var branch = flow.Network.Branches.First(n => n.Name == "R_001");
                        mapControl.SelectTool.Select(branch);
                        mapControl.DeleteTool.DeleteSelection();
                        
                        // undo last two actions
                        gui.UndoRedoManager.Undo();
                        gui.UndoRedoManager.Undo();
                    };
            WpfTestHelper.ShowModal(mainWindow, mainWindowShown);
        }

        [Test]
        public void UndoRedoDeleteOfModelAndSave()
        {
            onMainWindowShown = () =>
            {
                // delete model
                gui.CommandHandler.DeleteProjectItem(model);

                // undo delete
                gui.UndoRedoManager.Undo();

                // redo delete
                gui.UndoRedoManager.Redo();

                // save
                gui.Application.SaveProjectAs("undo_delete.dsproj");
            };
            WpfTestHelper.ShowModal(mainWindow, mainWindowShown);
        }

        [Test]
        public void CopyPasteLinkedModelUnlinkAndUndoRedoShouldNotCrash()
        {
            onMainWindowShown = () =>
            {
                gui.UndoRedoManager.TrackChanges = false;

                // add network
                var network = new HydroNetwork();
                var networkDataItem = new DataItem(network);
                project.RootFolder.Items.Add(networkDataItem);

                // link model network to project network
                var networkDataItemInModel = model.GetDataItemByValue(model.Network);

                project.BeginEdit("Link");
                networkDataItemInModel.LinkTo(networkDataItem);
                project.EndEdit();

                gui.UndoRedoManager.TrackChanges = true;

                // copy/paste model
                gui.CopyPasteHandler.Copy(model);
                gui.CopyPasteHandler.Paste(project, project.RootFolder);

                var modelCopy = (WaterFlowModel1D)project.RootFolder.Models.First(m => !ReferenceEquals(m, model));

                // unlink new model
                project.BeginEdit("Unlink");
                var networkDataItemCopy = modelCopy.GetDataItemByValue(modelCopy.Network);
                networkDataItemCopy.Unlink();
                project.EndEdit();

                // undo all
                while (gui.UndoRedoManager.UndoStack.Any())
                {
                    gui.UndoRedoManager.Undo();
                }

                // redo all - results in exception
                while (gui.UndoRedoManager.RedoStack.Any())
                {
                    gui.UndoRedoManager.Redo();
                }
            };

            WpfTestHelper.ShowModal(mainWindow, mainWindowShown);
        }

        [Test]
        public void CopyPasteLinkedModelAndUndoRedoShouldNotCrash()
        {
            onMainWindowShown = () =>
            {
                gui.UndoRedoManager.TrackChanges = false;

                // add network
                var network = new HydroNetwork();
                var networkDataItem = new DataItem(network);
                project.RootFolder.Items.Add(networkDataItem);

                // link model network to project network
                var networkDataItemInModel = model.GetDataItemByValue(model.Network);
                project.BeginEdit("Link");
                networkDataItemInModel.LinkTo(networkDataItem);
                project.EndEdit();

                gui.UndoRedoManager.TrackChanges = true;

                // copy/paste model
                gui.CopyPasteHandler.Copy(model);
                gui.CopyPasteHandler.Paste(project, project.RootFolder);

                // undo
                Assert.AreEqual(1, gui.UndoRedoManager.UndoStack.Count());
                gui.UndoRedoManager.Undo();

                // redo
                gui.UndoRedoManager.Redo();
            };

            WpfTestHelper.ShowModal(mainWindow, mainWindowShown);
        }

        [Test]
        [Category(TestCategory.Slow)]
        [Ignore("Flow1D (cf_dll) kernel is not in dimr set any more")]
        public void AddNewModelAddBranchAddCrossSectionGenerateGridUndoAllRedoAllAndRunShouldNotCrash()
        {
            onBeforeUndoRedoOn = () => project.RootFolder.Items.Remove(model);

            onMainWindowShown = () =>
            {
                // create network
                var node1 = new HydroNode { Name = "node1", Geometry = new Point(0, 0) };
                var node2 = new HydroNode { Name = "node2", Geometry = new Point(100, 0) };

                var branch1 = new Channel("branch1", node1, node2) { Geometry = new LineString(new [] { new Coordinate(0, 0), new Coordinate(100, 0) }) };

                var crs1Definition = new CrossSectionDefinitionXYZ { Name = "crs1def", Geometry = new LineString(new [] { new Coordinate(50, -10, 0), new Coordinate(50, 0, -2), new Coordinate(50, 10, 0) }) };
                var crs1 = new CrossSection(crs1Definition) { Name = "crs1", Branch = branch1 };
                branch1.BranchFeatures.Add(crs1);

                var network = new HydroNetwork { Nodes = { node1, node2 }, Branches = { branch1 } };

                ModelTestHelper.RefreshCrossSectionDefinitionSectionWidths(network);

                // add model
                model = new WaterFlowModel1D { Network = network };
                project.RootFolder.Items.Add(model);

                // generate grid
                var grid = model.NetworkDiscretization;
                grid[new NetworkLocation(branch1, 0)] = 0.0;
                grid[new NetworkLocation(branch1, 50)] = 0.0;
                grid[new NetworkLocation(branch1, 100)] = 0.0;

                var validationReport = model.Validate();
                validationReport.ErrorCount.Should().Be.EqualTo(0);

                // run
                gui.UndoRedoManager.TrackChanges = false;
                model.Initialize();
                model.Execute();
                Assert.AreEqual(ActivityStatus.Executed, model.Status, "check 1");
                model.Cleanup();
                
                gui.UndoRedoManager.TrackChanges = true;

                // undo all
                while (gui.UndoRedoManager.UndoStack.Any())
                {
                    gui.UndoRedoManager.Undo();
                }

                // redo all
                while (gui.UndoRedoManager.RedoStack.Any())
                {
                    gui.UndoRedoManager.Redo();
                }

                // run again - EXCEPTION
                gui.UndoRedoManager.TrackChanges = false;
                model.Initialize();
                model.Execute();
                Assert.AreEqual(ActivityStatus.Executed, model.Status, "check 2");
                model.Cleanup();

                gui.UndoRedoManager.TrackChanges = true;
            };

            WpfTestHelper.ShowModal(mainWindow, mainWindowShown);
        }
        
        [Test]
        [Category(TestCategory.Slow)]
        [Ignore("Flow1D (cf_dll) kernel is not in dimr set any more")]
        public void UndoRedoAddModelAndRunShouldNotCrash()
        {
            onBeforeUndoRedoOn = () => project.RootFolder.Items.Remove(model);

            onMainWindowShown = () =>
            {
                // create network
                var node1 = new HydroNode { Name = "node1", Geometry = new Point(0, 0) };
                var node2 = new HydroNode { Name = "node2", Geometry = new Point(100, 0) };

                var branch1 = new Channel("branch1", node1, node2) { Geometry = new LineString(new [] { new Coordinate(0, 0), new Coordinate(100, 0) }) };

                var crs1Definition = new CrossSectionDefinitionXYZ { Name = "crs1def", Geometry = new LineString(new [] { new Coordinate(50, -10, 0), new Coordinate(50, 0, -2), new Coordinate(50, 10, 0) }) };
                var crs1 = new CrossSection(crs1Definition) { Name = "crs1", Branch = branch1 };
                branch1.BranchFeatures.Add(crs1);

                var network = new HydroNetwork { Nodes = { node1, node2 }, Branches = { branch1 } };

                ModelTestHelper.RefreshCrossSectionDefinitionSectionWidths(network);

                // create model model
                model = new WaterFlowModel1D { Network = network };

                // generate grid
                var grid = model.NetworkDiscretization;
                grid[new NetworkLocation(branch1, 0)] = 0.0;
                grid[new NetworkLocation(branch1, 50)] = 0.0;
                grid[new NetworkLocation(branch1, 100)] = 0.0;

                var validationReport = model.Validate();
                validationReport.ErrorCount.Should().Be.EqualTo(0);

                // add model to project
                project.RootFolder.Items.Add(model);

                // undo & redo add
                gui.UndoRedoManager.Undo();
                gui.UndoRedoManager.Redo();

                // run
                gui.UndoRedoManager.TrackChanges = false;
                model.Initialize();
                model.Execute();
                Assert.AreEqual(ActivityStatus.Executed, model.Status);
                model.Cleanup();

            };

            WpfTestHelper.ShowModal(mainWindow, mainWindowShown);
        }

        [Test]
        public void UndoChangeDiscretizationTypeShouldNotGiveNetCdfException()
        {
            onBeforeUndoRedoOn = () => project.RootFolder.Items.Remove(model);

            onMainWindowShown = () =>
            {
                // create network
                var node1 = new HydroNode { Name = "node1", Geometry = new Point(0, 0) };
                var node2 = new HydroNode { Name = "node2", Geometry = new Point(100, 0) };

                var branch1 = new Channel("branch1", node1, node2) { Geometry = new LineString(new [] { new Coordinate(0, 0), new Coordinate(100, 0) }) };

                var crs1Definition = new CrossSectionDefinitionXYZ { Name = "crs1def", Geometry = new LineString(new [] { new Coordinate(50, -10, 0), new Coordinate(50, 0, -2), new Coordinate(50, 10, 0) }) };
                var crs1 = new CrossSection(crs1Definition) { Name = "crs1", Branch = branch1 };
                branch1.BranchFeatures.Add(crs1);

                var network = new HydroNetwork { Nodes = { node1, node2 }, Branches = { branch1 } };

                // create model model
                model = new WaterFlowModel1D { Network = network };

                // generate grid
                var grid = model.NetworkDiscretization;
                grid[new NetworkLocation(branch1, 0)] = 0.0;
                grid[new NetworkLocation(branch1, 50)] = 0.0;
                grid[new NetworkLocation(branch1, 100)] = 0.0;

                // add model to project
                project.RootFolder.Items.Add(model);

                // change discretization type
                var gridTypeParameter = model.OutputSettings.EngineParameters.First(p => p.Name == Model1DParameterNames.FiniteVolumeGridType);
                gridTypeParameter.AggregationOptions = (AggregationOptions) /* WTF?!? */ (int) FiniteVolumeDiscretizationType.OnGridPoints;
                // undo: exception was generated here, has to do with FixedSize = -1 in network coverages flushed into NetCDF files
                gui.UndoRedoManager.Undo();
            };


            WpfTestHelper.ShowModal(mainWindow, mainWindowShown);
        }
    }
}