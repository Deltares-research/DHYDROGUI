using System;
using System.Collections.Generic;
using System.Linq;
using DeltaShell.NGHS.IO.Helpers;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport.ModelDefinition
{
    public class WaterFlowModelSimulationOptionsSetter : IWaterFlowModelCategoryPropertySetter
    {
        public void SetProperties(DelftIniCategory simulationOptionsCategory, WaterFlowModel1D model,
            Action<string, IList<string>> createAndAddErrorReport)
        {
            if (simulationOptionsCategory?.Name != ModelDefinitionsRegion.SimulationOptionsValuesHeader) return;

            var errorMessages = new List<string>();

            foreach (var prop in simulationOptionsCategory.Properties)
            {
                var modelParameter = model.ParameterSettings.FirstOrDefault(ps => ps.Name == prop.Name);

                if (modelParameter != null)
                {
                    modelParameter.Value = prop.Value;
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
                        "Parameter {0} found in the md1d file. This parameter will not be imported, since it is not supported by the GUI",
                        prop.Name));
                }
            }

            if (errorMessages.Count > 0)
            {
                createAndAddErrorReport?.Invoke(
                    "An error occurred during reading the simulation options of the md1d file:", errorMessages);
            }
        }
    }
}