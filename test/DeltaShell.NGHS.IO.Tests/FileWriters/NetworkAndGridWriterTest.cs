using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.Helpers;
using DeltaShell.NGHS.IO.FileWriters.General;
using DeltaShell.NGHS.IO.FileWriters.Network;
using DeltaShell.NGHS.IO.TestUtils;
using GeoAPI.Extensions.Coverages;
using NetTopologySuite.Extensions.Coverages;
using NetTopologySuite.Geometries;
using NUnit.Framework;

namespace DeltaShell.NGHS.IO.Tests.FileWriters
{
    [TestFixture]
    public class NetworkAndGridWriterTest
    {
        private IHydroNetwork network;
        private IDiscretization discretization;

        private const double NODE1_X = 0.0;
        private const double NODE1_Y = 0.0;
        private const double NODE2_X = 50.0;
        private const double NODE2_Y = 50.0;
        private const double NODE3_X = 100.0;
        private const double NODE3_Y = 100.0;

        private readonly double[] gridPointsOffsets = new double[9] { 5.0, 10.0, 15.0, 20.0, 25.0, 30.0, 35.0, 40.0, 45.0 };
        private readonly double[] gridPointsX = new double[9] { 5.0, 10.0, 15.0, 20.0, 25.0, 30.0, 35.0, 40.0, 45.0 };
        private readonly double[] gridPointsY = new double[9] { 5.0, 10.0, 15.0, 20.0, 25.0, 30.0, 35.0, 40.0, 45.0 };
        private readonly string[] gridPointsNames = new string[9];

        [SetUp]
        public void SetUp()
        {
            network = HydroNetworkHelper.GetSnakeHydroNetwork(2, true);
            var branch1 = network.Channels.First();
            
            network.Nodes[0].Geometry = new Point(NODE1_X, NODE1_Y);
            network.Nodes[1].Geometry = new Point(NODE2_X, NODE2_Y);
            network.Nodes[2].Geometry = new Point(NODE3_X, NODE3_Y);

            for (var i = 0; i < gridPointsNames.Length; i++)
            {
                gridPointsNames[i] = branch1.Name + "_" + gridPointsOffsets[i].ToString(NetworkDefinitionRegion.GridPointOffsets.Format, CultureInfo.InvariantCulture);
            }

            IList<INetworkLocation> locations = new List<INetworkLocation>();
            for (var i = 0; i < gridPointsNames.Length; i++)
            {
                locations.Add(new NetworkLocation()
                {
                    Branch = branch1,
                    Chainage = gridPointsOffsets[i],
                    Geometry = new Point(gridPointsX[i], gridPointsY[i]),
                    Name = gridPointsNames[i]
                });
            }

            discretization = new Discretization();
            discretization.Locations.Values.AddRange(locations.ToList());
        }

        [TearDown]
        public void TearDown(){}

