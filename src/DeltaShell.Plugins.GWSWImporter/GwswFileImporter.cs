using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.Helpers;
using DelftTools.Hydro.SewerFeatures;
using DelftTools.Hydro.Structures;
using DelftTools.Shell.Core;
using DelftTools.Shell.Core.Extensions;
using DelftTools.Shell.Core.Workflow;
using DelftTools.Shell.Core.Workflow.DataItems;
using DelftTools.Utils;
using DelftTools.Utils.Aop;
using DelftTools.Utils.Collections.Generic;
using DelftTools.Utils.Csv.Importer;
using DelftTools.Utils.Editing;
using DeltaShell.NGHS.IO.DataObjects;
using DeltaShell.Plugins.DelftModels.HydroModel;
using DeltaShell.Plugins.DelftModels.RainfallRunoff;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Concepts.Nwrw;
using DeltaShell.Plugins.FMSuite.FlowFM;
using DeltaShell.Plugins.ImportExport.GWSW;
using DeltaShell.Plugins.ImportExport.GWSW.Properties;
using GeoAPI.Extensions.Coverages;
using GeoAPI.Extensions.Networks;
using GeoAPI.Geometries;
using log4net;
using NetTopologySuite.Extensions.Coverages;
using NetTopologySuite.Geometries;

namespace DeltaShell.Plugins.ImportExport.Gwsw
{
    /// <summary>
    /// Importer for GWSW files
    /// </summary>
    /// <seealso cref="DelftTools.Shell.Core.IFileImporter" />
    public class GwswFileImporter : IFileImporter
    {
        private readonly IDefinitionsProvider definitionsProvider;
        private static ILog Log = LogManager.GetLogger(typeof(GwswFileImporter));

        private CsvSettings csvSettings;

        private class GwswImportManager
        {
            private int totalAmountOfImportSteps;
            private int currentImportStep;
            public int AmountOfImportStepsPerFile;

            public void CalculateTotalAmountOfImportSteps(IList<string> filesToImport)
            {
                var amountOfFiles = filesToImport.Count;
                totalAmountOfImportSteps = amountOfFiles * AmountOfImportStepsPerFile + 1;
            }

            public void ReportProgress(string message)
            {
                currentImportStep++;
                new GwswFileImporter(new DefinitionsProvider()).SetProgress(message, currentImportStep, totalAmountOfImportSteps);
            }

            public void JumpImportStepsForNextFile()
            {
                currentImportStep += AmountOfImportStepsPerFile;
            }
        }

        public GwswFileImporter(IDefinitionsProvider definitionsProvider)
        {
            this.definitionsProvider = definitionsProvider ?? throw new ArgumentNullException(nameof(definitionsProvider));

            FilesToImport = new List<string>();
            GwswAttributesDefinition = new EventedList<GwswAttributeType>();
            GwswDefaultFeatures = new Dictionary<string, List<string>>();
            ImportManager = new GwswImportManager();
            CsvDelimeter = ';'; //Default value, can be changed.
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
            if (GwswAttributesDefinition == null || !GwswAttributesDefinition.Any())
            {
                Log.ErrorFormat(Resources.GwswFileImporter_ImportItem_No_mapping_was_found_to_import_Gwsw_Files_);
                return null;
            }

            if (!string.IsNullOrEmpty(path)) FilesToImport = new EventedList<string> {path};
            if (FilesToImport == null || FilesToImport.Count == 0) return null;

            Log.Info(Resources.GwswFileImporterBase_ImportFilesFromDefinitionFile_Importing_sub_files_);
            if (ShouldCancel)
                return null;
            var elementTypesList = ImportGwswElementsFromGwswFiles().ToList();

            var hydroModel = target is Project || target == null
                ? new HydroModelBuilder().BuildModel(ModelGroup.RHUModels)
                : target as HydroModel;


            var fmModel = hydroModel?.GetAllActivitiesRecursive<WaterFlowFMModel>()?.FirstOrDefault() ??
                          target as WaterFlowFMModel;
            if (fmModel != null)
            {
                ImportGwswNetworkInFmModel(elementTypesList, fmModel);
            }

            var rrModel = hydroModel?.GetAllActivitiesRecursive<RainfallRunoffModel>()?.FirstOrDefault() ??
                          target as RainfallRunoffModel;
            if (rrModel != null && fmModel?.Network != null)
            {
                ImportGwswNetworkInRrModel(elementTypesList, rrModel, fmModel?.Network, fmModel.LateralSourcesData);
                if (hydroModel != null)
                {
                    AddRRtoFMNwrwLinks(rrModel, fmModel.Network, fmModel.LateralSourcesData);
                }
            }

            if (hydroModel != null)
            {
                SetCurrentWorkflow(fmModel, rrModel, hydroModel);
            }

            return (target is Project || target == null) && !ShouldCancel ? hydroModel : null;
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
                var wfs = hydroModel.Workflows.Where(w => w.GetAllActivitiesRecursive<IActivity>().Count() == 2);
                var hydroModelCurrentWorkflow =
                    wfs.FirstOrDefault(wf => wf.GetActivitiesOfType<IWaterFlowFMModel>().Any());
                if (hydroModelCurrentWorkflow != null) hydroModel.CurrentWorkflow = hydroModelCurrentWorkflow;
            }
            else if (fmModel != null && rrModel != null)
            {
                var wfs = hydroModel.Workflows.Where(w => w.GetAllActivitiesRecursive<IActivity>().Count() == 3);
                var hydroModelCurrentWorkflow = wfs.FirstOrDefault(wf =>
                    wf.GetActivitiesOfType<IWaterFlowFMModel>().Any() &&
                    wf.GetActivitiesOfType<RainfallRunoffModel>().Any());
                if (hydroModelCurrentWorkflow != null) hydroModel.CurrentWorkflow = hydroModelCurrentWorkflow;
            }
            else if (fmModel == null && rrModel != null)
            {
                var wfs = hydroModel.Workflows.Where(w => w.GetAllActivitiesRecursive<IActivity>().Count() == 2);
                var hydroModelCurrentWorkflow =
                    wfs.FirstOrDefault(wf => wf.GetActivitiesOfType<RainfallRunoffModel>().Any());
                if (hydroModelCurrentWorkflow != null) hydroModel.CurrentWorkflow = hydroModelCurrentWorkflow;
            }
        }

