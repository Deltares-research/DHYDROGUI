using System.Collections.Generic;
using System.Globalization;
using DeltaShell.NGHS.IO.Helpers;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport.ModelDefinition
{
    public class WaterFlowModelSpecialsPropertiesSetter : IWaterFlowModelCategoryPropertySetter
    {
        public void SetProperties(DelftIniCategory category, WaterFlowModel1D model, IList<string> errorMessages)
        {
            var useDesignFactorDlg = category.ReadProperty<string>(ModelDefinitionsRegion.DesignFactorDlg.Key, true);

            if (useDesignFactorDlg != null) model.DesignFactorDlg = double.Parse(useDesignFactorDlg, CultureInfo.InvariantCulture);
        }
    }
}
