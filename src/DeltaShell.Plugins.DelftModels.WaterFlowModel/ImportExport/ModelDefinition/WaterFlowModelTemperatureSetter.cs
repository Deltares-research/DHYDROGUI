using DeltaShell.NGHS.IO.Helpers;
using System;
using System.Collections.Generic;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport.ModelDefinition
{
    public class WaterFlowModelTemperatureSetter : IWaterFlowModelCategoryPropertySetter
    {
        public void SetProperties(DelftIniCategory temperatureCategory, WaterFlowModel1D model, IList<string> errorMessages)
        {
            if (temperatureCategory?.Name != ModelDefinitionsRegion.TemperatureValuesHeader) return;

            foreach (var property in temperatureCategory.Properties)
            {
                if (property.Name == ModelDefinitionsRegion.BackgroundTemperature.Key)
                {
                    model.BackgroundTemperature = ParseStringToDouble(property, errorMessages);
                }
                else if (property.Name == ModelDefinitionsRegion.SurfaceArea.Key)
                {
                    model.SurfaceArea = ParseStringToDouble(property, errorMessages);
                }
                else if (property.Name == ModelDefinitionsRegion.AtmosphericPressure.Key)
                {
                    model.AtmosphericPressure = ParseStringToDouble(property, errorMessages);
                }
                else if (property.Name == ModelDefinitionsRegion.DaltonNumber.Key)
                {
                    model.DaltonNumber = ParseStringToDouble(property, errorMessages);
                }
                else if (property.Name == ModelDefinitionsRegion.StantonNumber.Key)
                {
                    model.StantonNumber = ParseStringToDouble(property, errorMessages);
                }
                else if (property.Name == ModelDefinitionsRegion.HeatCapacity.Key)
                {
                    model.HeatCapacityWater = ParseStringToDouble(property, errorMessages);
                }
                else
                {
                    errorMessages.Add(
                        $"Line {property.LineNumber}: Parameter '{property.Name}' found in the md1d file. This parameter will not be imported, since it is not supported by the GUI");
                }
            }
        }

        private double ParseStringToDouble(DelftIniProperty property, IList<string> errorMessages)
        {
            try
            {
                return double.Parse(property.Value, System.Globalization.CultureInfo.InvariantCulture);
            }
            catch (Exception)
            {
                errorMessages.Add($"Line {property.LineNumber}: Parameter '{property.Name}' will not be imported. Valid values are doubles only.");
                return GetDefaultValueForProperty(property.Name);
            }           
        }

        private double GetDefaultValueForProperty(string propertyName)
        {
            if (propertyName == ModelDefinitionsRegion.BackgroundTemperature.Key)
            {
                return WaterFlowModel1DDataSet.Meteo.valueBackgroundTemperatureDefault;
            }
            if (propertyName == ModelDefinitionsRegion.SurfaceArea.Key)
            {
                return WaterFlowModel1DDataSet.Meteo.valueSurfaceAreaDefault;
            }
            if (propertyName == ModelDefinitionsRegion.AtmosphericPressure.Key)
            {
                return WaterFlowModel1DDataSet.Meteo.valueAtmosphericPressureDefault;
            }
            if (propertyName == ModelDefinitionsRegion.DaltonNumber.Key)
            {
                return WaterFlowModel1DDataSet.Meteo.valueDaltonNumberDefault;
            }
            if (propertyName == ModelDefinitionsRegion.StantonNumber.Key)
            {
                return WaterFlowModel1DDataSet.Meteo.valueStantonNumberDefault;
            }
            if (propertyName == ModelDefinitionsRegion.HeatCapacity.Key)
            {
                return WaterFlowModel1DDataSet.Meteo.valueHeatCapacityWaterDefault;
            }
            return 0.0d;
        }
    }
}
