using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.Roughness;
using DelftTools.Utils.Validation;
using DeltaShell.NGHS.IO.DataObjects.Friction;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Validation
{
    public static class WaterFlowFMRoughnessValidator
    {
        public static ValidationReport Validate(WaterFlowFMModel model)
        {
            IList<ValidationIssue> issues = new List<ValidationIssue>();

            ValidateForMissingOrInvalidChannelFrictionData(model, issues);
            ValidateForConflictingChannelFrictionSpecifications(model, issues);

            return new ValidationReport("Roughness", issues);
        }

        private static void ValidateForMissingOrInvalidChannelFrictionData(WaterFlowFMModel model, ICollection<ValidationIssue> issues)
        {
            foreach (var channelFrictionDefinition in model.ChannelFrictionDefinitions.Where(cfd => cfd.SpecificationType == ChannelFrictionSpecificationType.SpatialChannelFrictionDefinition))
            {
                var channel = channelFrictionDefinition.Channel;
                var spatialChannelFrictionDefinition = channelFrictionDefinition.SpatialChannelFrictionDefinition;

                switch (spatialChannelFrictionDefinition.FunctionType)
                {
                    case RoughnessFunction.Constant:
                        var constantSpatialChannelFrictionDefinitions = spatialChannelFrictionDefinition.ConstantSpatialChannelFrictionDefinitions;
                        if (constantSpatialChannelFrictionDefinitions.Count == 0)
                        {
                            issues.Add(new ValidationIssue(channel,
                                ValidationSeverity.Error,
                                $"No '{RoughnessFunction.Constant}' values defined",
                                model.ChannelFrictionDefinitions));

                        }
                        else
                        {
                            var invalidChainages = constantSpatialChannelFrictionDefinitions
                                .Select(ccfd => ccfd.Chainage)
                                .Where(chainage => chainage < 0 || chainage > channel.Length)
                                .ToArray();

                            if (invalidChainages.Any())
                            {
                                issues.Add(new ValidationIssue(channel,
                                    ValidationSeverity.Error,
                                    $"One or more '{RoughnessFunction.Constant}' values are invalid regarding their 'Chainage'. The chainages involved are: " +
                                    $"{string.Join(", ", invalidChainages)}.",
                                    model.ChannelFrictionDefinitions));
                            }
                        }
                        break;
                    case RoughnessFunction.FunctionOfQ:
                        var functionOfQ = spatialChannelFrictionDefinition.Function;
                        if (functionOfQ.GetValues().Count == 0)
                        {
                            issues.Add(new ValidationIssue(channel,
                                ValidationSeverity.Error,
                                $"No '{RoughnessFunction.FunctionOfQ}' values defined",
                                model.ChannelFrictionDefinitions));
                        }
                        break;
                    case RoughnessFunction.FunctionOfH:
                        var functionOfH = spatialChannelFrictionDefinition.Function;
                        if (functionOfH.GetValues().Count == 0)
                        {
                            issues.Add(new ValidationIssue(channel,
                                ValidationSeverity.Error,
                                $"No '{RoughnessFunction.FunctionOfH}' values defined",
                                model.ChannelFrictionDefinitions));
                        }
                        break;
                }
            }
        }

        private static void ValidateForConflictingChannelFrictionSpecifications(WaterFlowFMModel model, ICollection<ValidationIssue> issues)
        {
            var channelFrictionDefinitionPerChannelLookup = model.ChannelFrictionDefinitions.ToDictionary(cfd => cfd.Channel, cfd => cfd);
            var channelsPerCrossSectionDefinitionLookup = model.Network.GetChannelsPerCrossSectionDefinitionLookup();

            foreach (var sharedCrossSectionDefinition in model.Network.SharedCrossSectionDefinitions)
            {
                if (!channelsPerCrossSectionDefinitionLookup.TryGetValue(sharedCrossSectionDefinition, out var channelsUsingSharedCrossSectionDefinition))
                {
                    continue; // Skip shared cross section definitions that are unused
                }

                var relatedChannelFrictionDefinitions = channelsUsingSharedCrossSectionDefinition.Select(c => channelFrictionDefinitionPerChannelLookup[c]).ToArray();

                if (relatedChannelFrictionDefinitions.All(cfd => cfd.SpecificationType == ChannelFrictionSpecificationType.RoughnessSections
                                                                 || cfd.SpecificationType == ChannelFrictionSpecificationType.CrossSectionFrictionDefinitions))
                {
                    continue;
                }

                if (relatedChannelFrictionDefinitions.All(cfd => cfd.SpecificationType != ChannelFrictionSpecificationType.RoughnessSections
                                                                 && cfd.SpecificationType != ChannelFrictionSpecificationType.CrossSectionFrictionDefinitions))
                {
                    continue;
                }

                issues.Add(new ValidationIssue(sharedCrossSectionDefinition,
                    ValidationSeverity.Error,
                    "This shared cross section definition is used on branches that have a conflicting " +
                    "roughness Specification type. The branches involved are: " +
                    $"{string.Join(", ", channelsUsingSharedCrossSectionDefinition.Select(c => c.Name))}.",
                    model.ChannelFrictionDefinitions));
            }
        }
    }
}