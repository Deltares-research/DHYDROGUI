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

            foreach (var property in initialConditionsParameterCategory.Properties)
            {
                var modelParameter = model.ParameterSettings.FirstOrDefault(ps => ps.Name == property.Name);
                if (modelParameter == null)
                {
                    errorMessages.Add($"Line {property.LineNumber}: Parameter {property.Name} found in the md1d file. This parameter will not be imported, since it is not supported by the GUI");
                    continue;
                }

                modelParameter.Value = Convert.ToString(Convert.ToBoolean(Convert.ToInt32(property.Value)));
            }
        }
    }
}
