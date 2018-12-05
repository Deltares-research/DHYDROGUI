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
                default:
                    throw new NotImplementedException();
            }
        }
    }
}
