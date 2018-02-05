using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Controls;
using DelftTools.Shell.Core;
using DelftTools.Shell.Core.Workflow;
using DelftTools.Shell.Gui;
using DelftTools.Shell.Gui.Swf;
using DelftTools.TestUtils;
using DelftTools.Utils.IO;
using DeltaShell.Core;
using DeltaShell.Gui;
using DeltaShell.Plugins.CommonTools;
using DeltaShell.Plugins.CommonTools.Gui;
using DeltaShell.Plugins.Data.NHibernate;
using DeltaShell.Plugins.FMSuite.FlowFM.Gui;
using DeltaShell.Plugins.FMSuite.FlowFM.Gui.Editors;
using DeltaShell.Plugins.FMSuite.FlowFM.Gui.NodePresenters;
using DeltaShell.Plugins.FMSuite.FlowFM.IO;
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
using NetTopologySuite.Extensions.Features;
using NetTopologySuite.Geometries;
using NetTopologySuite.Extensions.Geometries;
using NUnit.Framework;
using SharpMap.Data.Providers;
using SharpMap.Layers;
using SharpMap.SpatialOperations;
using LandBoundary2D = DelftTools.Hydro.LandBoundary2D;
using ObservationCrossSection2D = DelftTools.Hydro.ObservationCrossSection2D;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.Gui
{
    [TestFixture]
    public class WaterFlowFMModelGuiIntegrationTest
    {
        [Test]
        [Category(TestCategory.WindowsForms)]
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
                    Assert.That(targetModel.Name, Is.StringContaining("FlowFM"));

                    // Import new water flow model
                    var importer = app.FileImporters.OfType<WaterFlowFMFileImporter>().FirstOrDefault();
                    Assert.IsNotNull(importer);
                    importer.ImportItem(mduPath, targetModel);

                    // Check name of imported water flow model
                    targetModel = project.RootFolder.Models.OfType<WaterFlowFMModel>().FirstOrDefault();
                    Assert.IsNotNull(targetModel);
                    Assert.That(targetModel.Name, Is.StringContaining("har"));
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
                    Assert.That(testFolder.Name, Is.StringContaining("Test Folder"));

                    // Add new water flow model to the new folder and check its name
                    testFolder.Add(new WaterFlowFMModel());
                    var targetModel =
                        testFolder.Models.OfType<WaterFlowFMModel>().FirstOrDefault();
                    Assert.IsNotNull(targetModel);
                    Assert.That(targetModel.Name, Is.StringContaining("FlowFM"));

                    // Import new water flow model
                    var importer = app.FileImporters.OfType<WaterFlowFMFileImporter>().FirstOrDefault();
                    Assert.IsNotNull(importer);
                    importer.ImportItem(mduPath, targetModel);

                    // Check name of imported water flow model
                    targetModel = testFolder.Models.OfType<WaterFlowFMModel>().FirstOrDefault();
                    Assert.IsNotNull(targetModel);
                    Assert.That(targetModel.Name, Is.StringContaining("har"));
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
        public void DoubleClickingOnHisOutputCoverageShouldEnableLayerInCentralMap()
        {
            var mduPath =
                TestHelper.GetTestFilePath(@"data\f04_bottomfriction\c016_2DConveyance_bend\input\bendprof.mdu");
            mduPath = TestHelper.CreateLocalCopy(mduPath);

            var model = new WaterFlowFMModel(mduPath) {ShowModelRunConsole = true};
            model.ExplicitWorkingDirectory = model.WorkingDirectory;
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
        [Category(TestCategory.Integration)]
        [Category(TestCategory.Slow)]
        [Category(TestCategory.WindowsForms)]
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
                        .First(l => l.Name == "Grid-snapped features");

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
            ActivityRunner.RunActivity(model);

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


        [Test]
        [Category(TestCategory.Integration)]
        [Category(TestCategory.WindowsForms)]
        [Ignore("Gives out of memory error on build server")]
        public void ImportModelWithBigNetfileGridIntoProject()
        {
            var mduPath = TestHelper.GetTestFilePath(@"ModelWithBigGrid\FlowFM.mdu");
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
                    var timer = new Stopwatch();
                    timer.Start();
                    var model = new WaterFlowFMModel(mduPath);
                    timer.Stop();
                    Console.WriteLine("Import time = : " + timer.Elapsed);

                    timer.Restart();
                    // creates a copy in the temp folder
                    app.Project.RootFolder.Add(model);
                    timer.Stop();
                    Console.WriteLine("Import time = : " + timer.Elapsed);
                };
                WpfTestHelper.ShowModal((Control) gui.MainWindow, mainWindowShown);
            }
        }

        [Test]
        [Category(TestCategory.Integration)]
        [Category(TestCategory.WindowsForms)]
        [Ignore("Gives out of memory error on build server")]
        public void ImportModelWithBigUgridIntoProject()
        {
            var mduPath = TestHelper.GetTestFilePath(@"ModelWithBigGrid\FlowFMUgrid.mdu");
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
                    var timer = new Stopwatch();
                    timer.Start();
                    var model = new WaterFlowFMModel(mduPath);
                    timer.Stop();
                    Console.WriteLine("Import time = : " + timer.Elapsed);

                    timer.Restart();
                    // creates a copy in the temp folder
                    app.Project.RootFolder.Add(model);
                    timer.Stop();
                    Console.WriteLine("Import time = : " + timer.Elapsed);
                };
                WpfTestHelper.ShowModal((Control) gui.MainWindow, mainWindowShown);
            }
        }

        /// <summary>
        /// Test for issue TOOLS_22977, Not working test only reproducing scenario
        /// </summary>
        [Test]
        [Category(TestCategory.Integration)]
        [Category(TestCategory.WindowsForms)]
        [Category(TestCategory.WorkInProgress)]
        [Ignore]
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
        
        [Test]
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
                    gui.Plugins.Add(new FlowFMGuiPlugin(){GridHandler = null}); /* Using an extension to override the method. */
                    gui.Plugins.Add(new NetworkEditorGuiPlugin());

                    gui.Run();

                    Action mainWindowShown = delegate
                    {
                        // Add water flow model to project
                        var project = app.Project;
                        project.RootFolder.Add(new WaterFlowFMModel());

                        // Check model name
                        var targetModel = project.RootFolder.Models.OfType<WaterFlowFMModel>().FirstOrDefault();
                        Assert.IsNotNull(targetModel);
                        Assert.IsFalse(targetModel.Area.LandBoundaries.Any());

                        // Import Land boundaries
                        var importerLDB = app.FileImporters.OfType<LdbFileImporterExporter>().FirstOrDefault();
                        Assert.IsNotNull(importerLDB);

                        var ldbImported = importerLDB.ImportItem(landBoundaryPath, targetModel.Area.LandBoundaries);
                        Assert.IsNotNull(ldbImported as IList<LandBoundary2D>);
                        Assert.IsTrue((ldbImported as IList<LandBoundary2D>).Any());
                        Assert.IsTrue(targetModel.Area.LandBoundaries.Any());

                        //Import grid
                        var importerGrid = app.FileImporters.OfType<FlowFMNetFileImporter>().FirstOrDefault();
                        Assert.IsNotNull(importerGrid);
                        var gridImported = importerGrid.ImportItem(netFile, targetModel.Grid);
                        Assert.IsNotNull(gridImported);

                        //Open grid
                        /* Before fixes from rev 39122 (D3DFMIQ-16) performance was between 160 and 190 seconds. */
                        /* Personal machine : 5000ms avg.(5secs) */
                        /* x1.5 factor acceptance factor */
                        /* x3 factor TeamCity acceptance factor */
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
                    // Add water flow model to project
                    var project = app.Project;
                    project.RootFolder.Add(new WaterFlowFMModel());

                    // Check model name
                    var targetModel = project.RootFolder.Models.OfType<WaterFlowFMModel>().FirstOrDefault();
                    Assert.IsNotNull(targetModel);
                    Assert.IsFalse(targetModel.Area.LandBoundaries.Any());

                    // Import Land boundaries
                    var importerLDB = app.FileImporters.OfType<LdbFileImporterExporter>().FirstOrDefault();
                    Assert.IsNotNull(importerLDB);

                    var ldbImported = importerLDB.ImportItem(landBoundaryPath, targetModel.Area.LandBoundaries);
                    Assert.IsNotNull(ldbImported as IList<LandBoundary2D>);
                    Assert.IsTrue((ldbImported as IList<LandBoundary2D>).Any());
                    Assert.IsTrue(targetModel.Area.LandBoundaries.Any());

                    //Import grid
                    var importerGrid = app.FileImporters.OfType<FlowFMNetFileImporter>().FirstOrDefault();
                    Assert.IsNotNull(importerGrid);
                    var gridImported = importerGrid.ImportItem(netFile, targetModel.Grid);
                    Assert.IsNotNull(gridImported);

                    var writer = new MduFile();
                    var targetMduFilePath = targetModel.MduFilePath;
                    /* Personal machine : 330ms avg. */
                    /* x1.5 factor acceptance factor */
                    /* x3 factor TeamCity acceptance factor */
                    TestHelper.AssertIsFasterThan(1500,
                        () => writer.WriteLandBoundaries(targetMduFilePath, targetModel.ModelDefinition,
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
    }
}