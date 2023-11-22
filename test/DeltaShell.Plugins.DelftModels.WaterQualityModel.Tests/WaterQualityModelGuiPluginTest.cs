using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using DelftTools.Controls;
using DelftTools.Shell.Core;
using DelftTools.Shell.Core.Workflow;
using DelftTools.Shell.Gui;
using DelftTools.Shell.Gui.Forms;
using DelftTools.TestUtils;
using DelftTools.Utils.IO;
using DeltaShell.IntegrationTestUtils;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.DataObjects;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.Gui;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.Gui.Forms.ProjectExplorer;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.Gui.Forms.WaterQualityModelWizard;
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
using SharpMap.Api;
using SharpMap.UI.Tools;

namespace DeltaShell.Plugins.DelftModels.WaterQualityModel.Tests
{
    [TestFixture]
    public class WaterQualityModelGuiPluginTest
    {
        [Test]
        [Category(TestCategory.Integration)]
        [Category(TestCategory.Slow)]
        public void GetRibbonCommandHandlerReturnsWaterQualityRibbon()
        {
            var mocks = new MockRepository();
            var guiStub = mocks.Stub<IGui>();
            var applicationStub = mocks.Stub<IApplication>();
            guiStub.Application = applicationStub;

            // setup
            using (var gisPlugin = new SharpMapGisGuiPlugin())
            using (var guiPlugin = new WaterQualityModelGuiPlugin())
            {
                gisPlugin.Gui = guiStub;
                gisPlugin.InitializeSpatialOperationSetLayerView();

                // call
                IRibbonCommandHandler ribbonCommandHandler = guiPlugin.RibbonCommandHandler;

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
                bool canCopy = guiPlugin.CanCopy(modelStub);

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
                bool canCopy = guiPlugin.CanCut(modelStub);

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
                IMapTool addLoadTool = projectItemMapView.MapView.MapControl.GetToolByName(WaterQualityModelGuiPlugin.AddWaterQualityLoadMapToolName);
                Assert.IsInstanceOf<NewPointFeatureTool<WaterQualityLoad>>(addLoadTool);

                IMapTool addObservationPointTool = projectItemMapView.MapView.MapControl.GetToolByName(WaterQualityModelGuiPlugin.AddObservationPointMapToolName);
                Assert.IsInstanceOf<NewPointFeatureTool<WaterQualityObservationPoint>>(addObservationPointTool);
            }
        }

        [Test]
        public void GetProjectTreeViewNodePresentersTest()
        {
            // setup
            var guiPlugin = new WaterQualityModelGuiPlugin();

            // call
            ITreeNodePresenter[] nodePresenters = guiPlugin.GetProjectTreeViewNodePresenters().ToArray();

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
            var mocks = new MockRepository();
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
                IRibbonCommandHandler _ = guiPlugin.RibbonCommandHandler;

                // instantiate the ribbon command handler of the waq gui plugin
                SpatialOperationCommandBase command = gisPlugin.RibbonCommandHandler.Commands.OfType<SetLabelCommand>().First();
                List<Type> excludedTypes = guiPlugin.GetExcludedLayerDataTypesForSpatialOperation(command).ToList();

                Assert.IsEmpty(excludedTypes);

                command = gisPlugin.RibbonCommandHandler.Commands.OfType<SetValueCommand>().First();
                excludedTypes = guiPlugin.GetExcludedLayerDataTypesForSpatialOperation(command).ToList();

                Assert.Contains(typeof(WaterQualityObservationAreaCoverage), excludedTypes);
            }
        }

        [Test]
        [Category(TestCategory.Wpf)]
        public void Check_When_NewWaqModel_Created_And_HydFileImported_Then_ZoomToExtents()
        {
            string hydFile = TestHelper.GetTestFilePath(@"WaterQualityDataFiles\flow-model\westernscheldt01.hyd");
            hydFile = TestHelper.CreateLocalCopy(hydFile);

            using (var gui = DeltaShellCoreFactory.CreateGui())
            {
                IApplication app = gui.Application;
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
                    app.CreateNewProject();
                    
                    WaterQualityModel waqModel = AddWaterQualityModelToProject(app);
                    //First call to initialize the view.
                    gui.CommandHandler.OpenView(waqModel, typeof(ProjectItemMapView));

                    //import hyd model that contains grid
                    HydFileImporter importer = app.FileImporters.OfType<HydFileImporter>().FirstOrDefault();
                    Assert.IsNotNull(importer);

                    var subFileImportActivity =
                        new FileImportActivity(importer, waqModel)
                        {
                            Files = new[]
                            {
                                hydFile
                            }
                        };
                    app.RunActivity(subFileImportActivity);

                    //Get the view
                    ProjectItemMapView targetView = gui.DocumentViews.AllViews.OfType<ProjectItemMapView>().FirstOrDefault();

                    //Get the height and Width we got from the map after the above actions.
                    IMap map = targetView.MapView.Map;
                    double orHeight = targetView.MapView.Map.Envelope.Height;
                    double orWidth = targetView.MapView.Map.Envelope.Width;

                    Assert.AreEqual(orHeight, map.Envelope.Height);
                    Assert.AreEqual(orWidth, map.Envelope.Width);
                };
                WpfTestHelper.ShowModal((Control) gui.MainWindow, mainWindowShown);
            }

            //Clean directory
            FileUtils.DeleteIfExists(hydFile);
        }

        [Test]
        public void GetViewInfoForLoadsDataTableBoundaryImporter_ReturnsExpectedViewInfoConfiguration()
        {
            // Setup
            using (var plugin = new WaterQualityModelGuiPlugin())
            {
                // Call
                ViewInfo viewInfo = plugin.GetViewInfoObjects()
                                          .First(vi => vi.DataType == typeof(LoadsDataTableImporter));

                // Assert
                Assert.That(viewInfo.ViewType, Is.EqualTo(typeof(LoadsDataWizard)));
                Assert.That(viewInfo.Description, Is.EqualTo("Loads Data Wizard Dialog"));
                Assert.That(viewInfo.AdditionalDataCheck, Is.Not.Null);
            }
        }

        [Test]
        public void GetViewInfoForBoundaryDataTableImporter_ReturnsExpectedViewInfoConfiguration()
        {
            // Setup
            using (var plugin = new WaterQualityModelGuiPlugin())
            {
                // Call
                ViewInfo viewInfo = plugin.GetViewInfoObjects()
                                          .First(vi => vi.DataType == typeof(BoundaryDataTableImporter));

                // Assert
                Assert.That(viewInfo.ViewType, Is.EqualTo(typeof(BoundaryDataWizard)));
                Assert.That(viewInfo.Description, Is.EqualTo("Boundary Data Wizard Dialog"));
                Assert.That(viewInfo.AdditionalDataCheck, Is.Not.Null);
            }
        }

        private static WaterQualityModel AddWaterQualityModelToProject(IApplication app)
        {
            // Add WaterQualityModel to project
            Project project = app.Project;
            project.RootFolder.Add(new WaterQualityModel());

            //Check model name
            WaterQualityModel targetmodel = project.RootFolder.Models.OfType<WaterQualityModel>().FirstOrDefault();
            Assert.IsNotNull(targetmodel);
            return targetmodel;
        }
    }
}