using System;
using System.Collections.Generic;
using DeltaShell.NGHS.IO.Helpers;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport.ModelDefinition
{
    public class WaterFlowModelSpecialsPropertiesSetter : IWaterFlowModelCategoryPropertySetter
    {
        public void SetProperties(DelftIniCategory category, WaterFlowModel1D model, IList<string> errorMessages)
        {
            var useDesignFactorDlg = category.ReadProperty<string>(ModelDefinitionsRegion.DesignFactorDlg.Key, true);
            if(useDesignFactorDlg != null) model.DesignFactorDlg = ParseStringToDouble(useDesignFactorDlg);
        }

        private static double ParseStringToDouble(string value)
        {
            try
            {
                return double.Parse(value, System.Globalization.CultureInfo.InvariantCulture);
            }
            catch (Exception)
            {
              //errorMessages.Add(string.Format(Resources.WaterFlowModelTemperatureSetter_ParseStringToDouble_Line__0___Parameter___1___will_not_be_imported__Valid_values_are_doubles_only_, value.LineNumber, value.Name));
              return 1.0;
            }
        }
    }
}
