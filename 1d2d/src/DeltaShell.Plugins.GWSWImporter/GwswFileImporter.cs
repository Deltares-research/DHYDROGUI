﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using DelftTools.Hydro;
using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.CrossSections.StandardShapes;
using DelftTools.Hydro.Helpers;
using DelftTools.Hydro.SewerFeatures;
using DelftTools.Hydro.Structures;
using DelftTools.Hydro.Structures.WeirFormula;
using DelftTools.Shell.Core;
using DelftTools.Shell.Core.Extensions;
using DelftTools.Shell.Core.Workflow;
using DelftTools.Utils;
using DelftTools.Utils.Collections.Generic;
using DelftTools.Utils.Csv.Importer;
using Deltares.Infrastructure.API.Logging;
using DeltaShell.NGHS.Common;
using DeltaShell.NGHS.IO.DataObjects;
using DeltaShell.NGHS.Utils;
using DeltaShell.NGHS.Utils.Extensions;
using DeltaShell.Plugins.DelftModels.HydroModel;
using DeltaShell.Plugins.DelftModels.RainfallRunoff;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Concepts.Nwrw;
using DeltaShell.Plugins.FMSuite.FlowFM;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using DeltaShell.Plugins.ImportExport.GWSW.Logging;
using DeltaShell.Plugins.ImportExport.GWSW.Properties;
using DeltaShell.Plugins.ImportExport.GWSW.SewerFeatures;
using GeoAPI.Extensions.Coverages;
using GeoAPI.Extensions.Networks;
using GeoAPI.Geometries;
using log4net;
using NetTopologySuite.Extensions.Coverages;
using EnumerableExtensions = DelftTools.Utils.Collections.EnumerableExtensions;

namespace DeltaShell.Plugins.ImportExport.GWSW
{
    /// <summary>
    /// Importer for GWSW files
    /// </summary>
    /// <seealso cref="DelftTools.Shell.Core.IFileImporter"/>
    public class GwswFileImporter : IFileImporter
    {
        private readonly IDefinitionsProvider definitionsProvider;
        private static ILog Log = LogManager.GetLogger(typeof(GwswFileImporter));
        private readonly ILogHandler logHandler;

        private CsvSettings csvSettings;
        public IActivityRunner ActivityRunner { get; set; }

        public GwswFileImporter(IDefinitionsProvider definitionsProvider)
        {
            this.definitionsProvider = definitionsProvider ?? throw new ArgumentNullException(nameof(definitionsProvider));

            FilesToImport = new List<string>();
            GwswAttributesDefinition = new EventedList<GwswAttributeType>();
            GwswDefaultFeatures = new Dictionary<string, List<string>>();
            CsvDelimeter = ';'; //Default value, can be changed.
            logHandler = new ConcurrentLogHandler("Gwsw import", Log);
            this.definitionsProvider.LogHandler = logHandler;
        }

        /// <summary>
        /// Imports the given file as path. If it is null, then the list of files (FilesToImport) will be imported instead.
        /// A Gwsw Definition file needs to be loaded beforehand with method LoadDefinitionFile.
        /// By default, all files referenced in the GwswDefinitionFile are selected to import.
        /// </summary>
        /// <param name="path">File to import. If this argument is missing then FilesToImport will be taken instead.</param>
        /// <param name="target"></param>
        /// <returns></returns>
        public object ImportItem(string path, object target = null)
        {
            if (ActivityRunner == null)
            {
                ActivityRunner = new ActivityRunner();
                ActivityRunner.Activities.Add(new FileImportActivity(this));
            }

            var watch = new Stopwatch();
            watch.Start();

            if (GwswAttributesDefinition == null || !GwswAttributesDefinition.Any())
            {
                Log.ErrorFormat(Resources.GwswFileImporter_ImportItem_No_mapping_was_found_to_import_Gwsw_Files_);
                return null;
            }

            if (!string.IsNullOrEmpty(path))
            {
                FilesToImport = new EventedList<string> { path };
            }

            if (FilesToImport == null || FilesToImport.Count == 0)
            {
                return null;
            }

            Log.Info(Resources.GwswFileImporterBase_ImportFilesFromDefinitionFile_Importing_sub_files_);
            if (ShouldCancel)
            {
                return null;
            }

            HydroModel hydroModel = null;
            WaterFlowFMModel fmModel = null;
            RainfallRunoffModel rrModel = null;
            try
            {
                ILookup<SewerFeatureType, GwswElement> elementTypesList = ImportGwswElementsFromGwswFiles();

                hydroModel = target is Project || target == null
                                 ? new HydroModelBuilder().BuildModel(ModelGroup.RHUModels)
                                 : target as HydroModel;

                fmModel = hydroModel?.GetAllActivitiesRecursive<WaterFlowFMModel>()?.FirstOrDefault() ??
                          target as WaterFlowFMModel;
                if (fmModel != null)
                {
                    ImportGwswNetworkInFmModel(elementTypesList, fmModel);
                }

                rrModel = hydroModel?.GetAllActivitiesRecursive<RainfallRunoffModel>()?.FirstOrDefault() ??
                          target as RainfallRunoffModel;
                if (rrModel != null && fmModel?.Network != null)
                {
                    ImportGwswNetworkInRrModel(elementTypesList, rrModel, fmModel);
                    if (hydroModel != null)
                    {
                        AddRRtoFMNwrwLinks(rrModel, fmModel);
                    }
                }

                if (hydroModel != null)
                {
                    SetCurrentWorkflow(fmModel, rrModel, hydroModel);
                    hydroModel.CoordinateSystem = fmModel?.CoordinateSystem;
                    SetDefaultModelSettings(fmModel, hydroModel);
                }
            }
            catch (Exception exception)
            {
                Log.Error($"GWSW import failed : {exception.Message}");
            }