        private void AddRRtoFMNwrwLinks(RainfallRunoffModel rrModel, IHydroNetwork network,
            IEnumerable<Model1DLateralSourceData> lateralSourcesData)
        {
            IList<string> listOfErrors = new List<string>();
            var bubbelingEventSetting = EventSettings.BubblingEnabled;
            try
            {
                ReportProgress("Adding links from Rainfall Runoff Model to FM model.");
                EventSettings.BubblingEnabled = false;
                foreach (var nwrwData in rrModel.GetAllModelData().OfType<NwrwData>())
                {
                    try
                    {
                        IBranch branch = FindTargetBranchForNwrwCatchmentBranch(network, nwrwData.Name);

                        if (branch != null)
                        {
                            LateralSource lateralSource = new LateralSource
                            {
                                Branch = branch,
                                Chainage = branch.Length,
                                Name = nwrwData.Name,
                                LongName = nwrwData.Name
                            };

                            AddLateralSourceToBranch(branch, lateralSource);
                            AddHydroLinkToCatchment(nwrwData, lateralSource);

                            // at FM-side, create lateral data of type REALTIME
                            AddLateralDataToFmModel(lateralSourcesData, lateralSource, Model1DLateralDataType.FlowRealTime, default(double));
                        }
                    }
                    catch (Exception e)
                    {
                        listOfErrors.Add(
                            $"Could not create hydrolink between the Rainfall Runoff Model and Flow FM Model: {e.Message}");
                    }
                }

                if (listOfErrors.Any())
                {
                    Log.ErrorFormat(
                        $"While adding hydrolinks between Rainfall Runoff Model and Flow FM Model we encountered the following errors: {Environment.NewLine}{string.Join(Environment.NewLine, listOfErrors)}");
                }
            }
            finally
            {
                EventSettings.BubblingEnabled = bubbelingEventSetting;
            }
            
        }

        /// <summary>
        /// Adds the specified Model1DLateralDataType and Flow
        /// to the Model1DLateralSourceData.
        /// </summary>
        /// <param name="lateralSourcesData"></param>
        /// <param name="lateralSource"></param>
        /// <param name="model1DBoundaryDataType"></param>
        private void AddLateralDataToFmModel(
            IEnumerable<Model1DLateralSourceData> lateralSourcesData,
            LateralSource lateralSource,
            Model1DLateralDataType model1DBoundaryDataType,
            double flow)
        {
            Model1DLateralSourceData model1DLateralSourceData =
                lateralSourcesData.FirstOrDefault(lsd =>
                    lsd.Feature ==
                    lateralSource);
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

            var hydroLink = nwrwData.Catchment.LinkTo(lateralSource);
            hydroLink.Geometry = new LineString(new[]
            {
                nwrwData.Catchment.InteriorPoint.Coordinate,
                lateralSource.Geometry.Coordinate
            });
        }

