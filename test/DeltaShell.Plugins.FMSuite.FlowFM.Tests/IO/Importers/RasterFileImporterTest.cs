using System;
using System.Linq;
using System.Threading;
using System.Windows.Controls;
using DelftTools.Controls;
using DelftTools.Shell.Core;
using DelftTools.Shell.Gui;
using DelftTools.TestUtils;
using DelftTools.Utils.IO;
using DeltaShell.NGHS.IO.Grid;
using DeltaShell.NGHS.TestUtils.Builders;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Importers;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using DeltaShell.Plugins.SharpMapGis.Gui.Forms;
using NetTopologySuite.Extensions.Coverages;
using NetTopologySuite.Extensions.Grids;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.IO.Importers
{
    [TestFixture, Apartment(ApartmentState.STA)]
    public class RasterFileImporterTest
    {

        [Test]
        public void
            Given_ARasterFileImporterFromFmAppPlugin_When_ImportingATwoDecimalAscFileAsGrid_Then_AGridIsReturned()
        {
            try
            {
                var testFilePath = TestHelper.GetTestFilePath(@"RasterImport\ahn_breach.asc");
                var mduFilePath = TestHelper.GetTestFilePath(@"RasterImport\FlowFM.mdu");

                using (var app = new DHYDROApplicationBuilder().WithFlowFM().Build())
                {
                    app.Run();
                    IProjectService projectService = app.ProjectService;
                    Project project = projectService.CreateProject();
                    
                    using (var fmModel = new WaterFlowFMModel(mduFilePath))
                    {
                        project.RootFolder.Add(fmModel);

                        var rasterFileImporter = new RasterFileImporter();
                        rasterFileImporter.RegisterGetModelFunction<UnstructuredGrid>(grid => fmModel);
                        rasterFileImporter.RegisterGetModelFunction<UnstructuredGridCoverage>(grid => fmModel);

                        var expectedGrid =
                            rasterFileImporter.ImportItem(testFilePath, fmModel.Grid) as UnstructuredGrid;
                        Assert.IsNotNull(expectedGrid);

                        const int expectedCells = 252 * 173;
                        Assert.AreEqual(expectedCells, expectedGrid.Cells.Count);

                        const int expectedVertices = (252 + 1) * (173 + 1);
                        Assert.AreEqual(expectedVertices, expectedGrid.Vertices.Count);

                        const int expectedEdges = 87617;
                        Assert.AreEqual(expectedEdges, expectedGrid.Edges.Count);
                    }
                }
            }
            finally
            {
                var netFilePath = TestHelper.GetTestFilePath(@"RasterImport\FlowFM_net.nc");
                FileUtils.DeleteIfExists(netFilePath);
            }
        }

        [Test]
        public void Given_ARasterFileImporter_When_ImportingATwoDecimalAscFileAsGrid_Then_AGridIsReturned()
        {
            try
            {
                var mduFilePath = TestHelper.GetTestFilePath(@"RasterImport\FlowFM.mdu");
                var testFilePath = TestHelper.GetTestFilePath(@"RasterImport\ahn_breach.asc");
                var importer = new RasterFileImporter();
                var fmModel = new WaterFlowFMModel(mduFilePath);
                importer.RegisterGetModelFunction<UnstructuredGrid>(g => fmModel);

                var expectedGrid = importer.ImportItem(testFilePath, fmModel.Grid) as UnstructuredGrid;
                Assert.IsNotNull(expectedGrid);

                const int expectedCells = 252 * 173;
                Assert.AreEqual(expectedCells, expectedGrid.Cells.Count);

                const int expectedVertices = (252 + 1) * (173 + 1);
                Assert.AreEqual(expectedVertices, expectedGrid.Vertices.Count);

                const int expectedEdges = 87617;
                Assert.AreEqual(expectedEdges, expectedGrid.Edges.Count);

            }
            finally
            {
                var netFilePath = TestHelper.GetTestFilePath(@"RasterImport\FlowFM_net.nc");
                FileUtils.DeleteIfExists(netFilePath);
            }
        }

        [Test]
        public void Given_ARasterFileImporter_When_ImportingAFourDecimalAscFileAsGrid_Then_AGridIsReturned()
        {
            var testFilePath = TestHelper.GetTestFilePath(@"RasterImport\dem319x324_ref_dike.asc");
            var mduFilePath = TestHelper.GetTestFilePath(@"RasterImport\FlowFM.mdu");

            var model = new WaterFlowFMModel {MduFilePath = mduFilePath};

            try
            {
                var importer = new RasterFileImporter();
                importer.RegisterGetModelFunction<UnstructuredGrid>(g => model);
                var expectedGrid = importer.ImportItem(testFilePath, model.Grid) as UnstructuredGrid;
                Assert.IsNotNull(expectedGrid);

                const int expectedCells = 319 * 324;
                Assert.AreEqual(expectedCells, expectedGrid.Cells.Count);

                const int expectedVertices = (319 + 1) * (324 + 1);
                Assert.AreEqual(expectedVertices, expectedGrid.Vertices.Count);

                const int expectedEdges = 207355;
                Assert.AreEqual(expectedEdges, expectedGrid.Edges.Count);
            }
            finally
            {
                var netFilePath = TestHelper.GetTestFilePath(@"RasterImport\FlowFM_net.nc");
                FileUtils.DeleteIfExists(netFilePath);
            }
        }

        [Test]
        public void Given_ARasterFileImporter_When_ImportingATiffFileAsGrid_Then_AGridIsReturned()
        {
            var testFilePath = TestHelper.GetTestFilePath(@"RasterImport\Bed_Levels_SOBEK2.tif");
            var mduFilePath = TestHelper.GetTestFilePath(@"RasterImport\FlowFM.mdu");

            var model = new WaterFlowFMModel {MduFilePath = mduFilePath};

            try
            {
                var importer = new RasterFileImporter();
                importer.RegisterGetModelFunction<UnstructuredGrid>(g => model);
                var expectedGrid = importer.ImportItem(testFilePath, model.Grid) as UnstructuredGrid;
                Assert.IsNotNull(expectedGrid);

                const int expectedCells = 33 * 46;
                Assert.AreEqual(expectedCells, expectedGrid.Cells.Count);

                const int expectedVertices = (33 + 1) * (46 + 1);
                Assert.AreEqual(expectedVertices, expectedGrid.Vertices.Count);

                const int expectedEdges = 3115;
                Assert.AreEqual(expectedEdges, expectedGrid.Edges.Count);
            }
            finally
            {
                var netFilePath = TestHelper.GetTestFilePath(@"RasterImport\FlowFM_net.nc");
                FileUtils.DeleteIfExists(netFilePath);
            }
        }

        [Test]
        public void Given_A2x2Raster_When_ImportingBedLevels_Then_CorrectPointCloudIsReturned()
        {
            var testFilePath = TestHelper.GetTestFilePath(@"RasterImport\ahn_breach.asc");
            var mduFilePath = TestHelper.GetTestFilePath(@"RasterImport\FlowFM.mdu");
            var importer = new RasterFileImporter();
            var model = new WaterFlowFMModel(mduFilePath);
            importer.RegisterGetModelFunction<UnstructuredGrid>(g => model);
            importer.RegisterGetModelFunction<UnstructuredGridCoverage>(coverage => model);
            var grid = importer.ImportItem(testFilePath, model.Grid) as UnstructuredGrid;
            var bedLevels = importer.ImportItem(testFilePath, model.Bathymetry) as UnstructuredGridCoverage;
            Assert.IsNotNull(bedLevels);
            Assert.IsTrue(bedLevels.Components[0].Values.Count == grid.Vertices.Count);
        }
        [Test]
        public void Given_A2x2Raster_When_ReadingBedLevels_Then_CorrectPointCloudIsReturned()
        {
            var testFilePath = TestHelper.GetTestFilePath(@"RasterImport\ahn_breach.asc");
            var bedLevels = RasterFile.ReadPointValues(testFilePath).ToList();
            Assert.IsNotNull(bedLevels);

            Assert.AreEqual(252 * 173 - 2720, bedLevels.Count, "Should be 252 * 173 without 2720 no-data values");
        }

        [Test]
        public void Given_ATiffFile_When_ImportingBedLevelsFromTiff_Then_CorrectPointCloudIsReturned()
        {
            var testFilePath = TestHelper.GetTestFilePath(@"RasterImport\Bed_Levels_SOBEK2.tif");
            var mduFilePath = TestHelper.GetTestFilePath(@"RasterImport\FlowFM.mdu");
            var importer = new RasterFileImporter();
            var model = new WaterFlowFMModel(mduFilePath);
            importer.RegisterGetModelFunction<UnstructuredGrid>(g => model);
            importer.RegisterGetModelFunction<UnstructuredGridCoverage>(coverage => model);
            importer.ImportItem(testFilePath, model.Grid);
            Assert.IsNotNull(model);

            var grid = importer.ImportItem(testFilePath, model.Grid) as UnstructuredGrid;
            var bedLevels = importer.ImportItem(testFilePath, model.Bathymetry) as UnstructuredGridCoverage;
            Assert.IsNotNull(bedLevels);
            Assert.IsTrue(bedLevels.Components[0].Values.Count == grid.Vertices.Count);
        }
        [Test]
        public void Given_ATiffFile_When_ReadingBedLevelsFromTiff_Then_CorrectPointCloudIsReturned()
        {
            var testFilePath = TestHelper.GetTestFilePath(@"RasterImport\Bed_Levels_SOBEK2.tif");
            var bedLevels = RasterFile.ReadPointValues(testFilePath).ToList();
            Assert.IsNotNull(bedLevels);
            Assert.AreEqual(876, bedLevels.Count); //876 points with value others are nodatavalue  
        }
        [Test]
        [Category(TestCategory.WindowsForms)]
        [Category(TestCategory.Slow)]
        public void LoadbedlevelandgridfromrasterwithGui()
        {
            var testFilePath = TestHelper.GetTestFilePath(@"RasterImport\2D_bedlevels_Tutorial2.asc");
            testFilePath = TestHelper.CreateLocalCopy(testFilePath);
            
            using (var gui = CreateGui())
            {
                var app = gui.Application;
                gui.Run();

                Action mainWindowShown = delegate
                {
                    using (var model = new WaterFlowFMModel())
                    {
                        Project project = app.ProjectService.CreateProject();
                        project.RootFolder.Add(model);
                        var facesValue = ((int)BedLevelLocation.Faces).ToString();
                        model.ModelDefinition.GetModelProperty(KnownProperties.BedlevType).SetValueFromString(facesValue);

                        // open view for model
                        gui.CommandHandler.OpenView(model);
                        var importer = new RasterFileImporter();
                        importer.ImportItem(testFilePath, model);
                        // open view for model
                        gui.CommandHandler.OpenView(model);
                        gui.DocumentViews.ActiveView.GetViewsOfType<MapView>().FirstOrDefault()?.Map.ZoomToExtents();
                    }
                };

                WpfTestHelper.ShowModal((Control)gui.MainWindow, mainWindowShown);
            }
        }
        [Test]
        [Category(TestCategory.WindowsForms)]
        [Category(TestCategory.Slow)]
        public void LoadbedlevelandgridfromrasterwithGu1i()
        {
            var testFilePath = TestHelper.GetTestFilePath(@"RasterImport\2D_bedlevels_Tutorial2.asc");
            testFilePath = TestHelper.CreateLocalCopy(testFilePath);
            
            using (var gui = CreateGui())
            {
                gui.Run();

                Action mainWindowShown = delegate
                {
                    using (var model = new WaterFlowFMModel())
                    {
                        var project = gui.Application.ProjectService.CreateProject();
                        project.RootFolder.Add(model);

                        // open view for model
                        gui.CommandHandler.OpenView(model);
                        var importer = new RasterFileImporter();
                        importer.ImportItem(testFilePath, model);
                        // open view for model
                        gui.CommandHandler.OpenView(model);
                        gui.DocumentViews.ActiveView.GetViewsOfType<MapView>().FirstOrDefault()?.Map.ZoomToExtents();
                    }
                };

                WpfTestHelper.ShowModal((Control)gui.MainWindow, mainWindowShown);
            }
        }

        private static IGui CreateGui()
        {
            return new DHYDROGuiBuilder().WithFlowFM().Build();
        }
        [Test]
        [Category(TestCategory.WindowsForms)]
        [Category(TestCategory.Slow)]
        public void LoadbedlevelfromrasterwithGuifirstgridandthenbedlevel()
        {
            var testFilePath = TestHelper.GetTestFilePath(@"RasterImport\2D_bedlevels_Tutorial2.asc");
            testFilePath = TestHelper.CreateLocalCopy(testFilePath);

            

            using (var gui = CreateGui())
            {
                gui.Run();

                Action mainWindowShown = delegate
                {
                    using (var model = new WaterFlowFMModel())
                    {
                        Project project = gui.Application.ProjectService.CreateProject();
                        project.RootFolder.Add(model);
                        var facesValue = ((int)BedLevelLocation.Faces).ToString();
                        model.ModelDefinition.GetModelProperty(KnownProperties.BedlevType).SetValueFromString(facesValue);

                        // open view for model
                        gui.CommandHandler.OpenView(model);
                        var importer = new RasterFileImporter();
                        importer.RegisterGetModelFunction<UnstructuredGrid>(grid => model);
                        importer.RegisterGetModelFunction<UnstructuredGridCoverage>(coverage => model);
                        importer.ImportItem(testFilePath, model.Grid);
                        importer.ImportItem(testFilePath, model.Bathymetry);
                        // open view for model
                        gui.CommandHandler.OpenView(model);
                        gui.DocumentViews.ActiveView.GetViewsOfType<MapView>().FirstOrDefault()?.Map.ZoomToExtents();
                    }
                };

                WpfTestHelper.ShowModal((Control)gui.MainWindow, mainWindowShown);
            }
        }
    }

}

