using System;
using System.Collections.Generic;
using System.Linq;
using DeltaShell.NGHS.IO.Helpers;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport.ModelDefinition
{
    public class WaterFlowModelRestartSetter : IWaterFlowModelCategoryPropertySetter
    {
        public void SetProperties(DelftIniCategory category, WaterFlowModel1D model, IList<string> errorMessages)
        {
            if (category?.Name != ModelDefinitionsRegion.RestartHeader) return;
            
            //Set save state properties equal to imported run times
            model.SaveStateStartTime = model.StopTime;
            model.SaveStateStopTime = model.StopTime;
            model.SaveStateTimeStep = model.TimeStep;

            var containsRestartStartTime =
                category.Properties.Any(p => p.Name == ModelDefinitionsRegion.RestartStartTime.Key);
            var containsRestartStopTime =
                category.Properties.Any(p => p.Name == ModelDefinitionsRegion.RestartStopTime.Key);
            var containsRestartTimeStep =
                category.Properties.Any(p => p.Name == ModelDefinitionsRegion.RestartTimeStep.Key);

            if (containsRestartTimeStep && containsRestartStartTime && containsRestartStopTime)
            {
                foreach (var prop in category.Properties)
                {
                    if (prop.Name == ModelDefinitionsRegion.RestartStartTime.Key)
                    {
                        model.SaveStateStartTime = DateTime.Parse(prop.Value);
                    }

                    if (prop.Name == ModelDefinitionsRegion.RestartStopTime.Key)
                    {
                        model.SaveStateStopTime = DateTime.Parse(prop.Value);
                    }

                    if (prop.Name == ModelDefinitionsRegion.RestartTimeStep.Key)
                    {
                        model.SaveStateTimeStep = TimeSpan.FromSeconds(Convert.ToDouble(prop.Value));
                    }
                }
            }
            else
            {
                if (containsRestartTimeStep || containsRestartStartTime || containsRestartStopTime)
                {
                    errorMessages.Add(string.Format("Line {0}: Information about the Save State is not complete and therefore ignored during import", category.LineNumber));
                }
            }
            
            
            foreach (var prop in category.Properties)
            {

                if (prop.Name == ModelDefinitionsRegion.WriteRestart.Key)
                {
                    model.WriteRestart = Convert.ToBoolean(Convert.ToInt32(prop.Value));
                }
                else if (prop.Name == ModelDefinitionsRegion.UseRestart.Key)
                {
                    model.UseRestart = Convert.ToBoolean(Convert.ToInt32(prop.Value));
                }
                else if (prop.Name != ModelDefinitionsRegion.RestartStartTime.Key &&
                         prop.Name != ModelDefinitionsRegion.RestartStopTime.Key &&
                         prop.Name != ModelDefinitionsRegion.RestartTimeStep.Key)
                {
                    errorMessages.Add(string.Format(
                        "Line {0}: Parameter {1} found. This parameter will not be imported, since it is not supported by the GUI", prop.LineNumber,
                        prop.Name));
                }
            }
        }
    }
}