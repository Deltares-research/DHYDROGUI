using System;
using System.Collections.Generic;
using DeltaShell.NGHS.IO.Helpers;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.PhysicalParameters;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport.ModelDefinition
{
    public class WaterFlowModelTransportComputationPropertiesSetter : IWaterFlowModelCategoryPropertySetter
    {
        public void SetProperties(DelftIniCategory category, WaterFlowModel1D model, Action<string, IList<string>> createAndAddErrorReport)
        {
            var useTemperature = category.ReadProperty<string>(ModelDefinitionsRegion.UseTemperature.Key);
            var densityType = category.ReadProperty<string>(ModelDefinitionsRegion.Density.Key);
            var temperatureModelType = category.ReadProperty<string>(ModelDefinitionsRegion.HeatTransferModel.Key);

            model.UseTemperature = useTemperature != "0" && useTemperature == "1";
            model.DensityType = (DensityType) Enum.Parse(typeof(DensityType), densityType);
            model.TemperatureModelType = (TemperatureModelType) Enum.Parse(typeof(TemperatureModelType), temperatureModelType);
        }
    }
}
