using System;
using System.Collections.Generic;
using System.Linq;
using DeltaShell.NGHS.IO.Helpers;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport.ModelDefinition
{
    public class WaterFlowModelSalinitySetter : WaterFlowModelCategoryPropertySetter
    {
        public override void SetProperties(DelftIniCategory category, WaterFlowModel1D model, IList<string> errorMessages)
        {
            if (category == null) return;
            if (!string.Equals(category.Name, ModelDefinitionsRegion.SalinityValuesHeader, StringComparison.OrdinalIgnoreCase)) return;
            
            foreach (var property in category.Properties)
            {
                try
                {
                    if (string.Equals(property.Name, ModelDefinitionsRegion.SaltComputation.Key,
                        StringComparison.OrdinalIgnoreCase))
                    {
                        model.UseSaltInCalculation = Convert.ToBoolean(Convert.ToInt32(property.Value));
                    }
                    else if (string.Equals(property.Name, ModelDefinitionsRegion.DiffusionAtBoundaries.Key,
                        StringComparison.OrdinalIgnoreCase))
                    {
                        var diffusionAtBoundariesParameterSetting = model.ParameterSettings.FirstOrDefault(
                            ps => string.Equals(ps.Name, ModelDefinitionsRegion.DiffusionAtBoundaries.Key,
                                StringComparison.OrdinalIgnoreCase));
                        if (diffusionAtBoundariesParameterSetting != null)
                        {
                            diffusionAtBoundariesParameterSetting.Value =
                                Convert.ToString(Convert.ToBoolean(Convert.ToInt32(property.Value)));
                        }
                    }
                    else
                    {
                        errorMessages.Add(GetUnsupportedPropertyWarningMessage(property));
                    }
                }
                catch (Exception e)
                {
                    errorMessages.Add(string.Format("Line {0}: {1}",
                        property.LineNumber, e.Message));
                }
            }
        }
    }
}