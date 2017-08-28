using DelftTools.Hydro;
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
            Assert.AreEqual(new[] {0, 1, 2, 3, 5}, dataModel.Offset);
            Assert.AreEqual(new[] { "point_01", "my Branch 1_1.000", "point_03", "point_04", "point_05" }, dataModel.DiscretisationPointIds);
            Assert.AreEqual(new[] { "point_01_description", "branch_01_description", "point_03_description", "point_04_description", "point_05_description" }, dataModel.DiscretisationPointDescriptions);
        }

        [Test]
        public void ReconstructNetworkDiscretisationTest()
        {
            var discretisation = TestNetworkAndDiscretisation();
            var network = (IHydroNetwork)discretisation.Network;

            var dataModel = new NetworkDiscretisationUGridDataModel(discretisation);

            var reconstructedDiscretisation = NetworkDiscretisationUGridDataModel.ReconstructNetworkDiscretisation(network, dataModel.Name, dataModel.BranchIdx, dataModel.Offset, dataModel.DiscretisationPointIds, dataModel.DiscretisationPointDescriptions);

            Assert.AreEqual(discretisation.Name, reconstructedDiscretisation.Name);

            HydroNetworkTestHelper.CompareAndAssertNetworks(discretisation.Network, reconstructedDiscretisation.Network);
            HydroNetworkTestHelper.CompareAndAssertDiscretisations(discretisation, reconstructedDiscretisation);
        }
    }
}
