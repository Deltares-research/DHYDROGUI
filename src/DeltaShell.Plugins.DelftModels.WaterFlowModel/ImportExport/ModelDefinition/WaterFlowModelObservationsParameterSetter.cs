using System.Collections.Generic;
using System.Linq;
using DeltaShell.NGHS.IO.Helpers;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.Properties;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport.ModelDefinition
{
    public class WaterFlowModelObservationsParameterSetter : IWaterFlowModelCategoryPropertySetter
    {
        public void SetProperties(DelftIniCategory observationsParameterCategory, WaterFlowModel1D model, IList<string> errorMessages)
        {
            if (observationsParameterCategory?.Name != ModelDefinitionsRegion.ObservationsHeader) return;

            foreach (var property in observationsParameterCategory.Properties)
            {
                var modelParameter = model.ParameterSettings.FirstOrDefault(ps => ps.Name == property.Name);
                if (modelParameter == null)
                {
                    errorMessages.Add(string.Format(
                        Resources.SetProperties_Line__0___Parameter___1___found_in_the_md1d_file__This_parameter_will_not_be_imported__since_it_is_not_supported_by_the_GUI,
                        property.LineNumber, property.Name));
                    continue;
                }

                modelParameter.Value = property.Value;
            }
        }
    }
}