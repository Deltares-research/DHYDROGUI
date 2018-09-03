using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.Helpers;
using DelftTools.Utils;
using DelftTools.Utils.Validation;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.Properties;
using GeoAPI.Extensions.Networks;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.Validation
{
    public static class WaterFlowModel1DHydroNetworkValidator
    {
        public static ValidationReport Validate(IHydroNetwork target)
        {
            return new ValidationReport("Network", new List<ValidationIssue>(),
                                        new[]
                                            {
                                                ValidateCoordinateSystem(target),
                                                ValidateIds(target),
                                                ValidateCulverts(target),
                                                ValidateBranches(target),
                                                ValidateCrossSections(target)
                                            });
        }

        private static ValidationReport ValidateCulverts(IHydroNetwork target)
        {
            var issues = new List<ValidationIssue>();
            foreach (var culvert in target.Culverts)
            {
                if (culvert.IsGated && (culvert.GateOpeningLossCoefficientFunction == null || culvert.GateOpeningLossCoefficientFunction.Components.Any(c => c.Values.Count == 0)))
                {
                    issues.Add(new ValidationIssue(culvert, ValidationSeverity.Error,
                                                string.Format("Culvert {0} is gated and has no gateopening losscoefficient datatable",
                                                    culvert.Name), culvert));
                }
            }

            if (issues.Count == 0)
            {
                issues.Add(new ValidationIssue(target, ValidationSeverity.Info, "No error found for culvert"));
            }

            return new ValidationReport("Culverts", issues);
        }

        private static ValidationReport ValidateCoordinateSystem(IHydroNetwork target)
        {
            if (target.CoordinateSystem == null)
            {
                var issue = new ValidationIssue(target.CoordinateSystem, ValidationSeverity.Warning,
                    string.Format(
                        "No Coordinate System selected for Network. Default map projection will be used for distance calculations."), target.CoordinateSystem);

                return new ValidationReport("Coordinate system", new[] { issue });
            }

            if (target.CoordinateSystem != null && target.CoordinateSystem.IsGeographic)
            {
                var issue = new ValidationIssue(target.CoordinateSystem, ValidationSeverity.Error,
                                                string.Format(
                                                    "Cannot perform calculation in geographical coordinate system {0}",
                                                    target.CoordinateSystem.Name), target.CoordinateSystem);

                return new ValidationReport("Coordinate system", new[] {issue});
            }

            return new ValidationReport("Coordinate system", Enumerable.Empty<ValidationIssue>());
        }

        private static ValidationReport ValidateBranches(IHydroNetwork network)
        {
            var issues = network == null
                             ? Enumerable.Empty<ValidationIssue>()
                             : network.Channels.SelectMany(b => GetBranchValidationIssues(b, network)).ToList();

            var nodeIssues = network == null
                 ? Enumerable.Empty<ValidationIssue>()
                 : network.Nodes.SelectMany(n => GetBranchOrderNumbersAtNode(n, network)).ToList();

            return new ValidationReport("Branches", issues.Concat(nodeIssues));
        }

        private static IEnumerable<ValidationIssue> GetBranchOrderNumbersAtNode(INode node, INetwork network)
        {
            var mergedBranchesList = node.IncomingBranches.Concat(node.OutgoingBranches).ToList();
            var groupedBranchesPerOrderNumber = mergedBranchesList.GroupBy(b => b.OrderNumber).Select(b => new { OrderNumber = b.Key, Count = b.Count() });
            foreach (var orderNumberGroup in groupedBranchesPerOrderNumber)
            {
                if (orderNumberGroup.OrderNumber > 0 && orderNumberGroup.Count > 2)
                {
                    var message = string.Format("More than two branches with the same ordernumber '{0}' are connected to node {1}; can not start calculation.", orderNumberGroup.OrderNumber, node.Name);
                    yield return new ValidationIssue(node, ValidationSeverity.Error, message, network); 
                }
            }
        }

        private static IEnumerable<ValidationIssue> GetBranchValidationIssues(IChannel channel, INetwork network)
        {
            if (channel.Source.Name == channel.Target.Name)
            {
                var message = string.Format("Target and source node of branch '{0}' have the same id, '{1}'. Circular branch?", channel.Name, channel.Source.Name);
                yield return new ValidationIssue(channel, ValidationSeverity.Error, message, network);
            }

            if (channel.OrderNumber != -1 && channel.OrderNumber < 0)
            {
                var message = string.Format("Branch '{0}' has an order number of '{1}'. Ordernumber can be -1 (no interpolation over node) or greater than or equal to 0 ",
                        channel.Name, channel.OrderNumber);
                yield return new ValidationIssue(channel, ValidationSeverity.Error, message, network);
            }
        }

        private static ValidationReport ValidateIds(IHydroNetwork network)
        {
            var issuesAsArray = new[]
                                    {
                                        ValidationHelper.ValidateDuplicateNames(network.Branches.Cast<INameable>(), "branches", network),
                                        ValidationHelper.ValidateDuplicateNames(network.Bridges.Cast<INameable>(), "bridges", network),
                                        ValidationHelper.ValidateDuplicateNames(network.Culverts.Cast<INameable>(), "culverts", network),
                                        ValidationHelper.ValidateDuplicateNames(network.CrossSections.Cast<INameable>(), "cross sections", network),
                                        ValidationHelper.ValidateDuplicateNames(network.ExtraResistances.Cast<INameable>(), "extra resistances", network),
                                        ValidationHelper.ValidateDuplicateNames(network.Gullies.Cast<INameable>(), "gullies", network),
                                        ValidationHelper.ValidateDuplicateNames(network.HydroNodes.Cast<INameable>(), "nodes", network),
                                        ValidationHelper.ValidateDuplicateNames(network.LateralSources.Cast<INameable>(), "lateral sources", network),
                                        ValidationHelper.ValidateDuplicateNames(network.Manholes.Cast<INameable>(), "manholes", network),
                                        ValidationHelper.ValidateDuplicateNames(network.ObservationPoints.Cast<INameable>(), "observation points", network),
                                        ValidationHelper.ValidateDuplicateNames(network.Pipes.Cast<INameable>(), "pipes", network),
                                        ValidationHelper.ValidateDuplicateNames(network.Pumps.Cast<INameable>(), "pumps", network),
                                        ValidationHelper.ValidateDuplicateNames(network.Retentions.Cast<INameable>(), "retentions", network),
                                        ValidationHelper.ValidateDuplicateNames(network.Weirs.Cast<INameable>(), "weirs", network),
                                        ValidationHelper.ValidateDuplicateNames(network.Gates.Cast<INameable>(), "gates", network),
                                        ValidationHelper.ValidateDuplicateNames(network.CompositeBranchStructures.Cast<INameable>(), "composite branch structures", network),

                                        ValidationHelper.ValidateNoEmptyNames(network.Branches.Cast<INameable>(), "branch", network),
                                        ValidationHelper.ValidateNoEmptyNames(network.Bridges.Cast<INameable>(), "bridge", network),
                                        ValidationHelper.ValidateNoEmptyNames(network.Culverts.Cast<INameable>(), "culvert", network),
                                        ValidationHelper.ValidateNoEmptyNames(network.CrossSections.Cast<INameable>(), "cross section", network),
                                        ValidationHelper.ValidateNoEmptyNames(network.ExtraResistances.Cast<INameable>(), "extra resistance", network),
                                        ValidationHelper.ValidateNoEmptyNames(network.Gullies.Cast<INameable>(), "gully", network),
                                        ValidationHelper.ValidateNoEmptyNames(network.HydroNodes.Cast<INameable>(), "node", network),
                                        ValidationHelper.ValidateNoEmptyNames(network.LateralSources.Cast<INameable>(), "lateral source", network),
                                        ValidationHelper.ValidateNoEmptyNames(network.Manholes.Cast<INameable>(), "manhole", network),
                                        ValidationHelper.ValidateNoEmptyNames(network.ObservationPoints.Cast<INameable>(), "observation point", network),
                                        ValidationHelper.ValidateNoEmptyNames(network.Pipes.Cast<INameable>(), "pipe", network),
                                        ValidationHelper.ValidateNoEmptyNames(network.Pumps.Cast<INameable>(), "pump", network),
                                        ValidationHelper.ValidateNoEmptyNames(network.Retentions.Cast<INameable>(), "retention", network),
                                        ValidationHelper.ValidateNoEmptyNames(network.Weirs.Cast<INameable>(), "weir", network),
                                        ValidationHelper.ValidateNoEmptyNames(network.Gates.Cast<INameable>(), "gate", network),
                                        ValidationHelper.ValidateNoEmptyNames(network.CompositeBranchStructures.Cast<INameable>(), "composite branch structure", network)
                                    };

            var issues = issuesAsArray.SelectMany(iss => iss);

            return new ValidationReport("General", issues);
        }
        
        private static ValidationReport ValidateCrossSections(IHydroNetwork network)
        {
            var channelsCheckedOnInterpolationBranches = new HashSet<string>();
            var issues = network?.Channels.SelectMany(b => GetCrossSectionValidationIssues(b, network, channelsCheckedOnInterpolationBranches)).ToList() ?? new List<ValidationIssue>();

            var issuesContainSectionIssues = issues.Where(i => i.Message.Equals(Resources.WaterFlowModel1DHydroNetworkValidator_ValidateCrossSections_The_maximum_flow_width_of_this_cross_section_does_not_match_the_total_width_of_all_its_sections_)).ToList();
            var finalIssues = issues.Where(i => !i.Message.Equals(Resources.WaterFlowModel1DHydroNetworkValidator_ValidateCrossSections_The_maximum_flow_width_of_this_cross_section_does_not_match_the_total_width_of_all_its_sections_)).ToList();
            if (issuesContainSectionIssues.Any())
            {
                var crossSectionsToCorrect = issuesContainSectionIssues.Select(issue => issue.ViewData as ICrossSection).ToList();
                finalIssues.Add(new ValidationIssue($"Cross section sections issues ({crossSectionsToCorrect.Count})", ValidationSeverity.Error,
                    Resources.WaterFlowModel1DHydroNetworkValidator_ValidateCrossSections_The_maximum_flow_width_of_one_or_more_cross_sections_is_larger_than_the_total_width_of_all_its_sections_, crossSectionsToCorrect));
            }

            return new ValidationReport("Cross sections", finalIssues);
        }

        private static IEnumerable<ValidationIssue> GetCrossSectionValidationIssues(IChannel channel, IHydroNetwork network, HashSet<string> channelsCheckedOnInterpolationBranches)
        {
            foreach (var issue in channel.CrossSections.SelectMany(cs => GetCorrectCrossSectionIssue(cs, network)))
            {
                yield return issue;
            }

            if (channel.OrderNumber == -1)
            {
                foreach(var issue in GetCorrectCrossSectionsOnChannelIssue(new[] { channel }, network))
                {
                    yield return issue;
                }
            }
            else
            {
                if (!channelsCheckedOnInterpolationBranches.Contains(channel.Name))
                {
                    var chainOfChannels = GetChainOfChannelsWithSameOrderNumber(channel, network);

                    foreach (var issue in GetCorrectCrossSectionsOnChannelIssue(chainOfChannels, network))
                    {
                        yield return issue;
                    }

                    chainOfChannels.All(c => channelsCheckedOnInterpolationBranches.Add(c.Name));
                }
            }
        }

        private static IEnumerable<ValidationIssue> GetCorrectCrossSectionsOnChannelIssue(IEnumerable<IChannel> chainOfChannels, IHydroNetwork network)
        {
            var crossSections = chainOfChannels.SelectMany(c => c.CrossSections);
            var crossSectionTypes = crossSections.Select(cs => cs.CrossSectionType).Distinct();
            
            if (!crossSectionTypes.Any())
            {
                var message = string.Format("No cross sections on channel(s) {0}; can not start calculation.", string.Join(",", chainOfChannels.Select(c => c.Name).ToArray()));
                yield return new ValidationIssue(chainOfChannels.First(), ValidationSeverity.Error, message, network);
            }

            // Standard CS are sent as ZW to the modelApi at the moment, so we can consider them as the same
            if ((crossSectionTypes.Contains(CrossSectionType.GeometryBased) || crossSectionTypes.Contains(CrossSectionType.YZ)) &&
                (crossSectionTypes.Contains(CrossSectionType.Standard) || crossSectionTypes.Contains(CrossSectionType.ZW)))
            {
                var msg = string.Format("Multiple cross-section-types (mix of Standard/ZW and Geometry/YZ) per branch(es) not supported.({0})", string.Join(",",chainOfChannels.Select(c => c.Name).ToArray()));
                yield return new ValidationIssue(chainOfChannels.First(), ValidationSeverity.Error, msg, network);
            }
        }

        private static IEnumerable<ValidationIssue> GetCorrectCrossSectionIssue(ICrossSection crossSection, IHydroNetwork network)
        {
            string errorMessage;
            var crossSectionDefinition = crossSection.Definition;

            if (!CrossSectionValidator.IsCrossSectionAllowedOnBranch((CrossSection) crossSection, out errorMessage))
            {
                yield return new ValidationIssue(crossSection, ValidationSeverity.Error, errorMessage, network);
            }

            if (crossSection.Geometry.Coordinates.Length == 0)
            {
                yield return new ValidationIssue(crossSection, ValidationSeverity.Error, Resources.WaterFlowModel1DHydroNetworkValidator_GetCorrectCrossSectionIssue_No_profile_defined, network);
            }

            if (!CrossSectionValidator.IsFlowProfileValid(crossSectionDefinition))
            {
                if (crossSectionDefinition.CrossSectionType == CrossSectionType.ZW)
                {
                    yield return new ValidationIssue(crossSection, ValidationSeverity.Error,  
                        string.Format(Resources.WaterFlowModel1DHydroNetworkValidator_GetCorrectCrossSectionIssue_Tabulated_cross_section__0__cannot_have_zero_width_at_levels_above_deepest_point_of_its_definition_, crossSection));
                }
                else
                {
                    yield return new ValidationIssue(crossSection, ValidationSeverity.Error, Resources.WaterFlowModel1DHydroNetworkValidator_GetCorrectCrossSectionIssue_Invalid_flow_profile, network);
                }
            }

            if (!CrossSectionValidator.AreCrossSectionsLengthsLargerThanTheFlowWidth(crossSectionDefinition))
            {
                yield return new ValidationIssue(crossSection, ValidationSeverity.Error,
                    Resources.WaterFlowModel1DHydroNetworkValidator_ValidateCrossSections_The_maximum_flow_width_of_this_cross_section_does_not_match_the_total_width_of_all_its_sections_, crossSection);
            }

            if (!CrossSectionValidator.AreFloodPlain1AndFloodPlain2WidthsValid(crossSectionDefinition))
            {
                yield return new ValidationIssue(crossSection, ValidationSeverity.Error,
                    Resources.WaterFlowModel1DHydroNetworkValidator_GetCorrectCrossSectionIssue_FloodPlain2_width_may_not_be_larger_than_zero_if_FloodPlain1_width_is_equal_to_zero_, crossSection);
            }
        }

        private static IEnumerable<IChannel> GetChainOfChannelsWithSameOrderNumber(IChannel channel, IHydroNetwork network, IChannel previousLink = null)
        {
            var startNode = channel.Source;
            var channelTo = network.Channels.FirstOrDefault(c => c.OrderNumber == channel.OrderNumber && c.Target == startNode);

            if (channelTo != null && channelTo != previousLink)
            {
                yield return channelTo;
                foreach (var channelInChain in GetChainOfChannelsWithSameOrderNumber(channelTo, network, channel))
                {
                    yield return channelInChain;
                }
                
            }

            var endNode = channel.Target;
            var channelFrom = network.Channels.FirstOrDefault(c => c.OrderNumber == channel.OrderNumber && c.Source == endNode);

            if (channelFrom != null && channelFrom != previousLink)
            {
                yield return channelFrom;

                foreach (var channelInChain in GetChainOfChannelsWithSameOrderNumber(channelFrom, network, channel))
                {
                    yield return channelInChain;
                }
            }

            if (previousLink == null)
            {
                yield return channel;
            }
        }
    }
}