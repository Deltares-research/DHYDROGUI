using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.Structures;
using DelftTools.Shell.Core;
using DelftTools.Shell.Core.Extensions;
using DelftTools.Shell.Core.Workflow;
using DelftTools.Utils.Editing;
using DeltaShell.Plugins.DelftModels.HydroModel;
using DeltaShell.Plugins.DelftModels.RainfallRunoff;
using DeltaShell.Plugins.DelftModels.RealTimeControl;
using DeltaShell.Plugins.FMSuite.FlowFM;
using log4net;

namespace DeltaShell.Plugins.ImportExport.SobekNetwork
{
    public class SobekHydroModelImporter : IFileImporter, IPartialSobekImporter
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(SobekHydroModelImporter));

        private const string RtcWorkflowName = "RTC";
        private const string FlowFMWorkflowName = "FlowFM";
        private const string RRWorkflowName = "RR"; 

        public bool enableWaqOutput;
        public bool useRR;
        public bool useRTC;
        public bool useFm = true;

        private bool shouldCancel;
        private IPartialSobekImporter importer;

        // Used by reflection / importer, do not remove
        public SobekHydroModelImporter(): this(true)
        {

        }

        public SobekHydroModelImporter(bool useRR, bool useRTC = true, bool useFm = true)
        {
            this.useFm = useFm;
            this.useRR = useRR;
            this.useRTC = useRTC;
        }

        # region IFileImporter

        public string Name
        {
            get { return "Sobek 2 Model"; }
        }
        public string Description { get { return Name; } }
        public string Category
        {
            get { return "RHU"; }
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
                yield return typeof(WaterFlowFMModel);
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

        public bool OpenViewAfterImport { get { return false; } }

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
            TargetObject = target ?? CreateHydroModel();
            var targetIsEmptyProject = target as Project;
            if (targetIsEmptyProject != null && !targetIsEmptyProject.RootFolder.GetAllModelsRecursive().Any())
            {
                TargetObject = CreateHydroModel();
                //targetIsEmptyProject.BeginEdit(new DefaultEditAction("Import Sobek 2 model into Project"));
                //targetIsEmptyProject.RootFolder.Add(CreateHydroModel());
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

                SyncModelTimes();

                if (ShouldCancel)
                {
                    return null;
                }

                return TargetObject;
            }
            finally
            {
                importer = null; // make sure we keep no more references to the partial importers
                //targetIsEmptyProject?.EndEdit();
            }
        }


        # endregion

        # region IPartialSobekImporter

        public string PathSobek { get; set; }

        public string DisplayName
        {
            get { return null; }
        }

        public object TargetObject
        {
            get { return targetObject; }
            set
            {
                targetObjectWasSetExternal = true;
                targetObject = value;
            }
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

        public void Import()
        {
            PartialSobekImporter.Import();

            // In case an RTC model is present, but doesn't contain any control groups, the RTC model is removed. 
            var hydroModel = TargetObject as HydroModel;
            if (hydroModel == null)
            {
                var targetObjectIsProject = TargetObject as Project;
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
        }

        public bool IsActive { get; set; }

        public bool IsVisible { get; set; }

        public Action<IPartialSobekImporter> AfterImport { get; set; }

        public Action<IPartialSobekImporter> BeforeImport { get; set; }

        # endregion

        private void SyncModelTimes()
        {
            var hydroModel = TargetObject as HydroModel;
            if (hydroModel == null) return;

            var timeDependentModel = hydroModel.Activities.OfType<ITimeDependentModel>().FirstOrDefault();
            if (timeDependentModel != null)
            {
                // be careful: this overwrites the times of other models
                hydroModel.StartTime = timeDependentModel.StartTime;
                hydroModel.StopTime = timeDependentModel.StopTime;
            }
        }

        private void RebuildImporter()
        {
            if (!targetObjectWasSetExternal)
                targetObject = CreateHydroModel();

            // Build using the selected models
            var previousImporter = importer;
            importer = PartialSobekImporterBuilder.BuildPartialSobekImporter(PathSobek, CreateHydroModel()); //build on clean hydro model instance (not the target object)
            
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

                imp.TargetObject = TargetObject; // update the target object

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
                        ProgressChanged(DisplayName, totalSteps, totalSteps);
                };
            }
        }

        private int currentStep;
        private object targetObject;
        private bool targetObjectWasSetExternal;

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

            if (!useFm)
            {
                var fm = hydroModel.Activities.First(m => m is WaterFlowFMModel);
                hydroModel.Activities.Remove(fm);
            }

            if (!useRR)
            {
                var rr = hydroModel.Activities.First(m => m is RainfallRunoffModel);
                hydroModel.Activities.Remove(rr);
            }

            if (!useRTC)
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
                    parallelActivities.Add(RtcWorkflowName);
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
            if (description == "RR" && useRR)
            {
                return RRWorkflowName;
            }
            if (description.StartsWith("CF") && useFm)  // CF is old Flow1D, need to map on FM
            {
                return FlowFMWorkflowName;
            }
            if (description.StartsWith("RTC") && useRTC)
            {
                return RtcWorkflowName;
            }
            return null; 
        }

    }
}