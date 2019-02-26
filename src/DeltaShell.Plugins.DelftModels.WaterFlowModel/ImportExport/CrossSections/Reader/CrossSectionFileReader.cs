using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.CrossSections;
using DelftTools.Utils.Collections;
using DeltaShell.NGHS.IO.FileReaders;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport.CrossSections.Reader
{
    public class CrossSectionFileReader
    {
        private readonly Action<string, IList<string>> createAndAddErrorReport;

        private readonly CrossSectionDefinitionFileReader crossSectionDefinitionFileReader;

        private readonly CrossSectionLocationFileReader crossSectionLocationFileReader;

        private IHydroNetwork hydroNetwork;

        public CrossSectionFileReader(Action<string, IList<string>> createAndAddErrorReport)
        {
            this.createAndAddErrorReport = createAndAddErrorReport;

            crossSectionDefinitionFileReader = new CrossSectionDefinitionFileReader(createAndAddErrorReport);
            crossSectionLocationFileReader = new CrossSectionLocationFileReader(createAndAddErrorReport);
        }

        public IList<ICrossSectionDefinition> Read(string definitionFilePath, string locationFilePath, IHydroNetwork network)
        {
            hydroNetwork = network;
            var errorMessages = new List<string>();

            var crossSectionDefinitions = crossSectionDefinitionFileReader.Read(definitionFilePath, network);
            var crossSectionLocations = crossSectionLocationFileReader.Read(locationFilePath);
            crossSectionLocations?.ForEach(csl =>
            {
                try
                {
                    CreateCrossSectionAndAddToBranch(definitionFilePath, crossSectionDefinitions, csl);
                }
                catch (Exception e)
                {
                    errorMessages.Add(e.Message);
                }
            });

            CreateErrorReport("cross sections", errorMessages);
            return crossSectionDefinitions;
        }

        private void CreateCrossSectionAndAddToBranch(string definitionFilePath, IEnumerable<ICrossSectionDefinition> crossSectionDefinitions, ICrossSectionLocation crossSectionLocation)
        {
            var correspondingDefinition = crossSectionDefinitions?
                .FirstOrDefault(d => d.Name == crossSectionLocation.Definition);

            if (correspondingDefinition == null)
                throw new FileReadingException(
                    $"The cross section location '{crossSectionLocation.Name}' has no definition in the definition file: {definitionFilePath}");

            var correspondingDefinitionProxy = correspondingDefinition as CrossSectionDefinitionProxy;
            AddDefinitionToNetworkSharedDefinitionsIfNotExisting(correspondingDefinitionProxy);

            var crossSection = CreateCrossSection(correspondingDefinition, crossSectionLocation);
            AddToBranch(crossSectionLocation.BranchName, crossSection);
        }

        private void AddDefinitionToNetworkSharedDefinitionsIfNotExisting(CrossSectionDefinitionProxy correspondingDefinitionProxy)
        {
            if (correspondingDefinitionProxy == null || hydroNetwork.SharedCrossSectionDefinitions.Contains(correspondingDefinitionProxy.InnerDefinition)) return;

            hydroNetwork.SharedCrossSectionDefinitions.Add(correspondingDefinitionProxy.InnerDefinition);
        }

        private static CrossSection CreateCrossSection(ICrossSectionDefinition crossSectionDefinition, ICrossSectionLocation crossSectionLocation)
        {
            var definitionClone = (ICrossSectionDefinition)crossSectionDefinition.Clone();
            definitionClone.ShiftLevel(crossSectionLocation.Shift);

            var crossSection = new CrossSection(definitionClone)
            {
                Name = crossSectionLocation.Name,
                LongName = crossSectionLocation.LongName,
                Chainage = crossSectionLocation.Chainage
            };

            return crossSection;
        }

        private void AddToBranch(string branchName, CrossSection crossSection)
        {
            var branch = hydroNetwork.Branches.FirstOrDefault(b => b.Name == branchName);

            if (branch == null)
                throw new FileReadingException($"The read cross section '{crossSection.Name}' has a branch ID ({branchName}) which is not available in the model.");

            branch.BranchFeatures.Add(crossSection);
        }

        private void CreateErrorReport(string objectName, IList<string> errorMessages)
        {
            if (errorMessages.Count > 0)
                createAndAddErrorReport?.Invoke($"While importing the {objectName} to the network, the following errors occured", errorMessages);
        }
    }
}