            watch.Stop();
            Log.Info($"Done importing and generating model in {watch.ElapsedMilliseconds / 1000} sec");
            logHandler?.LogReport();
            ProgressChanged?.Invoke($"Done importing and generating model in {watch.ElapsedMilliseconds / 1000} sec from gwsw files, loading into DeltaShell", 10, 10);
            return (target is Project || target == null) && !ShouldCancel ? hydroModel : fmModel != null ? (object)fmModel : rrModel != null ? rrModel : null;
        }

        private void SetDefaultModelSettings(IWaterFlowFMModel fmModel, HydroModel hydroModel)
        {
            var timeStep = new TimeSpan(0, 1, 0);
            hydroModel.TimeStep = timeStep;
            double timeStepInSeconds = timeStep.TotalSeconds;

            var timeStepValue = timeStepInSeconds.ToString(CultureInfo.InvariantCulture);
            fmModel.ModelDefinition.SetModelProperty(KnownProperties.DtUser, timeStepValue);
            fmModel.ModelDefinition.SetModelProperty(GuiProperties.HisOutputDeltaT, timeStepValue);
            fmModel.ModelDefinition.SetModelProperty(GuiProperties.MapOutputDeltaT, timeStepValue);
        }

        /// <summary>
        /// Sets the CurrentWorkFlow property of the HydroModel.
        /// </summary>
        /// <param name="fmModel"></param>
        /// <param name="rrModel"></param>
        /// <param name="hydroModel"></param>
        private void SetCurrentWorkflow(WaterFlowFMModel fmModel, RainfallRunoffModel rrModel, HydroModel hydroModel)
        {
            if (fmModel != null && rrModel == null)
            {
                IEnumerable<ICompositeActivity> wfs = hydroModel.Workflows.Where(w => w.GetAllActivitiesRecursive<IActivity>().Count() == 2);
                ICompositeActivity hydroModelCurrentWorkflow =
                    wfs.FirstOrDefault(wf => wf.GetActivitiesOfType<IWaterFlowFMModel>().Any());
                if (hydroModelCurrentWorkflow != null)
                {
                    hydroModel.CurrentWorkflow = hydroModelCurrentWorkflow;
                }
            }
            else if (fmModel != null && rrModel != null)
            {
                IEnumerable<ICompositeActivity> wfs = hydroModel.Workflows.Where(w => w.GetAllActivitiesRecursive<IActivity>().Count() == 3);
                ICompositeActivity hydroModelCurrentWorkflow = wfs.FirstOrDefault(wf =>
                                                                                      wf.GetActivitiesOfType<IWaterFlowFMModel>().Any() &&
                                                                                      wf.GetActivitiesOfType<RainfallRunoffModel>().Any());
                if (hydroModelCurrentWorkflow != null)
                {
                    hydroModel.CurrentWorkflow = hydroModelCurrentWorkflow;
                }
            }
            else if (fmModel == null && rrModel != null)
            {
                IEnumerable<ICompositeActivity> wfs = hydroModel.Workflows.Where(w => w.GetAllActivitiesRecursive<IActivity>().Count() == 2);
                ICompositeActivity hydroModelCurrentWorkflow =
                    wfs.FirstOrDefault(wf => wf.GetActivitiesOfType<RainfallRunoffModel>().Any());
                if (hydroModelCurrentWorkflow != null)
                {
                    hydroModel.CurrentWorkflow = hydroModelCurrentWorkflow;
                }
            }
        }

        private void AddRRtoFMNwrwLinks(RainfallRunoffModel rrModel, WaterFlowFMModel fmModel)
        {
            if (rrModel == null)
            {
                throw new ArgumentNullException(nameof(rrModel));
            }

            if (fmModel == null)
            {
                throw new ArgumentNullException(nameof(fmModel));
            }

            IHydroNetwork network = fmModel.Network;

            if (network == null)
            {
                Log.Error("Could not add NWRW links between FM model and RR model");
                return;
            }

            network.BeginEdit("Importing GWSW database.");
            fmModel.UnSubscribeFromNetwork(network);

            Dictionary<string, IBranch> branchesByName = network.Branches.ToDictionary(b => b.Name, b => b, StringComparer.OrdinalIgnoreCase);
            ILookup<string, ISewerConnection> sewerConnectionsBySourceCompartmentName = network.SewerConnections.ToLookup(sc => sc.SourceCompartmentName, sc => sc, StringComparer.OrdinalIgnoreCase);
            ILookup<string, ISewerConnection> sewerConnectionsByTargetCompartmentName = network.SewerConnections.ToLookup(sc => sc.TargetCompartmentName, sc => sc, StringComparer.OrdinalIgnoreCase);
            Dictionary<string, IBranch> sewerConnectionsAsBranchesByCompartmentName = sewerConnectionsBySourceCompartmentName.Concat(sewerConnectionsByTargetCompartmentName)
                                                                                                                             .GroupBy(e => e.Key)
                                                                                                                             .ToDictionary(l => l.Key, l => l.First().First() as IBranch);
            var lateralSources = new ConcurrentQueue<LateralSource>();
            var lateralSourcesDataByLaterSource = new ConcurrentDictionary<LateralSource, Model1DLateralSourceData>();
            var linksOfNwrwDataToLateralSource = new ConcurrentDictionary<NwrwData, LateralSource>();
            try
            {
                ProgressChanged?.Invoke("Adding links from Rainfall Runoff Model to FM model.", 0, 0);
                ParallelHelper.RunActionInParallel(this, rrModel.GetAllModelData().OfType<NwrwData>().ToArray(),
                                                   nwrwData =>
                                                   {
                                                       try
                                                       {
                                                           IBranch branch = FindTargetBranchForNwrwCatchmentBranch(branchesByName,
                                                                                                                   sewerConnectionsAsBranchesByCompartmentName, nwrwData.Name);

                                                           if (branch != null)
                                                           {
                                                               var lateralSource = new LateralSource
                                                               {
                                                                   Branch = branch,
                                                                   Chainage = branch.Length,
                                                                   Name = nwrwData.Name,
                                                                   LongName = nwrwData.Name
                                                               };
                                                               lateralSources.Enqueue(lateralSource);
                                                               AddLateralSourceToBranch(branch, lateralSource);

                                                               var model1DLateralSourceData = new Model1DLateralSourceData
                                                               {
                                                                   Feature = lateralSource,
                                                                   UseSalt = fmModel.UseSalinity,
                                                                   UseTemperature = fmModel.UseTemperature
                                                               };
                                                               if (lateralSource.Branch is ISewerConnection sewerConnection)
                                                               {
                                                                   model1DLateralSourceData.Compartment = GetLateralSourceDataCompartment(sewerConnection, lateralSource);
                                                               }

                                                               lateralSourcesDataByLaterSource.AddOrUpdate(lateralSource, model1DLateralSourceData,
                                                                                                           (ls, m1lsd) => model1DLateralSourceData);

                                                               lock (fmModel.LateralSourcesData)
                                                               {
                                                                   fmModel.LateralSourcesData.Add(model1DLateralSourceData);
                                                               }

                                                               linksOfNwrwDataToLateralSource.AddOrUpdate(nwrwData, lateralSource, (ls, nwrw) => nwrw);
                                                           }
                                                       }
                                                       catch (Exception e)
                                                       {
                                                           logHandler?.ReportError(e.Message);
                                                       }
                                                   }, "Generating Hydrolinks");
                while (!network.LateralSources.Select(ls => ls.Name).AllUnique())
                {
                    NamingHelper.MakeNamesUnique(network.LateralSources);
                }

                ParallelHelper.RunActionInParallel(this, linksOfNwrwDataToLateralSource.ToArray(), link => AddHydroLinkToCatchment(link.Key, link.Value), "Adding Hydrolinks");
                // at FM-side, create lateral data of type REALTIME
                ParallelHelper.RunActionInParallel(this, lateralSources.ToArray(), ls => AddLateralDataToFmModel(lateralSourcesDataByLaterSource, ls, Model1DLateralDataType.FlowRealTime, default(double)), "At FM-side, create lateral data of type REALTIME");
            }
            finally
            {
                fmModel.SubscribeToNetwork(network);
                network.EndEdit();
            }
        }

