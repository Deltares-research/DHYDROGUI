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
            if (!ValueEqualsDefinition(category.Name, ModelDefinitionsRegion.TransportComputationValuesHeader)) return;

            foreach (var property in category.Properties)
            {
                if (ValueEqualsDefinition(property.Name, ModelDefinitionsRegion.UseTemperature.Key))
                {
                    model.UseTemperature = property.Value != "0" && property.Value == "1";
                }
                else if(ValueEqualsDefinition(property.Name, ModelDefinitionsRegion.Density.Key))
                {
                    model.DensityType = (DensityType)Enum.Parse(typeof(DensityType), property.Value);
                }
                else if (ValueEqualsDefinition(property.Name, ModelDefinitionsRegion.HeatTransferModel.Key))
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
