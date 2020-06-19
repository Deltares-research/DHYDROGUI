using DelftTools.Functions.Filters;
using DelftTools.Hydro.Roughness;
using DeltaShell.NGHS.IO.DataObjects.Friction;
using GeoAPI.Extensions.Coverages;
using GeoAPI.Extensions.Networks;
using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Functions;
using DelftTools.Hydro;

namespace DeltaShell.Plugins.ImportExport.Sobek
{
    public class SobekToWaterFlowFMRoughnessConverter
    {
        private const int NetworkLocationArgumentIndex = 0;
        private const int RoughnessValueComponentIndex = 0;
        private const int ChainageArgumentIndex = 0;
        private const int FunctionArgumentIndex = 1;

        /// <summary>
        /// Converts roughness from a collection of <see cref="RoughnessSection"/> to a collection of
        /// <see cref="ChannelFrictionDefinition"/>.
        /// </summary>
        /// <param name="channelFrictionDefinitions">The channel friction definitions to be updated.</param>
        /// <param name="defaultRoughnessSection">The roughness section to be converted to <see cref="ChannelFrictionDefinition"/>.</param>
        /// <param name="network">The network of the corresponding model.</param>
        /// <exception cref="ArgumentNullException">When one of the input parameters equals <c>null</c>.</exception>
        /// <exception cref="IndexOutOfRangeException"></exception>
        /// <exception cref="ArgumentOutOfRangeException">When an invalid <see cref="RoughnessFunction"/> is provided.</exception>
        public void ConvertSobekRoughnessToWaterFlowFmRoughness(
            IEnumerable<ChannelFrictionDefinition> channelFrictionDefinitions,
            RoughnessSection defaultRoughnessSection,
            IHydroNetwork network)
        {
            if (channelFrictionDefinitions == null)
            {
                throw new ArgumentNullException(nameof(channelFrictionDefinitions));
            }

            if (defaultRoughnessSection == null)
            {
                throw new ArgumentNullException(nameof(defaultRoughnessSection));
            }

            if (network == null)
            {
                throw new ArgumentNullException(nameof(network));
            }

            if (!channelFrictionDefinitions.Any())
            {
                return;
            }

            var roughnessSectionsPerBranch = GetRoughnessSectionsPerBranch(new [] { defaultRoughnessSection });

            foreach (var channelFrictionDefinition in channelFrictionDefinitions)
            {
                var channel = channelFrictionDefinition.Channel;
                if (!roughnessSectionsPerBranch.ContainsKey(channel))
                {
                    continue;
                }

                UpdateChannelFrictionDefinition(channelFrictionDefinition, roughnessSectionsPerBranch);
            }
        }

        private void UpdateChannelFrictionDefinition(
            ChannelFrictionDefinition channelFrictionDefinition, 
            IReadOnlyDictionary<IBranch, HashSet<RoughnessSection>> roughnessSectionsPerBranch)
        {
            var channel = channelFrictionDefinition.Channel;
            var sectionCount = roughnessSectionsPerBranch[channel].Count;

            // if roughness of branch is defined in none of the sections, set channelFrictionDefinition to 'Use global value'
            if (sectionCount == 0)
            {
                channelFrictionDefinition.SpecificationType = ChannelFrictionSpecificationType.ModelSettings;
                return;
            }

            // if roughness of branch is defined in multiple sections, set channelFrictionDefinition to 'On lanes'
            if (sectionCount > 1)
            {
                channelFrictionDefinition.SpecificationType = ChannelFrictionSpecificationType.RoughnessSections;
                return;
            }

            // if roughness of branch is defined in exactly one section, convert it to new data model and remove it from roughness section 
            if (sectionCount == 1)
            {
                var roughnessSection = roughnessSectionsPerBranch[channel].FirstOrDefault();
                if (roughnessSection == null) throw new IndexOutOfRangeException();
                ConvertRoughnessSectionToChannelFrictionDefinition(channelFrictionDefinition, roughnessSection);
            }
        }

        private void ConvertRoughnessSectionToChannelFrictionDefinition(
            ChannelFrictionDefinition channelFrictionDefinition, 
            RoughnessSection roughnessSection)
        {
            var channel = channelFrictionDefinition.Channel;

            var roughnessNetworkCoverage = roughnessSection.RoughnessNetworkCoverage;
            var networkLocations = roughnessNetworkCoverage.GetLocationsForBranch(channel);

            if (networkLocations.Count == 0)
            {
                channelFrictionDefinition.SpecificationType = ChannelFrictionSpecificationType.ModelSettings;
                return;
            }

            var sectionRoughnessType = GetRoughnessTypeForRoughnessSection(roughnessNetworkCoverage, networkLocations.First());

            if (networkLocations.Count == 1) // branch constant
            {
                var sectionRoughnessValue = roughnessNetworkCoverage.EvaluateRoughnessValue(networkLocations.First());
                SetChannelFrictionDefinitionToConstantDefinition(channelFrictionDefinition, sectionRoughnessValue, sectionRoughnessType);
            }

            if (networkLocations.Count > 1) // branch chainages
            {
                SetChannelFrictionDefinitionToSpatialDefinition(channelFrictionDefinition, roughnessSection, networkLocations, roughnessNetworkCoverage, sectionRoughnessType);
                roughnessSection.RemoveRoughnessFunctionsForBranch(channel);
            }

            // remove from network coverage
            roughnessNetworkCoverage.Arguments[RoughnessValueComponentIndex].RemoveValues(new VariableValueFilter<INetworkLocation>(roughnessNetworkCoverage.Arguments[0], networkLocations));
        }