        /// <summary>
        /// Adds the specified Model1DLateralDataType and Flow
        /// to the Model1DLateralSourceData.
        /// </summary>
        /// <param name="lateralSourcesDataByFeature"></param>
        /// <param name="lateralSource"></param>
        /// <param name="model1DBoundaryDataType"></param>
        /// <param name="flow"></param>
        private void AddLateralDataToFmModel(
            ConcurrentDictionary<LateralSource, Model1DLateralSourceData> lateralSourcesDataByFeature,
            LateralSource lateralSource,
            Model1DLateralDataType model1DBoundaryDataType,
            double flow)
        {
            if (!lateralSourcesDataByFeature.TryGetValue(lateralSource, out Model1DLateralSourceData model1DLateralSourceData))
            {
                logHandler?.ReportError($"Cannot find lateral source data generated for lateral source: {lateralSource}");
                return;
            }

            model1DLateralSourceData.DataType = model1DBoundaryDataType;
            model1DLateralSourceData.Flow = flow;
        }

        /// <summary>
        /// Creates a link between the NwrwData and the LateralSource.
        /// </summary>
        /// <param name="nwrwData"></param>
        /// <param name="lateralSource"></param>
        private void AddHydroLinkToCatchment(NwrwData nwrwData, LateralSource lateralSource)
        {
            nwrwData?.Catchment?.LinkTo(lateralSource);
        }

        /// <summary>
        /// Adds a LateralSource as a BranchFeature to a branch.
        /// </summary>
        /// <param name="branch"></param>
        /// <param name="name"></param>
        /// <param name="lateralSource"></param>
        private void AddLateralSourceToBranch(IBranch branch, LateralSource lateralSource)
        {
            if (branch is ISewerConnection sewerConnection && sewerConnection.Target is IManhole manhole && manhole.Name.Equals(lateralSource.Name, StringComparison.InvariantCultureIgnoreCase))
            {
                lateralSource.Geometry = HydroNetworkHelper.GetStructureGeometry(branch, branch.Length);
            }
            else
            {
                lateralSource.Geometry = HydroNetworkHelper.GetStructureGeometry(branch, 0);
            }

            lock (branch.BranchFeatures)
            {
                branch.BranchFeatures.Add(lateralSource);
            }
        }

        private void ImportGwswNetworkInRrModel(
            ILookup<SewerFeatureType, GwswElement> elementTypesList, RainfallRunoffModel rrModel, WaterFlowFMModel fmModel)
        {
            var errorsDuringImport = new List<string>();
            var importedFeatureElements = SewerFeatureFactory.CreateNwrwEntities(elementTypesList, this, errorsDuringImport);
            if (errorsDuringImport.Any())
            {
                Log.Error(
                    $"One or more errors occured during the import process: {string.Join(Environment.NewLine, errorsDuringImport)}");
            }

            if (rrModel == null || fmModel == null || ShouldCancel)
            {
                return;
            }

            ProgressChanged?.Invoke("Adding features to Rainfall Runoff Model.", 0, 0);
            AddNwrwFeaturesToRainfallRunoffModel(importedFeatureElements, rrModel, fmModel);
        }

