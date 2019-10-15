using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.Helpers;
using DeltaShell.NGHS.IO.Grid;
using DeltaShell.Plugins.NetworkEditor.Tests.Helpers;
using GeoAPI.Extensions.Coverages;
using GeoAPI.Geometries;
using NetTopologySuite.Extensions.Coverages;
using NetTopologySuite.Extensions.Networks;
using NetTopologySuite.Geometries;
using NUnit.Framework;

namespace DeltaShell.Plugins.NetworkEditor.Tests
{
    [TestFixture]
    public class NetworkDiscretisationUGridDataModelTest
    {
        private IDiscretization TestNetworkAndDiscretisation()
        {
            var network = new HydroNetwork() { Name = "my Network" };
            var hydroNode1 = new HydroNode()
            {
                Name = "my Node1",
                Description = "Node 1 Description",
                Geometry = new Point(0, 0),
                Network = network
            };
            network.Nodes.Add(hydroNode1);
            var hydroNode2 = new HydroNode()
            {
                Name = "my Node2",
                Description = "Node 2 Description",
                Geometry = new Point(3, 4),
                Network = network
            };
            network.Nodes.Add(hydroNode2);
            var branch1 = new Branch()
            {
                Name = "my Branch 1",
                Description = "Branch 1 Description",
                Network = network,
                Source = hydroNode1,
                Target = hydroNode2,
                Geometry = new LineString(new[]
                {
                    new Coordinate(0, 0),
                    new Coordinate(3, 4)
                })
            };
            network.Branches.Add(branch1);

            var networkDiscretisation = new Discretization
            {
                Name = "my Discretisation",
                Network = network
            };

            // add source node
            networkDiscretisation.Locations.Values.Add(new NetworkLocation(branch1, 0)
            {
                Name = "point_01",
                LongName = "point_01_description"
            });
            // add calculation points
            var location1 = new NetworkLocation(branch1, 1)
            {
                LongName = "branch_01_description"
            };
            networkDiscretisation.Locations.Values.Add(location1);
            var location2 = new NetworkLocation(branch1, 2)
            {
                Name = "point_03",
                LongName = "point_03_description"
            };
            networkDiscretisation.Locations.Values.Add(location2);
            var location3 = new NetworkLocation(branch1, 3)
            {
                Name = "point_04",
                LongName = "point_04_description"
            };
            networkDiscretisation.Locations.Values.Add(location3);
            // add target node
            networkDiscretisation.Locations.Values.Add(new NetworkLocation(branch1, 5)
            {
                Name = "point_05",
                LongName = "point_05_description"
            });

            return networkDiscretisation;
        }

