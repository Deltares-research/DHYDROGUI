using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.Roughness;
using DelftTools.Hydro.SewerFeatures;
using DelftTools.Utils.Collections;
using DeltaShell.NGHS.IO.DataObjects.Friction;
using DeltaShell.NGHS.IO.FileReaders.Location.CrossSections;
using DeltaShell.NGHS.IO.FileWriters.CrossSectionDefinition;
using DeltaShell.NGHS.IO.FileWriters.Roughness;
using DeltaShell.NGHS.IO.Helpers;
using DeltaShell.NGHS.IO.Properties;
using DeltaShell.NGHS.Utils;
using DeltaShell.NGHS.Utils.Extensions;
using GeoAPI.Extensions.Networks;
using log4net;
using log4net.Core;

namespace DeltaShell.NGHS.IO.FileReaders.CrossSectionDefinition
{
    public static class CrossSectionFileReader
    {
        private static readonly CrossSectionLocationFileReader locationFileReader = new CrossSectionLocationFileReader(new DelftIniReader());
        private static readonly ILog log = LogManager.GetLogger(typeof(CrossSectionFileReader));

        /// <summary>
        /// Reads the cross-sections and cross-section definitions and adds them to the provided <paramref name="hydroNetwork"/>
        /// </summary>
        /// <param name="crossSectionLocationFilePath">Path to the cross-section file</param>
        /// <param name="crossSectionDefinitionPath">Path to the cross-section definition file</param>
        /// <param name="hydroNetwork">Network to add the cross-sections to</param>
        /// <param name="channelFrictionDefinitions"></param>
        /// <returns>Definitions that are not coupled to cross-sections and are not shared (usually structure profiles)</returns>
        public static ICrossSectionDefinition[] ReadFile(string crossSectionLocationFilePath, string crossSectionDefinitionPath, IHydroNetwork hydroNetwork, IEnumerable<ChannelFrictionDefinition> channelFrictionDefinitions)
        {
            CrossSectionLocation[] crossSectionLocations = locationFileReader.Read(crossSectionLocationFilePath).ToArray();
            HashSet<string> duplicateDefinitionIds = GetDuplicateDefinitionIds(crossSectionLocations);
            
            channelFrictionDefinitions = channelFrictionDefinitions ?? Enumerable.Empty<ChannelFrictionDefinition>();
            var frictionDefinitions = channelFrictionDefinitions.ToLookup(cfd => cfd.Channel);

            var csDefinitionCategories = GetCrossSectionDefinitionCategories(crossSectionDefinitionPath);

            var sharedCsdCategories = csDefinitionCategories
                                      .Where(d => d.ReadProperty<bool>(DefinitionPropertySettings.IsShared, true) || 
                                                  duplicateDefinitionIds.Contains(d.ReadProperty<string>(DefinitionPropertySettings.Id.Key)))
                                      .ToArray();
            IDictionary<string, ICrossSectionDefinition> sharedDefinitionNameLookup = DefinitionNameLookup(hydroNetwork, sharedCsdCategories, "SharedCrossSections");
            IDictionary<string, ICrossSectionDefinition> unsharedDefinitionNameLookup = DefinitionNameLookup(hydroNetwork, csDefinitionCategories.Except(sharedCsdCategories), "UnsharedCrossSections");
            
            var assignedDefinitions = new List<ICrossSectionDefinition>();
            var fileReadingExceptions = new List<FileReadingException>();
            
            var branchLookup = hydroNetwork.Branches
                                           .ToDictionaryWithDuplicateLogging(nameof(hydroNetwork.Branches), b => b.Name, b => b, comparer:StringComparer.InvariantCultureIgnoreCase);

            // add shared cross-section definitions to network
            hydroNetwork.SharedCrossSectionDefinitions.AddRange(sharedDefinitionNameLookup.Values);

            foreach (CrossSectionLocation crossSectionLocation in crossSectionLocations)
            {
                string definitionId = crossSectionLocation.DefinitionId;
                string name = crossSectionLocation.Id;

                ICrossSectionDefinition definition = GetDefinitionForLocation(definitionId, name, sharedDefinitionNameLookup, unsharedDefinitionNameLookup, fileReadingExceptions);

                double chainage = crossSectionLocation.Chainage;
                string branchId = crossSectionLocation.BranchId;
                string crossSectionLongName = crossSectionLocation.LongName;

                if (!branchLookup.TryGetValue(branchId, out IBranch branch))
                {
                    fileReadingExceptions.Add(new FileReadingException($"The read cross section '{name}' has a branch id ({branchId}) which is not available in the model."));
                    continue;
                }
                var crossSection = new CrossSection(definition)
                {
                    LongName = crossSectionLongName,
                    Chainage = branch.GetBranchSnappedChainage(chainage)
                };

                crossSection.SetNameWithoutUpdatingDefinition(name);

                if (branch is IChannel channel)
                {
                    SetFrictionType(channel, crossSection, frictionDefinitions);
                }

                double shift = crossSectionLocation.Shift;
                if (AddCrossSectionToNetwork(branch, crossSection, shift))
                    assignedDefinitions.Add(definition);
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

        private static IDictionary<string, ICrossSectionDefinition> DefinitionNameLookup(IHydroNetwork hydroNetwork, IEnumerable<IDelftIniCategory> csdCategories, string context)
        {
            var crossSectionDefinitions = new ConcurrentDictionary<string, ICrossSectionDefinition>(StringComparer.InvariantCultureIgnoreCase);
            var duplicateDefinitionNameKeys = new ConcurrentQueue<string>();
            
            csdCategories
                .Select(c => c)
                .AsParallel()
                .ForAll(c =>
                {
                    var csd = CreateCrossSectionDefinitionFromCategory(c, hydroNetwork);
                    if (csd != null)
                    {
                        crossSectionDefinitions.AddOrUpdate(csd.Name, csd, (name, definition) =>
                        {
                            duplicateDefinitionNameKeys.Enqueue(name);
                            return definition;
                        });
                    }
                });
            
            if (duplicateDefinitionNameKeys.Any())
            {
                var message = $"{Resources.CrossSectionFileReader_DefinitionNameLookup_The_following_cross_section_entries_were_not_unique__}{string.Format(Resources.CrossSectionFileReader_DefinitionNameLookup___0____Total_non_unique___1___in___2__, string.Join(", ", duplicateDefinitionNameKeys), duplicateDefinitionNameKeys.Count, context)}";
                log.Warn(message);
            }

            return crossSectionDefinitions;
        }

        private static void SetFrictionType(IChannel channel, ICrossSection crossSection, ILookup<IChannel, ChannelFrictionDefinition> frictionDefinitions)
        {
            if (HasNonDefaultChannelSections(crossSection) && frictionDefinitions.Contains(channel))
            {
                frictionDefinitions[channel].ForEach(cfd => cfd.SpecificationType = ChannelFrictionSpecificationType.RoughnessSections);
            }
        }

        private static bool HasNonDefaultChannelSections(ICrossSection crossSection)
        {
            var sections = crossSection.Definition.Sections;

            return sections.Count > 1 
                   || sections.Count == 1 && !sections[0].IsDefaultChannelsSection;
        }

        private static bool AddCrossSectionToNetwork(IBranch branch, ICrossSection crossSection,  double shift)
        {
            switch (branch)
            {
                case ISewerConnection sewerConnection:
                {
                    if (Math.Abs(crossSection.Chainage) < 0.001) // chainage = 0, so set source
                    {
                        sewerConnection.LevelSource = shift;
                    }
                    else
                    {
                        sewerConnection.LevelTarget = shift;
                    }
                    sewerConnection.CrossSection = crossSection;
                    break;
                }

                case IChannel channel:
                {
                    // Reference level as used in sobek is not stored in cross section; correct z values.
                    if (crossSection.Definition.IsProxy)
                    {
                        ((CrossSectionDefinitionProxy) crossSection.Definition).LevelShift = shift;
                    }
                    else if (Math.Abs(shift) > double.Epsilon)
                    {
                        crossSection.Definition.ShiftLevel(shift);
                    }
                    
                    channel.BranchFeatures.Add(crossSection);

                    break;
                }
                default:
                {
                    return false;
                }
            }
            return true;
        }

        private static ICrossSectionDefinition GetDefinitionForLocation(string definitionId, string name, IDictionary<string, ICrossSectionDefinition> sharedDefinitionNameLookup, IDictionary<string, ICrossSectionDefinition> unsharedDefinitionNameLookup, List<FileReadingException> fileReadingExceptions)
        {
            if (sharedDefinitionNameLookup.TryGetValue(definitionId, out ICrossSectionDefinition definition))
            {
                definition = new CrossSectionDefinitionProxy(definition);
            }
            else if (!unsharedDefinitionNameLookup.TryGetValue(definitionId, out definition))
            {
                fileReadingExceptions.Add(new FileReadingException($"Could not find cross section definition {definitionId} for cross-section {name}"));
                return definition;
            }

            return definition;
        }

        private static ICrossSectionDefinition CreateCrossSectionDefinitionFromCategory(IDelftIniCategory csdCategory, IHydroNetwork network)
        {
            var typeProperty = csdCategory.ReadProperty<string>(DefinitionPropertySettings.DefinitionType);
            var templateProperty = csdCategory.ReadProperty<string>(DefinitionPropertySettings.Template, true);

            var definitionReader = DefinitionGeneratorFactory.GetDefinitionReaderCrossSection(typeProperty, templateProperty);
            if (definitionReader == null)
            {
                log.Error(string.Format(Resources.CrossSectionFileReader_CreateCrossSectionDefinitionFromCategory_No_definition_reader_available_for_this_cross_section_definition_type__0_, typeProperty));
                return null;
            }

            var definition = definitionReader.ReadDefinition(csdCategory);

            SetFrictionOnCrossSectionDefinition(csdCategory, definition, network);

            return definition;
        }

        private static DelftIniCategory[] GetCrossSectionDefinitionCategories(string csdFilename)
        {
            if (!File.Exists(csdFilename))
                throw new FileReadingException(string.Format(Resources.Could_not_read_file_0_properly_it_doesnt_exist, csdFilename));

            var csDefinitionCategories = new DelftIniReader().ReadDelftIniFile(csdFilename);
            if (csDefinitionCategories.Count == 0)
                throw new FileReadingException(string.Format(Resources.Could_not_read_file_0_properly_it_seems_empty, csdFilename));

            return csDefinitionCategories.Where(category =>
                                                    string.Equals(category.Name, DefinitionPropertySettings.Header,
                                                                  StringComparison.InvariantCultureIgnoreCase))
                                         .ToArray();
        }
        
        private static void SetFrictionOnCrossSectionDefinition(IDelftIniCategory csdDefinitionCategory, ICrossSectionDefinition readCrossSectionDefinition, IHydroNetwork network)
        {
            switch (readCrossSectionDefinition.CrossSectionType)
            {
                case CrossSectionType.YZ:
                case CrossSectionType.GeometryBased:
                {
                    SetYzGeometryBasedCrossSectionFriction(csdDefinitionCategory, readCrossSectionDefinition, network);
                    break;
                }
                case CrossSectionType.ZW:
                {
                    SetZwCrossSectionFriction(csdDefinitionCategory, readCrossSectionDefinition, network);
                    break;
                }
                case CrossSectionType.Standard:
                {
                    SetStandardCrossSectionFriction(csdDefinitionCategory, readCrossSectionDefinition, network);
                    break;
                }
            }
        }

        private static void SetStandardCrossSectionFriction(IDelftIniCategory csdDefinitionCategory, ICrossSectionDefinition readCrossSectionDefinition, IHydroNetwork network)
        {
            var frictionIds = csdDefinitionCategory.ReadPropertiesToListOfType<string>(DefinitionPropertySettings.FrictionId, true, ';');
            if (frictionIds == null) return;

            if (frictionIds.Count != 1)
                throw new FileReadingException("reading error");

            var sectionTypeName = frictionIds.FirstOrDefault();
            if (sectionTypeName == null)
                throw new FileReadingException("reading error");

            if (IsDefaultChannelsSectionId(sectionTypeName))
            {
                lock (network.CrossSectionSectionTypes)
                {
                    readCrossSectionDefinition.Sections.Add(new CrossSectionSection
                    {
                        SectionType = GetCrossSectionSectionType(RoughnessDataSet.MainSectionTypeName, network),
                        MinY = readCrossSectionDefinition.Left,
                        MaxY = readCrossSectionDefinition.Right,
                        IsDefaultChannelsSection = true
                    });
                }
                return;
            }

            lock (network.CrossSectionSectionTypes)
            {
                readCrossSectionDefinition.Sections.Add(new CrossSectionSection
                {
                    SectionType = GetCrossSectionSectionType(sectionTypeName, network),
                    MinY = readCrossSectionDefinition.Left,
                    MaxY = readCrossSectionDefinition.Right
                });
            }
        }

        private static void SetZwCrossSectionFriction(IDelftIniCategory csdDefinitionCategory, ICrossSectionDefinition readCrossSectionDefinition, IHydroNetwork network)
        {
            IList<double> flowWidths = csdDefinitionCategory.ReadPropertiesToListOfType<double>(DefinitionPropertySettings.FlowWidths);

            string frictionId = csdDefinitionCategory.ReadProperty<string>(DefinitionPropertySettings.FrictionId, true);
            IList<string> frictionIds = csdDefinitionCategory.ReadPropertiesToListOfType<string>(DefinitionPropertySettings.FrictionIds, true, ';');

            if (frictionId != null || (frictionIds != null && frictionIds.Any(id => !IsDefaultSectionId(id) && !IsDefaultChannelsSectionId(id))))
            {
                // read definition as zw definition
                string sectionName = frictionId ?? frictionIds.First();
                CrossSectionSectionType crossSectionSectionType = GetCrossSectionSectionType(sectionName, network);
                lock (network.CrossSectionSectionTypes)
                    readCrossSectionDefinition.AddSection(crossSectionSectionType, flowWidths.Max());
                return;
            }

            // read definition as zwRiver definition
            CrossSectionSectionType mainCrossSectionSectionType = GetCrossSectionSectionType(RoughnessDataSet.MainSectionTypeName, network);

            if (frictionIds != null && frictionIds.Count == 3 && frictionIds.All(IsDefaultChannelsSectionId))
            {
                lock (network.CrossSectionSectionTypes) 
                    readCrossSectionDefinition.AddSection(mainCrossSectionSectionType, readCrossSectionDefinition.Width);
                return;
            }

            double mainSectionWidth = csdDefinitionCategory.ReadProperty<double>(DefinitionPropertySettings.Main);
            double floodPlain1Width = csdDefinitionCategory.ReadProperty<double>(DefinitionPropertySettings.FloodPlain1, true);

            double floodPlain2Width = flowWidths.Max() - mainSectionWidth - floodPlain1Width; //FloodPlain2 is defined as max(FlowWidth) - Main - Floodplain1

            lock (network.CrossSectionSectionTypes) 
                readCrossSectionDefinition.Sections.Clear();
            lock (network.CrossSectionSectionTypes) 
                readCrossSectionDefinition.AddSection(mainCrossSectionSectionType, mainSectionWidth);

            if (Math.Abs(floodPlain1Width) > 1e-6)
            {
                CrossSectionSectionType floodPlain1CrossSectionSectionType = GetCrossSectionSectionType(RoughnessDataSet.Floodplain1SectionTypeName, network);
                lock (network.CrossSectionSectionTypes) 
                    readCrossSectionDefinition.AddSection(floodPlain1CrossSectionSectionType, floodPlain1Width);
            }

            if (Math.Abs(floodPlain2Width) > 1e-6)
            {
                CrossSectionSectionType floodPlain2CrossSectionSectionType = GetCrossSectionSectionType(RoughnessDataSet.Floodplain2SectionTypeName, network);
                lock (network.CrossSectionSectionTypes) 
                    readCrossSectionDefinition.AddSection(floodPlain2CrossSectionSectionType, floodPlain2Width);
            }
        }

        private static void SetYzGeometryBasedCrossSectionFriction(IDelftIniCategory csdDefinitionCategory, ICrossSectionDefinition readCrossSectionDefinition, IHydroNetwork network)
        {
            var frictionIds = csdDefinitionCategory.ReadPropertiesToListOfType<string>(DefinitionPropertySettings.FrictionIds, true, ';');
            if (frictionIds == null)
            {
                return;
            }

            if (frictionIds.Count == 1 && IsDefaultChannelsSectionId(frictionIds[0]))
            {
                lock (network.CrossSectionSectionTypes)
                {
                    readCrossSectionDefinition.Sections.Add(new CrossSectionSection
                    {
                        SectionType = GetCrossSectionSectionType(RoughnessDataSet.MainSectionTypeName, network),
                        MinY = readCrossSectionDefinition.Left,
                        MaxY = readCrossSectionDefinition.Right,
                        IsDefaultChannelsSection = true
                    });
                }
                return;
            }

            var frictionPositions = csdDefinitionCategory.ReadPropertiesToListOfType<double>(DefinitionPropertySettings.FrictionPositions);
            if (frictionPositions == null)
            {
                throw new FileReadingException("reading error");
            }

            if (frictionPositions.Count != frictionIds.Count + 1)
            {
                throw new FileReadingException("reading error");
            }

            lock (network.CrossSectionSectionTypes)
            {
                readCrossSectionDefinition.Sections.Clear();
            }

            for (int index = 0; index < frictionIds.Count; index++)
            {
                var networkSectionType = GetCrossSectionSectionType(frictionIds[index], network);
                lock (network.CrossSectionSectionTypes)
                {
                    readCrossSectionDefinition.Sections.Add(new CrossSectionSection
                    {
                        SectionType = networkSectionType,
                        MinY = frictionPositions[index],
                        MaxY = frictionPositions[index + 1]
                    });
                }
            }
        }

        private static bool IsDefaultSectionId(string sectionId)
        {
            return string.Equals(sectionId, RoughnessDataSet.MainSectionTypeName, StringComparison.InvariantCultureIgnoreCase)
                   || string.Equals(sectionId, RoughnessDataSet.Floodplain1SectionTypeName, StringComparison.InvariantCultureIgnoreCase)
                   || string.Equals(sectionId, RoughnessDataSet.Floodplain2SectionTypeName, StringComparison.InvariantCultureIgnoreCase);
        }

        private static bool IsDefaultChannelsSectionId(string sectionId)
        {
            return string.Equals(sectionId, RoughnessDataRegion.SectionId.DefaultValue, StringComparison.InvariantCultureIgnoreCase);
        }

        private static CrossSectionSectionType GetCrossSectionSectionType(string sectionTypeName, IHydroNetwork network)
        {
            lock (network.CrossSectionSectionTypes)
            {
                var crossSectionSectionType = network.CrossSectionSectionTypes.FirstOrDefault(cst => string.Equals(cst.Name, sectionTypeName, StringComparison.InvariantCultureIgnoreCase));
                if (crossSectionSectionType == null)
                {
                    crossSectionSectionType = new CrossSectionSectionType { Name = sectionTypeName };
                    network.CrossSectionSectionTypes.Add(crossSectionSectionType);
                }
                return crossSectionSectionType;
            }
        }

        private static HashSet<string> GetDuplicateDefinitionIds(IEnumerable<CrossSectionLocation> crossSectionLocations) => 
            new HashSet<string>(crossSectionLocations.Duplicates(l => l.DefinitionId));

    }
}