        [Test]
        public void TestNetworkAndGridWriter()
        {
            NetworkAndGridWriter.WriteFile(FileWriterTestHelper.ModelFileNames.Network, network, discretization);
            var categories = new DelftIniReader().ReadDelftIniFile(FileWriterTestHelper.ModelFileNames.Network);
            Assert.AreEqual(1, categories.Count(g => g.Name == GeneralRegion.IniHeader));           
            Assert.AreEqual(3, categories.Count(c => c.Name == NetworkDefinitionRegion.IniNodeHeader));
            Assert.AreEqual(2, categories.Count(c => c.Name == NetworkDefinitionRegion.IniBranchHeader));

            // Check nodes
            var nodeCategories = categories.Where(c => c.Name == NetworkDefinitionRegion.IniNodeHeader).ToList();
            Assert.AreEqual(3, nodeCategories[0].Properties.Count);
            
            var property = nodeCategories[0].Properties.First(p => p.Name == NetworkDefinitionRegion.Id.Key);
            Assert.AreEqual(network.Nodes[0].Name, property.Value);
            
            property = nodeCategories[0].Properties.First(p => p.Name == NetworkDefinitionRegion.X.Key);
            Assert.AreEqual(NODE1_X.ToString(NetworkDefinitionRegion.X.Format, CultureInfo.InvariantCulture), property.Value);
            
            property = nodeCategories[0].Properties.First(p => p.Name == NetworkDefinitionRegion.Y.Key);
            Assert.AreEqual(NODE1_Y.ToString(NetworkDefinitionRegion.Y.Format, CultureInfo.InvariantCulture), property.Value);

            property = nodeCategories[1].Properties.First(p => p.Name == NetworkDefinitionRegion.Id.Key);
            Assert.AreEqual(network.Nodes[1].Name, property.Value);

            property = nodeCategories[1].Properties.First(p => p.Name == NetworkDefinitionRegion.X.Key);
            Assert.AreEqual(NODE2_X.ToString(NetworkDefinitionRegion.X.Format, CultureInfo.InvariantCulture), property.Value);

            property = nodeCategories[1].Properties.First(p => p.Name == NetworkDefinitionRegion.Y.Key);
            Assert.AreEqual(NODE2_Y.ToString(NetworkDefinitionRegion.Y.Format, CultureInfo.InvariantCulture), property.Value);

            property = nodeCategories[2].Properties.First(p => p.Name == NetworkDefinitionRegion.Id.Key);
            Assert.AreEqual(network.Nodes[2].Name, property.Value);

            property = nodeCategories[2].Properties.First(p => p.Name == NetworkDefinitionRegion.X.Key);
            Assert.AreEqual(NODE3_X.ToString(NetworkDefinitionRegion.X.Format, CultureInfo.InvariantCulture), property.Value);

            property = nodeCategories[2].Properties.First(p => p.Name == NetworkDefinitionRegion.Y.Key);
            Assert.AreEqual(NODE3_Y.ToString(NetworkDefinitionRegion.Y.Format, CultureInfo.InvariantCulture), property.Value);

            // Check branches
            var branchCategories = categories.Where(c => c.Name == NetworkDefinitionRegion.IniBranchHeader).ToList();
            Assert.AreEqual(10, branchCategories[0].Properties.Count);

            property = branchCategories[0].Properties.First(p => p.Name == NetworkDefinitionRegion.Id.Key);
            Assert.AreEqual(network.Branches[0].Name, property.Value);

            property = branchCategories[0].Properties.First(p => p.Name == NetworkDefinitionRegion.FromNode.Key);
            Assert.AreEqual(network.Nodes[0].Name, property.Value);

            property = branchCategories[0].Properties.First(p => p.Name == NetworkDefinitionRegion.ToNode.Key);
            Assert.AreEqual(network.Nodes[1].Name, property.Value);

            property = branchCategories[0].Properties.First(p => p.Name == NetworkDefinitionRegion.BranchOrder.Key);
            Assert.AreEqual(network.Branches[0].OrderNumber.ToString(), property.Value);

            property = branchCategories[0].Properties.First(p => p.Name == NetworkDefinitionRegion.Geometry.Key);
            Assert.AreEqual(network.Branches[0].Geometry.ToString(), property.Value);

            property = branchCategories[0].Properties.First(p => p.Name == NetworkDefinitionRegion.GridPointsCount.Key);
            Assert.AreEqual(discretization.Locations.Values.Count.ToString(), property.Value);
            
            property = branchCategories[0].Properties.First(p => p.Name == NetworkDefinitionRegion.GridPointX.Key);
            var gridPointXString = string.Join(" ", gridPointsX.ToList().Select(
                av => av.ToString(NetworkDefinitionRegion.GridPointX.Format, CultureInfo.InvariantCulture)));
            Assert.AreEqual(gridPointXString, property.Value);

            property = branchCategories[0].Properties.First(p => p.Name == NetworkDefinitionRegion.GridPointY.Key);
            var gridPointYString = string.Join(" ", gridPointsY.ToList().Select(
                av => av.ToString(NetworkDefinitionRegion.GridPointY.Format, CultureInfo.InvariantCulture)));
            Assert.AreEqual(gridPointYString, property.Value);

            property = branchCategories[0].Properties.First(p => p.Name == NetworkDefinitionRegion.GridPointOffsets.Key);
            var gridPointOffsetsString = string.Join(" ", gridPointsOffsets.ToList().Select(
                av => av.ToString(NetworkDefinitionRegion.GridPointOffsets.Format, CultureInfo.InvariantCulture)));
            Assert.AreEqual(gridPointOffsetsString, property.Value);

            property = branchCategories[0].Properties.First(p => p.Name == NetworkDefinitionRegion.GridPointNames.Key);
            var gridPointNamesString = string.Join(";", gridPointsNames.ToList());
            Assert.AreEqual(gridPointNamesString, property.Value);
          
            Assert.AreEqual(6, branchCategories[1].Properties.Count);

            property = branchCategories[1].Properties.First(p => p.Name == NetworkDefinitionRegion.Id.Key);
            Assert.AreEqual(network.Branches[1].Name, property.Value);

            property = branchCategories[1].Properties.First(p => p.Name == NetworkDefinitionRegion.FromNode.Key);
            Assert.AreEqual(network.Nodes[1].Name, property.Value);

            property = branchCategories[1].Properties.First(p => p.Name == NetworkDefinitionRegion.ToNode.Key);
            Assert.AreEqual(network.Nodes[2].Name, property.Value);

            property = branchCategories[1].Properties.First(p => p.Name == NetworkDefinitionRegion.BranchOrder.Key);
            Assert.AreEqual(network.Branches[1].OrderNumber.ToString(), property.Value);

            property = branchCategories[1].Properties.First(p => p.Name == NetworkDefinitionRegion.Geometry.Key);
            Assert.AreEqual(network.Branches[1].Geometry.ToString(), property.Value);

            property = branchCategories[1].Properties.First(p => p.Name == NetworkDefinitionRegion.GridPointsCount.Key);
            Assert.AreEqual("0", property.Value);
        }
    }
}
