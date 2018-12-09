using System;
using System.Collections.Generic;
using System.Linq;
using DeltaShell.NGHS.IO.Helpers;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport.ModelDefinition
{
    public class WaterFlowModelInitialConditionsParameterSetter : IWaterFlowModelCategoryPropertySetter
    {
        public void SetProperties(DelftIniCategory initialConditionsParameterCategory, WaterFlowModel1D model, IList<string> errorMessages)
        {
            if (initialConditionsParameterCategory?.Name != ModelDefinitionsRegion.InitialConditionsValuesHeader) return;

            foreach (var prop in initialConditionsParameterCategory.Properties)
            {
                var modelParameter = model.ParameterSettings.FirstOrDefault(ps => ps.Name == prop.Name);
                if (modelParameter == null)
                {
                    errorMessages.Add($"Line {prop.LineNumber}: Parameter {prop.Name} found in the md1d file. This parameter will not be imported, since it is not supported by the GUI");
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
