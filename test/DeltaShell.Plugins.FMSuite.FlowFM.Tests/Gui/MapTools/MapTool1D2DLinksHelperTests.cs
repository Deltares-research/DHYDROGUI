using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.SewerFeatures;
using DelftTools.Hydro.Structures;
using DelftTools.TestUtils;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.NGHS.IO.Grid.MeshKernel;
using DeltaShell.Plugins.FMSuite.FlowFM.Gui.MapTools;
using GeoAPI.Geometries;
using NetTopologySuite.Extensions.Coverages;
using NetTopologySuite.Extensions.Grids;
using NetTopologySuite.Extensions.Networks;
using NetTopologySuite.Geometries;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.Gui.MapTools
{
    [TestFixture]
    public class MapTool1D2DLinksHelperTests
    {
        [Test]
        public void Get_1DPointsMask_HWA()
        {
            var discretisation = GetTestDiscretization();
            var filter1DPoints = Generate1D2DLinksHelper.GetMesh1DFilter(discretisation, LinkGeneratingType.GullySewer);

            Assert.AreEqual(2, filter1DPoints.Count(p => p));

            Assert.IsTrue(filter1DPoints[2]);
            Assert.IsTrue(filter1DPoints[3]);
        }

        [Test]
        [Category("Quarantine")]
        public void Get_Lateral1D2DLinks() 
        {
            var linkType = LinkGeneratingType.Lateral;
            var discretisation = GetTestDiscretization();

            var model = new WaterFlowFMModel(TestHelper.GetTestFilePath("dummy.mdu"));
            model.Network = (IHydroNetwork)discretisation.Network;
            model.NetworkDiscretization = discretisation;

            var areaCoordinates = new List<Coordinate>();
            areaCoordinates.Add(new Coordinate(-1,-10));
            areaCoordinates.Add(new Coordinate(41,-10));
            areaCoordinates.Add(new Coordinate(35, 20));
            areaCoordinates.Add(new Coordinate(5, 20));
            areaCoordinates.Add(new Coordinate(-1, -10));

            var area = new Polygon(new LinearRing(areaCoordinates.ToArray()));
            
            var links = MapTool1D2DLinksHelper.Generate1D2DLinks(area, linkType, model.Grid, model.Area.Gullies, model.NetworkDiscretization).ToList();

            Assert.AreEqual(1, links.Count);

            Assert.AreEqual(3, links[0].FaceIndex); //3rd face number
            Assert.AreEqual(3, links[0].DiscretisationPointIndex); //2nd discretisation point hwa 

        }

        [Test]
        [Category("Quarantine")]
        public void Get_Gully1D2DLinks()
        {
            var linkType = LinkGeneratingType.GullySewer;
            var discretisation = GetTestDiscretization();

            var model = new WaterFlowFMModel(TestHelper.GetTestFilePath("dummy.mdu"));
            model.Network = (IHydroNetwork)discretisation.Network;
            model.NetworkDiscretization = discretisation;

            var gully1 = new Gully {Geometry = new Point(5, 25)}; //cell 8
            var gully2 = new Gully { Geometry = new Point(35, 5) }; //cell 3

            model.Area.Gullies.Add(gully1);
            model.Area.Gullies.Add(gully2);

            var areaCoordinates = new List<Coordinate>(); //only gully2
            areaCoordinates.Add(new Coordinate(29, -1));
            areaCoordinates.Add(new Coordinate(41, -1));
            areaCoordinates.Add(new Coordinate(41, 11));
            areaCoordinates.Add(new Coordinate(29, 11));
            areaCoordinates.Add(new Coordinate(29, -1));

            var area = new Polygon(new LinearRing(areaCoordinates.ToArray()));
            var links = MapTool1D2DLinksHelper.Generate1D2DLinks(area, linkType, model.Grid, model.Area.Gullies, model.NetworkDiscretization).ToList();

            Assert.AreEqual(1, links.Count);

            Assert.AreEqual(3, links[0].FaceIndex); //3rd face number
            Assert.AreEqual(3, links[0].DiscretisationPointIndex); //2nd discretisation point hwa 
        }

        [Test]
        [Category("Quarantine")]
        public void Get_EmbeddedOneToOneLinks()
        {
            var linkType = LinkGeneratingType.EmbeddedOneToOne;
            var discretisation = GetTestDiscretization();

            var model = new WaterFlowFMModel(TestHelper.GetTestFilePath("dummy.mdu"))
            {
                Network = (IHydroNetwork) discretisation.Network,
                NetworkDiscretization = discretisation
            };

            var areaCoordinates = new List<Coordinate>();
            areaCoordinates.Add(new Coordinate(-1, -10));
            areaCoordinates.Add(new Coordinate(41, -10));
            areaCoordinates.Add(new Coordinate(35, 20));
            areaCoordinates.Add(new Coordinate(5, 20));
            areaCoordinates.Add(new Coordinate(-1, -10));

            var area = new Polygon(new LinearRing(areaCoordinates.ToArray()));
            var links = MapTool1D2DLinksHelper.Generate1D2DLinks(area, linkType, model.Grid, model.Area.Gullies, model.NetworkDiscretization).ToList();

            Assert.AreEqual(1, links.Count);


            Assert.AreEqual(4, links[0].DiscretisationPointIndex); //1st discretisation point branch
            Assert.AreEqual(8, links[0].FaceIndex); //cell 8

            Assert.AreEqual(5, links[1].DiscretisationPointIndex);
            Assert.AreEqual(11, links[1].FaceIndex); //11 face number
        }

        [Test]
        [Category("Quarantine")]
        public void Get_EmbeddedOneToManyLinks()
        {
            var linkType = LinkGeneratingType.EmbeddedOneToOne;
            var discretisation = GetTestDiscretization();

            var model = new WaterFlowFMModel(TestHelper.GetTestFilePath("dummy.mdu"))
            {
                Network = (IHydroNetwork)discretisation.Network,
                NetworkDiscretization = discretisation
            };

            var areaCoordinates = new List<Coordinate>();
            areaCoordinates.Add(new Coordinate(-1, -10));
            areaCoordinates.Add(new Coordinate(41, -10));
            areaCoordinates.Add(new Coordinate(35, 20));
            areaCoordinates.Add(new Coordinate(5, 20));
            areaCoordinates.Add(new Coordinate(-1, -10));

            var area = new Polygon(new LinearRing(areaCoordinates.ToArray()));
            var links = MapTool1D2DLinksHelper.Generate1D2DLinks(area, linkType, model.Grid, model.Area.Gullies, model.NetworkDiscretization).ToList();

            Assert.AreEqual(2, links.Count);

            Assert.AreEqual(4, links[0].DiscretisationPointIndex); //1st discretisation point branch
            Assert.AreEqual(8, links[0].FaceIndex); //cell 8

            Assert.AreEqual(5, links[1].DiscretisationPointIndex);
            Assert.AreEqual(11, links[1].FaceIndex); //11 face number
        }

        private static Discretization GetTestDiscretization()
        {
            var network = new HydroNetwork();

            var manhole1 = new Manhole("manhole1")
            {
                Compartments = new EventedList<ICompartment> { new Compartment()},
                Geometry = new Point(0, 0)
            };
            var manhole2 = new Manhole("manhole2")
            {
                Compartments = new EventedList<ICompartment> { new Compartment() },
                Geometry = new Point(100, 0)
            };
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

            var manhole3 = new Manhole("manhole3")
            {
                Compartments = new EventedList<ICompartment> { new Compartment() },
                Geometry = new Point(0, 10)
            };
            var manhole4 = new Manhole("manhole4")
            {
                Compartments = new EventedList<ICompartment> { new Compartment() },
                Geometry = new Point(100, 10)
            };
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

        private static UnstructuredGrid GetUnstructedTestGrid()
        {
            var grid = new UnstructuredGrid();
            var vertices = new List<Coordinate>();
            vertices.Add(new Coordinate(0, 0)); //1
            vertices.Add(new Coordinate(10, 0));
            vertices.Add(new Coordinate(20, 0));
            vertices.Add(new Coordinate(30, 0));
            vertices.Add(new Coordinate(40, 0));
            vertices.Add(new Coordinate(0, 10)); //6
            vertices.Add(new Coordinate(10, 10));
            vertices.Add(new Coordinate(20, 10));
            vertices.Add(new Coordinate(30, 10));
            vertices.Add(new Coordinate(40, 10));
            vertices.Add(new Coordinate(0, 20)); //11
            vertices.Add(new Coordinate(10, 20));
            vertices.Add(new Coordinate(20, 20));
            vertices.Add(new Coordinate(30, 20));
            vertices.Add(new Coordinate(40, 20));
            vertices.Add(new Coordinate(0, 30)); //16
            vertices.Add(new Coordinate(10, 30));
            vertices.Add(new Coordinate(20, 30));
            vertices.Add(new Coordinate(30, 30));
            vertices.Add(new Coordinate(40, 30)); //20

            var edgesVertexIndices = new[,]
            {
                {1, 2}, {2, 3}, {3, 4}, {4, 5}, //1-4
                {1, 6}, {2, 7}, {3, 8}, {4, 9}, {5, 10} , //5-9

                {6, 7}, {7, 8}, {8, 9}, {9, 10}, //10-13
                {6, 11}, {7, 12}, {8, 13}, {9, 14}, {10, 15} ,//14-18

                {11, 2}, {12, 13}, {13, 14}, {14, 15}, //19-22
                {11, 16}, {12, 17}, {13, 18}, {14, 19}, { 15, 20} , //23-27

                {16, 17}, {17, 18}, {18, 19}, {19, 20} //28-31
            };

            var cellVertexIndices = new[,] //based on vertices (why not edges?)
            {
                {1, 2, 7, 6},  {2, 3, 8, 7},  {3, 4, 9, 8},  {4, 5, 10, 9},
                {6, 7, 12, 11},  {7, 8, 13, 12},  {8, 9, 14, 13},  {9, 10, 15, 14},
                {11, 12, 17, 16},  {12, 13, 18, 17},  {13, 14, 19, 18},  {14, 15, 20, 19}
            };
            UnstructuredGridFactory.CreateFromVertexAndEdgeList(vertices, edgesVertexIndices, cellVertexIndices, grid);

            return grid;
        }
    }
}
