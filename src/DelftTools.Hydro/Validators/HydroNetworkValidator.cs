using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Functions;
using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.Helpers;
using DelftTools.Hydro.Structures;
using DelftTools.Utils;
using DelftTools.Utils.Validation;
using GeoAPI.Extensions.Networks;

namespace DelftTools.Hydro.Validators
{
    public static class HydroNetworkValidator
    {
        public static ValidationReport Validate(IHydroNetwork target)
        {
            var subReports = new List<ValidationReport>();
            if (target != null)
            {
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
                        ExtraResistanceValidator.Validate(target.Structures.Where(s => s is IExtraResistance)),
                    });
                }
                if (target.Compartments.Any() && target.Pipes.Any() && target.Manholes.Any())
                {
                    subReports.AddRange(new[]
                    {
                        ValidateCompartments(target)
                    });
                }
            }
            return new ValidationReport("Network", new List<ValidationIssue>(),
                                        subReports);
        }

        private static ValidationReport ValidateRetentions(IHydroNetwork network)
        {
            var issues = new List<ValidationIssue>();
            foreach (var retention in network.Retentions)
            {
                if (!retention.UseTable && Math.Abs(retention.StorageArea) < double.Epsilon)
                {
                    issues.Add(new ValidationIssue(retention,
                                                   ValidationSeverity.Error,
                                                   $"The values in the storage graph of retention {retention.Name} should be greater than zero."));


                }

                IFunction retentionTableData = retention.Data;
                if (retention.UseTable && (retentionTableData?.Components?[0] == null || retentionTableData.Components[0].Values.Count == 0))
                {
                    issues.Add(new ValidationIssue(retention,
                                                   ValidationSeverity.Error,
                                                   $"Table should be used for {retention.Name}, but no values are set.", retentionTableData));
                    continue;
                }

                if (retention.UseTable)
                {
                    var levelStorageValues = retentionTableData.Components[0].GetValues<double>().ToArray();
                    var levelStorageHeigth = retentionTableData.Arguments[0].GetValues<double>().ToArray();
                    if (levelStorageValues.Length != levelStorageHeigth.Length) continue;
                    for (var index = 0; index < levelStorageValues.Length; index++)
                    {
                        double value = levelStorageValues[index];
                        double height = levelStorageHeigth[index];
                        if (Math.Abs(value) < double.Epsilon)
                        {
                            issues.Add(new ValidationIssue(retention,
                                                           ValidationSeverity.Error,
                                                           $"Table should be used for {retention.Name}, but at height {height} storage value is {value} which is not allowed (should be higher than 0).", retentionTableData));
                        }
                    }
                }
            }

            return new ValidationReport("Retentions", issues);
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

            return new ValidationReport("Culverts", issues);
        }

        private static ValidationReport ValidateCoordinateSystem(IHydroNetwork target)
        {
            if (target.CoordinateSystem == null)
            {
                var issue = new ValidationIssue(target.CoordinateSystem, ValidationSeverity.Warning,
                    string.Format(
                        "No Coordinate System selected for Network. Default map projection will be used for distance calculations."), target.CoordinateSystem);

                return new ValidationReport("Network Coordinate system", new[] { issue });
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
            var issues = network?.Channels
                                .SelectMany(b => GetBranchValidationIssues(b, network))
                                .ToList() 
                         ?? Enumerable.Empty<ValidationIssue>();

            var nodeIssues = network?.Nodes
                                    .SelectMany(n => GetBranchOrderNumbersAtNode(n, network))
                                    .ToList() 
                             ?? Enumerable.Empty<ValidationIssue>();

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
                var message = $"Target and source node of branch '{channel.Name}' have the same id, '{channel.Source.Name}'. Circular branch?";
                yield return new ValidationIssue(channel, ValidationSeverity.Error, message, network);
            }

            if (channel.OrderNumber != -1 && channel.OrderNumber < 0)
            {
                var message = $"Branch '{channel.Name}' has an order number of '{channel.OrderNumber}'. Ordernumber can be -1 (no interpolation over node) or greater than or equal to 0 ";
                yield return new ValidationIssue(channel, ValidationSeverity.Error, message, network);
            }
        }

        private static ValidationReport ValidateIds(IHydroNetwork network)
        {
            IEnumerable<ICrossSectionDefinition> crossSectionDefinitions = GetCrossSectionDefinitions(network.CrossSections.Select(cs => cs.Definition),
                                                                                                      network.SharedCrossSectionDefinitions);
            
            var issuesAsArray = new[]
                                    {
                                        ValidationHelper.ValidateDuplicateNames(network.Branches.Cast<INameable>(), "branches", network),
                                        ValidationHelper.ValidateDuplicateNames(network.Bridges.Cast<INameable>(), "bridges", network),
                                        ValidationHelper.ValidateDuplicateNames(network.Culverts.Cast<INameable>(), "culverts", network),
                                        ValidationHelper.ValidateDuplicateNames(network.CrossSections.Cast<INameable>(), "cross sections", network),
                                        ValidationHelper.ValidateDuplicateNames(crossSectionDefinitions.Cast<INameable>(), "cross section definitions", network),
                                        ValidationHelper.ValidateDuplicateNames(network.ExtraResistances.Cast<INameable>(), "extra resistances", network),
                                        ValidationHelper.ValidateDuplicateNames(network.HydroNodes.Cast<INameable>(), "nodes", network),
                                        ValidationHelper.ValidateDuplicateNames(network.LateralSources.Cast<INameable>(), "lateral sources", network),
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
                                        ValidationHelper.ValidateNoEmptyNames(crossSectionDefinitions.Cast<INameable>(), "cross section definition", network),
                                        ValidationHelper.ValidateNoEmptyNames(network.ExtraResistances.Cast<INameable>(), "extra resistance", network),
                                        ValidationHelper.ValidateNoEmptyNames(network.HydroNodes.Cast<INameable>(), "node", network),
                                        ValidationHelper.ValidateNoEmptyNames(network.LateralSources.Cast<INameable>(), "lateral source", network),
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

        private static IEnumerable<ICrossSectionDefinition> GetCrossSectionDefinitions(IEnumerable<ICrossSectionDefinition> crossSectionDefinitions, 
                                                                                       IEnumerable<ICrossSectionDefinition> sharedDefinitions)
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
            var issues = network?.Channels.SelectMany(b => GetCrossSectionValidationIssues(b, network, channelsCheckedOnInterpolationBranches)).ToList() ?? new List<ValidationIssue>();

            var issuesContainSectionIssues = issues.Where(i => i.Message.Equals("The maximum flow width of this cross section does not match the total width of all its sections.")).ToList();
            var finalIssues = issues.Where(i => !i.Message.Equals("The maximum flow width of this cross section does not match the total width of all its sections.")).ToList();
            if (issuesContainSectionIssues.Any())
            {
                var crossSectionsToCorrect = issuesContainSectionIssues.Select(issue => issue.ViewData as ICrossSection).ToList();
                finalIssues.Add(new ValidationIssue($"Cross section sections issues ({crossSectionsToCorrect.Count})", ValidationSeverity.Error,
                    "The maximum flow width of one or more cross sections is larger than the total width of all its sections.", crossSectionsToCorrect));
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
                    var chainOfChannels = GetChainOfChannelsWithSameOrderNumber(channel, network).ToList();

                    foreach (var issue in GetCorrectCrossSectionsOnChannelIssue(chainOfChannels, network))
                    {
                        yield return issue;
                    }

                    chainOfChannels.ForEach(c => channelsCheckedOnInterpolationBranches.Add(c.Name));
                }
            }
        }

        private static IEnumerable<ValidationIssue> GetCorrectCrossSectionsOnChannelIssue(IEnumerable<IChannel> chainOfChannels, IHydroNetwork network)
        {
            var crossSections = chainOfChannels.SelectMany(c => c.CrossSections);
            var crossSectionTypes = crossSections.Select(cs => cs.CrossSectionType).Distinct().ToArray();
            
            if (!crossSectionTypes.Any())
            {
                var message = $"No cross sections on channel(s) {string.Join(",", chainOfChannels.Select(c => c.Name).ToArray())}; can not start calculation.";
                yield return new ValidationIssue(chainOfChannels.First(), ValidationSeverity.Error, message, new RegionFeatureSelection(network, chainOfChannels));
            }

            // Standard CS are sent as ZW to the modelApi at the moment, so we can consider them as the same
            if ((crossSectionTypes.Contains(CrossSectionType.GeometryBased) || crossSectionTypes.Contains(CrossSectionType.YZ)) &&
                (crossSectionTypes.Contains(CrossSectionType.Standard) || crossSectionTypes.Contains(CrossSectionType.ZW)))
            {
                var msg = $"Multiple cross-section-types (mix of Standard/ZW and Geometry/YZ) per branch(es) not supported.({string.Join(",", chainOfChannels.Select(c => c.Name).ToArray())})";
                yield return new ValidationIssue(chainOfChannels.First(), ValidationSeverity.Error, msg, network);
            }
        }

        private static ValidationReport ValidateCompartments(IHydroNetwork target)
        {
            var issues = new List<ValidationIssue>();

            foreach (var compartment in target.Compartments)
            {
                if (compartment.ManholeWidth <= 0)
                {
                    issues.Add(new ValidationIssue(compartment, ValidationSeverity.Error, "Width / diameter must be larger than 0"));
                }

                if (compartment.ManholeLength <= 0)
                {
                    issues.Add(new ValidationIssue(compartment, ValidationSeverity.Error, "Length must be larger than 0"));
                }

                if (compartment.FloodableArea <= 0)
                {
                    issues.Add(new ValidationIssue(compartment, ValidationSeverity.Warning, "Street storage area is set to 0. Recommended to use storage type closed instead of reservoir"));
                }
            }

            return new ValidationReport("Compartments", issues);
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
                yield return new ValidationIssue(crossSection, ValidationSeverity.Error, "No profile defined", network);
            }

            if (!CrossSectionValidator.IsFlowProfileValid(crossSectionDefinition))
            {
                if (crossSectionDefinition.CrossSectionType == CrossSectionType.ZW)
                {
                    yield return new ValidationIssue(crossSection, ValidationSeverity.Error,  
                        string.Format("Tabulated cross section {0} cannot have zero width at levels above deepest point of its definition.", crossSection));
                }
                else
                {
                    yield return new ValidationIssue(crossSection, ValidationSeverity.Error, "Invalid flow profile", network);
                }
            }

            if (!CrossSectionValidator.AreCrossSectionsEqualToTheFlowWidth(crossSectionDefinition))
            {
                yield return new ValidationIssue(crossSection, ValidationSeverity.Error,
                    "The maximum flow width of this cross section does not match the total width of all its sections.", crossSection);
            }

            if (!CrossSectionValidator.AreFloodPlain1AndFloodPlain2WidthsValid(crossSectionDefinition))
            {
                yield return new ValidationIssue(crossSection, ValidationSeverity.Error,
                    "FloodPlain2 width cannot be larger than 0.0, if FloodPlain1 width is equal to 0.0", crossSection);
            }
        }

        private static IEnumerable<IChannel> GetChainOfChannelsWithSameOrderNumber(IChannel channel, IHydroNetwork network, IList<IChannel> previousLinks = null)
        {
            var startNode = channel.Source;
            previousLinks = previousLinks ?? new List<IChannel>();
            var channelTo = network.Channels.FirstOrDefault(c => c.OrderNumber == channel.OrderNumber && c.Target == startNode);

            if (channelTo != null && !previousLinks.Contains(channelTo))
            {
                yield return channelTo;
                previousLinks.Add(channel);
                foreach (var channelInChain in GetChainOfChannelsWithSameOrderNumber(channelTo, network, previousLinks))
                {
                    yield return channelInChain;
                }
                previousLinks.Remove(channel);
            }

            var endNode = channel.Target;
            var channelFrom = network.Channels.FirstOrDefault(c => c.OrderNumber == channel.OrderNumber && c.Source == endNode);

            if (channelFrom != null && !previousLinks.Contains(channelFrom))
            {
                yield return channelFrom;
                previousLinks.Add(channel);
                foreach (var channelInChain in GetChainOfChannelsWithSameOrderNumber(channelFrom, network, previousLinks))
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