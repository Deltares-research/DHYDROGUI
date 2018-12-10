using System.Collections.Generic;
using System.Linq;
using DeltaShell.NGHS.IO.Helpers;

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
                    errorMessages.Add($"Line {property.LineNumber}: Parameter {property.Name} found in the md1d file. This parameter will not be imported, since it is not supported by the GUI");
                    continue;
                }

                modelParameter.Value = property.Value;
            }
        }
    }
}