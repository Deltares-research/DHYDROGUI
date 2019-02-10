using System.Collections.Generic;
using System.Globalization;
using DeltaShell.NGHS.IO.Helpers;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport.ModelDefinition
{
    public class WaterFlowModelSpecialsPropertiesSetter : WaterFlowModelCategoryPropertySetter
    {
        public override void SetProperties(DelftIniCategory category, WaterFlowModel1D model, IList<string> errorMessages)
        {
            foreach (var property in category.Properties)
            {
                if (property.Name == ModelDefinitionsRegion.DesignFactorDlg.Key)
                {
                    model.DesignFactorDlg = double.Parse(property.Value, CultureInfo.InvariantCulture);
                }
                else
                {
                    errorMessages.Add(GetUnsupportedPropertyWarningMessage(property));
                }
            }
        }
    }
}