        private void AddNwrwFeaturesToRainfallRunoffModel(IEnumerable<INwrwFeature> importedFeatureElements,
                                                          RainfallRunoffModel rrModel, WaterFlowFMModel fmModel)
        {
            INwrwFeature[] featureElements = importedFeatureElements.ToArray();
            Dictionary<string, IEnumerable<INwrwFeature>> featureElementsByName = featureElements.ToGroupedDictionary(e => e.Name);
            IHydroNetwork network = fmModel.Network;

            ILookup<string, IGeometry> branchesGeometryDict = network.Branches.ToLookup(b => b.Name, b => b.Target?.Geometry);
            var compartmentsGeometryDict = network.Compartments.ToLookup(c => c.Name, c => c.Geometry);
            var networkFeatureNameAndGeometries = branchesGeometryDict.Concat(compartmentsGeometryDict)
                                                                      .ToDictionary(nameGeometryLookup => nameGeometryLookup.Key, nameGeometryLookup => nameGeometryLookup.FirstOrDefault(), StringComparer.InvariantCultureIgnoreCase);
            Dictionary<string, IBranch> branchesByName = network.Branches.ToDictionary(b => b.Name, b => b, StringComparer.OrdinalIgnoreCase);
            ILookup<string, ISewerConnection> sewerConnectionsBySourceCompartmentName = network.SewerConnections.ToLookup(sc => sc.SourceCompartmentName, sc => sc, StringComparer.OrdinalIgnoreCase);
            ILookup<string, ISewerConnection> sewerConnectionsByTargetCompartmentName = network.SewerConnections.ToLookup(sc => sc.TargetCompartmentName, sc => sc, StringComparer.OrdinalIgnoreCase);
            Dictionary<string, IBranch> sewerConnetionsAsBranchesByCompartmentName = sewerConnectionsBySourceCompartmentName.Concat(sewerConnectionsByTargetCompartmentName)
                                                                                                                            .GroupBy(e => e.Key)
                                                                                                                            .ToDictionary(l => l.Key, l => l.First().First() as IBranch);
            var flowByLateralSources = new ConcurrentDictionary<LateralSource, double>();
            var helper = new NwrwImporterHelper();
            helper.CurrentNwrwCatchmentModelDataByNodeOrBranchId = new ConcurrentDictionary<string, NwrwData>(rrModel.GetAllModelData().OfType<NwrwData>().ToDictionary(md => md.Name, md => md, StringComparer.InvariantCultureIgnoreCase));
            var lateralSourcesDataByLaterSource = new ConcurrentDictionary<LateralSource, Model1DLateralSourceData>();

            fmModel.UnSubscribeFromNetwork(network);
            try
            {
                ParallelHelper.RunActionInParallel(this, featureElements, featureElement =>
                {
                    try
                    {
                        if (featureElement is NwrwDefinition ||
                            featureElement is NwrwDryWeatherFlowDefinition ||
                            (featureElement.Name != null &&
                             networkFeatureNameAndGeometries.ContainsKey(featureElement.Name)))
                        {
                            if (featureElement.Name != null &&
                                networkFeatureNameAndGeometries.ContainsKey(featureElement.Name))
                            {
                                featureElement.Geometry = networkFeatureNameAndGeometries[featureElement.Name];
                            }

                            lock (rrModel.Basin.Catchments)
                            {
                                featureElement.AddNwrwCatchmentModelDataToModel(rrModel, helper, logHandler);
                            }
                        }
                    }
                    catch (Exception exception)
                    {
                        logHandler?.ReportError(exception.Message);
                    }
                }, "Add nwrw feature to rainfall runoff model");
                ParallelHelper.RunActionInParallel(this, rrModel.GetAllModelData().OfType<NwrwData>().ToArray(), nwrwData =>
                {
                    EnumerableExtensions.ForEach(featureElementsByName[nwrwData.Name], (featureElement, indexOf) =>
                    {
                        if (ShouldCancel)
                        {
                            return;
                        }

                        try
                        {
                            featureElement.InitializeNwrwCatchmentModelData(nwrwData);
                        }
                        catch (Exception exception)
                        {
                            logHandler?.ReportError(exception.Message);
                        }
                    });
                }, "Initialize nwrw feature in rainfall runoff model");

                var nwrwDryWeatherFlowDefinitionByName = rrModel.NwrwDryWeatherFlowDefinitions.ToLookup(dwfd => dwfd.Name, dwfd => dwfd);

                ParallelHelper.RunActionInParallel(this, featureElements.OfType<NwrwDischargeData>().Where(nwrwDischargeData => !nwrwDischargeData.IsSpecialCase() && nwrwDischargeData.DischargeType == DischargeType.Lateral).ToArray(), nwrwDischargeData =>
                {
                    try
                    {
                        IBranch branch = FindTargetBranchForNwrwCatchmentBranch(branchesByName,
                                                                                sewerConnetionsAsBranchesByCompartmentName, nwrwDischargeData.Name);
                        if (branch != null)
                        {
                            var lateralSource = new LateralSource
                            {
                                Branch = branch,
                                Chainage = branch.Length,
                                Name = "lat_" + nwrwDischargeData.Name,
                                LongName = Name
                            };

                            AddLateralSourceToBranch(branch, lateralSource);
                            double calculateLateralFlow = nwrwDischargeData.CalculateLateralFlow(nwrwDryWeatherFlowDefinitionByName, logHandler);
                            if (!double.IsNaN(calculateLateralFlow))
                            {
                                flowByLateralSources.AddOrUpdate(lateralSource, calculateLateralFlow, (ls, oldSurfaceValue) => oldSurfaceValue);
                            }

                            var model1DLateralSourceData = new Model1DLateralSourceData
                            {
                                Feature = lateralSource,
                                UseSalt = fmModel.UseSalinity,
                                UseTemperature = fmModel.UseTemperature
                            };
                            if (lateralSource.Branch is ISewerConnection sewerConnection)
                            {
                                model1DLateralSourceData.Compartment = GetLateralSourceDataCompartment(sewerConnection, lateralSource);
                            }

                            lateralSourcesDataByLaterSource[lateralSource] = model1DLateralSourceData;

                            lock (fmModel.LateralSourcesData)
                            {
                                fmModel.LateralSourcesData.Add(model1DLateralSourceData);
                            }
                        }
                    }
                    catch (Exception exception)
                    {
                        logHandler?.ReportError(exception.Message);
                    }
                }, "Add nwrw lateral to fm model");

                // at FM-side, create lateral data of type CONSTANT
                ParallelHelper.RunActionInParallel(this, flowByLateralSources.ToArray(), flowByLs => AddLateralDataToFmModel(lateralSourcesDataByLaterSource, flowByLs.Key, Model1DLateralDataType.FlowConstant, flowByLs.Value), "At FM - side, create lateral data of type CONSTANT");
            }
            finally
            {
                fmModel.SubscribeToNetwork(network);
            }
        }

