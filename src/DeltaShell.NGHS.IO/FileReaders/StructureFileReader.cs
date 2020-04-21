using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.Helpers;
using DelftTools.Hydro.SewerFeatures;
using DelftTools.Hydro.Structures;
using DelftTools.Utils.Collections;
using DeltaShell.NGHS.IO.FileWriters.CrossSectionDefinition;
using DeltaShell.NGHS.IO.FileWriters.Structure;
using DeltaShell.NGHS.IO.Helpers;
using GeoAPI.Extensions.Networks;

namespace DeltaShell.NGHS.IO.FileReaders 
{
    public static class StructureFileReader
    {
        public static void ReadFile(string structureFilename, string csdFilename, IHydroNetwork network)
        {
            var fileReadingExceptions = new List<FileReadingException>();
            var crossSectionDefinitions = new List<ICrossSectionDefinition>();

            if (fileReadingExceptions.Count > 0)
            {
                var innerExceptionMessages = fileReadingExceptions.Select(fileReadingException => fileReadingException.InnerException.Message + Environment.NewLine);
                throw new FileReadingException(string.Format("While reading cross section definitions for structures an error occured :{0} {1}", Environment.NewLine, string.Join(Environment.NewLine, innerExceptionMessages)));
            }

            var structuresCategories = ReadStructureDelftIniCategories(structureFilename);
            if (structuresCategories.Count == 0)
                throw new FileReadingException(string.Format("Could not read file {0} properly, it seems empty", structureFilename));

            var structures = GetAllStructuresFromCategories(structuresCategories, crossSectionDefinitions, network, fileReadingExceptions);
            if (fileReadingExceptions.Count > 0)
            {
                var innerExceptionMessages = fileReadingExceptions.Select(fileReadingException => fileReadingException.InnerException.Message + Environment.NewLine);
                throw new FileReadingException(string.Format("While reading cross section definitions for structures an error occured :{0} {1}", Environment.NewLine, string.Join(Environment.NewLine, innerExceptionMessages)));
            }
            
            // do not add crossSectionDefinitions => already added
            AddStructuresToNetwork(structures);

            if (fileReadingExceptions.Count > 0)
            {
                var innerExceptionMessages = fileReadingExceptions.Select(fileReadingException => fileReadingException.InnerException.Message + Environment.NewLine);
                throw new FileReadingException(string.Format("While reading structures an error occured :{0} {1}", Environment.NewLine, string.Join(Environment.NewLine, innerExceptionMessages)));
            }
        }
        
        private static void AddStructuresToNetwork(IList<IStructure1D> structures)
        {
            var compoundStructures = structures.Where(s => s.GetStructureType() == StructureType.CompositeBranchStructure);
            var nonCoumpoundStructures = structures.Where(s => s.GetStructureType() != StructureType.CompositeBranchStructure);

            AddCompositeStructuresToNetwork(compoundStructures);

            var grouping = nonCoumpoundStructures.GroupBy(s => s.Branch);

            foreach (var group in grouping)
            {
                group.ForEach(s =>
                {
                    if (@group.Key is ISewerConnection sewerConnection && sewerConnection.IsInternalConnection())
                    {
                        sewerConnection.AddStructureToBranch(s);
                        foreach (var pointFeature in sewerConnection.BranchFeatures.OfType<IPointFeature>())
                        {
                            pointFeature.ParentPointFeature = sewerConnection.Source as IManhole;
                        }
                    }
                    else
                    {
                        HydroNetworkHelper.AddStructureToExistingCompositeStructureOrToANewOne(s, @group.Key);
                    }
                    
                });
            }
        }

        private static void AddCompositeStructuresToNetwork(IEnumerable<IStructure1D> compoundStructures)
        {
            compoundStructures.ForEach(c =>
            {
                var compositeBranchStructure = new CompositeBranchStructure
                {
                    Branch = c.Branch,
                    Network = c.Branch.Network,
                    Chainage = c.Branch.CorrectlyRoundOffChainageIfChainageIsOnEndOfBranch(c.Chainage),
                };
                compositeBranchStructure.Name = c.Name;
                compositeBranchStructure.LongName = c.LongName;
                compositeBranchStructure.Geometry = HydroNetworkHelper.GetStructureGeometry(c.Branch, compositeBranchStructure.Chainage);
                c.Branch.BranchFeatures.Add(compositeBranchStructure);
            });

        }

        private static IList<DelftIniCategory> ReadStructureDelftIniCategories(string structureFilename)
        {
            if (!File.Exists(structureFilename))
                throw new FileReadingException(string.Format("Could not read file {0} properly, it doesn't exist.", structureFilename));

            var structuresCategories = new DelftIniReader().ReadDelftIniFile(structureFilename);
            if (structuresCategories.Count == 0)
                throw new FileReadingException(string.Format("Could not read file {0} properly, it seems empty", structureFilename));

            return structuresCategories;
        }

        private static IList<IStructure1D> GetAllStructuresFromCategories(IList<DelftIniCategory> structuresCategories,
            IList<ICrossSectionDefinition> crossSectionDefinitions, IHydroNetwork network, IList<FileReadingException> fileReadingExceptions)
        {
            IList<IStructure1D> structure1Ds = new List<IStructure1D>();
            var branchLookup = network.Branches.Where(b => !string.IsNullOrEmpty(b.Name)).ToDictionary(b => b.Name);
            var structureNameLookup = new HashSet<string>();

            foreach (var structureDefinitionCategory in structuresCategories.Where(category => category.Name == StructureRegion.Header))
            {
                try
                {
                    var branchId = structureDefinitionCategory.ReadProperty<string>(StructureRegion.BranchId.Key, true);
                    if (string.IsNullOrWhiteSpace(branchId) || !branchLookup.TryGetValue(branchId, out var branch))
                    {
                        continue;
                    }

                    var structure1D = ReadStructureDefinition(structureDefinitionCategory, crossSectionDefinitions, branch);
                    if (structure1D == null)
                    {
                        continue;
                    }
                    
                    if (structureNameLookup.Contains(structure1D.Name))
                    {
                        throw new FileReadingException(string.Format(
                            "Structure with id {0} is already read, id's CAN NOT be duplicates!",
                            structure1D.Name));
                    }

                    structureNameLookup.Add(structure1D.Name);
                    structure1Ds.Add(structure1D);
                }
                catch (FileReadingException fileReadingException)
                {
                    fileReadingExceptions.Add(new FileReadingException("Could not structure.",
                        fileReadingException));
                }
            }

            return structure1Ds;
        }

        private static IStructure1D ReadStructureDefinition(DelftIniCategory definitionCategory, IList<ICrossSectionDefinition> crossSectionDefinitions, IBranch branch)
        {
            var type = definitionCategory.ReadProperty<string>(StructureRegion.DefinitionType.Key);

            if (!Enum.TryParse(type, true, out StructureType structureType))
            {
                if (type == "compound")
                {
                    structureType = StructureType.CompositeBranchStructure;
                }
                else
                {
                    throw new FileReadingException(string.Format("Couldn't parse this type '{0}' to an element of the structure type enum", type));
                }
            }

            var definitionReader = DefinitionGeneratorFactory.GetDefinitionReaderStructure(structureType);
            if (definitionReader == null)
            {
                throw new FileReadingException(string.Format("No definition reader available for this structure definition: {0}", type));
            }

            return definitionReader.ReadDefinition(definitionCategory, crossSectionDefinitions, branch);
        }
    }
}