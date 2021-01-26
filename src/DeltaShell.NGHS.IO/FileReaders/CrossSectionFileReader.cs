using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.Roughness;
using DelftTools.Hydro.SewerFeatures;
using DelftTools.Hydro.Structures;
using DelftTools.Utils.Collections;
using DeltaShell.NGHS.IO.FileWriters.CrossSectionDefinition;
using DeltaShell.NGHS.IO.FileWriters.Location;
using DeltaShell.NGHS.IO.Helpers;

namespace DeltaShell.NGHS.IO.FileReaders
{
    public static class CrossSectionFileReader
    {
        public static void ReadFile(
            string cslFilename,
            string csdFilename,
            IHydroNetwork network,
            string defaultFrictionId,
            Action<IChannel> onAddingCrossSectionWithFrictionToBranch)
        {
            IList<DelftIniCategory> cslCategories = new List<DelftIniCategory>();
            if (File.Exists(cslFilename))
            {
                cslCategories = new DelftIniReader().ReadDelftIniFile(cslFilename);
            }
            IList<DelftIniCategory> csIniLocations = new List<DelftIniCategory>();
            if (cslCategories.Count != 0) //throw new FileReadingException(string.Format("Could not read file {0} properly, it seems empty", cslFilename));
            {
                csIniLocations = cslCategories.Where(category => category.Name == CrossSectionRegion.IniHeader).ToList();
                if (!csIniLocations.Any()) throw new FileReadingException("Could not read any cross section locations it seems not available");
            }

            if (!File.Exists(csdFilename)) throw new FileReadingException(string.Format("Could not read file {0} properly, it doesn't exist.", csdFilename));
            var csdCategories = new DelftIniReader().ReadDelftIniFile(csdFilename);
            if (csdCategories.Count == 0) throw new FileReadingException(string.Format("Could not read file {0} properly, it seems empty", csdFilename));

            IList<FileReadingException> fileReadingExceptions = new List<FileReadingException>();

            var nonStructureCrossSectionDefinitions = csIniLocations.Select(csIniLocation => csIniLocation.ReadProperty<string>(LocationRegion.Definition.Key)).Distinct().ToArray();

            IList<ICrossSectionDefinition> crossSectionDefinitions = new List<ICrossSectionDefinition>();
            IList<ICrossSectionDefinition> crossSectionDefinitionsWithFriction = new List<ICrossSectionDefinition>();
            IList<ICrossSectionDefinition> sharedNotConnectedCrossSectionDefinitions = new List<ICrossSectionDefinition>();
            foreach (var csdDefinitionCategory in csdCategories.Where(category => category.Name.Equals(DefinitionPropertySettings.Header, StringComparison.InvariantCultureIgnoreCase)).OrderByDescending(cat => cat.ReadProperty<bool>(DefinitionPropertySettings.IsShared.Key, true)))
            {
                try
                {
                    var crossSectionDefinition = TransformDefinitionCategoryIntoCrossSectionDefinition(
                        csdDefinitionCategory, network, nonStructureCrossSectionDefinitions, defaultFrictionId,
                        out var hasFriction);

                    if (crossSectionDefinitions.Contains(crossSectionDefinition) 
                        || ( crossSectionDefinitions.Any(csd => csd.Name.Equals(crossSectionDefinition.Name, StringComparison.InvariantCultureIgnoreCase)) 
                             && !sharedNotConnectedCrossSectionDefinitions.Any( scsd => scsd.Name.Equals(crossSectionDefinition.Name, StringComparison.InvariantCultureIgnoreCase))))
                        throw new FileReadingException(string.Format("cross section definition with id {0} is already read, id's CAN NOT be duplicates!", crossSectionDefinition.Name));
                    
                    if (csdDefinitionCategory.ReadProperty<bool>(DefinitionPropertySettings.IsShared.Key, true))
                    {
                        sharedNotConnectedCrossSectionDefinitions.Add(crossSectionDefinition);
                    }
                    
                    crossSectionDefinitions.Add(crossSectionDefinition);

                    if (hasFriction)
                    {
                        crossSectionDefinitionsWithFriction.Add(crossSectionDefinition);
                    }
                }
                catch (FileReadingException fileReadingException)
                {
                    fileReadingExceptions.Add(new FileReadingException("Could not read cross section definition", fileReadingException));
                }
            }
            var crsDefCoupledToCrossSection = new List<ICrossSectionDefinition>();
            foreach (var csIniLocation in csIniLocations)
            {
                var crsLocationId = csIniLocation.ReadProperty<string>(LocationRegion.Id.Key);
                var crsLocationDefinitionId = csIniLocation.ReadProperty<string>(LocationRegion.Definition.Key);
                if ( network.CrossSections.Any(crs => crs.Name.Equals(crsLocationId, StringComparison.InvariantCultureIgnoreCase))) continue;
                foreach (var crossSectionDefinition in crossSectionDefinitions.Where(crsd => crsd.Name.Equals(crsLocationDefinitionId, StringComparison.InvariantCultureIgnoreCase)))
                {
                    try
                    {
                        var crossSectionLocationInfos = csIniLocations.Where(location =>
                        {
                            var crossSectionLocationDefinitionId =
                                location.ReadProperty<string>(LocationRegion.Definition.Key);
                            return crossSectionLocationDefinitionId == crossSectionDefinition.Name;
                        }).ToArray();

                        if (!crossSectionLocationInfos.Any())
                        {
                            if (sharedNotConnectedCrossSectionDefinitions.Contains(crossSectionDefinition))
                            {
                                network.SharedCrossSectionDefinitions.Add(crossSectionDefinition);
                            }
                            else
                            {
                                PlaceDefinitionOnBridgeOrCulvert(crossSectionDefinition,
                                    network.Bridges.Concat(
                                        network.Culverts.Cast<IStructureWithCrossSectionDefinition>()));
                            }

                            continue;
                        }

                        
                        var crossSectionLocationInfo = crossSectionLocationInfos.Length > 1
                            ? crossSectionLocationInfos
                                  .FirstOrDefault(cslInfo =>
                                      cslInfo.ReadProperty<string>(LocationRegion.Definition.Key)
                                          .Equals(crossSectionDefinition.Name,
                                              StringComparison.InvariantCultureIgnoreCase)
                                      && cslInfo.ReadProperty<string>(LocationRegion.Id.Key)
                                          .Equals(crsLocationId,
                                              StringComparison.InvariantCultureIgnoreCase))
                              ?? crossSectionLocationInfos.FirstOrDefault(cslInfo => cslInfo
                                  .ReadProperty<string>(LocationRegion.Definition.Key)
                                  .Equals(crossSectionDefinition.Name, StringComparison.InvariantCultureIgnoreCase))
                            : crossSectionLocationInfos.FirstOrDefault(cslInfo =>
                                cslInfo.ReadProperty<string>(LocationRegion.Definition.Key)
                                    .Equals(crossSectionDefinition.Name, StringComparison.InvariantCultureIgnoreCase));


                        if (crossSectionLocationInfo == null)
                        {
                            fileReadingExceptions.Add(new FileReadingException(
                                "Could not find the location for cross section definition : " +
                                crossSectionDefinition.Name));
                            continue;
                        }

                        var isSharedCrossSection = crossSectionLocationInfos.Length > 1;
                        var crossSectionDefinitionName = crossSectionDefinition.Name;
                            var crossSection = network.CrossSections.SingleOrDefault(cs => cs.Name.Equals(crsLocationId, StringComparison.InvariantCultureIgnoreCase)) ??
                                           network.AddCrossSection(crossSectionLocationInfo, crsLocationId, crossSectionDefinition,isSharedCrossSection);

                        var hasFriction = false;
                        if (crossSectionDefinitionsWithFriction.Contains(crossSectionDefinition) &&
                            crossSection?.Branch is Channel)
                        {
                            hasFriction = true;
                            onAddingCrossSectionWithFrictionToBranch((IChannel) crossSection.Branch);
                        }

                        if (isSharedCrossSection)
                        {
                            if (crossSection != null && crossSection.Definition != null &&
                                crossSectionDefinition != null &&
                                !crossSectionDefinition.GeometryBased) //"XYZ definitions can not be shared"
                            {
                                crossSectionDefinition.Name = crossSectionDefinitionName; 
                                if (!network.SharedCrossSectionDefinitions.Any(scsd =>
                                    scsd.Name.Equals(crossSectionDefinitionName,
                                        StringComparison.InvariantCultureIgnoreCase)) && !network.SharedCrossSectionDefinitions.Contains(crossSectionDefinition))
                                {
                                    network.SharedCrossSectionDefinitions.Add(crossSectionDefinition);
                                }

                                var shiftLevel =
                                    crossSectionLocationInfo.ReadProperty<double>(LocationRegion.Shift.Key);

                                var definition = network.SharedCrossSectionDefinitions.SingleOrDefault(scsd =>
                                    scsd.Name.Equals(crossSectionDefinitionName,
                                        StringComparison.InvariantCultureIgnoreCase));
                                if (definition == null)
                                {
                                    fileReadingExceptions.Add(new FileReadingException(
                                        "There was no single shared cross section definition with only this name: " +
                                        crossSectionDefinitionName));
                                    continue;
                                }

                                crossSection.UseSharedDefinition(definition);
                                crossSection.Definition.ShiftLevel(shiftLevel);


                                foreach (var sectionLocationInfo in crossSectionLocationInfos.Except(new[]
                                    {crossSectionLocationInfo}))
                                {
                                    var locationShift =
                                        sectionLocationInfo.ReadProperty<double>(LocationRegion.Shift.Key);
                                    var locationOtherId =
                                        sectionLocationInfo.ReadProperty<string>(LocationRegion.Id.Key);
                                    var sharedCrossSection = network.CrossSections.SingleOrDefault(cs =>
                                        cs.Name.Equals(locationOtherId, StringComparison.InvariantCultureIgnoreCase));
                                    if (sharedCrossSection != null)
                                    {
                                        sharedCrossSection.UseSharedDefinition(definition);
                                        sharedCrossSection.Definition.ShiftLevel(locationShift);
                                    }
                                    else
                                    {
                                        crossSection = network.AddCrossSection(sectionLocationInfo, locationOtherId, definition, false);
                                        definition.Name = crossSectionDefinitionName; // stupid thing
                                        crossSection.UseSharedDefinition(definition);
                                        crossSection.Definition.ShiftLevel(locationShift);
                                    }

                                    if (hasFriction && crossSection?.Branch is Channel)
                                    {
                                        onAddingCrossSectionWithFrictionToBranch((IChannel) crossSection.Branch);
                                    }
                                }
                            }
                        }
                        else if (sharedNotConnectedCrossSectionDefinitions.Contains(crossSectionDefinition))
                        {
                            if (crossSection != null && crossSection.Definition != null)
                            {
                                crossSectionDefinition.Name = crossSectionDefinitionName;
                                if (!network.SharedCrossSectionDefinitions.Any(scsd => scsd.Name.Equals(crossSectionDefinitionName, StringComparison.InvariantCultureIgnoreCase)) &&  !network.SharedCrossSectionDefinitions.Contains(crossSectionDefinition))
                                {
                                    network.SharedCrossSectionDefinitions.Add(crossSectionDefinition);
                                }
                                var shiftLevel = crossSectionLocationInfo.ReadProperty<double>(LocationRegion.Shift.Key);

                                var definition = network.SharedCrossSectionDefinitions.SingleOrDefault(scsd => scsd.Name.Equals(crossSectionDefinitionName, StringComparison.InvariantCultureIgnoreCase));
                                if (definition == null)
                                {
                                    fileReadingExceptions.Add(new FileReadingException("There was no single shared cross section definition with only this name: " + crossSectionDefinitionName));
                                    continue;
                                }
                                crossSection.UseSharedDefinition(definition);
                                crossSection.Definition.ShiftLevel(shiftLevel);
                            }
                        }
                        crsDefCoupledToCrossSection.Add(crossSectionDefinition);
                    }
                    catch (FileReadingException fileReadingException)
                    {
                        fileReadingExceptions.Add(
                            new FileReadingException("Could not read cross section location info data",
                                fileReadingException));
                    }
                }
            }

            foreach (var crossSectionDefinition in crossSectionDefinitions.Except(crsDefCoupledToCrossSection))
            {
                if(network.Culverts.Select(c => c.CrossSectionDefinition).Concat(network.Bridges.Select(c => c.CrossSectionDefinition)).Any(csd => csd.Name.Equals(crossSectionDefinition.Name, StringComparison.InvariantCultureIgnoreCase))) continue;
                
                network.SharedCrossSectionDefinitions.Add(crossSectionDefinition);
            }

            if (fileReadingExceptions.Count > 0)
            {
                var innerExceptionMessages = fileReadingExceptions.Select(fileReadingException => fileReadingException?.InnerException != null 
                    ? fileReadingException.InnerException.Message + Environment.NewLine 
                    : string.Empty
                    );
                throw new FileReadingException(
                    $"While reading cross sections an error occured :{Environment.NewLine} {string.Join(Environment.NewLine, innerExceptionMessages)}"
                    );
            }
        }

