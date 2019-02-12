using DeltaShell.NGHS.IO.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport.ModelDefinition
{
    /// <summary>
    /// WaterFlowModelAdvancedOptionsSetter sets property values described in the Advanced Options DelftIniCategory on the WaterFlowModel1D.
    /// </summary>
    /// <seealso cref="WaterFlowModelCategoryPropertySetter" />
    public class WaterFlowModelAdvancedOptionsSetter : WaterFlowModelCategoryPropertySetter
    {
        /// <summary>
        /// Sets the advanced option settings of <paramref name="model"/> as described in
        /// the AdvancedOptions DelftIniCategory.
        /// </summary>
        ///  <param name="category">The Advanced Options DelftIniCategory.</param>
        ///  <param name="model">The WaterFlow1D model.</param>
        ///  <param name="errorMessages"> A collection of error messages that can be added to in case errors occur in this method. </param>
        /// <remarks>
        ///  Pre-condition: category != null && model != null
        ///  If category.Name != AdvancedOptions then nothing happens
        ///  </remarks>
        public override void SetProperties(DelftIniCategory category, WaterFlowModel1D model, IList<string> errorMessages)
        {
            if (category?.Name != ModelDefinitionsRegion.AdvancedOptionsHeader) return;

            foreach (var property in category.Properties)
            {
                var modelParameter = model.ParameterSettings.FirstOrDefault(ps => ps.Name == property.Name);

                if (modelParameter != null)
                {
                    modelParameter.Value = modelParameter.Type == "typeof(bool)" 
                        ? Convert.ToString(Convert.ToBoolean(Convert.ToInt32(property.Value))) 
                        : property.Value;
                }
                else if (property.Name == ModelDefinitionsRegion.CalculateDelwaqOutput.Key)
                {
                    model.HydFileOutput = property.Value == "1";
                }
                else if (property.Name == ModelDefinitionsRegion.Latitude.Key)
                {
                    model.Latitude = double.Parse(property.Value, System.Globalization.CultureInfo.InvariantCulture);
                }
                else if (property.Name == ModelDefinitionsRegion.Longitude.Key)
                {
                    model.Longitude = double.Parse(property.Value, System.Globalization.CultureInfo.InvariantCulture);
                }
                else
                {
                    errorMessages.Add(GetUnsupportedPropertyWarningMessage(property));
                }
            }
        }
    }
}
