using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Functions;
using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.Helpers;
using DelftTools.Hydro.Properties;
using DelftTools.Hydro.SewerFeatures;
using DelftTools.Hydro.Structures;
using DelftTools.Utils.Validation;
using Deltares.Infrastructure.API.Guards;
using GeoAPI.Extensions.Feature;
using GeoAPI.Extensions.Networks;

namespace DelftTools.Hydro.Validators
{
    /// <summary>
    /// Validator for <see cref="IHydroNetwork"/>.
    /// </summary>
    public static class HydroNetworkValidator
    {
        /// <summary>
        /// Validate the given <see cref="IHydroNetwork"/>.
        /// </summary>
        /// <param name="target">The hydro network to validate.</param>
        /// <returns>A <see cref="ValidationReport"/> containing the results of the validation.</returns>
        public static ValidationReport Validate(IHydroNetwork target)
        {
            Ensure.NotNull(target, nameof(target));

            var subReports = new List<ValidationReport>();

            if (target.HydroNodes.Any())
            {
                subReports.AddRange(new[]
                {
                    ValidateCoordinateSystem(target),
                    ValidateIds(target),
                    ValidateCulverts(target),
                    ValidateBranches(target),
                    ValidateCrossSections(target),
                    ValidateRetentions(target),
                    StructuresValidator.Validate(target),
                });
            }

            if (target.Manholes.Any())
            {
                subReports.AddRange(new[]
                {
                    ValidateManholes(target),
                    ValidateCompartments(target),
                    ValidateSewerConnections(target)
                });
            }

            if (target.Routes.Any())
            {
                subReports.Add(ValidateRoutes(target));
            }

            return new ValidationReport("Network", new List<ValidationIssue>(), subReports);
        }

        private static ValidationReport ValidateRoutes(IHydroNetwork network)
        {
            var issues = new List<ValidationIssue>();
            
            issues.AddRange(ValidationHelper.ValidateDuplicateNames(network.Routes, "routes", network));
            
            return new ValidationReport("Routes", issues);
        }

        private static ValidationReport ValidateRetentions(IHydroNetwork network)
        {
            var issues = new List<ValidationIssue>();
            foreach (IRetention retention in network.Retentions)
            {
                if (!retention.UseTable && Math.Abs(retention.StorageArea) < double.Epsilon)
                {
                    issues.Add(new ValidationIssue(retention,
                                                   ValidationSeverity.Error,
                                                   string.Format(Resources.HydroNetworkValidator_Values_storage_graph_retention_should_be_greater_than_zero, retention.Name)));
                }

                IFunction retentionTableData = retention.Data;
                if (retention.UseTable && (retentionTableData?.Components?[0] == null || retentionTableData.Components[0].Values.Count == 0))
                {
                    issues.Add(new ValidationIssue(retention,
                                                   ValidationSeverity.Error,
                                                   string.Format(Resources.HydroNetworkValidator_Table_should_be_used_for_retention, retention.Name)));
                    continue;
                }

                if (retention.UseTable)
                {
                    double[] levelStorageValues = retentionTableData.Components[0].GetValues<double>().ToArray();
                    double[] levelStorageHeigth = retentionTableData.Arguments[0].GetValues<double>().ToArray();
                    if (levelStorageValues.Length != levelStorageHeigth.Length)
                    {
                        continue;
                    }

                    for (var index = 0; index < levelStorageValues.Length; index++)
                    {
                        double value = levelStorageValues[index];
                        double height = levelStorageHeigth[index];
                        if (Math.Abs(value) < double.Epsilon)
                        {
                            issues.Add(new ValidationIssue(retention,
                                                           ValidationSeverity.Error,
                                                           string.Format(Resources.HydroNetworkValidator_Table_is_used_for_retention_but_wrong_value_at_specific_height,
                                                                         retention.Name, height, value),
                                                           retentionTableData));
                        }
                    }
                }
            }

            return new ValidationReport("Retentions", issues);
        }

        private static ValidationReport ValidateCulverts(IHydroNetwork target)
        {
            var issues = new List<ValidationIssue>();
            foreach (ICulvert culvert in target.Culverts)
            {
                if (culvert.IsGated && (culvert.GateOpeningLossCoefficientFunction == null || culvert.GateOpeningLossCoefficientFunction.Components.Any(c => c.Values.Count == 0)))
                {
                    issues.Add(new ValidationIssue(culvert,
                                                   ValidationSeverity.Error,
                                                   string.Format(string.Format(Resources.HydroNetworkValidator_Gated_culvert_has_no_gate_opening_table), culvert.Name),
                                                   culvert));
                }
            }

            return new ValidationReport("Culverts", issues);
        }

