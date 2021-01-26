using System;
using System.Linq;
using DelftTools.Hydro.Area.Objects;
using DelftTools.TestUtils;
using DelftTools.Utils.IO;
using DeltaShell.Plugins.FMSuite.FlowFM.Api;
using DeltaShell.Plugins.FMSuite.FlowFM.Model;
using NUnit.Framework;
using SharpMap.Api;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.Api
{
    [TestFixture]
    [Category(TestCategory.Integration)]
    [Category(TestCategory.Slow)]
    public class GridOperationApiTest
    {
        [Test]
        public void GetEdgeCellsViaApi()
        {
            string path = TestHelper.GetTestFilePath(@"developer1d2dmodel\dflow-fm.mdu");
            path = TestHelper.CreateLocalCopy(path);

            var model = new WaterFlowFMModel();
            model.ImportFromMdu(path);

            IGridOperationApi api = new UnstrucGridOperationApi(model);
            int[] result = api.GetLinkedCells();

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Any());
        }

        [Test]
        public void GetEdgeCellsViaIndirectApiCall()
        {
            string path = TestHelper.GetTestFilePath(@"developer1d2dmodel\dflow-fm.mdu");
            path = TestHelper.CreateLocalCopy(path);

            var model = new WaterFlowFMModel();
            model.ImportFromMdu(path);

            int[] result = model.GetLinkedCells();

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Any());
        }

        [Test]
        public void InitializeUnstrucGridOperationApi_DoesNotWrite_StructureProperty()
        {
            string mduPath = TestHelper.GetTestFilePath(@"GridOperationApi\FlowFM\FlowFM.mdu");
            mduPath = TestHelper.CreateLocalCopy(mduPath);

            var model = new WaterFlowFMModel();
            model.ImportFromMdu(mduPath);

            try
            {
                using (var api = new UnstrucGridOperationApi(model, false))
                {
                    IPump pump = model.Area.Pumps.FirstOrDefault();
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