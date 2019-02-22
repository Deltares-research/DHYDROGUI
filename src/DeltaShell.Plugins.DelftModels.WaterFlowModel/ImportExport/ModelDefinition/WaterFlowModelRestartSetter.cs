using System;
using System.Collections.Generic;
using System.Linq;
using DeltaShell.NGHS.IO.Helpers;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport.ModelDefinition
{
    public class WaterFlowModelRestartSetter : WaterFlowModelCategoryPropertySetter
    {
        public override void SetProperties(DelftIniCategory category, WaterFlowModel1D model, IList<string> errorMessages)
        {
            if (category == null) return;
            if (!string.Equals(category.Name, ModelDefinitionsRegion.RestartHeader, StringComparison.OrdinalIgnoreCase)) return;
            
            //Set save state properties equal to imported run times
            model.SaveStateStartTime = model.StopTime;
            model.SaveStateStopTime = model.StopTime;
            model.SaveStateTimeStep = model.TimeStep;

            var containsRestartStartTime =
                category.Properties.Any(p =>
                    string.Equals(p.Name, ModelDefinitionsRegion.RestartStartTime.Key, StringComparison.OrdinalIgnoreCase));
            var containsRestartStopTime =
                category.Properties.Any(p =>
                    string.Equals(p.Name, ModelDefinitionsRegion.RestartStopTime.Key, StringComparison.OrdinalIgnoreCase));
            var containsRestartTimeStep =
                category.Properties.Any(p =>
                    string.Equals(p.Name, ModelDefinitionsRegion.RestartTimeStep.Key, StringComparison.OrdinalIgnoreCase));

            if (containsRestartTimeStep && containsRestartStartTime && containsRestartStopTime)
            {
                foreach (var property in category.Properties)
                {
                    if (string.Equals(property.Name, ModelDefinitionsRegion.RestartStartTime.Key,
                        StringComparison.OrdinalIgnoreCase))
                    {
                        model.SaveStateStartTime = DateTime.Parse(property.Value);
                    }

                    if (string.Equals(property.Name, ModelDefinitionsRegion.RestartStopTime.Key,
                        StringComparison.OrdinalIgnoreCase))
                    {
                        model.SaveStateStopTime = DateTime.Parse(property.Value);
                    }

                    if (string.Equals(property.Name, ModelDefinitionsRegion.RestartTimeStep.Key,
                        StringComparison.OrdinalIgnoreCase))
                    {
                        model.SaveStateTimeStep = TimeSpan.FromSeconds(Convert.ToDouble(property.Value));
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
            
            
            foreach (var property in category.Properties)
            {

                if (string.Equals(property.Name, ModelDefinitionsRegion.WriteRestart.Key, StringComparison.OrdinalIgnoreCase))
                {
                    model.WriteRestart = Convert.ToBoolean(Convert.ToInt32(property.Value));
                }
                else if (string.Equals(property.Name, ModelDefinitionsRegion.UseRestart.Key, StringComparison.OrdinalIgnoreCase))
                {
                    model.UseRestart = Convert.ToBoolean(Convert.ToInt32(property.Value));
                }
                else if (!string.Equals(property.Name, ModelDefinitionsRegion.RestartStartTime.Key,
                             StringComparison.OrdinalIgnoreCase) &&
                         !string.Equals(property.Name, ModelDefinitionsRegion.RestartStopTime.Key,
                             StringComparison.OrdinalIgnoreCase) &&
                         !string.Equals(property.Name, ModelDefinitionsRegion.RestartTimeStep.Key,
                             StringComparison.OrdinalIgnoreCase))
                {
                    errorMessages.Add(GetUnsupportedPropertyWarningMessage(property));
                }
            }
        }
    }
}