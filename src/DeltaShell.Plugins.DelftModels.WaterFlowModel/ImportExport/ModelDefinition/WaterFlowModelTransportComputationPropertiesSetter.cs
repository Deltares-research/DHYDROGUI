using System;
using System.Collections.Generic;
using DeltaShell.NGHS.IO.Helpers;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.PhysicalParameters;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.Properties;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport.ModelDefinition
{
    public class WaterFlowModelTransportComputationPropertiesSetter : IWaterFlowModelCategoryPropertySetter
    {
        public void SetProperties(DelftIniCategory category, WaterFlowModel1D model, IList<string> errorMessages)
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
                    errorMessages.Add(string.Format(Resources.SetProperties_Line__0___Parameter___1___found_in_the_md1d_file__This_parameter_will_not_be_imported,
                        property.LineNumber, property.Name));
                }
            }
        }
    }
}
