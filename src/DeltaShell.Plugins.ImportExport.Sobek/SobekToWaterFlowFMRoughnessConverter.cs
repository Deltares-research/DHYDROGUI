using DelftTools.Functions.Filters;
using DelftTools.Hydro.Roughness;
using DeltaShell.NGHS.IO.DataObjects.Friction;
using GeoAPI.Extensions.Coverages;
using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Functions;
using DelftTools.Hydro;
using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.CrossSections.Extensions;
using NetTopologySuite.Extensions.Coverages;

namespace DeltaShell.Plugins.ImportExport.Sobek
{
    public class SobekToWaterFlowFMRoughnessConverter
    {
        private const int RoughnessValueComponentIndex = 0;
        private const int ChainageArgumentIndex = 0;
        private const int FunctionArgumentIndex = 1;

        /// <summary>
        /// Converts roughness from a collection of <see cref="RoughnessSection"/> to a collection of
        /// <see cref="ChannelFrictionDefinition"/>.
        /// </summary>
        /// <param name="channelFrictionDefinitions">The channel friction definitions to be updated.</param>
        /// <param name="defaultRoughnessSection">The roughness section to be converted to <see cref="ChannelFrictionDefinition"/>.</param>
        /// <param name="network">The network of the model.</param>
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

            var remainingChannelFrictionDefinitions = SetOnLanesForRelevantChannelFrictionDefinitions(channelFrictionDefinitions, network);

            SetDefaultRoughnessSectionDataToRemainingChannelFrictionDefinitions(remainingChannelFrictionDefinitions, defaultRoughnessSection);
        }

        private static IEnumerable<ChannelFrictionDefinition> SetOnLanesForRelevantChannelFrictionDefinitions(
            IEnumerable<ChannelFrictionDefinition> channelFrictionDefinitions,
            IHydroNetwork network)
        {
            var remainingChannelFrictionDefinitions = new List<ChannelFrictionDefinition>();

            foreach (var channelFrictionDefinition in channelFrictionDefinitions)
            {
                var channel = channelFrictionDefinition.Channel;
                if (ChannelHasLanesDefinitions(channel))
                {
                    channelFrictionDefinition.SpecificationType = ChannelFrictionSpecificationType.RoughnessSections;
                }
                else
                {
                    remainingChannelFrictionDefinitions.Add(channelFrictionDefinition);
                }
            }

            return remainingChannelFrictionDefinitions;
        }

        private static bool ChannelHasLanesDefinitions(IChannel channel)
        {
            var crossSectionDefinitions = channel.CrossSections.Select(cs => cs.GetCrossSectionDefinition());

            foreach (var crossSectionDefinition in crossSectionDefinitions)
            {
                switch (crossSectionDefinition.CrossSectionType)
                {
                    case CrossSectionType.GeometryBased:
                    case CrossSectionType.YZ:
                    case CrossSectionType.Standard:
                        if (crossSectionDefinition.Sections.Any(s => s.SectionType.Name != RoughnessDataSet.MainSectionTypeName))
                        {
                            return true;
                        }
                        break;
                    case CrossSectionType.ZW:
                        var crossSectionDefinitionZw = (CrossSectionDefinitionZW) crossSectionDefinition;
                        if (crossSectionDefinitionZw.GetSectionWidth(RoughnessDataSet.MainSectionTypeName) != crossSectionDefinitionZw.Width)
                        {
                            return true;
                        }
                        break;
                }
            }

            return false;
        }

