using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.Structures;
using DeltaShell.NGHS.IO;
using DeltaShell.NGHS.IO.FileReaders;
using DeltaShell.NGHS.IO.FileWriters;
using DeltaShell.NGHS.IO.FileWriters.CrossSectionDefinition;
using DeltaShell.NGHS.IO.FileWriters.Structure;
using DeltaShell.NGHS.IO.Helpers;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport
{
    public static class StructureFileReader
    {
        public static void ReadFile(string structureFilename, string csdFilename, WaterFlowModel1D waterFlowModel1D)
        {
            if (!File.Exists(structureFilename)) throw new FileReadingException(string.Format("Could not read file {0} properly, it doesn't exist.", structureFilename));
            var structuresCategories = new DelftIniReader().ReadDelftIniFile(structureFilename);
            if (structuresCategories.Count == 0) throw new FileReadingException(string.Format("Could not read file {0} properly, it seems empty", structureFilename));
            
            if (!File.Exists(csdFilename)) throw new FileReadingException(string.Format("Could not read file {0} properly, it doesn't exist.", csdFilename));
            var csdCategories = new DelftIniReader().ReadDelftIniFile(csdFilename);
            if (csdCategories.Count == 0) throw new FileReadingException(string.Format("Could not read file {0} properly, it seems empty", csdFilename));

            IList<FileReadingException> fileReadingExceptions = new List<FileReadingException>();

            IList<ICrossSectionDefinition> crossSectionDefinitions = new List<ICrossSectionDefinition>();
            foreach (var csdDefinitionCategory in csdCategories.Where(category => category.Name == DefinitionPropertySettings.Header))
            {
                try
                {
                    var crossSectionDefinition = CrossSectionFileReader.ReadCSDDefinition(csdDefinitionCategory, waterFlowModel1D);
                    if (crossSectionDefinitions.Contains(crossSectionDefinition) || crossSectionDefinitions.FirstOrDefault(csd => csd.Name == crossSectionDefinition.Name) != null)
                        throw new FileReadingException(string.Format("cross section definition with id {0} is already read, id's CAN NOT be duplicates!", crossSectionDefinition.Name));

                    crossSectionDefinitions.Add(crossSectionDefinition);
                }
                catch (FileReadingException fileReadingException)
                {
                    fileReadingExceptions.Add(new FileReadingException("Could not read cross section definition for structures", fileReadingException));
                }
            }

            if (fileReadingExceptions.Count > 0)
            {
                var innerExceptionMessages = fileReadingExceptions.Select(fileReadingException => fileReadingException.InnerException.Message + Environment.NewLine);
                throw new FileReadingException(string.Format("While reading cross section definitions for structures an error occured :{0} {1}", Environment.NewLine, string.Join(Environment.NewLine, innerExceptionMessages)));
            }

            IList<IStructure1D> structureDefinitions = new List<IStructure1D>();
            foreach (var structureDefinitionCategory in structuresCategories.Where(category => category.Name == StructureRegion.Header))
            {
                try
                {
                    var structureDefinition = ReadStructureDefinition(structureDefinitionCategory);
                    if (structureDefinitions.Contains(structureDefinition) || structureDefinitions.FirstOrDefault(csd => csd.Name == structureDefinition.Name) != null)
                        throw new FileReadingException(string.Format("cross section definition with id {0} is already read, id's CAN NOT be duplicates!", structureDefinition.Name));

                    structureDefinitions.Add(structureDefinition);
                }
                catch (FileReadingException fileReadingException)
                {
                    fileReadingExceptions.Add(new FileReadingException("Could not read cross section definition for structures", fileReadingException));
                }
            }

            if (fileReadingExceptions.Count > 0)
            {
                var innerExceptionMessages = fileReadingExceptions.Select(fileReadingException => fileReadingException.InnerException.Message + Environment.NewLine);
                throw new FileReadingException(string.Format("While reading cross section definitions for structures an error occured :{0} {1}", Environment.NewLine, string.Join(Environment.NewLine, innerExceptionMessages)));
            }


            foreach (var bridgeDefinition in structureDefinitions.OfType<IBridge>())
            {
                try
                {
                    var crossSectionDefinition = crossSectionDefinitions.FirstOrDefault(csd => csd.Name == bridgeDefinition.Name);
                    if (crossSectionDefinition == null) continue;
                    //bridgeDefinition.TabulatedCrossSectionDefinition = crossSectionDefinition;


                }
                catch (FileReadingException fileReadingException)
                {
                    fileReadingExceptions.Add(new FileReadingException("Could not read cross section location info data", fileReadingException));
                }
            }

            if (fileReadingExceptions.Count > 0)
            {
                var innerExceptionMessages = fileReadingExceptions.Select(fileReadingException => fileReadingException.InnerException.Message + Environment.NewLine);
                throw new FileReadingException(string.Format("While reading structures an error occured :{0} {1}", Environment.NewLine, string.Join(Environment.NewLine, innerExceptionMessages)));
            }
        }
        
        private static IStructure1D ReadStructureDefinition(IDelftIniCategory definitionCategory)
        {
            var type = definitionCategory.ReadProperty<string>(StructureRegion.DefinitionType.Key);
            StructureType structureType;
            if (!Enum.TryParse(type, true, out structureType))
            {
                var errorMessage = string.Format("Couldn't parse this type '{0}' to an element of the structure type enum", type);
                throw new FileReadingException(errorMessage);
            }
            var definitionReader = DefinitionGeneratorFactory.GetDefinitionReaderStructure(structureType);
            if (definitionReader == null)
            {
                var errorMessage = string.Format("No definition reader available for this structure definition: {0}",type);
                throw new FileReadingException(errorMessage);
            }

            var readCrossSectionDefinition = definitionReader.ReadStructureDefinition(definitionCategory);
            return readCrossSectionDefinition;
        }
    }
}