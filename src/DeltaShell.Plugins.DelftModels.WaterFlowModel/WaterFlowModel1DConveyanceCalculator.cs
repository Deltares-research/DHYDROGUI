using System;
using DelftTools.Functions;
using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.Helpers;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.ModelApiControllers;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.ModelApiControllers.ModelApi;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel
{
    public class WaterFlowModel1DConveyanceCalculator : ConveyanceCalculatorBase
    {
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
            if (crossSection.CrossSectionType != CrossSectionType.YZ &&
                crossSection.CrossSectionType != CrossSectionType.GeometryBased)
            {
                throw new ArgumentException("Conveyance calculation is only available for YZ and geometry based cross sections");
            }

            int engineId = crossSectionApi.SetProfileInModelApi(crossSection, crossSection.Definition, true);
            int idForConveyance = modelApi.NetworkSetCS(0, 0, engineId, 0);

            double[] levels = new double[0];
            double[] flowArea = new double[0];
            double[] flowWidth = new double[0];
            double[] perimeter = new double[0];
            double[] conveyancePos = new double[0];
            double[] conveyanceNeg = new double[0];
            double[] hydraulicRadius = new double[0];
            double[] totalWidth = new double[0];
            modelApi.GetConveyanceTable(idForConveyance,
                                        ref levels,
                                        ref flowArea,
                                        ref flowWidth,
                                        ref perimeter,
                                        ref hydraulicRadius,
                                        ref totalWidth,
                                        ref conveyancePos,
                                        ref conveyanceNeg);
            
            var conveyanceFunction = GetEmptyConveyanceFunction();
            conveyanceFunction.Arguments[0].SetValues(levels);
            conveyanceFunction.Components[0].SetValues(conveyancePos);
            conveyanceFunction.Components[1].SetValues(flowArea);
            conveyanceFunction.Components[2].SetValues(flowWidth);
            conveyanceFunction.Components[3].SetValues(perimeter);
            conveyanceFunction.Components[4].SetValues(hydraulicRadius);
            conveyanceFunction.Components[5].SetValues(totalWidth);
            conveyanceFunction.Components[6].SetValues(conveyanceNeg);

            
            modelApi.Finalize();

            return conveyanceFunction;
        }
    }
}
