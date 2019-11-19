using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.Structures;
using DelftTools.Utils.Collections;
using DeltaShell.NGHS.IO.FileWriters.CrossSectionDefinition;
using DeltaShell.NGHS.IO.FileWriters.Structure;
using DeltaShell.NGHS.IO.Helpers;
using GeoAPI.Extensions.Networks;
using GeoAPI.Geometries;
using NetTopologySuite.Extensions.Networks;
using NetTopologySuite.Geometries;
using NetTopologySuite.LinearReferencing;

namespace DeltaShell.NGHS.IO.FileReaders 
{
    public static class StructureFileReader
    {
        public static void ReadFile(string structureFilename, string csdFilename, IHydroNetwork network)
        {
            var fileReadingExceptions = new List<FileReadingException>();
            var crossSectionDefinitions = GetCrossSectionDefinitions(network, csdFilename, fileReadingExceptions);

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

            AddSubStructuresToCompositeStructuresOrAddNew(structures);

            // do not add crossSectionDefinitions => already added
            AddStructuresToNetwork(structures);

            // update geometry based on branch chainage
            structures.ForEach(s => s.Geometry = GetStructureGeometry(s));

            if (fileReadingExceptions.Count > 0)
            {
                var innerExceptionMessages = fileReadingExceptions.Select(fileReadingException => fileReadingException.InnerException.Message + Environment.NewLine);
                throw new FileReadingException(string.Format("While reading structures an error occured :{0} {1}", Environment.NewLine, string.Join(Environment.NewLine, innerExceptionMessages)));
            }
        }

        private static IGeometry GetStructureGeometry(IStructure1D structure1D)
        {
            var lengthIndexedLine = new LengthIndexedLine(structure1D.Branch.Geometry);
            var mapOffset = NetworkHelper.MapChainage(structure1D.Branch, structure1D.Chainage);
            return new Point((Coordinate)lengthIndexedLine.ExtractPoint(mapOffset).Clone());
        }

        private static void AddSubStructuresToCompositeStructuresOrAddNew(IList<IStructure1D> structures)
        {
            var compositeBranchStructures = structures.OfType<ICompositeBranchStructure>().ToList();
            compositeBranchStructures.ForEach(comp =>
            {
                var structureIds = comp.Tag as string;
                if (string.IsNullOrWhiteSpace(structureIds))
                {
                    return;
                }

                // todo : think about caching structures and name as a lookup
                structureIds.Split(new[] {';'}, StringSplitOptions.RemoveEmptyEntries)
                    .Select(sId => structures.FirstOrDefault(st => st.Name == sId))
                    .Where(s => s != null)
                    .ForEach(s =>
                    {
                        s.Geometry = comp.Geometry;
                        s.ParentStructure = comp;
                        comp.Structures.Add(s);
                    });
            });

            // generate composite structures for single structures
            var singleStructures = structures.Where(s => s.ParentStructure == null).ToList();

            singleStructures.ForEach((s, i) =>
            {
                var compositeStructure = new CompositeBranchStructure
                {
                    Name = $"Composite structure generated {i}",
                    Branch = s.Branch,
                    Chainage = s.Chainage,
                    Geometry = s.Geometry
                };

                structures.Add(compositeStructure);

                compositeStructure.Structures.Add(s);
                s.ParentStructure = compositeStructure;
            });
        }

        private static void AddStructuresToNetwork(IList<IStructure1D> structures)
        {
            var grouping = structures.GroupBy(s => s.Branch);

            foreach (var group in grouping)
            {
                group.Key.BranchFeatures.AddRange(group);
            }
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

        private static IList<ICrossSectionDefinition> GetCrossSectionDefinitions(IHydroNetwork network,string csdFilename, IList<FileReadingException> fileReadingExceptions)
        {
            if (!File.Exists(csdFilename))
            {
                return new List<ICrossSectionDefinition>();
            }
            //throw new FileReadingException(string.Format("Could not read file {0} properly, it doesn't exist.", csdFilename));

            var csdCategories = new DelftIniReader().ReadDelftIniFile(csdFilename);

            IList<ICrossSectionDefinition> crossSectionDefinitions = new List<ICrossSectionDefinition>();
            foreach (var csdDefinitionCategory in csdCategories.Where(category =>
                category.Name == DefinitionPropertySettings.Header))
            {
                try
                {
                    var crossSectionDefinition =
                        CrossSectionFileReader.TransformDefinitionCategoryIntoCrossSectionDefinition(csdDefinitionCategory,
                            network);
                    if (crossSectionDefinitions.Contains(crossSectionDefinition) ||
                        crossSectionDefinitions.FirstOrDefault(csd => csd.Name == crossSectionDefinition.Name) != null)
                        throw new FileReadingException(string.Format(
                            "cross section definition with id {0} is already read, id's CAN NOT be duplicates!",
                            crossSectionDefinition.Name));

                    crossSectionDefinitions.Add(crossSectionDefinition);
                }
                catch (FileReadingException fileReadingException)
                {
                    fileReadingExceptions.Add(new FileReadingException("Could not read cross section definition for structures",
                        fileReadingException));
                }
            }

            return crossSectionDefinitions;
        }

        private static IList<IStructure1D> GetAllStructuresFromCategories(IList<DelftIniCategory> structuresCategories,
            IList<ICrossSectionDefinition> crossSectionDefinitions, IHydroNetwork network, IList<FileReadingException> fileReadingExceptions)
        {
            IList<IStructure1D> structure1Ds = new List<IStructure1D>();
            var branchLookup = network.Branches.Where(b => b.Name != null).ToDictionary(b => b.Name);
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