        private static void PlaceDefinitionOnBridgeOrCulvert(ICrossSectionDefinition crossSectionDefinition, IEnumerable<IStructureWithCrossSectionDefinition> structureWithCrossSectionDefinitions)
        {
            //TODO: Waarom ligt dit niet bij de structures zelf?
            structureWithCrossSectionDefinitions
                .Where(s => s.CrossSectionDefinition.Name.Equals(crossSectionDefinition.Name, StringComparison.InvariantCultureIgnoreCase))
                .ForEach(
                    structureWithCrossSectionDefinition =>
                    {
                        structureWithCrossSectionDefinition.CrossSectionDefinition = crossSectionDefinition;
                    });

        }

        private static CrossSection AddCrossSection(this IHydroNetwork network,
            DelftIniCategory crossSectionLocationInfo, string crsLocationId,
            ICrossSectionDefinition crossSectionDefinition, bool isFirstOfSharedCrossSectionDefinitions)
        {
            var branchId = crossSectionLocationInfo.ReadProperty<string>(LocationRegion.BranchId.Key);
            var chainage = crossSectionLocationInfo.ReadProperty<double>(LocationRegion.Chainage.Key);
            var crossSectionName = crsLocationId;

            /*optional err message needs to be handled*/
            var crossSectionLongName = crossSectionLocationInfo.ReadProperty<string>(LocationRegion.Name.Key, true);

            var branch = network.Branches.FirstOrDefault(b => b.Name == branchId);
            if (branch == null)
                throw new FileReadingException(string.Format(
                    "The read cross section '{0}' has a branch id ({1}) which is not available in the model.",
                    crossSectionName,
                    branchId));
            if (branch is IPipe pipe)
            {
                pipe.CrossSection = new CrossSection(crossSectionDefinition);
                if (Math.Abs(chainage) < 0.001) // chainage = 0, so set source
                    pipe.LevelSource = crossSectionLocationInfo.ReadProperty<double>(LocationRegion.Shift.Key);
                else
                    pipe.LevelTarget = crossSectionLocationInfo.ReadProperty<double>(LocationRegion.Shift.Key);
                
                return null;
            }
            else
            {
                var crossSection = new CrossSection(crossSectionDefinition)
                {
                    Name = crossSectionName,
                    LongName = crossSectionLongName,
                    Chainage = branch.CorrectlyRoundOffChainageIfChainageIsOnEndOfBranch(chainage)
                };
                branch.BranchFeatures.Add(crossSection);
                return crossSection;
            }

            
        }

