using System;
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
using DelftTools.Utils.Editing;
using DeltaShell.Dimr;
using DeltaShell.Gui;
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

            using (var gui = new DeltaShellGui())
            {
                var app = gui.Application;
                app.Plugins.Add(new SharpMapGisApplicationPlugin());
                app.Plugins.Add(new NetworkEditorApplicationPlugin());
                gui.Plugins.Add(new ProjectExplorerGuiPlugin());
                gui.Plugins.Add(new NetworkEditorGuiPlugin());
                gui.Plugins.Add(new SharpMapGisGuiPlugin());
                gui.Plugins.Add(new FlowFMGuiPlugin());

                gui.Run();

                Action mainWindowShown = delegate
                {
                    var project = app.Project;
                    project.RootFolder.Add(model);
                    DimrApiDataSet.LogFileLevel = Level.All;
                    DimrApiDataSet.FeedbackLevel = Level.All;

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

            using (var gui = new DeltaShellGui())
            {
                var app = gui.Application;
                app.Plugins.Add(new SharpMapGisApplicationPlugin());
                app.Plugins.Add(new FlowFMApplicationPlugin());
                gui.Plugins.Add(new ProjectExplorerGuiPlugin());
                gui.Plugins.Add(new SharpMapGisGuiPlugin());
                gui.Plugins.Add(new FlowFMGuiPlugin());

                gui.Run();

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

            using (var gui = new DeltaShellGui())
            {
                var app = gui.Application;
                app.Plugins.Add(new SharpMapGisApplicationPlugin());
                app.Plugins.Add(new FlowFMApplicationPlugin());
                gui.Plugins.Add(new ProjectExplorerGuiPlugin());
                gui.Plugins.Add(new SharpMapGisGuiPlugin());
                gui.Plugins.Add(new FlowFMGuiPlugin());

                gui.Run();

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

            using (var gui = new DeltaShellGui())
            {
                var app = gui.Application;
                app.Plugins.Add(new SharpMapGisApplicationPlugin());
                app.Plugins.Add(new NetworkEditorApplicationPlugin());
                gui.Plugins.Add(new ProjectExplorerGuiPlugin());
                gui.Plugins.Add(new NetworkEditorGuiPlugin());
                gui.Plugins.Add(new SharpMapGisGuiPlugin());
                gui.Plugins.Add(new FlowFMGuiPlugin());

                gui.Run();

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

            using (var gui = new DeltaShellGui())
            {
                var app = gui.Application;
                app.Plugins.Add(new SharpMapGisApplicationPlugin());
                app.Plugins.Add(new NetworkEditorApplicationPlugin());
                gui.Plugins.Add(new ProjectExplorerGuiPlugin());
                gui.Plugins.Add(new NetworkEditorGuiPlugin());
                gui.Plugins.Add(new SharpMapGisGuiPlugin());
                gui.Plugins.Add(new FlowFMGuiPlugin());

                gui.Run();

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
            
            using (var gui = new DeltaShellGui())
            {
                var app = gui.Application;
                app.Plugins.Add(new SharpMapGisApplicationPlugin());
                app.Plugins.Add(new NetworkEditorApplicationPlugin());
                gui.Plugins.Add(new CommonToolsGuiPlugin());
                gui.Plugins.Add(new ProjectExplorerGuiPlugin());
                gui.Plugins.Add(new NetworkEditorGuiPlugin());
                gui.Plugins.Add(new SharpMapGisGuiPlugin());
                gui.Plugins.Add(new FlowFMGuiPlugin());

                gui.Run();

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

            using (var gui = new DeltaShellGui())
            {
                var app = gui.Application;
                app.Plugins.Add(new SharpMapGisApplicationPlugin());
                app.Plugins.Add(new NetworkEditorApplicationPlugin());
                gui.Plugins.Add(new ProjectExplorerGuiPlugin());
                gui.Plugins.Add(new NetworkEditorGuiPlugin());
                gui.Plugins.Add(new SharpMapGisGuiPlugin());
                gui.Plugins.Add(new FlowFMGuiPlugin());

                gui.Run();

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

            using (var gui = new DeltaShellGui())
            {
                var app = gui.Application;
                app.Plugins.Add(new SharpMapGisApplicationPlugin());
                app.Plugins.Add(new NetworkEditorApplicationPlugin());
                gui.Plugins.Add(new ProjectExplorerGuiPlugin());
                gui.Plugins.Add(new NetworkEditorGuiPlugin());
                gui.Plugins.Add(new SharpMapGisGuiPlugin());
                gui.Plugins.Add(new FlowFMGuiPlugin());

                gui.Run();

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
            using (var gui = new DeltaShellGui())
            {
                var app = gui.Application;

                app.Plugins.Add(new NHibernateDaoApplicationPlugin());
                app.Plugins.Add(new CommonToolsApplicationPlugin());
                app.Plugins.Add(new SharpMapGisApplicationPlugin());
                app.Plugins.Add(new FlowFMApplicationPlugin());
                app.Plugins.Add(new NetworkEditorApplicationPlugin());

                gui.Plugins.Add(new CommonToolsGuiPlugin());
                var sharpMapGisGuiPlugin = new SharpMapGisGuiPlugin();
                gui.Plugins.Add(sharpMapGisGuiPlugin);
                gui.Plugins.Add(new FlowFMGuiPlugin());
                gui.Plugins.Add(new NetworkEditorGuiPlugin());
                gui.Plugins.Add(new ProjectExplorerGuiPlugin());
                gui.Run();
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
        
/*        [Test]
        [Category(TestCategory.Integration)]
        [Category(TestCategory.Slow)]
        public void GivenNoBedLevelInNetCdfFileWhenSetXYZSamplesAndSaveAndLoadThenBedLevelInNetCdfFile()
        {
            using (var app = new DeltaShellApplication())
            {
                app.Plugins.Add(new NHibernateDaoApplicationPlugin());
                app.Plugins.Add(new CommonToolsApplicationPlugin());
                app.Plugins.Add(new SharpMapGisApplicationPlugin());
                app.Plugins.Add(new FlowFMApplicationPlugin());
                app.Plugins.Add(new NetworkEditorApplicationPlugin());


                app.Run();
                string path = "temp.dsproj";
                var tempPath1 = Path.GetTempFileName();
                File.Delete(tempPath1);
                Directory.CreateDirectory(tempPath1);

                app.SaveProjectAs(Path.Combine(tempPath1, path)); // save to initialize file repository..
                var testFilePath = TestHelper.GetTestFilePath(@"harlingen\har.mdu");
                testFilePath = TestHelper.CreateLocalCopy(testFilePath);

                var model = new WaterFlowFMModel(testFilePath);
                //default harlingen had bedlevel values :
                Assert.IsFalse(model.Bathymetry.GetValues<double>().All(v=> Math.Abs(v - (-999.0d)) < 0.01));
                app.Project.RootFolder.Add(model);
                
                var valueConverter = SpatialOperationValueConverterFactory.GetOrCreateSpatialOperationValueConverter(
                    model.GetDataItemByValue(model.Bathymetry));

                Assert.IsNotNull(valueConverter.SpatialOperationSet);

                
                // create polygon as big a bathemetry
                var polygons = new[] { new Feature
                {
                    Geometry = model.Bathymetry.Coordinates.ToPolygon()
                }};

                var eraseOperation = new EraseOperation();
                var mask = new FeatureCollection(polygons, typeof(Feature));
                eraseOperation.SetInputData(SpatialOperation.MaskInputName, mask);
                Assert.IsNotNull(valueConverter.SpatialOperationSet.AddOperation(eraseOperation));
                //erase all bedlevel values
                valueConverter.SpatialOperationSet.Execute();
                app.SaveProject();
                app.CloseProject();
                app.CreateNewProject();
                const string path2 = "temp2.dsproj";
                var tempPath2 = Path.GetTempFileName();
                File.Delete(tempPath2);
                Directory.CreateDirectory(tempPath2);

                app.SaveProjectAs(Path.Combine(tempPath2, path2));
                var otherModel = new WaterFlowFMModel(tempPath1+@"\temp.dsproj_data\har\har.mdu");
                Assert.IsTrue(otherModel.Bathymetry.GetValues<double>().All(v => Math.Abs(v - (-999.0d)) < 0.01));
                app.Project.RootFolder.Add(otherModel);
                valueConverter = SpatialOperationValueConverterFactory.GetOrCreateSpatialOperationValueConverter(
                    otherModel.GetDataItemByValue(otherModel.Bathymetry));

                Assert.IsNotNull(valueConverter.SpatialOperationSet);
                var samplesPath = TestHelper.GetTestFilePath(@"harlingen_model_3d\har_V3.xyz");
                var importSamples = new ImportSamplesOperation(false) { FilePath = samplesPath, Name = "Test import" };
                Assert.IsNotNull(valueConverter.SpatialOperationSet.AddOperation(importSamples));

                var interpolate = new InterpolateOperation
                {
                    InterpolationMethod = SpatialInterpolationMethod.Triangulation,
                    OperationType = PointwiseOperationType.OverwriteWhereMissing
                };
                interpolate.LinkInput(InterpolateOperation.InputSamplesName, importSamples.Output);

                valueConverter.SpatialOperationSet.AddOperation(interpolate);
                valueConverter.SpatialOperationSet.Execute();

                app.SaveProject();
                app.CloseProject();
                app.CreateNewProject();
                var anotherModel = new WaterFlowFMModel(tempPath2+@"\temp2.dsproj_data\har\har.mdu");
                Assert.IsFalse(anotherModel.Bathymetry.GetValues<double>().All(v => Math.Abs(v - (-999.0d)) < 0.01));
            }
        }

        [Test]
        [Category(TestCategory.Integration)]
        [Category(TestCategory.Performance)]
        [Category(TestCategory.WindowsForms)]
        [Ignore("Times-out on Build Server, needs to be run manually :(")]
        public void GivenEmptyFMModel_Import_LandBoundaries_And_Grid_Then_Open_RGFGrid_ShouldBeFast()
        {
            var landBoundaryPath = TestHelper.GetTestFilePath(@"D3DFMIQ-16\sealand.ldb");
            landBoundaryPath = TestHelper.CreateLocalCopy(landBoundaryPath);
            var netFile = TestHelper.GetTestFilePath(@"D3DFMIQ-16\westerscheldt04_net.nc");
            netFile = TestHelper.CreateLocalCopy(netFile);

            try
            {
                using (var gui = new DeltaShellGui())
                {
                    var app = gui.Application;
                    app.Plugins.Add(new SharpMapGisApplicationPlugin());
                    app.Plugins.Add(new NetworkEditorApplicationPlugin());
                    app.Plugins.Add(new FlowFMApplicationPlugin());

                    gui.Plugins.Add(new ProjectExplorerGuiPlugin());
                    gui.Plugins.Add(new SharpMapGisGuiPlugin());
                    gui.Plugins.Add(new FlowFMGuiPlugin(){GridHandler = null}); /* Using an extension to override the method. #1#
                    gui.Plugins.Add(new NetworkEditorGuiPlugin());

                    gui.Run();

                    Action mainWindowShown = delegate
                    {
                        var targetModel = ImportLdbAndGrid(app, landBoundaryPath, netFile);

                        //Open grid
                        /* Before fixes from rev 39122 (D3DFMIQ-16) performance was between 160 and 190 seconds. #1#
                        /* Personal machine : 5000ms avg.(5secs) #1#
                        /* x1.5 factor acceptance factor #1#
                        /* x3 factor TeamCity acceptance factor #1#
                        TestHelper.AssertIsFasterThan(22500, () => gui.CommandHandler.OpenView(targetModel.Grid, typeof(WaterFlowFMModelView)));
                    };
                    WpfTestHelper.ShowModal((Control)gui.MainWindow, mainWindowShown);
                }
            }
            catch (Exception e)
            {
                Assert.Fail("Unexpected exception: {0}", e.Message);
            }
            finally
            {
                //Clean directory
                FileUtils.DeleteIfExists(landBoundaryPath);
                FileUtils.DeleteIfExists(netFile);
            }
        }

        [Test]
        [Category(TestCategory.Integration)]
        [Category(TestCategory.Performance)]
        public void GivenEmptyFMModel_Import_LandBoundaries_And_Grid_Then_WriteLandBoundaries_ShouldBeFast()
        {
            var landBoundaryPath = TestHelper.GetTestFilePath(@"D3DFMIQ-16\sealand.ldb");
            landBoundaryPath = TestHelper.CreateLocalCopy(landBoundaryPath);
            var netFile = TestHelper.GetTestFilePath(@"D3DFMIQ-16\westerscheldt04_net.nc");
            netFile = TestHelper.CreateLocalCopy(netFile);
            try
            {
                using (var app = new DeltaShellApplication() {IsProjectCreatedInTemporaryDirectory = true})
                {
                    app.Plugins.Add(new SharpMapGisApplicationPlugin());
                    app.Plugins.Add(new NetworkEditorApplicationPlugin());
                    app.Plugins.Add(new FlowFMApplicationPlugin());

                    app.Run();

                    var targetModel = ImportLdbAndGrid(app, landBoundaryPath, netFile);

                    /* Personal machine : 330ms avg. #1#
                    /* x1.5 factor acceptance factor #1#
                    /* x3 factor TeamCity acceptance factor #1#
                    TestHelper.AssertIsFasterThan(1500,
                        () => new MduFile().WriteLandBoundaries(targetModel.MduFilePath, targetModel.ModelDefinition,
                            targetModel.Area));
                }
            }
            catch (Exception e)
            {
                Assert.Fail("Unexpected exception: {0}", e.Message);
            }
            finally
            {
                //Clean directory
                FileUtils.DeleteIfExists(landBoundaryPath);
                FileUtils.DeleteIfExists(netFile);
            }
        }


        [Test]
        [Category(TestCategory.Integration)]
        [Category(TestCategory.WindowsForms)]
        [Ignore("Times-out on Build Server, needs to be run manually :(")]
        public void AfterLoading_Grid_Map_Is_ZoomToExtents()
        {
            var netFile = TestHelper.GetTestFilePath(@"D3DFMIQ-16\westerscheldt04_net.nc");
            netFile = TestHelper.CreateLocalCopy(netFile);

            using (var gui = new DeltaShellGui())
            {
                var app = gui.Application;
                app.Plugins.Add(new SharpMapGisApplicationPlugin());
                app.Plugins.Add(new NetworkEditorApplicationPlugin());
                app.Plugins.Add(new FlowFMApplicationPlugin());

                gui.Plugins.Add(new ProjectExplorerGuiPlugin());
                gui.Plugins.Add(new SharpMapGisGuiPlugin());
                gui.Plugins.Add(new FlowFMGuiPlugin { GridHandler = null }); /* Using an extension to override the method. #1#
                gui.Plugins.Add(new NetworkEditorGuiPlugin());

                gui.Run();

                Action mainWindowShown = delegate
                {
                    var fmModel = AddFMModelToProject(app);

                    //We replicate here what the LoadGrid from RGFGrid does when importing an existing grid, but without triggering further events.
                    //We don't want to use the NetFileImporter for the same reason.
                    File.Copy(netFile, fmModel.NetFilePath, true);
                    fmModel.ModelDefinition.GetModelProperty(KnownProperties.NetFile).SetValueAsString(Path.GetFileName(fmModel.NetFilePath));

                    //First call to initialize the view.
                    gui.CommandHandler.OpenView(fmModel, typeof(WaterFlowFMModelView));

                    //Open grid
                    gui.CommandHandler.OpenView(fmModel.Grid, typeof(WaterFlowFMModelView));
                    gui.DocumentViews.AllViews.OfType<WaterFlowFMModelView>().FirstOrDefault();
                    
                    //Get the view
                    var targetView = gui.DocumentViews.AllViews.OfType<WaterFlowFMModelView>().FirstOrDefault();
                    Assert.IsNotNull(targetView);

                    // Get the height and Width we got from the map after the above actions.
                    var map = targetView.Layer.Map;
                    var orHeight = targetView.Layer.Map.Envelope.Height;
                    var orWidth = targetView.Layer.Map.Envelope.Width;

                    // Changing the zoom will modify the height and width of the map.
                    map.Zoom = 1000;
                    Assert.AreNotEqual(orHeight, map.Envelope.Height);
                    Assert.AreNotEqual(orWidth, map.Envelope.Width);

                    //Apply the zoom to extents and check the height and width match the original values.
                    map.ZoomToExtents();
                    Assert.AreEqual(orHeight, map.Envelope.Height);
                    Assert.AreEqual(orWidth, map.Envelope.Width);
                };
                WpfTestHelper.ShowModal((Control)gui.MainWindow, mainWindowShown);
            }
            //Clean directory
            FileUtils.DeleteIfExists(netFile);
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void TestGetSnappedBoundaryConditionWithNoPreviousSnappedFeatures()
        {
            var importer = new FlowFMNetFileImporter();
            Assert.IsNotNull(importer);
            //Using some example files from another tests
            var netFile = TestHelper.GetTestFilePath(@"D3DFMIQ-16\westerscheldt04_net.nc");
            netFile = TestHelper.CreateLocalCopy(netFile);
            Assert.IsTrue(File.Exists(netFile));

            using (var gui = new DeltaShellGui())
            {
                var app = gui.Application;
                app.Plugins.Add(new SharpMapGisApplicationPlugin());
                app.Plugins.Add(new NetworkEditorApplicationPlugin());
                app.Plugins.Add(new FlowFMApplicationPlugin());

                gui.Plugins.Add(new ProjectExplorerGuiPlugin());
                gui.Plugins.Add(new SharpMapGisGuiPlugin());
                gui.Plugins.Add(new FlowFMGuiPlugin { GridHandler = null }); /* Using an extension to override the method. #1#
                gui.Plugins.Add(new NetworkEditorGuiPlugin());

                gui.Run();

                var fmModel = AddFMModelToProject(app);

                //We replicate here what the LoadGrid from RGFGrid does when importing an existing grid, but without triggering further events.
                //We don't want to use the NetFileImporter for the same reason.
                File.Copy(netFile, fmModel.NetFilePath, true);
                fmModel.ModelDefinition.GetModelProperty(KnownProperties.NetFile).SetValueAsString(Path.GetFileName(fmModel.NetFilePath));

                var boundary = new Feature2D{ Geometry = new LineString(new[] { new Coordinate(0.0, 0.0), new Coordinate(100.0, 100.0) }) };
                fmModel.Boundaries.Add(boundary);

                Assert.DoesNotThrow(
                    () =>
                        fmModel.GetGridSnappedGeometry(UnstrucGridOperationApi.Boundary, boundary.Geometry)
                );
            }

            FileUtils.DeleteIfExists(netFile);
        }

        [Test]
        [Category(TestCategory.Integration)]
        [Ignore("Times-out on Build Server, needs to be run manually :(")]
        public void TestGetSnappedPumpAndDryPointWithNoPreviousSnappedFeatures()
        {
            var importer = new FlowFMNetFileImporter();
            Assert.IsNotNull(importer);
            //Using some example files from another tests
            var netFile = TestHelper.GetTestFilePath(@"D3DFMIQ-16\westerscheldt04_net.nc");
            netFile = TestHelper.CreateLocalCopy(netFile);
            Assert.IsTrue(File.Exists(netFile));

            using (var gui = new DeltaShellGui())
            {
                var app = gui.Application;
                app.Plugins.Add(new SharpMapGisApplicationPlugin());
                app.Plugins.Add(new NetworkEditorApplicationPlugin());
                app.Plugins.Add(new FlowFMApplicationPlugin());

                gui.Plugins.Add(new ProjectExplorerGuiPlugin());
                gui.Plugins.Add(new SharpMapGisGuiPlugin());
                gui.Plugins.Add(new FlowFMGuiPlugin { GridHandler = null }); /* Using an extension to override the method. #1#
                gui.Plugins.Add(new NetworkEditorGuiPlugin());

                gui.Run();

                var fmModel = AddFMModelToProject(app);

                //We replicate here what the LoadGrid from RGFGrid does when importing an existing grid, but without triggering further events.
                //We don't want to use the NetFileImporter for the same reason.
                File.Copy(netFile, fmModel.NetFilePath, true);
                fmModel.ModelDefinition.GetModelProperty(KnownProperties.NetFile).SetValueAsString(Path.GetFileName(fmModel.NetFilePath));

                var pump2D = new Pump2D{Geometry = new LineString(new[] { new Coordinate(0.0, 0.0), new Coordinate(100.0, 100.0) }) };
                fmModel.Area.Pumps.Add(pump2D);
                var obsPoint = new GroupableFeature2DPoint{ Geometry = new Point(new Coordinate(100.0, 100.0)) };
                fmModel.Area.ObservationPoints.Add(obsPoint);

                Assert.DoesNotThrow(
                    () => { 
                        //Open grid
                        gui.CommandHandler.OpenView(fmModel.Grid, typeof(WaterFlowFMModelView));

                        var mapView = gui.DocumentViews.OfType<ProjectItemMapView>().FirstOrDefault();
                        var modelLayer = (GroupLayer)mapView.MapView.GetLayerForData(fmModel);

                        var snappedLayer =
                            modelLayer.Layers.FirstOrDefault(
                                    l => l.Name == FlowFMMapLayerProvider.GridSnappedFeaturesLayerName) as
                                GroupLayer;
                        Assert.IsNotNull(snappedLayer);
                        //Make sure the layers are visible.
                        snappedLayer.Visible = true;
                        GetSnappedFeatureCollectionFromLayers(snappedLayer.Layers, "Pumps (Snapped)");
                        GetSnappedFeatureCollectionFromLayers(snappedLayer.Layers, "Observation Points (Snapped)");
                    }
                );
            }

            FileUtils.DeleteIfExists(netFile);
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void TestGetSnappedBoundaryConditionThatWillFail_ThenEmptyGeometryCollectionIsReturned()
        {
            var importer = new FlowFMNetFileImporter();
            Assert.IsNotNull(importer);
            //Using some example files from another tests
            var netFile = TestHelper.GetTestFilePath(@"D3DFMIQ-16\westerscheldt04_net.nc");
            netFile = TestHelper.CreateLocalCopy(netFile);
            Assert.IsTrue(File.Exists(netFile));

            using (var gui = new DeltaShellGui())
            {
                var app = gui.Application;
                app.Plugins.Add(new SharpMapGisApplicationPlugin());
                app.Plugins.Add(new NetworkEditorApplicationPlugin());
                app.Plugins.Add(new FlowFMApplicationPlugin());

                gui.Plugins.Add(new ProjectExplorerGuiPlugin());
                gui.Plugins.Add(new SharpMapGisGuiPlugin());
                gui.Plugins.Add(new FlowFMGuiPlugin { GridHandler = null }); /* Using an extension to override the method. #1#
                gui.Plugins.Add(new NetworkEditorGuiPlugin());

                gui.Run();

                var fmModel = AddFMModelToProject(app);

                //We replicate here what the LoadGrid from RGFGrid does when importing an existing grid, but without triggering further events.
                //We don't want to use the NetFileImporter for the same reason.
                File.Copy(netFile, fmModel.NetFilePath, true);
                fmModel.ModelDefinition.GetModelProperty(KnownProperties.NetFile).SetValueAsString(Path.GetFileName(fmModel.NetFilePath));

                //This geometry does not match with the grid above imported, so it will fail when trying to snap.
                var boundary = new Feature2D { Geometry = new LineString(new[] { new Coordinate(0.0, 0.0), new Coordinate(100.0, 100.0) }) };
                fmModel.Boundaries.Add(boundary);

                TestHelper.AssertLogMessagesCount(() =>
                {
                    var snappedGeometry =
                        fmModel.GetGridSnappedGeometry(UnstrucGridOperationApi.Boundary, boundary.Geometry);
                    Assert.AreEqual(snappedGeometry, GeometryCollection.Empty);
                }, 0);              
            }


            FileUtils.DeleteIfExists(netFile);
        }

        [Test]
        [Category(TestCategory.Integration)]
        [Category(TestCategory.WindowsForms)]
        [Ignore("Times-out on Build Server, needs to be run manually :(")]
        public void TestGetSnappedFeatureAfterFailGetSnappedBoundaryConditionWillNotCrash()
        {
            var importer = new FlowFMNetFileImporter();
            Assert.IsNotNull(importer);
            //Using some example files from another tests
            var netFile = TestHelper.GetTestFilePath(@"basicGrid\basicGrid_net.nc");
            netFile = TestHelper.CreateLocalCopy(netFile);
            Assert.IsTrue(File.Exists(netFile));

            using (var gui = new DeltaShellGui())
            {
                var app = gui.Application;
                app.Plugins.Add(new SharpMapGisApplicationPlugin());
                app.Plugins.Add(new NetworkEditorApplicationPlugin());
                app.Plugins.Add(new FlowFMApplicationPlugin());

                gui.Plugins.Add(new ProjectExplorerGuiPlugin());
                gui.Plugins.Add(new SharpMapGisGuiPlugin());
                gui.Plugins.Add(new FlowFMGuiPlugin
                {
                    GridHandler = null
                }); /* Using an extension to override the method. #1#
                gui.Plugins.Add(new NetworkEditorGuiPlugin());
                gui.Run();

                var fmModel = AddFMModelToProject(app);

                //We replicate here what the LoadGrid from RGFGrid does when importing an existing grid, but without triggering further events.
                //We don't want to use the NetFileImporter for the same reason.
                File.Copy(netFile, fmModel.NetFilePath, true);
                fmModel.ModelDefinition.GetModelProperty(KnownProperties.NetFile)
                    .SetValueAsString(Path.GetFileName(fmModel.NetFilePath));

                ImportGrid(app, netFile, fmModel);

                //Open grid
                gui.CommandHandler.OpenView(fmModel.Grid, typeof(WaterFlowFMModelView));

                var mapView = gui.DocumentViews.OfType<ProjectItemMapView>().FirstOrDefault();
                var modelLayer = (GroupLayer)mapView.MapView.GetLayerForData(fmModel);

                var snappedLayer =
                    modelLayer.Layers.FirstOrDefault(
                            l => l.Name == FlowFMMapLayerProvider.GridSnappedFeaturesLayerName) as
                        GroupLayer;
                Assert.IsNotNull(snappedLayer);
                //Make sure the layers are visible.
                snappedLayer.Visible = false;
                var boundary = new Feature2D
                {
                    Geometry = new LineString(new[] { new Coordinate(-10.0, 900.0), new Coordinate(-10.0, 100.0) })
                };
                fmModel.Boundaries.Add(boundary);
                snappedLayer.Visible = true;

                var thinDam2D = new ThinDam2D
                {
                    Geometry = new LineString(new[] { new Coordinate(300.0, 15.0), new Coordinate(1200.0, 1250.0) })
                };

                WpfTestHelper.ShowModal((Control)gui.MainWindow, () =>
                {
                    try
                    {
                        gui.CommandHandler.OpenView(fmModel.Grid, typeof(WaterFlowFMModelView));
                        fmModel.Area.ThinDams.Add(thinDam2D);
                    }
                    catch (Exception e)
                    {
                        Assert.Fail("Not expected to throw an exception here. {0}", e.Message);
                    }
                });
            }

            FileUtils.DeleteIfExists(netFile);
        }
 */
        [Test]
        [Category(TestCategory.Performance)]
        [Category(TestCategory.Slow)]
        public void ImportingOfDryPointsWithProjectItemMapViewOpenShouldBeFast()
        {
            using (var gui = new DeltaShellGui())
            {
                //setup env
                var app = gui.Application;

                app.Plugins.Add(new CommonToolsApplicationPlugin());
                app.Plugins.Add(new NetworkEditorApplicationPlugin());
                app.Plugins.Add(new SharpMapGisApplicationPlugin());
                app.Plugins.Add(new FlowFMApplicationPlugin());

                gui.Plugins.Add(new ProjectExplorerGuiPlugin());
                gui.Plugins.Add(new SharpMapGisGuiPlugin());
                gui.Plugins.Add(new NetworkEditorGuiPlugin());
                gui.Plugins.Add(new FlowFMGuiPlugin());
                gui.Run();

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
            using (var gui = new DeltaShellGui())
            {
                var app = gui.Application;
                app.Plugins.Add(new NetworkEditorApplicationPlugin());
                app.Plugins.Add(new SharpMapGisApplicationPlugin());
                app.Plugins.Add(new CommonToolsApplicationPlugin());
                app.Plugins.Add(new FlowFMApplicationPlugin());

                gui.Plugins.Add(new ProjectExplorerGuiPlugin());
                gui.Plugins.Add(new CommonToolsGuiPlugin());
                gui.Plugins.Add(new SharpMapGisGuiPlugin());
                gui.Plugins.Add(new NetworkEditorGuiPlugin());
                gui.Plugins.Add(new FlowFMGuiPlugin());

                gui.Run();

                app.UserSettings["autosaveWindowLayout"] = false; // skip damagin of window layout
                Action formVisibleChangedAction = () =>
                {
                    using (var model = new WaterFlowFMModel())
                    {
                        app.Project.RootFolder.Add(model);
                        gui.CommandHandler.OpenView(model);
                        var network = model.Network;

                        network.BeginEdit(new DefaultEditAction("Adding pipe..."));
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
    }
}