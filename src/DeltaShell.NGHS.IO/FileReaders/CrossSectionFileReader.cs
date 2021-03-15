using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.Roughness;
using DelftTools.Hydro.SewerFeatures;
using DeltaShell.NGHS.IO.FileWriters.CrossSectionDefinition;
using DeltaShell.NGHS.IO.FileWriters.Location;
using DeltaShell.NGHS.IO.Helpers;
using DeltaShell.NGHS.Utils;

namespace DeltaShell.NGHS.IO.FileReaders
{
    public static class CrossSectionFileReader
    {
        /// <summary>
        /// Reads the cross-sections and cross-section definitions and adds them to the provided <paramref name="hydroNetwork"/>
        /// </summary>
        /// <param name="crossSectionLocationFilePath">Path to the cross-section file</param>
        /// <param name="crossSectionDefinitionPath">Path to the cross-section definition file</param>
        /// <param name="hydroNetwork">Network to add the cross-sections to</param>
        /// <param name="defaultFrictionId">The default friction identifier</param>
        /// <param name="afterCrossSectionAddedAction">Action to perform after adding a cross-section</param>
        /// <returns>Definitions that are not coupled to cross-sections and are not shared (usually structure profiles)</returns>
        public static ICrossSectionDefinition[] ReadFile(string crossSectionLocationFilePath, string crossSectionDefinitionPath, IHydroNetwork hydroNetwork, string defaultFrictionId, Action<IChannel> afterCrossSectionAddedAction)
        {
            var csDefinitionCategories = GetCrossSectionDefinitionCategories(crossSectionDefinitionPath);

            var sharedCsdCategories = csDefinitionCategories
                .Where(d => d.ReadProperty<bool>(DefinitionPropertySettings.IsShared.Key, true))
                .ToArray();

            // Shared cross-section definitions lookup
            var sharedDefinitionNameLookup = sharedCsdCategories
                .Select(c => CreateCrossSectionDefinitionFromCategory(c, hydroNetwork, defaultFrictionId))
                .ToDictionary(d => d.Name.ToLower());

            // Unshared cross-section definitions lookup
            var unsharedDefinitionNameLookup = csDefinitionCategories
                .Except(sharedCsdCategories)
                .Select(c => CreateCrossSectionDefinitionFromCategory(c, hydroNetwork, defaultFrictionId))
                .ToDictionary(d => d.Name.ToLower());

            var assignedDefinitions = new List<ICrossSectionDefinition>();
            var fileReadingExceptions = new List<FileReadingException>();
            var branchLookup = hydroNetwork.Branches.ToDictionaryWithDuplicateLogging("Branches", b => b.Name.ToLower());

            // add shared cross-section definitions to network1
            hydroNetwork.SharedCrossSectionDefinitions.AddRange(sharedDefinitionNameLookup.Values);

            foreach (var crossSectionCategory in GetCrossSectionCategories(crossSectionLocationFilePath))
            {
                var definitionId = crossSectionCategory.ReadProperty<string>(LocationRegion.Definition.Key).ToLower();

                ICrossSectionDefinition definition;
                if (sharedDefinitionNameLookup.ContainsKey(definitionId))
                {
                    definition = new CrossSectionDefinitionProxy(sharedDefinitionNameLookup[definitionId]);
                }
                else if (unsharedDefinitionNameLookup.ContainsKey(definitionId))
                {
                    definition = unsharedDefinitionNameLookup[definitionId];
                }
                else
                {
                    fileReadingExceptions.Add(new FileReadingException($"Could not find cross section definition {definitionId} for cross-section {crossSectionCategory.ReadProperty<string>(LocationRegion.Id.Key)}"));
                    continue;
                }

                var name = crossSectionCategory.ReadProperty<string>(LocationRegion.Id.Key);
                var chainage = crossSectionCategory.ReadProperty<double>(LocationRegion.Chainage.Key);
                var branchId = crossSectionCategory.ReadProperty<string>(LocationRegion.BranchId.Key);
                var crossSectionLongName = crossSectionCategory.ReadProperty<string>(LocationRegion.Name.Key, true);

                var branch = branchLookup.ContainsKey(branchId.ToLower())
                    ? branchLookup[branchId.ToLower()]
                    : null;

                switch (branch)
                {
                    case null:
                        throw new FileReadingException($"The read cross section '{name}' has a branch id ({branchId}) which is not available in the model.");
                    case IPipe pipe:
                        {
                            pipe.CrossSection = new CrossSection(definition);

                            if (Math.Abs(chainage) < 0.001) // chainage = 0, so set source
                                pipe.LevelSource = crossSectionCategory.ReadProperty<double>(LocationRegion.Shift.Key);
                            else
                                pipe.LevelTarget = crossSectionCategory.ReadProperty<double>(LocationRegion.Shift.Key);
                            break;
                        }
                    default:
                        {
                            var crossSection = new CrossSection(definition)
                            {
                                Name = name,
                                LongName = crossSectionLongName,
                                Chainage = branch.CorrectlyRoundOffChainageIfChainageIsOnEndOfBranch(chainage)
                            };

                            branch.BranchFeatures.Add(crossSection);
                            assignedDefinitions.Add(definition);
                            afterCrossSectionAddedAction?.Invoke(crossSection.Branch as IChannel);
                            break;
                        }
                }
            }

            if (fileReadingExceptions.Count > 0)
            {
                var innerExceptionMessages = fileReadingExceptions
                    .Select(e => e?.InnerException != null
                        ? e.InnerException.Message + Environment.NewLine
                        : string.Empty
                );

                throw new FileReadingException($"While reading cross sections an error occured :{Environment.NewLine} {string.Join(Environment.NewLine, innerExceptionMessages)}");
            }

            return unsharedDefinitionNameLookup.Values.Except(assignedDefinitions).ToArray();
        }

