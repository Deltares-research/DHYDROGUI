using DelftTools.Hydro;
using DelftTools.Hydro.CrossSections;
using DeltaShell.NGHS.IO.FileReaders;
using System.Linq;
using DelftTools.Utils.Collections;
using DeltaShell.Plugins.Delftnetworks.WaterFlownetwork.ImportExport.CrossSections;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport.CrossSections
{
    public static class CrossSectionFileReader
    {
        public static void Read(string definitionFilePath, string locationFilePath, IHydroNetwork network)
        {
            var crossSectionDefinitions = CrossSectionDefinitionFileReader.Read(definitionFilePath, network);

            var crossSectionLocations = CrossSectionLocationFileReader.Read(locationFilePath);

            crossSectionLocations?.ForEach(csl =>
            {
                var correspondingDefinition = crossSectionDefinitions?
                    .FirstOrDefault(d => d.Name == csl.Definition);

                if (correspondingDefinition == null)
                    throw new FileReadingException($"The cross section location '{csl.Name}' has no definition in the definition file: {definitionFilePath}");

                var correspondingDefinitionProxy = correspondingDefinition as CrossSectionDefinitionProxy;
                
                if (correspondingDefinitionProxy != null && !network.SharedCrossSectionDefinitions.Contains(correspondingDefinitionProxy.InnerDefinition))
                {
                    network.SharedCrossSectionDefinitions.Add(correspondingDefinitionProxy.InnerDefinition);
                }


                var crossSection = CreateCrossSection(correspondingDefinition, csl);

                AddToBranch(network, csl.BranchName, crossSection);
            });
        }

        private static void AddToBranch(IHydroNetwork network, string branchName, CrossSection crossSection)
        {
            var branch = network.Branches.FirstOrDefault(b => b.Name == branchName);

            if (branch == null)
                throw new FileReadingException($"The read cross section '{crossSection.Name}' has a branch ID ({branchName}) which is not available in the model.");

            branch.BranchFeatures.Add(crossSection);
        }

        private static CrossSection CreateCrossSection(ICrossSectionDefinition crossSectionDefinition, ICrossSectionLocation crossSectionLocation)
        {
            crossSectionDefinition?.ShiftLevel(crossSectionLocation.Shift);

            var crossSection = new CrossSection(crossSectionDefinition)
            {
                Name = crossSectionLocation.Name,
                LongName = crossSectionLocation.LongName,
                Chainage = crossSectionLocation.Chainage,
            };

            return crossSection;
        }

        
    }
}