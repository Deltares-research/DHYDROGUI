using System;
using System.Collections.Generic;
using System.Linq;
using DeltaShell.NGHS.IO.Helpers;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.Properties;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport.ModelDefinition
{
    public class WaterFlowModelNumericalParametersSetter : IWaterFlowModelCategoryPropertySetter
    {
        public void SetProperties(DelftIniCategory numericalParametercategory, WaterFlowModel1D model, IList<string> errorMessages)
        {
            if (numericalParametercategory?.Name != ModelDefinitionsRegion.NumericalParametersValuesHeader) return;

            foreach (var prop in numericalParametercategory.Properties)
            {
                var modelParameter = model.ParameterSettings.FirstOrDefault(ps => ps.Name == prop.Name);
                if (modelParameter == null)
                {
                    errorMessages.Add(string.Format(
                        Resources.SetProperties_Line__0___Parameter___1___found_in_the_md1d_file__This_parameter_will_not_be_imported,
                        prop.LineNumber, prop.Name));
                    continue;
                }

                if (modelParameter.Type == "typeof(bool)")
                {
                    modelParameter.Value = Convert.ToString(Convert.ToBoolean(Convert.ToInt32(prop.Value)));
                }
                else
                {
                    modelParameter.Value = prop.Value;
                }
            }
        }
    }
}