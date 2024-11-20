using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.TestUtils;
using DelftTools.Utils.IO;
using DeltaShell.Plugins.FMSuite.FlowFM;
using DeltaShell.Plugins.ImportExport.Sobek.PartialSobekImporter;
using GeoAPI.Geometries;
using NetTopologySuite.Extensions.Coverages;
using NetTopologySuite.Geometries;
using NUnit.Framework;

namespace DeltaShell.Plugins.ImportExport.Sobek.Tests.PartialSobekImport
{
    [TestFixture]
    public class SobekComputationalGridImporterTest
    {

        [Test]
        [Category(TestCategory.DataAccess)]
        public void ImportComputationalGrid()
        {
            var pathToSobekNetwork = TestHelper.GetTestDataDirectory() + @"\ReModels\JAMM2010.sbk\40\DEFTOP.1";
            var waterFlowFmModel = new WaterFlowFMModel();

            var importer = PartialSobekImporterBuilder.BuildPartialSobekImporter(pathToSobekNetwork,
                waterFlowFmModel,
                new IPartialSobekImporter[]
                {
                    new SobekBranchesImporter(),
                    new SobekComputationalGridImporter()
                });

            importer.Import();

            Assert.IsNotNull(waterFlowFmModel.NetworkDiscretization);
            Assert.AreEqual(721, waterFlowFmModel.NetworkDiscretization.Locations.Values.Count);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        [Category(TestCategory.Slow)]
        public void ImportComputationalGridLeiderdorp()
        {
            string testDataFilePath = TestHelper.GetTestFilePath(@"Leiddrp.lit.zip");
            TestHelper.PerformActionInTemporaryDirectory(tempDir =>
            {
                ZipFileUtils.Extract(testDataFilePath, tempDir);
                var pathToSobekNetwork = Path.Combine(tempDir, "Leiddrp.lit","11", "NETWORK.tp");
                var waterFlowFmModel = new WaterFlowFMModel();

                var importer = PartialSobekImporterBuilder.BuildPartialSobekImporter(pathToSobekNetwork,
                    waterFlowFmModel,
                    new IPartialSobekImporter[]
                    {
                        new SobekBranchesImporter(),
                        new SobekComputationalGridImporter()
                    });

                importer.Import();

                Assert.IsNotNull(waterFlowFmModel.NetworkDiscretization);
                Assert.AreEqual(6646, waterFlowFmModel.NetworkDiscretization.Locations.Values.Count);
            });
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        [Category(TestCategory.Slow)]
        [TestCase(1, 3535)]
        [TestCase(2, 3535)]
        [TestCase(3, 3541)] // almost all branches have a geometry with length = 0 but with custom length 1
        [TestCase(4, 3535)]
        [TestCase(10, 3500)]
        public void ImportComputationalGridHeemskerk(int sobekCase, int points)
        {
            string testDataFilePath = TestHelper.GetTestFilePath(@"Heemsker.lit.zip");

            TestHelper.PerformActionInTemporaryDirectory(tempDir =>
            {
                ZipFileUtils.Extract(testDataFilePath, tempDir);
                var pathToSobekNetwork = Path.Combine(tempDir, "Heemsker.lit", sobekCase.ToString(), "NETWORK.tp");
                var waterFlowFmModel = new WaterFlowFMModel();

                var importer = PartialSobekImporterBuilder.BuildPartialSobekImporter(pathToSobekNetwork,
                    waterFlowFmModel,
                    new IPartialSobekImporter[]
                    {
                        new SobekBranchesImporter(),
                        new SobekComputationalGridImporter()
                    });

                importer.Import();

                Assert.IsNotNull(waterFlowFmModel.NetworkDiscretization);
                Assert.AreEqual(points, waterFlowFmModel.NetworkDiscretization.Locations.Values.Count);
            });
        }
        [Test]
        [Category(TestCategory.DataAccess)]
        public void ImportComputationalGridReWithOptionOnCrossSectionsOnly()
        {
            var pathToSobekNetwork = TestHelper.GetTestDataDirectory() + @"\ReModels\JAMM2010.sbk\40\DEFTOP.1";
            var waterFlowFmModel = new WaterFlowFMModel("waterflowfm");

            var importer = PartialSobekImporterBuilder.BuildPartialSobekImporter(pathToSobekNetwork,
                waterFlowFmModel,
                new IPartialSobekImporter[]
                {
                    new SobekBranchesImporter(),
                    new SobekCrossSectionsImporter(),
                    new SobekComputationalGridImporter()
                });

            importer.Import();

            // nr of cross sections on branch, per id, for branches which
            // have 'on cross section = 1' in  DEFGRD.1. The cross section
            // definitions are from DEFCRS.1 in SobekRE model "JAMM2010.sbk\40\"
            var nrOfCrossSectionsLookup = new Dictionary<string, int>()
            {
                {"025", 2},
                {"026", 2},
                {"027", 21},
                {"028", 2},
                {"030", 6},
                {"031", 2},
                {"033", 19},
                {"034", 13}
            };

            // note: for this model, there are always cross sections positioned on the branch's start and end points
            foreach (var idAndNrCrossSections in nrOfCrossSectionsLookup)
            {
                var branch = waterFlowFmModel.Network.Branches.First(b => b.Name == idAndNrCrossSections.Key);
                var nrOfPoints = waterFlowFmModel.NetworkDiscretization.GetLocationsForBranch(branch).Count;
                Assert.AreEqual(idAndNrCrossSections.Value, nrOfPoints,
                    String.Format("Expected grid points for branch {0}", idAndNrCrossSections.Key));
            }

        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void ImportComputationalGrid_ShouldNotRemoveTheGridPointsOfPipes_OnlyGridPointsOfChannels_AndRetainTheFixedGridPoints()
        {
            //initialize
            var pathToSobekNetwork = TestHelper.GetTestDataDirectory() + @"\Groesbeek.lit\Network.TP";
            var waterFlowFmModel = new WaterFlowFMModel();

            var pipeImporter = PartialSobekImporterBuilder.BuildPartialSobekImporter(pathToSobekNetwork, waterFlowFmModel,
                new IPartialSobekImporter[]
                {
                    new SobekBranchesImporter()
                });
            pipeImporter.Import();

            var nPipeCalculationPoints = 870;

            Assert.AreEqual(nPipeCalculationPoints, waterFlowFmModel.NetworkDiscretization.Locations.AllValues.Count);
            
            for (int i = 5; i < 10; i++)
            {
                waterFlowFmModel.NetworkDiscretization.ToggleFixedPoint(waterFlowFmModel.NetworkDiscretization.Locations.Values[i]);
            }

            //add channel & nodes
            var node1 = new HydroNode("node1"){Geometry = new Point(0,0)};
            var node2 = new HydroNode("node2"){Geometry = new Point(10,0)};
            var channel = new Channel("channel1", node1, node2){Geometry = new LineString(new[]{ new Coordinate(0,0), new Coordinate(10, 0) })};
            waterFlowFmModel.Network.Branches.Add(channel);

            ////add channel location which should NOT be removed because we MERGE!
            var channelLocation = new NetworkLocation(channel, 0.12345);
            waterFlowFmModel.NetworkDiscretization.Locations.Values.Add(channelLocation);
            Assert.AreEqual(1, waterFlowFmModel.NetworkDiscretization.Locations.AllValues.Count(l => l.Equals(channelLocation)));
            for (int i = 5; i < 10; i++)
            {
                Assert.IsTrue(waterFlowFmModel.NetworkDiscretization.IsFixedPoint(waterFlowFmModel.NetworkDiscretization.Locations.Values[i]));
            }

            //action
            var calculationPointsImporter = PartialSobekImporterBuilder.BuildPartialSobekImporter(pathToSobekNetwork,
                waterFlowFmModel,
                new IPartialSobekImporter[]
                {
                    new SobekComputationalGridImporter()
                });

            calculationPointsImporter.Import();

            //check
            Assert.GreaterOrEqual(waterFlowFmModel.NetworkDiscretization.Locations.AllValues.Count, nPipeCalculationPoints);
            Assert.AreEqual(1,waterFlowFmModel.NetworkDiscretization.Locations.AllValues.Count(l => l.Equals(channelLocation)));
            for (int i = 5; i < 10; i++)
            {
                Assert.IsTrue(waterFlowFmModel.NetworkDiscretization.IsFixedPoint(waterFlowFmModel.NetworkDiscretization.Locations.Values[i]));
            }
        }
    }
}
