using System;
using System.Collections.Generic;
using System.Linq;
using DeltaShell.NGHS.IO.Helpers;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport.ModelDefinition
{
    public class WaterFlowModelNumericalParametersSetter : WaterFlowModelCategoryPropertySetter
    {
        public override void SetProperties(DelftIniCategory numericalParameterCategory, WaterFlowModel1D model, IList<string> errorMessages)
        {
            if (numericalParameterCategory == null) return;
            if (!string.Equals(numericalParameterCategory.Name, ModelDefinitionsRegion.NumericalParametersValuesHeader,
                StringComparison.OrdinalIgnoreCase)) return;

            foreach (var property in numericalParameterCategory.Properties)
            {
                var modelParameter = model.ParameterSettings.FirstOrDefault(ps =>
                    string.Equals(ps.Name, property.Name, StringComparison.OrdinalIgnoreCase));
                if (modelParameter == null)
                {
                    errorMessages.Add(GetUnsupportedPropertyWarningMessage(property));
                    continue;
                }

                if (modelParameter.Type == "typeof(bool)")
                {
                    modelParameter.Value = Convert.ToString(Convert.ToBoolean(Convert.ToInt32(property.Value)));
                }
                else
                {
                    modelParameter.Value = property.Value;
                }
            }
        }
    }
}