        private static ICompartment GetLateralSourceDataCompartment(ISewerConnection sewerConnection, LateralSource lateralSource)
        {
            return sewerConnection.SourceCompartmentName != null &&
                   sewerConnection.SourceCompartmentName.Equals(lateralSource.Name)
                       ? sewerConnection.SourceCompartment
                       : sewerConnection.TargetCompartment;
        }

        private IBranch FindTargetBranchForNwrwCatchmentBranch(Dictionary<string, IBranch> branchesByName, Dictionary<string, IBranch> pipesByCompartmentName, string name)
        {
            if (branchesByName == null)
            {
                logHandler?.ReportError($"Cannot find target branch because no branches are loaded ({nameof(branchesByName)} is null)");
                return null;
            }

            if (pipesByCompartmentName == null)
            {
                logHandler?.ReportError($"Cannot find target branch because no pipes are loaded ({nameof(pipesByCompartmentName)} is null)");
                return null;
            }

            if (string.IsNullOrWhiteSpace(name))
            {
                logHandler?.ReportError($"Cannot find target branch because branch name to search for is null or empty");
                return null;
            }

            IBranch branch = null;
            if (branchesByName.TryGetValue(name, out branch))
            {
                return branch;
            }

            if (!pipesByCompartmentName.TryGetValue(name, out branch))
            {
                logHandler?.ReportWarning($"Cannot find branch for {name}");
            }

            return branch;
        }

        private void ImportGwswNetworkInFmModel(ILookup<SewerFeatureType, GwswElement> elementTypesList, WaterFlowFMModel fmModel)
        {
            if (fmModel is null)
            {
                return;
            }

            IHydroNetwork network = fmModel.Network;

            network.BeginEdit("Importing GWSW database.");
            fmModel.DisableNetworkSynchronization = true;
            try
            {
                var importedFeatureElements = SewerFeatureFactory.CreateSewerEntities(elementTypesList, this).ToArray();
                if (network != null && !ShouldCancel)
                {
                    ProgressChanged?.Invoke("Adding features to network.", 0, 0);
                    AddSewerFeaturesToNetwork(importedFeatureElements, network);

                    ProgressChanged?.Invoke("Adding discretisation points of sewerconnection to networkdiscretisation.", 0, 0);
                    AddDiscretisationPointsOfSewerConnections(network, fmModel?.NetworkDiscretization);

                    EventingHelper.DoWithEvents(() =>
                    {
                        ProgressChanged?.Invoke("Adding roughness sections of sewer connections to roughness 1d list.", 4, 10);
                    });

                    fmModel.UpdateRoughnessSections();

                    ProgressChanged?.Invoke("Adding model1d lateral source of sewer connections to lateral source (data) list.", 0, 0);
                    AddModel1DLateralSourceToFmModel(network, fmModel);

                    ProgressChanged?.Invoke("Adding model1d boundary nodes of manholes to boundary data list.", 0, 0);
                    AddModel1DBoundaryNodesToFmModel(network, fmModel);

                    ProgressChanged?.Invoke("Add Boundaries Of Network Outlet Compartments To ModelDefinition.", 0, 0);
                    AddBoundariesOfNetworkOutletCompartmentsToModelDefinition(fmModel);
                }
            }
            finally
            {
                fmModel.DisableNetworkSynchronization = false;
                network?.EndEdit();
            }
        }

        private void AddModel1DLateralSourceToFmModel(IHydroNetwork network, WaterFlowFMModel fmModel)
        {
            LateralSource[] lateralSources = network.Channels.SelectMany(c => c.BranchSources).ToArray();

            ParallelHelper.RunActionInParallel(this, lateralSources,
                                               lateralSource =>
                                               {
                                                   if (fmModel.LateralSourcesData.Any(lsd => lsd.Feature == lateralSource))
                                                   {
                                                       return;
                                                   }

                                                   var model1DLateralSourceData = new Model1DLateralSourceData
                                                   {
                                                       Feature = lateralSource,
                                                       UseSalt = fmModel.UseSalinity,
                                                       UseTemperature = fmModel.UseTemperature
                                                   };
                                                   lock (fmModel.LateralSourcesData)
                                                   {
                                                       fmModel.LateralSourcesData.Add(model1DLateralSourceData);
                                                   }
                                               }, "adding model1d lateral sources to fm model");
        }

        private void AddModel1DBoundaryNodesToFmModel(IHydroNetwork network, WaterFlowFMModel fmModel)
        {
            IManhole[] networkManholes = network.Manholes.ToArray();
            var listOfErrors = new List<string>();

            EventingHelper.DoWithoutEvents(() =>
            {
                ParallelHelper.RunActionInParallel(this, networkManholes, manhole =>
                {
                    Model1DBoundaryNodeData bc = Helper1D.CreateDefaultBoundaryCondition(manhole, false, false);
                    bc.SetBoundaryConditionDataForOutlet();
                    lock (fmModel.BoundaryConditions1D)
                    {
                        fmModel.BoundaryConditions1D.Add(bc);
                    }
                }, "Add Model 1D Boundary Nodes to Fm Model");

                if (listOfErrors.Any())
                {
                    Log.ErrorFormat(
                        $"While adding model1d boundary data nodes to fm model we encountered the following errors: {Environment.NewLine}{string.Join(Environment.NewLine, listOfErrors)}");
                }
            });
        }

