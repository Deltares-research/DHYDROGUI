using System;
using System.Collections.Generic;
using DeltaShell.NGHS.IO.FileReaders;
using DeltaShell.NGHS.IO.Helpers;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.Properties;
using log4net;


namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport.ModelDefinition
{
    /// <summary>
    /// ModelDefinitionFileReader provides the methods to set different
    /// model wide aspects based upon a data access model of the md1d file.
    /// </summary>
    public static class ModelDefinitionFileReader
    {
        public static void SetWaterFlowModelProperties(string filePath, WaterFlowModel1D model, Action<string, IList<string>> createAndAddErrorReport)
        {
            var errorMessages = new List<string>();
            var modelSettingsCategories = ReadCategoriesFromFileAndCollectErrorMessages(filePath, errorMessages);

            foreach (var category in modelSettingsCategories)
            {
                try
                {
                    var propertySetter = WaterFlowModelPropertySetterFactory.GetPropertySetter(category);
                    propertySetter.SetProperties(category, model, createAndAddErrorReport);
                }
                catch (Exception)
                {
                    var errorMessage = string.Format(Resources.WaterFlowModelPropertySetter_SetWaterFlowModelProperties_There_is_unrecognized_data_read_from_the_md1d_file_with_header___0___, category.Name);
                    errorMessages.Add(errorMessage);
                }
            }

            createAndAddErrorReport?.Invoke("The following errors occurred when reading the md1d file:", errorMessages);
            model.UseSalt = true;
            model.UseTemperature = true;
        }

        private static IList<DelftIniCategory> ReadCategoriesFromFileAndCollectErrorMessages(string filePath, List<string> errorMessages)
        {
            IList<DelftIniCategory> categories = new List<DelftIniCategory>();
            try
            {
                categories = DelftIniFileParser.ReadFile(filePath);
            }
            catch (Exception e)
            {
                errorMessages.Add(e.Message);
            }

            return categories;
        }
    }
}