        public static ICrossSectionDefinition TransformDefinitionCategoryIntoCrossSectionDefinition(
            IDelftIniCategory crossSectionDefinitionCategory,
            IHydroNetwork network,
            string[] nonStructureCrossSectionDefinitions,
            string defaultFrictionId, out bool hasFriction)
        {
            hasFriction = false;
            var typeProperty = crossSectionDefinitionCategory.ReadProperty<string>(DefinitionPropertySettings.DefinitionType.Key);
                
            var templateProperty = crossSectionDefinitionCategory.ReadProperty<string>(DefinitionPropertySettings.Template.Key,true);

            var definitionReader = DefinitionGeneratorFactory.GetDefinitionReaderCrossSection(typeProperty, templateProperty);
            if (definitionReader == null)
            {
                var errorMessage = "No definition reader available for this cross section definition";
                throw new FileReadingException(errorMessage);
            }

            var readCrossSectionDefinition = definitionReader.ReadDefinition(crossSectionDefinitionCategory);

            // Don't set friction for structure related cross sections
            if (nonStructureCrossSectionDefinitions != null && (nonStructureCrossSectionDefinitions.Contains(readCrossSectionDefinition.Name) || crossSectionDefinitionCategory.ReadProperty<bool>(DefinitionPropertySettings.IsShared.Key, true)))
            {
                SetFrictionOnCrossSectionDefinition(crossSectionDefinitionCategory, readCrossSectionDefinition, network,
                    defaultFrictionId, out hasFriction);
            }

            return readCrossSectionDefinition;
        }

