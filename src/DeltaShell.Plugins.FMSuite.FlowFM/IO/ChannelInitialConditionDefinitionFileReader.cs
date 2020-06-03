using DelftTools.Hydro;
using DeltaShell.NGHS.IO;
using DeltaShell.NGHS.IO.DataObjects.InitialConditions;
using DeltaShell.NGHS.IO.FileReaders;
using DeltaShell.NGHS.IO.Helpers;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using DeltaShell.Plugins.FMSuite.FlowFM.Properties;
using GeoAPI.Extensions.Networks;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using log4net;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO
{
    /// <summary>
    /// File reader for channel initial condition quantity ini files.
    /// </summary>
    public static class ChannelInitialConditionDefinitionFileReader
    {
        private static ILog Log = LogManager.GetLogger(typeof(ChannelInitialConditionDefinitionFileReader));
        private static InitialConditionQuantity readQuantity;

        /// <summary>
        /// Reads a channel initial condition quantity ini file.
        /// </summary>
        /// <param name="filePath">Path to the file to read.</param>
        /// <param name="modelDefinition">A <see cref="WaterFlowFMModelDefinition"/>.</param>
        /// <param name="branchDictionary">A dictionary of branch names and <see cref="IBranch"/>.</param>
        /// <param name="channelInitialConditionDefintions">A collection of <see cref="ChannelInitialConditionDefinition"/> to write.</param>
        /// <exception cref="FileReadingException">When an error occcurs during reading of the file.</exception>
        public static void ReadFile(
            string filePath,
            WaterFlowFMModelDefinition modelDefinition,
            Dictionary<string, IBranch> branchDictionary,
            IList<ChannelInitialConditionDefinition> channelInitialConditionDefintions)
        {
            if (!File.Exists(filePath)) throw new FileReadingException(string.Format(Resources.ReadFile_Could_not_read_file__0__properly__it_doesn_t_exist, filePath));
            var categories = new DelftIniReader().ReadDelftIniFile(filePath);
            if (categories.Count == 0) throw new FileReadingException(string.Format(Resources.ReadFile_Could_not_read_file__0__properly__it_seems_empty, filePath));

            // [Global]
            var globalCategory = categories.FirstOrDefault(category => category.Name.Equals(InitialConditionRegion.GlobalDefinitionIniHeader));
            if (globalCategory == null) throw new FileReadingException(string.Format(Resources.ReadFile_Could_not_read_file__0__properly_no_global_property_was_found, filePath));
            SetGlobalDefinition(globalCategory, modelDefinition, filePath);

            // [Branch]
            IEnumerable<DelftIniCategory> channelInitialConditionDefinitionsCategories =
                categories.Where(category => category.Name.Equals(InitialConditionRegion.ChannelInitialConditionDefinitionIniHeader));

            var channelInitialConditionDefinitionDictionary = channelInitialConditionDefintions.ToDictionary(
                def => def.Channel.Name, def => def, StringComparer.InvariantCultureIgnoreCase);
            ReadChannelInitialConditionDefinitions(filePath, branchDictionary, channelInitialConditionDefinitionDictionary, channelInitialConditionDefinitionsCategories);

        }

        private static void ReadChannelInitialConditionDefinitions(
            string filePath,
            Dictionary<string, IBranch> branchDictionary,
            Dictionary<string, ChannelInitialConditionDefinition> channelInitialConditionDefinitionDictionary, 
            IEnumerable<DelftIniCategory> channelInitialConditionDefinitionsCategories)
        {
            foreach (var channelInitialConditionDefinitionsCategory in channelInitialConditionDefinitionsCategories)
            {
                var branchId = channelInitialConditionDefinitionsCategory.ReadProperty<string>(InitialConditionRegion.BranchId.Key);
                if (!branchDictionary.ContainsKey(branchId)) throw new FileReadingException(string.Format(Resources.ChannelInitialConditionDefinitionFileReader_ReadFile_Branch___0___where_the_initial_condition_should_be_put_on_is_not_available_in_the_model, branchId));
                var branch = branchDictionary[branchId];
                if (!(branch is IChannel)) throw new FileReadingException(string.Format(Resources.ChannelInitialConditionDefinitionFileReader_ReadFile_Branch___0___where_the_initial_condition_should_be_put_on_is_not_available_in_the_model, branchId));
                var branchName = branch.Name;

                if (!channelInitialConditionDefinitionDictionary.ContainsKey(branchName)) throw new FileReadingException(string.Format(Resources.ChannelInitialConditionDefinitionFileReader_ReadFile_Branch___0___where_the_initial_condition_should_be_put_on_is_not_available_in_the_model, branchId));
                var channelInitialConditionDefinition = channelInitialConditionDefinitionDictionary[branchName];

                if (IsSpatialDefinition(channelInitialConditionDefinitionsCategory))
                {
                    ReadSpatialDefinition(channelInitialConditionDefinitionsCategory, channelInitialConditionDefinition);
                }
                else
                {
                    ReadConstantDefinition(channelInitialConditionDefinitionsCategory, channelInitialConditionDefinition);
                }
            }
        }

        private static void ReadConstantDefinition(
            DelftIniCategory channelInitialConditionDefinitionsCategory, 
            ChannelInitialConditionDefinition channelInitialConditionDefinition)
        {
            var value = channelInitialConditionDefinitionsCategory.ReadProperty<double>(InitialConditionRegion.Values.Key);
            channelInitialConditionDefinition.SpecificationType =
                ChannelInitialConditionSpecificationType.ConstantChannelInitialConditionDefinition;

            channelInitialConditionDefinition.ConstantChannelInitialConditionDefinition.Value = value;
            channelInitialConditionDefinition.ConstantChannelInitialConditionDefinition.Quantity = readQuantity;
        }

        private static void ReadSpatialDefinition(
            DelftIniCategory channelInitialConditionDefinitionsCategory, 
            ChannelInitialConditionDefinition channelInitialConditionDefinition)
        {
            var numLocations = channelInitialConditionDefinitionsCategory.ReadProperty<int>(InitialConditionRegion.NumLocations.Key);
            var chainages = channelInitialConditionDefinitionsCategory.ReadPropertiesToListOfType<double>(InitialConditionRegion.Chainage.Key);
            var values = channelInitialConditionDefinitionsCategory.ReadPropertiesToListOfType<double>(InitialConditionRegion.Values.Key);

            channelInitialConditionDefinition.SpecificationType =
                ChannelInitialConditionSpecificationType.SpatialChannelInitialConditionDefinition;

            for (int i = 0; i < numLocations; i++)
            {
                var constantDefinition = new ConstantSpatialChannelInitialConditionDefinition
                {
                    Chainage = chainages[i],
                    Value = values[i]
                };
                channelInitialConditionDefinition.SpatialChannelInitialConditionDefinition
                    .ConstantSpatialChannelInitialConditionDefinitions.Add(constantDefinition);
            }
        }

        /// <summary>
        /// Checks if a DelftIniCategory contains a 'chainage' property indicating
        /// a spatial definition.
        /// </summary>
        /// <param name="channelInitialConditionDefinitionsCategory"></param>
        /// <returns>True if the category contains a 'chainage' property.</returns>
        private static bool IsSpatialDefinition(DelftIniCategory channelInitialConditionDefinitionsCategory)
        {
            return channelInitialConditionDefinitionsCategory.Properties.Any(p => p.Name.Equals(InitialConditionRegion.Chainage.Key));
        }

        private static void SetGlobalDefinition(DelftIniCategory globalCategory, WaterFlowFMModelDefinition modelDefinition, string filePath)
        {
            var globalValue = globalCategory.ReadProperty<string>(InitialConditionRegion.Value.Key);
            if (string.IsNullOrWhiteSpace(globalValue))
            {
                Log.Warn("No global value is specified. Using default value.");
            }
            else
            {
                modelDefinition.SetModelProperty(GuiProperties.InitialConditionGlobalValue1D, globalValue);
            }

            var globalQuantityString = globalCategory.ReadProperty<string>(InitialConditionRegion.Quantity.Key);
            var globalQuantity = InitialConditionQuantityTypeConverter.ConvertStringToInitialConditionQuantity(globalQuantityString);

            modelDefinition.SetModelProperty(GuiProperties.InitialConditionGlobalQuantity1D, $"{ (int)globalQuantity }");
            
            readQuantity = globalQuantity;
        }

    }
}