using System;
using System.Collections.Generic;
using System.Linq;
using DeltaShell.NGHS.IO.Helpers;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport.ModelDefinition
{
    public class WaterFlowModelSimulationOptionsSetter : WaterFlowModelCategoryPropertySetter
    {
        public override void SetProperties(DelftIniCategory simulationOptionsCategory, WaterFlowModel1D model, IList<string> errorMessages)
        {
            if (simulationOptionsCategory == null) return;
            if (!ValueEqualsDefinition(simulationOptionsCategory.Name, ModelDefinitionsRegion.SimulationOptionsValuesHeader)) return;

            //Set save state properties equal to imported run times
            model.SaveStateStartTime = model.StopTime;
            model.SaveStateStopTime = model.StopTime;
            model.SaveStateTimeStep = model.TimeStep;

            foreach (var property in simulationOptionsCategory.Properties)
            {
                var modelParameter = model.ParameterSettings.FirstOrDefault(ps =>
                    ValueEqualsDefinition(ps.Name, property.Name));

                if (modelParameter != null)
                {
                    if (modelParameter.Type == "typeof(bool)")
                    {
                        modelParameter.Value = Convert.ToString(Convert.ToBoolean(Convert.ToInt32(property.Value)));
                    }
                    else
                    {
                        modelParameter.Value = property.Value;
                    }
                }
                else if (ValueEqualsDefinition(property.Name, ModelDefinitionsRegion.UseRestart.Key))
                {
                    model.UseRestart = Convert.ToBoolean(Convert.ToInt32(property.Value));
                }
                else if (ValueEqualsDefinition(property.Name, ModelDefinitionsRegion.WriteRestart.Key))
                {
                    model.WriteRestart = Convert.ToBoolean(Convert.ToInt32(property.Value));
                }
                //Do nothing with "WriteNetCDF", always true in GUI.
                else if (!ValueEqualsDefinition(property.Name, ModelDefinitionsRegion.WriteNetCDF.Key))
                {
                    errorMessages.Add(GetUnsupportedPropertyWarningMessage(property));
                }
            }
        }
    }
}