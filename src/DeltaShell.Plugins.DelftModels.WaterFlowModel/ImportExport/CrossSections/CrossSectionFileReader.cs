using DelftTools.Hydro;
using DelftTools.Hydro.CrossSections;
using DelftTools.Utils.Collections;
using DeltaShell.NGHS.IO.FileReaders;
using DeltaShell.Plugins.Delftnetworks.WaterFlownetwork.ImportExport.CrossSections;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport.CrossSections
{
    public class CrossSectionFileReader
    {
        private readonly Action<string, IList<string>> createAndAddErrorReport;

        private readonly CrossSectionDefinitionFileReader crossSectionDefinitionFileReader;

        private readonly CrossSectionLocationFileReader crossSectionLocationFileReader;

        public CrossSectionFileReader(Action<string, IList<string>> createAndAddErrorReport)
        {
            this.createAndAddErrorReport = createAndAddErrorReport;

            crossSectionDefinitionFileReader = new CrossSectionDefinitionFileReader(createAndAddErrorReport);

            crossSectionLocationFileReader = new CrossSectionLocationFileReader(createAndAddErrorReport);
        }

        public void Read(string definitionFilePath, string locationFilePath, IHydroNetwork network)
        {
            var errorMessages = new List<string>();

            var crossSectionDefinitions = crossSectionDefinitionFileReader.Read(definitionFilePath, network);

            var crossSectionLocations = crossSectionLocationFileReader.Read(locationFilePath);

            crossSectionLocations?.ForEach(csl =>
            {
                try
                {
                    CreateCrossSectionAndAddToBranch(definitionFilePath, network, crossSectionDefinitions, csl);
                }
                catch (Exception e)
                {
                    errorMessages.Add(e.Message);
                }
            });

            CreateErrorReport("cross sections", errorMessages);
        }

        private static void CreateCrossSectionAndAddToBranch(string definitionFilePath, IHydroNetwork network,
            IList<ICrossSectionDefinition> crossSectionDefinitions, ICrossSectionLocation csl)
        {
            var correspondingDefinition = crossSectionDefinitions?
                .FirstOrDefault(d => d.Name == csl.Definition);

            if (correspondingDefinition == null)
                throw new FileReadingException(
                    $"The cross section location '{csl.Name}' has no definition in the definition file: {definitionFilePath}");

            var correspondingDefinitionProxy = correspondingDefinition as CrossSectionDefinitionProxy;

            if (correspondingDefinitionProxy != null &&
                !network.SharedCrossSectionDefinitions.Contains(correspondingDefinitionProxy.InnerDefinition))
            {
                network.SharedCrossSectionDefinitions.Add(correspondingDefinitionProxy.InnerDefinition);
            }

            var crossSection = CreateCrossSection(correspondingDefinition, csl);

            AddToBranch(network, csl.BranchName, crossSection);
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

        private void CreateErrorReport(string objectName, List<string> errorMessages)
        {
            if (errorMessages.Count > 0)
                createAndAddErrorReport?.Invoke($"While importing the {objectName} to the network, the following errors occured", errorMessages);
        }
    }
}