using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.Helpers;
using DelftTools.Hydro.Structures;
using DelftTools.Shell.Core;
using DelftTools.Shell.Core.Extensions;
using DelftTools.Shell.Core.Workflow;
using DelftTools.Utils.Reflection;
using DeltaShell.NGHS.Common;
using DeltaShell.NGHS.Utils.Extensions;
using DeltaShell.Plugins.DelftModels.HydroModel;
using DeltaShell.Plugins.DelftModels.RainfallRunoff;
using DeltaShell.Plugins.DelftModels.RealTimeControl;
using DeltaShell.Plugins.FMSuite.FlowFM;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using DeltaShell.Plugins.ImportExport.Sobek.PartialSobekImporter;
using log4net;

namespace DeltaShell.Plugins.ImportExport.Sobek
{
    public class SobekHydroModelImporter : IFileImporter, IPartialSobekImporter
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(SobekHydroModelImporter));

        private const string RtcWorkflowName = "RTC";
        private const string FlowFMWorkflowName = "FlowFM";
        private const string RRWorkflowName = "RR"; 

        public bool enableWaqOutput;
        private bool shouldCancel;
        private IPartialSobekImporter importer;

        // Used by reflection / importer, do not remove
        private int currentStep;
        private object targetObject;
        private bool targetObjectWasSetExternal;

        public SobekHydroModelImporter(): this(true)
        {

        }

        public SobekHydroModelImporter(bool useRR, bool useRTC = true, bool useFm = true)
        {
            UseFm = useFm;
            UseRR = useRR;
            UseRTC = useRTC;
        }

        public bool UseRR { get; set; }

        public bool UseRTC { get; set; }

        public bool UseFm { get; set; }

        # region IFileImporter

        public string Name
        {
            get { return "Sobek 2 Model"; }
        }

        public string Description
        {
            get { return "Imports a Sobek 2 model"; }
        }

        public string Category
        {
            get { return ProductCategories.OneDTwoDModelImportCategory; }
        }

        public Bitmap Image
        {
            get { return Properties.Resources.sobek; }
        }

        public IEnumerable<Type> SupportedItemTypes
        {
            get
            {
                yield return typeof(HydroModel);
            }
        }

        public bool CanImportOn(object targetObject)
        {
            return true;
        }

        public virtual bool CanImportOnRootLevel
        {
            get { return true; }
        }

        public string FileFilter
        {
            get { return "All Supported Files|network.tp;deftop.1|Sobek 2.1* network files|network.tp|SobekRE network files|deftop.1"; }
        }

        public string TargetDataDirectory { get; set; }

        public bool OpenViewAfterImport { get { return true; } }

        public bool ShouldCancel
        {
            get { return shouldCancel; } 
            set
            {
                shouldCancel = value;

                if (importer != null)
                {
                    importer.ShouldCancel = value;
                }
            }
        }

        public ImportProgressChangedDelegate ProgressChanged { get; set; }

        public object ImportItem(string path, object target = null)
        {
            // Configure the SobekPath of the IPartialSobekImporter part of the importer
            if (!string.IsNullOrEmpty(path))
            {
                PathSobek = Path.GetFullPath(path.Trim());
            }

            // Configure the TargetObject of the IPartialSobekImporter part of the importer
            var targetObjectInternal = target ?? TargetObject;
            var targetIsEmptyProject = targetObjectInternal as Project;
            if (targetIsEmptyProject != null && !targetIsEmptyProject.RootFolder.GetAllModelsRecursive().Any())
            {
                targetObjectInternal = TargetObject;
                targetObject = targetObjectInternal;
                targetObjectWasSetExternal = true;
            }

            IProjectService projectService = Application?.ProjectService;
            if (projectService != null && !projectService.IsProjectOpen)
            {
                projectService.CreateProject();
            }

            if (projectService != null && projectService.Project.RootFolder.GetAllModelsRecursive().Contains(targetObjectInternal))
            {
                projectService.Project.RootFolder.Items.Remove((HydroModel)targetObjectInternal);
            }
            
            if (ShouldCancel)
            {
                return null;
            }

            // Import by using the import logic of the IPartialSobekImporter part of the importer
            try
            {
                // Because the importers form a chain, this call will trigger this particular importer, and all consecutive ones. 
                Import();

                if (ShouldCancel)
                {
                    return null;
                }
                SetImportedSobekFileObjectModelToDHydroDomainObjectModel(targetObjectInternal);

                SyncModelTimes();
                SetDefaultModelSettings();

                if (ShouldCancel)
                {
                    return null;
                }

                return targetObjectInternal;
            }
            finally
            {
                importer = null; // make sure we keep no more references to the partial importers
                if (projectService != null && !projectService.Project.RootFolder.GetAllModelsRecursive().Contains(targetObjectInternal) && target is HydroModel)
                {
                    projectService.Project.RootFolder.Items.Add((HydroModel)targetObjectInternal);
                }
            }
        }

        private void SetDefaultModelSettings()
        {
            var hydroModel = TargetObject as HydroModel;
            if (hydroModel == null) return;

            if (UseFm)
            {
                var waterFlowFmModels = hydroModel.GetActivitiesOfType<WaterFlowFMModel>();
                foreach (WaterFlowFMModel waterFlowFmModel in waterFlowFmModels)
                { 
                    waterFlowFmModel.ModelDefinition.SetModelProperty(KnownProperties.RstInterval, "0");
                }
            }
        }

        # endregion

        # region IPartialSobekImporter

        public string PathSobek { get; set; }

        public string DisplayName
        {
            get { return Name; }
        }

        SobekImporterCategories IPartialSobekImporter.Category { get; } = SobekImporterCategories.IntegratedModel;

        public object TargetObject
        {
            get
            {
                if (targetObject != null)
                {
                    return targetObject;
                }
                targetObject = CreateHydroModel();
                targetObjectWasSetExternal = targetObject != null;
                return targetObject;
            }
            set
            {
                targetObject = value;
                targetObjectWasSetExternal = targetObject != null;
            }
        }

        public void Import()
        {
            PartialSobekImporter.Import();
        }

        public IPartialSobekImporter PartialSobekImporter
        {
            get
            {
                RebuildImporter();

                return importer;
            }
            set { }
        }

        private void SetImportedSobekFileObjectModelToDHydroDomainObjectModel(object targetObjectInternal)
        {
            // In case an RTC model is present, but doesn't contain any control groups, the RTC model is removed. 
            var hydroModel = targetObjectInternal as HydroModel;
            if (hydroModel == null)
            {
                var targetObjectIsProject = targetObjectInternal as Project;
                if (targetObjectIsProject == null)
                    return;
                hydroModel = targetObjectIsProject.RootFolder.GetAllModelsRecursive().OfType<HydroModel>().FirstOrDefault();
            }

            bool rtcFound = false;

            var realTimeControlModel = hydroModel.Activities.OfType<RealTimeControlModel>().FirstOrDefault();
            if (realTimeControlModel != null && realTimeControlModel.ControlGroups.Count == 0)
            {
                hydroModel.Activities.Remove(realTimeControlModel);
            }
            else
            {
                rtcFound = true;
            }

            // Make sure that the workflow is adapted to the one indicated in the SETTINGS.DAT file. 
            AdaptWorkflow(rtcFound);

            var waterFlowFMModel = hydroModel.Activities.OfType<WaterFlowFMModel>().FirstOrDefault();
            if (waterFlowFMModel == null) return;

            waterFlowFMModel.HydFileOutput = enableWaqOutput;
            if (enableWaqOutput)
            {
                log.Warn("Skipped import of waterquality model and enabled hyd file output on waterflow model.");
            }

            waterFlowFMModel.Network?.MakeNamesUnique<ICompositeBranchStructure>();
            hydroModel.CoordinateSystem = waterFlowFMModel.CoordinateSystem;
        }

        public bool IsActive { get; set; }

        public bool IsVisible { get; set; }

        public Action<IPartialSobekImporter> AfterImport { get; set; }

        public Action<IPartialSobekImporter> BeforeImport { get; set; }
        public IApplication Application { get; set; }

        # endregion

        private void SyncModelTimes()
        {
            var hydroModel = TargetObject as HydroModel;
            if (hydroModel == null)
            {
                return;
            }

            ITimeDependentModel[] models = hydroModel.Activities.OfType<ITimeDependentModel>().ToArray();
            if (models.Select(m => m.StartTime).AllEqual() &&
                models.Select(m => m.StopTime).AllEqual())
            {
                hydroModel.OverrideStartTime = true;
                hydroModel.OverrideStopTime = true;
                hydroModel.OverrideTimeStep = true;
            }
            else
            {
                ITimeDependentModel timeDependentModel = models.FirstOrDefault();
                if (timeDependentModel != null)
                {
                    hydroModel.StartTime = timeDependentModel.StartTime;
                    hydroModel.StopTime = timeDependentModel.StopTime;
                }
            }
        }

        private void RebuildImporter()
        {
            if (!targetObjectWasSetExternal)
            {
                targetObject = CreateHydroModel();
                targetObjectWasSetExternal = targetObject != null;
            }

            // Build using the selected models
            var previousImporter = importer;
            importer = PartialSobekImporterBuilder.BuildPartialSobekImporter(PathSobek, targetObject);
            if (UseFm && !GetImporters(importer).Any(i => PartialSobekImporterBuilder.GetWaterFlowFMModelImporters().Any(imp => i.GetType().Implements(imp.GetType()))))
            {
                importer = PartialSobekImporterBuilder.BuildPartialSobekImporter(importer.PathSobek,
                    importer.TargetObject, GetImporters(importer).Reverse().Concat(PartialSobekImporterBuilder.GetWaterFlowFMModelImporters())
                        .Distinct(new ImporterTypeComparer()).ToList());

            }
            if (UseRR && !GetImporters(importer).Any(i => PartialSobekImporterBuilder.GetRainfallRunoffModelImporters().Any(imp => i.GetType().Implements(imp.GetType()))))
            {
                importer = PartialSobekImporterBuilder.BuildPartialSobekImporter(importer.PathSobek,
                    importer.TargetObject,
                    GetImporters(importer).Reverse().Concat(PartialSobekImporterBuilder.GetRainfallRunoffModelImporters())
                        .Distinct(new ImporterTypeComparer()).ToList());

            }
            if (UseRTC && !GetImporters(importer).Any(i => PartialSobekImporterBuilder.GetRealTimeControlModelImporters().All(imp => i.GetType().Implements(imp.GetType()))))
            {
                importer = PartialSobekImporterBuilder.BuildPartialSobekImporter(importer.PathSobek,
                    importer.TargetObject,
                    GetImporters(importer).Reverse().Concat(PartialSobekImporterBuilder.GetRealTimeControlModelImporters())
                        .Distinct(new ImporterTypeComparer()).ToList());
            }
            var importers = GetImporters(importer).Reverse().ToList();

            // Restore IsActive setting:
            var oldImporters = previousImporter != null
                                   ? GetImporters(previousImporter).ToList()
                                   : new List<IPartialSobekImporter>();
            foreach (var partialImporter in importers)
            {
                var matchingImporter = oldImporters.FirstOrDefault(i => i.GetType() == partialImporter.GetType());
                if (matchingImporter != null)
                    partialImporter.IsActive = matchingImporter.IsActive;
            }
            // End restore IsActive..

            var totalSteps = importers.Count(i => i.IsActive);
            currentStep = 1;
            for (var i = 0; i < importers.Count; i++)
            {
                var imp = importers[i];

                imp.TargetObject = targetObject; // update the target object

                imp.BeforeImport = currentImporter =>
                    {
                        if (!imp.IsActive)
                            return;

                        if (ProgressChanged != null)
                        {
                            ProgressChanged(currentImporter.DisplayName, currentStep, totalSteps);
                        }
                    };

                imp.AfterImport = currentImporter =>
                {
                    if (!imp.IsActive)
                        return;

                    currentStep++;

                    if (ProgressChanged == null) return;

                    var nextStartIndex = importers.IndexOf(imp) + 1;
                    var nextImporterIndex = nextStartIndex >= importers.Count
                                                ? -1
                                                : importers.FindIndex(nextStartIndex, im => im.IsActive);

                    if (nextImporterIndex >= 0)
                        ProgressChanged(importers[nextImporterIndex].DisplayName, currentStep, totalSteps);
                    else
                        ProgressChanged(DisplayName + " imported successfully, loading into DeltaShell", totalSteps, totalSteps);
                };
            }
        }

        private IEnumerable<IPartialSobekImporter> GetImporters(IPartialSobekImporter partialImporter)
        {
            while (partialImporter != null)
            {
                yield return partialImporter;
                partialImporter = partialImporter.PartialSobekImporter;
            }
        }

        private HydroModel CreateHydroModel()
        {
            var hydroModelBuilder = new HydroModelBuilder();

            var hydroModel = hydroModelBuilder.BuildModel(ModelGroup.RHUModels);

            if (!UseFm)
            {
                var fm = hydroModel.Activities.First(m => m is WaterFlowFMModel);
                hydroModel.Region.SubRegions.Remove(((WaterFlowFMModel)fm).Network);
                hydroModel.Region.SubRegions.Remove(((WaterFlowFMModel)fm).Area);
                hydroModel.Activities.Remove(fm);
            }

            if (!UseRR)
            {
                var rr = hydroModel.Activities.First(m => m is RainfallRunoffModel);
                hydroModel.Region.SubRegions.Remove(((RainfallRunoffModel) rr).Basin);
                hydroModel.Activities.Remove(rr);
            }

            if (!UseRTC)
            {
                var rtc = hydroModel.Activities.First(m => m is RealTimeControlModel);
                hydroModel.Activities.Remove(rtc);
            }

            return hydroModel;
        }

        private void AdaptWorkflow(bool rtcFound)
        {
            string settingsFilename = Path.Combine(Path.GetDirectoryName(PathSobek), "settings.dat");
            if (!File.Exists(settingsFilename))
            {
                // Some tests do not provide a settings.dat file. 
                return; 
            }

            using (var settingsFile = new StreamReader(settingsFilename))
            {
                var hydroModel = TargetObject as HydroModel;
                if (hydroModel == null)
                {
                    // throw Exception? 
                    return;
                }

                // If there is only one possible workflow, just select that one. This happens when only one model is running standalone. 
                if (hydroModel.Workflows.Count <= 1)
                {
                    return; 
                }

                // In these two variables, a sequential list of parallel activities will be built up. 
                var sequentialActivities = new List<string>();
                var parallelActivities = new List<string>();

                // With this initialisation, finding Task1Module1 leads to appending a new activity to parallelActivity, but not to a replacement of the sequentialActivity. 
                int currentTask = 1;
                int currentModule = 0;

                while (!settingsFile.EndOfStream)
                {
                    String line = settingsFile.ReadLine().Trim();
                    if (line.Contains("Restart"))
                    {
                        break;
                    }

                    if (line.Count() > 12 && line.StartsWith("Task") && line.Substring(5,6) == "Module")
                    {
                        int fileTask = int.Parse(line.Substring(4, 1));
                        int fileModule = int.Parse(line.Substring(11, 1));
                        
                        string activityDescription = line.Substring(line.IndexOf('=') + 1);
                        string activity = FindActivityFromDescription(activityDescription); 
                        if (activity == null)
                        {
                            continue; 
                            // Exception? 
                        }
                        if (activity == "waq")
                        {
                            enableWaqOutput = true;
                            continue;
                        }
                        if (currentTask != fileTask)
                        {
                            // A new step in the sequential activity, so add existing parallel activities to the sequential activities, and start a new group of parallel activities. 

                            // If an RTC group is detected, but not included as parallel to the flow module, add it. 
                            if (parallelActivities.Contains(FlowFMWorkflowName) && !parallelActivities.Contains(RtcWorkflowName) && rtcFound)
                            {
                                parallelActivities.Add(RtcWorkflowName);
                            }

                            if (parallelActivities.Count == 1)
                            {
                                sequentialActivities.Add(parallelActivities[0]);
                            }
                            else if (parallelActivities.Count > 1)
                            {
                                sequentialActivities.Add("(" + String.Join(" + ", parallelActivities) + ")");
                            }
                            
                            parallelActivities.Clear();
                            currentTask = fileTask;

                            parallelActivities.Add(activity);
                            currentModule = fileModule; 
                        }
                        else if (currentTask == fileTask && fileModule != currentModule)
                        {
                            // Add an activity to the current parallel activity. 
                            parallelActivities.Add(activity);
                            currentModule = fileModule; 
                        }
                    }
                }

                // If an RTC group is detected, but not included as parallel to the flow module, add it. 
                if (parallelActivities.Contains(FlowFMWorkflowName) && !parallelActivities.Contains(RtcWorkflowName) && rtcFound)
                {
                    parallelActivities.Insert(parallelActivities.IndexOf(FlowFMWorkflowName),RtcWorkflowName);
                }
                
                // Add remaining items in parallelActivities and construct the string representation of the workflow , e.g.: (rr + flow) + waq
                if (parallelActivities.Count == 1)
                {
                    sequentialActivities.Add(parallelActivities[0]);
                }
                else if (parallelActivities.Count > 1)
                {
                    sequentialActivities.Add("(" + String.Join(" + ", parallelActivities) + ")");
                }
                string workflowDescription = String.Join(" + ", sequentialActivities);
                
                // Assign the current workflow to the one that was constructed. 
                var newWorkflow = hydroModel.Workflows.FirstOrDefault(w => w.ToString().Equals(workflowDescription));
                if (newWorkflow == null)
                {
                    log.Warn("Workflow cannot be applied; first workflow will be selected.");
                    hydroModel.CurrentWorkflow = hydroModel.Workflows.FirstOrDefault();
                }
                else
                {
                    hydroModel.CurrentWorkflow = newWorkflow;
                }
            }
        }

        private string FindActivityFromDescription(string description)
        {
            if (description == "RR" && UseRR)
            {
                return RRWorkflowName;
            }
            if (description.StartsWith("CF") && UseFm)  // CF is old Flow1D, need to map on FM
            {
                return FlowFMWorkflowName;
            }
            if (description.StartsWith("RTC") && UseRTC)
            {
                return RtcWorkflowName;
            }
            return null; 
        }

    }
}