        private void AddDiscretisationPointsOfSewerConnections(IHydroNetwork network, IDiscretization networkDiscretization)
        {
            while (!network.Branches.Select(ls => ls.Name).AllUnique())
            {
                NamingHelper.MakeNamesUnique(network.Branches);
            }

            ISewerConnection[] networkSewerConnections = network.SewerConnections.ToArray();
            int nrOfImportedFeatureElements = networkSewerConnections.Length;
            int stepSize = nrOfImportedFeatureElements / 20;
            var listOfErrors = new List<string>();
            var newLocations = new List<NetworkLocation>();

            EnumerableExtensions.ForEach(networkSewerConnections, (sewerConnection, indexOf) =>
            {
                try
                {
                    if (ShouldCancel)
                    {
                        return;
                    }

                    if (stepSize != 0 && indexOf % stepSize == 0)
                    {
                        EventingHelper.DoWithEvents(() =>
                        {
                            ProgressChanged?.Invoke($"Adding network discretizations points of sewer connection to model ({((double)indexOf / (double)nrOfImportedFeatureElements):P0})", indexOf, nrOfImportedFeatureElements);
                        });
                    }

                    // add location for begin and end of the sewer connection
                    newLocations.Add(new NetworkLocation(sewerConnection, 0.0));

                    if (sewerConnection.Length > 0)
                    {
                        newLocations.Add(new NetworkLocation(sewerConnection, sewerConnection.Length));
                    }
                }
                catch (Exception exception)
                {
                    listOfErrors.Add(exception.Message + Environment.NewLine);
                }
            });
            networkDiscretization.UpdateNetworkLocations(newLocations);

            if (listOfErrors.Any())
            {
                Log.ErrorFormat(
                    $"While adding discretisation points to network discretisation we encountered the following errors: {Environment.NewLine}{string.Join(Environment.NewLine, listOfErrors)}");
            }
        }

        private void AddSewerFeaturesToNetwork(IEnumerable<ISewerFeature> importedFeatureElements,
                                               IHydroNetwork network)
        {
            var helper = new SewerImporterHelper();
            ParallelHelper.RunActionInParallel(this, importedFeatureElements.ToArray(), feature => feature.AddToHydroNetwork(network, helper), "Network");
            ParallelHelper.RunActionInParallel(this, helper.SewerConnectionsByName.Values.Where(sc => sc.BranchFeatures.Count > 0).ToArray(), sewerConnection => sewerConnection.UpdateBranchFeatureGeometries(), "");
            ParallelHelper.RunActionInParallel(this, network.SewerConnections.Where(sc => sc.Geometry == null).ToArray(), sc => network.FindAndConnectManholesInNetwork(sc), "Update empty geometries");
            ParallelHelper.RunActionInParallel(this, helper.SewerConnectionsByName.Values.Where(sc => Math.Abs(sc.Length) < 1.0e-6).ToArray(), sewerConnection => sewerConnection.SetLengthOfConnectionBasedOnConnectedCompartmentsOrSetAFake(), "Update length of sewer connections");
            ParallelHelper.RunActionInParallel(this, network.SewerConnections.ToArray(), sewerConnection =>
            {
                if (string.IsNullOrWhiteSpace(sewerConnection.CrossSectionDefinitionName))
                {
                    // use default!
                    if (sewerConnection is IPipe pipe)
                    {
                        logHandler.ReportWarningFormat(Resources.GwswFileImporter_AddSewerFeaturesToNetwork_No_cross_section_id_defined_in_Verbinding_csv_for_pipe__0___Using_default_pipe_profile, pipe.PipeId);
                    }
                    else
                    {
                        logHandler.ReportWarningFormat(Resources.GwswFileImporter_AddSewerFeaturesToNetwork_No_cross_section_id_defined_in_Verbinding_csv_for_sewer_connection__0___Using_default_sewer_profile, sewerConnection.Name);
                    }

                    sewerConnection.GenerateDefaultProfileForSewerConnections();

                    helper.PipeCrossSections?.Enqueue(sewerConnection.CrossSection);
                }
                else if (helper.CrossSectionDefinitionsByPipe.TryGetValue(sewerConnection.CrossSectionDefinitionName, out CrossSectionDefinitionProxy crossSectionDefinition))
                {
                    ICrossSection crossSection = CrossSection.CreateDefault(CrossSectionType.Standard, sewerConnection, sewerConnection.Length / 2, false);
                    crossSection.Name = $"SewerProfile_";
                    crossSection.UseSharedDefinition(crossSectionDefinition);
                    sewerConnection.CrossSection = crossSection;
                    helper.PipeCrossSections?.Enqueue(crossSection);
                }
                
            }, "Update sewer connections profiles in network");

            ParallelHelper.RunActionInParallel(this, network.Pipes.ToArray(), pipe =>
            {
                if (helper.SewerProfileMaterialsByPipe.TryGetValue(pipe.CrossSectionDefinitionName, out SewerProfileMapping.SewerProfileMaterial material))
                {
                    pipe.Material = material;
                }
            }, "Update pipe material in network");

            ParallelHelper.RunActionInParallel(this, network.Orifices.OfType<GwswConnectionOrifice>().Cast<GwswConnectionOrifice>().ToArray(), (orifice) =>
            {
                try
                {
                    ICrossSectionDefinition definition = network.GetNetworkCrossSectionDefinitions().SingleOrDefault(csd => string.Equals(csd.Name, orifice.CrossSectionDefinitionName, StringComparison.InvariantCultureIgnoreCase))
                                                         ?? network.SharedCrossSectionDefinitions.SingleOrDefault(scsd => string.Equals(scsd.Name, orifice.CrossSectionDefinitionName, StringComparison.InvariantCultureIgnoreCase));
                    if (definition == null)
                    {
                        return;
                    }

                    var formula = (GatedWeirFormula)orifice.WeirFormula;

                    if (definition.CrossSectionType == CrossSectionType.Standard && definition is CrossSectionDefinitionStandard csdefStandard)
                    {
                        switch (csdefStandard.Shape)
                        {
                            case CrossSectionStandardShapeWidthHeightBase rectangle:
                                formula.GateOpening = rectangle.Height;
                                orifice.CrestWidth = rectangle.Width;
                                break;
                            case CrossSectionStandardShapeCircle circle:
                            {
                                double diameter = circle.Diameter;
                                double crestWidth = Math.Sqrt(((diameter * diameter) / 4) * Math.PI);
                                orifice.CrestWidth = crestWidth;
                                formula.GateOpening = crestWidth;
                                formula.LowerEdgeLevel = orifice.CrestLevel + orifice.CrestWidth;
                                break;
                            }
                            default:
                                logHandler?.ReportWarning($"Shape '{csdefStandard.Shape}' is not fully supported for orifice '{orifice.Name}'. Setting the gate opening to the highest point of the profile definition and the crest width to the width of the profile definition.");
                                formula.GateOpening = definition.HighestPoint;
                                orifice.CrestWidth = definition.Width;
                                break;
                        }
                    }
                    else
                    {
                        logHandler?.ReportWarning($"Orifice '{orifice.Name}' has a non-standard cross section type. Setting the gate opening to the highest point of the profile definition and the crest width to the width of the profile definition.");
                        formula.GateOpening = definition.HighestPoint;
                        orifice.CrestWidth = definition.Width;
                    }
                }
                catch
                {
                    logHandler?.ReportWarning($"Could not update the orifice crest width or level for '{orifice.Name}'.");
                }
            }, "Update orifices width and level");

            ParallelHelper.RunActionInParallel(this, network.SewerConnections.ToArray(), connection =>
            {
                if (connection.Source == null)
                {
                    lock (network.SewerConnections)
                    {
                        logHandler?.ReportWarning($"Could not find source node for connection '{connection.Name}'. Removing it from the model.");
                        network.Branches.Remove(connection);
                    }
                }

                if (connection.Target == null)
                {
                    logHandler?.ReportWarning($"Could not find target node for connection '{connection.Name}'. Removing it from the model.");
                    network.Branches.Remove(connection);
                }
            }, "Remove unconnected sewer connections.");

            NamingHelper.MakeNamesUnique(helper.PipeCrossSections);
            NamingHelper.MakeNamesUnique(helper.CompositeBranchStructures);
        }

