using System;
using System.Linq;
using System.Windows;
using DelftTools.Hydro;
using DelftTools.TestUtils;
using DeltaShell.Gui;
using DeltaShell.IntegrationTestUtils;
using DeltaShell.Plugins.CommonTools;
using DeltaShell.Plugins.CommonTools.Gui;
using DeltaShell.Plugins.Data.NHibernate;
using DeltaShell.Plugins.DelftModels.RainfallRunoff;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Concepts;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Meteo;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Gui;
using DeltaShell.Plugins.NetCDF;
using DeltaShell.Plugins.NetworkEditor;
using DeltaShell.Plugins.NetworkEditor.Gui;
using DeltaShell.Plugins.SharpMapGis;
using DeltaShell.Plugins.SharpMapGis.Gui;
using DeltaShell.Plugins.SharpMapGis.Gui.Forms;
using GeoAPI.Extensions.Coverages;
using GeoAPI.Geometries;
using NUnit.Framework;
using Point = NetTopologySuite.Geometries.Point;

namespace Sobek.IntegrationTests
{
    [TestFixture]
    [Category(TestCategory.WindowsForms)]
    [Category(TestCategory.UndoRedo)]
    public class UndoRedoRainfallRunoffCentralMapIntegrationTest : UndoRedoCentralMapTestBase
    {
        private RainfallRunoffModel model;

