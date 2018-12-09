using System;
using System.Collections.Generic;
using System.Linq;
using DeltaShell.NGHS.IO.Helpers;

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
                        "Line {0}: Parameter {1} found in the md1d file. This parameter will not be imported, since it is not supported by the GUI",
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