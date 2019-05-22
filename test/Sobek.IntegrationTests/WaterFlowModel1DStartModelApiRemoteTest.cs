using DelftTools.TestUtils;
using DelftTools.Utils.Remoting;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.ModelApiControllers.ModelApi;
using log4net.Core;
using NUnit.Framework;

namespace Sobek.IntegrationTests
{

    [TestFixture]
    public class WaterFlowModel1DStartModelApiRemoteTest
    {

        [Test]
        [Category(TestCategory.Integration)] // TODO: what do we test here?!? Improve or remove.
        [Category(TestCategory.WorkInProgress)] //add proto serialziation / or custom type converter which uses xml serialization
        public void StartDummyModelApiImplementationRemote()
        {
            LogHelper.ConfigureLogging(Level.Debug);
            IModelApi modelApi = RemoteInstanceContainer.CreateInstance<IModelApi, ModelApi>();
            RemoteInstanceContainer.RemoveInstance(modelApi);
        }
    }
}
