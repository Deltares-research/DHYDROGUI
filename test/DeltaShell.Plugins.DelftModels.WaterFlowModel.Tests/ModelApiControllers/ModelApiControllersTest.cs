using System;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.ModelApiControllers;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.ModelApiControllers.ModelApi;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.Tests.ModelApiControllers
{
    public class ModelApiControllersTest
    {
        #region RemoteModelApiWrapperTest
        [Test]
        public void GetRemoteModelApiWrapperTest()
        {
            var PathValue = Environment.GetEnvironmentVariable("PATH");
            var remoteApiWrapper = new RemoteModelApiWrapper();
            Assert.AreNotEqual(PathValue, Environment.GetEnvironmentVariable("PATH"));
        }
        #endregion

        #region ModelApi
        [Test]
        public void GetModelApiWrapperTest()
        {
            var PathValue = Environment.GetEnvironmentVariable("PATH");
            var apiWrapper = new ModelApi();
            Assert.AreNotEqual(PathValue, Environment.GetEnvironmentVariable("PATH"));
        }

        [Test]
        public void ModelApiWrapperLoggingTest()
        {
            var apiWrapper = new ModelApi();
            apiWrapper.LoggingEnabled = false;
            Assert.IsFalse(apiWrapper.LoggingEnabled);
            apiWrapper.LoggingEnabled = true;
            Assert.IsTrue(apiWrapper.LoggingEnabled);
        }
        #endregion

        #region WaterFlowModelApiFactory

        [Test]
        public void CreateApiForWaterFlowModelApiFactoryTest()
        {
            try
            {
                var api = WaterFlowModelApiFactory.CreateApi(false);
                Assert.IsNotNull(api);
                //clean api
                WaterFlowModelApiFactory.Cleanup(false, api);
                //Assert.IsNull(api);
                var newApi = WaterFlowModelApiFactory.CreateApi(true);
                Assert.IsNotNull(newApi);
                WaterFlowModelApiFactory.Cleanup(true, newApi);
            }
            catch (Exception e)
            {
                Assert.Fail("Error with WaterFlowModelApiFactory: {0}", e.Message);
            }
        }
        #endregion
    }
}