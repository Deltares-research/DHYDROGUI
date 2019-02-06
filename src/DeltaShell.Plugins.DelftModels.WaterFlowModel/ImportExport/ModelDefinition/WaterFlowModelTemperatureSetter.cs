using DeltaShell.NGHS.IO.Helpers;
using System;
using System.Collections.Generic;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.Properties;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport.ModelDefinition
{
    /// <summary>
    /// WaterFlowModelTemperatureSetter sets property values described in the Temperature DelftIniCategory on the WaterFlowModel1D.
    /// </summary>
    /// <seealso cref="DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport.ModelDefinition.IWaterFlowModelCategoryPropertySetter" />
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
                        string.Format(Resources.SetProperties_Line__0___Parameter___1___found_in_the_md1d_file__This_parameter_will_not_be_imported__since_it_is_not_supported_by_the_GUI, property.LineNumber, property.Name));
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
