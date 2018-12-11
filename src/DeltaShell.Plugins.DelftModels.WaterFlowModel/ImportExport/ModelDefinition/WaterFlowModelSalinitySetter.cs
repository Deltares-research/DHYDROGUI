using System;
using System.Collections.Generic;
using System.Linq;
using DeltaShell.NGHS.IO.Helpers;

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
                        errorMessages.Add(string.Format("Line {0}: Unknown property {1} found in salinity category",
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