using System;
using System.Collections.Generic;
using System.Linq;
using DeltaShell.NGHS.IO.Helpers;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport.ModelDefinition
{
    public class WaterFlowModelInitialConditionsParameterSetter : IWaterFlowModelCategoryPropertySetter
    {
        public void SetProperties(DelftIniCategory initialConditionsParameterCategory, WaterFlowModel1D model, Action<string, IList<string>> createAndAddErrorReport)
        {
            if (initialConditionsParameterCategory?.Name != ModelDefinitionsRegion.InitialConditionsValuesHeader) return;

            var errorMessages = new List<string>();

            foreach (var prop in initialConditionsParameterCategory.Properties)
            {
                var modelParameter = model.ParameterSettings.FirstOrDefault(ps => ps.Name == prop.Name);
                if (modelParameter == null)
                {
                    errorMessages.Add($"Parameter {prop.Name} found in the md1d file. This parameter will not be imported, since it is not supported by the GUI");
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

            if (errorMessages.Count > 0)
            {
                createAndAddErrorReport?.Invoke(
                    "An error occurred during reading the initial conditions of the md1d file:", errorMessages);
            }
        }
    }
}
