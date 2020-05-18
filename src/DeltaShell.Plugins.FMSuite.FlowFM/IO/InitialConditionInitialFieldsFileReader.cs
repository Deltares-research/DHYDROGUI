using DelftTools.Hydro;
using DeltaShell.NGHS.IO;
using DeltaShell.NGHS.IO.FileReaders;
using DeltaShell.NGHS.IO.Helpers;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using log4net;
using System.IO;
using System.Linq;
using NetTopologySuite.Extensions.Properties;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO
{
    /// <summary>
    /// File reader for initialFields.ini.
    /// </summary>
    public static class InitialConditionInitialFieldsFileReader
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(InitialConditionInitialFieldsFileReader));

        /// <summary>
        /// Reads an initialFields.ini file.
        /// </summary>
        /// <param name="filePath">Path to the file to read.</param>
        /// <param name="modelDefinition">A <see cref="WaterFlowFMModelDefinition"/>.</param>
        /// <exception cref="FileReadingException">When an error occurs during reading of the file.</exception>
        /// <returns></returns>
        public static (InitialConditionQuantity, string) ReadFile(string filePath, WaterFlowFMModelDefinition modelDefinition)
        {
            if (!File.Exists(filePath)) throw new FileReadingException(string.Format(Properties.Resources.ReadFile_Could_not_read_file__0__properly__it_doesn_t_exist, filePath));

            var categories = new DelftIniReader().ReadDelftIniFile(filePath);
            if (categories.Count == 0) throw new FileReadingException(string.Format(Properties.Resources.ReadFile_Could_not_read_file__0__properly__it_seems_empty, filePath));


            // [Initial]
            var initialConditionCategories = categories.Where(category => category.Name.Equals(InitialConditionRegion.InitialConditionIniHeader)).ToList();
            var initialConditionCategoryCount = initialConditionCategories.Count;
            if (initialConditionCategoryCount == 0) throw new FileReadingException(string.Format(Properties.Resources.ReadFile_Could_not_read_file__0__properly__no_valid_content_categories_found, filePath));

            if (initialConditionCategoryCount > 1)
            {
                Log.Warn(Properties.Resources.Initial_Condition_Warning_Only_one_quantity_type_is_currently_supported_reading_the_first_and_ignoring_all_others);
            }
            
            var initialConditionCategory = initialConditionCategories.First();

            return ReadInitialConditionCategory(modelDefinition, initialConditionCategory);
            
        }

        private static (InitialConditionQuantity, string) ReadInitialConditionCategory(WaterFlowFMModelDefinition modelDefinition, DelftIniCategory initialConditionCategory)
        {
            var quantityString = initialConditionCategory.ReadProperty<string>(InitialConditionRegion.Quantity.Key);
            var quantity = InitialConditionQuantityTypeConverter.ConvertStringToInitialConditionQuantity(quantityString);
            var dataFile = initialConditionCategory.ReadProperty<string>(InitialConditionRegion.DataFile.Key);
            //var dataFileType = initialConditionCategory.ReadProperty<string>("dataFileType"); // not used currently

            return (quantity, dataFile);
        }

    }
}