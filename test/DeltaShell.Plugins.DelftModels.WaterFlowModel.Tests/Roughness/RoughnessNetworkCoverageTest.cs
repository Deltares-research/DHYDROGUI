using System.Linq;
using System.Windows.Forms.VisualStyles;
using DelftTools.Hydro;
using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.Helpers;
using DelftTools.Hydro.Roughness;
using NetTopologySuite.IO;
using NetTopologySuite.Extensions.Coverages;
using NetTopologySuite.Extensions.Networks;
using NUnit.Framework;
using Point = NetTopologySuite.Geometries.Point;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.Tests.Roughness
{
    [TestFixture]
    public class RoughnessNetworkCoverageTest
    {
        [Test]
        public void RoughnessNetworkCoverageTestInterpolation()
        {
            var network = CreateHydroNetwork();
            var branch = network.Branches[0];
            
            // create and initialize network coverage
            var roughnessCoverage = new RoughnessNetworkCoverage("",false,  null) { Network = network };

            roughnessCoverage[new NetworkLocation(branch, 0)] = new object[] {100.0, RoughnessType.StricklerKn};
            roughnessCoverage[new NetworkLocation(branch, 1.0)] = new object[] {200.0, RoughnessType.StricklerKn};

            // value will be interpolated
            var rougnhess = roughnessCoverage.EvaluateRoughnessValue((new NetworkLocation(branch, 0.5)));
            Assert.AreEqual(150.0, rougnhess);

            var rougnhessType = roughnessCoverage.EvaluateRoughnessType((new NetworkLocation(branch, 0.5)));
            Assert.AreEqual(RoughnessType.StricklerKn, rougnhessType);
        }

        [Test]
        public void CloneRougnessNetworkCoverage()
        {
            var network = new HydroNetwork {Name = "W00T"};

            var fromNode = new Node("Frm") { Geometry = new WKTReader().Read("POINT(20 20)") };
            var toNode = new Node("To") { Geometry = new WKTReader().Read("POINT(40 20)") };
            network.Nodes.Add(fromNode);
            network.Nodes.Add(toNode);

            var branch = new Branch
                             {
                                 Source = fromNode,
                                 Target = toNode,
                                 Length = 1.0,
                                 Geometry = new WKTReader().Read("LINESTRING(20 20,40 20)")
                             };

            network.Branches.Add(branch);
            network.CrossSectionSectionTypes.Add(new CrossSectionSectionType {Name = "main"});

            var roughnessCoverage = new RoughnessNetworkCoverage("main", false, null) { Network = network };

            roughnessCoverage[new NetworkLocation(branch, 0)] = new object[] { 100.0, RoughnessType.Chezy };
            roughnessCoverage[new NetworkLocation(branch, 1.0)] = new object[] { 200.0, RoughnessType.Chezy };
            
            var roughnessCoverageClone = roughnessCoverage.Clone();
            Assert.AreEqual(roughnessCoverage.Name, ((RoughnessNetworkCoverage)roughnessCoverageClone).Name);
            Assert.AreEqual(roughnessCoverage.Network.Name, ((RoughnessNetworkCoverage)roughnessCoverageClone).Network.Name);
            Assert.AreEqual(roughnessCoverage.Network.Nodes.FirstOrDefault(), ((RoughnessNetworkCoverage)roughnessCoverageClone).Network.Nodes.FirstOrDefault());
            Assert.AreEqual(roughnessCoverage.Network.Branches.FirstOrDefault(), ((RoughnessNetworkCoverage)roughnessCoverageClone).Network.Branches.FirstOrDefault());

            Assert.AreEqual(roughnessCoverage[new NetworkLocation(branch, 0)], ((RoughnessNetworkCoverage)roughnessCoverageClone)[new NetworkLocation(branch, 0)]);
        }

        [Test]
        public void SplitBranchRoughnessNetworkCoverage()
        {
            var network = HydroNetworkHelper.GetSnakeHydroNetwork(new Point(0,0),new Point(100,0));
            var roughnessCoverage = new RoughnessNetworkCoverage("", false, null) { Network = network };
            var branch = network.Branches.First();

            roughnessCoverage[new NetworkLocation(branch, 0)] = new object[] { 0.0, RoughnessType.StricklerKn };
            roughnessCoverage[new NetworkLocation(branch, 100.0)] = new object[] { 100.0, RoughnessType.StricklerKn };

            //split the branch halfway
            var result = NetworkHelper.SplitBranchAtNode(branch, 50.0d);

            var secondBranch = result.NewBranch;

            //check a point is added at the start of the new 
            var startLocation = new NetworkLocation(secondBranch,0);
            Assert.AreEqual( 50.0, roughnessCoverage.RoughnessValueComponent[startLocation]);
            Assert.AreEqual(RoughnessType.StricklerKn, (RoughnessType)roughnessCoverage.RoughnessTypeComponent[startLocation]);

            //and at the end of the 'old' branch
            var endLocation= new NetworkLocation(branch, 50.0d);
            Assert.AreEqual(50.0, roughnessCoverage.RoughnessValueComponent[endLocation]);
            Assert.AreEqual(RoughnessType.StricklerKn, (RoughnessType)roughnessCoverage.RoughnessTypeComponent[endLocation]);
        }

        [Test]
        public void AddLocationExistingBranchUsesExistingValues()
        {
            var network = HydroNetworkHelper.GetSnakeHydroNetwork(new Point(0, 0), new Point(100, 0));
            var roughnessCoverage = new RoughnessNetworkCoverage("", false, null) { Network = network };
            var branch = network.Branches.First();

            roughnessCoverage[new NetworkLocation(branch, 0)] = new object[] { 22.0, RoughnessType.StricklerKn };

            var value = roughnessCoverage.EvaluateRoughnessValue(new NetworkLocation(branch, 10));
            //action!
            NetworkLocation newLocation = new NetworkLocation(branch, 10);
            roughnessCoverage.Locations.Values.Add(newLocation);

            //check the same values are applied
            Assert.AreEqual(22.0d, roughnessCoverage.EvaluateRoughnessValue(newLocation));
            Assert.AreEqual(RoughnessType.StricklerKn, roughnessCoverage.EvaluateRoughnessType(newLocation));
        }

       

        [Test]
        public void SplitBranchRoughnessNetworkCoverageDoesNotAddPointForEmptyBranch()
        {
            //if the coverage has no location on the split branch do nothing
            var network = HydroNetworkHelper.GetSnakeHydroNetwork(new Point(0, 0), new Point(100, 0));
            var roughnessCoverage = new RoughnessNetworkCoverage("", false, null) { Network = network };
            var branch = network.Branches.First();

            NetworkHelper.SplitBranchAtNode(branch, 50.0d);

            Assert.AreEqual(0,roughnessCoverage.Locations.Values.Count);
        }

        [Test]
        public void MergeBranchesWithRoughnessNetworkCoverage()
        {
            var network = HydroNetworkHelper.GetSnakeHydroNetwork(new Point(0, 0), new Point(0, 100),
                                                                  new Point(100, 100));
            var roughnessCoverage = new RoughnessNetworkCoverage("", false, null) { Network = network };
            
            //add locations on both branches
            var networkLocation = new NetworkLocation(network.Branches[0], 50);
            var networkLocation2 = new NetworkLocation(network.Branches[1], 50);
            roughnessCoverage[networkLocation] = new object[] { 22.0, RoughnessType.StricklerKn };
            roughnessCoverage[networkLocation2] = new object[] { 33.0, RoughnessType.Manning };

            //merge the branches/remove the node
            NetworkHelper.MergeNodeBranches(network.Nodes[1], network);

            //check the values were removed
            Assert.AreEqual(0, roughnessCoverage.RoughnessValueComponent.Values.Count);
            Assert.AreEqual(0, roughnessCoverage.RoughnessTypeComponent.Values.Count);
            
            
        }

        [Test]
        public void SplitAndMergeShouldWork()
        {
            var network = HydroNetworkHelper.GetSnakeHydroNetwork(new Point(0, 0), new Point(100, 0));
            var roughnessCoverage = new RoughnessNetworkCoverage("", false, null) { Network = network };
            var branch = network.Branches.First();

            roughnessCoverage[new NetworkLocation(branch,10)] = new object[] { 22.0, RoughnessType.StricklerKn };

            var result = NetworkHelper.SplitBranchAtNode(branch, 50.0d);
            NetworkHelper.MergeNodeBranches(result.NewNode,network);

            //check the locations are unique
            Assert.AreEqual(roughnessCoverage.Locations.Values.Count,roughnessCoverage.Locations.Values.Distinct().Count());
        }

        private static IHydroNetwork CreateHydroNetwork()
        {
            var network = new HydroNetwork();
            
            var fromNode = new Node();
            var toNode = new Node();
            network.Nodes.Add(fromNode);
            network.Nodes.Add(toNode);

            network.Branches.Add(new Branch { Source = fromNode, Target = toNode, Length = 1.0 });
            network.CrossSectionSectionTypes.Add(new CrossSectionSectionType {Name = "main"});
            return network;
        }
    }
}
