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
using DeltaShell.NGHS.IO.FileReaders;
using DeltaShell.NGHS.IO.Helpers;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.DataObjects;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport.Boundary;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport.ModelDefinition;
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
                const int totalSteps = 10;
                var stepCounter = 1;

                reportProgress($"'Reading model wide parameters from {Path.GetFileName(modelFilename)} file", stepCounter, totalSteps);
                ReadMd1dFile(modelFilename, model);
                stepCounter++;

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
            }
            catch (Exception)
            {
                LogErrorReport(errorReport, report => Log.Error(report));
                return null;
            }

            LogErrorReport(errorReport, report => Log.Warn(report));
            return model;
        }

        private static void ReadMd1dFile(string filePath, WaterFlowModel1D model)
        {
            var modelPropertySettings = DelftIniFileParser.ReadFile(filePath);
            WaterFlowModelPropertySetter.SetProperties(modelPropertySettings, model);

            model.UseSalt = true;
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

            foreach (var boundaryLocation in boundaryLocations)
            {
                var correspondingNode = boundaryNodes.FirstOrDefault(e => e.Feature.Name == boundaryLocation.Name);
                correspondingNode.ThatcherHarlemannCoefficient = boundaryLocation.ThatcherHarlemannCoefficient;
            }
        }

        private static void ReadBoundaryConditionFile(string boundaryConditionsFilePath, WaterFlowModel1D model, Action<string, IList<string>> createAndAddErrorReport)
        {
            var bcCategories = DelftBcFileParser.ReadFile(boundaryConditionsFilePath);
            AddMeteoData(bcCategories, model);
            AddWindData(bcCategories, model);
            AddBoundaryConditionData(bcCategories, model.BoundaryConditions);
            AddLateralDischargeData(bcCategories, model.LateralSourceData);
        }

        private static void AddMeteoData(IList<IDelftBcCategory> bcCategories, WaterFlowModel1D model)
        {
            var meteoData = MeteoDataConverter.Convert(bcCategories, new List<string>());
            model.MeteoData.Arguments[0].SetValues(meteoData.Arguments[0].Values);
            model.MeteoData.AirTemperature.SetValues(meteoData.AirTemperature.Values);
            model.MeteoData.Cloudiness.SetValues(meteoData.Cloudiness.Values);
            model.MeteoData.RelativeHumidity.SetValues(meteoData.RelativeHumidity.Values);
        }

        private static void AddWindData(IList<IDelftBcCategory> bcCategories, WaterFlowModel1D model)
        {
            var windData = WindDataConverter.Convert(bcCategories, new List<string>());
            model.Wind.Arguments[0].SetValues(windData.Arguments[0].Values);
            model.Wind.Direction.SetValues(windData.Direction.Values);
            model.Wind.Velocity.SetValues(windData.Velocity.Values);
        }

        private static void AddBoundaryConditionData(IList<IDelftBcCategory> bcCategories,
                                                     IEventedList<WaterFlowModel1DBoundaryNodeData> boundaryNodes)
        {
            var boundaryConditionData = BoundaryConditionConverter.Convert(bcCategories, new List<string>());
            foreach (var boundaryNode in boundaryNodes)
            {
                if (!boundaryConditionData.ContainsKey(boundaryNode.Feature.Name)) continue;

                var nodeData = boundaryConditionData[boundaryNode.Feature.Name];
                if (nodeData.WaterComponent != null)
                {
                    boundaryNode.DataType = nodeData.WaterComponent.BoundaryType;

                    switch (boundaryNode.DataType)
                    {
                        case WaterFlowModel1DBoundaryNodeDataType.FlowConstant:
                            boundaryNode.Flow = nodeData.WaterComponent.ConstantBoundaryValue;
                            break;
                        case WaterFlowModel1DBoundaryNodeDataType.WaterLevelConstant:
                            boundaryNode.WaterLevel = nodeData.WaterComponent.ConstantBoundaryValue;
                            break;
                        case WaterFlowModel1DBoundaryNodeDataType.FlowWaterLevelTable:
                        case WaterFlowModel1DBoundaryNodeDataType.FlowTimeSeries:
                        case WaterFlowModel1DBoundaryNodeDataType.WaterLevelTimeSeries:
                            copyFunction(nodeData.WaterComponent.TimeDependentBoundaryValue, boundaryNode.Data);
                            break;
                    }
                }

                if (nodeData.SaltComponent != null)
                {
                    boundaryNode.SaltConditionType = nodeData.SaltComponent.BoundaryType;

                    switch (boundaryNode.SaltConditionType)
                    {
                        case SaltBoundaryConditionType.Constant:
                            boundaryNode.SaltConcentrationConstant = nodeData.SaltComponent.ConstantBoundaryValue;
                            break;
                        case SaltBoundaryConditionType.TimeDependent:
                            copyFunction(nodeData.SaltComponent.TimeDependentBoundaryValue, boundaryNode.SaltConcentrationTimeSeries);
                            break;
                        case SaltBoundaryConditionType.None:
                            break;
                    }
                }

                if (nodeData.TemperatureComponent != null)
                {
                    boundaryNode.TemperatureConditionType = nodeData.TemperatureComponent.BoundaryType;

                    switch (boundaryNode.TemperatureConditionType)
                    {
                        case TemperatureBoundaryConditionType.Constant:
                            boundaryNode.TemperatureConstant = nodeData.TemperatureComponent.ConstantBoundaryValue;
                            break;
                        case TemperatureBoundaryConditionType.TimeDependent:
                            copyFunction(nodeData.TemperatureComponent.TimeDependentBoundaryValue, boundaryNode.TemperatureTimeSeries);
                            break;
                        case TemperatureBoundaryConditionType.None:
                            break;
                    }
                }
            }
        }

        private static void AddLateralDischargeData(IList<IDelftBcCategory> bcCategories,
                                                    IEventedList<WaterFlowModel1DLateralSourceData> lateralNodes)
        {
            var lateralDischargeData = LateralDischargeConverter.Convert(bcCategories, new List<string>());
            foreach (var lateralNode in lateralNodes)
            {
                if (!lateralDischargeData.ContainsKey(lateralNode.Feature.Name)) continue;

                var nodeData = lateralDischargeData[lateralNode.Feature.Name];
                if (nodeData.WaterComponent != null)
                {
                    lateralNode.DataType = nodeData.WaterComponent.BoundaryType;

                    switch (lateralNode.DataType)
                    {
                        case WaterFlowModel1DLateralDataType.FlowConstant:
                            lateralNode.Flow = nodeData.WaterComponent.ConstantBoundaryValue;
                            break;
                        case WaterFlowModel1DLateralDataType.FlowWaterLevelTable:
                        case WaterFlowModel1DLateralDataType.FlowTimeSeries:
                            copyFunction(nodeData.WaterComponent.TimeDependentBoundaryValue, lateralNode.Data);
                            break;
                    }
                }

                if (nodeData.SaltComponent != null)
                {
                    lateralNode.SaltLateralDischargeType = nodeData.SaltComponent.BoundaryType;

                    switch (lateralNode.SaltLateralDischargeType)
                    {
                        case SaltLateralDischargeType.ConcentrationConstant:
                            lateralNode.SaltConcentrationDischargeConstant = nodeData.SaltComponent.ConstantBoundaryValue;
                            break;
                        case SaltLateralDischargeType.ConcentrationTimeSeries:
                            copyFunction(nodeData.SaltComponent.TimeDependentBoundaryValue, lateralNode.SaltConcentrationTimeSeries);
                            break;
                        case SaltLateralDischargeType.MassConstant:
                            lateralNode.SaltMassDischargeConstant = nodeData.SaltComponent.ConstantBoundaryValue;
                            break;
                        case SaltLateralDischargeType.MassTimeSeries:
                            copyFunction(nodeData.SaltComponent.TimeDependentBoundaryValue, lateralNode.SaltMassTimeSeries);
                            break;
                        case SaltLateralDischargeType.Default:
                            break;
                    }
                }

                if (nodeData.TemperatureComponent != null)
                {
                    lateralNode.TemperatureLateralDischargeType = nodeData.TemperatureComponent.BoundaryType;

                    switch (lateralNode.TemperatureLateralDischargeType)
                    {
                        case TemperatureLateralDischargeType.Constant:
                            lateralNode.TemperatureConstant = nodeData.TemperatureComponent.ConstantBoundaryValue;
                            break;
                        case TemperatureLateralDischargeType.TimeDependent:
                            copyFunction(nodeData.TemperatureComponent.TimeDependentBoundaryValue, lateralNode.TemperatureTimeSeries);
                            break;
                        case TemperatureLateralDischargeType.None:
                            break;
                    }
                }
            }
        }

        private static void copyFunction(IFunction from, IFunction to)
        {
            if (from.Arguments.Count != to.Arguments.Count || from.Components.Count != to.Components.Count)
                return;

            for (var i = 0; i < from.Arguments.Count; i++)
            {
                to.Arguments[i].SetValues(from.Arguments[i].Values);
                to.Arguments[i].ExtrapolationType = from.Arguments[i].ExtrapolationType;
                to.Arguments[i].InterpolationType = from.Arguments[i].InterpolationType;
            }

            for (var i = 0; i < from.Components.Count; i++)
            {
                to.Components[i].SetValues(from.Components[i].Values);
                to.Components[i].ExtrapolationType = from.Components[i].ExtrapolationType;
                to.Components[i].InterpolationType = from.Components[i].InterpolationType;
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