        /// <summary>
        /// Adds a LateralSource as a BranchFeature to a branch.
        /// </summary>
        /// <param name="branch"></param>
        /// <param name="name"></param>
        /// <param name="lateralSource"></param>
        private void AddLateralSourceToBranch(IBranch branch, LateralSource lateralSource)
        {
            lateralSource.Geometry = HydroNetworkHelper.GetStructureGeometry(branch, branch.Length);
            branch.BranchFeatures.Add(lateralSource);
        }

        /// <summary>
        /// Finds a Branch in a Network based on a Node name or Branch name.
        /// In case the name is Node name, we return the branch where the
        /// target or source compartment name is equal to the provided name.
        /// </summary>
        /// <param name="network"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        private IBranch FindTargetBranchForNwrwCatchmentBranch(IHydroNetwork network, string name)
        {
            if (network == null) throw new ArgumentNullException(nameof(network));
            IBranch branch = network.Branches.FirstOrDefault(b =>
                b.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase));
            if (branch == null)
            {
                branch = network.Branches
                    .OfType<IPipe>()
                    .FirstOrDefault(p =>
                        p.TargetCompartmentName.Equals(name,
                            StringComparison.InvariantCultureIgnoreCase) ||
                        p.SourceCompartmentName.Equals(name,
                            StringComparison.InvariantCultureIgnoreCase));
            }

