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
using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.Helpers;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport.Structures;
using DelftTools.Utils.Collections;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.NGHS.IO.FileReaders.Retention;
using DeltaShell.NGHS.IO.FileReaders.SpatialData;
using DeltaShell.NGHS.IO.Helpers;
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

        private static int stepCounter;
        private static ModelFileNames fileNames;
        private const int TotalSteps = 13;

        public static WaterFlowModel1D Read(string modelFilename, Action<string, int, int> reportProgress = null)
        {
            reportProgress = reportProgress ?? ((s, c, t) => { });
            var errorReport = new List<string>();
            Action<string, IList<string>> CreateAndAddErrorReport = (header, errorMessages) =>
                errorReport.Add($"{header}:{Environment.NewLine}    {string.Join($"{Environment.NewLine}    ", errorMessages)}");

            var name = Path.GetFileNameWithoutExtension(modelFilename);
            var model = name?.Length > 0 ? new WaterFlowModel1D(name) : new WaterFlowModel1D();

            try
            {
                stepCounter = 1;

                reportProgress($"Reading filenames from {Path.GetFileName(modelFilename)}.", stepCounter, TotalSteps);
                fileNames = new ModelFileNames(modelFilename);
                stepCounter++;

                reportProgress($"Reading network from {fileNames.Network}.", stepCounter, TotalSteps);
                ReadNetworkDefinitionFile(model, CreateAndAddErrorReport);
                if (errorReport.Any())
                {
                    throw new Exception(); // If anything goes wrong with reading the network, stop reading.
                }
                stepCounter++;

                reportProgress($"'Reading model wide parameters from {Path.GetFileName(modelFilename)}", stepCounter, TotalSteps);
                ModelDefinitionFileReader.SetWaterFlowModelProperties(modelFilename, model, CreateAndAddErrorReport);

                if (fileNames.Salinity == null)
                {
                    reportProgress("Skipping reading of Salinity data.", stepCounter, TotalSteps);
                }
                else
                {
                    reportProgress($"'Reading salinity from {fileNames.Salinity}.", stepCounter, TotalSteps);
                    var estuaryMouthNodeId = new SalinityFileReader(CreateAndAddErrorReport).ReadEstuaryMouthNodeId(fileNames.Salinity);
                    if (estuaryMouthNodeId != null) model.SalinityEstuaryMouthNodeId = estuaryMouthNodeId;
                }

                reportProgress($"Reading lateral discharge locations from {fileNames.LateralDischarge}.", stepCounter,
                    TotalSteps);
                ReadFileLateralDischargeLocations(model.Network.Channels, CreateAndAddErrorReport);
                stepCounter++;

                reportProgress($"Reading boundary condition locations from {fileNames.BoundaryLocations}.",
                    stepCounter, TotalSteps);
                ReadFileBoundaryConditionLocations(model.BoundaryConditions, CreateAndAddErrorReport);
                stepCounter++;

                reportProgress(
                    $"Reading boundary conditions and lateral sources from {fileNames.BoundaryConditions}.", stepCounter,
                    TotalSteps);
                ReadBoundaryConditionFile(model, CreateAndAddErrorReport);
                stepCounter++;

                reportProgress($"Reading observation points from {fileNames.ObservationPoints}.", stepCounter, TotalSteps);
                ReadFileObservationPointLocations(model.Network.Channels, CreateAndAddErrorReport);
                stepCounter++;

                var totalRoughnessFiles = fileNames.RoughnessFiles.Count;
                var i = 1;
                if (totalRoughnessFiles > 0)
                    model.RoughnessSections.Clear();

                foreach (var roughnessFilePath in fileNames.RoughnessFiles)
                {
                    var importUpdateText = $"Reading roughness section from {roughnessFilePath}. (reading roughness file {i} of {totalRoughnessFiles})";
                    reportProgress(importUpdateText, stepCounter, TotalSteps);
                    i++;
                    var roughnessSection = ReadRoughnessFile(roughnessFilePath, model, CreateAndAddErrorReport);
                    AddRoughnessSectionToModel(model, roughnessSection);
                }
                stepCounter++;

                ReadStructuresAndCrossSections(model.Network, reportProgress, CreateAndAddErrorReport);

                reportProgress($"Reading retention from {fileNames.Retention}.",
                    stepCounter, TotalSteps);
                ReadRetentionFile(model.Network, CreateAndAddErrorReport);
                stepCounter++;

                reportProgress(
                    $"Reading spatial data from {fileNames}.",
                    stepCounter, TotalSteps);

                var spatialDataFileNames = PopulateSpatialDataFileNamesList(model.DispersionFormulationType);

                foreach (var fileName in spatialDataFileNames)
                {
                    ReadSpatialData(fileName, model, CreateAndAddErrorReport);
                }

            }
            catch (Exception e) when (e is FormatException || e is PropertyNotFoundInFileException)
            {
                throw;
            }
            catch (Exception)
            {
                LogErrorReport(errorReport, report => Log.Error(report));
                return null;
            }

            LogErrorReport(errorReport, report => Log.Warn(report));
            return model;
        }

        /// <summary>
        /// Reads structures and cross sections.
        /// </summary>
        /// <param name="network">The model network.</param>
        /// <param name="reportProgress">The report progress that is shown to the user.</param>
        /// <param name="CreateAndAddErrorReport">The create and add error report action.</param>
        /// <remarks>We want to read cross sections and cross section definitions before we read structures, because some structures
        /// (like Culvert and Bridge objects) need cross section definitions. So, do not interchange the order of reading please.</remarks>
        private static void ReadStructuresAndCrossSections(IHydroNetwork network,
            Action<string, int, int> reportProgress, Action<string, IList<string>> CreateAndAddErrorReport)
        {
            reportProgress($"Reading cross sections from {fileNames.CrossSectionLocations} and {fileNames.CrossSectionDefinitions}.",
                stepCounter, TotalSteps);
            var crossSectionDefinitions = ReadCrossSectionsFile(network, CreateAndAddErrorReport);
            stepCounter++;

            var groundLayerData = CrossSectionDefinitionFileReader.ReadGroundLayerData(fileNames.CrossSectionDefinitions).ToArray();

            reportProgress($"Reading structures from {fileNames.Structures}.", stepCounter, TotalSteps);
            ReadStructuresFile(network, crossSectionDefinitions, groundLayerData, CreateAndAddErrorReport);
            stepCounter++;
        }

        private static IList<ICrossSectionDefinition> ReadCrossSectionsFile(IHydroNetwork network, Action<string, IList<string>> createAndAddErrorReport)
        {
            var crossSectionsReader = new CrossSectionFileReader(createAndAddErrorReport);
            return crossSectionsReader.Read(fileNames.CrossSectionDefinitions, fileNames.CrossSectionLocations, network);
        }

        private static void ReadStructuresFile(IHydroNetwork network, IList<ICrossSectionDefinition> crossSectionDefinitions, GroundLayerDataTransferObject[] groundLayerDataTransferObject, Action<string, IList<string>> createAndAddErrorReport)
        {
            var structuresFileReader = new StructuresFileReader(createAndAddErrorReport);
            var compositeBranchStructures = structuresFileReader.ReadStructures(fileNames.Structures, network.Channels.ToList(), crossSectionDefinitions, groundLayerDataTransferObject);

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

        private static IEnumerable<string> PopulateSpatialDataFileNamesList(DispersionFormulationType modelDispersionFormulationType)
        {
            var spatialDataFileNames = new List<string>
            {
                fileNames.InitialDischarge,
                fileNames.InitialSalinity,
                fileNames.InitialTemperature,
                fileNames.InitialWaterLevel,
                fileNames.InitialWaterDepth,
                fileNames.Dispersion,
                fileNames.WindShielding
            };

            if (modelDispersionFormulationType != DispersionFormulationType.Constant)
            {
                spatialDataFileNames.Add(fileNames.DispersionF3);
                spatialDataFileNames.Add(fileNames.DispersionF4);
            }

            return spatialDataFileNames.Where(fn => fn != null);
        }

        private static void ReadRetentionFile(IHydroNetwork network, Action<string, IList<string>> createAndAddErrorReport)
        {
            var retentionFileReader= new RetentionFileReader(createAndAddErrorReport);
            var retentionList = retentionFileReader.ReadRetention(fileNames.Retention, network.Channels.ToList());

            foreach (var retention in retentionList)
            {
                var correspondingBranch = network.Channels.FirstOrDefault(c => c.Name == retention.Branch.Name);
                correspondingBranch?.BranchFeatures.Add(retention);
            }

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

        private static void ReadNetworkDefinitionFile(WaterFlowModel1D model, Action<string, IList<string>> createAndAddErrorReport)
        {
            var network = model.Network;
            var networkDefinitionFileReader = new NetworkDefinitionFileReader(createAndAddErrorReport);

            var nodes = networkDefinitionFileReader.ReadHydroNodes(fileNames.Network);
            network.Nodes.AddRange(nodes);

            var branches = networkDefinitionFileReader.ReadBranches(fileNames.Network, network.Nodes);
            network.Branches.AddRange(branches);

            var readNetworkLocations = networkDefinitionFileReader.ReadNetworkLocations(fileNames.Network, network.Branches).ToList();
            model.NetworkDiscretization.Locations.Values.AddRange(readNetworkLocations);
        }

        private static void ReadFileLateralDischargeLocations(IEnumerable<IChannel> channels, Action<string, IList<string>> createAndAddErrorReport)
        {
            var channelsList = channels.ToList();

            var lateralSourceFileReader = new LateralSourceFileReader(createAndAddErrorReport);

            var lateralSources = lateralSourceFileReader.ReadLateralSources(fileNames.LateralDischarge, channelsList);

            foreach (var lateralSource in lateralSources)
            {
                var reference = channelsList.FirstOrDefault(c => c.Name == lateralSource.Branch.Name);
                reference?.BranchFeatures.Add(lateralSource);
            }
        }

        private static void ReadFileBoundaryConditionLocations(IEventedList<WaterFlowModel1DBoundaryNodeData> boundaryNodes, Action<string, IList<string>> createAndAddErrorReport)
        {
            var boundaryLocations = new BoundaryLocationReader(createAndAddErrorReport).Read(fileNames.BoundaryLocations);
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

        private static void ReadBoundaryConditionFile(WaterFlowModel1D model, Action<string, IList<string>> createAndAddErrorReport)
        {
            new BoundaryConditionFileReader(createAndAddErrorReport).Read(fileNames.BoundaryConditions, 
                                                                          model.MeteoData, 
                                                                          model.Wind, 
                                                                          model.BoundaryConditions, 
                                                                          model.LateralSourceData);
        }

        private static void ReadFileObservationPointLocations(IEnumerable<IChannel> channels, Action<string, IList<string>> createAndAddErrorReport)
        {
            var channelsList = channels.ToList();

            var observationPointFileReader = new ObservationPointFileReader(createAndAddErrorReport);

            var observationPoints = observationPointFileReader.ReadObservationPoints(fileNames.ObservationPoints, channelsList);

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