        private static void SetFrictionOnCrossSectionDefinition(IDelftIniCategory csdDefinitionCategory,
            ICrossSectionDefinition readCrossSectionDefinition,
            IHydroNetwork network,
            string defaultFrictionId,
            out bool hasFriction)
        {
            hasFriction = false;

            if (readCrossSectionDefinition.CrossSectionType == CrossSectionType.YZ || readCrossSectionDefinition.CrossSectionType == CrossSectionType.GeometryBased)
            {
                var frictionIds = csdDefinitionCategory.ReadPropertiesToListOfType<string>(DefinitionPropertySettings.FrictionIds.Key,true,';');
                if (frictionIds == null) return;
                if (frictionIds.Count < 0 )
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

                if (frictionPositions.Count  != frictionIds.Count+1)
                    throw new FileReadingException("reading error");

                readCrossSectionDefinition.Sections.Clear();
                
                for (int index = 0; index < frictionIds.Count; index++)
                {
                    var networkSectionType = GetCrossSectionSectionType(frictionIds[index], network);

                    readCrossSectionDefinition.Sections.Add(new CrossSectionSection
                    {
                        SectionType = networkSectionType,
                        MinY = frictionPositions[index],
                        MaxY = frictionPositions[index+1]
                    });
                }

                hasFriction = true;
            }

            if (readCrossSectionDefinition.CrossSectionType == CrossSectionType.ZW)
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
                var floodPlain1Width = csdDefinitionCategory.ReadProperty<double>(DefinitionPropertySettings.FloodPlain1.Key,true);
           
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

                hasFriction = true;
            }

            if (readCrossSectionDefinition.CrossSectionType == CrossSectionType.Standard)
            {
                var frictionIds = csdDefinitionCategory.ReadPropertiesToListOfType<string>(DefinitionPropertySettings.FrictionId.Key, true,';');
                if (frictionIds == null ) return;
                
                if (frictionIds.Count != 1)
                    throw new FileReadingException("reading error");

                var sectionTypeName = frictionIds.FirstOrDefault();
                if (sectionTypeName == null)
                    throw new FileReadingException("reading error");

                if (sectionTypeName.Equals(defaultFrictionId))
                {
                    readCrossSectionDefinition.Sections.Add(new CrossSectionSection { SectionType = GetCrossSectionSectionType(RoughnessDataSet.MainSectionTypeName, network), MinY = readCrossSectionDefinition.Left, MaxY = readCrossSectionDefinition.Right});
                    return;
                }

                readCrossSectionDefinition.Sections.Add(new CrossSectionSection{SectionType = GetCrossSectionSectionType(sectionTypeName, network), MinY = readCrossSectionDefinition.Left, MaxY = readCrossSectionDefinition.Right });

                hasFriction = true;
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