using DeltaShell.NGHS.IO.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.Properties;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport.ModelDefinition
{
    /// <summary>
    /// WaterFlowModelAdvancedOptionsSetter sets property values described in the Advanced Options DelftIniCategory on the WaterFlowModel1D.
    /// </summary>
    /// <seealso cref="DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport.ModelDefinition.IWaterFlowModelCategoryPropertySetter" />
    public class WaterFlowModelAdvancedOptionsSetter : IWaterFlowModelCategoryPropertySetter
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
        public void SetProperties(DelftIniCategory advancedOptionsCategory, WaterFlowModel1D model, IList<string> errorMessages)
        {
            if (advancedOptionsCategory?.Name != ModelDefinitionsRegion.AdvancedOptionsHeader) return;

            foreach (var prop in advancedOptionsCategory.Properties)
            {
                var modelParameter = model.ParameterSettings.FirstOrDefault(ps => ps.Name == prop.Name);

                if (modelParameter != null)
                {
                    if (modelParameter.Type == "typeof(bool)")
                    {
                        modelParameter.Value = Convert.ToString(Convert.ToBoolean(Convert.ToInt32(prop.Value)));
                    }
                    else
                    {
                        modelParameter.Value = prop.Value;
                    }
                }
                else if (prop.Name == ModelDefinitionsRegion.CalculateDelwaqOutput.Key)
                {
                    model.HydFileOutput = prop.Value == "1";
                }
                else if (prop.Name == ModelDefinitionsRegion.Latitude.Key)
                {
                    model.Latitude = double.Parse(prop.Value, System.Globalization.CultureInfo.InvariantCulture);
                }
                else if (prop.Name == ModelDefinitionsRegion.Longitude.Key)
                {
                    model.Longitude = double.Parse(prop.Value, System.Globalization.CultureInfo.InvariantCulture);
                }
                else
                {
                    errorMessages.Add(string.Format(
                        Resources.SetProperties_Line__0___Parameter___1___found_in_the_md1d_file__This_parameter_will_not_be_imported__since_it_is_not_supported_by_the_GUI,
                        prop.LineNumber, prop.Name));
                }
            }
        }
    }
}
