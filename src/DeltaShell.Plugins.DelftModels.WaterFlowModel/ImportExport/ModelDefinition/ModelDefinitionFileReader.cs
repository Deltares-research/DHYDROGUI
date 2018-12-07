using System;
using System.Collections.Generic;
using System.Linq;
using DeltaShell.NGHS.IO.FileReaders;
using DeltaShell.NGHS.IO.Helpers;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.Properties;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport.ModelDefinition
{
    /// <summary>
    /// ModelDefinitionFileReader provides the methods to set different
    /// model wide aspects based upon a data access model of the md1d file.
    /// </summary>
    public static class ModelDefinitionFileReader
    {
        /// <summary>
        /// Sets the properties according to the read data from the md1d file at location <paramref name="filePath"/>.
        /// </summary>
        /// <param name="filePath"> Location of the md1d file. </param>
        /// <param name="model"> The water flow model object to set the properties on. </param>
        /// <param name="createAndAddErrorReport"> Action that creates an error report for this specific reader. This report will be shown to the user 
        /// in the GUI and informs the user about possible errors when reading the md1d file. </param>
        public static void SetWaterFlowModelProperties(string filePath, WaterFlowModel1D model, Action<string, IList<string>> createAndAddErrorReport)
        {
            model.UseSalt = true;        // This should be removed as part of issue SOBEK3-1562

            var errorMessages = new List<string>();
            var modelSettingsCategories = ReadCategoriesFromFileAndCollectErrorMessages(filePath, errorMessages);
            model.SetInitialModelProperties(createAndAddErrorReport, modelSettingsCategories, errorMessages);
            model.SetSecondaryModelProperties(createAndAddErrorReport, modelSettingsCategories, errorMessages);

            createAndAddErrorReport?.Invoke("The following errors occurred when reading the md1d file:", errorMessages);
        }

        private static IList<DelftIniCategory> ReadCategoriesFromFileAndCollectErrorMessages(string filePath, ICollection<string> errorMessages)
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

        /// <summary>
        /// There are a few model properties that need to be read and set first, before the other properties are.
        /// </summary>
        private static void SetInitialModelProperties(this WaterFlowModel1D model, Action<string, IList<string>> createAndAddErrorReport, IEnumerable<DelftIniCategory> modelSettingsCategories, IList<string> errorMessages)
        {
            var initialCategories = modelSettingsCategories.Where(IsInitialCategory);
            model.SetProperties(initialCategories, errorMessages, createAndAddErrorReport);
        }

        /// <summary>
        /// Read and set the model properties that do not have to be read and set in any specific order.
        /// </summary>
        private static void SetSecondaryModelProperties(this WaterFlowModel1D model, Action<string, IList<string>> createAndAddErrorReport, IEnumerable<DelftIniCategory> modelSettingsCategories, IList<string> errorMessages)
        {
            var secondaryCategories = modelSettingsCategories.Where(category => !IsInitialCategory(category));
            model.SetProperties(secondaryCategories, errorMessages, createAndAddErrorReport);
        }

        private static bool IsInitialCategory(DelftIniCategory category)
        {
            return category.Name == ModelDefinitionsRegion.TransportComputationValuesHeader;
        }

        private static void SetProperties(this WaterFlowModel1D model, IEnumerable<DelftIniCategory> modelSettingsCategories, IList<string> errorMessages, Action<string, IList<string>> createAndAddErrorReport)
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
                    var errorMessage = string.Format(Resources.WaterFlowModelPropertySetter_SetWaterFlowModelProperties_There_is_unrecognized_data_read_from_the_md1d_file_with_header___0___, category.Name);
                    errorMessages.Add(errorMessage);
                }
            }
        }
    }
}