        [SetUp]
        public void SetUp()
        {
            // sometimes NUnit crashes
            if (gui != null)
            {
                gui.Dispose();
            }

            gui = new DeltaShellGui();
            var app = gui.Application;

            app.Plugins.Add(new CommonToolsApplicationPlugin());
            app.Plugins.Add(new SharpMapGisApplicationPlugin());
            app.Plugins.Add(new NetworkEditorApplicationPlugin());
            app.Plugins.Add(new RainfallRunoffApplicationPlugin());
            app.Plugins.Add(new NHibernateDaoApplicationPlugin());
            app.Plugins.Add(new NetCdfApplicationPlugin());

            gui.Plugins.Add(new CommonToolsGuiPlugin());
            gui.Plugins.Add(new SharpMapGisGuiPlugin());
            gui.Plugins.Add(new NetworkEditorGuiPlugin());
            gui.Plugins.Add(new RainfallRunoffGuiPlugin());


            gui.Run();

            project = app.Project;

            // add data
            model = new RainfallRunoffModel();
            project.RootFolder.Add(model);

            // show gui main window
            mainWindow = (Window)gui.MainWindow;

            // wait until gui starts
            mainWindowShown = () =>
                {
                    basin = model.Basin;
                    gui.CommandHandler.OpenView(model, typeof(ProjectItemMapView));

                    mapView = gui.DocumentViews.OfType<ProjectItemMapView>().FirstOrDefault();

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
            mainWindowShown = null;
            LogHelper.ResetLogging();
        }
        
        [Test]
        public void AddCatchment()
        {
            onMainWindowShown = () =>
                {
                    AddCatchment(new Coordinate(0, 0), CatchmentType.Unpaved);

                    Assert.AreEqual(1, gui.UndoRedoManager.UndoStack.Count());

                    gui.UndoRedoManager.Undo();

                    Assert.AreEqual(0, basin.Catchments.Count);

                    gui.UndoRedoManager.Redo();

                    Assert.AreEqual(1, basin.Catchments.Count);
                };

            WpfTestHelper.ShowModal(mainWindow, mainWindowShown);
        }

        [Test]
        public void ExceptionOnAddingLineShapedCatchmentDoesNotCorruptUndoRedoTools8776()
        {
            onMainWindowShown = () =>
                {
                    var createdCatchment = AddCatchment(new Coordinate(0, 0), CatchmentType.Polder, false, true); //throws exception

                    Assert.IsNull(createdCatchment);
                    Assert.AreEqual(1, gui.UndoRedoManager.UndoStack.Count());

                    gui.UndoRedoManager.Undo(); //NOP

                    Assert.AreEqual(0, basin.Catchments.Count);

                    gui.UndoRedoManager.Redo(); //NOP

                    Assert.AreEqual(0, basin.Catchments.Count);
                };

            WpfTestHelper.ShowModal(mainWindow, mainWindowShown);
        }

        [Test]
        public void AddWasteWaterTreatmentPlant()
        {
            onMainWindowShown = () =>
                {
                    AddWasteWaterTreatmentPlant(new Coordinate(0, 0));

                    Assert.AreEqual(1, gui.UndoRedoManager.UndoStack.Count());

                    gui.UndoRedoManager.Undo();

                    Assert.AreEqual(0, basin.WasteWaterTreatmentPlants.Count);

                    gui.UndoRedoManager.Redo();

                    Assert.AreEqual(1, basin.WasteWaterTreatmentPlants.Count);
                };

            WpfTestHelper.ShowModal(mainWindow, mainWindowShown);
        }

        [Test]
        public void AddRunoffBoundary()
        {
            onMainWindowShown = () =>
                {
                    AddRunoffBoundary(new Coordinate(0, 0));

                    Assert.AreEqual(1, gui.UndoRedoManager.UndoStack.Count());

                    gui.UndoRedoManager.Undo();

                    Assert.AreEqual(0, basin.Boundaries.Count);
                    Assert.AreEqual(0, model.BoundaryData.Count);

                    gui.UndoRedoManager.Redo();

                    Assert.AreEqual(1, basin.Boundaries.Count);
                    Assert.AreEqual(1, model.BoundaryData.Count);
                };

            WpfTestHelper.ShowModal(mainWindow, mainWindowShown);
        }

        [Test]
        public void ChangeMeteoType()
        {
            onMainWindowShown = () =>
                {
                    gui.CommandHandler.OpenView(model.Precipitation);

                    model.Precipitation.Data[new DateTime(2000, 1, 1)] = 5.0;

                    model.Precipitation.DataDistributionType = MeteoDataDistributionType.PerFeature;

                    Assert.AreEqual(2, gui.UndoRedoManager.UndoStack.Count());

                    gui.UndoRedoManager.Undo();

                    Assert.AreEqual(1, gui.UndoRedoManager.UndoStack.Count());
                    Assert.AreEqual(5.0, model.Precipitation.Data[new DateTime(2000, 1, 1)]);

                    gui.UndoRedoManager.Redo();

                    Assert.IsInstanceOf(typeof(IFeatureCoverage), model.Precipitation.Data);
                };

            WpfTestHelper.ShowModal(mainWindow, mainWindowShown);
        }

        [Test]
        public void ChangeMeteoTypeToPerStation()
        {
            onMainWindowShown = () =>
                {
                    gui.UndoRedoManager.TrackChanges = false;
                    gui.CommandHandler.OpenView(model.Precipitation);
                    model.Precipitation.Data[new DateTime(2000, 1, 1)] = 5.0;
                    model.MeteoStations.Add("Station_A");
                    model.MeteoStations.Add("Station_B");

                    gui.UndoRedoManager.TrackChanges = true;

                    // switch type
                    model.Precipitation.DataDistributionType = MeteoDataDistributionType.PerStation;

                    // add an extra station
                    model.MeteoStations.Add("Station_C");

                    // set value
                    model.Precipitation.Data[new DateTime(2000, 1, 1), "Station_C"] = 145.0;

                    Assert.AreEqual(3, gui.UndoRedoManager.UndoStack.Count());

                    // undo value & adding station
                    gui.UndoRedoManager.Undo();
                    gui.UndoRedoManager.Undo();

                    // asserts
                    Assert.AreEqual(1, gui.UndoRedoManager.UndoStack.Count());
                    Assert.AreEqual(2, model.Precipitation.Data.Arguments[1].Values.Count);

                    // undo changing type
                    gui.UndoRedoManager.Undo();

                    // asserts
                    Assert.AreEqual(5.0, model.Precipitation.Data[new DateTime(2000, 1, 1)]);

                    // redo all
                    gui.UndoRedoManager.Redo();
                    gui.UndoRedoManager.Redo();
                    gui.UndoRedoManager.Redo();

                    Assert.AreEqual(3, model.Precipitation.Data.Arguments[1].Values.Count);
                    Assert.AreEqual(3, model.MeteoStations.Count);
                    Assert.AreEqual(145.0, model.Precipitation.Data[new DateTime(2000, 1, 1), "Station_C"]);
                };

            WpfTestHelper.ShowModal(mainWindow, mainWindowShown);
        }

        [Test]
        public void ExceptionUndoingChangeOfWaterLevelBoundaryConditionTools9069()
        {
            onMainWindowShown = () =>
                {
                    // setup model
                    gui.UndoRedoManager.TrackChanges = false;
                    var c1 = AddCatchment(new Coordinate(0, 0));
                    var unpavedModelData = (UnpavedData)model.GetCatchmentModelData(c1);
                    gui.UndoRedoManager.TrackChanges = true;

                    // change boundary water level value
                    unpavedModelData.BoundaryData.Value = 15.0;

                    Assert.AreEqual(1, gui.UndoRedoManager.UndoStack.Count());

                    gui.UndoRedoManager.Undo();

                    Assert.AreEqual(0.0, unpavedModelData.BoundaryData.Value);
                    Assert.AreEqual(0, gui.UndoRedoManager.UndoStack.Count());

                    gui.UndoRedoManager.Redo();
                    Assert.AreEqual(15.0, unpavedModelData.BoundaryData.Value);
                    Assert.AreEqual(1, gui.UndoRedoManager.UndoStack.Count());
                };

            WpfTestHelper.ShowModal(mainWindow, mainWindowShown);
        }

        [Test]
        public void ExceptionChangingSewerSystemInPavedTools9070()
        {
            onMainWindowShown = () =>
                {
                    // setup model
                    gui.UndoRedoManager.TrackChanges = false;
                    var c1 = AddCatchment(new Coordinate(0, 0), CatchmentType.Paved);
                    var pavedModelData = (PavedData)model.GetCatchmentModelData(c1);
                    gui.UndoRedoManager.TrackChanges = true;

                    // open paved editor
                    gui.CommandHandler.OpenView(pavedModelData);

                    // change sewer system type
                    pavedModelData.SewerType = PavedEnums.SewerType.SeparateSystem;
                    pavedModelData.CapacityMixedAndOrRainfall = 15.0;
                    pavedModelData.IsSewerPumpCapacityFixed = false;

                    Assert.AreEqual(3, gui.UndoRedoManager.UndoStack.Count());

                    gui.UndoRedoManager.Undo();
                    gui.UndoRedoManager.Undo();
                    gui.UndoRedoManager.Undo();

                    Assert.AreEqual(0, gui.UndoRedoManager.UndoStack.Count());

                    gui.UndoRedoManager.Redo();
                    gui.UndoRedoManager.Redo();
                    gui.UndoRedoManager.Redo();

                    Assert.AreEqual(3, gui.UndoRedoManager.UndoStack.Count());
                    Assert.AreEqual(PavedEnums.SewerType.SeparateSystem, pavedModelData.SewerType);
                    Assert.AreEqual(15.0, pavedModelData.CapacityMixedAndOrRainfall);
                    Assert.IsFalse(pavedModelData.IsSewerPumpCapacityFixed);
                };

            WpfTestHelper.ShowModal(mainWindow, mainWindowShown);
        }

        [Test]
        public void ChangingBoundaryWaterLevelToTimeSeriesResultsInOneAction()
        {
            onMainWindowShown = () =>
                {
                    // setup model
                    gui.UndoRedoManager.TrackChanges = false;
                    var c1 = AddCatchment(new Coordinate(0, 0));
                    var unpavedModelData = (UnpavedData)model.GetCatchmentModelData(c1);
                    gui.UndoRedoManager.TrackChanges = true;

                    // change boundary water level value
                    unpavedModelData.BoundaryData.IsTimeSeries = true;

                    Assert.AreEqual(1, gui.UndoRedoManager.UndoStack.Count());

                    gui.UndoRedoManager.Undo();

                    Assert.IsFalse(unpavedModelData.BoundaryData.IsTimeSeries);
                    Assert.AreEqual(0, gui.UndoRedoManager.UndoStack.Count());

                    gui.UndoRedoManager.Redo();
                    Assert.IsTrue(unpavedModelData.BoundaryData.IsTimeSeries);
                    Assert.AreEqual(1, gui.UndoRedoManager.UndoStack.Count());
                };

            WpfTestHelper.ShowModal(mainWindow, mainWindowShown);
        }

        [Test]
        public void SetUnpavedCropArea()
        {
            onMainWindowShown = () =>
                {
                    var c1 = AddCatchment(new Coordinate(0, 0));
                    var unpavedModelData = (UnpavedData)model.GetCatchmentModelData(c1);

                    unpavedModelData.AreaPerCrop[UnpavedEnums.CropType.BulbousPlants] = 15.0;

                    Assert.AreEqual(2, gui.UndoRedoManager.UndoStack.Count());

                    gui.UndoRedoManager.Undo();

                    Assert.AreEqual(1, gui.UndoRedoManager.UndoStack.Count());
                    Assert.AreEqual(0.0, unpavedModelData.AreaPerCrop[UnpavedEnums.CropType.BulbousPlants]);

                    gui.UndoRedoManager.Redo();

                    Assert.AreEqual(15.0, unpavedModelData.AreaPerCrop[UnpavedEnums.CropType.BulbousPlants]);
                };

            WpfTestHelper.ShowModal(mainWindow, mainWindowShown);
        }

        [Test]
        public void ResetUnpavedCropArea()
        {
            onMainWindowShown = () =>
                {
                    gui.UndoRedoManager.TrackChanges = false;

                    var c1 = AddCatchment(new Coordinate(0, 0));
                    var unpavedModelData = (UnpavedData)model.GetCatchmentModelData(c1);

                    unpavedModelData.AreaPerCrop[UnpavedEnums.CropType.BulbousPlants] = 15.0;
                    unpavedModelData.AreaPerCrop[UnpavedEnums.CropType.Corn] = 15.0;

                    gui.UndoRedoManager.TrackChanges = true;

                    unpavedModelData.AreaPerCrop.Reset(UnpavedEnums.CropType.Fallow, 10.0);

                    Assert.AreEqual(1, gui.UndoRedoManager.UndoStack.Count());

                    gui.UndoRedoManager.Undo();

                    Assert.AreEqual(0, gui.UndoRedoManager.UndoStack.Count());
                    Assert.AreEqual(15.0, unpavedModelData.AreaPerCrop[UnpavedEnums.CropType.BulbousPlants]);

                    gui.UndoRedoManager.Redo();

                    Assert.AreEqual(0.0, unpavedModelData.AreaPerCrop[UnpavedEnums.CropType.BulbousPlants]);
                    Assert.AreEqual(10.0, unpavedModelData.AreaPerCrop[UnpavedEnums.CropType.Fallow]);
                };

            WpfTestHelper.ShowModal(mainWindow, mainWindowShown);
        }

        [TestFixture]
        [Category(TestCategory.Integration)]
        [Category(TestCategory.UndoRedo)]
        public class ShotgunTest
        {
            [Test]
            [Ignore("too heavy?")]
            public void TestRRAssemblyForSideEffectsInSetters()
            {
                var pointType = typeof(Point); //force type load
                UndoRedoSideEffectTester.TestAssembly(typeof(RainfallRunoffModel).Assembly);
            }
        }
    }
}