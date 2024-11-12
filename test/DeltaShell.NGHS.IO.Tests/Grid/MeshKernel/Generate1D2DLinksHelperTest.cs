using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using DelftTools.Hydro;
using DelftTools.Hydro.Link1d2d;
using DelftTools.Hydro.SewerFeatures;
using DelftTools.Hydro.Structures;
using DelftTools.TestUtils;
using DelftTools.Utils.Collections.Generic;
using DelftTools.Utils.IO;
using DeltaShell.NGHS.IO.Grid;
using DeltaShell.NGHS.IO.Grid.MeshKernel;
using GeoAPI.Extensions.Networks;
using GeoAPI.Geometries;
using NetTopologySuite.Extensions.Coverages;
using NetTopologySuite.Extensions.Networks;
using NetTopologySuite.Geometries;
using NUnit.Framework;
using SharpMapTestUtils;

namespace DeltaShell.NGHS.IO.Tests.Grid.MeshKernel
{
    [TestFixture]
    public class Generate1D2DLinksHelperTest
    {
        /// <summary>
        /// Tests generating a computational grid filter for 1d2d linking
        /// </summary>
        /// <param name="linkGeneratingType">Type of linking</param>
        /// <param name="expectedFilter">Expected filter in correct order(DWA start,DWA end, HWA start, HWA end, channel1 start, channel1 end)</param>
        /// <param name="message">Message describing the correct behavior</param>
        [Test]
        [TestCase(LinkGeneratingType.Lateral, new[] { false, false, false, false, true, true }, "Should enable only computation points on channels")]
        [TestCase(LinkGeneratingType.EmbeddedOneToOne, new[] { false, false, true, true, true, true }, "Should enable only computation points on channels and non DryWater sewer connections")]
        [TestCase(LinkGeneratingType.EmbeddedOneToMany, new[] { false, false, true, true, true, true }, "Should enable only computation points on channels and non DryWater sewer connections")]
        [TestCase(LinkGeneratingType.GullySewer, new[] { false, false, true,true, true, true }, "Should enable only computation points on channels and Combined/StormWater sewer connections")]
        public void GivenGenerate1D2DLinksHelper_GetMesh1DFilter_ShouldEnableTheRightComputationPoints(LinkGeneratingType linkGeneratingType, bool[] expectedFilter, string message)
        {
            //Arrange
            var discretization = GetTestDiscretization();

            // Act
            var filter1DPoints = Generate1D2DLinksHelper.GetMesh1DFilter(discretization.Locations.Values, linkGeneratingType);

            // Assert
            Assert.AreEqual(expectedFilter, filter1DPoints, message);
        }

        [Test]
        public void GivenGenerate1D2DLinksHelper_GetMesh1DFilterForEmbeddedLinks_ShouldFilterOutNodesToExclude()
        {
            //Arrange
            var discretization = GetTestDiscretization();
            var hydroNetwork = (IHydroNetwork) discretization.Network;

            // Act & Assert
            var filter1DPoints = Generate1D2DLinksHelper.GetMesh1DFilter(discretization.Locations.Values, LinkGeneratingType.EmbeddedOneToMany);
            Assert.AreEqual(new[] { false, false, true, true, true, true }, filter1DPoints);

            var nodesToExclude = new INode[]{hydroNetwork.HydroNodes.First()};

            filter1DPoints = Generate1D2DLinksHelper.GetMesh1DFilter(discretization.Locations.Values, LinkGeneratingType.EmbeddedOneToMany, null, nodesToExclude);
            Assert.AreEqual(new[] { false, false, true, true, false, true }, filter1DPoints, "Computation points that are on nodes in NodesToExclude should be filtered");
        }

        [Test]
        [TestCase(LinkGeneratingType.Lateral, 24)]
        [TestCase(LinkGeneratingType.EmbeddedOneToMany, 22)]
        [TestCase(LinkGeneratingType.EmbeddedOneToOne, 4)]
        [TestCase(LinkGeneratingType.GullySewer, 1)]
        public void GivenGenerate1D2DLinksHelper_Generate1D2DLinks_ShouldCreateCorrectLinks(LinkGeneratingType linkGeneratingType, int expectedNumberOfLinks)
        {
            //Arrange
            var discretization = GetTestDiscretization();
            var area = GetFullArea();
            var unstructuredGrid = UnstructuredGridTestHelper.GenerateRegularGrid(11, 3, 10, 10, -5, -5);

            var gullies = linkGeneratingType == LinkGeneratingType.GullySewer
                              ? new List<Gully>
                              {
                                  new Gully { Geometry = new Point(-30, 22) }, // outside selection area
                                  new Gully { Geometry = new Point(10, 5) }
                              }
                              : new List<Gully>();

            // Act
            var links = Generate1D2DLinksHelper.Generate1D2DLinks(area, linkGeneratingType, unstructuredGrid, gullies, discretization).ToList();

            // Assert
            Assert.AreEqual(expectedNumberOfLinks, links.Count);
        }

        [Test]
        public void GivenGenerate1D2DLinksHelper_Generate1D2DLinks_ShouldWorkWithoutArea()
        {
            //Arrange
            var discretization = GetTestDiscretization();
            var unstructuredGrid = UnstructuredGridTestHelper.GenerateRegularGrid(11, 3, 10, 10, -5, -5);

            var gullies = new List<Gully>();

            // Act & Assert
            var area = GetFullArea();
            var linksWithArea = Generate1D2DLinksHelper.Generate1D2DLinks(area, LinkGeneratingType.EmbeddedOneToOne, unstructuredGrid, gullies, discretization).ToList();
            var linksWithoutArea = Generate1D2DLinksHelper.Generate1D2DLinks(null, LinkGeneratingType.EmbeddedOneToOne, unstructuredGrid, gullies, discretization).ToList();
            
            Assert.AreEqual(linksWithArea.Count, linksWithoutArea.Count, "When generating links without area, the total extend should be used");
        }

