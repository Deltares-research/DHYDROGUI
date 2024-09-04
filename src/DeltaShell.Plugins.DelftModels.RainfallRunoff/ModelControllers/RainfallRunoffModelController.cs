using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Shell.Core.Workflow;
using DelftTools.Utils;
using DelftTools.Utils.Collections;
using DelftTools.Utils.Collections.Generic;
using DelftTools.Utils.IO;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Concepts;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.FileWriter;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.IO.Converters;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.ModelControllers.Concepts;
using GeoAPI.Extensions.Coverages;
using GeoAPI.Extensions.Feature;
using GeoAPI.Geometries;
using log4net;
using NetTopologySuite.Geometries;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.ModelControllers
{
    public class RainfallRunoffModelController : IRainfallRunoffModelController
    {
        #region Fields

        private static readonly ILog log = LogManager.GetLogger(typeof(RainfallRunoffModelController));
        private static readonly IOEvaporationMeteoDataSourceConverter meteoDataSourceConverter = new IOEvaporationMeteoDataSourceConverter();

        private readonly IEnumerable<IConceptModelController> conceptControllers = new IConceptModelController[]
            {
                new PavedModelController(), new UnpavedModelController(), new OpenWaterModelController(),
                new GreenhouseModelController(), new SacramentoModelController(), new HbvModelController(), 
                new NwrwModelController(), 
            };

        private readonly RainfallRunoffModel model;

        private IList<IFeature> rrBoundaries;
        private IList<ModelLink> links;
        private IList<IFeature> allRRNodes;

        private IRRModelHybridFileWriter writer;
        
        private string originalDirectory = "";
        private string workingDirectory = "";
        private readonly DateTime currentTime = DateTime.MinValue;
        private const bool runningParallelWithFlow = false; //cached per run

        private IEnumerable<CatchmentModelData> modelDataCache;
        private IDictionary<Catchment, CatchmentModelData> catchmentDataLookUp;
        private IDictionary<RunoffBoundary, RunoffBoundaryData> boundaryDataLookUp;

        #endregion

        public RainfallRunoffModelController(RainfallRunoffModel model)
        {
            if (model == null)
            {
                throw new ArgumentNullException("model");
            }
            this.model = model;
            GetWorkingDirectoryDelegate = GetTempOrExplicitWorkingDirectory;
            OutputController = new RainfallRunoffModelOutputController();
        }

        private RainfallRunoffModelOutputController OutputController { get; }

        //For use in tests only
        public string LastCrashReason { get; set; }

        private IRRModelHybridFileWriter Writer
        {
            get { return writer; }
            set
            {
                writer = value;

                foreach (IConceptModelController controller in conceptControllers)
                {
                    controller.Writer = writer;
                    controller.RootController = this;
                }
            }
        }

       
        public Func<string> GetWorkingDirectoryDelegate { get; set; }

        public string WorkingDirectory
        {
            get { return workingDirectory; }
        }

        private string GetTempOrExplicitWorkingDirectory()
        {
            string workingDirectory;
            var explicitDir = model.WorkingDirectory;
            if (!String.IsNullOrEmpty(explicitDir) && Directory.Exists(explicitDir) &&
                !FileUtils.PathIsRelative(explicitDir))
            {
                workingDirectory = explicitDir;
            }
            else
            {
                workingDirectory = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            }
            FileUtils.DeleteIfExists(workingDirectory);
            Directory.CreateDirectory(workingDirectory);
            workingDirectory += "\\";
            return workingDirectory;
        }

        /// <summary>
        /// Writes the (potentially user customized fixed file) in the model run directory
        /// </summary>
        private void WriteSobekFixedFiles()
        {
            //note: || If you add a file here, make sure to remove it from the Fixed folder in the ||
            //note: || RainfallRunoffModelEngine solution, otherwise this one will be overwritten  ||
            //note: || Also add it to the fixed folder in the test project and adjust the          ||
            //note: || FixtureSetUp in RRModelHybridEngineTest otherwise tests won't run           ||

            WriteFixedFile(model.FixedFiles.UnpavedCropFactorsFile);
            WriteFixedFile(model.FixedFiles.UnpavedStorageCoeffFile);
            WriteFixedFile(model.FixedFiles.GreenhouseClassesFile);
            WriteFixedFile(model.FixedFiles.GreenhouseStorageFile);
            WriteFixedFile(model.FixedFiles.GreenhouseUsageFile);
            WriteFixedFile(model.FixedFiles.OpenWaterCropFactorFile);
        }

        private static void WriteFixedFile(TextDocument textDocument)
        {
            File.WriteAllText(textDocument.Name, textDocument.Content);
        }

        internal static void AddFeaturesToFeatureCoverage(IFeatureCoverage featureCoverage, IList features)
        {
            featureCoverage.Features = new EventedList<IFeature>(features.OfType<IFeature>());
        }

        private void FillOutputCoveragesWithFeatures(EngineParameter modelParameter, ITimeBasedFunction coverage)
        {
            var featureCoverage = coverage.InnerFunction as IFeatureCoverage;
            if (featureCoverage == null)
                return; // nothing to do

            if (modelParameter.ElementSet == ElementSet.BoundaryElmSet)
            {
                AddFeaturesToFeatureCoverage(featureCoverage, rrBoundaries.ToList());
            }
            else if (modelParameter.ElementSet == ElementSet.LinkElmSet)
            {
                AddFeaturesToFeatureCoverage(featureCoverage, links.Select(l => l.RealLink).ToList());
            }
            else if (modelParameter.ElementSet == ElementSet.WWTPElmSet)
            {
                AddFeaturesToFeatureCoverage(featureCoverage, model.Basin.WasteWaterTreatmentPlants.ToList());
            }
            else if (modelParameter.ElementSet == ElementSet.BalanceNodeElmSet)
            {
                if (allRRNodes.Count == 0)
                {
                    return;
                }
                AddFeaturesToFeatureCoverage(featureCoverage, allRRNodes.ToList());
            }
            else
            {
                IConceptModelController applicableController = GetApplicableController(modelParameter);
                if (applicableController != null)
                {
                    applicableController.OnInitializeFeatureCoverage(modelParameter, featureCoverage);
                }
                else
                {
                    throw new NotImplementedException("Cannot fill spatial data: no applicable controller");
                }
            }
        }

        private IConceptModelController GetApplicableController(EngineParameter modelParameter)
        {
            List<IConceptModelController> applicableControllers =
                conceptControllers.Where(cc => cc.CanHandle(modelParameter.ElementSet)).ToList();
            if (applicableControllers.Count == 0 || applicableControllers.Count > 1)
            {
                throw new NotImplementedException(
                    String.Format("No applicable / multiple controller(s) for parameter {0} (count: {1})",
                        modelParameter, applicableControllers.Count));
            }

            return applicableControllers.First();
        }

        private void SendConceptsToModelAndUpdateLinks()
        {
            var conceptDataNames = new List<string>();
            foreach (var conceptData in modelDataCache)
            {
                var controller = conceptControllers.FirstOrDefault(cc => cc.CanHandle(conceptData));

                if (controller == null)
                {
                    conceptDataNames.Add(conceptData.Name);
                    continue;
                }
                controller.AddArea(model, conceptData, links, allRRNodes);
            }

            if (conceptDataNames.Any())
            {
                log.InfoFormat("No model controller found for concept data {0}", string.Join(",", conceptDataNames));
            }
        }

        private void SendLinksToModel()
        {
            foreach (var link in links)
            {
                Writer.AddLink(link.Name, link.FromId, link.ToId);
            }
        }

        private void SendWasteWaterTreatmentPlantsToModelAndUpdateLinks()
        {
            foreach (var wwtp in model.Basin.WasteWaterTreatmentPlants)
            {
                Writer.AddWasteWaterTreatmentPlant(wwtp.Name, wwtp.Geometry?.Coordinate.X ?? 0.0, wwtp.Geometry?.Coordinate.Y ?? 0.0);
                if (HasOutgoingLink(wwtp))
                {
                    links.Add(CreateModelLink(wwtp));
                }
                allRRNodes.Add(wwtp);
            }
        }

        private static bool HasOutgoingLink(WasteWaterTreatmentPlant wwtp)
        {
            return wwtp.Links.Any(l => l.Source.Equals(wwtp));
        }

        private void SendBoundariesToModel()
        {
            foreach (var link in links)
            {
                if (link.ToFeature == null || link.ToFeature is RunoffBoundary || link.ToFeature is ILateralSource)
                    //only fake features & explicit boundaries
                {
                    //if no target feature; fallback to source (catchment) as fake boundary
                    var boundary = link.ToFeature ?? link.FromFeature;

                    if (rrBoundaries.Contains(boundary))
                        continue; //already added

                    if (boundary is Catchment boundaryOnTheCatchment && Equals(boundaryOnTheCatchment.CatchmentType, CatchmentType.NWRW))
                        continue; //Nwrw catchments don't have catchment boundaries
                    
                    rrBoundaries.Add(boundary);
                    allRRNodes.Add(boundary);
                    Writer.AddBoundaryNode(link.ToId, GetWaterLevelAtBoundary(boundary), boundary.Geometry?.Coordinate.X ?? 0.0, boundary.Geometry?.Coordinate.Y ?? 0.0);
                }
            }
        }

       
        private CatchmentModelData GetCatchmentModelData(Catchment catchment)
        {
            CatchmentModelData cmd;
            if (!catchmentDataLookUp.TryGetValue(catchment, out cmd))
            {
                return null;
            }
            return cmd;
        }

        private RunoffBoundaryData GetBoundaryData(RunoffBoundary runoffBoundary)
        {
            RunoffBoundaryData bd;
            if (!boundaryDataLookUp.TryGetValue(runoffBoundary, out bd))
            {
                return null;
            }
            return bd;
        }

        public double GetWaterLevelAtBoundary(IFeature feature)
        {
            // if catchment, it's either
            //  -not linked: grab the local defined data
            //  -linked to RR boundary: grab boundary data
            //  -linked to flow:
            //   - running sequential: grab locally defined data
            //   - running simultaneous (parallel): grab flow data
            // if RR boundary, grab that data

            var catchment = feature as Catchment;
            if (catchment != null)
            {
                var link = catchment.Links.FirstOrDefault();
                if (link != null && link.Target is RunoffBoundary)
                {
                    //grab from boundary
                    return GetWaterLevelAtBoundary(link.Target);
                }
                else //linked to flow (or not unpaved)
                {
                    var modelData = GetCatchmentModelData(catchment) as UnpavedData;
                    if (modelData == null)
                        return -99; //not unpaved

                    if (link == null || !runningParallelWithFlow) //no link or sequential
                    {
                        //grab local
                        return modelData.BoundarySettings.BoundaryData.Evaluate(currentTime);
                    }

                    //grab from flow (only if linked simultaneous)
                    if (currentTime == model.StartTime) //flow hasn't ran yet
                        return 0.0;
                    return model.InputWaterLevel.Evaluate(currentTime, catchment);
                }
            }

            var runoffBoundary = feature as RunoffBoundary;
            if (runoffBoundary != null)
            {
                return GetBoundaryData(runoffBoundary).Series.Evaluate(currentTime);
            }

            var lateral = feature as ILateralSource;
            if (lateral != null)
            {
                return 0d;
            }

            var wwtp = feature as WasteWaterTreatmentPlant;
            if (wwtp != null)
            {
                return -99;
            }

            throw new InvalidOperationException("Unexpected feature");
        }

        public bool WriteFiles()
        {
            PrepareWorkingDirectory();
            Writer = RRModelHybridFileWriterFactory.GetWriter();
            DoInWorkingDirectory(() =>
            {
                modelDataCache = null;
                modelDataCache = model.GetAllModelData().ToList();
                catchmentDataLookUp = modelDataCache.ToDictionary(cd => cd.Catchment);
                boundaryDataLookUp = model.BoundaryData.ToDictionary(bd => bd.Boundary);

                allRRNodes = new List<IFeature>();
                links = new List<ModelLink>();
                rrBoundaries = new List<IFeature>();

                WriteSobekFixedFiles();
                SendConceptsToModelAndUpdateLinks();
                SendWasteWaterTreatmentPlantsToModelAndUpdateLinks();
                SendBoundariesToModel();
                SendLinksToModel();

                OutputController.Initialize(model, FillOutputCoveragesWithFeatures);

                Writer.AddIniOption("Options", "MinFillingPercentage", model.MinimumFillingStoragePercentage.ToString(CultureInfo.InvariantCulture));
                Writer.AddIniOption("OutputOptions", "OutputAtTimestep", (model.OutputTimeStep.TotalSeconds/model.TimeStep.TotalSeconds).ToString(CultureInfo.InvariantCulture));
                Writer.AddIniOption("TimeSettings", "EvaporationFromHrs", model.EvaporationStartActivePeriod.ToString(CultureInfo.InvariantCulture));
                Writer.AddIniOption("TimeSettings", "EvaporationToHrs", model.EvaporationEndActivePeriod.ToString(CultureInfo.InvariantCulture));

                Writer.AddIniOption("Options", "RestartIn", model.UseRestart ? "1" : "0");
                Writer.AddIniOption("Options", "RestartOut", model.WriteRestart ? "1" : "0");
                Writer.AddIniOption("Options", "RestartFileEachTimestep", model.UseSaveStateTimeRange ? "1" : "0");
                Writer.AddIniOption("Options", "GreenhouseYear", model.GreenhouseYear.ToString());

                SetOutputOptions(model.OutputSettings);

                if (model.CapSim)
                {
                    Writer.AddIniOption("Options", "UnsaturatedZone", "1");
                    Writer.AddIniOption("Options", "InitCapsimOption", ((int)model.CapSimInitOption).ToString());
                    Writer.AddIniOption("Options", "CapsimPerCropArea", ((int)model.CapSimCropAreaOption).ToString());
                }
                else
                {
                    Writer.AddIniOption("Options", "UnsaturatedZone", "0");
                }
                Writer.AddIniOption("Options", "ControlModule", model.IsRunningParallelWithFlow() ? "-1" : "0");

                Writer.SetSimulationTimesAndGenerateIniFile(RRModelEngineHelper.DateToInt(model.StartTime),
                    RRModelEngineHelper.TimeToInt(model.StartTime),
                    RRModelEngineHelper.DateToInt(model.StopTime),
                    RRModelEngineHelper.TimeToInt(model.StopTime),
                    (int)model.TimeStep.TotalSeconds,
                    (int)model.OutputTimeStep.TotalSeconds);
                
                Writer.EvaporationMeteoDataSource = meteoDataSourceConverter.ToIOMeteoDataSource(model.Evaporation.SelectedMeteoDataSource);

                Writer.WriteFiles(); 
                return true;
            });
            
            return true;            
        }

        private void SetOutputOptions(RainfallRunoffOutputSettingData outputSettings)
        {
            AggregationOptions aggregationOption = outputSettings.AggregationOption;
            Writer.AddIniOption("OutputOptions", "OutputAtTimestepOption", ((int)aggregationOption).ToString());
            
            Writer.AddIniOption("OutputOptions", "OutputRRPaved", outputSettings.IsOutputEnabledForElementSet(ElementSet.PavedElmSet) ? "-1" : "0");
            Writer.AddIniOption("OutputOptions", "OutputRRUnpaved", outputSettings.IsOutputEnabledForElementSet(ElementSet.UnpavedElmSet) ? "-1" : "0");
            Writer.AddIniOption("OutputOptions", "OutputRRGreenhouse", outputSettings.IsOutputEnabledForElementSet(ElementSet.GreenhouseElmSet) ? "-1" : "0");
            Writer.AddIniOption("OutputOptions", "OutputRROpenWater", outputSettings.IsOutputEnabledForElementSet(ElementSet.OpenWaterElmSet) ? "-1" : "0");
            Writer.AddIniOption("OutputOptions", "OutputRRBoundary", outputSettings.IsOutputEnabledForElementSet(ElementSet.BoundaryElmSet) ? "-1" : "0");
            Writer.AddIniOption("OutputOptions", "OutputRRNWRW", outputSettings.IsOutputEnabledForElementSet(ElementSet.NWRWElmSet) ? "-1" : "0");
            Writer.AddIniOption("OutputOptions", "OutputRRWWTP", outputSettings.IsOutputEnabledForElementSet(ElementSet.WWTPElmSet) ? "-1" : "0");
            Writer.AddIniOption("OutputOptions", "OutputRRSacramento", outputSettings.IsOutputEnabledForElementSet(ElementSet.SacramentoElmSet) ? "-1" : "0");
            Writer.AddIniOption("OutputOptions", "OutputRRLinkFlows", outputSettings.IsOutputEnabledForElementSet(ElementSet.LinkElmSet) ? "-1" : "0");

            bool enableRRBalanceOutput = outputSettings.IsOutputEnabledForElementSet(ElementSet.BalanceModelElmSet)
                                         || outputSettings.IsOutputEnabledForElementSet(ElementSet.BalanceNodeElmSet);
            Writer.AddIniOption("OutputOptions", "OutputRRBalance", enableRRBalanceOutput ? "-1" : "0");
        }

        #region Initialize/Execute/Finalize/Cleanup

        public void Cleanup()
        {
            try
            {
               
                if (model.Status != ActivityStatus.Failed)
                {
                    model.OutputFunctions.ForEach(outputFunction => model.ChangeToReadOnlyMapHisFileFunctionStore(outputFunction));
                    model.SetPathsOfFunctionStores(model.ModelController.WorkingDirectory);
                }
            }
            finally
            {
                Writer = null; //reset for now
                OutputController.Cleanup();
                modelDataCache = null;
                catchmentDataLookUp = null;
                boundaryDataLookUp = null;
            }
        }

        #endregion

        #region Working Directory

        private T DoInWorkingDirectory<T>(Func<T> function)
        {
            try
            {
                SwitchToWorkingDirectory();
                return function();
            }
            finally
            {
                RestoreWorkingDirectory();
            }
        }

        private void PrepareWorkingDirectory()
        {
            workingDirectory = GetWorkingDirectoryDelegate();
            if (!Directory.Exists(workingDirectory))
            {
                Directory.CreateDirectory(workingDirectory);
            }

            var hisFiles = new DirectoryInfo(workingDirectory).GetFiles("*.his");
            foreach (var hisFile in hisFiles)
            {
                try
                {
                    FileUtils.DeleteIfExists(hisFile.FullName);
                }
                catch (Exception)
                {
                    log.WarnFormat("Could not remove {0}", hisFile.FullName);
                }
            }

            log.InfoFormat("RR working directory is '{0}'", workingDirectory);
        }

        private void SwitchToWorkingDirectory()
        {
            originalDirectory = Environment.CurrentDirectory;
            Environment.CurrentDirectory = WorkingDirectory;
        }

        private void RestoreWorkingDirectory()
        {
            if (!string.IsNullOrEmpty(originalDirectory))
            {
                Environment.CurrentDirectory = originalDirectory;
            }
        }

        #endregion

        public static ModelLink CreateModelLink(IHydroObject hydroObject, HydroLink explicitWwtpLink = null)
        {
            HydroLink link = explicitWwtpLink ?? hydroObject.Links.FirstOrDefault(l => !(l.Target is WasteWaterTreatmentPlant));
            
            //link inside rr domain (wwtp or runoff boundary)
            if (link != null && (link.Target is WasteWaterTreatmentPlant || link.Target is RunoffBoundary || link.Target is ILateralSource))
            {
                return new ModelLink(link.Name, hydroObject, link, link.Target);
            }

            return CreateModelLinkOutsideRR(hydroObject, link);
        }

        /// <summary>
        /// link outside rr domain (flow), or non-existent: use string iso feature.
        /// </summary>
        /// <param name="hydroObject">hydroObject for creating model link.</param>
        /// <param name="link">Link for the model link.</param>
        /// <returns><see cref="ModelLink"/> outside RR domain.</returns>
        /// <remarks>When no link defined, creates a fake (implicit) link and boundary.</remarks>
        private static ModelLink CreateModelLinkOutsideRR(IHydroObject hydroObject, HydroLink link)
        {
            if (link == null)
            {
                link = new HydroLink(hydroObject, hydroObject)
                {
                    Name = hydroObject + "_link",
                    Geometry = GetFakeLinkGeometry(hydroObject)
                };
            }

            string targetName = hydroObject.Name + RainfallRunoffModel.BoundarySuffix;
            return new ModelLink(link.Name, hydroObject, link, targetName);
        }

        private static ILineString GetFakeLinkGeometry(IHydroObject hydroObject)
        {
            if (hydroObject.Geometry == null)
            {
                return new LineString(new[] {new Coordinate(0, 0), new Coordinate(0, 100)});
            }

            Coordinate coord = hydroObject.LinkingCoordinate;
            return new LineString(new[] {coord, new Coordinate(coord.X, coord.Y + 100)});
        }
    }

    public interface IRainfallRunoffModelController
    {
        double GetWaterLevelAtBoundary(IFeature feature);
    }
}