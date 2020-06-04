using DelftTools.Hydro;
using DeltaShell.Plugins.DelftModels.RainfallRunoff;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Concepts.Nwrw;
using DeltaShell.Plugins.FMSuite.FlowFM;
using DeltaShell.Sobek.Readers.Readers.SobekRrReaders;
using DeltaShell.Sobek.Readers.SobekDataObjects;
using GeoAPI.Extensions.Networks;
using log4net;
using NetTopologySuite.Geometries;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace DeltaShell.Plugins.ImportExport.Sobek.PartialSobekImporter
{
    public class SobekRRNwrwImporter : PartialSobekImporterBase
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(SobekRRNwrwImporter));

        private RainfallRunoffModel rrModel;
        private WaterFlowFMModel fmModel;
        private Dictionary<string, NwrwDryWeatherFlowDefinition> dryweatherFlowDefinitions;
        private Dictionary<string, INode> nodeDictionary;
        private Dictionary<string, NwrwData> nwrwDataDictionary = new Dictionary<string, NwrwData>();
        private List<string> listOfWarnings = new List<string>();

        public override string DisplayName => "Rainfall Runoff NWRW data";

        protected override void PartialImport()
        {
            Log.DebugFormat("Importing nwrw data ...");

            rrModel = GetModel<RainfallRunoffModel>();
            
            ImportNwrwDryweatherFlowDefinitions(rrModel.NwrwDryWeatherFlowDefinitions);
            ImportNwrwDefinitions(rrModel.NwrwDefinitions);

            try
            {
                fmModel = GetModel<WaterFlowFMModel>();
            }
            catch
            {
                Log.Warn("Can't import Nwrw catchments, no network found.");
                return;
            }

            ImportNwrwCatchments();

            if (listOfWarnings.Any())
                Log.Warn($"While importing nwrw we encountered the following {listOfWarnings.Count} warnings: {Environment.NewLine}{string.Join(Environment.NewLine, listOfWarnings)}");
        }

        #region Dryweather flow definitions
        private void ImportNwrwDryweatherFlowDefinitions(ICollection<NwrwDryWeatherFlowDefinition> rrModelDryweatherFlowDefinitions)
        {
            var readDefinitions = ReadDryweatherFlowDefinitions(GetFilePath(SobekFileNames.SobekRRNwrwDwaFileName));
            foreach (var dryweatherFlowDefinition in readDefinitions)
            {
                if (rrModelDryweatherFlowDefinitions.Any(definition => definition.Name.Equals(dryweatherFlowDefinition.Key, StringComparison.InvariantCultureIgnoreCase)))
                {
                    listOfWarnings.Add($"A dryweather flow definition with the name '{dryweatherFlowDefinition.Key}' already exists, skipping import.");
                    continue;
                }

                rrModelDryweatherFlowDefinitions.Add(CreateNewNwrwDryWeatherFlowDefinition(dryweatherFlowDefinition.Value));
            }
            dryweatherFlowDefinitions = rrModelDryweatherFlowDefinitions.ToDictionary(definition => definition.Name, StringComparer.InvariantCultureIgnoreCase);
        }

        private Dictionary<string, SobekRRDryWeatherFlow> ReadDryweatherFlowDefinitions(string filePath)
        {
            return new SobekRRDryWeatherFlowReader().Read(filePath).ToDictionaryWithErrorDetails(filePath, item => item.Id, item => item);
        }
        
        private NwrwDryWeatherFlowDefinition CreateNewNwrwDryWeatherFlowDefinition(SobekRRDryWeatherFlow readDefinition)
        {
            var newDefinition = new NwrwDryWeatherFlowDefinition();
            newDefinition.Name = readDefinition.Id;
            newDefinition.DistributionType = GetDistributionType(readDefinition.ComputationOption);
            newDefinition.DailyVolumeConstant = readDefinition.WaterUsePerHourForConstant;
            newDefinition.DailyVolumeVariable = readDefinition.WaterUsePerDayForVariable;
            if (readDefinition.WaterCapacityPerHour.Length != 24)
            {
                listOfWarnings.Add($"Expected 24 values but got {readDefinition.WaterCapacityPerHour.Length} values. Skipping import of water use per capita per hour.");
            }
            else
            {
                newDefinition.HourlyPercentageDailyVolume = readDefinition.WaterCapacityPerHour;
            }

            return newDefinition;
        }

        private DryweatherFlowDistributionType GetDistributionType(DWAComputationOption computationOption)
        {
            switch (computationOption)
            {
                case DWAComputationOption.NrPeopleTimesConstantPerHour:
                    return DryweatherFlowDistributionType.Constant;
                case DWAComputationOption.NrPeopleTimesVariablePerHour:
                    return DryweatherFlowDistributionType.Daily;
                case DWAComputationOption.ConstantDWAPerHour: // not supported?
                case DWAComputationOption.VariablePerHour: // not supported?
                case DWAComputationOption.UseTable: // not supported?
                default:
                    throw new InvalidOperationException($"{computationOption} is not a valid computation option.");

            }
        }
        #endregion

        #region Nwrw definitions
        private void ImportNwrwDefinitions(IList<NwrwDefinition> rrModelNwrwDefinitions)
        {
            if (rrModelNwrwDefinitions.Count != 12)
            {
                throw new InvalidOperationException();
            }

            var readNwrwSettings = new SobekRRNwrwSettingsReader().Read(GetFilePath(SobekFileNames.SobekRRNwrwSettingsFileName));
            var readNwrwSetting = readNwrwSettings.FirstOrDefault();
            if (readNwrwSetting == null) return;

            UpdateNwrwSettings(rrModelNwrwDefinitions, readNwrwSetting);
        }

        private void UpdateNwrwSettings(IEnumerable<NwrwDefinition> existingDefinitions, SobekRRNwrwSettings readSettings)
        {
            var nwrwDefinitionArray = existingDefinitions.ToArray();

            UpdateRunoffDelayFactors(nwrwDefinitionArray, readSettings);
            UpdateMaximumStorages(nwrwDefinitionArray, readSettings);
            UpdateMaximumInfiltrationCapacities(nwrwDefinitionArray, readSettings);
            UpdateMinimumInfiltrationCapacities(nwrwDefinitionArray, readSettings);
            UpdateInfiltrationCapacityDecrease(nwrwDefinitionArray, readSettings);
            UpdateInfiltrationCapacityIncrease(nwrwDefinitionArray, readSettings);
            //UpdateInfiltrationFromDepressionsOption(nwrwDefinitionArray, readSettings);
            //UpdateInfiltrationFromRunoffOption(nwrwDefinitionArray, readSettings);

        }

        private void UpdateRunoffDelayFactors(NwrwDefinition[] nwrwDefinitionArray, SobekRRNwrwSettings readSettings)
        {
            if (readSettings.RunoffDelayFactors != null)
            {
                for (int i = 0; i < readSettings.RunoffDelayFactors.Length; i++)
                {
                    nwrwDefinitionArray[i].RunoffDelay = readSettings.RunoffDelayFactors[i];
                }
            }
            else
            {
                if (readSettings.RunoffDelayFactorsOldTag == null)
                {
                    listOfWarnings.Add($"Could not find any runoff factors.");
                }
                else
                {
                    for (int i = 0; i < readSettings.RunoffDelayFactorsOldTag.Length; i++)
                    {
                        nwrwDefinitionArray[i].RunoffDelay = readSettings.RunoffDelayFactorsOldTag[i];
                        nwrwDefinitionArray[i + 3].RunoffDelay = readSettings.RunoffDelayFactorsOldTag[i];
                        nwrwDefinitionArray[i + 6].RunoffDelay = readSettings.RunoffDelayFactorsOldTag[i];
                        nwrwDefinitionArray[i + 9].RunoffDelay = readSettings.RunoffDelayFactorsOldTag[i];
                    }
                }
            }
        }

        private static void UpdateMaximumStorages(NwrwDefinition[] nwrwDefinitionArray, SobekRRNwrwSettings readSettings)
        {
            for (int i = 0; i < readSettings.MaximumStorages.Length; i++)
            {
                nwrwDefinitionArray[i].SurfaceStorage = readSettings.MaximumStorages[i];
            }
        }

        private void UpdateMaximumInfiltrationCapacities(NwrwDefinition[] nwrwDefinitionArray, SobekRRNwrwSettings readSettings)
        {
            for (int i = 0; i < readSettings.MaximumInfiltrationCapcaties.Length; i++)
            {
                nwrwDefinitionArray[i].InfiltrationCapacityMax = readSettings.MaximumInfiltrationCapcaties[i];
                nwrwDefinitionArray[i + 4].InfiltrationCapacityMax = readSettings.MaximumInfiltrationCapcaties[i];
                nwrwDefinitionArray[i + 8].InfiltrationCapacityMax = readSettings.MaximumInfiltrationCapcaties[i];
            }
        }

        private void UpdateMinimumInfiltrationCapacities(NwrwDefinition[] nwrwDefinitionArray, SobekRRNwrwSettings readSettings)
        {
            for (int i = 0; i < readSettings.MinimumInfiltrationCapcaties.Length; i++)
            {
                nwrwDefinitionArray[i].InfiltrationCapacityMin = readSettings.MinimumInfiltrationCapcaties[i];
                nwrwDefinitionArray[i + 4].InfiltrationCapacityMin = readSettings.MinimumInfiltrationCapcaties[i];
                nwrwDefinitionArray[i + 8].InfiltrationCapacityMin = readSettings.MinimumInfiltrationCapcaties[i];
            }
        }

        private void UpdateInfiltrationCapacityDecrease(NwrwDefinition[] nwrwDefinitionArray, SobekRRNwrwSettings readSettings)
        {
            for (int i = 0; i < readSettings.InfiltrationCapacityDecreases.Length; i++)
            {
                nwrwDefinitionArray[i].InfiltrationCapacityReduction = readSettings.InfiltrationCapacityDecreases[i];
                nwrwDefinitionArray[i + 4].InfiltrationCapacityReduction = readSettings.InfiltrationCapacityDecreases[i];
                nwrwDefinitionArray[i + 8].InfiltrationCapacityReduction = readSettings.InfiltrationCapacityDecreases[i];
            }
        }

        private void UpdateInfiltrationCapacityIncrease(NwrwDefinition[] nwrwDefinitionArray, SobekRRNwrwSettings readSettings)
        {
            for (int i = 0; i < readSettings.InfiltrationCapacityIncreases.Length; i++)
            {
                nwrwDefinitionArray[i].InfiltrationCapacityRecovery = readSettings.InfiltrationCapacityIncreases[i];
                nwrwDefinitionArray[i + 4].InfiltrationCapacityRecovery = readSettings.InfiltrationCapacityIncreases[i];
                nwrwDefinitionArray[i + 8].InfiltrationCapacityRecovery = readSettings.InfiltrationCapacityIncreases[i];
            }
        }

        private void UpdateInfiltrationFromDepressionsOption(NwrwDefinition[] nwrwDefinitionArray, SobekRRNwrwSettings readSettings)
        {
            throw new NotImplementedException();
        }

        private void UpdateInfiltrationFromRunoffOption(NwrwDefinition[] nwrwDefinitionArray, SobekRRNwrwSettings readSettings)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Nwrw catchments
        private void ImportNwrwCatchments()
        {
            nodeDictionary = fmModel.Network.Nodes.ToDictionary(node => node.Name, StringComparer.InvariantCultureIgnoreCase);
            if (nodeDictionary == null || nodeDictionary.Count == 0)
            {
                return;
            }

            var lateralSourceDictionary = fmModel.LateralSourcesData.Select(lsd => lsd.Feature).ToDictionary(lateral => lateral.Name, StringComparer.InvariantCultureIgnoreCase);
            var readSobekRRNodeDictionary = ReadNwrwNodes(GetFilePath(SobekFileNames.SobekRRRunoffNodesFileName));
            var filteredReadSobekRRNodeDictionary = FilterNonNwrwNodesAndNonExistingNodes(readSobekRRNodeDictionary);
            var catchmentModelData = GetNwrwCatchmentModelData();

            NwrwImporterHelper helper = new NwrwImporterHelper();
            helper.CurrentNwrwCatchmentModelDataByNodeOrBranchId = new ConcurrentDictionary<string, NwrwData>(catchmentModelData);

            var readNwrwDefinitions = ReadNwrwDefinitions(GetFilePath(SobekFileNames.SobekRRNwrwFileName));

            foreach (var readNwrwDefinition in readNwrwDefinitions.Values)
            {
                if (!filteredReadSobekRRNodeDictionary.ContainsKey(readNwrwDefinition.Id))
                {
                    listOfWarnings.Add($"Could not import nwrw catchment, target node {readNwrwDefinition.Id} was not found in the network.");
                    continue;
                }

                if (!lateralSourceDictionary.ContainsKey(readNwrwDefinition.Id))
                {
                    listOfWarnings.Add($"Could not import nwrw catchment, no lateral was found for the nwrw catchment.");
                    continue;
                }

                if (catchmentModelData.ContainsKey(readNwrwDefinition.Id))
                {
                    UpdateNwrwCatchmentData(catchmentModelData, readNwrwDefinition);
                }
                else
                {
                    AddNwrwCatchmentDataToModel(readNwrwDefinition, lateralSourceDictionary, helper);
                }
            }
        }

        private Dictionary<string, SobekRRNode> ReadNwrwNodes(string filePath)
        {
            return new SobekRRNodeReader().Read(filePath).ToDictionaryWithErrorDetails(filePath, n => n.Id, n => n);
        }

        private new Dictionary<string, SobekRRNode> FilterNonNwrwNodesAndNonExistingNodes(
            Dictionary<string, SobekRRNode> readSobekRRNodeDictionary)
        {
            var filteredReadSobekRRNodeDictionary = new Dictionary<string, SobekRRNode>();

            foreach (var readSobekRRNodeKeyValuePair in readSobekRRNodeDictionary)
            {
                var targetNodeName = readSobekRRNodeKeyValuePair.Key;
                var sobekRRNode = readSobekRRNodeKeyValuePair.Value;
                if (sobekRRNode.NodeType != SobekRRNodeType.NWRW) continue;

                if (nodeDictionary.ContainsKey(targetNodeName))
                {
                    filteredReadSobekRRNodeDictionary.Add(targetNodeName, sobekRRNode);
                }
                else
                {
                    listOfWarnings.Add(
                        $"Could not import nwrw catchment, target node {targetNodeName} was not found in the network.");
                }
            }

            return filteredReadSobekRRNodeDictionary;
        }

        private Dictionary<string, NwrwData> GetNwrwCatchmentModelData()
        {
            return rrModel.GetAllModelData()
                .OfType<NwrwData>()
                .ToDictionary(rra => rra.Name, StringComparer.InvariantCultureIgnoreCase);
        }

        private Dictionary<string, SobekRRNwrw> ReadNwrwDefinitions(string filePath)
        {
            return new SobekRRNwrwReader().Read(filePath).ToDictionaryWithErrorDetails(filePath, item => item.Id, item => item);
        }

        private void UpdateNwrwCatchmentData(Dictionary<string, NwrwData> catchmentModelData, SobekRRNwrw readDefinition)
        {
            var nwrwData = catchmentModelData[readDefinition.Id];
            SetNwrwCatchmentData(readDefinition, nwrwData);
        }

        private void AddNwrwCatchmentDataToModel(SobekRRNwrw readDefinition, Dictionary<string, LateralSource> lateralSourceDictionary, NwrwImporterHelper helper)
        {
            var nodeId = readDefinition.Id;

            NwrwData.CreateNewNwrwDataWithCatchment(rrModel, nodeId, helper);
            var nwrwData = helper.CurrentNwrwCatchmentModelDataByNodeOrBranchId[nodeId];
            nwrwData.NodeOrBranchId = nodeId;
            var catchment = nwrwData.Catchment;

            // var catchment = Catchment.CreateDefault();
            // catchment.CatchmentType = CatchmentType.NWRW;
            // catchment.Name = nodeId;
            catchment.IsGeometryDerivedFromAreaSize = true;
            catchment.Geometry = nodeDictionary[nodeId].Geometry;
            // var nwrwData = new NwrwData(catchment) {NodeOrBranchId = nodeId};
            //
            // rrModel.Basin.Catchments.Add(catchment);
            // retrieve added ModelData

            SetNwrwCatchmentData(readDefinition, nwrwData);
            AddNwrwCatchmentLinkToFmModel(catchment, lateralSourceDictionary);
        }

        private void AddNwrwCatchmentLinkToFmModel(Catchment catchment, Dictionary<string, LateralSource> lateralSourceDictionary)
        {
            var catchmentName = catchment.Name;
            if (!lateralSourceDictionary.ContainsKey(catchmentName))
            {
                listOfWarnings.Add($"Could not import nwrw catchment. Corresponding lateral source was not found.");
            }

            var lateralSource = lateralSourceDictionary[catchmentName];

            var hydroLink = catchment.LinkTo(lateralSource);
            if (hydroLink != null)
            {
                hydroLink.Geometry = new LineString(new[]
                {
                    catchment.InteriorPoint?.Coordinate,
                    lateralSource?.Geometry?.Coordinate
                });
            }
        }

        private void SetNwrwCatchmentData(SobekRRNwrw readDefinition, NwrwData nwrwData)
        {
            nwrwData.MeteoStationId = readDefinition.MeteoStationId;
            nwrwData.LateralSurface = readDefinition.SurfaceLevel;
            
            var inhabitantDwfName = readDefinition.InhabitantDwaId;
            if (!string.IsNullOrWhiteSpace(inhabitantDwfName))
            {
                if (!dryweatherFlowDefinitions.ContainsKey(inhabitantDwfName))
                {
                    listOfWarnings.Add($"Could not add dryweather flow definition {inhabitantDwfName} to nwrw catchment, because it is not defined.");
                }

                var dwfDefintion = new DryWeatherFlow(readDefinition.InhabitantDwaId) {NumberOfUnits = readDefinition.NumberOfPeople};
                nwrwData.DryWeatherFlows[0] = dwfDefintion;
            }

            var companyDwfName = readDefinition.CompanyDwaId;
            if (!string.IsNullOrWhiteSpace(companyDwfName))
            {
                if (!dryweatherFlowDefinitions.ContainsKey(companyDwfName))
                {
                    listOfWarnings.Add($"Could not add dryweather flow definition {companyDwfName} to nwrw catchment, because it is not defined.");
                }

                var dwfDefintion = new DryWeatherFlow(readDefinition.CompanyDwaId) {NumberOfUnits = readDefinition.NumberOfUnits};
                nwrwData.DryWeatherFlows[1] = dwfDefintion;
            }

            var surfaceTypesInCorrectOrder = NwrwSurfaceTypeHelper.SurfaceTypesInCorrectOrder.ToArray();
            for (int i = 0; i < surfaceTypesInCorrectOrder.Length; i++)
            {
                var currentSurfaceType = surfaceTypesInCorrectOrder[i];
                nwrwData.SurfaceLevelDict[currentSurfaceType] = readDefinition.Areas[i];
            }
            nwrwData.UpdateCatchmentAreaSize();
        }

        #endregion
    }
}