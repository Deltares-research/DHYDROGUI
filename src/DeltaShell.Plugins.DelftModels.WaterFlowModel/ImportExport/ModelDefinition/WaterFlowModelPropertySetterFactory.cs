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
                default:
                    throw new NotImplementedException();
            }
        }
    }
}
