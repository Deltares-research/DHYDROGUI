using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DelftTools.Hydro;
using DelftTools.Hydro.Structures;
using DeltaShell.Plugins.FMSuite.FlowFM.Gui.MapTools;
using GeoAPI.Extensions.Networks;
using GeoAPI.Geometries;
using NetTopologySuite.Extensions.Coverages;
using NetTopologySuite.Extensions.Networks;
using NetTopologySuite.Geometries;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.Gui.MapTools
{
    [TestFixture]
    public class GenerateLinksMapToolHelperTests
    {
        [Test]
        public void GetSelectedPointsTest_OutOfSelectionArea_NoPoints()
        {
            var discretisation = GetTestDiscretization();
            var coordinates = new List<Coordinate>();
            coordinates.Add(new Coordinate(-10,10));
            coordinates.Add(new Coordinate(-100, 10));
            coordinates.Add(new Coordinate(-100, 100));
            coordinates.Add(new Coordinate(-10, 100));
            coordinates.Add(new Coordinate(-10, 10));
            var selectionArea = new Polygon(new LinearRing(coordinates.ToArray()));
            var indexesAndPoints = GenerateLinksMapToolHelper.GetSelectedPoints(discretisation,selectionArea,SewerConnectionWaterType.DryWater);

            Assert.AreEqual(0, indexesAndPoints.Count);
        }

        [Test]
        public void GetSelectedPointsTest_DWA_DWAPoints()
        {
            var discretisation = GetTestDiscretization();
            var coordinates = new List<Coordinate>();
            coordinates.Add(new Coordinate(-10, -10));
            coordinates.Add(new Coordinate(110, -10));
            coordinates.Add(new Coordinate(110, 110));
            coordinates.Add(new Coordinate(-10, 110));
            coordinates.Add(new Coordinate(-10, -10));
            var selectionArea = new Polygon(new LinearRing(coordinates.ToArray()));
            var indexesAndPoints = GenerateLinksMapToolHelper.GetSelectedPoints(discretisation, selectionArea, SewerConnectionWaterType.DryWater);

            Assert.AreEqual(2, indexesAndPoints.Count);

            var index1 = indexesAndPoints[0].Item1;
            var point1 = indexesAndPoints[0].Item2;

            var index2 = indexesAndPoints[1].Item1;
            var point2 = indexesAndPoints[1].Item2;

            Assert.AreEqual(0, index1);
            Assert.AreEqual(0, point1.Coordinate.X);
            Assert.AreEqual(0, point1.Coordinate.Y);

            Assert.AreEqual(1, index2);
            Assert.AreEqual(100, point2.Coordinate.X);
            Assert.AreEqual(0, point2.Coordinate.Y);
        }

        [Test]
        public void GetSelectedPointsTest_HWA_HWAPoints()
        {
            var discretisation = GetTestDiscretization();
            var coordinates = new List<Coordinate>();
            coordinates.Add(new Coordinate(-10, -10));
            coordinates.Add(new Coordinate(110, -10));
            coordinates.Add(new Coordinate(110, 110));
            coordinates.Add(new Coordinate(-10, 110));
            coordinates.Add(new Coordinate(-10, -10));
            var selectionArea = new Polygon(new LinearRing(coordinates.ToArray()));
            var indexesAndPoints = GenerateLinksMapToolHelper.GetSelectedPoints(discretisation, selectionArea, SewerConnectionWaterType.StormWater);

            Assert.AreEqual(2, indexesAndPoints.Count);

            var index1 = indexesAndPoints[0].Item1;
            var point1 = indexesAndPoints[0].Item2;

            var index2 = indexesAndPoints[1].Item1;
            var point2 = indexesAndPoints[1].Item2;

            Assert.AreEqual(2, index1);
            Assert.AreEqual(0, point1.Coordinate.X);
            Assert.AreEqual(10, point1.Coordinate.Y);

            Assert.AreEqual(3, index2);
            Assert.AreEqual(100, point2.Coordinate.X);
            Assert.AreEqual(10, point2.Coordinate.Y);
        }

        private static Discretization GetTestDiscretization()
        {
            var network = new Network();

            var manhole1 = new Manhole("manhole1") {Geometry = new Point(0, 0)};
            var manhole2 = new Manhole("manhole2") {Geometry = new Point(100, 0)};
            var pipeDWA = new Pipe
            {
                Name = "pipeDWA",
                Geometry =
                    new LineString(new[]
                    {
                        new Coordinate(manhole1.XCoordinate, manhole1.YCoordinate),
                        new Coordinate(manhole2.XCoordinate, manhole2.YCoordinate)
                    })
            };
            pipeDWA.WaterType = SewerConnectionWaterType.DryWater;
            pipeDWA.Source = manhole1;
            pipeDWA.Target = manhole2;
            network.Nodes.Add(manhole1);
            network.Nodes.Add(manhole2);
            network.Branches.Add(pipeDWA);

            var manhole3 = new Manhole("manhole3") {Geometry = new Point(0, 10)};
            var manhole4 = new Manhole("manhole4") {Geometry = new Point(100, 10)};
            var pipeHWA = new Pipe
            {
                Name = "pipeHWA",
                Geometry =
                    new LineString(new[]
                    {
                        new Coordinate(manhole3.XCoordinate, manhole3.YCoordinate),
                        new Coordinate(manhole4.XCoordinate, manhole4.YCoordinate)
                    })
            };
            pipeHWA.WaterType = SewerConnectionWaterType.StormWater;
            pipeHWA.Source = manhole3;
            pipeHWA.Target = manhole4;
            network.Nodes.Add(manhole3);
            network.Nodes.Add(manhole4);
            network.Branches.Add(pipeHWA);

            var node1 = new HydroNode() {Name = "node1", Geometry = new Point(0, 20)};
            var node2 = new HydroNode() {Name = "node2", Geometry = new Point(100, 20)};
            var branch1 = new Branch(node1, node2)
            {
                Geometry =
                    new LineString(new[]
                    {new Coordinate(node1.XCoordinate, node1.YCoordinate), new Coordinate(node2.XCoordinate, node2.YCoordinate)})
            };
            network.Nodes.Add(node1);
            network.Nodes.Add(node2);
            network.Branches.Add(branch1);

            var discretisation = new Discretization() {Network = network};

            discretisation.Locations.Values.Add(new NetworkLocation()
            {
                Branch = pipeDWA,
                Chainage = 0,
                Geometry = new Point(0, 0),
                Name = "pipeDWA_begin"
            });

            discretisation.Locations.Values.Add(new NetworkLocation()
            {
                Branch = pipeDWA,
                Chainage = 100,
                Geometry = new Point(100, 0),
                Name = "pipeDWA_end"
            });

            discretisation.Locations.Values.Add(new NetworkLocation()
            {
                Branch = pipeHWA,
                Chainage = 0,
                Geometry = new Point(0, 10),
                Name = "pipeHWA_begin"
            });

            discretisation.Locations.Values.Add(new NetworkLocation()
            {
                Branch = pipeHWA,
                Chainage = 100,
                Geometry = new Point(100, 10),
                Name = "pipeHWA_end"
            });

            discretisation.Locations.Values.Add(new NetworkLocation()
            {
                Branch = branch1,
                Chainage = 0,
                Geometry = new Point(0, 20),
                Name = "branch1_begin"
            });

            discretisation.Locations.Values.Add(new NetworkLocation()
            {
                Branch = branch1,
                Chainage = 100,
                Geometry = new Point(100, 20),
                Name = "branch1_end"
            });

            return discretisation;
        }
    }
}
