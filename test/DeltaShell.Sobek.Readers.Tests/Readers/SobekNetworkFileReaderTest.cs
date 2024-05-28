using System.Linq;
using DeltaShell.Sobek.Readers.Readers;
using NUnit.Framework;

namespace DeltaShell.Sobek.Readers.Tests.Readers
{
    [TestFixture]
    public class SobekNetworkFileReaderTest
    {
        [Test]
        public void ReadBranch()
        {
            const string source = @"BRCH id '1' nm 'Tak1' bn '1' en '2' al 1233.4 brch";
            var sobekBranch = new SobekNetworkBranchFileReader().Parse(source).FirstOrDefault();

            Assert.IsNotNull(sobekBranch);
            Assert.AreEqual("1", sobekBranch.TextID);
            Assert.AreEqual("Tak1", sobekBranch.Name);
            Assert.AreEqual("1", sobekBranch.StartNodeID);
            Assert.AreEqual("2", sobekBranch.EndNodeID);
            Assert.AreEqual(1233.4, sobekBranch.Length);
        }

        [Test]
        public void ReadBranchWithAmpersandAndEqualSignInNodeName()
        {
            const string source = @"BRCH id '1' nm '' bn '5' en 'B2&B3_h_x=0m' al 1500 vc_opt 1 vc_equi -1 vc_len 250 brch";
            var sobekBranch = new SobekNetworkBranchFileReader().Parse(source).FirstOrDefault();

            Assert.IsNotNull(sobekBranch);
            Assert.AreEqual("1", sobekBranch.TextID);
            Assert.AreEqual("", sobekBranch.Name);
            Assert.AreEqual("5", sobekBranch.StartNodeID);
            Assert.AreEqual("B2&B3_h_x=0m", sobekBranch.EndNodeID);
            Assert.AreEqual(1500.0, sobekBranch.Length);
        }

        [Test]
        public void ReadBranchWithSearchKeyInSearchValues()
        {
            const string source = @"BRCH id 'Almelose kanaal ben id nm bn en al ' nm 'Almelose kanaal ben id nm bn en al ' bn '1 id nm bn en al ' en 'benedenrand_ZwarteWater id nm bn en al ' al 480.03 brch";
            var sobekBranch = new SobekNetworkBranchFileReader().Parse(source).FirstOrDefault();

            Assert.IsNotNull(sobekBranch);
            Assert.AreEqual("Almelose kanaal ben id nm bn en al ", sobekBranch.TextID);
            Assert.AreEqual("Almelose kanaal ben id nm bn en al ", sobekBranch.Name);
            Assert.AreEqual("1 id nm bn en al ", sobekBranch.StartNodeID);
            Assert.AreEqual("benedenrand_ZwarteWater id nm bn en al ", sobekBranch.EndNodeID);
            Assert.AreEqual(480.03, sobekBranch.Length);
        }

        [Test]
        public void ReadBranchWithKindOfSearchKeyInName()
        {
            const string source = @"BRCH id 'Onl_DM1062' nm 'Stadskanaal - 5e verlaat' bn 'Onl_DM1140' en 'Onl_DM1137' al 7608.51 vc_opt 1 vc_equi -1 vc_len 3000 brch";
            var sobekBranch = new SobekNetworkBranchFileReader().Parse(source).FirstOrDefault();

            Assert.IsNotNull(sobekBranch);
            Assert.AreEqual("Stadskanaal - 5e verlaat", sobekBranch.Name);
            Assert.AreEqual(7608.51, sobekBranch.Length);
        }

        [Test]
        public void ReadNode()
        {
            const string source = @"NODE id '1' nm 'Node1' px 11404.2 py 123768.5 node";
            var sobekNode = new SobekNetworkNodeFileReader().Parse(source).FirstOrDefault();

            Assert.IsNotNull(sobekNode);
            Assert.AreEqual("1", sobekNode.ID);
            Assert.AreEqual("Node1", sobekNode.Name);
            Assert.AreEqual(11404.2, sobekNode.X);
            Assert.AreEqual(123768.5, sobekNode.Y);
            Assert.AreEqual(false, sobekNode.IsLinkageNode);
        }

        [Test]
        public void ReadLinkageNode()
        {
            const string source = @"NDLK id '125' ci 'RIV_350' lc 25261.4 ndlk";
            var sobekLinkageNode = new SobekNetworkLinkageNodeFileReader().Parse(source).FirstOrDefault();

            Assert.IsNotNull(sobekLinkageNode);
            Assert.AreEqual("125", sobekLinkageNode.ID);
            Assert.AreEqual("RIV_350", sobekLinkageNode.BranchID);
            Assert.AreEqual(25261.4, sobekLinkageNode.ReachLocation);
            Assert.AreEqual(true, sobekLinkageNode.IsLinkageNode);
        }

        [Test]
        public void ReadLinkageNodeWithXY()
        {
            const string source = @"NDLK id '6' nm '' px 3359.319312144 py 0 ci '1' lc 3359.319312144 ndlk";
            var sobekLinkageNode = new SobekNetworkLinkageNodeFileReader().Parse(source).FirstOrDefault();

            Assert.IsNotNull(sobekLinkageNode);
            Assert.AreEqual("6", sobekLinkageNode.ID);
            Assert.AreEqual("1", sobekLinkageNode.BranchID);
            Assert.AreEqual(3359.319312144, sobekLinkageNode.X);
            Assert.AreEqual(0, sobekLinkageNode.Y);
            Assert.AreEqual(3359.319312144, sobekLinkageNode.ReachLocation);
            Assert.AreEqual(true, sobekLinkageNode.IsLinkageNode);
        }
    }
}
