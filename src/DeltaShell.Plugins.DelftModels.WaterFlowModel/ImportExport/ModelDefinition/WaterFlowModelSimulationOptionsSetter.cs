using System;
using System.Collections.Generic;
using System.Linq;
using DeltaShell.NGHS.IO.Helpers;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport.ModelDefinition
{
    public class WaterFlowModelSimulationOptionsSetter : IWaterFlowModelCategoryPropertySetter
    {
        public void SetProperties(DelftIniCategory simulationOptionsCategory, WaterFlowModel1D model, IList<string> errorMessages)
        {
            if (simulationOptionsCategory?.Name != ModelDefinitionsRegion.SimulationOptionsValuesHeader) return;
            
            foreach (var prop in simulationOptionsCategory.Properties)
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
                else if (prop.Name == ModelDefinitionsRegion.UseRestart.Key)
                {
                    model.UseRestart = Convert.ToBoolean(Convert.ToInt32(prop.Value));
                }
                else if (prop.Name == ModelDefinitionsRegion.WriteRestart.Key)
                {
                    model.WriteRestart = Convert.ToBoolean(Convert.ToInt32(prop.Value));
                }
                //Do nothing with "WriteNetCDF", always true in GUI.
                else if (prop.Name != ModelDefinitionsRegion.WriteNetCDF.Key)
                {
                    errorMessages.Add(string.Format(
                        "Line {0}: Parameter {1} found. This parameter will not be imported, since it is not supported by the GUI",
                        prop.LineNumber, prop.Name));
                }
            }
        }
    }
}