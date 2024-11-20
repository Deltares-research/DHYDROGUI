using System.IO;
using System.Linq;
using DelftTools.TestUtils;
using DeltaShell.Plugins.FMSuite.FlowFM.FunctionStores;
using GeoAPI.Extensions.Coverages;
using NetTopologySuite.Extensions.Coverages;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.IO {
    [TestFixture]
    [Category(TestCategory.DataAccess)]
    public class FMClassMapFileFunctionStoreTest
    {
        [Test]
        public void OpenClassMapFileCheckFunctions()
        {
            string testDataFilePath = TestHelper.GetTestFilePath(@"output_clmfiles");
            string flowfmClmNc = "FlowFM_clm.nc";
            flowfmClmNc = TestHelper.CreateLocalCopy(Path.Combine(testDataFilePath, flowfmClmNc)); 
            var store = new FMClassMapFileFunctionStore(flowfmClmNc);
            Assert.AreEqual(4, store.Functions.OfType<INetworkCoverage>().Count());
            var waterLevelFunction = (NetworkCoverage)store.Functions.FirstOrDefault(f => f.Components[0].Name == "mesh1d_s1");
            Assert.That(waterLevelFunction, Is.Not.Null);
            Assert.That(waterLevelFunction.GetValues<double>().Count, Is.EqualTo(2150));

        }
    }
}