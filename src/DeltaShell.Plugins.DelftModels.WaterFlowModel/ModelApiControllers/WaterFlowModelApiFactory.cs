using DelftTools.Utils.Remoting;
using DeltaShell.Dimr;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.ModelApiControllers.ModelApi;
using ProtoBufRemote;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.ModelApiControllers
{
    public static class WaterFlowModelApiFactory
    {

        static WaterFlowModelApiFactory()
        {
            RemotingTypeConverters.RegisterTypeConverter(new ModelApiParameterTypeConverter());
            RemotingTypeConverters.RegisterTypeConverter(new LoggerToProtoConverter());
        }

        public static IModelApi CreateApi(string workingDirectory = null)
        {
            return CreateApi(true, workingDirectory);
        }

        public static IModelApi CreateApi(bool remote, string workingDirectory=null)
        {
            if (!remote) 
                return new ModelApi.ModelApi();

            var remoteModelApiWrapper = RemoteInstanceContainer.CreateInstance<IRemoteModelApiWrapper, RemoteModelApiWrapper>(workingDirectory, false, typeof(ModelApi.ModelApi).Assembly, typeof(DimrApi).Assembly);               
            return new LocalModelApiWrapper(remoteModelApiWrapper);
        }

        public static void Cleanup(bool remote, IModelApi api)
        {
            if (api is ModelApi.ModelApi)
                return; // nothing to cleanup

            var wrapper = api as LocalModelApiWrapper;
            if (wrapper != null)
                api = wrapper.RemoteApi;

            RemoteInstanceContainer.RemoveInstance(api);
        }
    }
}
