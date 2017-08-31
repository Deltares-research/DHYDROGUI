using System;
using DeltaShell.Dimr;
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
            var environmentVariable = Environment.GetEnvironmentVariable("PATH");
            if (environmentVariable.Contains(DimrApiDataSet.SharedDllPath))
            {
                var newValue = environmentVariable.Replace(DimrApiDataSet.SharedDllPath, String.Empty);
                Environment.SetEnvironmentVariable("PATH", newValue);
            }
            Assert.IsFalse(Environment.GetEnvironmentVariable("PATH").Contains(DimrApiDataSet.SharedDllPath));
            var remoteApiWrapper = new RemoteModelApiWrapper();
            Assert.IsTrue(Environment.GetEnvironmentVariable("PATH").Contains(DimrApiDataSet.SharedDllPath));
        }
        #endregion

        #region ModelApi
        [Test]
        public void GetModelApiWrapperTest()
        {
            var apiWrapper = new ModelApi();
            Assert.IsTrue(Environment.GetEnvironmentVariable("PATH").Contains(DimrApiDataSet.SharedDllPath));
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