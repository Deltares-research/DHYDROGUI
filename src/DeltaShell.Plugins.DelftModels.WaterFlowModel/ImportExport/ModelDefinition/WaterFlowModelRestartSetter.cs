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
            if (!ValueEqualsDefinition(category.Name, ModelDefinitionsRegion.RestartHeader)) return;
            
            //Set save state properties equal to imported run times
            model.SaveStateStartTime = model.StopTime;
            model.SaveStateStopTime = model.StopTime;
            model.SaveStateTimeStep = model.TimeStep;

            var containsRestartStartTime =
                category.Properties.Any(p =>
                    ValueEqualsDefinition(p.Name, ModelDefinitionsRegion.RestartStartTime.Key));
            var containsRestartStopTime =
                category.Properties.Any(p =>
                    ValueEqualsDefinition(p.Name, ModelDefinitionsRegion.RestartStopTime.Key));
            var containsRestartTimeStep =
                category.Properties.Any(p =>
                    ValueEqualsDefinition(p.Name, ModelDefinitionsRegion.RestartTimeStep.Key));

            if (containsRestartTimeStep && containsRestartStartTime && containsRestartStopTime)
            {
                foreach (var property in category.Properties)
                {
                    if (ValueEqualsDefinition(property.Name, ModelDefinitionsRegion.RestartStartTime.Key))
                    {
                        model.SaveStateStartTime = DateTime.Parse(property.Value);
                    }

                    if (ValueEqualsDefinition(property.Name, ModelDefinitionsRegion.RestartStopTime.Key))
                    {
                        model.SaveStateStopTime = DateTime.Parse(property.Value);
                    }

                    if (ValueEqualsDefinition(property.Name, ModelDefinitionsRegion.RestartTimeStep.Key))
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

                if (ValueEqualsDefinition(property.Name, ModelDefinitionsRegion.WriteRestart.Key))
                {
                    model.WriteRestart = Convert.ToBoolean(Convert.ToInt32(property.Value));
                }
                else if (ValueEqualsDefinition(property.Name, ModelDefinitionsRegion.UseRestart.Key))
                {
                    model.UseRestart = Convert.ToBoolean(Convert.ToInt32(property.Value));
                }
                else if (!ValueEqualsDefinition(property.Name, ModelDefinitionsRegion.RestartStartTime.Key) &&
                         !ValueEqualsDefinition(property.Name, ModelDefinitionsRegion.RestartStopTime.Key) &&
                         !ValueEqualsDefinition(property.Name, ModelDefinitionsRegion.RestartTimeStep.Key))
                {
                    errorMessages.Add(GetUnsupportedPropertyWarningMessage(property));
                }
            }
        }
    }
}