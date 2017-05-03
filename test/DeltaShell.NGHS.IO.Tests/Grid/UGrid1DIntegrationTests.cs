using System.Linq;
using DelftTools.Hydro;
using DelftTools.TestUtils;
using DelftTools.Utils.IO;
using DeltaShell.NGHS.IO.Grid;
using GeoAPI.Extensions.Networks;
using GeoAPI.Geometries;
using NetTopologySuite.Extensions.Networks;
using NetTopologySuite.Geometries;
using NUnit.Framework;

namespace DeltaShell.NGHS.IO.Tests.Grid
{
    [TestFixture]
    public class UGrid1DIntegrationTests
    {
        private const string UGRID_TEST_FILE = @"ugrid\Custom_Ugrid.nc";

        [Test]
        [Category(TestCategory.DataAccess)]
        public void Test1()
        {
            var network = new HydroNetwork() {Name="myNetwork"};
            var hydroNode1 = new HydroNode() {Name = "myNode1", Geometry = new Point(1, 4), Network = network };
            network.Nodes.Add(hydroNode1);
            var hydroNode2 = new HydroNode() {Name="myNode2", Geometry = new Point(5, 1), Network = network };
            network.Nodes.Add(hydroNode2);
            var branch1 = new Branch() {Name = "myBranch1", Network = network, Source = hydroNode1, Target = hydroNode2, Geometry = new LineString(new[] { new Coordinate(1, 4), new Coordinate(6, 12), new Coordinate(5, 1) }) };
            network.Branches.Add(branch1);
            var testFilePath =
                TestHelper.GetTestFilePath(UGRID_TEST_FILE);

            var localCopyOfTestFile = TestHelper.CreateLocalCopy(testFilePath);
            try
            {
                using (var ugrid1D = new UGrid1D(localCopyOfTestFile))
                {
                    var totalNumberOfGeometryPoints = network.Branches.Sum(b => b.Geometry.Coordinates.Length);
                    
                    //var numberOfNetworkNodes = ugrid1D.GetNumberOfNetworkNodes();
                    //Assert.That(numberOfNetworkNodes == 0);

                    //var numberOfNetworkBranches = ugrid1D.GetNumberOfNetworkBranches();
                    //Assert.That(numberOfNetworkBranches == 0);

                    ugrid1D.Create1DGridInFile(
                        network.Name, 
                        network.Nodes.Count, 
                        network.Branches.Count,
                        totalNumberOfGeometryPoints);

                    ugrid1D.Write1DNetworkNodes(
                        network.Nodes.Select(n => n.Geometry.Coordinates[0].X).ToArray(),
                        network.Nodes.Select(n => n.Geometry.Coordinates[0].Y).ToArray(),
                        network.Nodes.Select(n => n.Name).ToArray(), network.Nodes.Select(n => n.Description).ToArray());

                    var numberOfNetworkNodes = ugrid1D.GetNumberOfNetworkNodes();
                    Assert.That(numberOfNetworkNodes == 2);
                    
                    ugrid1D.Write1DNetworkBranches(
                        network.Branches.Select(b => b.Source).ToArray().Select(n => network.Nodes.IndexOf(n)).ToArray(),
                        network.Branches.Select(b => b.Target).ToArray().Select(n => network.Nodes.IndexOf(n)).ToArray(),
                        network.Branches.Select(b => b.Length).ToArray(),
                        network.Branches.Select(b =>
                        {
                            if (b.Geometry != null && b.Geometry.Coordinates != null)
                                return b.Geometry.Coordinates.Length;
                            return 0;
                        }).ToArray(),
                        network.Branches.Select(b => b.Name).ToArray(),
                        network.Branches.Select(b => b.Description).ToArray()
                        
                    );

                    var numberOfNetworkBranches = ugrid1D.GetNumberOfNetworkBranches();
                    Assert.That(numberOfNetworkBranches == 1);

                    ugrid1D.Write1DNetworkGeometry(
                        network.Branches.SelectMany(b => b.Geometry.Coordinates.Select(c => c.X).ToArray()).ToArray(),
                        network.Branches.SelectMany(b => b.Geometry.Coordinates.Select(c => c.Y).ToArray()).ToArray());
                    var numberOfNetWorkGeometryPoints = ugrid1D.GetNumberOfNetworkGeometryPoints();
                    Assert.That(numberOfNetWorkGeometryPoints == 3);
                }

            }
            finally
            {
                FileUtils.DeleteIfExists(localCopyOfTestFile);
            }
            
        }


    }
}