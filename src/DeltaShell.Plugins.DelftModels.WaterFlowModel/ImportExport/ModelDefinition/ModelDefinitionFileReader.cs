using System;
using System.Collections.Generic;
using System.Linq;
using DeltaShell.NGHS.IO.FileReaders;
using DeltaShell.NGHS.IO.FileWriters.General;
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
           var errorMessages = new List<string>();
            var modelSettingsCategories = ReadCategoriesFromFileAndCollectErrorMessages(filePath, errorMessages);
            model.SetInitialModelProperties(modelSettingsCategories, errorMessages);
            model.SetSecondaryModelProperties(modelSettingsCategories, errorMessages);

            createAndAddErrorReport?.Invoke("The following errors occurred when reading the md1d file", errorMessages);
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
        /// There are a few model properties that need to be read and set first, before any other properties are.
        /// </summary>
        private static void SetInitialModelProperties(this WaterFlowModel1D model, IEnumerable<DelftIniCategory> modelSettingsCategories, IList<string> errorMessages)
        {
            var initialCategories = modelSettingsCategories.Where(IsInitialCategory);
            model.SetProperties(initialCategories, errorMessages);
        }

        /// <summary>
        /// Read and set the model properties that do not have to be read and set in any specific order.
        /// </summary>
        private static void SetSecondaryModelProperties(this WaterFlowModel1D model, IEnumerable<DelftIniCategory> modelSettingsCategories, IList<string> errorMessages)
        {
            var secondaryCategories = modelSettingsCategories.Where(IsSecondaryCategory);
            model.SetProperties(secondaryCategories, errorMessages);
        }

        private static bool IsInitialCategory(IDelftIniCategory category)
        {
            return (category.Name == ModelDefinitionsRegion.TransportComputationValuesHeader || category.Name == ModelDefinitionsRegion.SalinityValuesHeader);
        }

        private static bool IsSecondaryCategory(IDelftIniCategory category)
        {
            return (!IsInitialCategory(category) && !IsExcluded(category));
        }

        /// <summary>
        /// Determines whether the specified category is excluded.
        /// </summary>
        /// <remarks>
        /// Predicate to determine which categories are currently excluded. These values
        /// will not be acted on, nor will they generate error messages.
        /// </remarks>
        /// <param name="category">The category.</param>
        /// <returns>
        ///   <c>true</c> if the specified category is excluded; otherwise, <c>false</c>.
        /// </returns>
        private static bool IsExcluded(IDelftIniCategory category)
        {
            return category.Name == GeneralRegion.IniHeader || 
                   category.Name == ModelDefinitionsRegion.FilesIniHeader;
        }

        private static void SetProperties(this WaterFlowModel1D model, IEnumerable<DelftIniCategory> modelSettingsCategories, IList<string> errorMessages)
        {
            foreach (var category in modelSettingsCategories)
            {
                try
                {
                    var propertySetter = WaterFlowModelPropertySetterFactory.GetPropertySetter(category);
                    propertySetter.SetProperties(category, model, errorMessages);
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