        private IDiscretization TestNetworkAndDiscretisation_TModel()
        {
            var network = new HydroNetwork() { Name = "my Network" };
            
            //4 nodes
            var hydroNode1 = new HydroNode()
            {
                Name = "my Node1",
                Description = "Node 1 Description",
                Geometry = new Point(0, 0),
                Network = network
            };
            network.Nodes.Add(hydroNode1);
            var hydroNode2 = new HydroNode()
            {
                Name = "my Node2",
                Description = "Node 2 Description",
                Geometry = new Point(12, 0),
                Network = network
            };
            network.Nodes.Add(hydroNode2);

            var hydroNode3 = new HydroNode()
            {
                Name = "my Node3",
                Description = "Node 3 Description",
                Geometry = new Point(12, 10),
                Network = network
            };
            network.Nodes.Add(hydroNode3);

            var hydroNode4 = new HydroNode()
            {
                Name = "my Node4",
                Description = "Node 4 Description",
                Geometry = new Point(12, -10),
                Network = network
            };
            network.Nodes.Add(hydroNode4);
            
            // 3 branches
            var branch1 = new Channel()
            {
                Name = "my Branch 1",
                Description = "Branch 1 Description",
                Network = network,
                Source = hydroNode1,
                Target = hydroNode2,
                Geometry = new LineString(new[]
                {
                    new Coordinate(0, 0),
                    new Coordinate(12, 0)
                })
            };
            network.Branches.Add(branch1);

            var branch2 = new Channel()
            {
                Name = "my Branch 2",
                Description = "Branch 2 Description",
                Network = network,
                Source = hydroNode2,
                Target = hydroNode3,
                Geometry = new LineString(new[]
                {
                    new Coordinate(12, 0),
                    new Coordinate(12, 10)
                })
            };
            network.Branches.Add(branch2);

            var branch3 = new Channel()
            {
                Name = "my Branch 3",
                Description = "Branch 3 Description",
                Network = network,
                Source = hydroNode3,
                Target = hydroNode4,
                Geometry = new LineString(new[]
                {
                    new Coordinate(12, -10),
                    new Coordinate(12, 0)
                })
            };
            network.Branches.Add(branch3);
            
            //zucht en nu iets met disctisatie
            var networkDiscretisation = new Discretization
            {
                Name = "my Discretisation",
                Network = network
            };
            /*
            // add source node
            networkDiscretisation.Locations.Values.Add(new NetworkLocation(branch1, 0)
            {
                Name = "point_01",
                LongName = "point_01_description"
            });
            // add calculation points
            var location1 = new NetworkLocation(branch1, 3)
            {
                LongName = "branch_01_description"
            };
            networkDiscretisation.Locations.Values.Add(location1);

            var location2 = new NetworkLocation(branch1, 6)
            {
                Name = "point_03",
                LongName = "point_03_description"
            };
            networkDiscretisation.Locations.Values.Add(location2);

            var location3 = new NetworkLocation(branch1, 8)
            {
                Name = "point_04",
                LongName = "point_04_description"
            };

            networkDiscretisation.Locations.Values.Add(location3);
            
            // add target node
            networkDiscretisation.Locations.Values.Add(new NetworkLocation(branch2, 0)
            {
                Name = "point_05",
                LongName = "point_05_description"
            });

            networkDiscretisation.Locations.Values.Add(new NetworkLocation(branch2, 4)
            {
                Name = "point_06",
                LongName = "point_06_description"
            });

            networkDiscretisation.Locations.Values.Add(new NetworkLocation(branch2, 8)
            {
                Name = "point_06",
                LongName = "point_06_description"
            });

            networkDiscretisation.Locations.Values.Add(new NetworkLocation(branch2, 10)
            {
                Name = "point_07",
                LongName = "point_07_description"
            });

            networkDiscretisation.Locations.Values.Add(new NetworkLocation(branch3, 4)
            {
                Name = "point_08",
                LongName = "point_08_description"
            });

            networkDiscretisation.Locations.Values.Add(new NetworkLocation(branch3, 7)
            {
                Name = "point_09",
                LongName = "point_09_description"
            });

            networkDiscretisation.Locations.Values.Add(new NetworkLocation(branch3, 10)
            {
                Name = "point_10",
                LongName = "point_10_description"
            });
            */
            HydroNetworkHelper.GenerateDiscretization(networkDiscretisation, true, false, 0.5, false, 1.0, false, false, true, 2, null);
            return networkDiscretisation;
        }

        [Test]
        public void ConstructNetworkDiscretisationDataTModelTest()
        {
            var discretisation = TestNetworkAndDiscretisation_TModel();

            var dataModel = new NetworkDiscretisationUGridDataModel(discretisation);
        }

        [Test]
        public void ConstructNetworkDiscretisationDataModelTest()
        {
            var discretisation = TestNetworkAndDiscretisation();
            
            var dataModel = new NetworkDiscretisationUGridDataModel(discretisation);

            // Test the data
            Assert.AreEqual("my Discretisation", dataModel.Name);
            Assert.AreEqual(5, dataModel.NumberOfDiscretisationPoints);
            Assert.AreEqual(4, dataModel.NumberOfMeshEdges);
            Assert.AreEqual(new[] {0, 0, 0, 0, 0}, dataModel.BranchIdx);
            Assert.AreEqual(new[] {0, 1, 2, 3, 5}, dataModel.Offsets);
            Assert.AreEqual(new[] { "point_01", "my Branch 1_1.000", "point_03", "point_04", "point_05" }, dataModel.DiscretisationPointIds);
            Assert.AreEqual(new[] { "point_01_description", "branch_01_description", "point_03_description", "point_04_description", "point_05_description" }, dataModel.DiscretisationPointDescriptions);
        }

        [Test]
        public void ReconstructNetworkDiscretisationTest()
        {
            var discretisation = TestNetworkAndDiscretisation();
            var network = (IHydroNetwork)discretisation.Network;

            var dataModel = new NetworkDiscretisationUGridDataModel(discretisation);

            var reconstructedDiscretisation = NetworkDiscretisationFactory.CreateNetworkDiscretisation(network, dataModel);

            Assert.AreEqual(discretisation.Name, reconstructedDiscretisation.Name);

            HydroNetworkTestHelper.CompareNetworks(discretisation.Network, reconstructedDiscretisation.Network);
            HydroNetworkTestHelper.CompareDiscretisations(discretisation, reconstructedDiscretisation);
        }
    }
}
