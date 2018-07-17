using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.Structures;
using DeltaShell.NGHS.IO.Grid;
using DeltaShell.Plugins.FMSuite.FlowFM.Gui.MapTools;
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
        public void Get_1DPointsMask_DWA()
        {
            var discretisation = GetTestDiscretization();
            var filter1DPoints = GenerateLinksMapToolHelper.GetMesh1DFilter(discretisation, GridApiDataSet.LinkType.InhabitantsSewer);

            Assert.AreEqual(2, filter1DPoints.Count(p => p));

            Assert.IsTrue(filter1DPoints[0]);
            Assert.IsTrue(filter1DPoints[1]);
        }

        [Test]
        public void Get_1DPointsMask_HWA()
        {
            var discretisation = GetTestDiscretization();
            var filter1DPoints = GenerateLinksMapToolHelper.GetMesh1DFilter(discretisation, GridApiDataSet.LinkType.RoofSewer);

            Assert.AreEqual(2, filter1DPoints.Count(p => p));

            Assert.IsTrue(filter1DPoints[2]);
            Assert.IsTrue(filter1DPoints[3]);
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
