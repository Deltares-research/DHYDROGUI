using System;
using System.Linq;
using DelftTools.TestUtils;
using DelftTools.Utils.IO;
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
        [TestFixtureSetUp]
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

        [Test]
        public void InitializeUnstrucGridOperationApi_DoesNotWrite_StructureProperty()
        {
            var mduPath = TestHelper.GetTestFilePath(@"GridOperationApi\FlowFM\FlowFM.mdu");
            mduPath = TestHelper.CreateLocalCopy(mduPath);

            var model = new WaterFlowFMModel(mduPath);

            try
            {
                using (var api = new UnstrucGridOperationApi(model, false))
                {
                    var pump = model.Area.Pumps.FirstOrDefault();
                    Assert.IsNotNull(pump);
                    try
                    {
                        api.GetGridSnappedGeometry(UnstrucGridOperationApi.Pump, pump.Geometry);
                    }
                    catch (Exception e)
                    {
                        Assert.Fail("It should have not thrown the following exception: {0}", e.Message);
                    }
                }
            }
            catch (Exception e)
            {
                Assert.Fail("It should have not thrown the following exception: {0}", e.Message);
            }
            

            FileUtils.DeleteIfExists(mduPath);
        }
    }
}