        private static ValidationReport ValidateCoordinateSystem(IHydroNetwork target)
        {
            if (target.CoordinateSystem == null)
            {
                var issue = new ValidationIssue(target.CoordinateSystem, ValidationSeverity.Warning,
                                                Resources.HydroNetworkValidator_No_coordinate_system_selected_for_network,
                                                target.CoordinateSystem);

                return new ValidationReport("Network Coordinate system", new[]
                {
                    issue
                });
            }

            if (target.CoordinateSystem != null && target.CoordinateSystem.IsGeographic)
            {
                var issue = new ValidationIssue(target.CoordinateSystem, ValidationSeverity.Error,
                                                string.Format(
                                                    Resources.HydroNetworkValidator_Cannot_perform_calculation_in_geographical_coordinate_system,
                                                    target.CoordinateSystem.Name), target.CoordinateSystem);

                return new ValidationReport("Coordinate system", new[]
                {
                    issue
                });
            }

            return new ValidationReport("Coordinate system", Enumerable.Empty<ValidationIssue>());
        }

        private static ValidationReport ValidateBranches(IHydroNetwork network)
        {
            IEnumerable<ValidationIssue> issues = network?.Channels
                                                         .SelectMany(b => GetBranchValidationIssues(b))
                                                         .ToList()
                                                  ?? Enumerable.Empty<ValidationIssue>();

            IEnumerable<ValidationIssue> nodeIssues = network?.Nodes
                                                             .SelectMany(n => GetBranchOrderNumbersAtNode(n, network))
                                                             .ToList()
                                                      ?? Enumerable.Empty<ValidationIssue>();

            return new ValidationReport("Branches", issues.Concat(nodeIssues));
        }

        private static IEnumerable<ValidationIssue> GetBranchOrderNumbersAtNode(INode node, INetwork network)
        {
            List<IBranch> mergedBranchesList = node.IncomingBranches.Concat(node.OutgoingBranches).ToList();
            IEnumerable<IGrouping<int, IBranch>> groupedBranchesPerOrderNumber = mergedBranchesList.GroupBy(b => b.OrderNumber);
            foreach (IGrouping<int, IBranch> orderNumberGroup in groupedBranchesPerOrderNumber)
            {
                int orderNumber = orderNumberGroup.Key;
                if (orderNumber > 0 && orderNumberGroup.Count() > 2)
                {
                    string message = string.Format(Resources.HydroNetworkValidator_Multiple_branches_with_same_order_number_connected_to_single_node, orderNumber, node.Name);
                    yield return new ValidationIssue(node, ValidationSeverity.Error, message, new ValidatedFeatures(network, new List<IFeature>(orderNumberGroup) { node }.ToArray()));
                }
            }
        }

        private static IEnumerable<ValidationIssue> GetBranchValidationIssues(IChannel channel)
        {
            if (channel.Source.Name == channel.Target.Name)
            {
                string message = string.Format(Resources.HydroNetworkValidator_Target_and_source_node_of_branch_have_same_id, channel.Name, channel.Source.Name);
                yield return new ValidationIssue(channel, ValidationSeverity.Error, message, new ValidatedFeatures(channel.Network, channel));
            }

            if (channel.OrderNumber != -1 && channel.OrderNumber < 0)
            {
                string message = string.Format(Resources.HydroNetworkValidator_Branch_has_invalid_order_number, channel.Name, channel.OrderNumber);
                yield return new ValidationIssue(channel, ValidationSeverity.Error, message, new ValidatedFeatures(channel.Network, channel));
            }
        }

