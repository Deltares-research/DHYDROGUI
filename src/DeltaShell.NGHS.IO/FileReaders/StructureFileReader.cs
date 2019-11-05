using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.CrossSections;
using DeltaShell.NGHS.IO.FileWriters.CrossSectionDefinition;
using DeltaShell.NGHS.IO.FileWriters.Structure;
using DeltaShell.NGHS.IO.Helpers;

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
                throw new FileReadingException(string.Format("Could not read file {0} properly, it seems empty", csdFilename));

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
            var grouping = structures.GroupBy(s => s.Branch);

            foreach (var group in grouping)
            {
                group.Key.BranchFeatures.AddRange(group);
            }
        }

        private static IList<DelftIniCategory> ReadStructureDelftIniCategories(string structureFilename)
        {
            if (!File.Exists(structureFilename))
                throw new FileReadingException(string.Format("Could not read file {0} properly, it doesn't exist.",
                    structureFilename));

            var structuresCategories = new DelftIniReader().ReadDelftIniFile(structureFilename);
            if (structuresCategories.Count == 0)
                throw new FileReadingException(string.Format("Could not read file {0} properly, it seems empty",
                    structureFilename));

            return structuresCategories;
        }

        private static IList<ICrossSectionDefinition> GetCrossSectionDefinitions(IHydroNetwork network,string csdFilename, IList<FileReadingException> fileReadingExceptions)
        {
            if (!File.Exists(csdFilename))
                throw new FileReadingException(string.Format("Could not read file {0} properly, it doesn't exist.", csdFilename));

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
            IList<IStructure1D> structureDefinitions = new List<IStructure1D>();

            foreach (var structureDefinitionCategory in structuresCategories.Where(category => category.Name == StructureRegion.Header))
            {
                try
                {
                    var structureDefinition = ReadStructureDefinition(structureDefinitionCategory);

                    if (structureDefinitions.Contains(structureDefinition) ||
                        structureDefinitions.FirstOrDefault(csd => csd.Name == structureDefinition.Name) != null)

                        throw new FileReadingException(string.Format(
                            "cross section definition with id {0} is already read, id's CAN NOT be duplicates!",
                            structureDefinition.Name));

                    structureDefinitions.Add(structureDefinition);
                }
                catch (FileReadingException fileReadingException)
                {
                    fileReadingExceptions.Add(new FileReadingException("Could not read cross section definition for structures",
                        fileReadingException));
                }
            }

            return structureDefinitions;
        }

        private static IStructure1D ReadStructureDefinition(IDelftIniCategory definitionCategory)
        {
            var type = definitionCategory.ReadProperty<string>(StructureRegion.DefinitionType.Key);

            if (!Enum.TryParse(type, true, out StructureType structureType))
            {
                throw new FileReadingException(string.Format("Couldn't parse this type '{0}' to an element of the structure type enum", type));
            }

            var definitionReader = DefinitionGeneratorFactory.GetDefinitionReaderStructure(structureType);
            if (definitionReader == null)
            {
                throw new FileReadingException(string.Format("No definition reader available for this structure definition: {0}",type));
            }

            return definitionReader.ReadStructureDefinition(definitionCategory);
        }
    }
}