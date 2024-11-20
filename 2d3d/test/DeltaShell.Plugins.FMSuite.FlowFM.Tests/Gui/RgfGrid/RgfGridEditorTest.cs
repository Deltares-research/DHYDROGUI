using System.Collections.Generic;
using System.IO;
using System.Threading;
using DelftTools.TestUtils;
using DeltaShell.NGHS.IO.Grid;
using DeltaShell.Plugins.FMSuite.Common.Gui.RgfGrid;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.Gui.RgfGrid
{
    [Category(TestCategory.WindowsForms)]
    public class RgfGridEditorTest
    {
        [Test]
        [Apartment(ApartmentState.MTA)]
        [Category(TestCategory.Slow)]
        public void GeneratePolygonsForEmbankments()
        {
            var pointList = new[]
            {
                new Coordinate
                {
                    X = 10,
                    Y = 10
                },
                new Coordinate
                {
                    X = 30,
                    Y = 10
                },
                new Coordinate
                {
                    X = 50,
                    Y = 20
                },
                new Coordinate
                {
                    X = 40,
                    Y = 40
                },
                new Coordinate
                {
                    X = 20,
                    Y = 50
                },
                new Coordinate
                {
                    X = 0,
                    Y = 30
                },
                new Coordinate
                {
                    X = 10,
                    Y = 10
                }
            };

            var polygons = new List<IPolygon> {new Polygon(new LinearRing(pointList))};

            TestHelper.PerformActionInTemporaryDirectory(temporaryDir =>
            {
                string gridPath = Path.Combine(temporaryDir, "empty_grid.nc");
                UnstructuredGridFileHelper.WriteEmptyUnstructuredGridFile(gridPath);

                // perform operation
                RgfGridEditor.OpenGrid(gridPath, false, polygons, "polygon.pol");

                Assert.IsTrue(new FileInfo(gridPath).Length > 0, "Generated grid file is empty, RGFGrid generation failed.");

                using (var uGrid = new UGrid(gridPath))
                {
                    int numEdges = uGrid.GetNumberOfEdgesForMeshId(1);
                    Assert.AreEqual(12, numEdges); // 12 new rows. 
                }
            });
        }

        [Test]
        [Apartment(ApartmentState.MTA)]
        [Category(TestCategory.Slow)]
        public void GenerateAnExtraGrid()
        {
            var pointList = new[]
            {
                new Coordinate
                {
                    X = 110,
                    Y = 10
                },
                new Coordinate
                {
                    X = 130,
                    Y = 10
                },
                new Coordinate
                {
                    X = 150,
                    Y = 20
                },
                new Coordinate
                {
                    X = 140,
                    Y = 40
                },
                new Coordinate
                {
                    X = 120,
                    Y = 50
                },
                new Coordinate
                {
                    X = 100,
                    Y = 30
                },
                new Coordinate
                {
                    X = 110,
                    Y = 10
                }
            };
            var polygons = new List<IPolygon> {new Polygon(new LinearRing(pointList))};
            string gridPath = TestHelper.GetTestFilePath(@"grid_generation\existing_grid.nc");
            gridPath = TestHelper.CreateLocalCopy(gridPath);

            // perform operation
            RgfGridEditor.OpenGrid(gridPath, false, polygons, "polygon.pol");

            Assert.IsTrue(new FileInfo(gridPath).Length > 0, "Generated grid file is empty, RGFGrid generation failed.");

            using (var uGrid = new UGrid(gridPath))
            {
                int numEdges = uGrid.GetNumberOfEdgesForMeshId(1);
                Assert.AreEqual(24, numEdges); // 12 existing + 12 new rows.
            }
        }
    }
}