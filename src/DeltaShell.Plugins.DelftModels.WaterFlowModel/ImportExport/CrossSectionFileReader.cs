using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.CrossSections;
using DeltaShell.NGHS.IO;
using DeltaShell.NGHS.IO.FileReaders;
using DeltaShell.NGHS.IO.FileWriters;
using DeltaShell.NGHS.IO.FileWriters.CrossSectionDefinition;
using DeltaShell.NGHS.IO.FileWriters.Location;
using DeltaShell.NGHS.IO.Helpers;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport
{
    public static class CrossSectionFileReader
    {
        public static void ReadFile(string cslFilename, string csdFilename, WaterFlowModel1D waterFlowModel1D)
        {
            if (!File.Exists(cslFilename)) throw new FileReadingException(string.Format("Could not read file {0} properly, it doesn't exist.", cslFilename));
            var cslCategories = new DelftIniReader().ReadDelftIniFile(cslFilename);
            if (cslCategories.Count == 0) throw new FileReadingException(string.Format("Could not read file {0} properly, it seems empty", cslFilename));
            var csIniLocations = cslCategories.Where(category => category.Name == CrossSectionRegion.IniHeader).ToList();
            if (!csIniLocations.Any()) throw new FileReadingException("Could not read any cross section locations it seems not available");

            if (!File.Exists(csdFilename)) throw new FileReadingException(string.Format("Could not read file {0} properly, it doesn't exist.", csdFilename));
            var csdCategories = new DelftIniReader().ReadDelftIniFile(csdFilename);
            if (csdCategories.Count == 0) throw new FileReadingException(string.Format("Could not read file {0} properly, it seems empty", csdFilename));

            IList<FileReadingException> fileReadingExceptions = new List<FileReadingException>();

            IList<ICrossSectionDefinition> crossSectionDefinitions = new List<ICrossSectionDefinition>();
            foreach (var csdDefinitionCategory in csdCategories.Where(category => category.Name == DefinitionRegion.Header))
            {
                try
                {
                    var crossSectionDefinition = ReadCSDDefinition(csdDefinitionCategory, waterFlowModel1D);
                    if (crossSectionDefinitions.Contains(crossSectionDefinition) || crossSectionDefinitions.FirstOrDefault(csd => csd.Name == crossSectionDefinition.Name) != null)
                        throw new FileReadingException(string.Format("cross section definition with id {0} is already read, id's CAN NOT be duplicates!", crossSectionDefinition.Name));

                    crossSectionDefinitions.Add(crossSectionDefinition);
                }
                catch (FileReadingException fileReadingException)
                {
                    fileReadingExceptions.Add(new FileReadingException("Could not read cross section definition", fileReadingException));
                }
            }

            if (fileReadingExceptions.Count > 0)
            {
                var innerExceptionMessages = fileReadingExceptions.Select(fileReadingException => fileReadingException.InnerException.Message + Environment.NewLine);
                throw new FileReadingException(string.Format("While reading cross section definitions an error occured :{0} {1}", Environment.NewLine, string.Join(Environment.NewLine, innerExceptionMessages)));
            }
            
            foreach (var crossSectionDefinition in crossSectionDefinitions)
            {
                try
                {
                    var crossSectionLocationInfo = csIniLocations.FirstOrDefault(csIniLocation =>
                    {
                        var crossSectionLocationDefinitionId =
                            csIniLocation.ReadProperty<string>(CrossSectionRegion.Definition.Key);
                        return crossSectionLocationDefinitionId == crossSectionDefinition.Name;
                    });

                    if (crossSectionLocationInfo == null)
                        continue;
                        //throw new CrossSectionReadingException(string.Format("The read cross section definition '{0}' has no location in the provided location file: {1}",crossSectionDefinition.Name, cslFilename));

                    var shiftLevel = crossSectionLocationInfo.ReadProperty<double>(CrossSectionRegion.Shift.Key);
                    crossSectionDefinition.ShiftLevel(shiftLevel);

                    var branchId = crossSectionLocationInfo.ReadProperty<string>(LocationRegion.BranchId.Key);
                    var chainage = crossSectionLocationInfo.ReadProperty<double>(LocationRegion.Chainage.Key);
                    var crossSectionName = crossSectionLocationInfo.ReadProperty<string>(LocationRegion.Id.Key);

                    /*optional err message needs to be handled*/
                    var crossSectionLongName = crossSectionLocationInfo.ReadProperty<string>(LocationRegion.Name.Key, true);

                    var branch = waterFlowModel1D.Network.Branches.FirstOrDefault(b => b.Name == branchId);
                    if (branch == null)
                        throw new FileReadingException(string.Format("The read cross section '{0}' has a branch id ({1}) which is not available in the model.", crossSectionName, branchId));
                    
                    var crossSection = new CrossSection(crossSectionDefinition)
                    {
                        Name = crossSectionName,
                        LongName = crossSectionLongName,
                        Chainage = chainage
                    };
                    branch.BranchFeatures.Add(crossSection);
                }
                catch (FileReadingException fileReadingException)
                {
                    fileReadingExceptions.Add(new FileReadingException("Could not read cross section location info data", fileReadingException));
                }
            }

            if (fileReadingExceptions.Count > 0)
            {
                var innerExceptionMessages = fileReadingExceptions.Select(fileReadingException => fileReadingException.InnerException.Message + Environment.NewLine);
                throw new FileReadingException(string.Format("While reading cross sections an error occured :{0} {1}", Environment.NewLine, string.Join(Environment.NewLine, innerExceptionMessages)));
            }
        }

        public static ICrossSectionDefinition ReadCSDDefinition(IDelftIniCategory csdDefinitionCategory, WaterFlowModel1D crossSectionSectionTypes)
        {
            var typeProperty = csdDefinitionCategory.Properties.First(p => p.Name == DefinitionRegion.DefinitionType.Key);
            var definitionReader = DefinitionGeneratorFactory.GetDefinitionReaderCrossSection(typeProperty.Value);
            if (definitionReader == null)
            {
                var errorMessage = "No definition reader available for this cross section definition";
                throw new FileReadingException(errorMessage);
            }

            var readCrossSectionDefinition = definitionReader.ReadCrossSectionDefinition(csdDefinitionCategory);
            SetFrictionOnCrossSectionDefinition(csdDefinitionCategory, readCrossSectionDefinition, crossSectionSectionTypes);
            //groundlayer??
            return readCrossSectionDefinition;
        }

        private static void SetFrictionOnCrossSectionDefinition(IDelftIniCategory csdDefinitionCategory, ICrossSectionDefinition readCrossSectionDefinition, WaterFlowModel1D model)
        {
            if (readCrossSectionDefinition.CrossSectionType == CrossSectionType.YZ || readCrossSectionDefinition.CrossSectionType == CrossSectionType.GeometryBased)
            {
                var roughnessNames = csdDefinitionCategory.ReadPropertiesToListOfType<string>(DefinitionRegion.RoughnessNames.Key);
                if (roughnessNames.Count < 0 )
                    throw new FileReadingException("reading error");

                var roughnessPositions = csdDefinitionCategory.ReadPropertiesToListOfType<double>(DefinitionRegion.RoughnessPositions.Key);
                if (roughnessPositions.Count < 0)
                    throw new FileReadingException("reading error");

                if (roughnessPositions.Count  != roughnessNames.Count+1)
                    throw new FileReadingException("reading error");


                readCrossSectionDefinition.Sections.Clear();
                
                for (int index = 0; index < roughnessNames.Count; index++)
                {
                    var networkSectionType = GetCrossSectionSectionType(roughnessNames[index], model.Network);

                    readCrossSectionDefinition.Sections.Add(new CrossSectionSection
                    {
                        SectionType = networkSectionType,
                        MinY = roughnessPositions[index],
                        MaxY = roughnessPositions[index+1]
                    });
                }
            }

            if (readCrossSectionDefinition.CrossSectionType == CrossSectionType.ZW)
            {
                var mainCrossSectionSectionType = GetCrossSectionSectionType(CrossSectionDefinitionZW.MainSectionName, model.Network);
                var floodPlain1CrossSectionSectionType = GetCrossSectionSectionType(CrossSectionDefinitionZW.Floodplain1SectionTypeName, model.Network);
                var floodPlain2mainCrossSectionSectionType = GetCrossSectionSectionType(CrossSectionDefinitionZW.Floodplain2SectionTypeName, model.Network);

                var main = csdDefinitionCategory.ReadProperty<double>(DefinitionRegion.Main.Key);
                var floodPlain1 = csdDefinitionCategory.ReadProperty<double>(DefinitionRegion.FloodPlain1.Key,true);
                var flowWidths = csdDefinitionCategory.ReadPropertiesToListOfType<double>(DefinitionRegion.FlowWidths.Key);
            
                var floodPlain2 = flowWidths.Max() - main - floodPlain1; //FloodPlain2 is defined as max(FlowWidth) - Main - Floodplain1

                double offset = 0.0d;

                readCrossSectionDefinition.Sections.Clear();

                readCrossSectionDefinition.Sections.Add(
                    new CrossSectionSection()
                    {
                        SectionType = mainCrossSectionSectionType,
                        MinY = offset,
                        MaxY = offset+main / 2
                    }
                    );
                
                offset += main/2;

                readCrossSectionDefinition.Sections.Add(
                    new CrossSectionSection()
                    {
                        SectionType = floodPlain1CrossSectionSectionType,
                        MinY = offset,
                        MaxY = offset+floodPlain1 / 2
                    }
                    );
                offset += floodPlain1 / 2;
                readCrossSectionDefinition.Sections.Add(
                    new CrossSectionSection()
                    {
                        SectionType = floodPlain2mainCrossSectionSectionType,
                        MinY = offset,
                        MaxY = offset+floodPlain2 / 2
                    }
                    );
            }
            
            if (readCrossSectionDefinition.CrossSectionType == CrossSectionType.Standard)
            {
                var roughnessNames = csdDefinitionCategory.ReadPropertiesToListOfType<string>(DefinitionRegion.RoughnessNames.Key, true);
                if (roughnessNames == null ) return;
                
                if (roughnessNames.Count != 1)
                    throw new FileReadingException("reading error");

                var sectionTypeName = roughnessNames.FirstOrDefault();
                if (sectionTypeName == null)
                    throw new FileReadingException("reading error");

                readCrossSectionDefinition.Sections.Add(
                    new CrossSectionSection()
                    {
                        SectionType = GetCrossSectionSectionType(sectionTypeName, model.Network)
                    }
                    );
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