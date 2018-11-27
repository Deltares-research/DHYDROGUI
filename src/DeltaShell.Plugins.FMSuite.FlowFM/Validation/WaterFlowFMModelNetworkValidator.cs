using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.Helpers;
using DelftTools.Hydro.Roughness;
using DelftTools.Hydro.Structures;
using DelftTools.Utils;
using DelftTools.Utils.Validation;
using DeltaShell.Plugins.FMSuite.FlowFM.Properties;
using GeoAPI.Extensions.Coverages;
using GeoAPI.Extensions.Networks;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Validation
{
    public static class WaterFlowFMModelNetworkValidator
    {
        public static string CategoryName = "Network";
        public static ValidationReport Validate(IHydroNetwork target, IDiscretization networkDiscretization = null, IEnumerable<RoughnessSection> roughnessSections = null)
        {
            var subReports = new List<ValidationReport>();
            if (target != null)
            {
                subReports.AddRange( new []
                {
                    ValidateIds(target),
                    ValidateBranches(target),
                    ValidateCrossSections(target)
                });
                //if (target.HydroNodes.Any())
                //{
                //    subReports.AddRange(new[]
                //    {
                //        WaterFlowModel1DHydroNetworkValidator.Validate(target),
                //        WaterFlowModel1DModelDataValidator.ValidateStructures(target),
                //        WaterFlowModel1DModelDataValidator.ValidateExtraResistance(target.Structures.Where(s => s is IExtraResistance)),
                //    });

                //}
                //if (target.HydroNodes.Any() && networkDiscretization != null)
                //{
                //    subReports.AddRange(new[]
                //    {
                //        WaterFlowModel1DDiscretizationValidator.Validate(networkDiscretization),
                //    });

                //}
                //if (target.HydroNodes.Any() && roughnessSections != null)
                //{
                //    subReports.AddRange(new[]
                //    {
                //        WaterFlowModel1DModelDataValidator.ValidateRoughness(target, roughnessSections),
                //    });
                //}
            }
            return new ValidationReport(CategoryName, new List<ValidationIssue>(), subReports);
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
                    var message = string.Format(Resources.WaterFlowFMModelNetworkValidator_GetBranchOrderNumbersAtNode_More_than_two_branches_with_the_same_ordernumber___0___are_connected_to_node__1___can_not_start_calculation_, orderNumberGroup.OrderNumber, node.Name);
                    yield return new ValidationIssue(node, ValidationSeverity.Error, message, network);
                }
            }
        }

        private static IEnumerable<ValidationIssue> GetBranchValidationIssues(IChannel channel, INetwork network)
        {
            if (channel.Source.Name == channel.Target.Name)
            {
                var message = string.Format(Resources.WaterFlowFMModelNetworkValidator_GetBranchValidationIssues_Target_and_source_node_of_branch___0___have_the_same_id____1____Circular_branch_, channel.Name, channel.Source.Name);
                yield return new ValidationIssue(channel, ValidationSeverity.Error, message, network);
            }

            if (channel.OrderNumber != -1 && channel.OrderNumber < 0)
            {
                var message = string.Format(Resources.WaterFlowFMModelNetworkValidator_GetBranchValidationIssues_Branch___0___has_an_order_number_of___1____Ordernumber_can_be__1__no_interpolation_over_node__or_greater_than_or_equal_to_0_,
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
                ValidationHelper.ValidateDuplicateNames(network.ObservationPoints.Cast<INameable>(), "observation points", network),
                ValidationHelper.ValidateDuplicateNames(network.Pipes.Cast<INameable>(), "pipes", network),
                ValidationHelper.ValidateDuplicateNames(network.Pumps.Cast<INameable>(), "pumps", network),
                ValidationHelper.ValidateDuplicateNames(network.Retentions.Cast<INameable>(), "retentions", network),
                ValidationHelper.ValidateDuplicateNames(network.Weirs.Cast<INameable>(), "weirs", network),
                ValidationHelper.ValidateDuplicateNames(network.Gates.Cast<INameable>(), "gates", network),

                ValidationHelper.ValidateNoEmptyNames(network.Branches.Cast<INameable>(), "branch", network),
                ValidationHelper.ValidateNoEmptyNames(network.Bridges.Cast<INameable>(), "bridge", network),
                ValidationHelper.ValidateNoEmptyNames(network.Culverts.Cast<INameable>(), "culvert", network),
                ValidationHelper.ValidateNoEmptyNames(network.CrossSections.Cast<INameable>(), "cross section", network),
                ValidationHelper.ValidateNoEmptyNames(network.ExtraResistances.Cast<INameable>(), "extra resistance", network),
                ValidationHelper.ValidateNoEmptyNames(network.Gullies.Cast<INameable>(), "gully", network),
                ValidationHelper.ValidateNoEmptyNames(network.HydroNodes.Cast<INameable>(), "node", network),
                ValidationHelper.ValidateNoEmptyNames(network.LateralSources.Cast<INameable>(), "lateral source", network),
                ValidationHelper.ValidateNoEmptyNames(network.ObservationPoints.Cast<INameable>(), "observation point", network),
                ValidationHelper.ValidateNoEmptyNames(network.Pipes.Cast<INameable>(), "pipe", network),
                ValidationHelper.ValidateNoEmptyNames(network.Pumps.Cast<INameable>(), "pump", network),
                ValidationHelper.ValidateNoEmptyNames(network.Retentions.Cast<INameable>(), "retention", network),
                ValidationHelper.ValidateNoEmptyNames(network.Weirs.Cast<INameable>(), "weir", network),
                ValidationHelper.ValidateNoEmptyNames(network.Gates.Cast<INameable>(), "gate", network)
            };

            var issues = issuesAsArray.SelectMany(iss => iss);

            return new ValidationReport("General", issues);
        }

        private static ValidationReport ValidateCrossSections(IHydroNetwork network)
        {
            var channelsCheckedOnInterpolationBranches = new HashSet<string>();
            var issues = network == null
                ? Enumerable.Empty<ValidationIssue>()
                : network.Channels.SelectMany(b => GetCrossSectionValidationIssues(b, network, channelsCheckedOnInterpolationBranches)).ToList();

            return new ValidationReport("Cross sections", issues);
        }

        public static IEnumerable<ValidationIssue> GetCrossSectionValidationIssues(IChannel channel, IHydroNetwork network, HashSet<string> channelsCheckedOnInterpolationBranches)
        {
            if (!network.CrossSections.Any())
            {
                yield return new ValidationIssue("CrossSection", ValidationSeverity.Warning, Resources.WaterFlowFMModelNetworkValidator_GetCrossSectionValidationIssues_No_CrossSection_defined__all_channels_will_be_using_the_default_values_, network);
            }
            
            foreach (var issue in channel.CrossSections.SelectMany(cs => GetCorrectCrossSectionIssue(cs, network)))
            {
                yield return issue;
            }

            if (channel.OrderNumber == -1)
            {
                foreach (var issue in GetCorrectCrossSectionsOnChannelIssue(new[] {channel}, network))
                {
                    yield return issue;
                }
            }
            else
            {
                if (!channelsCheckedOnInterpolationBranches.Contains(channel.Name))
                {
                    var chainOfChannels = GetChainOfChannelsWithSameOrderNumber(channel, network).ToList();

                    foreach (var issue in GetCorrectCrossSectionsOnChannelIssue(chainOfChannels, network))
                    {
                        yield return issue;
                    }

                    chainOfChannels.All(c => channelsCheckedOnInterpolationBranches.Add(c.Name));
                }
            }
        }

        private static IEnumerable<ValidationIssue> GetCorrectCrossSectionsOnChannelIssue(IList<IChannel> chainOfChannels, IHydroNetwork network)
        {
            var crossSections = chainOfChannels.SelectMany(c => c.CrossSections);
            var crossSectionTypes = crossSections.Select(cs => cs.CrossSectionType).Distinct().ToList();

            // Standard CS are sent as ZW to the modelApi at the moment, so we can consider them as the same
            if ((crossSectionTypes.Contains(CrossSectionType.GeometryBased) || crossSectionTypes.Contains(CrossSectionType.YZ)) &&
                (crossSectionTypes.Contains(CrossSectionType.Standard) || crossSectionTypes.Contains(CrossSectionType.ZW)))
            {
                var msg = string.Format(Resources.WaterFlowFMModelNetworkValidator_GetCorrectCrossSectionsOnChannelIssue_Multiple_cross_section_types__mix_of_Standard_ZW_and_Geometry_YZ__per_branch_es__not_supported___0__, string.Join(",", chainOfChannels.Select(c => c.Name).ToArray()));
                yield return new ValidationIssue(chainOfChannels.First(), ValidationSeverity.Error, msg, network);
            }
        }

        private static IEnumerable<ValidationIssue> GetCorrectCrossSectionIssue(ICrossSection crossSection, IHydroNetwork network)
        {
            string errorMessage;

            if (!CrossSectionValidator.IsCrossSectionAllowedOnBranch((CrossSection)crossSection, out errorMessage))
            {
                yield return new ValidationIssue(crossSection, ValidationSeverity.Error, errorMessage, network);
            }

            if (crossSection.Geometry.Coordinates.Length == 0)
            {
                yield return new ValidationIssue(crossSection, ValidationSeverity.Error, "No profile defined", network);
            }

            if (!CrossSectionValidator.IsFlowProfileValid(crossSection.Definition))
            {
                if (crossSection.Definition.CrossSectionType == CrossSectionType.ZW)
                {
                    yield return new ValidationIssue(crossSection, ValidationSeverity.Error,
                        String.Format("tabulated cross section {0} cannot have zero width at levels above deepest point of its definition.", crossSection));
                }
                else
                {
                    yield return new ValidationIssue(crossSection, ValidationSeverity.Error, "Invalid flow profile", network);
                }
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