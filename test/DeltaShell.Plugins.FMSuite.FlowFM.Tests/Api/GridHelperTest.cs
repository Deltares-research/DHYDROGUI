using System;
using DelftTools.TestUtils;
using DeltaShell.Plugins.FMSuite.FlowFM.Api.TempImpl;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.Api
{
    [TestFixture]
    public class GridHelperTest
    {
        [Test]
        public void CreateUnstructuredGridFromNullPathTest()
        {
            Assert.IsNull(GridHelper.CreateUnstructuredGridFromNetCdfFor1D2DLinks(null));
        }

        [Test]
        [Category(TestCategory.WorkInProgress)]
        public void CreateUnstructuredGridFromNetCdfFor1D2DLinksFailsToOpenTest()
        {
            var testFile = TestHelper.GetTestFilePath(@"data\unstructured_example.net");
            testFile = TestHelper.CreateLocalCopy(testFile);

            try
            {
                Assert.IsNull(GridHelper.CreateUnstructuredGridFromNetCdfFor1D2DLinks(testFile));
            }
            catch (InvalidOperationException e)
            {
                //This is what we want to hit.
            }
            catch (Exception e)
            {
                Assert.Fail("Unexpected exception thrown: {0}", e.Message);
            }
        }
    }
}