        private static ValidationReport ValidateIds(IHydroNetwork network)
        {
            List<ICrossSectionDefinition> sharedCrossSectionDefinitions = network.SharedCrossSectionDefinitions.ToList();
            List<ICrossSectionDefinition> crossSectionDefinitions = GetCrossSectionDefinitions(network.CrossSections.Select(cs => cs.Definition),
                                                                                               sharedCrossSectionDefinitions).ToList();

            IList<ValidationIssue>[] issuesAsArray =
            {
                ValidationHelper.ValidateDuplicateNames(network.Branches, "branches", network),
                ValidationHelper.ValidateDuplicateNames(network.Bridges, "bridges", network),
                ValidationHelper.ValidateDuplicateNames(network.Culverts, "culverts", network),
                ValidationHelper.ValidateDuplicateNames(network.CrossSections, "cross sections", network),
                ValidationHelper.ValidateDuplicateNames(crossSectionDefinitions, "cross section definitions", network),
                ValidationHelper.ValidateDuplicateNames(network.HydroNodes, "nodes", network),
                ValidationHelper.ValidateDuplicateNames(network.Manholes, "manholes", network),
                ValidationHelper.ValidateDuplicateNames(network.LateralSources, "lateral sources", network),
                ValidationHelper.ValidateDuplicateNames(network.ObservationPoints, "observation points", network),
                ValidationHelper.ValidateDuplicateNames(network.Pipes, "pipes", network),
                ValidationHelper.ValidateDuplicateNames(network.Pumps, "pumps", network),
                ValidationHelper.ValidateDuplicateNames(network.Retentions, "retentions", network),
                ValidationHelper.ValidateDuplicateNames(network.Weirs, "weirs", network),
                ValidationHelper.ValidateDuplicateNames(network.Gates, "gates", network),
                ValidationHelper.ValidateDuplicateNames(network.CompositeBranchStructures, "composite branch structures", network),

                ValidationHelper.ValidateNoEmptyNames(network.Branches, "branch", network),
                ValidationHelper.ValidateNoEmptyNames(network.Bridges, "bridge", network),
                ValidationHelper.ValidateNoEmptyNames(network.Culverts, "culvert", network),
                ValidationHelper.ValidateNoEmptyNames(network.CrossSections, "cross section", network),
                ValidationHelper.ValidateNoEmptyNames(crossSectionDefinitions, "cross section definition", network),
                ValidationHelper.ValidateNoEmptyNames(network.HydroNodes, "node", network),
                ValidationHelper.ValidateNoEmptyNames(network.LateralSources, "lateral source", network),
                ValidationHelper.ValidateNoEmptyNames(network.Manholes, "manholes", network),
                ValidationHelper.ValidateNoEmptyNames(network.ObservationPoints, "observation point", network),
                ValidationHelper.ValidateNoEmptyNames(network.Pipes, "pipe", network),
                ValidationHelper.ValidateNoEmptyNames(network.Pumps, "pump", network),
                ValidationHelper.ValidateNoEmptyNames(network.Retentions, "retention", network),
                ValidationHelper.ValidateNoEmptyNames(network.Weirs, "weir", network),
                ValidationHelper.ValidateNoEmptyNames(network.Gates, "gate", network),
                ValidationHelper.ValidateNoEmptyNames(network.CompositeBranchStructures, "composite branch structure", network)
            };

            IEnumerable<ValidationIssue> issues = issuesAsArray.SelectMany(iss => iss);

            return new ValidationReport("General", issues);
        }

        private static IEnumerable<ICrossSectionDefinition> GetCrossSectionDefinitions(IEnumerable<ICrossSectionDefinition> crossSectionDefinitions,
                                                                                       IReadOnlyCollection<ICrossSectionDefinition> sharedDefinitions)
        {
            // We have to validate whether there are any duplicate ids for cross-section definitions.
            // This can, for example, occur whenever two cross-sections both have a definition with the same name,
            // but the two definitions are not the same instance.
            // In order to validate this we have to construct a list of ICrossSectionDefinitions where each shared definitions
            // is added only once and all non-shared definitions are added.
            
            Dictionary<string, ICrossSectionDefinition> sharedDefinitionsLookup = sharedDefinitions.ToDictionary(def => def.Name, StringComparer.InvariantCultureIgnoreCase);

            IEnumerable<ICrossSectionDefinition> nonSharedCrossSections = crossSectionDefinitions.Where(csd => csd is CrossSectionDefinitionProxy == false
                                                                                                               && (!sharedDefinitionsLookup.ContainsKey(csd.Name)
                                                                                                                   || !sharedDefinitionsLookup[csd.Name].Equals(csd)));

            return sharedDefinitions.Concat(nonSharedCrossSections);
        }

