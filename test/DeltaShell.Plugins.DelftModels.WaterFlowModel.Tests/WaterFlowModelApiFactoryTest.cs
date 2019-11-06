using System;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.ModelApiControllers;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.ModelApiControllers.ModelApi;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.Tests
{
    [TestFixture]
    public class WaterFlowModelApiFactorTest
    {
        private IModelApi api;

        [SetUp]
        public void SetUp()
        {
            api = WaterFlowModelApiFactory.CreateApi(true);
        }

        [Test]
        [Ignore("Flow1D (cf_dll) kernel is not in dimr set any more")]
        public void TestGetVarShape()
        {
            // arrays cannot be declared by out, only by ref.
            int[] shape = new int[6];
            api.get_var_shape("s1", ref shape);
        }

        [Test]
        [Ignore("Flow1D (cf_dll) kernel is not in dimr set any more")]
        public void TestGetVarType()
        {
            // testing a simple call to the RPC without crashing.
            string result;
            api.get_var_type("s1", out result);
        }

        [Test]
        [Ignore("Flow1D (cf_dll) kernel is not in dimr set any more")]
        public void TestGetVar()
        {
            Array result = new int[0];
            api.get_var("s1", ref result);
        }
    }
}