        /// <summary>
        /// Tests generating a computational grid filter for 1d2d linking with correct grid cell index when this is reversed
        /// Embedded one to many makes its own 2d administration en thus needs to be mapped. To see the visualized ugrid
        /// entities see issue : FM1D2D-3079
        /// </summary>
        /// <param name="linkGeneratingType">Type of linking</param>
        /// <param name="expectedNumberOfLinks">Nr of links expected to be generated</param>
        [TestCase(LinkGeneratingType.Lateral, 16, 60)]
        [TestCase(LinkGeneratingType.EmbeddedOneToMany, 20, 89)]
        [TestCase(LinkGeneratingType.EmbeddedOneToOne, 3, 79)]
        [TestCase(LinkGeneratingType.GullySewer, 1, 98)]
        public void GivenGenerate1D2DLinksHelper_Generate1D2DLinksWithDifferentCellIndicesAsMK2dMesh_ShouldCreateCorrectLinks(LinkGeneratingType linkGeneratingType, int expectedNumberOfLinks, int firstDiscretizationPointConnectsToCellIndex)
        {
            //Arrange
            var discretization = GetTestChannelDiscretization();
            var unstructuredGrid = UnstructuredGridTestHelper.GenerateRegularGrid(10, 10, 10, 10);
            var area = GetFullArea();
            var gullies = linkGeneratingType == LinkGeneratingType.GullySewer
                              ? new List<Gully>
                              {
                                  new Gully { Geometry = new Point(-30, 22) }, // outside selection area
                                  new Gully { Geometry = new Point(10, 5) }
                              }
                              : new List<Gully>();
            // Act
            //Change (reverse) cell indices so calculation mesh2d is different, similar to a grid geom read grid.
            unstructuredGrid.ResetState(unstructuredGrid.Vertices, unstructuredGrid.Edges, unstructuredGrid.Cells.Reverse().ToList());
            var links = Generate1D2DLinksHelper.Generate1D2DLinks(area, linkGeneratingType, unstructuredGrid, gullies, discretization).ToList();

            // Assert
            Assert.AreEqual(expectedNumberOfLinks, links.Count);
            Assert.That(links[0].FaceIndex, Is.EqualTo(firstDiscretizationPointConnectsToCellIndex));
        }

        private static IPolygon GetFullArea()
        {
            var areaCoordinates = new List<Coordinate>();
            areaCoordinates.Add(new Coordinate(-20, -20));
            areaCoordinates.Add(new Coordinate(-20, 40));
            areaCoordinates.Add(new Coordinate(120, 40));
            areaCoordinates.Add(new Coordinate(120, -20));
            areaCoordinates.Add(new Coordinate(-20, -20));

            return new Polygon(new LinearRing(areaCoordinates.ToArray()));
        }
        
        /// <summary>
        /// Discretization with a 1d computational node on every branch start and end node
        /// (DWA, HWA, branch1 -> start, end)
        /// 
        ///            Channel1 [2]
        ///   N1 -------------------- N2
        ///  0,20                   100,20
        ///
        ///             HWA [1]
        ///   M3 -------------------- M4
        ///  0,10                   100,10
        ///
        ///             DWA [0]
        ///   M1 -------------------- M2 
        ///  0,0                    100,0
        ///
        /// 
        /// </summary> 
        private static Discretization GetTestDiscretization()
        {
            var network = new HydroNetwork();

            var manhole1 = new Manhole("manhole1")
            {
                Compartments = new EventedList<ICompartment> { new Compartment() },
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

            var node1 = new HydroNode() { Name = "node1", Geometry = new Point(0, 20) };
            var node2 = new HydroNode() { Name = "node2", Geometry = new Point(100, 20) };
            var branch1 = new Channel(node1, node2)
            {
                Name = "Channel1",
                Geometry =
                    new LineString(new[]
                    {new Coordinate(node1.Geometry.Coordinate.X, node1.Geometry.Coordinate.Y), new Coordinate(node2.Geometry.Coordinate.X, node2.Geometry.Coordinate.Y)})
            };
            network.Nodes.Add(node1);
            network.Nodes.Add(node2);
            network.Branches.Add(branch1);

            var discretisation = new Discretization() { Network = network };

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

        /// <summary>
        /// Discretization with a 1d computational node on channel start, mid and end 
        /// (branch1 -> start, mid, end)
        /// 
        ///            Channel1 [2]
        ///   N1 ---------- N2 ---------- N3
        ///  0,20          50,20         100,20
        ///
        ///
        /// 
        /// </summary> 
        private static Discretization GetTestChannelDiscretization()
        {
            var network = new HydroNetwork();
            
            var node1 = new HydroNode() { Name = "node1", Geometry = new Point(0, 20) };
            var node2 = new HydroNode() { Name = "node2", Geometry = new Point(100, 20) };
            var branch1 = new Channel(node1, node2)
            {
                Name = "Channel1",
                Geometry =
                    new LineString(new[]
                    {new Coordinate(node1.Geometry.Coordinate.X, node1.Geometry.Coordinate.Y), new Coordinate(node2.Geometry.Coordinate.X, node2.Geometry.Coordinate.Y)})
            };
            network.Nodes.Add(node1);
            network.Nodes.Add(node2);
            network.Branches.Add(branch1);

            var discretisation = new Discretization() { Network = network };

            
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
                Chainage = 50,
                Geometry = new Point(50, 20),
                Name = "branch1_mid"
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