        private static ValidationReport ValidateCrossSections(IHydroNetwork network)
        {
            var channelsCheckedOnInterpolationBranches = new HashSet<string>();
            List<ValidationIssue> issues = network?.Channels.SelectMany(b => GetCrossSectionValidationIssues(b, network, channelsCheckedOnInterpolationBranches)).ToList() ?? new List<ValidationIssue>();

            List<ValidationIssue> issuesContainSectionIssues = issues.Where(i => i.Message.Equals(Resources.HydroNetworkValidator_Maximum_flow_width_of_cross_section_does_not_match_total_width)).ToList();
            List<ValidationIssue> finalIssues = issues.Where(i => !i.Message.Equals(Resources.HydroNetworkValidator_Maximum_flow_width_of_cross_section_does_not_match_total_width)).ToList();
            if (issuesContainSectionIssues.Any())
            {
                List<ICrossSection> crossSectionsToCorrect = issuesContainSectionIssues.Select(issue => issue.ViewData as ICrossSection).ToList();
                finalIssues.Add(new ValidationIssue(string.Format(Resources.HydroNetworkValidator_Cross_section_sections_issues, crossSectionsToCorrect.Count),
                                                    ValidationSeverity.Error,
                                                    Resources.HydroNetworkValidator_Maximum_flow_width_of_cross_sections_is_larger_than_total_width,
                                                    crossSectionsToCorrect));
            }

            return new ValidationReport("Cross sections", finalIssues);
        }

        private static IEnumerable<ValidationIssue> GetCrossSectionValidationIssues(IChannel channel, IHydroNetwork network, HashSet<string> channelsCheckedOnInterpolationBranches)
        {
            foreach (ValidationIssue issue in channel.CrossSections.SelectMany(cs => GetCorrectCrossSectionIssue(cs, network)))
            {
                yield return issue;
            }

            if (channel.OrderNumber == -1)
            {
                foreach (ValidationIssue issue in GetCorrectCrossSectionsOnChannelIssue(new[]
                         {
                             channel
                         }, network))
                {
                    yield return issue;
                }
            }
            else
            {
                if (!channelsCheckedOnInterpolationBranches.Contains(channel.Name))
                {
                    List<IChannel> chainOfChannels = GetChainOfChannelsWithSameOrderNumber(channel, network).ToList();

                    foreach (ValidationIssue issue in GetCorrectCrossSectionsOnChannelIssue(chainOfChannels.ToArray(), network))
                    {
                        yield return issue;
                    }

                    chainOfChannels.ForEach(c => channelsCheckedOnInterpolationBranches.Add(c.Name));
                }
            }
        }

        private static IEnumerable<ValidationIssue> GetCorrectCrossSectionsOnChannelIssue(IChannel[] chainOfChannels, IHydroNetwork network)
        {
            IEnumerable<ICrossSection> crossSections = chainOfChannels.SelectMany(c => c.CrossSections);
            CrossSectionType[] crossSectionTypes = crossSections.Select(cs => cs.CrossSectionType).Distinct().ToArray();

            if (!crossSectionTypes.Any())
            {
                string message = string.Format(Resources.HydroNetworkValidator_No_cross_sections_on_channels, 
                                               string.Join(",", chainOfChannels.Select(c => c.Name).ToArray()));
                yield return new ValidationIssue(chainOfChannels.First(), ValidationSeverity.Error, message, new ValidatedFeatures(network, chainOfChannels.ToArray<IFeature>()));
            }

            // Standard CS are sent as ZW to the modelApi at the moment, so we can consider them as the same
            if ((crossSectionTypes.Contains(CrossSectionType.GeometryBased) || crossSectionTypes.Contains(CrossSectionType.YZ)) &&
                (crossSectionTypes.Contains(CrossSectionType.Standard) || crossSectionTypes.Contains(CrossSectionType.ZW)))
            {
                string msg = string.Format(Resources.HydroNetworkValidator_Multiple_cross_section_types_per_branch_not_supported,
                                           string.Join(",", chainOfChannels.Select(c => c.Name).ToArray()));
                yield return new ValidationIssue(chainOfChannels.First(), ValidationSeverity.Error, msg, new ValidatedFeatures(network, chainOfChannels.ToArray<IFeature>()));
            }
        }

        private static ValidationReport ValidateManholes(IHydroNetwork network)
        {
            var issues = new List<ValidationIssue>();
            
            issues.AddRange(ValidationHelper.ValidateDuplicateNames(network.Manholes, "manholes", network));
            
            return new ValidationReport("Manholes", issues);
        }
        
        private static ValidationReport ValidateCompartments(IHydroNetwork network)
        {
            var issues = new List<ValidationIssue>();

            issues.AddRange(ValidationHelper.ValidateDuplicateNames(network.Compartments, "compartments", network));
            
            foreach (Compartment compartment in network.Compartments)
            {
                if (compartment.ManholeWidth <= 0)
                {
                    issues.Add(new ValidationIssue(compartment,
                                                   ValidationSeverity.Error,
                                                   Resources.HydroNetworkValidator_Width_or_diamater_must_be_larger_than_0));
                }

                if (compartment.ManholeLength <= 0)
                {
                    issues.Add(new ValidationIssue(compartment,
                                                   ValidationSeverity.Error,
                                                   Resources.HydroNetworkValidator_Length_must_be_larger_than_0));
                }

                if (compartment.FloodableArea <= 0)
                {
                    issues.Add(new ValidationIssue(compartment,
                                                   ValidationSeverity.Warning,
                                                   Resources.HydroNetworkValidator_Street_storage_area_is_0));
                }
            }

            return new ValidationReport("Compartments", issues);
        }

