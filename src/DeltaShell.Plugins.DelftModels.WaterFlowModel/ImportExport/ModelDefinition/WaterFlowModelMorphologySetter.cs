using DeltaShell.NGHS.IO.Helpers;
using System;
using System.Collections.Generic;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.Properties;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport.ModelDefinition
{
    /// <inheritdoc />
    /// <summary>
    /// WaterFlowModelMorphologySetter sets property values described in the Morphology DelftIniCategory on the WaterFlowModel1D.
    /// </summary>
    /// <seealso cref="T:DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport.ModelDefinition.WaterFlowModelCategoryPropertySetter" />
    public class WaterFlowModelMorphologySetter : WaterFlowModelCategoryPropertySetter
    {
        public override void SetProperties(DelftIniCategory morphologyCategory, WaterFlowModel1D model, IList<string> errorMessages)
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
                    errorMessages.Add(GetUnsupportedPropertyWarningMessage(property));
                }
            }
        }

        private static bool ParseValueToBool(IDelftIniProperty property, ICollection<string> errorMessages)
        {
            try
            {
                return Convert.ToBoolean(Convert.ToInt32(property.Value));
            }
            catch (Exception)
            {
                errorMessages.Add(
                    string.Format(Resources.WaterFlowModelMorphologySetter_ParseValueToBool_Line__0___Parameter___1___will_not_be_imported__Valid_values_are__0___false__or__1___true__,
                    property.LineNumber, property.Name));
                return false;
            }
        }
    }
}
