using DelftTools.Hydro;
using DeltaShell.NGHS.IO.FileReaders.Location;
using DeltaShell.NGHS.IO.FileReaders.Network;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport.CrossSections;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport.Roughness;
using log4net;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Hydro.Helpers;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport.Structures;
using DelftTools.Utils.Collections;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.Roughness;

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
                const int totalSteps = 8;
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
                ReadFileLateralDischargeLocations(fileName.LateralDischarge, model.Network.Channels, CreateAndAddErrorReport);

                reportProgress(
                    $"Reading boundary conditions and lateral sources from {fileName.BoundaryConditions} file.", 4,
                    totalSteps);
                BoundaryFileReader.ReadFile(fileName.BoundaryConditions, model);

                reportProgress($"Reading observation points from {fileName.ObservationPoints} file.", 5, totalSteps);
                ReadFileObservationPointLocations(fileName.ObservationPoints, model.Network.Channels, CreateAndAddErrorReport);

                var totalRoughnessFiles = fileName.RoughnessFiles.Count;
                var i = 1;
                if (totalRoughnessFiles > 0)
                    model.RoughnessSections.Clear();

                foreach (var roughnessFilePath in fileName.RoughnessFiles)
                {
                    var importUpdateText = $"Reading roughness section from {roughnessFilePath} file. (reading roughness file {i} of {totalRoughnessFiles})";
                    reportProgress(importUpdateText, 6, totalSteps);
                    i++;
                    var roughnessSection = ReadRoughnessFile(roughnessFilePath, model, CreateAndAddErrorReport);
                    AddRoughnessSectionToModel(model, roughnessSection);
                }

                reportProgress(
                    $"Reading cross sections from {fileName.CrossSectionLocations} file and {fileName.CrossSectionDefinitions}.",
                    7, totalSteps);

                ReadCrossSectionsFile(fileName, model.Network, CreateAndAddErrorReport);

                reportProgress(
                    $"Reading structures from {fileName.Structures} file and {fileName.Structures}.",
                    8, totalSteps);

                ReadStructuresFile(fileName.Structures, model.Network, CreateAndAddErrorReport);
            }
            catch (Exception)
            {
                LogErrorReport(errorReport, report => Log.Error(report));
                return null;
            }

            LogErrorReport(errorReport, report => Log.Warn(report));
            return model;
        }

        private static void ReadStructuresFile(string fileName, IHydroNetwork network,
            Action<string, IList<string>> createAndAddErrorReport)
        {
            var structuresFileReader = new StructuresFileReader(createAndAddErrorReport);
            var compositeBranchStructures = structuresFileReader.ReadStructures(fileName, network.Channels.ToList());

            foreach (var compositeBranchStructure in compositeBranchStructures)
            {
                if (network.BranchFeatures.Any(bf => bf.Name == compositeBranchStructure.Name))
                {
                    //Extra check, since the composite structures will be added to the network at this level. 
                    compositeBranchStructure.Name =
                        HydroNetworkHelper.GetUniqueFeatureName(compositeBranchStructure.Network as HydroNetwork,
                            compositeBranchStructure);
                }

                var correspondingBranch = network.Channels.FirstOrDefault(c => c.Name == compositeBranchStructure.Branch.Name);
                correspondingBranch?.BranchFeatures.Add(compositeBranchStructure);
            }

        }
        private static void ReadCrossSectionsFile(ModelFileNames fileName, IHydroNetwork network, Action<string, IList<string>> createAndAddErrorReport)
        {
            var crossSectionsReader = new CrossSectionFileReader(createAndAddErrorReport);
            crossSectionsReader.Read(fileName.CrossSectionDefinitions, fileName.CrossSectionLocations, network);
        }

        private static RoughnessSection ReadRoughnessFile(string roughnessFilePath, WaterFlowModel1D model, Action<string, IList<string>> CreateAndAddErrorReport)
        {
            var roughnessReader = new RegularRoughnessFileReader(CreateAndAddErrorReport);
            var roughnessSection = roughnessReader.ReadFile(roughnessFilePath, model.Network, model.RoughnessSections);
            return roughnessSection;
        }

        private static void AddRoughnessSectionToModel(WaterFlowModel1D model, RoughnessSection roughnessSection)
        {
            model.RoughnessSections.RemoveAllWhere(rs => rs.Name == roughnessSection.Name);
            model.RoughnessSections.Add(roughnessSection);
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

        private static void ReadFileLateralDischargeLocations(string locationFilePath, IEnumerable<IChannel> channels,
            Action<string, IList<string>> createAndAddErrorReport)
        {
            var channelsList = channels.ToList();

            var lateralSourceFileReader = new LateralSourceFileReader(createAndAddErrorReport);

            var lateralSources = lateralSourceFileReader.ReadLateralSources(locationFilePath, channelsList);

            foreach (var lateralSource in lateralSources)
            {
                var reference = channelsList.FirstOrDefault(c => c.Name == lateralSource.Branch.Name);
                if (reference != null)
                {
                    reference.BranchFeatures.Add(lateralSource);
                }
            }
        }

        private static void ReadFileObservationPointLocations(string locationFilePath, IEnumerable<IChannel> channels,
            Action<string, IList<string>> createAndAddErrorReport)
        {
            var channelsList = channels.ToList();

            var observationPointFileReader = new ObservationPointFileReader(createAndAddErrorReport);

            var observationPoints = observationPointFileReader.ReadObservationPoints(locationFilePath, channelsList);

            foreach (var observationPoint in observationPoints)
            {
                var reference = channelsList.FirstOrDefault(c => c.Name == observationPoint.Branch.Name);
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
