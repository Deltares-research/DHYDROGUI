using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using DelftTools.Controls.Swf;
using DelftTools.Hydro;
using DelftTools.Shell.Core;
using DelftTools.Shell.Core.Workflow;
using DelftTools.Shell.Gui;
using DelftTools.TestUtils;
using DelftTools.Utils.IO;
using DelftTools.Utils.UndoRedo;
using DeltaShell.Gui;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.DataObjects;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.Gui;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.Gui.Forms.ProjectExplorer;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.Gui.Ribbon;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.IO;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.ObservationAreas;
using DeltaShell.Plugins.NetworkEditor.Gui;
using DeltaShell.Plugins.ProjectExplorer;
using DeltaShell.Plugins.SharpMapGis;
using DeltaShell.Plugins.SharpMapGis.Gui;
using DeltaShell.Plugins.SharpMapGis.Gui.Commands.SpatialOperations;
using DeltaShell.Plugins.SharpMapGis.Gui.Forms;

using NUnit.Framework;

using Rhino.Mocks;

using SharpMap.UI.Tools;

namespace DeltaShell.Plugins.DelftModels.WaterQualityModel.Tests
{
    [TestFixture]
    public class WaterQualityModelGuiPluginTest
    {
        [Test]
        public void GetRibbonCommandHandlerReturnsWaterQualityRibbon()
        {
            MockRepository mocks = new MockRepository();
            var guiStub = mocks.Stub<IGui>();
            var applicationStub = mocks.Stub<IApplication>();
            guiStub.Application = applicationStub;

            // setup
            using(var gisPlugin = new SharpMapGisGuiPlugin())
            using (var guiPlugin = new WaterQualityModelGuiPlugin())
            {
                gisPlugin.Gui = guiStub;
                gisPlugin.InitializeSpatialOperationSetLayerView();

                // call
                var ribbonCommandHandler = guiPlugin.RibbonCommandHandler;

                // assert
                Assert.IsInstanceOf<WaterQualityRibbon>(ribbonCommandHandler);
            }
        }

        [Test]
        public void CanCopy_ForWaterQualityModel_ReturnFalse()
        {
            // setup
            var mocks = new MockRepository();
            var modelStub = mocks.Stub<WaterQualityModel>();
            mocks.ReplayAll();
            using (var guiPlugin = new WaterQualityModelGuiPlugin())
            {
                // call
                var canCopy = guiPlugin.CanCopy(modelStub);

                // assert
                Assert.IsFalse(canCopy);
            }
        }

        [Test]
        public void CanCut_ForWaterQualityModel_ReturnFalse()
        {
            // setup
            var mocks = new MockRepository();
            var modelStub = mocks.Stub<WaterQualityModel>();
            mocks.ReplayAll();
            using (var guiPlugin = new WaterQualityModelGuiPlugin())
            {
                // call
                var canCopy = guiPlugin.CanCut(modelStub);

                // assert
                Assert.IsFalse(canCopy);
            }
        }
        
        [Test]
        public void OnAddingNewProjectItemMapViewWaqMapToolsShouldBeAdded()
        {
            // setup
            using (var guiPlugin = new WaterQualityModelGuiPlugin())
            using (var projectItemMapView = new ProjectItemMapView())
            {
                // call
                guiPlugin.OnViewAdded(projectItemMapView);

                // assert
                var addLoadTool = projectItemMapView.MapView.MapControl.GetToolByName(WaterQualityModelGuiPlugin.AddWaterQualityLoadMapToolName);
                Assert.IsInstanceOf<NewPointFeatureTool<WaterQualityLoad>>(addLoadTool);

                var addObservationPointTool = projectItemMapView.MapView.MapControl.GetToolByName(WaterQualityModelGuiPlugin.AddObservationPointMapToolName);
                Assert.IsInstanceOf<NewPointFeatureTool<WaterQualityObservationPoint>>(addObservationPointTool);
            }
        }

        [Test]
        public void GetProjectTreeViewNodePresentersTest()
        {
            // setup
            var guiPlugin = new WaterQualityModelGuiPlugin();

            // call
            var nodePresenters = guiPlugin.GetProjectTreeViewNodePresenters().ToArray();

            // assert
            Assert.IsTrue(nodePresenters.Any(np => np is SubstanceProcessLibraryNodePresenter));
            Assert.IsTrue(nodePresenters.Any(np => np is WaterQualityFunctionDataWrapperNodePresenter));
            Assert.IsTrue(nodePresenters.Any(np => np is WaterQualityFunctionWrapperNodePresenter));
            Assert.IsTrue(nodePresenters.Any(np => np is WaterQualityObservationVariableOutputNodePresenter));
            Assert.IsTrue(nodePresenters.Any(np => np is WaterQualityModelNodePresenter));
            Assert.IsTrue(nodePresenters.Any(np => np is DataTableManagerNodePresenter));
        }

        [Test]
        public void TestAllOperationsAreExcludedButSetLabel()
        {
            MockRepository mocks = new MockRepository();
            var guiStub = mocks.Stub<IGui>();
            var applicationStub = mocks.Stub<IApplication>();
            guiStub.Application = applicationStub;

            // setup
            using (var gisPlugin = new SharpMapGisGuiPlugin())
            using (var guiPlugin = new WaterQualityModelGuiPlugin())
            {
                gisPlugin.Gui = guiStub;
                gisPlugin.InitializeSpatialOperationSetLayerView();

                // initialize the ribbon
                var ribbon = guiPlugin.RibbonCommandHandler;

                // instantiate the ribbon command handler of the waq gui plugin
                SpatialOperationCommandBase command = gisPlugin.RibbonCommandHandler.Commands.OfType<SetLabelCommand>().First();
                var excludedTypes = guiPlugin.GetExcludedLayerDataTypesForSpatialOperation(command).ToList();

                Assert.IsEmpty(excludedTypes);

                command = gisPlugin.RibbonCommandHandler.Commands.OfType<SetValueCommand>().First();
                excludedTypes = guiPlugin.GetExcludedLayerDataTypesForSpatialOperation(command).ToList();

                Assert.Contains(typeof(WaterQualityObservationAreaCoverage), excludedTypes);
            }
        }

