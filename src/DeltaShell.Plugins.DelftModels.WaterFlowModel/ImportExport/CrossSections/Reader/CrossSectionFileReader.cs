using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.CrossSections;
using DelftTools.Utils.Collections;
using DeltaShell.NGHS.IO.FileReaders;
using GeoAPI.Extensions.Networks;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport.CrossSections.Reader
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

        public IList<ICrossSectionDefinition> Read(string definitionFilePath, string locationFilePath, IHydroNetwork network)
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
            return crossSectionDefinitions;
        }

        private static void CreateCrossSectionAndAddToBranch(string definitionFilePath, IHydroNetwork network,
            IEnumerable<ICrossSectionDefinition> crossSectionDefinitions, ICrossSectionLocation crossSectionLocation)
        {
            var correspondingDefinition = crossSectionDefinitions?
                .FirstOrDefault(d => d.Name == crossSectionLocation.Definition);

            if (correspondingDefinition == null)
                throw new FileReadingException(
                    $"The cross section location '{crossSectionLocation.Name}' has no definition in the definition file: {definitionFilePath}");

            var correspondingDefinitionProxy = correspondingDefinition as CrossSectionDefinitionProxy;
            AddDefinitionToNetworkSharedDefinitionsIfNotExisting(network, correspondingDefinitionProxy);

            var crossSection = CreateCrossSection(correspondingDefinition, crossSectionLocation);
            AddToBranch(network, crossSectionLocation.BranchName, crossSection);
        }

        private static void AddDefinitionToNetworkSharedDefinitionsIfNotExisting(IHydroNetwork network, CrossSectionDefinitionProxy correspondingDefinitionProxy)
        {
            if (correspondingDefinitionProxy == null || network.SharedCrossSectionDefinitions.Contains(correspondingDefinitionProxy.InnerDefinition)) return;

            network.SharedCrossSectionDefinitions.Add(correspondingDefinitionProxy.InnerDefinition);
        }

        private static CrossSection CreateCrossSection(ICrossSectionDefinition crossSectionDefinition, ICrossSectionLocation crossSectionLocation)
        {
            var definitionClone = (ICrossSectionDefinition)crossSectionDefinition?.Clone();
            definitionClone?.ShiftLevel(crossSectionLocation.Shift);

            var crossSection = new CrossSection(definitionClone)
            {
                Name = crossSectionLocation.Name,
                LongName = crossSectionLocation.LongName,
                Chainage = crossSectionLocation.Chainage,
            };

            return crossSection;
        }

        private static void AddToBranch(INetwork network, string branchName, CrossSection crossSection)
        {
            var branch = network.Branches.FirstOrDefault(b => b.Name == branchName);

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