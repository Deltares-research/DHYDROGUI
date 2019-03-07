using DelftTools.Hydro;
using DeltaShell.NGHS.IO.FileReaders.Location;
using DeltaShell.NGHS.IO.FileReaders.Network;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport.Roughness;
using log4net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.Helpers;
using DelftTools.Shell.Core.Workflow;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport.Structures;
using DelftTools.Utils.Collections;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.NGHS.IO.FileReaders;
using DeltaShell.NGHS.IO.FileReaders.Retention;
using DeltaShell.NGHS.IO.FileReaders.SpatialData;
using DeltaShell.NGHS.IO.Helpers;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.DataObjects;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport.Boundary;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport.CrossSections.Reader;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport.ModelDefinition;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport.SpatialData;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.Roughness;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport
{
    // TODO: this needs to be called from an integration test

    public static class WaterFlowModelFileReader
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(WaterFlowModelFileReader));

        private static int stepCounter;
        private static ModelFileNames fileNames;
        private const int TotalSteps = 14;

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
                fileNames = new ModelFileNames(modelFilename, CreateAndAddErrorReport);
                stepCounter++;

                reportProgress($"Reading network from {fileNames.Network}.", stepCounter, TotalSteps);
                ReadNetworkDefinitionFile(model, CreateAndAddErrorReport);
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

                var spatialDataFileNames = GetSpatialDataFileNames(model);
                SpatialDataReader.ReadSpatialData(spatialDataFileNames, model, CreateAndAddErrorReport);

                if (model.UseRestart)
                {
                    reportProgress(
                        $"Reading restart data from {fileNames}.",
                        stepCounter, TotalSteps);
                    ReadRestartFiles(fileNames.TargetPath, model);
                }
            }
            catch (Exception e) when (e is FormatException ||
                                      e is PropertyNotFoundInFileException ||
                                      e is FileNotFoundException ||
                                      e is FileReadingException)
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
        /// Creates a temporary <see cref="ModelFileBasedStateHandler"/>.
        /// </summary>
        /// <param name="directoryPath">The directory that contains the unzipped state files.</param>
        /// <param name="model">The name of the <see cref="WaterFlowModel1D"/> at stake.</param>
        /// <returns>A temporary instance of <see cref="ModelFileBasedStateHandler"/></returns>
        /// <remarks>The returned <see cref="ModelFileBasedStateHandler"/> differs from <see cref="WaterFlowModel1D.ModelStateHandler"/>
        /// when it comes to the out and in file names.</remarks>
        private static void ReadRestartFiles(string directoryPath, WaterFlowModel1D model)
        {
            var outAndInFileNames = new List<DelftTools.Utils.Tuple<string, string>>
            {
                new DelftTools.Utils.Tuple<string, string>("sobek.rda", "sobek.rda"),
                new DelftTools.Utils.Tuple<string, string>("sobek.rdf", "sobek.rdf"),
                new DelftTools.Utils.Tuple<string, string>("1Dlevels-in.xyz", "1Dlevels-in.xyz")
            };

            var tempModelStateHandler = new ModelFileBasedStateHandler(model.Name, outAndInFileNames)
            {
                ModelWorkingDirectory = directoryPath
            };

            var persistentStateFilePath = Path.Combine(Path.GetTempPath(), "importedState.zip");
            tempModelStateHandler.SaveStateToFile(tempModelStateHandler.GetState(), persistentStateFilePath);

            model.RestartInput = new FileBasedRestartState("Imported State", persistentStateFilePath);
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

        private static void ReadStructuresFile(IHydroNetwork network, IList<ICrossSectionDefinition> crossSectionDefinitions, GroundLayerDTO[] groundLayerDataTransferObject, Action<string, IList<string>> createAndAddErrorReport)
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
            var reader = new NetworkDefinitionFileReader(createAndAddErrorReport);
            var networkLocations = reader.ReadNetworkDefinitionFile(fileNames.Network, model.Network);
            model.NetworkDiscretization.Locations.Values.AddRange(networkLocations);
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

        private static IEnumerable<string> GetSpatialDataFileNames(WaterFlowModel1D model)
        {

            yield return fileNames.InitialDischarge;
            yield return fileNames.WindShielding;

            if (model.DispersionFormulationType != DispersionFormulationType.Constant)
            {
                yield return fileNames.DispersionF3;
                yield return fileNames.DispersionF4;
            }

            if (model.UseSalt)
            {
                yield return fileNames.InitialSalinity;
                yield return fileNames.Dispersion;
            }

            if (model.UseTemperature)
            {
                yield return fileNames.InitialTemperature;
            }

            yield return model.InitialConditionsType == InitialConditionsType.WaterLevel
                ? fileNames.InitialWaterLevel
                : fileNames.InitialWaterDepth;
        }

        private static void LogErrorReport(List<string> errorReport, Action<string> logAction)
        {
            errorReport.ForEach(logAction);
        }
    }
}
