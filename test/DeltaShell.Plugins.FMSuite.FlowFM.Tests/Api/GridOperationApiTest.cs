using System.Linq;
using DelftTools.TestUtils;
using DeltaShell.Plugins.FMSuite.FlowFM.Api;
using NUnit.Framework;
using SharpMap;
using SharpMap.Api;
using SharpMap.Extensions.CoordinateSystems;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.Api
{
    [TestFixture]
    [Category(TestCategory.Integration)]
    [Category(TestCategory.Slow)]
    public class GridOperationApiTest
    {
        [OneTimeSetUp]
        public void SetMapCoordinateSystemFactory()
        {
            if (Map.CoordinateSystemFactory == null)
                Map.CoordinateSystemFactory = new OgrCoordinateSystemFactory();
        }

        [Test]
        public void GetEdgeCellsViaApi()
        {
            var path = TestHelper.GetTestFilePath(@"developer1d2dmodel\dflow-fm.mdu");
            path = TestHelper.CreateLocalCopy(path);

            var model = new WaterFlowFMModel(path);

            IGridOperationApi api = new UnstrucGridOperationApi(model);
            var result = api.GetLinkedCells();

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Any());
        }

        [Test]
        public void GetEdgeCellsViaIndirectApiCall()
        {
            var path = TestHelper.GetTestFilePath(@"developer1d2dmodel\dflow-fm.mdu");
            path = TestHelper.CreateLocalCopy(path);

            var model = new WaterFlowFMModel(path);

            var result = model.GetLinkedCells();

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Any());
        }
    }
}