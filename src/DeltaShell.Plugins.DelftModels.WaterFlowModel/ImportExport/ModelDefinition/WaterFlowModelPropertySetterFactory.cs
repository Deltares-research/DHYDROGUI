using System;
using DeltaShell.NGHS.IO.Helpers;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport.ModelDefinition
{
    public static class WaterFlowModelPropertySetterFactory
    {
        public static IWaterFlowModelCategoryPropertySetter GetPropertySetter(DelftIniCategory category)
        {
            switch (category.Name)
            {
                case ModelDefinitionsRegion.TimeHeader:
                    return new WaterFlowModelTimePropertiesSetter();
                case ModelDefinitionsRegion.ResultsNodesHeader:
                case ModelDefinitionsRegion.ResultsBranchesHeader:
                case ModelDefinitionsRegion.ResultsStructuresHeader:
                case ModelDefinitionsRegion.ResultsPumpsHeader:
                case ModelDefinitionsRegion.ResultsObservationsPointsHeader:
                case ModelDefinitionsRegion.ResultsRetentionsHeader:
                case ModelDefinitionsRegion.ResultsLateralsHeader:
                case ModelDefinitionsRegion.ResultsWaterBalanceHeader:
                case ModelDefinitionsRegion.FiniteVolumeGridOnGridPoints:
                    return new WaterFlowModelOutputSetter();
                case ModelDefinitionsRegion.GlobalValuesHeader:
                    return new WaterFlowModelGlobalValuesSetter();
                case ModelDefinitionsRegion.TransportComputationValuesHeader:
                    return new WaterFlowModelTransportComputationPropertiesSetter();
                case ModelDefinitionsRegion.NumericalParametersValuesHeader:
                    return new WaterFlowModelNumericalParametersSetter();
                case ModelDefinitionsRegion.SimulationOptionsValuesHeader:
                    return new WaterFlowModelSimulationOptionsSetter();
                case ModelDefinitionsRegion.InitialConditionsValuesHeader:
                    return new WaterFlowModelInitialConditionsParameterSetter();
                case ModelDefinitionsRegion.ObservationsHeader:
                    return new WaterFlowModelObservationsParameterSetter();
                case ModelDefinitionsRegion.RestartHeader:
                    return new WaterFlowModelRestartSetter();
                default:
                    throw new NotImplementedException();
            }
        }
    }
}