        private static void SetChannelFrictionDefinitionToConstantDefinition(
            ChannelFrictionDefinition channelFrictionDefinition,
            double sectionRoughnessValue, RoughnessType sectionRoughnessType)
        {
            channelFrictionDefinition.SpecificationType = ChannelFrictionSpecificationType.ConstantChannelFrictionDefinition;
            channelFrictionDefinition.ConstantChannelFrictionDefinition.Value = sectionRoughnessValue;
            channelFrictionDefinition.ConstantChannelFrictionDefinition.Type = sectionRoughnessType;
        }

        private void SetChannelFrictionDefinitionToSpatialDefinition(
            ChannelFrictionDefinition channelFrictionDefinition, 
            RoughnessSection roughnessSection, 
            IList<INetworkLocation> networkLocations,
            RoughnessNetworkCoverage roughnessNetworkCoverage,
            RoughnessType roughnessType)
        {
            var channel = channelFrictionDefinition.Channel;
            
            var functionType = roughnessSection.GetRoughnessFunctionType(channel);

            channelFrictionDefinition.SpecificationType = ChannelFrictionSpecificationType.SpatialChannelFrictionDefinition;
            var spatialDefinition = channelFrictionDefinition.SpatialChannelFrictionDefinition;
            channelFrictionDefinition.SpatialChannelFrictionDefinition.Type = roughnessType;
            spatialDefinition.FunctionType = functionType;
            

            switch (functionType)
            {
                case RoughnessFunction.Constant:
                    UpdateConstantSpatialChannelFrictionDefinitions(channelFrictionDefinition, networkLocations, roughnessNetworkCoverage);
                    break;
                case RoughnessFunction.FunctionOfQ:
                    var functionOfQ = roughnessSection.FunctionOfQ(channel);
                    UpdateFunctionSpatialChannelFrictionDefinition(spatialDefinition, functionOfQ);
                    break;
                case RoughnessFunction.FunctionOfH:
                    var functionOfH = roughnessSection.FunctionOfH(channel);
                    UpdateFunctionSpatialChannelFrictionDefinition(spatialDefinition, functionOfH);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private static void UpdateFunctionSpatialChannelFrictionDefinition(SpatialChannelFrictionDefinition spatialDefinition, IFunction function)
        {
            var chainageArgument = function.Arguments[ChainageArgumentIndex];
            var functionArgument = function.Arguments[FunctionArgumentIndex];
            var roughnessComponent = function.Components[RoughnessValueComponentIndex];
            spatialDefinition.Function.Arguments[ChainageArgumentIndex].SetValues(chainageArgument.Values);
            spatialDefinition.Function.Arguments[FunctionArgumentIndex].SetValues(functionArgument.Values);
            spatialDefinition.Function.Components[RoughnessValueComponentIndex].SetValues(roughnessComponent.Values);
        }

        private static void UpdateConstantSpatialChannelFrictionDefinitions(
            ChannelFrictionDefinition channelFrictionDefinition,
            IEnumerable<INetworkLocation> networkLocations, 
            RoughnessNetworkCoverage roughnessNetworkCoverage)
        {
            foreach (var networkLocation in networkLocations)
            {
                var constantSpatialDefinition = new ConstantSpatialChannelFrictionDefinition()
                {
                    Chainage = networkLocation.Chainage,
                    Value = roughnessNetworkCoverage.EvaluateRoughnessValue(networkLocation)
                };
                channelFrictionDefinition.SpatialChannelFrictionDefinition.ConstantSpatialChannelFrictionDefinitions.Add(constantSpatialDefinition);
            }
        }

        private static RoughnessType GetRoughnessTypeForRoughnessSection(RoughnessNetworkCoverage roughnessNetworkCoverage, INetworkLocation networkLocation)
        {
            return roughnessNetworkCoverage.EvaluateRoughnessType(networkLocation);
        }

        private static Dictionary<IBranch, HashSet<RoughnessSection>> GetRoughnessSectionsPerBranch(IEnumerable<RoughnessSection> roughnessSections)
        {
            var roughnessSectionsPerBranch = new Dictionary<IBranch, HashSet<RoughnessSection>>();
            foreach (var roughnessSection in roughnessSections)
            {
                var roughnessNetworkCoverage = roughnessSection.RoughnessNetworkCoverage;

                foreach (var networkLocationObject in roughnessNetworkCoverage.Arguments[NetworkLocationArgumentIndex].Values)
                {
                    var networkLocation = (INetworkLocation) networkLocationObject;
                    var branch = networkLocation.Branch;

                    if (!roughnessSectionsPerBranch.ContainsKey(branch))
                    {
                        roughnessSectionsPerBranch.Add(branch, new HashSet<RoughnessSection>());
                    }

                    roughnessSectionsPerBranch[branch].Add(roughnessSection);
                }
            }

            return roughnessSectionsPerBranch;
        }
    }
}