using DelftTools.Hydro;
using DeltaShell.NGHS.IO.FileReaders.Location;
using DeltaShell.NGHS.IO.FileReaders.Network;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport.CrossSections;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport.Roughness;
using log4net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Functions;
using DelftTools.Hydro.Helpers;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport.Structures;
using DelftTools.Utils.Collections;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.NGHS.IO.FileReaders.SpatialData;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.DataObjects;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport.Boundary;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport.ModelDefinition;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.Roughness;
using GeoAPI.Extensions.Coverages;

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
                errorReport.Add($"{header}:{Environment.NewLine}    {string.Join($"{Environment.NewLine}    ", errorMessages)}");

            var name = Path.GetFileNameWithoutExtension(modelFilename);
            var model = name.Length > 0 ? new WaterFlowModel1D(name) : new WaterFlowModel1D();

            try
            {
                const int totalSteps = 12;
                var stepCounter = 1;

                reportProgress($"Reading filenames from {Path.GetFileName(modelFilename)} file.", stepCounter, totalSteps);
                var fileNames = new ModelFileNames(modelFilename);
                stepCounter++;

                var networkDefinitionFilePath = fileNames.Network;
                reportProgress($"Reading network from {networkDefinitionFilePath} file.", stepCounter, totalSteps);
                ReadNetworkDefinitionFile(networkDefinitionFilePath, model, CreateAndAddErrorReport);
                if (errorReport.Any())
                {
                    throw new Exception(); // If anything goes wrong with reading the network, stop reading.
                }
                stepCounter++;

                reportProgress($"'Reading model wide parameters from {Path.GetFileName(modelFilename)} file", stepCounter, totalSteps);
                ModelDefinitionFileReader.SetWaterFlowModelProperties(modelFilename, model, CreateAndAddErrorReport);

                reportProgress($"'Reading salinity from {fileNames.Salinity} file", stepCounter, totalSteps);
                var estuaryMouthNodeId = new SalinityFileReader(CreateAndAddErrorReport).ReadEstuaryMouthNodeId(fileNames.Salinity);
                model.SalinityEstuaryMouthNodeId = estuaryMouthNodeId;

                reportProgress($"Reading lateral discharge locations from {fileNames.LateralDischarge} file.", stepCounter,
                    totalSteps);
                ReadFileLateralDischargeLocations(fileNames.LateralDischarge, model.Network.Channels, CreateAndAddErrorReport);
                stepCounter++;

                reportProgress($"Reading boundary condition locations from {fileNames.BoundaryLocations} file.",
                    stepCounter, totalSteps);
                ReadFileBoundaryConditionLocations(fileNames.BoundaryLocations, model.BoundaryConditions, CreateAndAddErrorReport);
                stepCounter++;

                reportProgress(
                    $"Reading boundary conditions and lateral sources from {fileNames.BoundaryConditions} file.", stepCounter,
                    totalSteps);
                ReadBoundaryConditionFile(fileNames.BoundaryConditions, model, CreateAndAddErrorReport);
                stepCounter++;

                reportProgress($"Reading observation points from {fileNames.ObservationPoints} file.", stepCounter, totalSteps);
                ReadFileObservationPointLocations(fileNames.ObservationPoints, model.Network.Channels, CreateAndAddErrorReport);
                stepCounter++;

                var totalRoughnessFiles = fileNames.RoughnessFiles.Count;
                var i = 1;
                if (totalRoughnessFiles > 0)
                    model.RoughnessSections.Clear();

                foreach (var roughnessFilePath in fileNames.RoughnessFiles)
                {
                    var importUpdateText = $"Reading roughness section from {roughnessFilePath} file. (reading roughness file {i} of {totalRoughnessFiles})";
                    reportProgress(importUpdateText, stepCounter, totalSteps);
                    i++;
                    var roughnessSection = ReadRoughnessFile(roughnessFilePath, model, CreateAndAddErrorReport);
                    AddRoughnessSectionToModel(model, roughnessSection);
                }
                stepCounter++;

                reportProgress(
                    $"Reading cross sections from {fileNames.CrossSectionLocations} file and {fileNames.CrossSectionDefinitions}.",
                    stepCounter, totalSteps);
                ReadCrossSectionsFile(fileNames, model.Network, CreateAndAddErrorReport);
                stepCounter++;

                reportProgress(
                    $"Reading structures from {fileNames.Structures} file and {fileNames.Structures}.",
                    stepCounter, totalSteps);
                ReadStructuresFile(fileNames.Structures, model.Network, CreateAndAddErrorReport);
                stepCounter++;

                reportProgress(
                    $"Reading spatial data from {fileNames} file.",
                    stepCounter, totalSteps);

                ReadSpatialData(fileNames.InitialDischarge, model, CreateAndAddErrorReport);
                
                ReadSpatialData(fileNames.InitialSalinity, model, CreateAndAddErrorReport);
                
                ReadSpatialData(fileNames.InitialTemperature, model, CreateAndAddErrorReport);

                ReadSpatialData(fileNames.InitialWaterLevel, model, CreateAndAddErrorReport);

                ReadSpatialData(fileNames.InitialWaterDepth, model, CreateAndAddErrorReport);
                
                ReadSpatialData(fileNames.Dispersion, model, CreateAndAddErrorReport);

                if (model.DispersionFormulationType != DispersionFormulationType.Constant)
                {
                    ReadSpatialData(fileNames.DispersionF3, model, CreateAndAddErrorReport);
                    ReadSpatialData(fileNames.DispersionF4, model, CreateAndAddErrorReport);
                }

                ReadSpatialData(fileNames.WindShielding, model, CreateAndAddErrorReport);

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
                if (network.CompositeBranchStructures.Any(bf => bf.Name == compositeBranchStructure.Name))
                {
                    //Extra check, since the composite structures will be added to the network at this level and 
                    // due to this all the new composite branch structures containing only one structure have the same name.  
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
            if (roughnessSection is ReverseRoughnessSection)
            {
                model.UseReverseRoughness = true;
            }
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
                reference?.BranchFeatures.Add(lateralSource);
            }
        }

        private static void ReadFileBoundaryConditionLocations(string locationFilePath, IEventedList<WaterFlowModel1DBoundaryNodeData> boundaryNodes, Action<string, IList<string>> createAndAddErrorReport)
        {
            var boundaryLocations = (new BoundaryLocationReader(createAndAddErrorReport)).Read(locationFilePath);
            if (boundaryLocations == null) return; // File could not be read

            var errorMessages = new List<string>();
            foreach (var boundaryLocation in boundaryLocations)
            {
                var correspondingNode = boundaryNodes.FirstOrDefault(e => e.Feature.Name == boundaryLocation.Name);

                if (correspondingNode == null)
                {
                    errorMessages.Add($"No boundary with nodeId = {boundaryLocation.Name} found in the network.");
                    continue;
                }

                correspondingNode.ThatcherHarlemannCoefficient = boundaryLocation.ThatcherHarlemannCoefficient;
            }

            if (errorMessages.Count > 0)
                createAndAddErrorReport?.Invoke("While adding the boundary locations to the network, the following errors occured", errorMessages);
        }

        private static void ReadBoundaryConditionFile(string boundaryConditionsFilePath, WaterFlowModel1D model, Action<string, IList<string>> createAndAddErrorReport)
        {
            new BoundaryConditionFileReader(createAndAddErrorReport).Read(boundaryConditionsFilePath, 
                                                                          model.MeteoData, 
                                                                          model.Wind, 
                                                                          model.BoundaryConditions, 
                                                                          model.LateralSourceData);
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
        /// <summary>
        /// Reads the file spatial data. It first checks the locationFilePath for null because the file does not exist when it is null.
        /// </summary>
        /// <param name="locationFilePath"></param>
        /// <param name="model"></param>
        /// <param name="createAndAddErrorReport"></param>
        private static void ReadSpatialData(string locationFilePath, WaterFlowModel1D model,
            Action<string, IList<string>> createAndAddErrorReport)
        {
            //Necessary because the default value of a string is null and you do not want to continue 
            if (locationFilePath == null) return;

            var spatialFileDataReader = new SpatialDataReader(createAndAddErrorReport);
            if (!File.Exists(locationFilePath)) return;
            var spatialFileData = spatialFileDataReader.ReadSpatialFileData(locationFilePath, model.Network.Channels.ToList());

            SetModelSpatialDataOnModel(locationFilePath, model, spatialFileData);
        }

        private static void SetModelSpatialDataOnModel(string locationFilePath, WaterFlowModel1D model, INetworkCoverage spatialFileData)
        {
            var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(locationFilePath);

            switch (fileNameWithoutExtension)
            {
                case "InitialDischarge":
                    CopySpatialFileDataToModel(model.InitialFlow, spatialFileData);
                    break;
                case "InitialSalinity":
                    CopySpatialFileDataToModel(model.InitialSaltConcentration, spatialFileData);
                    break;
                case "InitialTemperature":
                    CopySpatialFileDataToModel(model.InitialTemperature, spatialFileData);
                    break;
                case "InitialWaterLevel":
                    CopySpatialFileDataToModel(model.InitialConditions, spatialFileData);
                    model.InitialConditionsType = InitialConditionsType.WaterLevel;
                    break;
                case "InitialWaterDepth":
                    CopySpatialFileDataToModel(model.InitialConditions, spatialFileData);
                    model.InitialConditionsType = InitialConditionsType.Depth;
                    break;
                case "Dispersion":
                    CopySpatialFileDataToModel(model.DispersionCoverage, spatialFileData);
                    break;
                case "DispersionF3":
                    CopySpatialFileDataToModel(model.DispersionF3Coverage, spatialFileData);
                    break;
                case "DispersionF4":
                    CopySpatialFileDataToModel(model.DispersionF4Coverage, spatialFileData);
                    break;
                case "WindShielding":
                    CopySpatialFileDataToModel(model.WindShielding, spatialFileData);
                    break;
                default:
                    Log.Warn($"Could not find any spatial data to set on the model. The file: {locationFilePath} does not have a correct name.");
                    break;
            }
        }

        private static void CopySpatialFileDataToModel(IFunction copyTo, IFunction copyFrom)
        {
            if(copyTo == null || copyFrom == null) return;
            copyTo.Arguments[0].SetValues(copyFrom.Arguments[0].Values);
            copyTo.Components[0].SetValues(copyFrom.Components[0].Values);
        }

        private static void LogErrorReport(List<string> errorReport, Action<string> logAction)
        {
            errorReport.ForEach(logAction);
        }
    }
}
