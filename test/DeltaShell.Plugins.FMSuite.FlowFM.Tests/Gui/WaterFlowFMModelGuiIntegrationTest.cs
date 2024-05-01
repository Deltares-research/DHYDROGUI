using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using BasicModelInterface;
using DelftTools.Controls;
using DelftTools.Hydro;
using DelftTools.Hydro.SewerFeatures;
using DelftTools.Hydro.Structures;
using DelftTools.Shell.Core;
using DelftTools.Shell.Core.Workflow;
using DelftTools.Shell.Core.Workflow.DataItems;
using DelftTools.Shell.Gui;
using DelftTools.Shell.Gui.Swf;
using DelftTools.TestUtils;
using DeltaShell.Dimr;
using DeltaShell.IntegrationTestUtils.Builders;
using DeltaShell.Plugins.CommonTools;
using DeltaShell.Plugins.CommonTools.Gui;
using DeltaShell.Plugins.Data.NHibernate;
using DeltaShell.Plugins.FMSuite.FlowFM.Gui;
using DeltaShell.Plugins.FMSuite.FlowFM.Gui.Layers;
using DeltaShell.Plugins.FMSuite.FlowFM.Gui.NodePresenters;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Importers;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using DeltaShell.Plugins.NetworkEditor;
using DeltaShell.Plugins.NetworkEditor.Gui;
using DeltaShell.Plugins.ProjectExplorer;
using DeltaShell.Plugins.SharpMapGis;
using DeltaShell.Plugins.SharpMapGis.Gui;
using DeltaShell.Plugins.SharpMapGis.Gui.Forms;
using DeltaShell.Plugins.SharpMapGis.Gui.Forms.CoverageViews;
using DeltaShell.Plugins.SharpMapGis.SpatialOperations;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using NUnit.Framework;
using SharpMap.Layers;
using SharpMap.SpatialOperations;
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
        [Category("Quarantine")]
        [Ignore("Crashes other tests, ignoring for now.")]
        public void RunModelShouldNotCrashWithOldOutputOpen()
        {
            var mduPath =
                TestHelper.GetTestFilePath(@"data\f04_bottomfriction\c016_2DConveyance_bend\input\bendprof.mdu");
            mduPath = TestHelper.CreateLocalCopy(mduPath);

            var model = new WaterFlowFMModel(mduPath);

            var pluginsToAdd = new List<IPlugin>()
            {
                new SharpMapGisApplicationPlugin(),
                new NetworkEditorApplicationPlugin(),
                new ProjectExplorerGuiPlugin(),
                new NetworkEditorGuiPlugin(),
                new SharpMapGisGuiPlugin(),
                new FlowFMGuiPlugin(),
            };
            using (var gui = new DeltaShellGuiBuilder().WithPlugins(pluginsToAdd).Build())
            {
                var app = gui.Application;

                gui.Run();

                app.CreateNewProject();

                Action mainWindowShown = delegate
                {
                    var project = app.Project;
                    project.RootFolder.Add(model);
                    DimrLogging.LogFileLevel = Level.All;
                    DimrLogging.FeedbackLevel = Level.All;

                    ActivityRunner.RunActivity(model);
                    Assert.AreEqual(ActivityStatus.Cleaned, model.Status);

                    // open standalone editor for his feature coverage
                    gui.CommandHandler.OpenView(model.OutputHisFileStore.Functions.First(), typeof (CoverageView));

                    Assert.AreEqual(1, gui.DocumentViews.Count);
                    model.ShowModelRunConsole = true;
                    model.Area.ObservationCrossSections.Add(new ObservationCrossSection2D
                    {
                        Name = "newobj",
                        Geometry = new LineString(new[]
                        {
                            new Coordinate(100, 100), new Coordinate(150, 100)
                        })
                    });
                    ActivityRunner.RunActivity(model);
                    Assert.AreEqual(ActivityStatus.Cleaned, model.Status);

                    Assert.IsNull(gui.DocumentViews.ActiveView);
                };
                WpfTestHelper.ShowModal((Control) gui.MainWindow, mainWindowShown);
            }
        }

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

                app.CreateNewProject();

                Action mainWindowShown = delegate
                {
                    // Add water flow model to project
                    var project = app.Project;
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

                app.CreateNewProject();

                Action mainWindowShown = delegate
                {
                    // Add new folder to project
                    var project = app.Project;
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
                var app = gui.Application;

                gui.Run();

                app.CreateNewProject();

                Action mainWindowShown = delegate
                {
                    var project = app.Project;
                    project.RootFolder.Add(model);

                    // set the heat flux model to excess temperature
                    model.ModelDefinition.GetModelProperty(KnownProperties.Temperature).SetValueAsString("3");

                    gui.CommandHandler.OpenView(model.ModelDefinition.HeatFluxModel);

                    Assert.IsTrue(
                        gui.DocumentViews.Any(v => ((HeatFluxModel) v.Data).Type == HeatFluxModelType.ExcessTemperature));

                    // set heat flux model to composite
                    model.ModelDefinition.GetModelProperty(KnownProperties.Temperature).SetValueAsString("5");

                    Assert.IsFalse(
                        gui.DocumentViews.Any(v => ((HeatFluxModel) v.Data).Type == HeatFluxModelType.ExcessTemperature));
                };
                WpfTestHelper.ShowModal((Control) gui.MainWindow, mainWindowShown);
            }
        }

        [Test]
        [Category(TestCategory.WindowsForms)]
        [Category("Quarantine")]
        public void DoubleClickingOnMapOutputCoverageShouldEnableLayerInCentralMap()
        {
            var mduPath =
                TestHelper.GetTestFilePath(@"data\f04_bottomfriction\c016_2DConveyance_bend\input\bendprof.mdu");
            mduPath = TestHelper.CreateLocalCopy(mduPath);

            var model = new WaterFlowFMModel(mduPath);

            using (var gui = CreateGuiMinimal())
            {
                var app = gui.Application;

                gui.Run();

                app.CreateNewProject();

                Action mainWindowShown = delegate
                {
                    var project = app.Project;
                    project.RootFolder.Add(model);

                    ActivityRunner.RunActivity(model);
                    Assert.AreEqual(ActivityStatus.Cleaned, model.Status);

                    // try from scratch:
                    DoubleClickOutputItemAndAssertLayerIsOn(model, gui, "s1");

                    // close all views:
                    gui.DocumentViews.Clear();
                    Assert.AreEqual(0, gui.DocumentViews.Count);

                    // open central map view
                    gui.CommandHandler.OpenView(model, typeof (ProjectItemMapView));

                    // try with already open view:
                    DoubleClickOutputItemAndAssertLayerIsOn(model, gui, "s1");
                };
                WpfTestHelper.ShowModal((Control) gui.MainWindow, mainWindowShown);
            }
        }

        [Test]
        [Category(TestCategory.WindowsForms)]
        [Category("Quarantine")]
        [Ignore("Crashes other tests, ignoring for now.")]
        public void DoubleClickingOnHisOutputCoverageShouldEnableLayerInCentralMap()
        {
            var mduPath =
                TestHelper.GetTestFilePath(@"data\f04_bottomfriction\c016_2DConveyance_bend\input\bendprof.mdu");
            mduPath = TestHelper.CreateLocalCopy(mduPath);

            var model = new WaterFlowFMModel(mduPath) {ShowModelRunConsole = true};
            
            var pluginsToAdd = new List<IPlugin>()
            {
                new SharpMapGisApplicationPlugin(),
                new NetworkEditorApplicationPlugin(),
                new CommonToolsGuiPlugin(),
                new ProjectExplorerGuiPlugin(),
                new NetworkEditorGuiPlugin(),
                new SharpMapGisGuiPlugin(),
                new FlowFMGuiPlugin(),
            };
            using (var gui = new DeltaShellGuiBuilder().WithPlugins(pluginsToAdd).Build())
            {
                var app = gui.Application;
                
                gui.Run();

                app.CreateNewProject();

                Action mainWindowShown = delegate
                {
                    var project = app.Project;
                    project.RootFolder.Add(model);

                    ActivityRunner.RunActivity(model);
                    Assert.AreEqual(ActivityStatus.Cleaned, model.Status);
                    
                    // try from scratch:
                    DoubleClickOutputItemAndAssertLayerIsOn(model, gui, "cross_section_discharge");

                    // close all views:
                    gui.DocumentViews.Clear();
                    Assert.AreEqual(0, gui.DocumentViews.Count);

                    // open central map view
                    gui.CommandHandler.OpenView(model, typeof(ProjectItemMapView));

                    // try with already open view:
                    DoubleClickOutputItemAndAssertLayerIsOn(model, gui, "cross_section_discharge");
                };
                WpfTestHelper.ShowModal((Control)gui.MainWindow, mainWindowShown);
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
                var app = gui.Application;

                gui.Run();

                app.CreateNewProject();

                Action mainWindowShown = delegate
                {
                    var project = app.Project;
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
                var app = gui.Application;

                gui.Run();

                app.CreateNewProject();

                Action mainWindowShown = delegate
                {
                    var project = app.Project;
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

        private static void DoubleClickOutputItemAndAssertLayerIsOn(WaterFlowFMModel model, IGui gui, string itemName)
        {
            // retrieve the data object for the output waterlevel through the node 
            // presenter (to make sure we use the path double clicking would follow):
            var nodePresenter = new WaterFlowFMModelNodePresenter(null);
            var childItems = nodePresenter.GetChildNodeObjects(model, null).OfType<TreeFolder>();
            var outputFolder = childItems.Last();
            var outputItemNode =
                outputFolder.ChildItems.OfType<object>().First(i => i.ToString().Contains(itemName));

            // mimic double click:
            gui.Selection = outputItemNode;
            gui.CommandHandler.OpenViewForSelection(typeof (ProjectItemMapView));

            Assert.AreEqual(1, gui.DocumentViews.Count);
            var activeMapView = FlowFMGuiPlugin.ActiveMapView;
            Assert.IsNotNull(activeMapView, "fm active map view");

            var coverageLayer = activeMapView.Map.GetAllLayers(false).FirstOrDefault(l => l.Name.Contains(itemName));

            Assert.IsNotNull(coverageLayer, "coverage layer not found");
            Assert.IsTrue(coverageLayer.Visible, "not visible");
        }

        /// <summary>
        /// Test for issue TOOLS_22977, Not working test only reproducing scenario
        /// </summary>
        [Test]
        [Category(TestCategory.Integration)]
        [Category(TestCategory.WindowsForms)]
        [Category(TestCategory.WorkInProgress)]
        [Ignore("Needs to be checked")]
        public void TOOLS_22977Test()
        {
            var sharpMapGisGuiPlugin = new SharpMapGisGuiPlugin();
            
            var pluginsToAdd = new List<IPlugin>()
            {
                new NHibernateDaoApplicationPlugin(),
                new CommonToolsApplicationPlugin(),
                new SharpMapGisApplicationPlugin(),
                new FlowFMApplicationPlugin(),
                new NetworkEditorApplicationPlugin(),
                new CommonToolsGuiPlugin(),                
                sharpMapGisGuiPlugin,
                new FlowFMGuiPlugin(),
                new NetworkEditorGuiPlugin(),
                new ProjectExplorerGuiPlugin(),
            };
            using (var gui = new DeltaShellGuiBuilder().WithPlugins(pluginsToAdd).Build())
            {
                var app = gui.Application;
                
                gui.Run();
                
                app.CreateNewProject();
                
                var testFilePath = TestHelper.GetTestFilePath(@"harlingen\har.mdu");
                testFilePath = TestHelper.CreateLocalCopy(testFilePath);

                var model = new WaterFlowFMModel(testFilePath);

                gui.Application.Project.RootFolder.Add(model);

                Assert.IsTrue(gui.DocumentViewsResolver.OpenViewForData(model, typeof (ProjectItemMapView)));
                var mapView = gui.DocumentViews.ActiveView as ProjectItemMapView;
                mapView.SetSpatialOperationLayer(mapView.MapView.GetLayerForData(model.Bathymetry), true);
                sharpMapGisGuiPlugin.FocusSpatialOperationView();

                var valueConverter = SpatialOperationValueConverterFactory.GetOrCreateSpatialOperationValueConverter(
                    model.GetDataItemByValue(model.Bathymetry));

                Assert.IsNotNull(valueConverter.SpatialOperationSet);

                var sampleSet = new SpatialOperationSet();
                valueConverter.SpatialOperationSet.AddOperation(sampleSet);

                var samplesPath = TestHelper.GetTestFilePath(@"harlingen_model_3d\har_V3.xyz");
                var importSamples = new ImportSamplesOperation(false) {FilePath = samplesPath, Name = "Test import"};
                Assert.IsNotNull(sampleSet.AddOperation(importSamples));

                var interpolate = new InterpolateOperation
                {
                    InterpolationMethod = SpatialInterpolationMethod.Triangulation,
                    OperationType = PointwiseOperationType.OverwriteWhereMissing
                };
                interpolate.LinkInput(InterpolateOperation.InputSamplesName, importSamples.Output);

                valueConverter.SpatialOperationSet.AddOperation(interpolate);
                valueConverter.SpatialOperationSet.Execute();
                Action onShown = delegate
                {

                    interpolate.OperationType = PointwiseOperationType.Overwrite;
                    var layer = (SpatialOperationSetLayer) mapView.SpatialOperationLayer;
                    valueConverter.SpatialOperationSet.Execute();

                    var beforeRefreshThreadCount = Process.GetCurrentProcess().Threads.Count;
                    TestHelper.AssertIsFasterThan(4000, layer.ShowOutputOnly);
                    Thread.Sleep(3000);
                    Assert.AreEqual(beforeRefreshThreadCount, Process.GetCurrentProcess().Threads.Count);

                };

                WpfTestHelper.ShowModal((Control) gui.MainWindow, onShown);
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
                //setup env
                var app = gui.Application;
                
                gui.Run();

                app.CreateNewProject();

                //create and add a HydroRegion with a HydroArea with DryPoints
                var project = app.Project;
                var area = new HydroArea();
                var hydroRegion = new HydroRegion
                {
                    Name = "Hydro region",
                    SubRegions = { area }
                };
                var dataItem = new DataItem(hydroRegion);
                project.RootFolder.Add(hydroRegion);

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

                app.CreateNewProject();
                
                app.UserSettings["autosaveWindowLayout"] = false; // skip damagin of window layout
                Action formVisibleChangedAction = () =>
                {
                    using (var model = new WaterFlowFMModel())
                    {
                        app.Project.RootFolder.Add(model);
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
                var app = gui.Application;

                gui.Run();

                app.CreateNewProject();

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