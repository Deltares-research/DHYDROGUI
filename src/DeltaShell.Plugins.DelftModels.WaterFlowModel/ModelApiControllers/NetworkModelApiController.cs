using DeltaShell.Plugins.DelftModels.WaterFlowModel.ModelApiControllers.ModelApi;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.ModelApiControllers
{
    /// <summary>
    /// Class responsible for setting network (not structure) properties in modelApi. Like discretization etc. Does MINIMAL validation. Extra validation should be done in caller
    /// </summary>
    public class NetworkModelApiController
    {
        private readonly IModelApi modelApi;

        public NetworkModelApiController(IModelApi modelApi)
        {
            this.modelApi = modelApi;
        }
    }
}