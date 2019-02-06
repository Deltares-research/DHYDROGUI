using System;
using System.Collections.Generic;
using System.Linq;
using DeltaShell.NGHS.IO.Helpers;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.Properties;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport.ModelDefinition
{
    public class WaterFlowModelSimulationOptionsSetter : IWaterFlowModelCategoryPropertySetter
    {
        public void SetProperties(DelftIniCategory simulationOptionsCategory, WaterFlowModel1D model, IList<string> errorMessages)
        {
            if (simulationOptionsCategory?.Name != ModelDefinitionsRegion.SimulationOptionsValuesHeader) return;

            //Set save state properties equal to imported run times
            model.SaveStateStartTime = model.StopTime;
            model.SaveStateStopTime = model.StopTime;
            model.SaveStateTimeStep = model.TimeStep;

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
                        Resources.SetProperties_Line__0___Parameter___1___found_in_the_md1d_file__This_parameter_will_not_be_imported__since_it_is_not_supported_by_the_GUI,
                        prop.LineNumber, prop.Name));
                }
            }
        }
    }
}