using DeltaShell.NGHS.IO.Helpers;
using System;
using System.Collections.Generic;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.Properties;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport.ModelDefinition
{
    /// <summary>
    /// WaterFlowModelTemperatureSetter sets property values described in the Temperature DelftIniCategory on the WaterFlowModel1D.
    /// </summary>
    /// <seealso cref="WaterFlowModelCategoryPropertySetter" />
    public class WaterFlowModelTemperatureSetter : WaterFlowModelCategoryPropertySetter
    {
        public override void SetProperties(DelftIniCategory temperatureCategory, WaterFlowModel1D model, IList<string> errorMessages)
        {
            if (temperatureCategory == null) return;
            if (!string.Equals(temperatureCategory.Name, ModelDefinitionsRegion.TemperatureValuesHeader,
                StringComparison.OrdinalIgnoreCase)) return;

            foreach (var property in temperatureCategory.Properties)
            {
                if (string.Equals(property.Name, ModelDefinitionsRegion.BackgroundTemperature.Key,
                    StringComparison.OrdinalIgnoreCase))
                {
                    model.BackgroundTemperature = ParseStringToDouble(property, errorMessages);
                }
                else if (string.Equals(property.Name, ModelDefinitionsRegion.SurfaceArea.Key, StringComparison.OrdinalIgnoreCase))
                {
                    model.SurfaceArea = ParseStringToDouble(property, errorMessages);
                }
                else if (string.Equals(property.Name, ModelDefinitionsRegion.AtmosphericPressure.Key,
                    StringComparison.OrdinalIgnoreCase))
                {
                    model.AtmosphericPressure = ParseStringToDouble(property, errorMessages);
                }
                else if (string.Equals(property.Name, ModelDefinitionsRegion.DaltonNumber.Key,
                    StringComparison.OrdinalIgnoreCase))
                {
                    model.DaltonNumber = ParseStringToDouble(property, errorMessages);
                }
                else if (string.Equals(property.Name, ModelDefinitionsRegion.StantonNumber.Key,
                    StringComparison.OrdinalIgnoreCase))
                {
                    model.StantonNumber = ParseStringToDouble(property, errorMessages);
                }
                else if (string.Equals(property.Name, ModelDefinitionsRegion.HeatCapacity.Key,
                    StringComparison.OrdinalIgnoreCase))
                {
                    model.HeatCapacityWater = ParseStringToDouble(property, errorMessages);
                }
                else
                {
                    errorMessages.Add(GetUnsupportedPropertyWarningMessage(property));
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
                errorMessages.Add(string.Format(Resources.WaterFlowModelTemperatureSetter_ParseStringToDouble_Line__0___Parameter___1___will_not_be_imported__Valid_values_are_doubles_only_, property.LineNumber, property.Name));
                return GetDefaultValueForProperty(property.Name);
            }           
        }

        private double GetDefaultValueForProperty(string propertyName)
        {
            if (string.Equals(propertyName, ModelDefinitionsRegion.BackgroundTemperature.Key, StringComparison.OrdinalIgnoreCase))
            {
                return WaterFlowModel1DDataSet.Meteo.valueBackgroundTemperatureDefault;
            }
            if (string.Equals(propertyName, ModelDefinitionsRegion.SurfaceArea.Key, StringComparison.OrdinalIgnoreCase))
            {
                return WaterFlowModel1DDataSet.Meteo.valueSurfaceAreaDefault;
            }
            if (string.Equals(propertyName, ModelDefinitionsRegion.AtmosphericPressure.Key, StringComparison.OrdinalIgnoreCase))
            {
                return WaterFlowModel1DDataSet.Meteo.valueAtmosphericPressureDefault;
            }
            if (string.Equals(propertyName, ModelDefinitionsRegion.DaltonNumber.Key, StringComparison.OrdinalIgnoreCase))
            {
                return WaterFlowModel1DDataSet.Meteo.valueDaltonNumberDefault;
            }
            if (string.Equals(propertyName, ModelDefinitionsRegion.StantonNumber.Key, StringComparison.OrdinalIgnoreCase))
            {
                return WaterFlowModel1DDataSet.Meteo.valueStantonNumberDefault;
            }
            if (string.Equals(propertyName, ModelDefinitionsRegion.HeatCapacity.Key, StringComparison.OrdinalIgnoreCase))
            {
                return WaterFlowModel1DDataSet.Meteo.valueHeatCapacityWaterDefault;
            }
            return 0.0d;
        }
    }
}
