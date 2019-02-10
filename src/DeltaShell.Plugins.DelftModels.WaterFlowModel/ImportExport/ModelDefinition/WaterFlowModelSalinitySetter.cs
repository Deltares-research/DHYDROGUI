using System;
using System.Collections.Generic;
using System.Linq;
using DeltaShell.NGHS.IO.Helpers;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.Properties;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport.ModelDefinition
{
    public class WaterFlowModelSalinitySetter : IWaterFlowModelCategoryPropertySetter
    {
        public void SetProperties(DelftIniCategory category, WaterFlowModel1D model, IList<string> errorMessages)
        {
            if (category?.Name != ModelDefinitionsRegion.SalinityValuesHeader) return;


            foreach (var prop in category.Properties)
            {
                try
                {
                    if (prop.Name == ModelDefinitionsRegion.SaltComputation.Key)
                    {
                        model.UseSaltInCalculation = Convert.ToBoolean(Convert.ToInt32(prop.Value));
                    }
                    else if (prop.Name == ModelDefinitionsRegion.DiffusionAtBoundaries.Key)
                    {
                        var diffusionAtBoundariesParameterSetting = model.ParameterSettings.FirstOrDefault(
                            ps => ps.Name == ModelDefinitionsRegion.DiffusionAtBoundaries.Key);
                        if (diffusionAtBoundariesParameterSetting != null)
                        {
                            diffusionAtBoundariesParameterSetting.Value =
                                Convert.ToString(Convert.ToBoolean(Convert.ToInt32(prop.Value)));
                        }
                    }
                    else
                    {
                        errorMessages.Add(string.Format(Resources.SetProperties_Line__0___Parameter___1___found_in_the_md1d_file__This_parameter_will_not_be_imported,
                            prop.LineNumber, prop.Name));
                    }
                }
                catch (Exception e)
                {
                    errorMessages.Add(string.Format("Line {0}: {1}",
                        prop.LineNumber, e.Message));
                }
            }
        }
    }
}