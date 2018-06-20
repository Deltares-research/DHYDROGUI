using System.Collections.Generic;
using DelftTools.TestUtils;
using DelftTools.Utils.IO;
using DeltaShell.NGHS.IO.Grid;
using DeltaShell.Plugins.FMSuite.FlowFM;
using DeltaShell.Plugins.FMSuite.FlowFM.IO;
using NUnit.Framework;

namespace DeltaShell.NGHS.IO.Tests.Grid
{
    [TestFixture]
    public class UGrid1D2DLinksAdapterTests
    {
        private const string UGRID_TEST_FILE = @"ugrid\Ugrid_1D2D.nc";

        [Test]
        public void Load1D2DLinks()
        {
            var testFilePath = TestHelper.GetTestFilePath(UGRID_TEST_FILE);

            var localCopyOfTestFile = TestHelper.CreateLocalCopy(testFilePath);
            var links = UGrid1D2DLinksAdapter.Load1D2DLinks(localCopyOfTestFile);

            Assert.IsNotNull(links);
            Assert.Greater(links.Count, 0);
        }

        [Test]
        public void SaveAndLoad1D2DLinks()
        {
            var testFilePath = TestHelper.GetTestFilePath(@"ugrid\Ugrid_Network_mesh1D_mesh2D_noLinks.nc");
            var localCopyOfTestFile = TestHelper.CreateLocalCopy(testFilePath);
            try
            {
                var links = new List<WaterFlowFM1D2DLink>
                {
                    new WaterFlowFM1D2DLink(2, 2) { Name = "link1", LongName = "link1 longname", TypeOfLink = GridApiDataSet.LinkType.Embedded },
                    new WaterFlowFM1D2DLink(4, 7) { Name = "link2", LongName = "link2 longname", TypeOfLink = GridApiDataSet.LinkType.RoofSewer }
                };

                UGrid1D2DLinksAdapter.Save1D2DLinks(localCopyOfTestFile, links);

                var retrievedLinks = UGrid1D2DLinksAdapter.Load1D2DLinks(localCopyOfTestFile);

                Assert.AreEqual(links.Count, retrievedLinks.Count);

                for (var i = 0; i < links.Count; i++)
                {
                    Assert.AreEqual(links[i].Name, retrievedLinks[i].Name);
                    Assert.AreEqual(links[i].LongName, retrievedLinks[i].LongName);
                    Assert.AreEqual(links[i].DiscretisationPointIndex, retrievedLinks[i].DiscretisationPointIndex);
                    Assert.AreEqual(links[i].FaceIndex, retrievedLinks[i].FaceIndex);
                    Assert.AreEqual(links[i].TypeOfLink, retrievedLinks[i].TypeOfLink);
                }
            }
            finally
            {
                FileUtils.DeleteIfExists(localCopyOfTestFile);
            }
        }

        [Test]
        public void SaveAndLoadNo1D2DLinks_ShouldNotGiveAnError()
        {
            var testFilePath = TestHelper.GetTestFilePath(@"ugrid\Ugrid_Network_mesh1D_mesh2D_noLinks.nc");
            var localCopyOfTestFile = TestHelper.CreateLocalCopy(testFilePath);
            try
            {
                var links = new List<WaterFlowFM1D2DLink>();

                UGrid1D2DLinksAdapter.Save1D2DLinks(localCopyOfTestFile, links);

                var retrievedLinks = UGrid1D2DLinksAdapter.Load1D2DLinks(localCopyOfTestFile);

                Assert.AreEqual(links.Count, retrievedLinks.Count);

            }
            finally
            {
                FileUtils.DeleteIfExists(localCopyOfTestFile);
            }
        }
    }
}