        [Test]
        public void ContextMenuForHydFileModelsShouldReturnAllHydFileModels()
        {
            var mocks = new MockRepository();
            var gui = mocks.Stub<IGui>();
            var app = mocks.Stub<IApplication>();
            var activityRunner = mocks.Stub<IActivityRunner>();
            var undoRedoManager = mocks.Stub<IUndoRedoManager>();
            var model = mocks.Stub<WaterQualityModel>();
            var hydFileModel1 = mocks.Stub<IHydFileModel>();
            var hydFileModel2 = mocks.Stub<IModel>();
            var hydFileModel3 = mocks.Stub<IHydFileModel>();

            var appPlugins = new List<ApplicationPlugin>();
            var guiPlugins = new List<GuiPlugin>();
            
            Expect.Call(app.ActivityRunner).Return(activityRunner).Repeat.Any();
            Expect.Call(app.Plugins).Return(appPlugins).Repeat.Any();
            Expect.Call(app.GetAllModelsInProject()).Return(new List<IModel> {hydFileModel1, hydFileModel2, hydFileModel3});

            Expect.Call(gui.Plugins).Return(guiPlugins).Repeat.Any();
            Expect.Call(gui.UndoRedoManager).Return(undoRedoManager).Repeat.Any();
            gui.Application = app;

            hydFileModel1.Name = "Model 1";
            hydFileModel2.Name = "Model 2";
            hydFileModel3.Name = "Model 3";

            mocks.ReplayAll();

            var guiPlugin = new WaterQualityModelGuiPlugin {Gui = gui};
            var menu = guiPlugin.GetContextMenu(null, model);

            Assert.IsInstanceOf<MenuItemContextMenuStripAdapter>(menu);
            
            var contextMenu = ((MenuItemContextMenuStripAdapter) menu).ContextMenuStrip;
            Assert.AreEqual(1, contextMenu.Items.Count);
            
            var toolStripItem = contextMenu.Items[0];
            Assert.AreEqual("Use the .hyd File from...", toolStripItem.Text);
            Assert.IsInstanceOf<ClonableToolStripMenuItem>(toolStripItem);

            var contextMenuItems = ((ClonableToolStripMenuItem)toolStripItem).DropDownItems;
            Assert.AreEqual(2, contextMenuItems.Count); // only IHydFileModels should be present
            Assert.AreEqual(hydFileModel1.Name, contextMenuItems[0].Text);
            Assert.AreEqual(hydFileModel3.Name, contextMenuItems[1].Text);

            mocks.VerifyAll();

        }

        private static WaterQualityModel AddWaterQualityModelToProject(IApplication app)
        {
            // Add WaterQualityModel to project
            var project = app.Project;
            project.RootFolder.Add(new WaterQualityModel());

            //Check model name
            var targetmodel = project.RootFolder.Models.OfType<WaterQualityModel>().FirstOrDefault();
            Assert.IsNotNull(targetmodel);
            return targetmodel;
        }

        [Test]
        [Category(TestCategory.Integration)]
        [Category(TestCategory.WindowsForms)]
        public void Check_When_NewWaqModel_Created_And_HydFileImported_Then_ZoomToExtents()
        {
            var hydFile = TestHelper.GetTestFilePath(@"WaterQualityDataFiles\flow-model\westernscheldt01.hyd");
            hydFile = TestHelper.CreateLocalCopy(hydFile);

            using (var gui = new DeltaShellGui())
            {
                var app = gui.Application;
                app.Plugins.Add(new SharpMapGisApplicationPlugin());
                app.Plugins.Add(new WaterQualityModelApplicationPlugin());

                gui.Plugins.Add(new ProjectExplorerGuiPlugin());
                gui.Plugins.Add(new SharpMapGisGuiPlugin());
                var waqGuiPlugin = new WaterQualityModelGuiPlugin();
                gui.Plugins.Add(waqGuiPlugin);
                gui.Plugins.Add(new NetworkEditorGuiPlugin());

                gui.Run();

                Action mainWindowShown = delegate
                {
                    var waqModel = AddWaterQualityModelToProject(app);
                    //First call to initialize the view.
                    gui.CommandHandler.OpenView(waqModel, typeof(ProjectItemMapView));

                    //import hyd model that contains grid
                    var importer = app.FileImporters.OfType<HydFileImporter>().FirstOrDefault();
                    Assert.IsNotNull(importer);

                    var subFileImportActivity =
                        new FileImportActivity(importer, waqModel)
                        {
                            Files = new[] { hydFile }
                        };
                    app.RunActivity(subFileImportActivity);

                    //Get the view
                    var targetView = gui.DocumentViews.AllViews.OfType<ProjectItemMapView>().FirstOrDefault();

                    //Get the height and Width we got from the map after the above actions.
                    var map = targetView.MapView.Map;
                    var orHeight = targetView.MapView.Map.Envelope.Height;
                    var orWidth = targetView.MapView.Map.Envelope.Width;

                    Assert.AreEqual(orHeight, map.Envelope.Height);
                    Assert.AreEqual(orWidth, map.Envelope.Width);
                };
                WpfTestHelper.ShowModal((Control)gui.MainWindow, mainWindowShown);
            }

            //Clean directory
            FileUtils.DeleteIfExists(hydFile);
        }
    }
}