        private static ICrossSectionDefinition CreateCrossSectionDefinitionFromCategory(DelftIniCategory csdCategory, IHydroNetwork network, string defaultFrictionId)
        {
            var typeProperty = csdCategory.ReadProperty<string>(DefinitionPropertySettings.DefinitionType.Key);
            var templateProperty = csdCategory.ReadProperty<string>(DefinitionPropertySettings.Template.Key, true);

            var definitionReader = DefinitionGeneratorFactory.GetDefinitionReaderCrossSection(typeProperty, templateProperty);
            if (definitionReader == null)
            {
                throw new FileReadingException("No definition reader available for this cross section definition");
            }

            var definition = definitionReader.ReadDefinition(csdCategory);

            SetFrictionOnCrossSectionDefinition(csdCategory, definition, network, defaultFrictionId);

            return definition;
        }

        private static DelftIniCategory[] GetCrossSectionDefinitionCategories(string csdFilename)
        {
            if (!File.Exists(csdFilename))
                throw new FileReadingException($"Could not read file {csdFilename} properly, it doesn't exist.");

            var csDefinitionCategories = new DelftIniReader().ReadDelftIniFile(csdFilename);
            if (csDefinitionCategories.Count == 0)
                throw new FileReadingException($"Could not read file {csdFilename} properly, it seems empty");

            var csdDefinitionCategories = csDefinitionCategories.Where(category =>
                    string.Equals(category.Name, DefinitionPropertySettings.Header,
                        StringComparison.InvariantCultureIgnoreCase))
                .OrderByDescending(cat => cat.ReadProperty<bool>(DefinitionPropertySettings.IsShared.Key, true))
                .ToArray();
            return csdDefinitionCategories;
        }

        private static DelftIniCategory[] GetCrossSectionCategories(string cslFilename)
        {
            var cslCategories = File.Exists(cslFilename)
                ? new DelftIniReader().ReadDelftIniFile(cslFilename)
                : new DelftIniCategory[0];

            var csIniLocations = cslCategories.Any()
                ? cslCategories.Where(category => category.Name == CrossSectionRegion.IniHeader).ToArray()
                : new DelftIniCategory[0];

            if (cslCategories.Any() && !csIniLocations.Any())
                throw new FileReadingException("Could not read any cross section locations it seems not available");

            return csIniLocations;
        }

