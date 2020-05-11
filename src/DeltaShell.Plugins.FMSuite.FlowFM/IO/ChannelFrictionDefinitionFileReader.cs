using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.Roughness;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.NGHS.IO;
using DeltaShell.NGHS.IO.DataObjects.Friction;
using DeltaShell.NGHS.IO.FileReaders;
using DeltaShell.NGHS.IO.FileWriters.Roughness;
using DeltaShell.NGHS.IO.FileWriters.SpatialData;
using DeltaShell.NGHS.IO.Helpers;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using DeltaShell.Plugins.FMSuite.FlowFM.Properties;
using GeoAPI.Extensions.Networks;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO
{
    /// <summary>
    /// File reader for 1D channel roughness.
    /// </summary>
    public class ChannelFrictionDefinitionFileReader
    {
        /// <summary>
        /// Reads a file with ChannelFrictionDefinitions into an IEventedList.
        /// </summary>
        /// <param name="filePath">The name of the file to read from.</param>
        /// <param name="network">The network of your model.</param>
        /// <param name="channelFrictionDefinitions">A collection of ChannelFrictionDefinitions.</param>
        /// <exception cref="FileReadingException">When the file is empty or contains invalid information.</exception>
        public static void ReadFile(string filePath, WaterFlowFMModelDefinition modelDefinition, INetwork network, IEventedList<ChannelFrictionDefinition> channelFrictionDefinitions)
        {
            if (!File.Exists(filePath)) throw new FileReadingException(string.Format(Resources.ChannelFrictionDefinitionFileReader_ReadFile_Could_not_read_file__0__properly__it_doesn_t_exist_1, filePath));

            var categories = new DelftIniMultiLineReader().ReadDelftIniFile(filePath);
            if (categories.Count == 0) throw new FileReadingException(string.Format(Resources.ChannelFrictionDefinitionFileReader_ReadFile_Could_not_read_file__0__properly__it_seems_empty_1, filePath));

            // [Global]
            var globalCategory = categories.FirstOrDefault(category => category.Name.Equals(RoughnessDataRegion.GlobalIniHeader));
            if (globalCategory == null) throw new FileReadingException(string.Format(Resources.ChannelFrictionDefinitionFileReader_ReadFile_Could_not_read_file__0__properly_no_global_property_was_found, filePath));
            SetGlobalDefinition(globalCategory, modelDefinition, filePath);

            // [Branch]
            IEnumerable<DelftIniCategory> channelFrictionDefinitionsCategories =
                categories.Where(category => category.Name.Equals(RoughnessDataRegion.BranchPropertiesIniHeader));
            ReadChannelFrictionDefinitions(filePath, network, channelFrictionDefinitions, channelFrictionDefinitionsCategories);
        }

        private static void SetGlobalDefinition(DelftIniCategory globalCategory, WaterFlowFMModelDefinition modelDefinition, string filePath)
        {
            var globalValue = globalCategory.ReadProperty<string>(RoughnessDataRegion.FrictionValue.Key);
            var globalTypeString = globalCategory.ReadProperty<string>(RoughnessDataRegion.FrictionType.Key);
            
            if (string.IsNullOrWhiteSpace(globalValue) ||
                Enum.TryParse(globalTypeString, out RoughnessType globalType) == false)
            {
                throw new FileReadingException(string.Format(Resources.ChannelFrictionDefinitionFileReader_ReadFile_Could_not_read_file__0__properly_invalid_global_value_was_given, filePath));
            }
            modelDefinition.SetModelProperty(GuiProperties.UnifFrictCoefChannels, globalValue);
            modelDefinition.SetModelProperty(GuiProperties.UnifFrictTypeChannels, $"{ (int)globalType }");
        }

        /// <summary>
        /// Puts the read ChannelFrictionDefinitions into the provided IEventedList.
        /// </summary>
        /// <param name="filePath">The name of the file to read from.</param>
        /// <param name="network">The network of your model.</param>
        /// <param name="channelFrictionDefinitions">A collection of ChannelFrictionDefinitions.</param>
        /// <param name="channelFrictionDefinitionsCategories">Collection of DelftIniCategories containing ChannelFrictionDefinitions.</param>
        /// <exception cref="FileReadingException">When the file is empty or contains invalid information.</exception>
        /// <exception cref="InvalidOperationException">When an unknown <see cref="RoughnessFunction"/> is provided.</exception>
        private static void ReadChannelFrictionDefinitions(string filePath, INetwork network,
            IEventedList<ChannelFrictionDefinition> channelFrictionDefinitions, IEnumerable<DelftIniCategory> channelFrictionDefinitionsCategories)
        {
            foreach (var channelFrictionDefinitionCategory in channelFrictionDefinitionsCategories)
            {
                var branchId = channelFrictionDefinitionCategory.ReadProperty<string>(SpatialDataRegion.BranchId.Key);
                var branch = network.Branches.FirstOrDefault(b => b.Name == branchId);
                if (!(branch is IChannel))
                    throw new FileReadingException(string.Format(Resources.ChannelFrictionDefinitionFileReader_ReadFile_Branch___0___where_the_roughness_should_be_put_on_is_not_available_in_the_model1, filePath));

                var channelFrictionDefinition = channelFrictionDefinitions.FirstOrDefault(cfd =>
                    cfd.Channel.Name.Equals(branch.Name, StringComparison.InvariantCultureIgnoreCase));
                if (channelFrictionDefinition == null)
                    throw new FileReadingException(string.Format(Resources.ChannelFrictionDefinitionFileReader_ReadFile_Branch___0___where_the_roughness_should_be_put_on_is_not_available_in_the_model1, filePath));

                var functionTypeString = channelFrictionDefinitionCategory.ReadProperty<string>(RoughnessDataRegion.FunctionType.Key);
                if (Enum.TryParse(functionTypeString, true, out RoughnessFunction functionType) == false)
                    throw new FileReadingException(string.Format(Resources.ChannelFrictionDefinitionFileReader_ReadFile_Could_not_read_content_section__0__properly__1__is_not_a_valid_function_type, filePath, functionTypeString));

                var roughnessTypeString = channelFrictionDefinitionCategory.ReadProperty<string>(RoughnessDataRegion.RoughnessType.Key);
                RoughnessType roughnessType = RoughnessHelper.ConvertStringToRoughnessType(roughnessTypeString);
                
                switch (functionType)
                {
                    case RoughnessFunction.Constant:

                        if (IsConstantFunction(channelFrictionDefinitionCategory))
                        {
                            AddConstantFunctionPropertiesToDefinition(channelFrictionDefinition, channelFrictionDefinitionCategory, roughnessType);
                        }
                        else
                        {
                            AddBranchConstantPropertiesToDefinition(channelFrictionDefinition, roughnessType, channelFrictionDefinitionCategory);
                        }
                        break;
                    case RoughnessFunction.FunctionOfQ:
                        AddFunctionPropertiesToDefinition(channelFrictionDefinitionCategory, channelFrictionDefinition, roughnessType, RoughnessFunction.FunctionOfQ);
                        break;
                    case RoughnessFunction.FunctionOfH:
                        AddFunctionPropertiesToDefinition(channelFrictionDefinitionCategory, channelFrictionDefinition, roughnessType, RoughnessFunction.FunctionOfH);
                        break;
                    default:
                        throw new InvalidOperationException();
                }
            }
        }

        private static void AddBranchConstantPropertiesToDefinition(ChannelFrictionDefinition channelFrictionDefinition,
            RoughnessType roughnessType, DelftIniCategory channelFrictionDefinitionCategory)
        {
            channelFrictionDefinition.SpecificationType = ChannelFrictionSpecificationType.ConstantChannelFrictionDefinition;
            channelFrictionDefinition.ConstantChannelFrictionDefinition.Type = roughnessType;
            var frictionValue = channelFrictionDefinitionCategory.ReadProperty<double>(RoughnessDataRegion.Values.Key);
            channelFrictionDefinition.ConstantChannelFrictionDefinition.Value = frictionValue;
        }

        private static void AddConstantFunctionPropertiesToDefinition(ChannelFrictionDefinition channelFrictionDefinition,
            DelftIniCategory channelFrictionDefinitionCategory, RoughnessType roughnessType)
        {
            channelFrictionDefinition.SpecificationType = ChannelFrictionSpecificationType.SpatialChannelFrictionDefinition;
            channelFrictionDefinition.SpatialChannelFrictionDefinition.FunctionType = RoughnessFunction.Constant;
            channelFrictionDefinition.SpatialChannelFrictionDefinition.Type = roughnessType;

            var numLocations = channelFrictionDefinitionCategory.ReadProperty<int>(RoughnessDataRegion.NumberOfLocations.Key);
            if (numLocations == 0) return;

            var chainages = channelFrictionDefinitionCategory.ReadPropertiesToListOfType<double>(SpatialDataRegion.Chainage.Key);
            var frictionValues = channelFrictionDefinitionCategory.ReadPropertiesToListOfType<double>(RoughnessDataRegion.Values.Key);
            
            for (int i = 0; i < numLocations; i++)
            {
                var constantDefinition = new ConstantSpatialChannelFrictionDefinition()
                {
                    Chainage = chainages[i],
                    Value = frictionValues[i]
                };
                channelFrictionDefinition.SpatialChannelFrictionDefinition.ConstantSpatialChannelFrictionDefinitions.Add(constantDefinition);
            }

        }

        private static bool IsConstantFunction(DelftIniCategory channelFrictionDefinitionCategory)
        {
            return channelFrictionDefinitionCategory.Properties.Any(p =>
                p.Name.Equals(RoughnessDataRegion.NumberOfLocations.Key));
        }

        private static void AddFunctionPropertiesToDefinition(DelftIniCategory channelFrictionDefinitionCategory,
            ChannelFrictionDefinition channelFrictionDefinition, RoughnessType roughnessType, RoughnessFunction roughnessFunctionType)
        {
            channelFrictionDefinition.SpecificationType = ChannelFrictionSpecificationType.SpatialChannelFrictionDefinition;
            channelFrictionDefinition.SpatialChannelFrictionDefinition.FunctionType = roughnessFunctionType;
            channelFrictionDefinition.SpatialChannelFrictionDefinition.Type = roughnessType;
            var function = channelFrictionDefinition.SpatialChannelFrictionDefinition.Function;

            var numLocations = channelFrictionDefinitionCategory.ReadProperty<int>(RoughnessDataRegion.NumberOfLocations.Key);
            var chainages = channelFrictionDefinitionCategory.ReadPropertiesToListOfType<double>(SpatialDataRegion.Chainage.Key).ToArray();

            var numLevels = channelFrictionDefinitionCategory.ReadProperty<int>(RoughnessDataRegion.NumberOfLevels.Key);
            if (numLevels == 0)
            {
                function.Arguments[0].SetValues(chainages);
                return;
            }

            var levels = channelFrictionDefinitionCategory.ReadPropertiesToListOfType<double>(RoughnessDataRegion.Levels.Key).ToArray();
            var frictionValuesList = channelFrictionDefinitionCategory.ReadPropertiesToListOfType<double>(RoughnessDataRegion.Values.Key);
            var frictionValues = Create2dArrayOfFrictionValuesFromList(frictionValuesList, numLevels, chainages.Length);

            for (int i = 0; i < numLevels; i++)
            {
                var level = levels[i];
                for (int j = 0; j < numLocations; j++)
                {
                    var chainage = chainages[j];
                    function[chainage, level] = frictionValues[i, j];
                }
            }
        }

        private static double[,] Create2dArrayOfFrictionValuesFromList(IList<double> frictionValues, int numLevels, int chainageCount)
        {
            var test = new double[numLevels,chainageCount];
            var frictionValueIndex = 0;
            for (int i = 0; i < numLevels; i++)
            {
                for (int j = 0; j < chainageCount; j++)
                {
                    test[i, j] = frictionValues[frictionValueIndex];
                    frictionValueIndex++;
                }
            }

            return test;
        }
    }
}
