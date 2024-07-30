using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using DelftTools.Controls;
using DelftTools.Hydro;
using DelftTools.Hydro.SewerFeatures;
using DelftTools.Hydro.Structures;
using DelftTools.Shell.Core;
using DelftTools.Shell.Core.Workflow;
using DelftTools.Shell.Core.Workflow.DataItems;
using DelftTools.Shell.Gui;
using DelftTools.TestUtils;
using DeltaShell.IntegrationTestUtils.Builders;
using DeltaShell.Plugins.CommonTools;
using DeltaShell.Plugins.CommonTools.Gui;
using DeltaShell.Plugins.FMSuite.FlowFM.Gui;
using DeltaShell.Plugins.FMSuite.FlowFM.Gui.Layers;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Importers;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using DeltaShell.Plugins.NetworkEditor;
using DeltaShell.Plugins.NetworkEditor.Gui;
using DeltaShell.Plugins.ProjectExplorer;
using DeltaShell.Plugins.SharpMapGis;
using DeltaShell.Plugins.SharpMapGis.Gui;
using DeltaShell.Plugins.SharpMapGis.Gui.Forms;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using NUnit.Framework;
using SharpMap.Layers;
using SharpMap.UI.Tools;
using Control = System.Windows.Controls.Control;
using Ribbon = Fluent.Ribbon;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.Gui
{
    [TestFixture, Apartment(ApartmentState.STA)]
    public class WaterFlowFMModelGuiIntegrationTest
    {
        [Test]
        [Category(TestCategory.WindowsForms)]
        public void FmModelShouldBeReplacedWhenImportedInRootFolder()
        {
            var mduPath = TestHelper.GetTestFilePath(@"harlingen\har.mdu");
            mduPath = TestHelper.CreateLocalCopy(mduPath);

            var pluginsToAdd = new List<IPlugin>()
            {
                new SharpMapGisApplicationPlugin(),
                new FlowFMApplicationPlugin(),
                new ProjectExplorerGuiPlugin(),
                new SharpMapGisGuiPlugin(),
                new FlowFMGuiPlugin(),
            };
            
            using (var gui = new DeltaShellGuiBuilder().WithPlugins(pluginsToAdd).Build())
            {
                var app = gui.Application;

                gui.Run();

                Action mainWindowShown = delegate
                {
                    // Add water flow model to project
                    Project project = app.ProjectService.CreateProject();
                    project.RootFolder.Add(new WaterFlowFMModel());

                    // Check model name
                    var targetModel = project.RootFolder.Models.OfType<WaterFlowFMModel>().FirstOrDefault();
                    Assert.IsNotNull(targetModel);
                    Assert.That(targetModel.Name.Contains("FlowFM"));

                    // Import new water flow model
                    var importer = app.FileImporters.OfType<WaterFlowFMFileImporter>().FirstOrDefault();
                    Assert.IsNotNull(importer);
                    importer.ImportItem(mduPath, targetModel);

                    // Check name of imported water flow model
                    targetModel = project.RootFolder.Models.OfType<WaterFlowFMModel>().FirstOrDefault();
                    Assert.IsNotNull(targetModel);
                    Assert.That(targetModel.Name.Contains("har"));
                };
                WpfTestHelper.ShowModal((Control)gui.MainWindow, mainWindowShown);
            }
        }

        [Test]
        [Category(TestCategory.WindowsForms)]
        public void FmModelShouldBeReplacedWhenImportedInFolder()
        {
            var mduPath = TestHelper.GetTestFilePath(@"harlingen\har.mdu");
            mduPath = TestHelper.CreateLocalCopy(mduPath);

            var pluginsToAdd = new List<IPlugin>()
            {
                new SharpMapGisApplicationPlugin(),
                new FlowFMApplicationPlugin(),
                new ProjectExplorerGuiPlugin(),
                new SharpMapGisGuiPlugin(),
                new FlowFMGuiPlugin(),
            };
            
            using (var gui = new DeltaShellGuiBuilder().WithPlugins(pluginsToAdd).Build())
            {
                var app = gui.Application;

                gui.Run();

                Action mainWindowShown = delegate
                {
                    // Add new folder to project
                    Project project = app.ProjectService.CreateProject();
                    project.RootFolder.Add(new Folder("Test Folder"));

                    // Check folder name
                    var testFolder = project.RootFolder.Folders.FirstOrDefault();
                    Assert.IsNotNull(testFolder);
                    Assert.That(testFolder.Name.Contains("Test Folder"));

                    // Add new water flow model to the new folder and check its name
                    testFolder.Add(new WaterFlowFMModel());
                    var targetModel =
                        testFolder.Models.OfType<WaterFlowFMModel>().FirstOrDefault();
                    Assert.IsNotNull(targetModel);
                    Assert.That(targetModel.Name.Contains("FlowFM"));

                    // Import new water flow model
                    var importer = app.FileImporters.OfType<WaterFlowFMFileImporter>().FirstOrDefault();
                    Assert.IsNotNull(importer);
                    importer.ImportItem(mduPath, targetModel);

                    // Check name of imported water flow model
                    targetModel = testFolder.Models.OfType<WaterFlowFMModel>().FirstOrDefault();
                    Assert.IsNotNull(targetModel);
                    Assert.That(targetModel.Name.Contains("har"));
                };
                WpfTestHelper.ShowModal((Control)gui.MainWindow, mainWindowShown);
            }
        }


        /// <summary>
        /// Test if the view of the heat flux model changes after you change
        /// the type of heat flux model in the combobox.
        /// </summary>
        [Test]
        [Category(TestCategory.WindowsForms)]
        public void TestCloseHeatFluxModelViewOnChange()
        {
            var model = new WaterFlowFMModel();

            using (var gui = CreateGuiMinimal())
            {
                gui.Run();

                Action mainWindowShown = delegate
                {
                    Project project = gui.Application.ProjectService.CreateProject();
                    project.RootFolder.Add(model);

                    // set the heat flux model to excess temperature
                    model.ModelDefinition.GetModelProperty(KnownProperties.Temperature).SetValueFromString("3");

                    gui.CommandHandler.OpenView(model.ModelDefinition.HeatFluxModel);

                    Assert.IsTrue(
                        gui.DocumentViews.Any(v => ((HeatFluxModel) v.Data).Type == HeatFluxModelType.ExcessTemperature));

                    // set heat flux model to composite
                    model.ModelDefinition.GetModelProperty(KnownProperties.Temperature).SetValueFromString("5");

                    Assert.IsFalse(
                        gui.DocumentViews.Any(v => ((HeatFluxModel) v.Data).Type == HeatFluxModelType.ExcessTemperature));
                };
                WpfTestHelper.ShowModal((Control) gui.MainWindow, mainWindowShown);
            }
        }
        
        [Test]
        [Category(TestCategory.WindowsForms)]
        [Category(TestCategory.Slow)]
        public void ShowSnappedFeatureLayersInMap()
        {
            var mduPath =
                TestHelper.GetTestFilePath(@"harlingen\har.mdu");
            mduPath = TestHelper.CreateLocalCopy(mduPath);

            var model = new WaterFlowFMModel(mduPath) {ShowModelRunConsole = true};

            using (var gui = CreateGuiMinimal())
            {
                gui.Run();

                Action mainWindowShown = delegate
                {
                    Project project = gui.Application.ProjectService.CreateProject();
                    project.RootFolder.Add(model);

                    // open central map view
                    gui.CommandHandler.OpenView(model, typeof (ProjectItemMapView));

                    var gridSnappedFeatureGroupLayer = ((ProjectItemMapView) gui.DocumentViews.ActiveView).MapView.Map.GetAllLayers(true)
                        .First(l => l.Name == FlowFMLayerNames.EstimatedSnappedFeaturesLayerName);

                    gridSnappedFeatureGroupLayer.Visible = true;

                    ((ProjectItemMapView) gui.DocumentViews.ActiveView).MapView.Refresh();
                };
                WpfTestHelper.ShowModal((Control) gui.MainWindow, mainWindowShown);
            }
        }

        [Test]
        [Category(TestCategory.Integration)]
        [Category(TestCategory.VerySlow)]
        public void RunningFMModelShouldGiveVectorVelocityLayer()
        {
            var mduPath =
                TestHelper.GetTestFilePath(@"harlingen\har.mdu");
            mduPath = TestHelper.CreateLocalCopy(mduPath);

            var model = new WaterFlowFMModel(mduPath) { ShowModelRunConsole = true };

            using (var gui = CreateGuiMinimal())
            {
                gui.Run();

                Action mainWindowShown = delegate
                {
                    Project project = gui.Application.ProjectService.CreateProject();
                    project.RootFolder.Add(model);
                    
                    ActivityRunner.RunActivity(model);

                    // open central map view
                    gui.CommandHandler.OpenView(model, typeof(ProjectItemMapView));

                    var layers = ((ProjectItemMapView)gui.DocumentViews.ActiveView).MapView.Map.GetAllLayers(true);
                    int i = 2;
                    var velocityLayer =
                        layers.FirstOrDefault(
                            l => l.Name == "velocity (ucx + ucy)" && l is UnstructuredGridCellVectorCoverageLayer);
                    Assert.NotNull(velocityLayer);
                };

                WpfTestHelper.ShowModal((Control)gui.MainWindow, mainWindowShown);

            }
        }

        [Test]
        [Category(TestCategory.Performance)]
        [Category(TestCategory.Slow)]
        public void ImportingOfDryPointsWithProjectItemMapViewOpenShouldBeFast()
        {
            var pluginsToAdd = new List<IPlugin>()
            {
                new CommonToolsApplicationPlugin(),
                new NetworkEditorApplicationPlugin(),
                new SharpMapGisApplicationPlugin(),
                new FlowFMApplicationPlugin(),
                new ProjectExplorerGuiPlugin(),
                new SharpMapGisGuiPlugin(),
                new NetworkEditorGuiPlugin(),
                new FlowFMGuiPlugin(),
            };
            using (var gui = new DeltaShellGuiBuilder().WithPlugins(pluginsToAdd).Build())
            {
                gui.Run();
                gui.Application.ProjectService.CreateProject();
                
                //create and add a HydroRegion with a HydroArea with DryPoints
                var area = new HydroArea();
                var hydroRegion = new HydroRegion
                {
                    Name = "Hydro region",
                    SubRegions = { area }
                };
                var dataItem = new DataItem(hydroRegion);

                var waterFlowFMModel = new WaterFlowFMModel();
                waterFlowFMModel.Area = area;
                
                gui.Application.ProjectService.Project.RootFolder.Add(waterFlowFMModel);
                gui.Application.ProjectService.Project.RootFolder.Add(hydroRegion);

                WpfTestHelper.ShowModal((Control)gui.MainWindow, () =>
                {
                    //load needed views
                    gui.CommandHandler.OpenView(dataItem, typeof(ProjectItemMapView));
                    var projectItemMapView = gui.DocumentViews.OfType<ProjectItemMapView>().FirstOrDefault();
                    Assert.NotNull(projectItemMapView);

                    //importing harlingen point ~ 28800 points... this took over 15 min to load
                    var fmtestPath = TestHelper.GetTestDataDirectoryPathForAssembly(typeof(WaterFlowFMModelTest).Assembly);
                    var xyzPath = Path.Combine(fmtestPath, @"harlingen_model_3d\har_V3.xyz");
                    var selection = new DataItem(area.DryPoints);

                    gui.Selection = selection;

                    //start the import and check the speed (TOOLS-21888)
                    TestHelper.AssertIsFasterThan(20000, () =>
                    {
                        gui.CommandHandler.ImportFilesToGuiSelection(new[] { xyzPath });
                        while (gui.Application.ActivityRunner.IsRunning)
                        {
                            Application.DoEvents();
                        }
                    });

                    //zoom to extend for fun
                    projectItemMapView.MapView.Map.ZoomToExtents();

                    //switch from layer
                    gui.Selection = area.DryAreas;

                    //switch back to drypoints layer and check speed of selection (<4000ms!) & selection count (== SelectTool.MaxSelectedFeatures)
                    TestHelper.AssertIsFasterThan(4000, () => gui.Selection = area.DryPoints);
                    Assert.AreEqual(SelectTool.MaxSelectedFeatures, projectItemMapView.MapView.MapControl.SelectedFeatures.Count());
                });
            }
        }

        [Test]
        [Category(TestCategory.WindowsForms)]
        public void DrawingPipeCorrectlyAddsCompartmentsToCompartmentLayer()
        {
            var pluginsToAdd = new List<IPlugin>()
            {
                new NetworkEditorApplicationPlugin(),
                new SharpMapGisApplicationPlugin(),
                new CommonToolsApplicationPlugin(),
                new FlowFMApplicationPlugin(),
                new ProjectExplorerGuiPlugin(),
                new CommonToolsGuiPlugin(),
                new SharpMapGisGuiPlugin(),
                new NetworkEditorGuiPlugin(),
                new FlowFMGuiPlugin(),
            };
            using (var gui = new DeltaShellGuiBuilder().WithPlugins(pluginsToAdd).Build())
            {
                var app = gui.Application;

                gui.Run();
                
                app.UserSettings["autosaveWindowLayout"] = false; // skip damagin of window layout
                Action formVisibleChangedAction = () =>
                {
                    using (var model = new WaterFlowFMModel())
                    {
                        Project project = gui.Application.ProjectService.CreateProject();
                        project.RootFolder.Add(model);
                        gui.CommandHandler.OpenView(model);
                        var network = model.Network;

                        network.BeginEdit("Adding pipe...");
                        IPipe pipe = new Pipe()
                        {
                            Geometry = new LineString(new[] {new Coordinate(0, 0), new Coordinate(0, 100),})
                        };
                        SewerFactory.AddDefaultPipeToNetwork(pipe, network);
                        Assert.That(() => network.EndEdit(), Throws.Nothing);

                        var mapView = gui.DocumentViews.ActiveView.GetViewsOfType<MapView>().FirstOrDefault();
                        Assert.That(mapView, Is.Not.Null);

                        var compartmentLayer = mapView.MapControl.Map.GetAllLayers(true).FirstOrDefault(l => l.DataSource?.FeatureType == typeof(Compartment));
                        Assert.That(compartmentLayer, Is.Not.Null);

                        Assert.That(compartmentLayer.DataSource.Features, Has.Member(pipe.SourceCompartment as Compartment));
                        Assert.That(compartmentLayer.DataSource.Features, Has.Member(pipe.TargetCompartment as Compartment));
                    }
                };
                WpfTestHelper.ShowModal((Control) gui.MainWindow, formVisibleChangedAction);
            }
        }
        private static IGui CreateGuiMinimal()
        {
            var pluginsToAdd = new List<IPlugin>()
            {
                new SharpMapGisApplicationPlugin(),
                new NetworkEditorApplicationPlugin(),
                new ProjectExplorerGuiPlugin(),
                new NetworkEditorGuiPlugin(),
                new SharpMapGisGuiPlugin(),
                new FlowFMGuiPlugin(),
            };
            return new DeltaShellGuiBuilder().WithPlugins(pluginsToAdd).Build();
        }
        
        [Test]
        [Category(TestCategory.Wpf)]
        public void MapRibbonGroupsShouldBeSorted()
        {
            using (var gui = CreateGuiMinimal())
            {
                gui.Run();

                gui.Application.ProjectService.CreateProject();

                var mainWindow = (Control)gui.MainWindow;

                WpfTestHelper.ShowModal(mainWindow);

                var ribbon = (Ribbon)mainWindow.FindName("MainWindowRibbon");
                Assert.IsNotNull(ribbon);
                
                var mapTab = ribbon.Tabs.FirstOrDefault(x => x.Header.Equals("Map"));
                Assert.IsNotNull(mapTab);
                
                Assert.AreEqual(12, mapTab.Groups.Count);
                Assert.AreEqual("Decorations", mapTab.Groups[0].Header);
                Assert.AreEqual("Tools", mapTab.Groups[1].Header);
                Assert.AreEqual("Background layers", mapTab.Groups[2].Header);
                Assert.AreEqual("Edit", mapTab.Groups[3].Header);
                Assert.AreEqual("Grid Profile", mapTab.Groups[4].Header);
                Assert.AreEqual("RR Basin", mapTab.Groups[5].Header);
                Assert.AreEqual("RR Region", mapTab.Groups[6].Header);
                Assert.AreEqual("1D Network", mapTab.Groups[7].Header);
                Assert.AreEqual("2D Area", mapTab.Groups[8].Header);
                Assert.AreEqual("2D Region", mapTab.Groups[9].Header);
                Assert.AreEqual("1D2D Links", mapTab.Groups[10].Header);
                Assert.AreEqual("Network Coverage", mapTab.Groups[11].Header);
            }
        }
    }
}