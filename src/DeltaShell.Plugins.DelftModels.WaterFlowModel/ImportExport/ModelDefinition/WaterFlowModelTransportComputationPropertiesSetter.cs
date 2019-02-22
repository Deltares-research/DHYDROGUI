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
            if (category == null) return;
            if (!string.Equals(category.Name, ModelDefinitionsRegion.TransportComputationValuesHeader, StringComparison.OrdinalIgnoreCase)) return;

            foreach (var property in category.Properties)
            {
                if (string.Equals(property.Name, ModelDefinitionsRegion.UseTemperature.Key, StringComparison.OrdinalIgnoreCase))
                {
                    model.UseTemperature = property.Value != "0" && property.Value == "1";
                }
                else if(string.Equals(property.Name, ModelDefinitionsRegion.Density.Key, StringComparison.OrdinalIgnoreCase))
                {
                    model.DensityType = (DensityType)Enum.Parse(typeof(DensityType), property.Value);
                }
                else if (string.Equals(property.Name, ModelDefinitionsRegion.HeatTransferModel.Key,
                    StringComparison.OrdinalIgnoreCase))
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