        private static void SetDefaultRoughnessSectionDataToRemainingChannelFrictionDefinitions(
            IEnumerable<ChannelFrictionDefinition> channelFrictionDefinitions,
            RoughnessSection defaultRoughnessSection)
        {
            foreach (var channelFrictionDefinition in channelFrictionDefinitions)
            {
                var channel = channelFrictionDefinition.Channel;
                var functionType = defaultRoughnessSection.GetRoughnessFunctionType(channel);
                var networkLocations = defaultRoughnessSection.RoughnessNetworkCoverage.GetLocationsForBranch(channel);

                switch (functionType)
                {
                    case RoughnessFunction.Constant:
                        switch (networkLocations.Count)
                        {
                            case 0: // Branch Constant
                                channelFrictionDefinition.SpecificationType = ChannelFrictionSpecificationType.ConstantChannelFrictionDefinition;
                                channelFrictionDefinition.ConstantChannelFrictionDefinition.Type = defaultRoughnessSection.GetDefaultRoughnessType();
                                channelFrictionDefinition.ConstantChannelFrictionDefinition.Value = defaultRoughnessSection.GetDefaultRoughnessValue();
                                break;
                            default: // Branch Chainages - Constant (one or more locations)
                                channelFrictionDefinition.SpecificationType = ChannelFrictionSpecificationType.SpatialChannelFrictionDefinition;
                                channelFrictionDefinition.SpatialChannelFrictionDefinition.Type = GetRoughnessTypeForChannel(defaultRoughnessSection.RoughnessNetworkCoverage, channel);
                                channelFrictionDefinition.SpatialChannelFrictionDefinition.FunctionType = RoughnessFunction.Constant;
                                SetSpatialValuesToSpatialChannelFrictionDefinition(channelFrictionDefinition.SpatialChannelFrictionDefinition, defaultRoughnessSection.RoughnessNetworkCoverage, networkLocations);
                                break;
                        }

                        break;
                    case RoughnessFunction.FunctionOfQ: // Branch Chainages - Function of Q
                        channelFrictionDefinition.SpecificationType = ChannelFrictionSpecificationType.SpatialChannelFrictionDefinition;
                        channelFrictionDefinition.SpatialChannelFrictionDefinition.Type = GetRoughnessTypeForChannel(defaultRoughnessSection.RoughnessNetworkCoverage, channel);
                        channelFrictionDefinition.SpatialChannelFrictionDefinition.FunctionType = RoughnessFunction.FunctionOfQ;
                        SetFunctionValuesToSpatialChannelFrictionDefinition(channelFrictionDefinition.SpatialChannelFrictionDefinition, defaultRoughnessSection.FunctionOfQ(channel));
                        break;
                    case RoughnessFunction.FunctionOfH: // Branch Chainages - Function of H
                        channelFrictionDefinition.SpecificationType = ChannelFrictionSpecificationType.SpatialChannelFrictionDefinition;
                        channelFrictionDefinition.SpatialChannelFrictionDefinition.Type = GetRoughnessTypeForChannel(defaultRoughnessSection.RoughnessNetworkCoverage, channel);
                        channelFrictionDefinition.SpatialChannelFrictionDefinition.FunctionType = RoughnessFunction.FunctionOfH;
                        SetFunctionValuesToSpatialChannelFrictionDefinition(channelFrictionDefinition.SpatialChannelFrictionDefinition, defaultRoughnessSection.FunctionOfH(channel));
                        break;
                }

                if (networkLocations.Any())
                {
                    defaultRoughnessSection.RoughnessNetworkCoverage.Arguments[RoughnessValueComponentIndex].RemoveValues(new VariableValueFilter<INetworkLocation>(defaultRoughnessSection.RoughnessNetworkCoverage.Arguments[0], networkLocations));
                    defaultRoughnessSection.RemoveRoughnessFunctionsForBranch(channel);
                }
            }
        }

        private static RoughnessType GetRoughnessTypeForChannel(
            RoughnessNetworkCoverage roughnessNetworkCoverage,
            IChannel channel)
        {
            return roughnessNetworkCoverage.EvaluateRoughnessType(new NetworkLocation(channel, 0));
        }

        private static void SetSpatialValuesToSpatialChannelFrictionDefinition(
            SpatialChannelFrictionDefinition spatialChannelFrictionDefinition,
            RoughnessNetworkCoverage roughnessNetworkCoverage,
            IEnumerable<INetworkLocation> networkLocations)
        {
            foreach (var networkLocation in networkLocations)
            {
                spatialChannelFrictionDefinition.ConstantSpatialChannelFrictionDefinitions.Add(new ConstantSpatialChannelFrictionDefinition
                {
                    Chainage = networkLocation.Chainage,
                    Value = roughnessNetworkCoverage.EvaluateRoughnessValue(networkLocation)
                });
            }
        }

        private static void SetFunctionValuesToSpatialChannelFrictionDefinition(
            SpatialChannelFrictionDefinition spatialChannelFrictionDefinition,
            IFunction function)
        {
            var chainageArgument = function.Arguments[ChainageArgumentIndex];
            var functionArgument = function.Arguments[FunctionArgumentIndex];
            var roughnessComponent = function.Components[RoughnessValueComponentIndex];
            spatialChannelFrictionDefinition.Function.Arguments[ChainageArgumentIndex].SetValues(chainageArgument.Values);
            spatialChannelFrictionDefinition.Function.Arguments[FunctionArgumentIndex].SetValues(functionArgument.Values);
            spatialChannelFrictionDefinition.Function.Components[RoughnessValueComponentIndex].SetValues(roughnessComponent.Values);
        }
    }
}