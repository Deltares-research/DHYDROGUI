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

            foreach (var prop in observationsParameterCategory.Properties)
            {
                var modelParameter = model.ParameterSettings.FirstOrDefault(ps => ps.Name == prop.Name);
                if (modelParameter == null)
                {
                    errorMessages.Add($"Line {prop.LineNumber}: Parameter {prop.Name} found in the md1d file. This parameter will not be imported, since it is not supported by the GUI");
                    continue;
                }

                modelParameter.Value = prop.Value;
            }
        }
    }
}