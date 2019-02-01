using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using DeltaShell.NGHS.IO.Helpers;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport.ModelDefinition
{
    public class WaterFlowModelSedimentOptionsSetter : IWaterFlowModelCategoryPropertySetter
    {
        public void SetProperties(DelftIniCategory sedimentParameterCategory, WaterFlowModel1D model, IList<string> errorMessages)
        {
            if (sedimentParameterCategory?.Name != ModelDefinitionsRegion.SedimentValuesHeader) return;

            foreach (var property in sedimentParameterCategory.Properties)
            {
                var modelParameter = model.ParameterSettings.FirstOrDefault(ps => ps.Name == property.Name);
                if (modelParameter == null)
                {
                    errorMessages.Add($"Line {property.LineNumber}: Parameter {property.Name} found in the md1d file. This parameter will not be imported, since it is not supported by the GUI");
                    continue;
                }

                switch (property.Name)
                {
                    case "D50":
                        model.D50 = double.Parse(property.Value, CultureInfo.InvariantCulture);
                        break;
                    case "D90":
                        model.D90 = double.Parse(property.Value, CultureInfo.InvariantCulture);
                        break;
                    case "DepthUsedForSediment":
                        model.DepthUsedForSediment = double.Parse(property.Value, CultureInfo.InvariantCulture);
                        break;
                }
            }
        }
    }
}
