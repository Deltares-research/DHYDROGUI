using System;
using System.IO;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.TestUtils;
using DelftTools.Utils.IO;
using DeltaShell.Plugins.DelftModels.WaterFlowModel;
using DeltaShell.Plugins.ImportExport.Sobek.PartialSobekImporter;
using NetTopologySuite.Extensions.Networks;
using NUnit.Framework;

namespace DeltaShell.Plugins.ImportExport.Sobek.Tests.PartialSobekImport
{
    [TestFixture]
    public class SobekLinkageNodeImporterTest
    {

        [Test]
        [Category(TestCategory.DataAccess)]
        public void ImportNetworkWithConnectionNodeSetOrderNumber()
        {
            string pathToSobekNetwork = TestHelper.GetTestFilePath(@"LinkageNodes\TestConnectionNode\Network.TP");

            var network = new HydroNetwork();
            var importer = PartialSobekImporterBuilder.BuildPartialSobekImporter(pathToSobekNetwork, network, new IPartialSobekImporter[] { new SobekBranchesImporter(), new SobekLinkageNodeImporter() });

            importer.Import();


            Assert.AreEqual(3, network.Nodes.Count);
            Assert.AreEqual(2, network.Channels.Count());

            var lstChannels = network.Channels.ToList();

            Assert.AreEqual(-1, lstChannels[0].OrderNumber);
            Assert.AreEqual(-1, lstChannels[1].OrderNumber);

        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void ImportNetworkWithLinkageNodeSetOrderNumber()
        {
            string pathToSobekNetwork = TestHelper.GetTestFilePath(@"LinkageNodes\TestLinkageNode\Network.TP");

            var network = new HydroNetwork();
            var importer = PartialSobekImporterBuilder.BuildPartialSobekImporter(pathToSobekNetwork, network, new IPartialSobekImporter[] { new SobekBranchesImporter(), new SobekLinkageNodeImporter() });

            importer.Import();

            Assert.AreEqual(4, network.Nodes.Count);
            Assert.AreEqual(3, network.Channels.Count());

            var lstChannels = network.Channels.ToList();

            //spitted branch has been added with index 1
            IChannel splittedChannel = lstChannels.First(b => b.Name == "1_B");
            Assert.AreEqual(lstChannels[0].Target.Geometry, splittedChannel.Source.Geometry);

            Assert.AreEqual(1, lstChannels[0].OrderNumber); //first part split branch
            Assert.AreEqual(-1, lstChannels[2].OrderNumber);

            Assert.AreEqual(lstChannels[0].OrderNumber, splittedChannel.OrderNumber); //second part split branch: same order number first part

        }

        [Test]
        [Category(TestCategory.DataAccess)]
        [Category(TestCategory.Slow)]
        public void ImportNetworkWithLinkageNodesCheckNumberOfBranchesAndOrderNumbers()
        {
            var zipFilePath = TestHelper.GetTestFilePath("LMW_LinkageNodes.zip");

            TestHelper.PerformActionInTemporaryDirectory(tempDir =>
            {
                ZipFileUtils.Extract(zipFilePath, tempDir);
                var pathToSobekNetwork = Path.Combine(tempDir, "LMW_LinkageNodes", "NETWORK.TP");

                var networkNotBeenSplitBranches = new HydroNetwork();
                var branchesImporter = PartialSobekImporterBuilder.BuildPartialSobekImporter(pathToSobekNetwork, networkNotBeenSplitBranches, new IPartialSobekImporter[] { new SobekBranchesImporter() });

                branchesImporter.Import();


                var networkBranchesSplitByLinkageNode = new HydroNetwork();
                var branchesSplitByLinkageNodeImporter = PartialSobekImporterBuilder.BuildPartialSobekImporter(pathToSobekNetwork, networkBranchesSplitByLinkageNode, new IPartialSobekImporter[] { new SobekBranchesImporter(), new SobekLinkageNodeImporter() });

                branchesSplitByLinkageNodeImporter.Import();

                Assert.AreEqual(networkNotBeenSplitBranches.Nodes.Count, networkBranchesSplitByLinkageNode.Nodes.Count);

                Assert.Less(networkNotBeenSplitBranches.Branches.Count, networkBranchesSplitByLinkageNode.Branches.Count);

                Assert.AreEqual(322, networkBranchesSplitByLinkageNode.Branches.Select(b => b.OrderNumber).Distinct().Count());
            });
        }

        [Test]
        [Category(TestCategory.VerySlow)]
        public void ImportModelWithLinkageNodesCheckUpdateDiscretization()
        {
            var zipFilePath = TestHelper.GetTestFilePath("LMW_LinkageNodes.zip");

            TestHelper.PerformActionInTemporaryDirectory(tempDir =>
            {
                ZipFileUtils.Extract(zipFilePath, tempDir);
                var pathToSobekNetwork = Path.Combine(tempDir, "LMW_LinkageNodes", "NETWORK.TP");

                var model = new WaterFlowModel1D();

                var branchesComputationalGridAndLinkageNodeImporter =
                    PartialSobekImporterBuilder.BuildPartialSobekImporter(
                        pathToSobekNetwork, model,
                        new IPartialSobekImporter[]
                        {
                            new SobekBranchesImporter(), new SobekComputationalGridImporter(),
                            new SobekLinkageNodeImporter()
                        });

                branchesComputationalGridAndLinkageNodeImporter.Import();

                var nBranches = model.Network.Branches.Count;

                //test if branches has been split
                Assert.Greater(
                    nBranches,
                    model.Network.Branches.GroupBy(b => b.OrderNumber)
                         .Count()); // Should be true for any network with at least 1 linkage Node

                //check calculation points at the end of each branch
                Assert.AreEqual(
                    nBranches,
                    model.NetworkDiscretization.Locations.Values.Count(
                        l => Math.Abs(l.Chainage - l.Branch.Length) < BranchFeature.Epsilon));

                //check calculation points at the begin of each branch
                Assert.AreEqual(
                    nBranches,
                    model.NetworkDiscretization.Locations.Values.Count(
                        l => Math.Abs(l.Chainage) < BranchFeature.Epsilon));
            });
        }
    }
}
