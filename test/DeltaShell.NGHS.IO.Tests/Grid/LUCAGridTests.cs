using System.Collections.Generic;
using System.IO;
using DelftTools.Hydro;
using DelftTools.Hydro.Helpers;
using DelftTools.Hydro.Link1d2d;
using DelftTools.Shell.Core.Workflow;
using DelftTools.TestUtils;
using DelftTools.Utils.IO;
using DeltaShell.Core;
using DeltaShell.Dimr;
using DeltaShell.NGHS.IO.FileReaders.Network;
using DeltaShell.NGHS.IO.Grid;
using DeltaShell.Plugins.FMSuite.FlowFM;
using GeoAPI.Extensions.Coverages;
using GeoAPI.Geometries;
using NetTopologySuite.Extensions.Coverages;
using NetTopologySuite.Geometries;
using NUnit.Framework;
using GeometryFactory = SharpMap.Converters.Geometries.GeometryFactory;

namespace DeltaShell.NGHS.IO.Tests.Grid
{
    [TestFixture]
    public class LUCAGridTests
    {
        [Test]
        public void ReadWritegrid()
        {
            DimrApiDataSet.SetSharedPath();
            var testFilePath = TestHelper.GetTestFilePath(@"luca\Custom_Ugrid.nc");
            var localCopyOfTestFile = TestHelper.CreateLocalCopy(testFilePath);
            var grid = UnstructuredGridFileHelper.LoadFromFile(localCopyOfTestFile);
            
            UnstructuredGridFileHelper.WriteGridToFile(@"c:\test.nc", grid, null, null, null, "Luca", "LucaPlugin", "1.0",UnstructuredGridFileHelper.BedLevelLocation.NodesMaxLev, new double[1] {80.1});


        }
        [Test]
        public void ReadWritenetworkanddiscrandgridandlinks()
        {
            DimrApiDataSet.SetSharedPath();
            var testFilePath = TestHelper.GetTestFilePath(@"luca\Custom_Ugrid.nc");
            var localCopyOfTestFile = TestHelper.CreateLocalCopy(testFilePath);
            var grid = UnstructuredGridFileHelper.LoadFromFile(localCopyOfTestFile);
            var pathRead = TestHelper.GetTestFilePath(@"luca\NetworkDefinition.ini");
            var network = new HydroNetwork();
            var discretization = new Discretization { Network = network };
            NetworkAndGridReader.ReadFile(pathRead, network, discretization);

            var links = new List<ILink1D2D>()
            {
                new Link1D2D(5, 6),
                new Link1D2D(6, 7)
            };


//            UnstructuredGridFileHelper.WriteGridToFile(@"c:\test.nc", grid, network, discretization, links, "Luca", "LucaPlugin", "1.0", UnstructuredGridFileHelper.BedLevelLocation.NodesMaxLev, new double[1] { 80.1 });
            UnstructuredGridFileHelper.WriteGridToFile(@"c:\test.nc", grid, network, discretization, links, "Luca", "LucaPlugin", "1.0", UnstructuredGridFileHelper.BedLevelLocation.NodesMaxLev, new double[5] { 80.1,80.2,80.3,80.4, 80.5 });


        }
        [Test]
        public void Writenetworkanddiscr2()
        {
            var network = new HydroNetwork();

            // add nodes and branches
            var node1 = new HydroNode { Name = "node1", Network = network, Geometry = new Point(0, 0)};
            var node2 = new HydroNode { Name = "node2", Network = network, Geometry = new Point(100, 0) };

            network.Nodes.Add(node1);
            network.Nodes.Add(node2);

            var branch1 = new Channel("branch1", node1, node2);
            var vertices = new List<Coordinate>
            {
                new Coordinate(0, 0),
                new Coordinate(100, 0)
            };
            branch1.Geometry = GeometryFactory.CreateLineString(vertices.ToArray());


            network.Branches.Add(branch1);

            
            // add cross-sections
            AddDefaultCrossSection(branch1, "crs1", 40);
            // add discretization
            var networkDiscretization = new Discretization
            {
                Network = network,
                SegmentGenerationMethod =
                    SegmentGenerationMethod.SegmentBetweenLocationsFullyCovered
            };

            foreach (IChannel channel in network.Channels)
            {
                HydroNetworkHelper.GenerateDiscretization(networkDiscretization, channel, 0, false, 0.5, false, false, true,
                    channel.Length / 2.0);
            }
            UnstructuredGridFileHelper.WriteGridToFile(@"c:\test.nc", null, network, networkDiscretization, null, "Luca", "LucaPlugin", "1.0", UnstructuredGridFileHelper.BedLevelLocation.NodesMaxLev, new double[1] { 80.1 });


        }
        [Test]
        [Ignore]
        public void Runnetworkanddiscr()
        {
            var network = new HydroNetwork();

            // add nodes and branches
            var node1 = new HydroNode { Name = "node1", Network = network, Geometry = new Point(0, 0) };
            var node2 = new HydroNode { Name = "node2", Network = network, Geometry = new Point(100, 0) };

            network.Nodes.Add(node1);
            network.Nodes.Add(node2);

            var branch1 = new Channel("branch1", node1, node2);
            var vertices = new List<Coordinate>
            {
                new Coordinate(0, 0),
                new Coordinate(100, 0)
            };
            branch1.Geometry = GeometryFactory.CreateLineString(vertices.ToArray());


            network.Branches.Add(branch1);


            // add cross-sections
            AddDefaultCrossSection(branch1, "crs1", 40);
            // add discretization
            var networkDiscretization = new Discretization
            {
                Network = network,
                SegmentGenerationMethod =
                    SegmentGenerationMethod.SegmentBetweenLocationsFullyCovered
            };

            foreach (IChannel channel in network.Channels)
            {
                HydroNetworkHelper.GenerateDiscretization(networkDiscretization, channel, 0, false, 0.5, false, false, true,
                    channel.Length / 2.0);
            }
            using(var app = new DeltaShellApplication()) 
            {
                app.Plugins.Add(new FlowFMApplicationPlugin());
                app.Run();
                using (var fmmodel = new WaterFlowFMModel()
                {
                    Network = network,
                    NetworkDiscretization = networkDiscretization,
                    MduFilePath = Path.Combine(FileUtils.CreateTempDirectory(), "LucaIsGreat.mdu")
                })
                {
                    ActivityRunner.RunActivity(fmmodel);
                    Assert.AreEqual(ActivityStatus.Cleaned, fmmodel.Status);
                }
            }

        }
        /// <summary>
        /// same as DeltaShell.Plugins.DelftModels.WaterFlowModel.Tests.WaterFlowModel1DTestHelper
        /// adding reference to DeltaShell.Plugins.DelftModels.WaterFlowModel.Tests breaks tests on buildserver
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="name"></param>
        /// <param name="chainage"></param>
        private static void AddDefaultCrossSection(IChannel channel, string name, double chainage)
        {
            var yzCoordinates = new List<Coordinate>
            {
                new Coordinate(0.0, 0.0),
                new Coordinate(100.0, 0.0),
                new Coordinate(150.0, -10.0),
                new Coordinate(300.0, -10.0),
                new Coordinate(350.0, 0.0),
                new Coordinate(500.0, 0.0)
            };

            var cs1 = CrossSectionHelper.AddXYZCrossSectionFromYZCoordinates(channel, chainage, yzCoordinates);
            cs1.Definition.Name = name;
        }
    }
}