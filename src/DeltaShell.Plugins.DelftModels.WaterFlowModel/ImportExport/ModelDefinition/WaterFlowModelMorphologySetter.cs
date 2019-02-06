using DeltaShell.NGHS.IO.Helpers;
using System;
using System.Collections.Generic;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.Properties;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport.ModelDefinition
{
    /// <summary>
    /// WaterFlowModelMorphologySetter sets property values described in the Morphology DelftIniCategory on the WaterFlowModel1D.
    /// </summary>
    /// <seealso cref="DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport.ModelDefinition.IWaterFlowModelCategoryPropertySetter" />
    public class WaterFlowModelMorphologySetter : IWaterFlowModelCategoryPropertySetter
    {
        public void SetProperties(DelftIniCategory morphologyCategory, WaterFlowModel1D model, IList<string> errorMessages)
        {
            if (morphologyCategory?.Name != ModelDefinitionsRegion.MorphologyValuesHeader) return;

            foreach (var property in morphologyCategory.Properties)
            {
                if (property.Name == ModelDefinitionsRegion.CalculateMorphology.Key)
                {
                    model.UseMorphology = ParseValueToBool(property, errorMessages);
                }
                else if (property.Name == ModelDefinitionsRegion.AdditionalOutput.Key)
                {
                    model.AdditionalMorphologyOutput = ParseValueToBool(property, errorMessages);
                }
                else if (property.Name == ModelDefinitionsRegion.MorphologyInputFile.Key)
                {
                    model.MorphologyPath = property.Value;
                }
                else if (property.Name == ModelDefinitionsRegion.SedimentInputFile.Key)
                {
                    model.SedimentPath = property.Value;
                }
                else
                {
                    errorMessages.Add(
                        string.Format(
                            Resources.SetProperties_Line__0___Parameter___1___found_in_the_md1d_file__This_parameter_will_not_be_imported__since_it_is_not_supported_by_the_GUI,
                            property.LineNumber, property.Name));
                }
            }
        }

        private bool ParseValueToBool(DelftIniProperty property, IList<string> errorMessages)
        {
            try
            {
                return Convert.ToBoolean(Convert.ToInt32(property.Value));
            }
            catch (Exception)
            {
                errorMessages.Add($"Line {property.LineNumber}: Parameter '{property.Name}' will not be imported. Valid values are '0' (false) or '1' (true).");
                return false;
            }
        }
    }
}
