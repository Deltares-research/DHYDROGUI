using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using DelftTools.Controls;
using DelftTools.Functions;
using DelftTools.Hydro;
using DelftTools.Hydro.Area.Objects;
using DelftTools.Shell.Core;
using DelftTools.Shell.Core.Workflow;
using DelftTools.Shell.Core.Workflow.DataItems;
using DelftTools.Shell.Gui;
using DelftTools.Shell.Gui.Swf;
using DelftTools.TestUtils;
using DelftTools.Utils;
using DelftTools.Utils.IO;
using DeltaShell.IntegrationTestUtils.Builders;
using DeltaShell.Plugins.CommonTools;
using DeltaShell.Plugins.CommonTools.Gui;
using DeltaShell.Plugins.CommonTools.Gui.Forms.Functions;
using DeltaShell.Plugins.Data.NHibernate;
using DeltaShell.Plugins.FMSuite.Common.FeatureData;
using DeltaShell.Plugins.FMSuite.FlowFM.Coverages;
using DeltaShell.Plugins.FMSuite.FlowFM.FeatureData;
using DeltaShell.Plugins.FMSuite.FlowFM.FeatureData.SourcesAndSinks;
using DeltaShell.Plugins.FMSuite.FlowFM.Gui;
using DeltaShell.Plugins.FMSuite.FlowFM.Gui.Editors;
using DeltaShell.Plugins.FMSuite.FlowFM.Gui.NodePresenters;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.ImportExport.Importers;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.ImportExport.ImportersExporters;
using DeltaShell.Plugins.FMSuite.FlowFM.Model;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using DeltaShell.Plugins.FMSuite.FlowFM.Tests.Model;
using DeltaShell.Plugins.NetworkEditor;
using DeltaShell.Plugins.NetworkEditor.Gui;
using DeltaShell.Plugins.ProjectExplorer;
using DeltaShell.Plugins.SharpMapGis;
using DeltaShell.Plugins.SharpMapGis.Gui;
using DeltaShell.Plugins.SharpMapGis.Gui.Forms;
using DeltaShell.Plugins.SharpMapGis.Gui.Forms.CoverageViews;
using GeoAPI.Geometries;
using NetTopologySuite.Extensions.Features;
using NetTopologySuite.Geometries;
using NUnit.Framework;
using SharpMap.Api.Layers;
using SharpMap.Layers;
using SharpMap.UI.Tools;
using Control = System.Windows.Controls.Control;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.Gui
{
    [TestFixture]
    public class WaterFlowFMModelGuiIntegrationTest
    {
        [Test]
        [Category(TestCategory.Wpf)]
        public void GivenWaterFlowFmModel_WhenRunningModel_ThenShouldNotCrashWithOldOutputOpen()
        {
            string mduPath = TestHelper.GetTestFilePath(@"data\f04_bottomfriction\c016_2DConveyance_bend\input\bendprof.mdu");
            mduPath = TestHelper.CreateLocalCopy(mduPath);

            var model = new WaterFlowFMModel();
            model.ImportFromMdu(mduPath);

            using (IGui gui = CreateRunningGui())
            {
                Project project = gui.Application.ProjectService.CreateProject();

                Action mainWindowShown = delegate
                {
                    project.RootFolder.Add(model);

                    ActivityRunner.RunActivity(model);
                    Assert.AreEqual(ActivityStatus.Cleaned, model.Status);


                    // open standalone editor for his feature coverage
                    IFunction coverage = model.OutputHisFileStore.Functions.First(f => f.Components[0].Name == "cross_section_discharge");
                    gui.CommandHandler.OpenView(coverage, typeof(CoverageView));

                    Assert.AreEqual(1, gui.DocumentViews.Count);
                    model.ShowModelRunConsole = true;
                    model.Area.ObservationCrossSections.Add(new ObservationCrossSection2D
                    {
                        Name = "newobj",
                        Geometry = new LineString(new[]
                        {
                            new Coordinate(100, 100),
                            new Coordinate(150, 100)
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
        [Category(TestCategory.Wpf)]
        public void GivenWaterFlowFmModel_WhenImportedInRootFolder_ThenShouldBeReplaced()
        {
            string mduPath = TestHelper.GetTestFilePath(@"harlingen\har.mdu");
            mduPath = TestHelper.CreateLocalCopy(mduPath);

            using (IGui gui = CreateRunningGui())
            {
                IApplication app = gui.Application;
                Project project = app.ProjectService.CreateProject();

                Action mainWindowShown = delegate
                {
                    // Add water flow model to project
                    project.RootFolder.Add(new WaterFlowFMModel());

                    // Check model name
                    WaterFlowFMModel targetModel = project.RootFolder.Models.OfType<WaterFlowFMModel>().FirstOrDefault();
                    Assert.IsNotNull(targetModel);
                    Assert.That(targetModel.Name, Does.Contain("FlowFM"));

                    // Import new water flow model
                    WaterFlowFMFileImporter importer = app.FileImporters.OfType<WaterFlowFMFileImporter>().FirstOrDefault();
                    Assert.IsNotNull(importer);
                    importer.ImportItem(mduPath, targetModel);

                    // Check name of imported water flow model
                    targetModel = project.RootFolder.Models.OfType<WaterFlowFMModel>().FirstOrDefault();
                    Assert.IsNotNull(targetModel);
                    Assert.That(targetModel.Name, Does.Contain("har"));
                };
                WpfTestHelper.ShowModal((Control) gui.MainWindow, mainWindowShown);
            }
        }

        [Test]
        [Category(TestCategory.Wpf)]
        public void GivenWaterFlowFmModel_WhenImportedInFolder_ThenShouldBeReplaced()
        {
            string mduPath = TestHelper.GetTestFilePath(@"harlingen\har.mdu");
            mduPath = TestHelper.CreateLocalCopy(mduPath);

            using (IGui gui = CreateRunningGui())
            {
                IApplication app = gui.Application;
                Project project = gui.Application.ProjectService.CreateProject();

                Action mainWindowShown = delegate
                {
                    // Add new folder to project
                    project.RootFolder.Add(new Folder("Test Folder"));

                    // Check folder name
                    Folder testFolder = project.RootFolder.Folders.FirstOrDefault();
                    Assert.IsNotNull(testFolder);
                    Assert.That(testFolder.Name, Does.Contain("Test Folder"));

                    // Add new water flow model to the new folder and check its name
                    testFolder.Add(new WaterFlowFMModel());
                    WaterFlowFMModel targetModel =
                        testFolder.Models.OfType<WaterFlowFMModel>().FirstOrDefault();
                    Assert.IsNotNull(targetModel);
                    Assert.That(targetModel.Name, Does.Contain("FlowFM"));

                    // Import new water flow model
                    WaterFlowFMFileImporter importer = app.FileImporters.OfType<WaterFlowFMFileImporter>().FirstOrDefault();
                    Assert.IsNotNull(importer);
                    importer.ImportItem(mduPath, targetModel);

                    // Check name of imported water flow model
                    targetModel = testFolder.Models.OfType<WaterFlowFMModel>().FirstOrDefault();
                    Assert.IsNotNull(targetModel);
                    Assert.That(targetModel.Name, Does.Contain("har"));
                };
                WpfTestHelper.ShowModal((Control) gui.MainWindow, mainWindowShown);
            }
        }

        /// <summary>
        /// Test if the view of the heat flux model changes after you change
        /// the type of heat flux model in the combobox.
        /// </summary>
        [Test]
        [Category(TestCategory.Wpf)]
        public void GivenWaterFlowFmModel_WhenOnChange_CloseHeatFluxModelViewIsTested()
        {
            var model = new WaterFlowFMModel();

            using (IGui gui = CreateRunningGui())
            {
                Project project = gui.Application.ProjectService.CreateProject();

                Action mainWindowShown = delegate
                {
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
        [Category(TestCategory.Wpf)]
        public void GivenWaterFlowFmModel_WhenDoubleClickingOnMap_ThenOutputCoverageShouldEnableLayerInCentralMap()
        {
            string mduPath = TestHelper.GetTestFilePath(@"data\f04_bottomfriction\c016_2DConveyance_bend\input\bendprof.mdu");
            mduPath = TestHelper.CreateLocalCopy(mduPath);

            var model = new WaterFlowFMModel();
            model.ImportFromMdu(mduPath);

            using (IGui gui = CreateRunningGui())
            {
                Project project = gui.Application.ProjectService.CreateProject();

                Action mainWindowShown = delegate
                {
                    project.RootFolder.Add(model);

                    ActivityRunner.RunActivity(model);
                    Assert.AreEqual(ActivityStatus.Cleaned, model.Status);

                    // try from scratch:
                    DoubleClickOutputItemAndAssertLayerIsOn(model, gui, "s1");

                    // close all views:
                    gui.DocumentViews.Clear();
                    Assert.AreEqual(0, gui.DocumentViews.Count);

                    // open central map view
                    gui.CommandHandler.OpenView(model, typeof(ProjectItemMapView));

                    // try with already open view:
                    DoubleClickOutputItemAndAssertLayerIsOn(model, gui, "s1");
                };
                WpfTestHelper.ShowModal((Control) gui.MainWindow, mainWindowShown);
            }
        }

        [Test]
        [Category(TestCategory.Wpf)]
        public void GivenWaterFlowFmModel_WhenDoubleClickingOnHis_ThenOutputCoverageShouldEnableLayerInCentralMap()
        {
            string mduPath =
                TestHelper.GetTestFilePath(@"data\f04_bottomfriction\c016_2DConveyance_bend\input\bendprof.mdu");
            mduPath = TestHelper.CreateLocalCopy(mduPath);

            var model = new WaterFlowFMModel { ShowModelRunConsole = true };
            model.ImportFromMdu(mduPath);

            using (IGui gui = CreateRunningGui())
            {
                Project project = gui.Application.ProjectService.CreateProject();

                Action mainWindowShown = delegate
                {
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
                WpfTestHelper.ShowModal((Control) gui.MainWindow, mainWindowShown);
            }
        }

        [Test]
        [Category(TestCategory.Wpf)]
        [Category(TestCategory.Slow)]
        public void GivenWaterFlowFmModel_WhenShowSnapped_ThenFeatureLayersInMap()
        {
            string mduPath =
                TestHelper.GetTestFilePath(@"harlingen\har.mdu");
            mduPath = TestHelper.CreateLocalCopy(mduPath);

            var model = new WaterFlowFMModel {ShowModelRunConsole = true};
            model.ImportFromMdu(mduPath);

            using (IGui gui = CreateRunningGui())
            {
                Project project = gui.Application.ProjectService.CreateProject();

                Action mainWindowShown = delegate
                {
                    project.RootFolder.Add(model);

                    // open central map view
                    gui.CommandHandler.OpenView(model, typeof(ProjectItemMapView));

                    ILayer gridSnappedFeatureGroupLayer = ((ProjectItemMapView) gui.DocumentViews.ActiveView).MapView.Map.GetAllLayers(true)
                                                                                                             .First(l => l.Name == "Estimated Grid-snapped features");

                    gridSnappedFeatureGroupLayer.Visible = true;

                    ((ProjectItemMapView) gui.DocumentViews.ActiveView).MapView.Refresh();
                };
                WpfTestHelper.ShowModal((Control) gui.MainWindow, mainWindowShown);
            }
        }

        [Test]
        [Category(TestCategory.Wpf)]
        [Category(TestCategory.VerySlow)]
        public void GivenWaterFlowFmModel_WhenRunning_ThenShouldGiveVectorVelocityLayer()
        {
            string mduPath =
                TestHelper.GetTestFilePath(@"harlingen\har.mdu");
            mduPath = TestHelper.CreateLocalCopy(mduPath);

            var model = new WaterFlowFMModel { ShowModelRunConsole = true };
            model.ImportFromMdu(mduPath);

            using (IGui gui = CreateRunningGui())
            {
                Project project = gui.Application.ProjectService.CreateProject();

                Action mainWindowShown = delegate
                {
                    project.RootFolder.Add(model);

                    ActivityRunner.RunActivity(model);

                    // open central map view
                    gui.CommandHandler.OpenView(model, typeof(ProjectItemMapView));

                    IEnumerable<ILayer> layers = ((ProjectItemMapView) gui.DocumentViews.ActiveView).MapView.Map.GetAllLayers(true);

                    ILayer velocityLayer =
                        layers.FirstOrDefault(
                            l => l.Name == "velocity (ucx + ucy)" && l is UnstructuredGridCellVectorCoverageLayer);
                    Assert.NotNull(velocityLayer);
                };

                WpfTestHelper.ShowModal((Control) gui.MainWindow, mainWindowShown);
            }
        }

        [Test]
        [Category(TestCategory.Wpf)]
        [Category(TestCategory.VerySlow)]
        public void ImportModelWithBigNetfileGridIntoProject()
        {
            using (IGui gui = CreateRunningGui())
            {
                Project project = gui.Application.ProjectService.CreateProject();

                Action mainWindowShown = delegate
                {
                    TestHelper.PerformActionInTemporaryDirectory(tempDir =>
                    {
                        var modelFolderName = "ModelWithBigGrid";
                        var netCdfZipFileName = "FlowFM.zip";
                        var uGridZipFileName = "FlowFM_Ugrid.zip";

                        string modelFolder = TestHelper.GetTestFilePath(modelFolderName);
                        string netCdfFilePath = Path.Combine(tempDir, netCdfZipFileName);
                        string uGridFilePath = Path.Combine(modelFolder, uGridZipFileName);

                        FileUtils.CopyDirectory(modelFolder, tempDir, uGridFilePath);
                        ZipFileUtils.Extract(netCdfFilePath, tempDir);

                        Stopwatch timer = StartTimer();

                        var mduFileName = "FlowFM.mdu";
                        WaterFlowFMModel model = ImportModelFromTemporaryDirectory(tempDir, mduFileName);

                        StopTimer(timer);

                        timer.Restart();
                        project.RootFolder.Add(model);

                        StopTimer(timer);
                    });
                };

                WpfTestHelper.ShowModal((Control) gui.MainWindow, mainWindowShown);
            }
        }

        [Test]
        [Category(TestCategory.Wpf)]
        [Category(TestCategory.VerySlow)]
        public void ImportModelWithBigUgridIntoProject()
        {
            using (IGui gui = CreateRunningGui())
            {
                Project project = gui.Application.ProjectService.CreateProject();

                Action mainWindowShown = delegate
                {
                    TestHelper.PerformActionInTemporaryDirectory(tempDir =>
                    {
                        var modelFolderName = "ModelWithBigGrid";
                        var uGridZipFileName = "FlowFM_Ugrid.zip";
                        var netCdfZipFileName = "FlowFM.zip";

                        string modelFolder = TestHelper.GetTestFilePath(modelFolderName);
                        string netCdfFilePath = Path.Combine(tempDir, netCdfZipFileName);
                        string uGridFilePath = Path.Combine(modelFolder, uGridZipFileName);

                        FileUtils.CopyDirectory(modelFolder, tempDir, netCdfFilePath);
                        ZipFileUtils.Extract(uGridFilePath, tempDir);

                        Stopwatch timer = StartTimer();

                        var mduFileName = "FlowFMUgrid.mdu";
                        WaterFlowFMModel model = ImportModelFromTemporaryDirectory(tempDir, mduFileName);

                        StopTimer(timer);

                        timer.Restart();
                        project.RootFolder.Add(model);

                        StopTimer(timer);
                    });
                };

                WpfTestHelper.ShowModal((Control) gui.MainWindow, mainWindowShown);
            }
        }

        [Test]
        [Category(TestCategory.Performance)]
        [Category(TestCategory.Slow)]
        public void ImportingOfDryPointsWithProjectItemMapViewOpenShouldBeFast()
        {
            using (IGui gui = CreateRunningGui())
            {
                Project project = gui.Application.ProjectService.CreateProject();

                //create and add a HydroRegion with a HydroArea with DryPoints
                var area = new HydroArea();
                var hydroRegion = new HydroRegion
                {
                    Name = "Hydro region",
                    SubRegions = {area}
                };
                var dataItem = new DataItem(hydroRegion);

                var waterFlowFMModel = new WaterFlowFMModel();
                waterFlowFMModel.Area = area;

                project.RootFolder.Add(waterFlowFMModel);
                project.RootFolder.Add(hydroRegion);

                WpfTestHelper.ShowModal((Control) gui.MainWindow, () =>
                {
                    //load needed views
                    gui.CommandHandler.OpenView(dataItem, typeof(ProjectItemMapView));
                    ProjectItemMapView projectItemMapView = gui.DocumentViews.OfType<ProjectItemMapView>().FirstOrDefault();
                    Assert.NotNull(projectItemMapView);

                    //importing harlingen point ~ 28800 points... this took over 15 min to load
                    string fmtestPath = TestHelper.GetTestDataDirectoryPathForAssembly(typeof(WaterFlowFMModelTest).Assembly);
                    string xyzPath = Path.Combine(fmtestPath, @"harlingen_model_3d\har_V3.xyz");
                    var selection = new DataItem(area.DryPoints);

                    gui.Selection = selection;

                    //start the import and check the speed (TOOLS-21888)
                    TestHelper.AssertIsFasterThan(20000, () =>
                    {
                        gui.CommandHandler.ImportFilesToGuiSelection(new[]
                        {
                            xyzPath
                        });
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
                    TestHelper.AssertIsFasterThan(4400, () => gui.Selection = area.DryPoints);
                    Assert.AreEqual(SelectTool.MaxSelectedFeatures, projectItemMapView.MapView.MapControl.SelectedFeatures.Count());
                });
            }
        }

        [Test]
        [Category(TestCategory.Wpf)]
        public void Given_WaterFlowFmModel_With_MultipleFunctionView_When_CloseProject_Then_MultipleFunctionView_Is_Closed()
        {
            // 1. Prepare test data
            string fileLocation = TestHelper.GetTestFilePath(@"DELFT3DFM-1178\FlowFM");

            // 2. Set up test action
            Action<IGui> testAction = gui => gui.CommandHandler.CloseProject();

            // 3. Run and verify test
            AssertMultipleFunctionViewClosedAsExpected(fileLocation, testAction);
        }

        [Test]
        [Category(TestCategory.Wpf)]
        public void Given_WaterFlowFmModel_With_MultipleFunctionView_When_DeleteModel_Then_MultipleFunctionView_Is_Closed()
        {
            // 1. Prepare test data
            string fileLocation = TestHelper.GetTestFilePath(@"DELFT3DFM-1178\FlowFM");

            // 2. Set up test action
            Action<IGui> testAction = gui =>
            {
                Project project = gui.Application.ProjectService.Project;
                WaterFlowFMModel[] models = project.RootFolder.Models.OfType<WaterFlowFMModel>().ToArray();
                Assert.That(models.Any(), Is.True, "No WaterFlowFMModels were added to the project.");
                foreach (WaterFlowFMModel waterFlowFmModel in models)
                {
                    project.RootFolder.Items.Remove(waterFlowFmModel);
                }
            };

            // 3. Run and verify test
            AssertMultipleFunctionViewClosedAsExpected(fileLocation, testAction);
        }

        [Test]
        [Category(TestCategory.Integration)]
        [Category(TestCategory.Slow)]
        public void GivenAWaterFlowFmModelWithoutOutputHisFileStore_WithMultipleFunctionViewInGui_WhenDeleteModel_ThenDoesNotThrowException()
        {
            using (IGui gui = CreateRunningGui())
            {
                Project project = gui.Application.ProjectService.CreateProject();

                using (var model = new WaterFlowFMModel())
                {
                    project.RootFolder.Add(model);
                    gui.DocumentViews.Add(new MultipleFunctionView());
                    Assert.That(model.OutputHisFileStore, Is.Null);

                    // When
                    void TextAction()
                    {
                        project.RootFolder.Items.Remove(model);
                    }

                    // Then
                    Assert.DoesNotThrow(TextAction);
                }
            }
        }

        [Test]
        [Category(TestCategory.Wpf)]
        public void Given_FlowFmModel_When_Importing_Pli_Files_Then_UpdatesTreeView()
        {
            // 1. Define test data
            var testFmModel = new WaterFlowFMModel();
            var testParentNodeName = "Sources and Sinks";
            string testFileLocation = TestHelper.GetTestFilePath(@"pli_files\waal_test_134_laterals.pli");

            Action<Project> prepareTestProject = (project) => project.RootFolder.Add(testFmModel);
            Func<IGui, ITreeNode> getSourceAndSinksNode = (gui) =>
                gui.MainWindow.ProjectExplorer.TreeView.AllLoadedNodes.FirstOrDefault(
                    ln => ln.Text.Equals(testParentNodeName));
            Func<IGui, PliFileImporterExporter<SourceAndSink, Feature2D>> getImporter = (gui) =>
                gui.Application.FileImporters.OfType<PliFileImporterExporter<SourceAndSink, Feature2D>>()
                   .SingleOrDefault();

            // 2. Define initial expectations
            Action<IGui> verifyInitialExpectations = (gui) =>
            {
                Assert.That(gui, Is.Not.Null, "Gui not initialized correctly.");

                Project project = gui.Application?.ProjectService.Project;
                Assert.That(project, Is.Not.Null, "Project was not initialized");
                Assert.That(project.GetAllItemsRecursive().Contains(testFmModel), Is.True,
                            "Fm Model was not loaded correctly");

                // Verify test elements.
                ITreeNode sourceAndSinksNode = getSourceAndSinksNode(gui);
                Assert.That(sourceAndSinksNode, Is.Not.Null, "Parent node for Sources And Sinks not found.");
                Assert.That(sourceAndSinksNode.Nodes.Any(), Is.False,
                            "Nodes already present under the parent node.");
                Assert.That(testFmModel.SourcesAndSinks.Any(), Is.False, "There are already sources and sinks, the test cannot proceed.");
                Assert.That(getImporter(gui), Is.Not.Null, "No Importer was found, the test cannot proceed.");
            };

            // 3. Define final expectations
            Action<IGui> verifyFinalExpectations = (gui) =>
            {
                Assert.That(testFmModel.SourcesAndSinks.Any(), Is.True, "No sources and sinks were imported.");
                Assert.That(getSourceAndSinksNode(gui).Nodes.Any(), Is.True,
                            "No nodes were added to the project tree folder.");
            };

            // 4. Run test
            using (var dsProjLocation = new TemporaryDirectory())
            {
                string tempFileLocation = dsProjLocation.CopyTestDataFileToTempDirectory(testFileLocation);
                using (IGui gui = CreateRunningGui())
                {
                    Project project = gui.Application.ProjectService.CreateProject();
                    
                    Action mainWindowShown = () =>
                    {
                        prepareTestProject(project);
                        verifyInitialExpectations(gui);
                        PliFileImporterExporter<SourceAndSink, Feature2D> importer = getImporter(gui);
                        var fileImportActivity = new FileImportActivity(importer, testFmModel.SourcesAndSinks)
                        {
                            Files = new[]
                            {
                                tempFileLocation
                            }
                        };
                        gui.Application.RunActivity(fileImportActivity);
                        verifyFinalExpectations(gui);
                    };
                    WpfTestHelper.ShowModal(gui.MainWindow as Control, mainWindowShown);
                    gui.Dispose();
                }
            }
        }

        [Test]
        [Category(TestCategory.Wpf)]
        public void GivenWaterFlowFMModelWithEmptyBoundarySet_WhenOpeningBoundaryConditionsEditor_ThenEditorCorrectlyConfigured()
        {
            // Given
            using (IGui gui = CreateRunningGui())
            using (var model = new WaterFlowFMModel())
            {
                Project project = gui.Application.ProjectService.CreateProject();
                
                var boundaryConditionSet = new BoundaryConditionSet
                {
                    Feature = new Feature2D
                    {
                        Name = "Feature",
                        Geometry = new LineString(new Coordinate[0])
                    }
                };
                model.BoundaryConditionSets.Add(boundaryConditionSet);

                project.RootFolder.Add(model);

                Action mainWindowShown = () =>
                {
                    // When
                    gui.CommandHandler.OpenView(boundaryConditionSet);
                    IView activeView = gui.DocumentViews.ActiveView;

                    // Then
                    Assert.That(activeView, Is.TypeOf<BoundaryConditionEditor>(), $"Associated view with a {typeof(BoundaryConditionSet)} must be of type {typeof(BoundaryConditionEditor)}");

                    var editor = (BoundaryConditionEditor) activeView;
                    Assert.That(editor.SelectedCategory, Is.Not.Null, "The selected category cannot be NULL");
                    Assert.That(editor.Data, Is.SameAs(boundaryConditionSet), "Data on the editor must have the same reference for which it was opened for");

                    var controller = editor.Controller as FlowBoundaryConditionEditorController;
                    Assert.That(controller, Is.Not.Null, "Controller must be instantiated");
                    Assert.That(controller.Model, Is.SameAs(model), "Controller must have the same reference of the model it belongs to");
                };

                WpfTestHelper.ShowModal(gui.MainWindow as Control, mainWindowShown);
                gui.Dispose();
            }
        }

        [Test]
        [Category(TestCategory.Wpf)]
        public void GivenWaterFlowFMModelWithBoundarySet_WhenOpeningBoundaryConditionsEditor_ThenEditorCorrectlyConfigured()
        {
            // Given
            using (IGui gui = CreateRunningGui())
            using (var model = new WaterFlowFMModel())
            {
                var boundaryConditionSet = new BoundaryConditionSet
                {
                    Feature = new Feature2D
                    {
                        Name = "Feature",
                        Geometry = new LineString(new[]
                        {
                            new Coordinate(0, 0),
                            new Coordinate(1, 0),
                            new Coordinate(2, 0)
                        })
                    }
                };
                var boundaryConditions = new[]
                {
                    new FlowBoundaryCondition(FlowBoundaryQuantityType.Salinity, BoundaryConditionDataType.TimeSeries),
                    new FlowBoundaryCondition(FlowBoundaryQuantityType.Temperature, BoundaryConditionDataType.TimeSeries)
                };
                boundaryConditionSet.BoundaryConditions.AddRange(boundaryConditions);
                model.BoundaryConditionSets.Add(boundaryConditionSet);

                Project project = gui.Application.ProjectService.CreateProject();
                project.RootFolder.Add(model);

                Action mainWindowShown = () =>
                {
                    // When
                    gui.CommandHandler.OpenView(boundaryConditionSet);
                    IView activeView = gui.DocumentViews.ActiveView;

                    // Then
                    Assert.That(activeView, Is.TypeOf<BoundaryConditionEditor>(), $"Associated view with a {typeof(BoundaryConditionSet)} must be of type {typeof(BoundaryConditionEditor)}");

                    var editor = (BoundaryConditionEditor) activeView;
                    Assert.That(editor.SelectedCategory, Is.EqualTo("Salinity"), "First initialization with a non-empty set must select the first boundary condition in the set");
                    Assert.That(editor.Data, Is.SameAs(boundaryConditionSet), "Data on the editor must have the same reference for which it was opened for");

                    var controller = editor.Controller as FlowBoundaryConditionEditorController;
                    Assert.That(controller, Is.Not.Null, "Controller must be instantiated");
                    Assert.That(controller.Model, Is.SameAs(model), "Controller must have the same reference of the model it belongs to");
                };

                WpfTestHelper.ShowModal(gui.MainWindow as Control, mainWindowShown);
            }
        }

        [Test]
        [Category(TestCategory.Wpf)]
        [TestCaseSource(nameof(GetConfiguredBoundaryConditionSets))]
        public void GivenWaterFlowFMModelWithBoundarySetAndEditorOpened_WhenOpeningBoundaryConditionsEditorForOtherSet_ThenEditorCorrectlyConfigured(
            IEnumerable<BoundaryConditionSet> boundaryConditionSets)
        {
            // Given
            using (IGui gui = CreateRunningGui())
            using (var model = new WaterFlowFMModel())
            {
                model.BoundaryConditionSets.AddRange(boundaryConditionSets);

                Project project = gui.Application.ProjectService.CreateProject();
                project.RootFolder.Add(model);

                Action mainWindowShown = () =>
                {
                    BoundaryConditionSet boundaryConditionSetOne = boundaryConditionSets.ElementAt(0);
                    BoundaryConditionSet boundaryConditionSetTwo = boundaryConditionSets.ElementAt(1);
                    gui.CommandHandler.OpenView(boundaryConditionSetOne);

                    // When 
                    gui.CommandHandler.OpenView(boundaryConditionSetTwo);
                    IView activeView = gui.DocumentViews.ActiveView;

                    // Then
                    Assert.That(activeView, Is.TypeOf<BoundaryConditionEditor>(), $"Associated view with a {typeof(BoundaryConditionSet)} must be of type {typeof(BoundaryConditionEditor)}");

                    var editor = (BoundaryConditionEditor) activeView;
                    Assert.That(editor.SelectedCategory, Is.EqualTo("Salinity"), "Second initialization of the editor with a non-empty set must select the same category as was selected by the first initialization");
                    Assert.That(editor.Data, Is.SameAs(boundaryConditionSetTwo), "Data on the editor must have the same reference for which it was opened for");
                };

                WpfTestHelper.ShowModal(gui.MainWindow as Control, mainWindowShown);
            }
        }

        private static void AssertMultipleFunctionViewClosedAsExpected(string testDataPath, Action<IGui> guiAction)
        {
            using (var tempDir = new TemporaryDirectory())
            {
                // 1. Load test data
                string modelDir = tempDir.CopyDirectoryToTempDirectory(testDataPath);
                string mduFilePath = Path.Combine(modelDir, "input", "FlowFM.mdu");

                Assert.That(File.Exists(mduFilePath));
                // 2. Prepare Test Project
                using (IGui gui = CreateRunningGui())
                using(var fmModel = new WaterFlowFMModel())
                {
                    Project project = gui.Application.ProjectService.CreateProject();

                    fmModel.LoadFromMdu(mduFilePath);
                    project.RootFolder.Add(fmModel);

                    // 3.1. Verify data loaded correctly.
                    Assert.That(fmModel, Is.Not.Null, "Not found FM Model");

                    TimeSeries hisTimeSerie = fmModel.OutputHisFileStore.Functions
                                                 .OfType<TimeSeries>()
                                                 .FirstOrDefault();
                    Assert.That(hisTimeSerie, Is.Not.Null, "No timeserie was found.");

                    FileBasedFeatureCoverage hisCoverage = fmModel.OutputHisFileStore.Functions
                                                               .OfType<FileBasedFeatureCoverage>()
                                                               .FirstOrDefault();
                    Assert.That(hisCoverage, Is.Not.Null, "No output coverage was found.");

                    // Simulate behaviour what normally will be done if you select observation cross section and then query timeseries
                    IFunction hisTimeSerieForObsCrossSection = hisCoverage.GetTimeSeries(fmModel.Area.ObservationCrossSections.FirstOrDefault());
                    Assert.That(hisCoverage, Is.Not.Null, "No output coverage for the observation cross section was found.");
                    hisTimeSerieForObsCrossSection.Parent = hisCoverage;
                    var list = new List<IFunction>{hisTimeSerieForObsCrossSection};
                    

                    // 4. Do test action
                    Action mainWindowShown = () =>
                    {
                        Assert.That(gui.DocumentViews.Any(), Is.False);

                        // 4.1. Open MultipleFunctionView for his TimeSerie
                        gui.CommandHandler.OpenView(hisTimeSerie);

                        // Normally called by the QueryTimeSeriesMapTool
                        gui.CommandHandler.OpenView(list);

                        Assert.That(gui.DocumentViews.OfType<MultipleFunctionView>().Count(), Is.EqualTo(2), "No MultipleFunction view was generated.");

                        // 5. Do action
                        guiAction(gui);

                        // 6. Verify final expectations
                        Assert.That(gui.DocumentViews.OfType<MultipleFunctionView>().Any(), Is.False, "Not all views were closed correctly.");
                    };
                    WpfTestHelper.ShowModal(gui.MainWindow as Control, mainWindowShown);
                    gui.Dispose();
                }
            }
        }

        #region Helper methods

        private static IGui CreateRunningGui()
        {
            var pluginsToAdd = new List<IPlugin>()
            {
                // Load app plugins
                new NHibernateDaoApplicationPlugin(),
                new CommonToolsApplicationPlugin(),
                new SharpMapGisApplicationPlugin(),
                new NetworkEditorApplicationPlugin(),
                new FlowFMApplicationPlugin(),
                // Load gui plugins
                new CommonToolsGuiPlugin(),
                new ProjectExplorerGuiPlugin(),
                new NetworkEditorGuiPlugin(),
                new SharpMapGisGuiPlugin(),
                new FlowFMGuiPlugin(),
                
            };
            IGui gui = new DeltaShellGuiBuilder().WithPlugins(pluginsToAdd).Build();
            
            gui.Run();

            return gui;
        }

        private static Stopwatch StartTimer()
        {
            var timer = new Stopwatch();
            timer.Start();
            return timer;
        }

        private static void StopTimer(Stopwatch timer)
        {
            timer.Stop();
            Console.WriteLine($"Import time = : {timer.Elapsed}");
        }

        private static WaterFlowFMModel ImportModelFromTemporaryDirectory(string tempDir, string mduFileName)
        {
            string mduPath = Path.Combine(tempDir, mduFileName);

            var model = new WaterFlowFMModel();
            model.ImportFromMdu(mduPath);

            return model;
        }

        private static void DoubleClickOutputItemAndAssertLayerIsOn(WaterFlowFMModel model, IGui gui, string itemName)
        {
            // retrieve the data object for the output waterlevel through the node 
            // presenter (to make sure we use the path double clicking would follow):
            var nodePresenter = new WaterFlowFMModelNodePresenter(null);
            IEnumerable<TreeFolder> childItems = nodePresenter.GetChildNodeObjects(model, null).OfType<TreeFolder>();
            TreeFolder outputFolder = childItems.Last();
            object outputItemNode =
                outputFolder.ChildItems.OfType<object>().First(i => i.ToString().Contains(itemName));

            // mimic double click:
            gui.Selection = outputItemNode;
            gui.CommandHandler.OpenViewForSelection(typeof(ProjectItemMapView));

            Assert.AreEqual(1, gui.DocumentViews.Count);
            MapView activeMapView = FlowFMGuiPlugin.ActiveMapView;
            Assert.IsNotNull(activeMapView, "fm active map view");

            ILayer coverageLayer = activeMapView.Map.GetAllLayers(false).FirstOrDefault(l => l.Name.Contains(itemName));

            Assert.IsNotNull(coverageLayer, "coverage layer not found");
            Assert.IsTrue(coverageLayer.Visible, "not visible");
        }

        private static IEnumerable<TestCaseData> GetConfiguredBoundaryConditionSets()
        {
            var defaultBoundaryConditionsSet = new BoundaryConditionSet
            {
                Feature = new Feature2D
                {
                    Name = "First Set",
                    Geometry = new LineString(new[]
                    {
                        new Coordinate(0, 0),
                        new Coordinate(1, 0),
                        new Coordinate(2, 0)
                    })
                }
            };
            defaultBoundaryConditionsSet.BoundaryConditions.AddRange(new[]
            {
                new FlowBoundaryCondition(FlowBoundaryQuantityType.Salinity, BoundaryConditionDataType.TimeSeries),
                new FlowBoundaryCondition(FlowBoundaryQuantityType.Temperature, BoundaryConditionDataType.TimeSeries)
            });

            yield return new TestCaseData(new List<BoundaryConditionSet>
            {
                defaultBoundaryConditionsSet,
                new BoundaryConditionSet
                {
                    Feature = new Feature2D
                    {
                        Name = "Second Set",
                        Geometry = new LineString(new[]
                        {
                            new Coordinate(0, 0),
                            new Coordinate(1, 0),
                            new Coordinate(2, 0)
                        })
                    }
                }
            }).SetName("Second boundary set empty");

            var boundaryConditionSetWithMatchingBoundaryCondition = new BoundaryConditionSet
            {
                Feature = new Feature2D
                {
                    Name = "Second Set",
                    Geometry = new LineString(new[]
                    {
                        new Coordinate(0, 0),
                        new Coordinate(1, 0),
                        new Coordinate(2, 0)
                    })
                }
            };
            boundaryConditionSetWithMatchingBoundaryCondition.BoundaryConditions.AddRange(new[]
            {
                new FlowBoundaryCondition(FlowBoundaryQuantityType.Salinity, BoundaryConditionDataType.TimeSeries)
            });
            yield return new TestCaseData(new List<BoundaryConditionSet>
            {
                defaultBoundaryConditionsSet,
                boundaryConditionSetWithMatchingBoundaryCondition
            }).SetName("Second boundary set, matching flow boundary condition");
        }

        #endregion
    }
}