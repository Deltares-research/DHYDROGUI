using System;
using System.Collections.Generic;
using DeltaShell.NGHS.IO.Helpers;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.Properties;
using log4net;


namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport.ModelDefinition
{
    /// <summary>
    /// WaterFlowModelPropertySetter provides the methods to set different
    /// model wide aspects based upon a data access model of the md1d file.
    /// </summary>
    public static class WaterFlowModelPropertySetter
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(WaterFlowModel1D));

        public static void SetWaterFlowModelProperties(IEnumerable<DelftIniCategory> modelSettingsCategories, WaterFlowModel1D model, Action<string, IList<string>> createAndAddErrorReport)
        {
            foreach (var category in modelSettingsCategories)
            {
                try
                {
                    var propertySetter = WaterFlowModelPropertySetterFactory.GetPropertySetter(category);
                    propertySetter.SetProperties(category, model, createAndAddErrorReport);
                }
                catch (Exception)
                {
                    Log.WarnFormat(Resources.WaterFlowModelPropertySetter_SetWaterFlowModelProperties_There_is_unrecognized_data_read_from_the_md1d_file_with_header___0___, category.Name);
                }
            }
        }
    }
}
