using System.IO;
using System.Linq;
using DelftTools.TestUtils;
using DeltaShell.Plugins.ImportExport.Sobek.Tests;
using DeltaShell.Sobek.Readers.Readers.SobekRrReaders;
using NUnit.Framework;

namespace DeltaShell.Sobek.Readers.Tests.Readers.SobekRrReaders
{

    [TestFixture]
    public class SobekRRLinkReaderTest
    {
        //id   =          link identification 
        //nm  =          name of the link
        //ri    =          reach identification
        //mt  =          model type 
        //bt   =          branch type
        //ObID=       Object identification
        //bn  =          identification of begin node (‘from’ node)
        //en   =          identification of end node (‘to’ node)

        //The model type, branch type en Object Id are not used by the RR-computational core, but are used by user-interface programs (Netter).

        [Test]
        [Category(TestCategory.DataAccess)]
        public void ReadRRLinkFromTholen()
        {
            string line = @"BRCH id 'RRRR 3' nm '3' ri '-1' mt 1 '0' bt 17 ObID '3B_LINK' bn 'upGFE822' en 'cGFE822'  brch";

            var linkFile = "tmp.links";
            File.WriteAllText(linkFile, line);
            var sobekRRLink = new SobekRRLinkReader().Read(linkFile).First();

            Assert.AreEqual("RRRR 3", sobekRRLink.Id);
            Assert.AreEqual("3", sobekRRLink.Name);
            Assert.AreEqual("-1", sobekRRLink.ReachId);
            Assert.AreEqual("upGFE822", sobekRRLink.NodeFromId);
            Assert.AreEqual("cGFE822", sobekRRLink.NodeToId);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void ReadSobekRRLinkFile()
        {
            var path = TestHelper.GetTestDataDirectoryPathForAssembly(typeof(SobekNetworkImporterTest).Assembly,@"Tholen.lit\29\3B_Link.tp");
            var lstRRLink = new SobekRRLinkReader().Read(path);
            Assert.AreEqual(426, lstRRLink.Count());
        }
    }
}