        /// <summary>
        /// Loads the feature files from a directory.
        /// </summary>
        /// <param name="directoryPath">The directory path.</param>
        public void LoadFeatureFiles(string directoryPath)
        {
            if (string.IsNullOrWhiteSpace(directoryPath))
            {
                return;
            }

            GwswAttributesDefinition = definitionsProvider.GetDefinitions(directoryPath);
            try
            {
                GwswDefaultFeatures = CreateFileNameToViewDataDictionary(directoryPath);
            }
            catch (Exception)
            {
                Log.ErrorFormat(Resources.GwswFileImporterBase_ImportDefinitionFile_Not_possible_to_import__0_,
                                directoryPath);
            }

            FilesToImport = new EventedList<string>(GwswDefaultFeatures?.Select(f => f.Value[2]));
        }

        public ILookup<SewerFeatureType, GwswElement> ImportGwswElementsFromGwswFiles(string path = null)
        {
            if (ActivityRunner == null)
            {
                ActivityRunner = new ActivityRunner();
                ActivityRunner.Activities.Add(new FileImportActivity(this));
            }

            FileImportActivity importGwswFileActivity = ActivityRunner.Activities.OfType<FileImportActivity>().FirstOrDefault(fia => fia.FileImporter.Equals(this));
            if (importGwswFileActivity == null)
            {
                return Enumerable.Empty<GwswElement>().ToLookup(gwswElement => default(SewerFeatureType));
            }

            ActivityRunner.MaxRunningTaskCount = 2 * Environment.ProcessorCount;

            var listOfImportedGwswFileImportActivities = new List<GwswFileImportActivity>();
            if (!string.IsNullOrEmpty(path))
            {
                var gwswFileImportActivity = new GwswFileImportActivity(path, GwswAttributesDefinition,
                                                                        CsvDelimeter, CsvSettingsSemiColonDelimeted, this);
                listOfImportedGwswFileImportActivities.Add(gwswFileImportActivity);
            }
            else
            {
                foreach (string filePath in FilesToImport)
                {
                    if (ShouldCancel)
                    {
                        break;
                    }

                    if (!File.Exists(filePath))
                    {
                        logHandler?.ReportErrorFormat(
                            Resources.GwswFileImporterBase_ImportFilesFromDefinitionFile_Could_not_find_file__0__,
                            (object)filePath);
                        continue;
                    }

                    var gwswFileImportActivity = new GwswFileImportActivity(filePath, GwswAttributesDefinition,
                                                                            CsvDelimeter, CsvSettingsSemiColonDelimeted, this);
                    listOfImportedGwswFileImportActivities.Add(gwswFileImportActivity);
                }
            }

            foreach (GwswFileImportActivity gwswFileImportActivity in listOfImportedGwswFileImportActivities)
            {
                ActivityRunner.Enqueue(gwswFileImportActivity);
            }

            while (listOfImportedGwswFileImportActivities.Any(im => im.Status != ActivityStatus.Cleaned))
            {
                Thread.Sleep(100);
            }

            return listOfImportedGwswFileImportActivities
                   .SelectMany(l => l.Elements)
                   .Where(gwswElement => Enum.IsDefined(typeof(SewerFeatureType), gwswElement.ElementTypeName))
                   .ToLookup(gwswElement => (SewerFeatureType)Enum.Parse(typeof(SewerFeatureType), gwswElement.ElementTypeName), gwswElement => gwswElement);
        }

        private void AddBoundariesOfNetworkOutletCompartmentsToModelDefinition(IWaterFlowFMModel fmModel)
        {
            ILookup<INode, Model1DBoundaryNodeData> boundaryConditionsByNode = fmModel.BoundaryConditions1D.ToLookup(bc => bc.Node, bc => bc);

            OutletCompartment[] networkOutletCompartments = fmModel.Network.OutletCompartments.ToArray();
            ParallelHelper.RunActionInParallel(this, networkOutletCompartments, outletCompartment =>
            {
                if (ShouldCancel)
                {
                    return;
                }

                if (!boundaryConditionsByNode.Contains(outletCompartment.ParentManhole))
                {
                    return;
                }

                foreach (Model1DBoundaryNodeData boundaryCondition in boundaryConditionsByNode[outletCompartment.ParentManhole])
                {
                    boundaryCondition.DataType = Model1DBoundaryNodeDataType.WaterLevelConstant;
                    boundaryCondition.WaterLevel = outletCompartment.SurfaceWaterLevel;
                }
            }, "Add Boundaries of Network OutletCompartments to model");
        }

