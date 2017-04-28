using System.Collections.Generic;
using System.IO;
using DelftTools.TestUtils;
using DelftTools.Utils.IO;
using DelftTools.Utils.NetCdf;
using DeltaShell.Plugins.FMSuite.Common.Gui.RgfGrid;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.Gui.RgfGrid
{
    [TestFixture]
    [Category(TestCategory.WindowsForms)]
    public class RgfGridEditorTest
    {
        [Test]
        [Ignore("blocks; local only")]
        public void ShowWithData()
        {
            var mduPath = TestHelper.GetTestFilePath(@"data\f04_bottomfriction\c016_2DConveyance_bend\input\bendprof.mdu");
            mduPath = TestHelper.CreateLocalCopy(mduPath);
            var model = new WaterFlowFMModel(mduPath);
            RgfGridEditor.OpenGrid(model.NetFilePath);
        }

        [Test]
        [Ignore("blocks; local only")]
        public void ShowWithEmptyGrid()
        {
            var model = new WaterFlowFMModel();
            ((IFileBased) model).CreateNew(Path.Combine(Path.GetTempPath(), "model"));
            model.ModelDefinition.GetModelProperty(KnownProperties.NetFile)
                .SetValueAsString(model.Name + "_net.nc");

            RgfGridEditor.OpenGrid(model.NetFilePath, true, new string[0]);
        }

        [Test]
        [Ignore("blocks; local only")]
        public void ShowWithDataAndLandBoundary()
        {
            var mduPath = TestHelper.GetTestFilePath(@"harlingen\har.mdu");
            mduPath = TestHelper.CreateLocalCopy(mduPath);
            var model = new WaterFlowFMModel(mduPath);
            RgfGridEditor.OpenGrid(model.NetFilePath, false, new[] {TestHelper.GetTestFilePath(@"harlingen\Harlingen_haven.ldb")});
        }

        [Test]
        [Ignore("blocks; local only")]
        public void GeneratePolygonsForEmbankments()
        {
            var pointList = new []
            {
                new Coordinate {X = 10, Y = 10},
                new Coordinate {X = 30, Y = 10},
                new Coordinate {X = 50, Y = 20},
                new Coordinate {X = 40, Y = 40},
                new Coordinate {X = 20, Y = 50},
                new Coordinate {X = 0, Y = 30},
                new Coordinate {X = 10, Y = 10},
            };
            var polygons = new List<IPolygon> {new Polygon(new LinearRing(pointList))};
            var gridPath = TestHelper.GetTestFilePath(@"grid_generation\empty_grid.nc");
            gridPath = TestHelper.CreateLocalCopy(gridPath);
            RgfGridEditor.OpenGrid(gridPath, false, polygons, "polygon.pol");

            Assert.IsTrue(new FileInfo(gridPath).Length > 0, "Generated grid file is empty, RGFGrid generation failed.");

            NetCdfFile netCdfFile = NetCdfFile.OpenExisting(gridPath);
            var netlinks = netCdfFile.GetVariableByName("NetLink");
            Assert.IsTrue(netCdfFile.GetSize(netlinks) == 24); // 2 columns, 12 new rows. 
        }

        [Test]
        [Ignore("blocks; local only")]
        public void GenerateAnExtraGrid()
        {
            var pointList = new []
            {
                new Coordinate {X = 110, Y = 10},
                new Coordinate {X = 130, Y = 10},
                new Coordinate {X = 150, Y = 20},
                new Coordinate {X = 140, Y = 40},
                new Coordinate {X = 120, Y = 50},
                new Coordinate {X = 100, Y = 30},
                new Coordinate {X = 110, Y = 10},
            };
            var polygons = new List<IPolygon>{new Polygon(new LinearRing(pointList))};
            var gridPath = TestHelper.GetTestFilePath(@"grid_generation\existing_grid.nc");
            gridPath = TestHelper.CreateLocalCopy(gridPath);
            RgfGridEditor.OpenGrid(gridPath, false, polygons, "polygon.pol");

            Assert.IsTrue(new FileInfo(gridPath).Length > 0, "Generated grid file is empty, RGFGrid generation failed.");

            NetCdfFile netCdfFile = NetCdfFile.OpenExisting(gridPath);
            var netlinks = netCdfFile.GetVariableByName("NetLink");
            Assert.IsTrue(netCdfFile.GetSize(netlinks) == 48); // 2 columns, 12 existing + 12 new rows. 
        }
    }
}