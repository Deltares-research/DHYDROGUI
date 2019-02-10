using System;
using System.Collections.Generic;
using System.Linq;
using DeltaShell.NGHS.IO.Helpers;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport.ModelDefinition
{
    public class WaterFlowModelNumericalParametersSetter : WaterFlowModelCategoryPropertySetter
    {
        public override void SetProperties(DelftIniCategory numericalParametercategory, WaterFlowModel1D model, IList<string> errorMessages)
        {
            if (numericalParametercategory?.Name != ModelDefinitionsRegion.NumericalParametersValuesHeader) return;

            foreach (var property in numericalParametercategory.Properties)
            {
                var modelParameter = model.ParameterSettings.FirstOrDefault(ps => ps.Name == property.Name);
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