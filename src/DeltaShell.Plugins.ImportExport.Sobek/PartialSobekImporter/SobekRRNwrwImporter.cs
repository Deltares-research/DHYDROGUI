using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Utils.Collections;
using DeltaShell.NGHS.Common.Logging;
using DeltaShell.NGHS.IO.DataObjects;
using DeltaShell.NGHS.Utils;
using DeltaShell.Plugins.DelftModels.RainfallRunoff;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Concepts.Nwrw;
using DeltaShell.Plugins.FMSuite.FlowFM;
using DeltaShell.Plugins.ImportExport.Sobek.Builders;
using DeltaShell.Sobek.Readers.Readers.SobekRrReaders;
using DeltaShell.Sobek.Readers.SobekDataObjects;
using log4net;
using NetTopologySuite.Geometries;

namespace DeltaShell.Plugins.ImportExport.Sobek.PartialSobekImporter
{
    public class SobekRRNwrwImporter : PartialSobekImporterBase
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(SobekRRNwrwImporter));
        private static readonly ILogHandler logHandler = new LogHandler("importing RR NWRW data", Log);
        private static readonly NwrwDryWeatherFlowDefinitionBuilder nwrwDryWeatherFlowDefinitionBuilder = new NwrwDryWeatherFlowDefinitionBuilder(logHandler);

        private RainfallRunoffModel rrModel;
        
        private HashSet<string> dryweatherFlowDefinitions;
        private HashSet<string> nodeDictionary;
        private HashSet<string> branchDictionary;
        private HashSet<string> singleUnitDryweatherFlowDefinitions = new HashSet<string>();

        public override string DisplayName => "Rainfall Runoff NWRW data";

        public override SobekImporterCategories Category { get; } = SobekImporterCategories.RainfallRunoff;

        protected override void PartialImport()
        {
            Log.DebugFormat("Importing nwrw data ...");

            rrModel = GetModel<RainfallRunoffModel>();
            
            var lateralSourceFeatureDictionary = new Dictionary<string, ILateralSource>(StringComparer.InvariantCultureIgnoreCase);

            // Read all NWRW definitions
            Dictionary<string, SobekRRNwrw> readNwrwDefinitions = ReadNwrwDefinitions(GetFilePath(SobekFileNames.SobekRRNwrwFileName));
            Dictionary<string, SobekRRNode> readSobekRrNodeDictionary = ReadNwrwNodes(GetFilePath(SobekFileNames.SobekRRRunoffNodesFileName));

            if (readSobekRrNodeDictionary.Any())
            {
                ImportNwrwDryweatherFlowDefinitions(rrModel.NwrwDryWeatherFlowDefinitions);
                ImportNwrwDefinitions(rrModel.NwrwDefinitions);
            }

            if (HydroNetwork != null) // importing RR and FLOW
            {
                if (!CreateNodeAndBranchDictionary())
                {
                    return;
                }
                lateralSourceFeatureDictionary = HydroNetwork.LateralSources.ToDictionary(ls => ls.Name, StringComparer.InvariantCultureIgnoreCase);
                FilterNwrwDefinitions(readNwrwDefinitions, lateralSourceFeatureDictionary, readSobekRrNodeDictionary);
            }

            AddNwrwDefinitionsToModel(readNwrwDefinitions.Values, lateralSourceFeatureDictionary, readSobekRrNodeDictionary);

            logHandler.LogReport();
        }

        private bool CreateNodeAndBranchDictionary()
        {
            nodeDictionary = new HashSet<string>(HydroNetwork.Nodes.Select(node => node.Name), StringComparer.InvariantCultureIgnoreCase);
            if (nodeDictionary == null || nodeDictionary.Count == 0)
            {
                logHandler.ReportWarning("Cannot import NWRW catchments, no existing nodes found.");
                return false;
            }

            branchDictionary = new HashSet<string>(HydroNetwork.Branches.Select(branch => branch.Name), StringComparer.InvariantCultureIgnoreCase);
            if (branchDictionary == null || branchDictionary.Count == 0)
            {
                logHandler.ReportWarning("Cannot import NWRW catchments, no existing branches found.");
                return false;
            }

            return true;
        }

        #region Dryweather flow definitions

        private void ImportNwrwDryweatherFlowDefinitions(ICollection<NwrwDryWeatherFlowDefinition> existingDefinitions)
        {
            const string defaultDwa = "Default_DWA";
            
            Dictionary<string, SobekRRDryWeatherFlow> readDefinitions = ReadDryweatherFlowDefinitions(GetFilePath(SobekFileNames.SobekRRNwrwDwaFileName));

            var existingDefinitionsSet = new HashSet<string>(existingDefinitions.Select(def => def.Name.ToLowerInvariant()));
            foreach (KeyValuePair<string, SobekRRDryWeatherFlow> readDefinition in readDefinitions)
            {
                if (existingDefinitionsSet.Contains(readDefinition.Key.ToLowerInvariant()))
                {
                    if (!string.Equals(readDefinition.Key, defaultDwa, StringComparison.InvariantCultureIgnoreCase))
                    {
                        logHandler.ReportWarning($"A dryweather flow definition with the name '{readDefinition.Key}' already exists, skipping import.");
                        continue;
                    }
                    
                    existingDefinitions.RemoveAllWhere(definition => string.Equals(definition.Name, defaultDwa, StringComparison.InvariantCultureIgnoreCase));
                    AddToSingleUnitDryWeatherFlowDefinitions(readDefinition.Value);
                    existingDefinitions.Add(nwrwDryWeatherFlowDefinitionBuilder.Build(readDefinition.Value));
                    continue;
                }

                if (readDefinition.Value.ComputationOption == DWAComputationOption.UseTable)
                {
                    logHandler.ReportWarning($"Using tables for dryweather flow definitions is currently not supported. Skipping import of '{readDefinition.Key}'.");
                    continue;
                }

                AddToSingleUnitDryWeatherFlowDefinitions(readDefinition.Value);
                existingDefinitions.Add(nwrwDryWeatherFlowDefinitionBuilder.Build(readDefinition.Value));
            }

            dryweatherFlowDefinitions = new HashSet<string>(existingDefinitions.Select(definition => definition.Name), StringComparer.InvariantCultureIgnoreCase);
        }

        private Dictionary<string, SobekRRDryWeatherFlow> ReadDryweatherFlowDefinitions(string filePath)
        {
            return new SobekRRDryWeatherFlowReader().Read(filePath).ToDictionaryWithErrorDetails(filePath, item => item.Id, item => item);
        }

        private void AddToSingleUnitDryWeatherFlowDefinitions(SobekRRDryWeatherFlow readDefinition)
        {
            DWAComputationOption readComputationOption = readDefinition.ComputationOption;
            if (readComputationOption == DWAComputationOption.ConstantDWAPerHour || readComputationOption == DWAComputationOption.VariablePerHour)
            {
                singleUnitDryweatherFlowDefinitions.Add(readDefinition.Id);
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
                logHandler.ReportWarning("Could not find any runoff factors.");
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
                logHandler.ReportWarning("No settings found for maximum storages.");
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
                logHandler.ReportWarning("No settings found for maximum infiltration capacities.");
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
                logHandler.ReportWarning("No settings found for minimum infiltration capacities.");
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
                logHandler.ReportWarning("No settings found for infiltration capacity reduction.");
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
                logHandler.ReportWarning("No settings found for infiltration capacity recovery.");
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
        #endregion

        #region Nwrw catchments

        private void FilterNwrwDefinitions(Dictionary<string, SobekRRNwrw> readNwrwDefinitions, Dictionary<string, ILateralSource> lateralSourceDictionary, Dictionary<string, SobekRRNode> readSobekRrNodeDictionary)
        {
            Dictionary<string, SobekRRNode> filteredReadSobekRRNodeDictionary = FilterSobekRRNodes(readSobekRrNodeDictionary);
            
            foreach (SobekRRNwrw readNwrwDefinition in readNwrwDefinitions.Values.ToArray())
            {
                if (!filteredReadSobekRRNodeDictionary.ContainsKey(readNwrwDefinition.Id))
                {
                    logHandler.ReportWarning($"Could not import NWRW catchment, target node or branch '{readNwrwDefinition.Id}' was not found in the network.");
                    readNwrwDefinitions.Remove(readNwrwDefinition.Id);
                    continue;
                }

                if (!lateralSourceDictionary.ContainsKey(readNwrwDefinition.Id))
                {
                    logHandler.ReportWarning($"Could not import NWRW catchment, no lateral was found for the NWRW catchment '{readNwrwDefinition.Id}.");
                    readNwrwDefinitions.Remove(readNwrwDefinition.Id);
                }
            }
        }

        private void AddNwrwDefinitionsToModel(IEnumerable<SobekRRNwrw> readNwrwDefinitions, Dictionary<string, ILateralSource> lateralSourceDictionary, Dictionary<string, SobekRRNode> readSobekRrNodeDictionary)
        {
            Dictionary<string, NwrwData> catchmentModelData = rrModel.GetAllModelData()
                                                                     .OfType<NwrwData>()
                                                                     .ToDictionary(rra => rra.Name, StringComparer.InvariantCultureIgnoreCase);

            var flowFmModel = TryGetModel<WaterFlowFMModel>();
            bool hasNetwork = HydroNetwork != null;

            var lateralDataLookup = flowFmModel?.LateralSourcesData.ToDictionary(d => (ILateralSource) d.Feature);

            var unFoundNodeIds = new List<string>();
            var definitions = readNwrwDefinitions.ToArray();

            var objectDefinitionList = new List<Tuple<SobekRRNwrw, object>>();
            
            foreach (SobekRRNwrw readNwrwDefinition in definitions)
            {
                if (catchmentModelData.TryGetValue(readNwrwDefinition.Id, out var nwrwData))
                {
                    SetNwrwCatchmentData(nwrwData, readNwrwDefinition);
                    continue;
                }

                if (hasNetwork && lateralSourceDictionary.TryGetValue(readNwrwDefinition.Id, out var targetLateralSource) && flowFmModel?.LateralSourcesData != null)
                {
                    if (lateralDataLookup.TryGetValue(targetLateralSource, out var lateralData) && 
                        lateralData.DataType != Model1DLateralDataType.FlowRealTime)
                    {
                        lateralData.DataType = Model1DLateralDataType.FlowRealTime;
                    }

                    objectDefinitionList.Add(new Tuple<SobekRRNwrw, object>(readNwrwDefinition, targetLateralSource));
                    continue;
                }

                if (!readSobekRrNodeDictionary.TryGetValue(readNwrwDefinition.Id, out var rrNode))
                {
                    unFoundNodeIds.Add(readNwrwDefinition.Id);
                    continue;
                }

                objectDefinitionList.Add(new Tuple<SobekRRNwrw, object>(readNwrwDefinition, rrNode));
            }

            AddNwrwCatchmentDataToModel(objectDefinitionList);

            if (unFoundNodeIds.Any())
            {
                Log.Warn($"Could not find the following NWRW id's {string.Join(",", unFoundNodeIds)}");
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
            HashSet<string> networkCompartments = new HashSet<string>();
            if (HydroNetwork != null)
            {
                networkCompartments = new HashSet<string>(HydroNetwork.Compartments.Select(c => c.Name), StringComparer.InvariantCultureIgnoreCase);
            }

            foreach (var readSobekRRNode in readSobekRRNodeDictionary)
            {
                string targetName = readSobekRRNode.Key;
                SobekRRNode sobekRRNode = readSobekRRNode.Value;
                string objectTypeName = sobekRRNode.ObjectTypeName;

                if (sobekRRNode.NodeType != SobekRRNodeType.NWRW || string.IsNullOrWhiteSpace(objectTypeName))
                {
                    continue; // filter out non-nwrw node types
                }

                if (objectTypeName.IndexOf(nodeIdentifier, StringComparison.InvariantCultureIgnoreCase) >= 0) // check if target is node
                {
                    if (nodeDictionary.Contains(targetName)) // on a hydro node?
                    {
                        filteredReadSobekRRNodeDictionary[targetName] = sobekRRNode;
                        continue;
                    }

                    if (branchDictionary.Contains(targetName)) // on a channel?
                    {
                        filteredReadSobekRRNodeDictionary[targetName] = sobekRRNode;
                        continue;
                    }

                    if (networkCompartments.Contains(targetName)) // on a compartment?
                    {
                        filteredReadSobekRRNodeDictionary[targetName] = sobekRRNode;
                    }
                    continue;
                }

                if (objectTypeName.IndexOf(pipeIdentifier, StringComparison.InvariantCultureIgnoreCase) >=0 
                    && branchDictionary.Contains(sobekRRNode.ReachId)) // check if target is branch
                {
                    filteredReadSobekRRNodeDictionary[sobekRRNode.ReachId] = sobekRRNode; // find by branchId
                    filteredReadSobekRRNodeDictionary[targetName] = sobekRRNode;          // find by targetIdentifier
                    continue;
                }

                logHandler.ReportWarning($"Could not import NWRW catchment, target node or branch '{targetName}' was not found in the network.");
            }

            return filteredReadSobekRRNodeDictionary;
        }

        private Dictionary<string, SobekRRNwrw> ReadNwrwDefinitions(string filePath)
        {
            return new SobekRRNwrwReader().Read(filePath).ToDictionaryWithErrorDetails(filePath, item => item.Id, item => item);
        }

        private void AddNwrwCatchmentDataToModel(List<Tuple<SobekRRNwrw, object>> tuples)
        {
            try
            {
                var catchmentModelData = NwrwData.CreateNewNwrwDataAndCatchments(rrModel, tuples
                                                                                          .Select(t => GetIdForObject(t.Item2))
                                                                                          .ToArray());

                var data = tuples.Select(t => new
                {
                    Definition = t.Item1,
                    Object = t.Item2,
                    Id = GetIdForObject(t.Item2),
                    catchmentData = catchmentModelData[GetIdForObject(t.Item2)]
                }).ToArray();

                foreach (var dataObject in data)
                {
                    dataObject.catchmentData.NodeOrBranchId = dataObject.Id;

                    Catchment catchment = dataObject.catchmentData.Catchment;
                    catchment.IsGeometryDerivedFromAreaSize = true;
                
                    if (dataObject.Object is LateralSource lateralSource)
                    {
                        catchment.Geometry = lateralSource.Geometry;
                        catchment.LinkTo(lateralSource);
                    }
                    else if (dataObject.Object is SobekRRNode rrNode)
                    {
                        catchment.Geometry = new Point(rrNode.X, rrNode.Y);
                    }

                    SetNwrwCatchmentData(dataObject.catchmentData, dataObject.Definition);
                }
            }
            catch (ArgumentNullException exception)
            {
                Log.Error(exception.Message);
            }
        }

        private static string GetIdForObject(object item)
        {
            switch (item)
            {
                case LateralSource lateralSource:
                    return lateralSource.Name;
                case SobekRRNode rrNode:
                    return rrNode.Id;
                default:
                    throw new NotSupportedException();
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
                if (!dryweatherFlowDefinitions.Contains(dwaId))
                {
                    logHandler.ReportWarning($"Could not add dryweather flow definition {dwaId} to nwrw catchment, because it is not defined.");
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