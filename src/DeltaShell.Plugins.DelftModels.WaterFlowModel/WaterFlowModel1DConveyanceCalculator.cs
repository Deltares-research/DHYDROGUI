using DelftTools.Functions;
using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.Helpers;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.ModelApiControllers;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.ModelApiControllers.ModelApi;
using log4net;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel
{
    public class WaterFlowModel1DConveyanceCalculator : ConveyanceCalculatorBase
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(WaterFlowModel1DConveyanceCalculator));

        private readonly CrossSectionModelApiController crossSectionApi;
        private readonly IModelApi modelApi;
        
        /// <summary>
        /// Create a conveyance calculator using a specific flow model
        /// </summary>
        /// <param name="flowModel">the flow model</param>
        public WaterFlowModel1DConveyanceCalculator(WaterFlowModel1D flowModel)
        {
            // use local instance
            modelApi = WaterFlowModelApiFactory.CreateApi(true);
            crossSectionApi = new CrossSectionModelApiController(modelApi, flowModel.RoughnessSections);
        }

        public override IFunction GetConveyance(ICrossSection crossSection)
        {
            // TODO: ModelApi functions were removed, this needs to be re-implemented at some point
            Log.Error("Get Conveyance feature has been disabled");
            return null;
        }
    }
}
