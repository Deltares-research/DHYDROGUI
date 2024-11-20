using System.Linq;
using DelftTools.TestUtils;
using DeltaShell.Plugins.ImportExport.Sobek.Tests;
using DeltaShell.Sobek.Readers.Readers.SobekRrReaders;
using DeltaShell.Sobek.Readers.SobekDataObjects;
using NUnit.Framework;

namespace DeltaShell.Sobek.Readers.Tests.Readers.SobekRrReaders
{
    [TestFixture]
    public class SobekRRNodeReaderTest
    {


 
      //id   =          node identification
      //nm  =          name of the node 
      //ri    =          reach identification 
      //mt  =          model nodetype
      //nt   =          netter nodetype
      //ObID =      Object id 
      //px  =          position X (X coordinate)
      //py  =          position Y (Y coordinate)

        [Test]
        [Category(TestCategory.DataAccess)]
        public void ReadRRNodeFromTholen()
        {
            string line =
                @"NODE id 'cGFE1002' nm 'cGFE1002Name' ri '499' mt 2 '6' nt 78 ObID 'SBK_SBK-3B-NODE' px 70100.3676822612 py 399831.775610428  node";

            var sobekRRNode = new SobekRRNodeReader().Parse(line).First();

            Assert.AreEqual("cGFE1002", sobekRRNode.Id);
            Assert.AreEqual("cGFE1002Name", sobekRRNode.Name);
            Assert.AreEqual("499", sobekRRNode.ReachId);
            Assert.AreEqual(SobekRRNodeType.Boundary, sobekRRNode.NodeType);
            Assert.AreEqual(78, sobekRRNode.NetterType);
            Assert.AreEqual("SBK_SBK-3B-NODE", sobekRRNode.ObjectTypeName);
            Assert.AreEqual(70100.3676822612, sobekRRNode.X);
            Assert.AreEqual(399831.775610428, sobekRRNode.Y);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void ReadSobekRRNodeFile()
        {
            var path = TestHelper.GetTestDataDirectoryPathForAssembly(typeof(SobekNetworkImporterTest).Assembly, @"Tholen.lit\29\3B_Nod.tp");
            var lstRRNode = new SobekRRNodeReader().Read(path);
            Assert.AreEqual(680, lstRRNode.Count());
        }
    }

}
