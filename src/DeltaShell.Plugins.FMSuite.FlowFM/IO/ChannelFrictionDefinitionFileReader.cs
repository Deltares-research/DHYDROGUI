using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.Roughness;
using DelftTools.Utils.Collections.Generic;
using Deltares.Infrastructure.IO.Ini;
using DeltaShell.NGHS.IO.DataObjects.Friction;
using DeltaShell.NGHS.IO.FileReaders;
using DeltaShell.NGHS.IO.FileWriters.Roughness;
using DeltaShell.NGHS.IO.FileWriters.SpatialData;
using DeltaShell.NGHS.IO.Helpers;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using DeltaShell.Plugins.FMSuite.FlowFM.Properties;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO
{
    /// <summary>
    /// File reader for 1D channel roughness.
    /// </summary>
    public class ChannelFrictionDefinitionFileReader
    {
        /// <summary>
        /// Reads a file with 1D channel roughness.
        /// </summary>
        /// <param name="filePath">Path to the file to read.</param>
        /// <param name="modelDefinition">A <see cref="WaterFlowFMModelDefinition"/>.</param>
        /// <param name="network">The network containing the channels that are involved.</param>
        /// <param name="channelFrictionDefinitions">A collection of <see cref="ChannelFrictionDefinition"/>.</param>
        /// <exception cref="FileReadingException">When an error occcurs during reading of the file.</exception>
        public static void ReadFile(
            string filePath,
            WaterFlowFMModelDefinition modelDefinition,
            IHydroNetwork network,
            IEventedList<ChannelFrictionDefinition> channelFrictionDefinitions)
        {
            if (!File.Exists(filePath)) throw new FileReadingException(string.Format(Resources.ReadFile_Could_not_read_file__0__properly__it_doesn_t_exist, filePath));

            var iniSections = new IniMultiLineReader().ReadIniFile(filePath);
            if (iniSections.Count == 0) throw new FileReadingException(string.Format(Resources.ReadFile_Could_not_read_file__0__properly__it_seems_empty, filePath));

            // [Global]
            var globalIniSection = iniSections.FirstOrDefault(iniSection => iniSection.Name.Equals(RoughnessDataRegion.GlobalIniHeader));
            if (globalIniSection == null) throw new FileReadingException(string.Format(Resources.ReadFile_Could_not_read_file__0__properly_no_global_property_was_found, filePath));
            SetGlobalDefinition(globalIniSection, modelDefinition, filePath);

            // [Branch]
            var channelFrictionDefinitionsIniSections = iniSections.Where(iniSection => iniSection.Name.Equals(RoughnessDataRegion.BranchPropertiesIniHeader));
            ReadChannelFrictionDefinitions(network, channelFrictionDefinitions, channelFrictionDefinitionsIniSections);
            
            var channelFrictionDefinitionsLookup = channelFrictionDefinitions.ToDictionary(cfd => cfd.Channel);
            SynchronizeOnLanesSpecificationBasedOnSharedCrossSectionDefinitions(network, channelFrictionDefinitionsLookup);
        }

        private static void SetGlobalDefinition(IniSection globalIniSection, WaterFlowFMModelDefinition modelDefinition, string filePath)
        {
            var globalValue = globalIniSection.ReadProperty<string>(RoughnessDataRegion.FrictionValue.Key);
            var globalTypeString = globalIniSection.ReadProperty<string>(RoughnessDataRegion.FrictionType.Key);
            
            if (string.IsNullOrWhiteSpace(globalValue) || Enum.TryParse(globalTypeString, out RoughnessType globalType) == false)
            {
                throw new FileReadingException(string.Format(Resources.ChannelFrictionDefinitionFileReader_ReadFile_Could_not_read_file__0__properly_invalid_global_value_was_given, filePath));
            }
            
            modelDefinition.SetModelProperty(GuiProperties.UnifFrictTypeChannels, $"{ (int) globalType }");
            modelDefinition.SetModelProperty(GuiProperties.UnifFrictCoefChannels, globalValue);
        }

        private static void ReadChannelFrictionDefinitions(
            IHydroNetwork network,
            IEventedList<ChannelFrictionDefinition> channelFrictionDefinitions,
            IEnumerable<IniSection> channelFrictionDefinitionsIniSections)
        {
            foreach (var channelFrictionDefinitionIniSection in channelFrictionDefinitionsIniSections)
            {
                var branchId = channelFrictionDefinitionIniSection.ReadProperty<string>(SpatialDataRegion.BranchId.Key);
                var branch = network.Branches.FirstOrDefault(b => b.Name == branchId);
                if (!(branch is IChannel)) throw new FileReadingException(string.Format(Resources.ChannelFrictionDefinitionFileReader_ReadFile_Branch___0___where_the_roughness_should_be_put_on_is_not_available_in_the_model1, branchId));

                var channelFrictionDefinition = channelFrictionDefinitions.FirstOrDefault(cfd => cfd.Channel.Name.Equals(branch.Name, StringComparison.InvariantCultureIgnoreCase));
                if (channelFrictionDefinition == null) throw new FileReadingException(string.Format(Resources.ChannelFrictionDefinitionFileReader_ReadFile_Branch___0___where_the_roughness_should_be_put_on_is_not_available_in_the_model1, branchId));

                var functionTypeString = channelFrictionDefinitionIniSection.ReadProperty<string>(RoughnessDataRegion.FunctionType.Key);
                var functionType = RoughnessHelper.ConvertStringToRoughnessFunction(functionTypeString);

                var roughnessTypeString = channelFrictionDefinitionIniSection.ReadProperty<string>(RoughnessDataRegion.RoughnessType.Key);
                var roughnessType = RoughnessHelper.ConvertStringToRoughnessType(roughnessTypeString);
                
                switch (functionType)
                {
                    case RoughnessFunction.Constant:
                        if (IsSpatialDefinition(channelFrictionDefinitionIniSection))
                        {
                            ReadConstantSpatialChannelFrictionDefinitions(channelFrictionDefinition, channelFrictionDefinitionIniSection, roughnessType);
                        }
                        else
                        {
                            ReadConstantChannelFrictionDefinition(channelFrictionDefinition, roughnessType, channelFrictionDefinitionIniSection);
                        }
                        break;
                    case RoughnessFunction.FunctionOfQ:
                        ReadSpatialFunctionDefinition(channelFrictionDefinitionIniSection, channelFrictionDefinition, roughnessType, RoughnessFunction.FunctionOfQ);
                        break;
                    case RoughnessFunction.FunctionOfH:
                        ReadSpatialFunctionDefinition(channelFrictionDefinitionIniSection, channelFrictionDefinition, roughnessType, RoughnessFunction.FunctionOfH);
                        break;
                    default:
                        throw new InvalidOperationException();
                }
            }
        }

        private static void SynchronizeOnLanesSpecificationBasedOnSharedCrossSectionDefinitions(IHydroNetwork network, Dictionary<IChannel, ChannelFrictionDefinition> channelFrictionDefinitionsLookup)
        {
            foreach (var sharedCrossSectionDefinition in network.SharedCrossSectionDefinitions)
            {
                var crossSectionsUsingDefinition = sharedCrossSectionDefinition.FindUsage(network);
                var correspondingChannels = crossSectionsUsingDefinition.Select(cs => cs.Branch).Distinct().OfType<IChannel>();
                var correspondingChannelFrictionDefinitions = correspondingChannels.Select(channel => channelFrictionDefinitionsLookup[channel]);
                
                if (!correspondingChannelFrictionDefinitions.Any(cfd => cfd.SpecificationType == ChannelFrictionSpecificationType.RoughnessSections))
                {
                    continue;
                }

                foreach (var channelFrictionDefinition in correspondingChannelFrictionDefinitions)
                {
                    if (channelFrictionDefinitionsLookup.Values.Contains(channelFrictionDefinition))
                    {
                        channelFrictionDefinition.SpecificationType = ChannelFrictionSpecificationType.RoughnessSections;
                    }
                }
            }
        }

        private static bool IsSpatialDefinition(IniSection channelFrictionDefinitionIniSection)
        {
            return channelFrictionDefinitionIniSection.Properties.Any(p => p.Key.Equals(RoughnessDataRegion.NumberOfLocations.Key));
        }

        private static void ReadConstantSpatialChannelFrictionDefinitions(
            ChannelFrictionDefinition channelFrictionDefinition,
            IniSection channelFrictionDefinitionIniSection,
            RoughnessType roughnessType)
        {
            channelFrictionDefinition.SpecificationType = ChannelFrictionSpecificationType.SpatialChannelFrictionDefinition;
            channelFrictionDefinition.SpatialChannelFrictionDefinition.FunctionType = RoughnessFunction.Constant;
            channelFrictionDefinition.SpatialChannelFrictionDefinition.Type = roughnessType;

            var numLocations = channelFrictionDefinitionIniSection.ReadProperty<int>(RoughnessDataRegion.NumberOfLocations.Key);
            if (numLocations == 0) return;

            var chainages = channelFrictionDefinitionIniSection.ReadPropertiesToListOfType<double>(SpatialDataRegion.Chainage.Key);
            var frictionValues = channelFrictionDefinitionIniSection.ReadPropertiesToListOfType<double>(RoughnessDataRegion.Values.Key);
            
            for (var i = 0; i < numLocations; i++)
            {
                var constantDefinition = new ConstantSpatialChannelFrictionDefinition()
                {
                    Chainage = chainages[i],
                    Value = frictionValues[i]
                };
                channelFrictionDefinition.SpatialChannelFrictionDefinition.ConstantSpatialChannelFrictionDefinitions.Add(constantDefinition);
            }
        }

        private static void ReadConstantChannelFrictionDefinition(
            ChannelFrictionDefinition channelFrictionDefinition,
            RoughnessType roughnessType,
            IniSection channelFrictionDefinitionIniSection)
        {
            channelFrictionDefinition.SpecificationType = ChannelFrictionSpecificationType.ConstantChannelFrictionDefinition;
            channelFrictionDefinition.ConstantChannelFrictionDefinition.Type = roughnessType;

            var frictionValue = channelFrictionDefinitionIniSection.ReadProperty<double>(RoughnessDataRegion.Values.Key);
            channelFrictionDefinition.ConstantChannelFrictionDefinition.Value = frictionValue;
        }

        private static void ReadSpatialFunctionDefinition(
            IniSection channelFrictionDefinitionIniSection,
            ChannelFrictionDefinition channelFrictionDefinition,
            RoughnessType roughnessType,
            RoughnessFunction roughnessFunctionType)
        {
            channelFrictionDefinition.SpecificationType = ChannelFrictionSpecificationType.SpatialChannelFrictionDefinition;
            channelFrictionDefinition.SpatialChannelFrictionDefinition.FunctionType = roughnessFunctionType;
            channelFrictionDefinition.SpatialChannelFrictionDefinition.Type = roughnessType;

            var function = channelFrictionDefinition.SpatialChannelFrictionDefinition.Function;

            var numLocations = channelFrictionDefinitionIniSection.ReadProperty<int>(RoughnessDataRegion.NumberOfLocations.Key);
            var chainages = channelFrictionDefinitionIniSection.ReadPropertiesToListOfType<double>(SpatialDataRegion.Chainage.Key).ToArray();

            var numLevels = channelFrictionDefinitionIniSection.ReadProperty<int>(RoughnessDataRegion.NumberOfLevels.Key);
            if (numLevels == 0)
            {
                function.Arguments[0].SetValues(chainages);
                return;
            }

            var levels = channelFrictionDefinitionIniSection.ReadPropertiesToListOfType<double>(RoughnessDataRegion.Levels.Key).ToArray();
            var frictionValuesList = channelFrictionDefinitionIniSection.ReadPropertiesToListOfType<double>(RoughnessDataRegion.Values.Key);
            var frictionValues = Create2dArrayOfFrictionValuesFromList(frictionValuesList, numLevels, chainages.Length);

            for (var i = 0; i < numLevels; i++)
            {
                var level = levels[i];
                for (var j = 0; j < numLocations; j++)
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
            for (var i = 0; i < numLevels; i++)
            {
                for (var j = 0; j < chainageCount; j++)
                {
                    test[i, j] = frictionValues[frictionValueIndex];
                    frictionValueIndex++;
                }
            }

            return test;
        }
    }
}
