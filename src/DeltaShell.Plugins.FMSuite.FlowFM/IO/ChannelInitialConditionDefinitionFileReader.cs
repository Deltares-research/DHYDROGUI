using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Hydro;
using DeltaShell.NGHS.IO.DataObjects.InitialConditions;
using DeltaShell.NGHS.IO.FileReaders;
using DeltaShell.NGHS.IO.Helpers;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using DeltaShell.Plugins.FMSuite.FlowFM.Properties;
using DHYDRO.Common.IO.Ini;
using GeoAPI.Extensions.Networks;
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
        /// <param name="channelInitialConditionDefinitions">A collection of <see cref="ChannelInitialConditionDefinition"/> to write.</param>
        /// <exception cref="FileReadingException">When an error occcurs during reading of the file.</exception>
        public static void ReadFile(
            string filePath,
            WaterFlowFMModelDefinition modelDefinition,
            Dictionary<string, IBranch> branchDictionary,
            IEnumerable<ChannelInitialConditionDefinition> channelInitialConditionDefinitions)
        {
            if (!File.Exists(filePath))
            {
                Log.Warn(Resources.FeatureFile1D2DReader_ReadInitialConditionFiles_No_Initial_Quantity_ini_file_found_);
                return;
            }

            var iniSections = new IniReader().ReadIniFile(filePath);
            if (iniSections.Count == 0) throw new FileReadingException(string.Format(Resources.ReadFile_Could_not_read_file__0__properly__it_seems_empty, filePath));

            // [Global]
            var globalIniSection = iniSections.FirstOrDefault(iniSection => iniSection.Name.Equals(InitialConditionRegion.GlobalDefinitionIniHeader));
            if (globalIniSection == null) throw new FileReadingException(string.Format(Resources.ReadFile_Could_not_read_file__0__properly_no_global_property_was_found, filePath));
            SetGlobalDefinition(globalIniSection, modelDefinition);

            // [Branch]
            var channelInitialConditionDefinitionsIniSections = iniSections.Where(iniSection => iniSection.Name.Equals(InitialConditionRegion.ChannelInitialConditionDefinitionIniHeader));
            var channelInitialConditionDefinitionDictionary = channelInitialConditionDefinitions.ToDictionary(def => def.Channel.Name, def => def, StringComparer.InvariantCultureIgnoreCase);
            ReadChannelInitialConditionDefinitions(branchDictionary, channelInitialConditionDefinitionDictionary, channelInitialConditionDefinitionsIniSections);
        }

        private static void SetGlobalDefinition(IniSection globalIniSection, WaterFlowFMModelDefinition modelDefinition)
        {
            var globalValue = globalIniSection.ReadProperty<string>(InitialConditionRegion.Value.Key);
            if (string.IsNullOrWhiteSpace(globalValue))
            {
                Log.Warn("No global value is specified. Using default value.");
            }
            else
            {
                modelDefinition.SetModelProperty(GuiProperties.InitialConditionGlobalValue1D, globalValue);
            }

            var globalQuantityString = globalIniSection.ReadProperty<string>(InitialConditionRegion.Quantity.Key);
            var globalQuantity = InitialConditionQuantityTypeConverter.ConvertStringToInitialConditionQuantity(globalQuantityString);

            modelDefinition.SetModelProperty(GuiProperties.InitialConditionGlobalQuantity1D, $"{ (int) globalQuantity }");
            
            readQuantity = globalQuantity;
        }

        private static void ReadChannelInitialConditionDefinitions(
            IReadOnlyDictionary<string, IBranch> branchDictionary,
            IReadOnlyDictionary<string, ChannelInitialConditionDefinition> channelInitialConditionDefinitionDictionary, 
            IEnumerable<IniSection> channelInitialConditionDefinitionsIniSections)
        {
            foreach (var channelInitialConditionDefinitionsIniSection in channelInitialConditionDefinitionsIniSections)
            {
                var branchId = channelInitialConditionDefinitionsIniSection.ReadProperty<string>(InitialConditionRegion.BranchId.Key);
                if (!branchDictionary.ContainsKey(branchId)) throw new FileReadingException(string.Format(Resources.ChannelInitialConditionDefinitionFileReader_ReadFile_Branch___0___where_the_initial_condition_should_be_put_on_is_not_available_in_the_model, branchId));

                var branch = branchDictionary[branchId];
                if (!(branch is IChannel)) throw new FileReadingException(string.Format(Resources.ChannelInitialConditionDefinitionFileReader_ReadFile_Branch___0___where_the_initial_condition_should_be_put_on_is_not_available_in_the_model, branchId));

                var branchName = branch.Name;
                if (!channelInitialConditionDefinitionDictionary.ContainsKey(branchName)) throw new FileReadingException(string.Format(Resources.ChannelInitialConditionDefinitionFileReader_ReadFile_Branch___0___where_the_initial_condition_should_be_put_on_is_not_available_in_the_model, branchId));

                var channelInitialConditionDefinition = channelInitialConditionDefinitionDictionary[branchName];

                if (IsSpatialDefinition(channelInitialConditionDefinitionsIniSection))
                {
                    ReadSpatialDefinition(channelInitialConditionDefinitionsIniSection, channelInitialConditionDefinition);
                }
                else
                {
                    ReadConstantDefinition(channelInitialConditionDefinitionsIniSection, channelInitialConditionDefinition);
                }
            }
        }

        private static bool IsSpatialDefinition(IniSection channelInitialConditionDefinitionsIniSection)
        {
            return channelInitialConditionDefinitionsIniSection.Properties.Any(p => p.Key.Equals(InitialConditionRegion.NumLocations.Key));
        }

        private static void ReadConstantDefinition(
            IniSection channelInitialConditionDefinitionsIniSection, 
            ChannelInitialConditionDefinition channelInitialConditionDefinition)
        {
            channelInitialConditionDefinition.SpecificationType = ChannelInitialConditionSpecificationType.ConstantChannelInitialConditionDefinition;
            channelInitialConditionDefinition.ConstantChannelInitialConditionDefinition.Quantity = readQuantity;

            var value = channelInitialConditionDefinitionsIniSection.ReadProperty<double>(InitialConditionRegion.Values.Key);
            channelInitialConditionDefinition.ConstantChannelInitialConditionDefinition.Value = value;
        }

        private static void ReadSpatialDefinition(
            IniSection channelInitialConditionDefinitionsIniSection, 
            ChannelInitialConditionDefinition channelInitialConditionDefinition)
        {
            channelInitialConditionDefinition.SpecificationType = ChannelInitialConditionSpecificationType.SpatialChannelInitialConditionDefinition;
            channelInitialConditionDefinition.SpatialChannelInitialConditionDefinition.Quantity = readQuantity;

            var numLocations = channelInitialConditionDefinitionsIniSection.ReadProperty<int>(InitialConditionRegion.NumLocations.Key);
            if (numLocations == 0) return;

            var chainages = channelInitialConditionDefinitionsIniSection.ReadPropertiesToListOfType<double>(InitialConditionRegion.Chainage.Key);
            var values = channelInitialConditionDefinitionsIniSection.ReadPropertiesToListOfType<double>(InitialConditionRegion.Values.Key);

            for (var i = 0; i < numLocations; i++)
            {
                var constantDefinition = new ConstantSpatialChannelInitialConditionDefinition
                {
                    Chainage = chainages[i],
                    Value = values[i]
                };

                channelInitialConditionDefinition.SpatialChannelInitialConditionDefinition.ConstantSpatialChannelInitialConditionDefinitions.Add(constantDefinition);
            }
        }
    }
}