        /// <summary>
        /// Transforms a CSV data file, into tables that we can handle internally
        /// </summary>
        /// <param name="path">Location of the CSV file to import.</param>
        /// <param name="mappingData">Delimeters and properties for handling the CSV file.</param>
        /// <returns>DataTable with the content of the CSV file of
        /// <param name="path"/>
        /// .
        /// </returns>
        public DataTable ImportFileAsDataTable(string path, CsvMappingData mappingData = null)
        {
            if (mappingData == null)
            {
                mappingData = GetCsvMappingDataForFileFromDefinition(path);
            }

            if (mappingData == null)
            {
                logHandler?.ReportErrorFormat(Resources.GwswFileImporterBase_ImportItem_No_mapping_was_found_to_import_File__0__,
                                              path);
                return null;
            }

            var csvImporter = new CsvImporter { AllowEmptyCells = true };
            var importedCsv = new DataTable();
            try
            {
                importedCsv =
                    csvImporter.ImportCsv(path, mappingData);
            }
            catch (Exception e)
            {
                Log.ErrorFormat(Resources.GwswFileImporterBase_ImportItem_Could_not_import_file__0___Reason___1_, path,
                                e.Message);
            }

            return importedCsv;
        }

        /// <summary>
        /// Gets or sets the CSV delimeter to split a line in a csv file.
        /// </summary>
        /// <value>
        /// The CSV delimeter.
        /// </value>
        public char CsvDelimeter { get; set; }

        /// <summary>
        /// Gets or sets the files to import.
        /// </summary>
        /// <value>
        /// The files to import.
        /// </value>
        public IList<string> FilesToImport { get; set; }

        private CsvSettings CsvSettingsSemiColonDelimeted
        {
            get
            {
                return csvSettings ?? (csvSettings = new CsvSettings
                                          {
                                              Delimiter = CsvDelimeter,
                                              FirstRowIsHeader = true,
                                              SkipEmptyLines = true
                                          });
            }
        }

        public IEventedList<GwswAttributeType> GwswAttributesDefinition { get; private set; }

        /// <summary>
        /// Dictionary content:
        /// Key = Feature FileName.
        /// Value = List containing 3 strings:
        /// [0] <string>ElementName</string>
        /// [1] <string>SewerFeatureType (mapped value)</string>
        /// [2] <string>Full path</string>
        /// </summary>
        public IDictionary<string, List<string>> GwswDefaultFeatures { get; private set; }

        public ILogHandler LogHandler => logHandler;

        #region IFileImporter

        public string Name
        {
            get
            {
                return "GWSW Feature Files";
            }
        }

        public string Description
        {
            get
            {
                return "Import model data from GWSW feature files";
            }
        }

        public string Category
        {
            get
            {
                return ProductCategories.OneDTwoDModelImportCategory;
            }
        }

        public Bitmap Image
        {
            get
            {
                return Resources.StructureFeatureSmall;
            }
        }

        public IEnumerable<Type> SupportedItemTypes
        {
            get
            {
                yield return typeof(IWaterFlowFMModel);
                yield return typeof(HydroModel);
                yield return typeof(RainfallRunoffModel);
            }
        }

        public bool CanImportOnRootLevel
        {
            get
            {
                return true;
            }
        }

        public string FileFilter
        {
            get
            {
                return "GWSW Csv Files (*.csv)|*.csv";
            }
        }

        public string TargetDataDirectory { get; set; }
        public bool ShouldCancel { get; set; }
        public ImportProgressChangedDelegate ProgressChanged { get; set; }

        public bool OpenViewAfterImport
        {
            get
            {
                return true;
            }
        }

        public bool CanImportOn(object targetObject)
        {
            return true;
        }

        #endregion

        private IDictionary<string, List<string>> CreateFileNameToViewDataDictionary(string directoryPath)
        {
            //Get the items to import
            var dictionary = GwswAttributesDefinition?.GroupBy(i => i.FileName)
                                                     .ToDictionary(
                                                         grp => grp.Key,
                                                         grp =>
                                                         {
                                                             var valueList = new List<string>();
                                                             var elementName = grp.FirstOrDefault(g => !string.IsNullOrEmpty(g.ElementName))?.ElementName;
                                                             SewerFeatureType featureName;
                                                             Enum.TryParse(elementName, out featureName);
                                                             valueList.Add(elementName);
                                                             valueList.Add(featureName.ToString());
                                                             valueList.Add(Path.Combine(directoryPath, grp.Key));
                                                             return valueList;
                                                         });

            return dictionary;
        }

        private CsvMappingData GetCsvMappingDataForFileFromDefinition(string fileName)
        {
            //Import file elements based on their attributes
            if (GwswAttributesDefinition == null || !GwswAttributesDefinition.Any())
            {
                Log.ErrorFormat(Resources.GwswFileImporterBase_ImportItem_No_mapping_was_found_to_import_File__0__,
                                fileName);
                return null;
            }

            List<GwswAttributeType> fileAttributes = GwswAttributesDefinition.Where(at => at.FileName.Equals(Path.GetFileName(fileName)))
                                                                             .ToList();
            var fileColumnMapping = new Dictionary<CsvRequiredField, CsvColumnInfo>();
            //Create column mapping
            fileAttributes.ForEach(
                attr =>
                    fileColumnMapping.Add(
                        new CsvRequiredField(attr.Key, attr.AttributeType),
                        attr.AttributeType == typeof(DateTime) ? new CsvColumnInfo(fileAttributes.IndexOf(attr), new DateTimeFormatInfo() { FullDateTimePattern = "yyyyMMdd" }) : new CsvColumnInfo(fileAttributes.IndexOf(attr), CultureInfo.InvariantCulture)));

            var mapping = new CsvMappingData
            {
                Settings = CsvSettingsSemiColonDelimeted,
                FieldToColumnMapping = fileColumnMapping,
            };
            return mapping;
        }
    }
}