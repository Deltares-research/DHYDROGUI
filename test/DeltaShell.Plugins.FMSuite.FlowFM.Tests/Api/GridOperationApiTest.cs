using System;
using System.Linq;
using DelftTools.Hydro.Area.Objects.StructureObjects;
using DelftTools.TestUtils;
using DelftTools.Utils.IO;
using DeltaShell.Plugins.FMSuite.FlowFM.Api;
using DeltaShell.Plugins.FMSuite.FlowFM.Model;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.Api
{
    [TestFixture]
    [Category(TestCategory.Integration)]
    [Category(TestCategory.Slow)]
    public class GridOperationApiTest
    {
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