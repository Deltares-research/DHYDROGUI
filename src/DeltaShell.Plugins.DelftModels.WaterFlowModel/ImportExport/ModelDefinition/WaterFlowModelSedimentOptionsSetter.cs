using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using DeltaShell.NGHS.IO.Helpers;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.Properties;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport.ModelDefinition
{
    public class WaterFlowModelSedimentOptionsSetter : IWaterFlowModelCategoryPropertySetter
    {
        /// <summary>
        /// Sets the properties of the Sediment DelftIniCategory.
        /// </summary>
        /// <param name="sedimentParameterCategory">The sediment parameter category.</param>
        /// <param name="model">The model.</param>
        /// <param name="errorMessages">The error messages.</param>
        public void SetProperties(DelftIniCategory sedimentParameterCategory, WaterFlowModel1D model, IList<string> errorMessages)
        {
            if (sedimentParameterCategory?.Name != ModelDefinitionsRegion.SedimentValuesHeader) return;

            foreach (var property in sedimentParameterCategory.Properties)
            {
                var modelParameter = model.ParameterSettings.FirstOrDefault(ps => ps.Name == property.Name);
                if (modelParameter == null)
                {
                    errorMessages.Add(string.Format(
                        Resources.SetProperties_Line__0___Parameter___1___found_in_the_md1d_file__This_parameter_will_not_be_imported__since_it_is_not_supported_by_the_GUI,
                        property.LineNumber, property.Name));
                    continue;
                }

                switch (property.Name)
                {
                    case "D50":
                        bool d50ParseSuccess = ParseProperty(property, out var d50);
                        if (d50ParseSuccess)
                        {
                            model.D50 = d50;
                        }
                        break;
                    case "D90":
                        bool d90ParseSuccess = ParseProperty(property, out var d90);
                        if (d90ParseSuccess)
                        {
                            model.D90 = d90;
                        }
                        break;
                    case "DepthUsedForSediment":
                        bool depthUsedForSedimentParseSuccess = ParseProperty(property, out var depthUsedForSediment);
                        if (depthUsedForSedimentParseSuccess)
                        {
                            model.DepthUsedForSediment = depthUsedForSediment;
                        }
                        break;
                }
            }
        }

        private static bool ParseProperty(DelftIniProperty property, out double parsedValue)
        {
            return double.TryParse(property.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out parsedValue);
        }
    }
}
