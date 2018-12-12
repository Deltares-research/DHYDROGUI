using DeltaShell.NGHS.IO.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;

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
                    errorMessages.Add($"Line {prop.LineNumber}: Parameter '{prop.Name}' found in the md1d file. This parameter will not be imported, since it is not supported by the GUI");
                }
            }
        }
    }
}
