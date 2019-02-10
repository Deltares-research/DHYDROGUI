using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using DeltaShell.NGHS.IO.Helpers;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport.ModelDefinition
{
    public class WaterFlowModelSedimentOptionsSetter : WaterFlowModelCategoryPropertySetter
    {
        /// <summary>
        /// Sets the properties of the Sediment DelftIniCategory.
        /// </summary>
        /// <param name="sedimentParameterCategory">The sediment parameter category.</param>
        /// <param name="model">The model.</param>
        /// <param name="errorMessages">The error messages.</param>
        public override void SetProperties(DelftIniCategory sedimentParameterCategory, WaterFlowModel1D model, IList<string> errorMessages)
        {
            if (sedimentParameterCategory?.Name != ModelDefinitionsRegion.SedimentValuesHeader) return;

            foreach (var property in sedimentParameterCategory.Properties)
            {
                var modelParameter = model.ParameterSettings.FirstOrDefault(ps => ps.Name == property.Name);
                if (modelParameter == null)
                {
                    errorMessages.Add(GetUnsupportedPropertyWarningMessage(property));
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
