using System;
using System.Collections.Generic;
using DeltaShell.NGHS.IO.Helpers;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.PhysicalParameters;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport.ModelDefinition
{
    public class WaterFlowModelTransportComputationPropertiesSetter : WaterFlowModelCategoryPropertySetter
    {
        public override void SetProperties(DelftIniCategory category, WaterFlowModel1D model, IList<string> errorMessages)
        {
            foreach (var property in category.Properties)
            {
                if (property.Name == ModelDefinitionsRegion.UseTemperature.Key)
                {
                    model.UseTemperature = property.Value != "0" && property.Value == "1";
                }
                else if(property.Name == ModelDefinitionsRegion.Density.Key)
                {
                    model.DensityType = (DensityType)Enum.Parse(typeof(DensityType), property.Value);
                }
                else if (property.Name == ModelDefinitionsRegion.HeatTransferModel.Key)
                {
                    model.TemperatureModelType = (TemperatureModelType)Enum.Parse(typeof(TemperatureModelType), property.Value);
                }
                else
                {
                    errorMessages.Add(GetUnsupportedPropertyWarningMessage(property));
                }
            }
        }
    }
}
