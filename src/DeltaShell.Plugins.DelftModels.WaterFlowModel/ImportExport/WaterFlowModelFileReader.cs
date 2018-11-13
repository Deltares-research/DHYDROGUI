using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Hydro;
using DeltaShell.NGHS.IO.FileReaders.Location;
using DeltaShell.NGHS.IO.FileReaders.Network;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport.CrossSections;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport.Roughness;
using log4net;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport
{
    // TODO: this needs to be called from an integration test

    public static class WaterFlowModel1DFileReader
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(WaterFlowModel1DFileReader));

        public static WaterFlowModel1D Read(string modelFilename, Action<string, int, int> reportProgress = null)
        {
            reportProgress = reportProgress ?? ((s, c, t) => { });
            var errorReport = new List<string>();
            Action<string, IList<string>> CreateAndAddErrorReport = (header, errorMessages) =>
                errorReport.Add($"{header}:{Environment.NewLine} {string.Join(Environment.NewLine, errorMessages)}");

            var model = new WaterFlowModel1D();
            try
            {
                const int totalSteps = 7;
                reportProgress($"Reading filenames from {Path.GetFileName(modelFilename)} file.", 1, totalSteps);
                var fileName = new ModelFileNames(modelFilename);

                var networkDefinitionFilePath = fileName.Network;
                reportProgress($"Reading network from {networkDefinitionFilePath} file.", 2, totalSteps);
                ReadNetworkDefinitionFile(networkDefinitionFilePath, model, CreateAndAddErrorReport);
                if (errorReport.Any())
                {
                    throw new Exception(); // If anything goes wring with reading the network, stop reading.
                }

                reportProgress($"Reading lateral discharge locations from {fileName.LateralDischarge} file.", 3,
                    totalSteps);
                ReadFileLateralDischargeLocations(fileName.LateralDischarge, model.Network, CreateAndAddErrorReport);

                reportProgress(
                    $"Reading boundary conditions and lateral sources from {fileName.BoundaryConditions} file.", 4,
                    totalSteps);
                BoundaryFileReader.ReadFile(fileName.BoundaryConditions, model);

                reportProgress($"Reading observation points from {fileName.ObservationPoints} file.", 5, totalSteps);
                ReadFileObservationPointLocations(fileName.ObservationPoints, model.Network, CreateAndAddErrorReport);

                var totalRoughnessFiles = fileName.RoughnessFiles.Count;
                var i = 1;
                if (totalRoughnessFiles > 0)
                    model.RoughnessSections.Clear();

                foreach (var roughnessFilePath in fileName.RoughnessFiles)
                {
                    var importUpdateText = $"Reading roughness section from {roughnessFilePath} file. (reading roughness file {i} of {totalRoughnessFiles})";
                    reportProgress(importUpdateText, 6, totalSteps);
                    i++;
                    ReadRoughnessFile(CreateAndAddErrorReport, roughnessFilePath, model);
                }

                reportProgress(
                    $"Reading cross sections from {fileName.CrossSectionLocations} file and {fileName.CrossSectionDefinitions}.",
                    7, totalSteps);
                CrossSectionFileReader.Read(fileName.CrossSectionDefinitions, fileName.CrossSectionLocations, 
                    model.Network);
            }
            catch (Exception)
            {
                LogErrorReport(errorReport, report => Log.Error(report));
                return null;
            }

            LogErrorReport(errorReport, report => Log.Warn(report));
            return model;
        }

        private static void ReadRoughnessFile(Action<string, IList<string>> CreateAndAddErrorReport, string roughnessFilePath, WaterFlowModel1D model)
        {
            var roughnessReader = new RegularRoughnessFileReader(CreateAndAddErrorReport);
            var roughnessSection = roughnessReader.ReadFile(roughnessFilePath, model.Network, model.RoughnessSections);

            model.RoughnessSections.Add(roughnessSection);
            var sectionType = roughnessSection.CrossSectionSectionType;
            if (model.Network.CrossSectionSectionTypes.All(t => t.Name != sectionType.Name))
            {
                model.Network.CrossSectionSectionTypes.Add(sectionType);
            }
        }

        private static void ReadNetworkDefinitionFile(string networkDefinitionFilePath, WaterFlowModel1D model,
            Action<string, IList<string>> createAndAddErrorReport)
        {
            var network = model.Network;
            var networkDefinitionFileReader = new NetworkDefinitionFileReader(createAndAddErrorReport);

            var nodes = networkDefinitionFileReader.ReadHydroNodes(networkDefinitionFilePath);
            network.Nodes.AddRange(nodes);

            var branches = networkDefinitionFileReader.ReadBranches(networkDefinitionFilePath, network.Nodes);
            network.Branches.AddRange(branches);

            var readNetworkLocations = networkDefinitionFileReader.ReadNetworkLocations(networkDefinitionFilePath, network.Branches).ToList();
            model.NetworkDiscretization.Locations.Values.AddRange(readNetworkLocations);
        }

        private static void ReadFileLateralDischargeLocations(string locationFilePath, IHydroNetwork network,
            Action<string, IList<string>> createAndAddErrorReport)
        {
            var lateralSourceFileReader = new LateralSourceFileReader(createAndAddErrorReport);

            var lateralSources = lateralSourceFileReader.ReadLateralSources(locationFilePath, network);

            foreach (var lateralSource in lateralSources)
            {
                var reference = network.Channels.FirstOrDefault(c => c.Name == lateralSource.Branch.Name);
                if (reference != null)
                {
                    reference.BranchFeatures.Add(lateralSource);
                }
            }
        }

        private static void ReadFileObservationPointLocations(string locationFilePath, IHydroNetwork network,
            Action<string, IList<string>> createAndAddErrorReport)
        {
            var observationPointFileReader = new ObservationPointFileReader(createAndAddErrorReport);

            var observationPoints = observationPointFileReader.ReadObservationPoints(locationFilePath, network);

            foreach (var observationPoint in observationPoints)
            {
                var reference = network.Channels.FirstOrDefault(c => c.Name == observationPoint.Branch.Name);
                if (reference != null)
                {
                    reference.BranchFeatures.Add(observationPoint);
                }
            }
        }

        private static void LogErrorReport(List<string> errorReport, Action<string> logAction)
        {
            errorReport.ForEach(logAction);
        }
    }
}