        private static void SetFrictionOnCrossSectionDefinition(IDelftIniCategory csdDefinitionCategory, ICrossSectionDefinition readCrossSectionDefinition, IHydroNetwork network, string defaultFrictionId)
        {
            switch (readCrossSectionDefinition.CrossSectionType)
            {
                case CrossSectionType.YZ:
                case CrossSectionType.GeometryBased:
                    {
                        var frictionIds = csdDefinitionCategory.ReadPropertiesToListOfType<string>(DefinitionPropertySettings.FrictionIds.Key, true, ';');
                        if (frictionIds == null) return;
                        if (frictionIds.Count < 0)
                            throw new FileReadingException("reading error");

                        if (frictionIds.Count == 1 && frictionIds[0].Equals(defaultFrictionId))
                        {
                            readCrossSectionDefinition.Sections.Add(new CrossSectionSection
                            {
                                SectionType = GetCrossSectionSectionType(RoughnessDataSet.MainSectionTypeName, network),
                                MinY = readCrossSectionDefinition.Left,
                                MaxY = readCrossSectionDefinition.Right
                            });

                            return;
                        }

                        var frictionPositions = csdDefinitionCategory.ReadPropertiesToListOfType<double>(DefinitionPropertySettings.FrictionPositions.Key);
                        if (frictionPositions.Count < 0)
                            throw new FileReadingException("reading error");

                        if (frictionPositions.Count != frictionIds.Count + 1)
                            throw new FileReadingException("reading error");

                        readCrossSectionDefinition.Sections.Clear();

                        for (int index = 0; index < frictionIds.Count; index++)
                        {
                            var networkSectionType = GetCrossSectionSectionType(frictionIds[index], network);

                            readCrossSectionDefinition.Sections.Add(new CrossSectionSection
                            {
                                SectionType = networkSectionType,
                                MinY = frictionPositions[index],
                                MaxY = frictionPositions[index + 1]
                            });
                        }

                        return;
                    }
                case CrossSectionType.ZW:
                    {
                        var mainCrossSectionSectionType = GetCrossSectionSectionType(RoughnessDataSet.MainSectionTypeName, network);
                        var flowWidths = csdDefinitionCategory.ReadPropertiesToListOfType<double>(DefinitionPropertySettings.FlowWidths.Key);

                        var frictionId = csdDefinitionCategory.ReadProperty<string>(DefinitionPropertySettings.FrictionId.Key, true);
                        if (frictionId != null)
                        {
                            // Handle scenario of a zw profile (tabulated) that doesn't contain a template
                            readCrossSectionDefinition.AddSection(mainCrossSectionSectionType, flowWidths.Max());
                            return;
                        }

                        var frictionIds = csdDefinitionCategory.ReadPropertiesToListOfType<string>(DefinitionPropertySettings.FrictionIds.Key, true, ';');
                        if (frictionIds != null && frictionIds.Count == 3 && frictionIds.All(fi => fi.Equals(defaultFrictionId)))
                        {
                            readCrossSectionDefinition.AddSection(mainCrossSectionSectionType, readCrossSectionDefinition.Width);
                            return;
                        }

                        var mainSectionWidth = csdDefinitionCategory.ReadProperty<double>(DefinitionPropertySettings.Main.Key);
                        var floodPlain1Width = csdDefinitionCategory.ReadProperty<double>(DefinitionPropertySettings.FloodPlain1.Key, true);

                        var floodPlain2Width = flowWidths.Max() - mainSectionWidth - floodPlain1Width; //FloodPlain2 is defined as max(FlowWidth) - Main - Floodplain1

                        readCrossSectionDefinition.Sections.Clear();
                        readCrossSectionDefinition.AddSection(mainCrossSectionSectionType, mainSectionWidth);

                        if (Math.Abs(floodPlain1Width) > 1e-6)
                        {
                            var floodPlain1CrossSectionSectionType = GetCrossSectionSectionType(RoughnessDataSet.Floodplain1SectionTypeName, network);
                            readCrossSectionDefinition.AddSection(floodPlain1CrossSectionSectionType, floodPlain1Width);
                        }

                        if (Math.Abs(floodPlain2Width) > 1e-6)
                        {
                            var floodPlain2CrossSectionSectionType = GetCrossSectionSectionType(RoughnessDataSet.Floodplain2SectionTypeName, network);
                            readCrossSectionDefinition.AddSection(floodPlain2CrossSectionSectionType, floodPlain2Width);
                        }

                        return;
                    }
                case CrossSectionType.Standard:
                    {
                        var frictionIds = csdDefinitionCategory.ReadPropertiesToListOfType<string>(DefinitionPropertySettings.FrictionId.Key, true, ';');
                        if (frictionIds == null) return;

                        if (frictionIds.Count != 1)
                            throw new FileReadingException("reading error");

                        var sectionTypeName = frictionIds.FirstOrDefault();
                        if (sectionTypeName == null)
                            throw new FileReadingException("reading error");

                        if (sectionTypeName.Equals(defaultFrictionId))
                        {
                            readCrossSectionDefinition.Sections.Add(new CrossSectionSection
                            {
                                SectionType = GetCrossSectionSectionType(RoughnessDataSet.MainSectionTypeName, network),
                                MinY = readCrossSectionDefinition.Left,
                                MaxY = readCrossSectionDefinition.Right
                            });
                            return;
                        }

                        readCrossSectionDefinition.Sections.Add(new CrossSectionSection
                        {
                            SectionType = GetCrossSectionSectionType(sectionTypeName, network),
                            MinY = readCrossSectionDefinition.Left,
                            MaxY = readCrossSectionDefinition.Right
                        });

                        return;
                    }
                default:
                    return;
            }
        }

        private static CrossSectionSectionType GetCrossSectionSectionType(string sectionTypeName, IHydroNetwork network)
        {
            var crossSectionSectionType = network.CrossSectionSectionTypes.FirstOrDefault(cst => cst.Name == sectionTypeName);
            if (crossSectionSectionType == null)
            {
                crossSectionSectionType = new CrossSectionSectionType { Name = sectionTypeName };
                network.CrossSectionSectionTypes.Add(crossSectionSectionType);
            }
            return crossSectionSectionType;
        }
    }
}