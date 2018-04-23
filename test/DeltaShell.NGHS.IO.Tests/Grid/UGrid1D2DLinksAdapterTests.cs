using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DelftTools.TestUtils;
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
            Assert.Greater(links.Count,0);
        }

        [Test]
        public void SaveAndLoad1D2DLinks()
        {
            var testFilePath = "haha.nc";
            UGrid1D2DLinksAdapter.Save1D2DLinks(testFilePath, new List<WaterFlowFM1D2DLink>());

            UGrid1D2DLinksAdapter.Load1D2DLinks(testFilePath);
        }
    }
}
