using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Utils.Collections;
using DeltaShell.NGHS.IO.DataObjects;
using DeltaShell.NGHS.Utils;
using DeltaShell.Plugins.DelftModels.RainfallRunoff;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Concepts.Nwrw;
using DeltaShell.Plugins.FMSuite.FlowFM;
using DeltaShell.Sobek.Readers.Readers.SobekRrReaders;
using DeltaShell.Sobek.Readers.SobekDataObjects;
using GeoAPI.Extensions.Networks;
using log4net;
using NetTopologySuite.Geometries;

namespace DeltaShell.Plugins.ImportExport.Sobek.PartialSobekImporter
{
    public class SobekRRNwrwImporter : PartialSobekImporterBase
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(SobekRRNwrwImporter));

        private RainfallRunoffModel rrModel;
        private WaterFlowFMModel fmModel;

        private Dictionary<string, NwrwDryWeatherFlowDefinition> dryweatherFlowDefinitions;
        private Dictionary<string, INode> nodeDictionary;
        private Dictionary<string, IBranch> branchDictionary;
        private HashSet<string> singleUnitDryweatherFlowDefinitions = new HashSet<string>();
        private List<string> listOfWarnings = new List<string>();

        public override string DisplayName => "Rainfall Runoff NWRW data";

        public override SobekImporterCategories Category { get; } = SobekImporterCategories.RainfallRunoff;

        protected override void PartialImport()
        {
            Log.DebugFormat("Importing nwrw data ...");

            rrModel = GetModel<RainfallRunoffModel>();

            ImportNwrwDryweatherFlowDefinitions(rrModel.NwrwDryWeatherFlowDefinitions);
            ImportNwrwDefinitions(rrModel.NwrwDefinitions);

            var lateralSourceDictionary = new Dictionary<string, LateralSource>(StringComparer.InvariantCultureIgnoreCase);

            // Read all NWRW definitions
            Dictionary<string, SobekRRNwrw> readNwrwDefinitions = ReadNwrwDefinitions(GetFilePath(SobekFileNames.SobekRRNwrwFileName));

            if (HydroNetwork != null) // importing RR and FLOW
            {
                if (!CreateNodeAndBranchDictionary())
                {
                    return;
                }

                lateralSourceDictionary = fmModel.LateralSourcesData.Select(lsd => lsd.Feature)
                                                 .ToDictionary(lateral => lateral.Name, StringComparer.InvariantCultureIgnoreCase);

                FilterNwrwDefinitions(readNwrwDefinitions, lateralSourceDictionary);
            }

            AddNwrwDefinitionsToModel(readNwrwDefinitions, lateralSourceDictionary);

            if (listOfWarnings.Any())
            {
                Log.Warn($"While importing nwrw we encountered the following {listOfWarnings.Count} warnings: {Environment.NewLine}{string.Join(Environment.NewLine, listOfWarnings)}");
            }
        }

        private bool CreateNodeAndBranchDictionary()
        {
            nodeDictionary = fmModel.Network.Nodes.ToDictionary(node => node.Name, StringComparer.InvariantCultureIgnoreCase);
            if (nodeDictionary == null || nodeDictionary.Count == 0)
            {
                Log.Warn("Can't import Nwrw catchments, no existing nodes found.");
                return false;
            }

            branchDictionary = fmModel.Network.Branches.ToDictionary(branch => branch.Name, StringComparer.InvariantCultureIgnoreCase);
            if (branchDictionary == null || branchDictionary.Count == 0)
            {
                Log.Warn("Can't import Nwrw catchments, no existing branches found.");
                return false;
            }

            return true;
        }

        #region Dryweather flow definitions

        private void ImportNwrwDryweatherFlowDefinitions(ICollection<NwrwDryWeatherFlowDefinition> existingDefinitions)
        {
            Dictionary<string, SobekRRDryWeatherFlow> readDefinitions = ReadDryweatherFlowDefinitions(GetFilePath(SobekFileNames.SobekRRNwrwDwaFileName));

            var existingDefinitionsSet = new HashSet<string>(existingDefinitions.Select(def => def.Name.ToLowerInvariant()));
            foreach (KeyValuePair<string, SobekRRDryWeatherFlow> readDefinition in readDefinitions)
            {
                if (existingDefinitionsSet.Contains(readDefinition.Key.ToLowerInvariant()))
                {
                    listOfWarnings.Add($"A dryweather flow definition with the name '{readDefinition.Key}' already exists, skipping import.");
                    continue;
                }

                if (readDefinition.Value.ComputationOption == DWAComputationOption.UseTable)
                {
                    listOfWarnings.Add($"Using tables for dryweather flow definitions is currently not supported. Skipping import of '{readDefinition.Key}'.");
                    continue;
                }

                existingDefinitions.Add(CreateNewNwrwDryWeatherFlowDefinition(readDefinition.Value));
            }

            dryweatherFlowDefinitions = existingDefinitions.ToDictionary(definition => definition.Name, StringComparer.InvariantCultureIgnoreCase);
        }

        private Dictionary<string, SobekRRDryWeatherFlow> ReadDryweatherFlowDefinitions(string filePath)
        {
            return new SobekRRDryWeatherFlowReader().Read(filePath).ToDictionaryWithErrorDetails(filePath, item => item.Id, item => item);
        }

        private NwrwDryWeatherFlowDefinition CreateNewNwrwDryWeatherFlowDefinition(SobekRRDryWeatherFlow readDefinition)
        {
            DWAComputationOption readComputationOption = readDefinition.ComputationOption;
            if (readComputationOption == DWAComputationOption.ConstantDWAPerHour || readComputationOption == DWAComputationOption.VariablePerHour)
            {
                singleUnitDryweatherFlowDefinitions.Add(readDefinition.Id);
            }

            var newDefinition = new NwrwDryWeatherFlowDefinition
            {
                Name = readDefinition.Id,
                DistributionType = GetDistributionType(readComputationOption),
                DailyVolumeConstant = readDefinition.WaterUsePerHourForConstant,
                DailyVolumeVariable = readDefinition.WaterUsePerDayForVariable
            };

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
                case DWAComputationOption.ConstantDWAPerHour:
                    return DryweatherFlowDistributionType.Constant;
                case DWAComputationOption.NrPeopleTimesVariablePerHour:
                case DWAComputationOption.VariablePerHour:
                    return DryweatherFlowDistributionType.Daily;
                case DWAComputationOption.UseTable:
                default:
                    throw new NotSupportedException($"{computationOption} is not a valid computation option.");
            }
        }

        #endregion

        #region Nwrw definitions

        private void ImportNwrwDefinitions(ICollection<NwrwDefinition> rrModelNwrwDefinitions)
        {
            if (rrModelNwrwDefinitions.Count != 12)
            {
                throw new ArgumentException();
            }

            SobekRRNwrwSettings[] readNwrwSettings = new SobekRRNwrwSettingsReader().Read(GetFilePath(SobekFileNames.SobekRRNwrwSettingsFileName)).ToArray();
            SobekRRNwrwSettings readNwrwSetting = readNwrwSettings.FirstOrDefault();
            if (readNwrwSetting == null)
            {
                Log.WarnFormat($"No nwrw settings were found.");
                return;
            }

            if (readNwrwSettings.Count() > 1)
            {
                Log.WarnFormat($"Found multiple nwrw settings. Importing the first settings and ignoring the others.");
            }

            UpdateNwrwSettings(rrModelNwrwDefinitions, readNwrwSetting);
        }

        private void UpdateNwrwSettings(IEnumerable<NwrwDefinition> existingDefinitions, SobekRRNwrwSettings readSettings)
        {
            NwrwDefinition[] nwrwDefinitionArray = existingDefinitions.ToArray();

            UpdateRunoffDelayFactors(nwrwDefinitionArray, readSettings);
            UpdateMaximumStorages(nwrwDefinitionArray, readSettings);
            UpdateMaximumInfiltrationCapacities(nwrwDefinitionArray, readSettings);
            UpdateMinimumInfiltrationCapacities(nwrwDefinitionArray, readSettings);
            UpdateInfiltrationCapacityDecrease(nwrwDefinitionArray, readSettings);
            UpdateInfiltrationCapacityIncrease(nwrwDefinitionArray, readSettings);
        }

        private void UpdateRunoffDelayFactors(NwrwDefinition[] nwrwDefinitions, SobekRRNwrwSettings readSettings)
        {
            if (readSettings.RunoffDelayFactors == null)
            {
                listOfWarnings.Add($"Could not find any runoff factors.");
                return;
            }

            if (readSettings.IsOldFormatData)
            {
                for (var i = 0; i < readSettings.RunoffDelayFactors.Length; i++)
                {
                    nwrwDefinitions[i].RunoffDelay = readSettings.RunoffDelayFactors[i];
                    nwrwDefinitions[i + 3].RunoffDelay = readSettings.RunoffDelayFactors[i];
                    nwrwDefinitions[i + 6].RunoffDelay = readSettings.RunoffDelayFactors[i];
                    nwrwDefinitions[i + 9].RunoffDelay = readSettings.RunoffDelayFactors[i];
                }
            }
            else
            {
                for (var i = 0; i < readSettings.RunoffDelayFactors.Length; i++)
                {
                    nwrwDefinitions[i].RunoffDelay = readSettings.RunoffDelayFactors[i];
                }
            }
        }

        private void UpdateMaximumStorages(NwrwDefinition[] nwrwDefinitionArray, SobekRRNwrwSettings readSettings)
        {
            if (readSettings.MaximumStorages == null)
            {
                listOfWarnings.Add("No settings found for maximum storages.");
                return;
            }

            for (var i = 0; i < readSettings.MaximumStorages.Length; i++)
            {
                nwrwDefinitionArray[i].SurfaceStorage = readSettings.MaximumStorages[i];
            }
        }

        private void UpdateMaximumInfiltrationCapacities(NwrwDefinition[] nwrwDefinitionArray, SobekRRNwrwSettings readSettings)
        {
            if (readSettings.MaximumInfiltrationCapcaties == null)
            {
                listOfWarnings.Add("No settings found for maximum infiltration capacities.");
                return;
            }

            for (var i = 0; i < readSettings.MaximumInfiltrationCapcaties.Length - 1; i++)
            {
                nwrwDefinitionArray[i].InfiltrationCapacityMax = readSettings.MaximumInfiltrationCapcaties[0];
                nwrwDefinitionArray[i + 3].InfiltrationCapacityMax = readSettings.MaximumInfiltrationCapcaties[1];
                nwrwDefinitionArray[i + 6].InfiltrationCapacityMax = readSettings.MaximumInfiltrationCapcaties[2];
                nwrwDefinitionArray[i + 9].InfiltrationCapacityMax = readSettings.MaximumInfiltrationCapcaties[3];
            }
        }

        private void UpdateMinimumInfiltrationCapacities(NwrwDefinition[] nwrwDefinitionArray, SobekRRNwrwSettings readSettings)
        {
            if (readSettings.MinimumInfiltrationCapcaties == null)
            {
                listOfWarnings.Add("No settings found for minimum infiltration capacities.");
                return;
            }

            for (var i = 0; i < readSettings.MinimumInfiltrationCapcaties.Length - 1; i++)
            {
                nwrwDefinitionArray[i].InfiltrationCapacityMin = readSettings.MinimumInfiltrationCapcaties[0];
                nwrwDefinitionArray[i + 3].InfiltrationCapacityMin = readSettings.MinimumInfiltrationCapcaties[1];
                nwrwDefinitionArray[i + 6].InfiltrationCapacityMin = readSettings.MinimumInfiltrationCapcaties[2];
                nwrwDefinitionArray[i + 9].InfiltrationCapacityMin = readSettings.MinimumInfiltrationCapcaties[3];
            }
        }

        private void UpdateInfiltrationCapacityDecrease(NwrwDefinition[] nwrwDefinitionArray, SobekRRNwrwSettings readSettings)
        {
            if (readSettings.InfiltrationCapacityDecreases == null)
            {
                listOfWarnings.Add("No settings found for infiltration capacity reduction.");
                return;
            }

            for (var i = 0; i < readSettings.InfiltrationCapacityDecreases.Length - 1; i++)
            {
                nwrwDefinitionArray[i].InfiltrationCapacityReduction = readSettings.InfiltrationCapacityDecreases[0];
                nwrwDefinitionArray[i + 3].InfiltrationCapacityReduction = readSettings.InfiltrationCapacityDecreases[1];
                nwrwDefinitionArray[i + 6].InfiltrationCapacityReduction = readSettings.InfiltrationCapacityDecreases[2];
                nwrwDefinitionArray[i + 9].InfiltrationCapacityReduction = readSettings.InfiltrationCapacityDecreases[3];
            }
        }

        private void UpdateInfiltrationCapacityIncrease(NwrwDefinition[] nwrwDefinitionArray, SobekRRNwrwSettings readSettings)
        {
            if (readSettings.InfiltrationCapacityIncreases == null)
            {
                listOfWarnings.Add("No settings found for infiltration capacity recovery.");
                return;
            }

            for (var i = 0; i < readSettings.InfiltrationCapacityIncreases.Length - 1; i++)
            {
                nwrwDefinitionArray[i].InfiltrationCapacityRecovery = readSettings.InfiltrationCapacityIncreases[0];
                nwrwDefinitionArray[i + 3].InfiltrationCapacityRecovery = readSettings.InfiltrationCapacityIncreases[1];
                nwrwDefinitionArray[i + 6].InfiltrationCapacityRecovery = readSettings.InfiltrationCapacityIncreases[2];
                nwrwDefinitionArray[i + 9].InfiltrationCapacityRecovery = readSettings.InfiltrationCapacityIncreases[3];
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

        private void FilterNwrwDefinitions(Dictionary<string, SobekRRNwrw> readNwrwDefinitions, Dictionary<string, LateralSource> lateralSourceDictionary)
        {
            Dictionary<string, SobekRRNode> readSobekRRNodeDictionary = ReadNwrwNodes(GetFilePath(SobekFileNames.SobekRRRunoffNodesFileName));
            Dictionary<string, SobekRRNode> filteredReadSobekRRNodeDictionary = FilterSobekRRNodes(readSobekRRNodeDictionary);

            foreach (SobekRRNwrw readNwrwDefinition in readNwrwDefinitions.Values)
            {
                if (!filteredReadSobekRRNodeDictionary.ContainsKey(readNwrwDefinition.Id))
                {
                    listOfWarnings.Add($"Could not import nwrw catchment, target node or branch '{readNwrwDefinition.Id}' was not found in the network.");
                    readNwrwDefinitions.Remove(readNwrwDefinition.Id);
                    continue;
                }

                if (!lateralSourceDictionary.ContainsKey(readNwrwDefinition.Id))
                {
                    listOfWarnings.Add($"Could not import nwrw catchment, no lateral was found for the nwrw catchment '{readNwrwDefinition.Id}.");
                    readNwrwDefinitions.Remove(readNwrwDefinition.Id);
                }
            }
        }

        private void AddNwrwDefinitionsToModel(Dictionary<string, SobekRRNwrw> readNwrwDefinitions, Dictionary<string, LateralSource> lateralSourceDictionary)
        {
            Dictionary<string, NwrwData> catchmentModelData = GetNwrwCatchmentModelData();

            var helper = new NwrwImporterHelper
            {
                CurrentNwrwCatchmentModelDataByNodeOrBranchId =
                    new ConcurrentDictionary<string, NwrwData>(catchmentModelData)
            };

            foreach (SobekRRNwrw readNwrwDefinition in readNwrwDefinitions.Values)
            {
                if (catchmentModelData.ContainsKey(readNwrwDefinition.Id))
                {
                    UpdateNwrwCatchmentData(catchmentModelData, readNwrwDefinition);
                }
                else
                {
                    if (HydroNetwork != null)
                    {
                        LateralSource targetLateralSource = lateralSourceDictionary[readNwrwDefinition.Id];
                        fmModel.LateralSourcesData
                               .Where(lsd => Equals(lsd.Feature, targetLateralSource) &&
                                             !Equals(lsd.DataType, Model1DLateralDataType.FlowRealTime))
                               .ForEach(lsd => lsd.DataType = Model1DLateralDataType.FlowRealTime);

                        AddNwrwCatchmentDataToModel(readNwrwDefinition, targetLateralSource, helper);
                    }

                    AddNwrwCatchmentDataToModel(readNwrwDefinition, helper);
                }
            }
        }

        private Dictionary<string, SobekRRNode> ReadNwrwNodes(string filePath)
        {
            return new SobekRRNodeReader().Read(filePath).ToDictionaryWithErrorDetails(filePath, n => n.Id, n => n);
        }

        private Dictionary<string, SobekRRNode> FilterSobekRRNodes(Dictionary<string, SobekRRNode> readSobekRRNodeDictionary)
        {
            var filteredReadSobekRRNodeDictionary = new Dictionary<string, SobekRRNode>();
            var pipeIdentifier = "SBK_PIPE";
            var nodeIdentifier = "SBK_CONN";

            foreach (KeyValuePair<string, SobekRRNode> readSobekRRNodeKeyValuePair in readSobekRRNodeDictionary)
            {
                string targetName = readSobekRRNodeKeyValuePair.Key;
                SobekRRNode sobekRRNode = readSobekRRNodeKeyValuePair.Value;
                string objectTypeName = sobekRRNode.ObjectTypeName;

                if (sobekRRNode.NodeType != SobekRRNodeType.NWRW)
                {
                    continue; // filter out non-nwrw node types
                }

                if (objectTypeName.Contains(nodeIdentifier)) // check if target is node
                {
                    if (nodeDictionary.ContainsKey(targetName))
                    {
                        filteredReadSobekRRNodeDictionary[targetName] = sobekRRNode;
                    }
                    else if (branchDictionary.ContainsKey(targetName))
                    {
                        filteredReadSobekRRNodeDictionary[targetName] = sobekRRNode;
                    }
                    else if (fmModel.Network.Compartments.Any(c => c.Name.Equals(targetName, StringComparison.InvariantCultureIgnoreCase)))
                    {
                        filteredReadSobekRRNodeDictionary[targetName] = sobekRRNode;
                    }
                }
                else if (objectTypeName.Contains(pipeIdentifier)) // check if target is branch
                {
                    if (branchDictionary.ContainsKey(sobekRRNode.ReachId))
                    {
                        filteredReadSobekRRNodeDictionary[sobekRRNode.ReachId] = sobekRRNode; // find by branchId
                        filteredReadSobekRRNodeDictionary[targetName] = sobekRRNode;          // find by targetIdentifier
                    }
                }
                else
                {
                    listOfWarnings.Add($"Could not import nwrw catchment, target node or branch '{targetName}' was not found in the network.");
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
            NwrwData nwrwData = catchmentModelData[readDefinition.Id];
            SetNwrwCatchmentData(nwrwData, readDefinition);
        }

        private void AddNwrwCatchmentDataToModel(SobekRRNwrw readDefinition, LateralSource lateralSource, NwrwImporterHelper helper)
        {
            string nodeOrBranchId = lateralSource.Name;

            NwrwData.CreateNewNwrwDataWithCatchment(rrModel, nodeOrBranchId, helper);
            NwrwData nwrwData = helper.CurrentNwrwCatchmentModelDataByNodeOrBranchId[nodeOrBranchId];
            nwrwData.NodeOrBranchId = nodeOrBranchId;

            Catchment catchment = nwrwData.Catchment;
            catchment.IsGeometryDerivedFromAreaSize = true;
            catchment.Geometry = lateralSource.Geometry;

            SetNwrwCatchmentData(nwrwData, readDefinition);
            AddNwrwCatchmentLinkToFmModel(catchment, lateralSource);
        }

        private void AddNwrwCatchmentDataToModel(SobekRRNwrw readDefinition, NwrwImporterHelper helper)
        {
            string nodeOrBranchId = readDefinition.Id;

            NwrwData.CreateNewNwrwDataWithCatchment(rrModel, nodeOrBranchId, helper);
            NwrwData nwrwData = helper.CurrentNwrwCatchmentModelDataByNodeOrBranchId[nodeOrBranchId];
            nwrwData.NodeOrBranchId = nodeOrBranchId;

            Catchment catchment = nwrwData.Catchment;
            catchment.IsGeometryDerivedFromAreaSize = true;

            SetNwrwCatchmentData(nwrwData, readDefinition);
        }

        private void AddNwrwCatchmentLinkToFmModel(Catchment catchment, LateralSource lateralSource)
        {
            HydroLink hydroLink = catchment.LinkTo(lateralSource);
            if (hydroLink != null)
            {
                hydroLink.Geometry = new LineString(new[]
                {
                    catchment.InteriorPoint?.Coordinate,
                    lateralSource.Geometry?.Coordinate
                });
            }
        }

        private void SetNwrwCatchmentData(NwrwData nwrwData, SobekRRNwrw readDefinition)
        {
            nwrwData.MeteoStationId = readDefinition.MeteoStationId;
            nwrwData.LateralSurface = readDefinition.SurfaceLevel;

            SetDryweatherFlows(nwrwData, (rd) => rd.InhabitantDwaId, (rd) => rd.NumberOfPeople, readDefinition, 0);
            SetDryweatherFlows(nwrwData, (rd) => rd.CompanyDwaId, (rd) => rd.NumberOfUnits, readDefinition, 1);
            SetSurfaceTypes(nwrwData, readDefinition);

            nwrwData.UpdateCatchmentAreaSize();
        }

        private void SetDryweatherFlows(NwrwData data,
                                        Func<SobekRRNwrw, string> getDwaIdFunc,
                                        Func<SobekRRNwrw, double> getNumberFunc,
                                        SobekRRNwrw dataToSetFrom,
                                        int index)
        {
            string dwaId = getDwaIdFunc(dataToSetFrom);
            if (!string.IsNullOrWhiteSpace(dwaId))
            {
                if (!dryweatherFlowDefinitions.ContainsKey(dwaId))
                {
                    listOfWarnings.Add($"Could not add dryweather flow definition {dwaId} to nwrw catchment, because it is not defined.");
                    return;
                }

                var dwfDefintion = new DryWeatherFlow(dwaId) {NumberOfUnits = getNumberFunc(dataToSetFrom)};
                data.DryWeatherFlows[index] = dwfDefintion;

                if (singleUnitDryweatherFlowDefinitions.Contains(dwaId))
                {
                    dwfDefintion.NumberOfUnits = 1;
                }
            }
        }

        private void SetSurfaceTypes(NwrwData nwrwData, SobekRRNwrw readDefinition)
        {
            NwrwSurfaceType[] surfaceTypesInCorrectOrder = NwrwSurfaceTypeHelper.SurfaceTypesInCorrectOrder.ToArray();
            for (var i = 0; i < surfaceTypesInCorrectOrder.Length; i++)
            {
                NwrwSurfaceType currentSurfaceType = surfaceTypesInCorrectOrder[i];
                nwrwData.SurfaceLevelDict[currentSurfaceType] = readDefinition.Areas[i];
            }
        }

        #endregion
    }
}