        private static ValidationReport ValidateSewerConnections(IHydroNetwork network)
        {
            var issues = new List<ValidationIssue>();
            
            issues.AddRange(ValidationHelper.ValidateDuplicateNames(network.SewerConnections, "sewer connections", network));
            
            return new ValidationReport("Sewer Connections", issues);
        }
        
        private static IEnumerable<ValidationIssue> GetCorrectCrossSectionIssue(ICrossSection crossSection, IHydroNetwork network)
        {
            string errorMessage;
            ICrossSectionDefinition crossSectionDefinition = crossSection.Definition;

            if (!CrossSectionValidator.IsCrossSectionAllowedOnBranch((CrossSection)crossSection, out errorMessage))
            {
                yield return new ValidationIssue(crossSection, ValidationSeverity.Error, errorMessage, new ValidatedFeatures(network, crossSection));
            }

            if (crossSection.Geometry.Coordinates.Length == 0)
            {
                yield return new ValidationIssue(crossSection,
                                                 ValidationSeverity.Error,
                                                 Resources.HydroNetworkValidator_No_profile_defined,
                                                 crossSection);
            }

            if (!CrossSectionValidator.IsFlowProfileValid(crossSectionDefinition))
            {
                if (crossSectionDefinition.CrossSectionType == CrossSectionType.ZW)
                {
                    yield return new ValidationIssue(crossSection, 
                                                     ValidationSeverity.Error,
                                                     string.Format(Resources.HydroNetworkValidator_Tabulated_cross_section_cannot_have_0_width_above_deepest_point, crossSection),
                                                     crossSection);
                }
                else
                {
                    yield return new ValidationIssue(crossSection,
                                                     ValidationSeverity.Error,
                                                     Resources.HydroNetworkValidator_Invalid_flow_profile,
                                                     crossSection);
                }
            }

            if (!CrossSectionValidator.AreCrossSectionsEqualToTheFlowWidth(crossSectionDefinition))
            {
                yield return new ValidationIssue(crossSection,
                                                 ValidationSeverity.Error,
                                                 Resources.HydroNetworkValidator_Maximum_flow_width_of_cross_section_does_not_match_total_width,
                                                 crossSection);
            }

            if (!CrossSectionValidator.AreFloodPlain1AndFloodPlain2WidthsValid(crossSectionDefinition))
            {
                yield return new ValidationIssue(crossSection,
                                                 ValidationSeverity.Error,
                                                 Resources.HydroNetworkValidator_Floodplain2_width_cannot_be_larger_than_0_if_floodplain1_width_is_0,
                                                 crossSection);
            }
        }

        private static IEnumerable<IChannel> GetChainOfChannelsWithSameOrderNumber(IChannel channel, IHydroNetwork network, IList<IChannel> previousLinks = null)
        {
            INode startNode = channel.Source;
            previousLinks = previousLinks ?? new List<IChannel>();
            IChannel channelTo = network.Channels.FirstOrDefault(c => c.OrderNumber == channel.OrderNumber && c.Target == startNode);

            if (channelTo != null && !previousLinks.Contains(channelTo))
            {
                yield return channelTo;
                previousLinks.Add(channel);
                foreach (IChannel channelInChain in GetChainOfChannelsWithSameOrderNumber(channelTo, network, previousLinks))
                {
                    yield return channelInChain;
                }

                previousLinks.Remove(channel);
            }

            INode endNode = channel.Target;
            IChannel channelFrom = network.Channels.FirstOrDefault(c => c.OrderNumber == channel.OrderNumber && c.Source == endNode);

            if (channelFrom != null && !previousLinks.Contains(channelFrom))
            {
                yield return channelFrom;
                previousLinks.Add(channel);
                foreach (IChannel channelInChain in GetChainOfChannelsWithSameOrderNumber(channelFrom, network, previousLinks))
                {
                    yield return channelInChain;
                }

                previousLinks.Remove(channel);
            }

            if (!previousLinks.Any())
            {
                yield return channel;
            }
        }
    }
}