            return branch;
        }

        private void ImportGwswNetworkInRrModel(
            IEnumerable<KeyValuePair<SewerFeatureType, GwswElement>> elementTypesList, RainfallRunoffModel rrModel,
            IHydroNetwork network, IEnumerable<Model1DLateralSourceData> lateralSourcesData)
        {
            var bubblingEnabledSetting = EventSettings.BubblingEnabled;
            try
            {
                var errorsDuringImport = new List<string>();
                var importedFeatureElements = ImportGwswDatabaseForRr(elementTypesList, errorsDuringImport).ToArray();
                if (errorsDuringImport.Any())
                {
                    Log.Error(
                        $"One or more errors occured during the import process: {string.Join(Environment.NewLine, errorsDuringImport)}");
                }

                if (rrModel == null || network == null || ShouldCancel) return;
                ReportProgress("Adding features to Rainfall Runoff Model.");
                EventSettings.BubblingEnabled = false;
                AddNwrwFeaturesToRainfallRunoffModel(importedFeatureElements, rrModel, network, lateralSourcesData);

            }
            finally
            {
                EventSettings.BubblingEnabled = bubblingEnabledSetting;
            }
            
        }

        private void AddNwrwFeaturesToRainfallRunoffModel(IEnumerable<INwrwFeature> importedFeatureElements,
            RainfallRunoffModel rrModel, IHydroNetwork network, IEnumerable<Model1DLateralSourceData> lateralSourcesData)
        {
            var featureElements = importedFeatureElements.ToList();
            var nrOfImportedFeatureElements = featureElements.Count;
            var stepSize = nrOfImportedFeatureElements / 20;

            var branchesGeometryDict = network.Branches.Select(b => new { b.Name, b.Target.Geometry });
            var compartmentsGeometryDict = network.Nodes.OfType<IManhole>().SelectMany(m => m.Compartments)
                .Select(c => new { c.Name, c.Geometry });
            var networkFeatureNameAndGeometries = branchesGeometryDict.Concat(compartmentsGeometryDict)
                .ToDictionary(a => a.Name, b => b.Geometry, StringComparer.InvariantCultureIgnoreCase);

            var listOfErrors = new List<string>();

            for (int i = 0; i < featureElements.Count; i++)
            {
                INwrwFeature e = featureElements[i];
                try
                {
                    if (ShouldCancel)
                        return;

                    var indexOf = i;

                    if (stepSize != 0 && indexOf % stepSize == 0)
                        SetProgress(
                            $"Adding feature to Rainfall Runoff Model ({indexOf / (double) nrOfImportedFeatureElements:P0})",
                            indexOf, nrOfImportedFeatureElements);

                    if (e is NwrwDefinition ||
                        e is NwrwDryWeatherFlowDefinition ||
                        e.Name != null && networkFeatureNameAndGeometries.ContainsKey(e.Name))
                    {
                        if (e.Name != null && networkFeatureNameAndGeometries.ContainsKey(e.Name))
                            e.Geometry = networkFeatureNameAndGeometries[e.Name];
                        e.AddNwrwCatchmentModelDataToModel(rrModel);
                    }

                    if (e is NwrwDischargeData nwrwDischargeData && nwrwDischargeData.DischargeType == DischargeType.Lateral)
                    {
                        IBranch branch = FindTargetBranchForNwrwCatchmentBranch(network, nwrwDischargeData.Name);
                        if (branch != null)
                        {
                            LateralSource lateralSource = new LateralSource
                            {
                                Branch = branch,
                                Chainage = branch.Length,
                                Name = nwrwDischargeData.Name,
                                LongName = nwrwDischargeData.Name
                            };

                            AddLateralSourceToBranch(branch, lateralSource);

                            // make sure the discharge data has the correct LateralSurface value
                            nwrwDischargeData.SetCorrectLateralSurface(rrModel);

                            // at FM-side, create lateral data of type CONSTANT
                            AddLateralDataToFmModel(lateralSourcesData, lateralSource, Model1DLateralDataType.FlowConstant, nwrwDischargeData.LateralSurface);
                        }

                    }
                }
                catch (Exception exception)
                {
                    listOfErrors.Add(exception.Message + Environment.NewLine);
                }
            }

            if (listOfErrors.Any())
                Log.ErrorFormat(
                    $"While adding GWSW features to Rainfall Runoff Model we encountered the following errors: {Environment.NewLine}{string.Join(Environment.NewLine, listOfErrors)}");
        }

        private void ImportGwswNetworkInFmModel(
            IEnumerable<KeyValuePair<SewerFeatureType, GwswElement>> elementTypesList, WaterFlowFMModel fmModel)
        {
            var network = fmModel?.Network;
            network?.BeginEdit(new DefaultEditAction("Importing GWSW database."));
            fmModel?.UnSubscribeFromNetwork(network);
            var bubblingEnabledSetting = EventSettings.BubblingEnabled;
            try
            {
                var importedFeatureElements = SewerFeatureFactory.CreateSewerEntities(elementTypesList, SetProgress, this).ToList();
                EventSettings.BubblingEnabled = false;
                if (network != null && !ShouldCancel)
                {
                    ReportProgress("Adding features to network.");
                    AddSewerFeaturesToNetwork(importedFeatureElements, network);

                    ReportProgress("Adding discretisation points of sewerconnection to networkdiscretisation.");
                    AddDiscretisationPointsOfSewerConnections(network, fmModel?.NetworkDiscretization);

                    EventSettings.BubblingEnabled = true;
                    SetProgress("Adding roughness sections of sewer connections to roughness 1d list.",4,10);
                    EventSettings.BubblingEnabled = false;

                    fmModel.UpdateRoughnessSections();

                    ReportProgress("Adding model1d lateral source of sewer connections to lateral source (data) list.");
                    AddModel1DLateralSourceToFmModel(network, fmModel);

                    ReportProgress("Adding model1d boundary nodes of manholes to boundary data list.");
                    AddModel1DBoundaryNodesToFmModel(network, fmModel);

                    ReportProgress("Add Boundaries Of Network Outlet Compartments To ModelDefinition.");
                    AddBoundariesOfNetworkOutletCompartmentsToModelDefinition(fmModel);
                }
            }
            finally
            {
                EventSettings.BubblingEnabled = bubblingEnabledSetting;
                fmModel?.SubscribeToNetwork(network);
                network?.EndEdit();
            }
        }
        
        private void AddModel1DLateralSourceToFmModel(IHydroNetwork network, WaterFlowFMModel fmModel)
        {
            var lateralSources = network.Channels.SelectMany(c => c.BranchSources).ToList();
            var nrOfImportedFeatureElements = lateralSources.Count;
            var stepSize = nrOfImportedFeatureElements / 20;
            var listOfErrors = new List<string>();
            try
            {
                fmModel.UnSubscribeLateralSourcesData();
                
                lateralSources
                    .ForEach(
                        lateralSource =>
                        {
                            try
                            {
                                if (ShouldCancel)
                                    return;
                                var indexOf = lateralSources.IndexOf(lateralSource);

                                if (stepSize != 0 && indexOf % stepSize == 0)
                                {
                                    EventSettings.BubblingEnabled = true;
                                    SetProgress(
                                        $"Adding channel lateral sources to model ({((double)((double)indexOf / (double)nrOfImportedFeatureElements)):P0})",
                                        indexOf, nrOfImportedFeatureElements);
                                    EventSettings.BubblingEnabled = false;
                                }

                                if (!fmModel.LateralSourcesData.Any(lsd => lsd.Feature == lateralSource))
                                {
                                    var model1DLateralSourceData = new Model1DLateralSourceData { Feature = lateralSource, UseSalt = false, UseTemperature = false };
                                    fmModel.LateralSourcesData.Add(model1DLateralSourceData);
                                    fmModel.LateralSourcesDataItemSet.DataItems.Add(new DataItem(model1DLateralSourceData));
                                }
                            }
                            catch (Exception exception)
                            {
                                listOfErrors.Add(exception.Message + Environment.NewLine);
                            }
                        });
                if (listOfErrors.Any())
                    Log.ErrorFormat(
                        $"While adding model1d lateral sources to fm model we encountered the following errors: {Environment.NewLine}{string.Join(Environment.NewLine, listOfErrors)}");
            }
            finally
            {
                fmModel.SubscribeLateralSourcesData();
            }
        }
        private void AddModel1DBoundaryNodesToFmModel(IHydroNetwork network, WaterFlowFMModel fmModel)
        {
            var networkManholes = network.Manholes.ToList();
            var nrOfImportedFeatureElements = networkManholes.Count;
            var stepSize = nrOfImportedFeatureElements / 20;
            var listOfErrors = new List<string>();
            try
            {
                fmModel.UnSubscribeBoundaryConditions1D();
                
                networkManholes.ForEach(manhole =>
                {
                    try
                    {
                        if (ShouldCancel)
                            return;
                        var indexOf = networkManholes.IndexOf(manhole);

                        if (stepSize != 0 && indexOf % stepSize == 0)
                        {
                            EventSettings.BubblingEnabled = true;
                            SetProgress(
                                $"Adding model1d boundary nodes to model ({((double) ((double) indexOf / (double) nrOfImportedFeatureElements)):P0})",
                                indexOf, nrOfImportedFeatureElements);
                            EventSettings.BubblingEnabled = false;
                        }

                        var bc = Helper1D.CreateDefaultBoundaryCondition(manhole, false, false);
                        bc.SetBoundaryConditionDataForOutlet();
                        fmModel.BoundaryConditions1D.Add(bc);
                        fmModel.BoundaryConditions1DDataItemSet.DataItems.Add(new DataItem(bc) {Hidden = bc?.DataType == Model1DBoundaryNodeDataType.None});
                    }
                    catch (Exception exception)
                    {
                        listOfErrors.Add(exception.Message + Environment.NewLine);
                    }
                });
                
                if (listOfErrors.Any())
                    Log.ErrorFormat(
                        $"While adding model1d boundary data nodes to fm model we encountered the following errors: {Environment.NewLine}{string.Join(Environment.NewLine, listOfErrors)}");
            }
            finally
            {
                fmModel.SubscribeBoundaryConditions1D();
            }
        }

        private void AddDiscretisationPointsOfSewerConnections(IHydroNetwork network, IDiscretization networkDiscretization)
        {
            NamingHelper.MakeNamesUnique(network.Branches);
            var networkSewerConnections = network.SewerConnections.ToList();
            var nrOfImportedFeatureElements = networkSewerConnections.Count;
            var stepSize = nrOfImportedFeatureElements / 20;
            var listOfErrors = new List<string>();
            var currentLocations = new HashSet<Coordinate>(networkDiscretization.Locations.Values.Select(l => l.Geometry?.Coordinate));
            var newLocations = new List<NetworkLocation>();
            networkSewerConnections.ForEach(sewerConnection =>
            {
                try
                {
                    if (ShouldCancel)
                        return;
                    var indexOf = networkSewerConnections.IndexOf(sewerConnection);

                    if (stepSize != 0 && indexOf % stepSize == 0)
                    {
                        EventSettings.BubblingEnabled = true;
                        SetProgress(
                            $"Adding network discretizations points of sewer connection to model ({((double) ((double) indexOf / (double) nrOfImportedFeatureElements)):P0})",
                            indexOf, nrOfImportedFeatureElements);
                        EventSettings.BubblingEnabled = false;
                    }

                    var sourceLocation = new NetworkLocation(sewerConnection, 0.0);
                    var locationGeometry = sourceLocation.Geometry;
                    if (locationGeometry != null)
                    {
                        if (!currentLocations.Contains(locationGeometry.Coordinate))
                        {
                            newLocations.Add(sourceLocation);
                            currentLocations.Add(locationGeometry.Coordinate);
                        }
                    }


                    if (sewerConnection?.Length > 0)
                    {
                        var targetLocation = new NetworkLocation(sewerConnection, sewerConnection.Length);
                        locationGeometry = targetLocation.Geometry;
                        if (locationGeometry != null)
                        {
                            if (!currentLocations.Contains(locationGeometry.Coordinate))
                            {
                                newLocations.Add(targetLocation);
                                currentLocations.Add(locationGeometry.Coordinate);
                            }
                        }
                    }
                }
                catch (Exception exception)
                {
                    listOfErrors.Add(exception.Message + Environment.NewLine);
                }
            });
            networkDiscretization.Locations.AddValues(newLocations);
            if (listOfErrors.Any())
                Log.ErrorFormat(
                    $"While adding discretisation points to network discretisation we encountered the following errors: {Environment.NewLine}{string.Join(Environment.NewLine, listOfErrors)}");
        }

        private GwswImportManager ImportManager { get; }

        private IEnumerable<INwrwFeature> ImportGwswDatabaseForRr(
            IEnumerable<KeyValuePair<SewerFeatureType, GwswElement>> elementTypesList, IList<string> errorsDuringImport)
        {
            // Surface types (oppervlak.csv)
            var surfaceElements = elementTypesList.Where(k => k.Key == SewerFeatureType.Surface).Select(k => k.Value)
                .ToArray();
            foreach (var feature in CreateNwrwFeatures(surfaceElements, GwswNwrwGenerator.CreateNewNwrwSurfaceData,
                errorsDuringImport))
            {
                yield return feature;
            }

            // Runoff types (nwrw.csv)
            var runOffElements = elementTypesList.Where(k => k.Key == SewerFeatureType.Runoff).Select(k => k.Value)
                .ToArray();
            foreach (var feature in CreateNwrwFeatures(runOffElements, GwswNwrwGenerator.CreateNewNwrwRunoffDefinition,
                errorsDuringImport))
            {
                yield return feature;
            }

            // Distribution types (verloop.csv)
            var dryWeatherFlowElements = elementTypesList.Where(k => k.Key == SewerFeatureType.Distribution)
                .Select(k => k.Value).ToArray();
            foreach (var feature in CreateNwrwFeatures(dryWeatherFlowElements,
                GwswNwrwGenerator.CreateNewNwrwDryWeatherFlowDefinition, errorsDuringImport))
            {
                yield return feature;
            }

            // Discharge types (debiet.csv)
            var dischargeElements = elementTypesList.Where(k => k.Key == SewerFeatureType.Discharge)
                .Select(k => k.Value).ToArray();
            foreach (var feature in CreateNwrwFeatures(dischargeElements, GwswNwrwGenerator.CreateNewNwrwDischargeData,
                errorsDuringImport))
            {
                yield return feature;
            }
        }

        private IEnumerable<INwrwFeature> CreateNwrwFeatures(IEnumerable<GwswElement> elementTypesCollection,
            Func<GwswElement, IList<string>, INwrwFeature> createNwrwFeatureFunc,
            IList<string> listOfErrorsGenerated)
        {
            var totalNrOfElements = elementTypesCollection.Count();
            var currentStep = 1;
            foreach (GwswElement gwswElement in elementTypesCollection)
            {
                if (ShouldCancel)
                    yield break;

                var stepSize = totalNrOfElements / 20;
                if (stepSize != 0 && currentStep % stepSize == 0)
                {
                    SetProgress("Generating Rainfall Runoff features", currentStep, totalNrOfElements);
                }

                yield return createNwrwFeatureFunc(gwswElement, listOfErrorsGenerated);


                currentStep++;
            }
        }

        private IEnumerable<KeyValuePair<SewerFeatureType, GwswElement>> ImportGwswElementsFromGwswFiles()
        {
            InitializeImportManager();
            foreach (var filePath in FilesToImport)
            {
                if (ShouldCancel)
                    yield break;
                if (!File.Exists(filePath))
                {
                    Log.ErrorFormat(
                        Resources.GwswFileImporterBase_ImportFilesFromDefinitionFile_Could_not_find_file__0__,
                        filePath);
                    ImportManager.JumpImportStepsForNextFile();
                    continue;
                }

                var gwswElements = ImportGwswElementList(filePath);

                foreach (var gwswElement in gwswElements)
                {
                    if (ShouldCancel)
                        yield break;
                    SewerFeatureType elementType;
                    if (!Enum.TryParse(gwswElement?.ElementTypeName, out elementType)) continue;

                    yield return new KeyValuePair<SewerFeatureType, GwswElement>(elementType, gwswElement);
                }
            }
        }

        private void AddSewerFeaturesToNetwork(IEnumerable<ISewerFeature> importedFeatureElements,
            IHydroNetwork network)
        {
            var featureElements = importedFeatureElements.ToList();
            var nrOfImportedFeatureElements = featureElements.Count;
            var stepSize = nrOfImportedFeatureElements / 20;
            var listOfErrors = new List<string>();
            var helper = new SewerImporterHelper();
            featureElements.ForEach(e =>
            {
                try
                {
                    if (ShouldCancel)
                        return;
                    var indexOf = featureElements.IndexOf(e);

                    if (stepSize != 0 && indexOf % stepSize == 0)
                    {
                        EventSettings.BubblingEnabled = true;
                        SetProgress(
                            $"Adding feature to network ({((double) ((double) indexOf / (double) nrOfImportedFeatureElements)):P0})",
                            indexOf, nrOfImportedFeatureElements);
                        EventSettings.BubblingEnabled = false;
                    }
                    
                    e.AddToHydroNetwork(network, helper);
                }
                catch (Exception exception)
                {
                    listOfErrors.Add(exception.Message + Environment.NewLine);
                }
            });
            if (listOfErrors.Any())
                Log.ErrorFormat(
                    $"While adding GWSW features to network we encountered the following errors: {Environment.NewLine}{string.Join(Environment.NewLine, listOfErrors)}");
        }

        private void AddBoundariesOfNetworkOutletCompartmentsToModelDefinition(IWaterFlowFMModel fmModel)
        {
            foreach (var outletCompartment in fmModel.Network.OutletCompartments)
            {
                if (ShouldCancel)
                    return;
                var boundaryCondition =
                    fmModel.BoundaryConditions1D.FirstOrDefault(bc => bc.Node == outletCompartment.ParentManhole);
                if (boundaryCondition == null) continue;

                boundaryCondition.DataType = Model1DBoundaryNodeDataType.WaterLevelConstant;
                boundaryCondition.WaterLevel = outletCompartment.SurfaceWaterLevel;
            }
        }

        private void InitializeImportManager()
        {
            ImportManager.AmountOfImportStepsPerFile = 2;
            ImportManager.CalculateTotalAmountOfImportSteps(FilesToImport);
        }

        private void ReportProgress(string message)
        {
            ImportManager.ReportProgress(message);
        }

        /// <summary>
        /// Loads the feature files from a directory.
        /// </summary>
        /// <param name="directoryPath">The directory path.</param>
        public void LoadFeatureFiles(string directoryPath)
        {
            if (string.IsNullOrWhiteSpace(directoryPath)) return;

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

        /// <summary>
        /// Given a file path, it tries to import a CSV file and generate Gwsw elements out of the data on it.
        /// </summary>
        /// <param name="path">The location of the CSV file we want to transform into Gwsw elements.</param>
        /// <returns>List of GwswElements or null</returns>
        public IEnumerable<GwswElement> ImportGwswElementList(string path)
        {
            var importedDataTable = ImportFileAsDataTable(path); // TODO Sil -> invalid cast exception from this method
            if (importedDataTable == null)
                yield break;


            var elementTypeFound =
                GwswAttributesDefinition.FirstOrDefault(at => at.FileName.Equals(Path.GetFileName(path)));
            var elementTypeName = string.Empty;
            if (elementTypeFound != null)
            {
                elementTypeName = elementTypeFound.ElementName;
                Log.InfoFormat(Resources.GwswFileImporterBase_ImportItem_Mapping_file__0__as_element__1_, path,
                    elementTypeName);
            }
            else
            {
                Log.InfoFormat(
                    Resources
                        .GwswFileImporterBase_ImportItem_Occurrences_on_file__0__will_not_be_mapped_to_any_element_,
                    path);
                yield break;
            }

            if (!IsColumnMappingCorrect(path, importedDataTable))
            {
                yield break;
            }

            var nrOfRows = importedDataTable.Rows.Count;
            foreach (DataRow dataRow in importedDataTable.Rows)
            {
                if (ShouldCancel)
                    yield break;
                var lineNumber = importedDataTable.Rows.IndexOf(dataRow);
                var stepSize = (int) nrOfRows / 10;
                if (stepSize != 0 && lineNumber % stepSize == 0)
                    SetProgress($"Importing file {Path.GetFileName(path) ?? ("<unknown_file>")}", lineNumber, nrOfRows);
                var element = new GwswElement {ElementTypeName = elementTypeName};
                for (var i = 0; i < dataRow.ItemArray.Length; i++)
                {
                    var cell = dataRow.ItemArray[i];
                    var columnName = importedDataTable.Columns[i].ColumnName;
                    var attribute = new GwswAttribute
                    {
                        LineNumber = lineNumber,
                        ValueAsString = cell.ToString()
                    };
                    if (GwswAttributesDefinition != null)
                    {
                        var foundAttributeType = GwswAttributesDefinition.FirstOrDefault(attr =>
                            attr.ElementName.Equals(elementTypeName) && attr.Key.Equals(columnName));
                        attribute.GwswAttributeType = foundAttributeType;
                    }

                    element.GwswAttributeList.Add(attribute);
                }

                yield return element;
            }
        }

        private bool IsColumnMappingCorrect(string path, DataTable importedDataTable)
        {
            var result = true;
            string headerLineFile;
            using (var reader = new StreamReader(path))
            {
                headerLineFile = reader.ReadLine() ?? string.Empty;
            }

            var headersFile = headerLineFile.Split(CsvDelimeter).Distinct().ToArray();
            var fileAttributes = GwswAttributesDefinition
                .Where(at => at.FileName.Equals(Path.GetFileName(Path.GetFileName(path)))).ToList();
            for (var columnIndex = 0; columnIndex < importedDataTable.Columns.Count; columnIndex++)
            {
                var columnName = importedDataTable.Columns[columnIndex].ColumnName;
                var fileAttribute = fileAttributes.First(a => a.Key.Equals(columnName));
                var expectedHeader = fileAttribute.LocalKey;
                var headerName = headersFile[columnIndex];
                if (!expectedHeader.ToLower().Equals(headerName.ToLower().Trim()))
                {
                    Log.ErrorFormat(
                        Resources
                            .GwswFileImporterBase_ImportItem_column__0__expectedcolumn__1__of_file__2__was_not_mapped_correctly__,
                        headerName, expectedHeader, path);
                    result = false;
                }
            }

            return result;
        }

        /// <summary>
        /// Transforms a CSV data file, into tables that we can handle internally
        /// </summary>
        /// <param name="path">Location of the CSV file to import.</param>
        /// <param name="mappingData">Delimeters and properties for handling the CSV file.</param>
        /// <returns>DataTable with the content of the CSV file of <param name="path"/>.</returns>
        public DataTable ImportFileAsDataTable(string path, CsvMappingData mappingData = null)
        {
            if (mappingData == null)
                mappingData = GetCsvMappingDataForFileFromDefinition(path);

            if (mappingData == null)
            {
                Log.ErrorFormat(Resources.GwswFileImporterBase_ImportItem_No_mapping_was_found_to_import_File__0__,
                    path);
                return null;
            }

            var csvImporter = new CsvImporter {AllowEmptyCells = true};
            var importedCsv = new DataTable();
            try
            {
                importedCsv =
                    csvImporter.ImportCsv(path, mappingData); // TODO Sil -> Invalid cast exception from this method
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
        ///     [0] <string>ElementName</string>
        ///     [1] <string>SewerFeatureType (mapped value)</string>
        ///     [2] <string>Full path</string>
        /// </summary>
        public IDictionary<string, List<string>> GwswDefaultFeatures { get; private set; }
        
        #region IFileImporter

        public string Name
        {
            get { return "GWSW Feature File importer"; }
        }

        public string Description
        {
            get { return Name; }
        }

        public string Category
        {
            get { return "1D / 2D"; }
        }

        public Bitmap Image
        {
            get { return Resources.StructureFeatureSmall; }
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
            get { return true; }
        }

        public string FileFilter
        {
            get { return "GWSW Csv Files (*.csv)|*.csv"; }
        }

        public string TargetDataDirectory { get; set; }
        public bool ShouldCancel { get; set; }
        public ImportProgressChangedDelegate ProgressChanged { get; set; }

        public bool OpenViewAfterImport
        {
            get { return true; }
        }

        public bool CanImportOn(object targetObject)
        {
            return true;
        }

        #endregion

        private void SetProgress(string currentStepName, int currentStep, int totalSteps)
        {
            ProgressChanged?.Invoke(currentStepName, currentStep, totalSteps);
        }
        
        private IDictionary<string, List<string>> CreateFileNameToViewDataDictionary(string directoryPath)
        {
            //Get the items to import
            var dictionary = GwswAttributesDefinition?.GroupBy(i => i.FileName)
                .ToDictionary(
                    grp => grp.Key,
                    grp =>
                    {
                        var valueList = new List<string>();
                        var elementName = grp.FirstOrDefault(g => !String.IsNullOrEmpty(g.ElementName))?.ElementName;
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

            var fileAttributes = GwswAttributesDefinition.Where(at => at.FileName.Equals(Path.GetFileName(fileName)))
                .ToList();
            var fileColumnMapping = new Dictionary<CsvRequiredField, CsvColumnInfo>();
            //Create column mapping
            fileAttributes.ForEach(
                attr =>
                    fileColumnMapping.Add(
                        new CsvRequiredField(attr.Key, attr.AttributeType),
                        attr.AttributeType == typeof(DateTime)? new CsvColumnInfo(fileAttributes.IndexOf(attr), new DateTimeFormatInfo()
                        {
                            FullDateTimePattern = "yyyyMMdd"
                        }) : new CsvColumnInfo(fileAttributes.IndexOf(attr), CultureInfo.InvariantCulture)));

            var mapping = new CsvMappingData
            {
                Settings = CsvSettingsSemiColonDelimeted,
                FieldToColumnMapping = fileColumnMapping,
            };
            return mapping;
        }
    }
}