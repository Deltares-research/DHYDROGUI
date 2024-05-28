using System.Linq;
using DelftTools.Hydro;
using DelftTools.TestUtils;
using DeltaShell.Plugins.ImportExport.Sobek.PartialSobekImporter;
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
            string pathToSobekNetwork = TestHelper.GetTestFilePath(@"LMW_LinkageNodes\Network.TP");

            var networkNotBeenSplitBranches = new HydroNetwork();
            var branchesImporter = PartialSobekImporterBuilder.BuildPartialSobekImporter(pathToSobekNetwork, networkNotBeenSplitBranches, new IPartialSobekImporter[] { new SobekBranchesImporter() });

            branchesImporter.Import();


            var networkBranchesSplitByLinkageNode = new HydroNetwork();
            var branchesSplitByLinkageNodeImporter = PartialSobekImporterBuilder.BuildPartialSobekImporter(pathToSobekNetwork, networkBranchesSplitByLinkageNode, new IPartialSobekImporter[] { new SobekBranchesImporter(), new SobekLinkageNodeImporter() });

            branchesSplitByLinkageNodeImporter.Import();

            Assert.AreEqual(networkNotBeenSplitBranches.Nodes.Count, networkBranchesSplitByLinkageNode.Nodes.Count);

            Assert.Less(networkNotBeenSplitBranches.Branches.Count,networkBranchesSplitByLinkageNode.Branches.Count);

            Assert.AreEqual(322, networkBranchesSplitByLinkageNode.Branches.Select(b => b.OrderNumber).Distinct().Count());
        }
    }
}
