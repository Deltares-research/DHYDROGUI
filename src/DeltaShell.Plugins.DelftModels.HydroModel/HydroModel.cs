using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.Helpers;
using DelftTools.Shell.Core;
using DelftTools.Shell.Core.Extensions;
using DelftTools.Shell.Core.Workflow;
using DelftTools.Shell.Core.Workflow.DataItems;
using DelftTools.Shell.Core.Workflow.DataItems.ValueConverters;
using DelftTools.Units;
using DelftTools.Utils;
using DelftTools.Utils.Aop;
using DelftTools.Utils.Collections;
using DelftTools.Utils.Collections.Generic;
using DelftTools.Utils.IO;
using DelftTools.Utils.Validation;
using Deltares.Infrastructure.API.Guards;
using DeltaShell.Dimr;
using DeltaShell.NGHS.Common;
using DeltaShell.Plugins.DelftModels.HydroModel.Export;
using DeltaShell.Plugins.DelftModels.HydroModel.Import;
using DeltaShell.Plugins.DelftModels.HydroModel.Properties;
using DeltaShell.Plugins.DelftModels.HydroModel.Validation;
using GeoAPI.Extensions.Feature;
using log4net;

namespace DeltaShell.Plugins.DelftModels.HydroModel
{
    /// <summary>
    /// Composite hydro model which is able to simulate its <see cref="Region"/> or parts of it using child models.
    /// </summary>
    [Entity(FireOnCollectionChange = false)]
    public partial class HydroModel : TimeDependentModelBase,
                                      IHydroModel,
                                      ICompositeActivity,
                                      IFileBased,
                                      IModelMerge,
                                      IDisposable
    {
        private const string HydroRegionTag = "RootHydroRegion";
        private static readonly ILog Log = LogManager.GetLogger(typeof(HydroModel));

        /// <summary>
        /// The file context during import and export of this <see cref="HydroModel"/>. 
        /// </summary>
        public virtual HydroModelFileContext FileContext { get; } = new HydroModelFileContext(new FileHierarchyResolver(new FileSystem()));

        #region Fields and properties

        private IEventedList<IActivity> activities;
        private IEventedList<ICompositeActivity> workflows;

        private bool creating;
        private bool updating; // updating property values which can trigger other changes

        private bool overrideStartTime;
        private bool overrideStopTime;
        private bool overrideTimeStep;

        private ICompositeActivity currentWorkflow;
        private CompositeHydroModelWorkFlowData currentWorkFlowData;

        public virtual bool ReadOnly { get; set; }

        /// <summary>
        /// Make a copy of the file if it is located in the DeltaShell working directory
        /// </summary>
        public virtual bool CopyFromWorkingDirectory { get; }

        /// <summary>
        /// Gets the configuration exporter used for this model.
        /// </summary>
        public virtual DHydroConfigXmlExporter HydroModelExporter { get; }

        public override bool CanRun => Models.Any() && Models.Any(m => m.CanRun);

        #endregion

        #region Constructor and dispose

        public HydroModel()
        {
            Name = "Integrated Model";

            creating = true;

            Workflows = new EventedList<ICompositeActivity>();

            HydroModelExporter = new DHydroConfigXmlExporter();
            
            Activities = new EventedList<IActivity>();
            RefreshDefaultModelWorkflows();

            // add hydro region
            var hydroRegion = new HydroRegion();
            ((INotifyCollectionChange)hydroRegion).CollectionChanged += OnHydroRegionCollectionChanged;
            DataItems.Add(new DataItem
            {
                ValueType = typeof(HydroRegion),
                Value = hydroRegion,
                Name = "Region",
                Tag = HydroRegionTag
            });

            creating = false;

            OverrideStartTime = true;

            OverrideStopTime = true;

            OverrideTimeStep = true;

            // for triggering nhibernate storage of unmapped part:
            ((INotifyPropertyChanged)this).PropertyChanged += (s, e) => MarkDirty();
            ((INotifyCollectionChanged)this).CollectionChanged += (s, e) => MarkDirty();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                foreach (IDisposable activity in Activities.GetActivitiesOfType<IDisposable>())
                {
                    activity.Dispose();
                }

                dimrApi?.Dispose();
            }
        }

        #endregion

        #region Working Directory

        private Func<string> workingDirectoryPathFunc = () => DefaultModelSettings.DefaultDeltaShellWorkingDirectory;

        /// <summary>
        /// Func for retrieving the current working directory set in the framework.
        /// </summary>
        /// <exception cref="ArgumentNullException">
        /// Thrown when value is null.
        /// </exception>
        public virtual Func<string> WorkingDirectoryPathFunc
        {
            get => workingDirectoryPathFunc;
            set
            {
                Ensure.NotNull(value, nameof(value));
                workingDirectoryPathFunc = value;
            }
        }

        /// <summary>
        /// Property for retrieving the current working directory set in the framework
        /// and adding subfolder with model name.
        /// </summary>
        public virtual string WorkingDirectoryPath => Path.Combine(WorkingDirectoryPathFunc(), Name);

        #endregion

        #region Time settings

        public virtual bool OverrideStartTime
        {
            get => overrideStartTime;
            set
            {
                overrideStartTime = value;
                SetChildActivitiesStartTime();
            }
        }

        public virtual bool OverrideStopTime
        {
            get => overrideStopTime;
            set
            {
                overrideStopTime = value;
                SetChildActivitiesStopTime();
            }
        }

        public virtual bool OverrideTimeStep
        {
            get => overrideTimeStep;
            set
            {
                overrideTimeStep = value;
                SetChildActivitiesTimeStep();
            }
        }

        [NoNotifyPropertyChange]
        public override DateTime StartTime
        {
            get => base.StartTime;
            set
            {
                this.BeginEdit("Change start time to: " + value);
                base.StartTime = value;
                SetChildActivitiesStartTime();
                EndEdit();
            }
        }

        [NoNotifyPropertyChange]
        public override DateTime StopTime
        {
            get => base.StopTime;
            set
            {
                this.BeginEdit("Change stop time to: " + value);
                base.StopTime = value;
                SetChildActivitiesStopTime();
                EndEdit();
            }
        }

        [NoNotifyPropertyChange]
        public override TimeSpan TimeStep
        {
            get => base.TimeStep;
            set
            {
                this.BeginEdit("Change time step to: " + value);
                base.TimeStep = value;
                SetChildActivitiesTimeStep();
                EndEdit();
            }
        }

        public override string ProgressText
        {
            get
            {
                // Force reset when model is being reinitialized
                if (Status == ActivityStatus.Initializing || Status == ActivityStatus.None)
                {
                    return GetProgressTextCore(0);
                }

                TimeDependentModelBase[] timeDependentModelBases = Activities.GetActivitiesOfType<TimeDependentModelBase>().ToArray();
                return timeDependentModelBases.Any()
                           ? GetProgressTextCore(timeDependentModelBases.Average(m => m.ProgressPercentage))
                           : GetProgressTextCore(0);
            }
        }

        private void SetChildActivitiesStartTime()
        {
            if (!OverrideStartTime)
            {
                return;
            }

            updating = true;
            foreach (ITimeDependentModel subModel in Activities.GetActivitiesOfType<ITimeDependentModel>().Where(a => a.StartTime != StartTime))
            {
                subModel.StartTime = StartTime;
            }

            updating = false;
        }

        private void SetChildActivitiesStopTime()
        {
            if (!OverrideStopTime)
            {
                return;
            }

            updating = true;
            foreach (ITimeDependentModel subModel in Activities.GetActivitiesOfType<ITimeDependentModel>().Where(a => a.StopTime != StopTime))
            {
                subModel.StopTime = StopTime;
            }

            updating = false;
        }

        private void SetChildActivitiesTimeStep()
        {
            if (!OverrideTimeStep)
            {
                return;
            }

            updating = true;
            foreach (ITimeDependentModel subModel in Activities.GetActivitiesOfType<ITimeDependentModel>().Where(a => a.TimeStep != TimeStep))
            {
                subModel.TimeStep = TimeStep;
            }

            updating = false;
        }

        #endregion

        #region Activities

        public virtual IEventedList<IActivity> Activities
        {
            get => activities;
            protected set
            {
                if (activities != null)
                {
                    activities.CollectionChanging -= ActivitiesCollectionChanging;
                    activities.CollectionChanged -= ActivitiesCollectionChanged;
                    ((INotifyPropertyChanged)activities).PropertyChanged -= OnActivitiesPropertyChanged;
                }

                activities = value;

                if (activities != null)
                {
                    foreach (IModel model in activities.OfType<IModel>())
                    {
                        model.Owner = this;
                    }

                    activities.CollectionChanging += ActivitiesCollectionChanging;
                    activities.CollectionChanged += ActivitiesCollectionChanged;
                    ((INotifyPropertyChanged)activities).PropertyChanged += OnActivitiesPropertyChanged;
                }
            }
        }

        private void OnActivitiesPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (updating)
            {
                return;
            }

            // Do not rebuild links if there is just one Activity present in this model.
            if (sender is IHydFileModel && e.PropertyName == "Name" && activities.Count > 1)
            {
                RebuildModelLinks();
            }

            var parameter = sender as Parameter;
            if (parameter == null || e.PropertyName != "Value")
            {
                return;
            }

            // underlying model parameter has been changed - reset override flag
            IEnumerable<ITimeDependentModel> timeDependentModels = Activities.OfType<ITimeDependentModel>();
            switch (parameter.Name)
            {
                case StartTimeName:
                    if (timeDependentModels.Any(m => m.StartTime != StartTime))
                    {
                        OverrideStartTime = false;
                    }

                    StartTime = timeDependentModels.Select(m => m.StartTime).Min();
                    break;
                case StopTimeName:
                    if (timeDependentModels.Any(m => m.StopTime != StopTime))
                    {
                        OverrideStopTime = false;
                    }

                    StopTime = timeDependentModels.Select(m => m.StopTime).Max();
                    break;
                case TimeStepName:
                    if (timeDependentModels.Any(m => m.TimeStep != TimeStep))
                    {
                        OverrideTimeStep = false;
                    }

                    TimeStep = timeDependentModels.Select(m => m.TimeStep).Min();
                    break;
            }
        }

        private void ActivitiesCollectionChanging(object sender, NotifyCollectionChangingEventArgs e)
        {
            BubbleCollectionChangingEvent(sender, e);
        }

        /// <summary>
        /// Updates default workflow.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ActivitiesCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            OnActivitiesCollectionChanged(sender, e);
            BubbleCollectionChangedEvent(sender, e);
        }

        private void OnActivitiesCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            // activity was removed / added
            if (Equals(sender, activities))
            {
                object removedOrAddedItem = e.GetRemovedOrAddedItem();
                var model = removedOrAddedItem as IModel;
                var timeDependentModel = removedOrAddedItem as ITimeDependentModel;
                switch (e.Action)
                {
                    case NotifyCollectionChangedAction.Add:
                        if (model != null)
                        {
                            model.Owner = this;

                            if (IsEditing && (CurrentEditAction is MovingModelAction ||
                                              CurrentEditAction is ImportingFullModelAction))
                            {
                                // rebuild links and keep related data structure
                                RebuildModelLinks();
                                AutoAddRequiredLinks(model, true);
                            }
                            else
                            {
                                // rebuild links only
                                AutoAddRequiredLinks(model);
                            }
                        }

                        if (timeDependentModel != null)
                        {
                            if (activities.Count == 1)
                            {
                                // first activity, take hydromodel times from child model
                                StartTime = timeDependentModel.StartTime;
                                StopTime = timeDependentModel.StopTime;
                                TimeStep = timeDependentModel.TimeStep;
                            }
                            else
                            {
                                // Set the updating flag to true while updating the submodel,
                                // such that these changes will not update the hydro model
                                updating = true;
                                timeDependentModel.StartTime = StartTime;
                                timeDependentModel.StopTime = StopTime;
                                timeDependentModel.TimeStep = TimeStep;
                                updating = false;
                            }
                        }

                        break;
                    case NotifyCollectionChangedAction.Remove:
                        if (model != null)
                        {
                            model.DisconnectExternalDataItems();
                            model.Owner = null;
                            RemoveDimrModelFromFileContext(model);
                        }

                        break;
                    default:
                        throw new NotImplementedException();
                }

                RefreshDefaultModelWorkflows();
                CurrentWorkflow = Workflows.FirstOrDefault();
            }
        }

        private void RemoveDimrModelFromFileContext(IModel model)
        {
            if (model is IDimrModel dimrModel)
            {
                FileContext.RemoveModel(dimrModel);
            }
        }

        public virtual IEnumerable<IModel> Models => Activities.OfType<IModel>();

        #endregion

        #region Workflows

        public virtual IEventedList<ICompositeActivity> Workflows
        {
            get => workflows;
            set
            {
                if (workflows != null)
                {
                    workflows.CollectionChanging -= WorkflowsOnCollectionChanging;
                    workflows.CollectionChanged -= WorkflowsOnCollectionChanged;
                }

                workflows = value;

                if (workflows != null)
                {
                    workflows.CollectionChanging += WorkflowsOnCollectionChanging;
                    workflows.CollectionChanged += WorkflowsOnCollectionChanged;
                }
            }
        }

        private void WorkflowsOnCollectionChanging(object sender, NotifyCollectionChangingEventArgs e)
        {
            BubbleCollectionChangingEvent(sender, e);
        }

        private void WorkflowsOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            object removedOrAddedItem = e.GetRemovedOrAddedItem();
            if (removedOrAddedItem is ICompositeActivity activity)
            {
                activity.GetActivitiesOfType<IHydroModelWorkFlow>().ForEach(lfc => HydroModelWorkFlowHandler(lfc, e.Action));
            }

            if (removedOrAddedItem is IDisposable disposable &&
                e.Action == NotifyCollectionChangedAction.Remove)
            {
                disposable.Dispose();
            }

            BubbleCollectionChangedEvent(sender, e);
        }

        private void HydroModelWorkFlowHandler(IHydroModelWorkFlow hydroModelWorkFlow, NotifyCollectionChangedAction action)
        {
            if (action == NotifyCollectionChangedAction.Add)
            {
                hydroModelWorkFlow.HydroModel = this;
            }

            if (action == NotifyCollectionChangedAction.Remove)
            {
                hydroModelWorkFlow.HydroModel = null;
            }
        }

        [Aggregation]
        public virtual ICompositeActivity CurrentWorkflow
        {
            get => currentWorkflow;
            set
            {
                if (currentWorkflow == value)
                {
                    return;
                }

                if (currentWorkflow != null)
                {
                    currentWorkflow.StatusChanged -= CurrentWorkflowOnStatusChanged;
                }

                currentWorkflow = value;

                if (currentWorkflow != null)
                {
                    currentWorkflow.StatusChanged += CurrentWorkflowOnStatusChanged;
                }

                currentWorkFlowData = null;

                if (!creating)
                {
                    RebuildModelLinks();
                }
            }
        }

        [InvokeRequired]
        private void LogInvalidActivities()
        {
            Log.ErrorFormat(Resources.HydroModel_LogInvalidActivities_The_integrated_model___0___could_not_initialize__Please_check_the_validation_report_, Name);
        }

        private void CurrentWorkflowOnStatusChanged(object sender,
                                                    ActivityStatusChangedEventArgs activityStatusChangedEventArgs)
        {
            Status = currentWorkflow.Status;
        }

        private void RebuildModelLinks()
        {
            builder.RebuildAllModelLinks(this);
        }

        //nhibernate
        [NoNotifyPropertyChange]
        protected virtual int CurrentWorkflowIndex
        {
            get => Workflows.IndexOf(CurrentWorkflow);
            set
            {
                if (Workflows.Count <= 1) //only default workflow: can happen on load
                {
                    RefreshDefaultModelWorkflows();
                }

                if (value > -1 && value < Workflows.Count)
                {
                    CurrentWorkflow = Workflows[value];
                }
            }
        }

        /// <summary>
        /// Data of the <see cref="CurrentWorkflow"/>
        /// </summary>
        public virtual CompositeHydroModelWorkFlowData CurrentWorkFlowData
        {
            get
            {
                if (currentWorkFlowData == null)
                {
                    currentWorkFlowData = new CompositeHydroModelWorkFlowData
                    {
                        HydroModelWorkFlowDataLookUp = GetActivitiesLevelIndices(CurrentWorkflow, new int[]
                                                                                     {})
                                                       .Where(t => t.Item1 != null)
                                                       .ToDictionary(t => t.Item1, t => t.Item2)
                    };
                }

                return currentWorkFlowData;
            }
            set
            {
                currentWorkFlowData = value;

                if (currentWorkFlowData == null)
                {
                    return;
                }

                currentWorkFlowData.TryRestoreData(CurrentWorkflow);
            }
        }

        private IEnumerable<System.Tuple<IHydroModelWorkFlowData, IList<int>>> GetActivitiesLevelIndices(IActivity activity, int[] indicesList)
        {
            if (activity is IHydroModelWorkFlow hydroModelWorkFlow)
            {
                yield return new System.Tuple<IHydroModelWorkFlowData, IList<int>>(hydroModelWorkFlow.Data, new List<int>(indicesList));
            }

            if (!(activity is ICompositeActivity compositeActivity))
            {
                yield break;
            }

            for (var i = 0; i < compositeActivity.Activities.Count; i++)
            {
                int[] newLevelIndices = new List<int>(indicesList.Concat(new[]
                {
                    i
                })).ToArray();

                foreach (System.Tuple<IHydroModelWorkFlowData, IList<int>> tuple in GetActivitiesLevelIndices(compositeActivity.Activities[i], newLevelIndices))
                {
                    yield return tuple;
                }
            }
        }

        #endregion

        #region Model run

        private bool DoDimrRun()
        {
            if (currentWorkflow == null)
            {
                return false;
            }

            return currentWorkflow.Activities.GetActivitiesOfType<IModel>().Count() ==
                   currentWorkflow.Activities.GetActivitiesOfType<IDimrModel>().Count();
        }

        protected override void OnReset()
        {
            base.OnReset();
            if (DoDimrRun())
            {
                currentWorkflow.Activities
                               .GetActivitiesOfType<IDimrModel>()
                               .ForEach(a => a.ResetActivity());
            }
            else
            {
                CurrentWorkflow.ResetActivity();
            }
        }

        protected override void OnExecute()
        {
            if (CurrentWorkflow == null)
            {
                return;
            }

            if (DoDimrRun())
            {
                try
                {
                    int returnCode = dimrApi.Update(dimrApi.TimeStep.TotalSeconds);
                    if (returnCode != 0)
                    {
                        throw new DimrErrorCodeException(Status, returnCode);
                    }

                    CurrentTime = dimrApi.CurrentTime;
                    currentWorkflow.Activities.GetActivitiesOfType<IDimrModel>()
                                   .ForEach(m => m.CurrentTime = CurrentTime);
                    OnProgressChanged();
                    if (dimrApi.StopTime.Subtract(dimrApi.CurrentTime).TotalSeconds <= 0)
                    {
                        Status = ActivityStatus.Done;
                    }
                }
                catch (Exception e)
                {
                    Log.ErrorFormat(e.Message);
                    Console.WriteLine(e.Message);
                    Status = ActivityStatus.Failed;
                }
            }
            else
            {
                CurrentWorkflow.Execute();
            }
        }

        public override ActivityStatus Status
        {
            get => base.Status;
            protected set
            {
                base.Status = value;
                if (!DoDimrRun())
                {
                    return;
                }

                Activities.GetActivitiesOfType<IDimrModel>().ForEach(dimrModel => dimrModel.Status = Status);
            }
        }

        protected override void OnInitialize()
        {
            OnProgressChanged();

            ValidationReport validationReport = Validate();
            if (validationReport.ErrorCount > 0)
            {
                LogInvalidActivities();
                Status = ActivityStatus.Failed;
                return;
            }

            if (DoDimrRun())
            {
                try
                {
                    List<IDimrModel> dimrModels = CurrentWorkflow.Activities.GetActivitiesOfType<IDimrModel>()
                                                                 .Plus(CurrentWorkflow as IDimrModel).Where(dm => dm != null)
                                                                 .ToList();
                    var fileExceptions = new List<string>();

                    foreach (IDimrModel m in dimrModels)
                    {
                        m.RunsInIntegratedModel = true;
                        m.DisconnectOutput();
                        fileExceptions.AddRange(m.IgnoredFilePathsWhenCleaningWorkingDirectory);
                    }

                    string kernelDirectories = GetKernelDirectories(dimrModels);
                    if (kernelDirectories == null)
                    {
                        return;
                    }

                    PrepareWorkingDirectory(fileExceptions);

                    if (!ExportHydroModel())
                    {
                        Status = ActivityStatus.Failed;
                        return;
                    }

                    dimrApi = dimrApiFactory.CreateNew();
                    if (dimrApi == null)
                    {
                        throw new InvalidOperationException("Could not load the Dimr api.");
                    }

                    //run dimr

                    dimrApi.KernelDirs = kernelDirectories;
                    dimrApi.DimrRefDate = StartTime;

                    int returnCode = dimrApi.Initialize(Path.Combine(WorkingDirectoryPath, "dimr.xml"));
                    if (returnCode != 0)
                    {
                        throw new DimrErrorCodeException(Status, returnCode);
                    }

                    CurrentTime = StartTime;

                    currentWorkflow.Activities.GetActivitiesOfType<IDimrModel>()
                                   .ForEach(m => m.CurrentTime = CurrentTime);
                    OnProgressChanged();
                }
                catch (DimrErrorCodeException e)
                {
                    HandleDimrErrorCodeException(e);
                    throw;
                }
            }
            else
            {
                CurrentWorkflow.Initialize();
            }
        }

        private bool ExportHydroModel() =>
            HydroModelExporter.Export(this, Path.Combine(WorkingDirectoryPath, "dimr.xml"));

        private void PrepareWorkingDirectory(IEnumerable<string> fileExceptions)
        {
            if (Directory.Exists(WorkingDirectoryPath))
            {
                FileUtils.ClearDirectory(WorkingDirectoryPath, fileExceptions);
            }
            else
            {
                Directory.CreateDirectory(WorkingDirectoryPath);
            }
        }

        private void HandleDimrErrorCodeException(DimrErrorCodeException e)
        {
            Log.ErrorFormat(e.Message);

            dimrApi?.Dispose();
            dimrApi = null;
        }

        private string GetKernelDirectories(IEnumerable<IDimrModel> dimrModels)
        {
            try
            {
                return string.Join(";", dimrModels.Select(activity => activity.KernelDirectoryLocation).ToArray());
            }
            catch (Exception ex)
            {
                Log.ErrorFormat("Error retrieving kernel directories: {0}", ex.Message);
                return null;
            }
        }

        public virtual ValidationReport Validate()
        {
            return new HydroModelValidator().Validate(this);
        }

        protected override void OnProgressChanged()
        {
            dimrApi?.ProcessMessages();
            base.OnProgressChanged();
        }

        protected override void OnCancel()
        {
            if (CurrentWorkflow == null)
            {
                return;
            }

            if (!DoDimrRun())
            {
                CurrentWorkflow.Cancel();
            }
        }

        protected override void OnCleanup()
        {
            if (dimrApi != null)
            {
                try
                {
                    dimrApi.Dispose();
                    dimrApi = null;
                }
                catch (Exception e)

                {
                    Log.Debug(e.Message);
                }
            }

            if (DoDimrRun())
            {
                string validPath = WorkingDirectoryPath;
                if (!Directory.Exists(validPath))
                {
                    return;
                }

                List<IDimrModel> dimrModels = currentWorkflow.GetActivitiesOfType<IDimrModel>().ToList();
                dimrModels.ForEach(m => m.RunsInIntegratedModel = false);

                foreach (IDimrModel dimrModel in dimrModels)
                {
                    string outputDirectory = Path.Combine(validPath, dimrModel.DimrModelRelativeOutputDirectory);
                    dimrModel.ConnectOutput(outputDirectory);
                }

                var CurrentWorkflowIsDimr = CurrentWorkflow as IDimrModel;
                if (CurrentWorkflowIsDimr != null)
                {
                    CurrentWorkflowIsDimr.ConnectOutput(validPath);
                    CurrentWorkflowIsDimr.RunsInIntegratedModel = false;
                }

                DimrRunHelper.ConnectDimrRunLogFile(this, WorkingDirectoryPath);
                OutputIsEmpty = false;
            }
            else
            {
                CurrentWorkflow?.Cleanup();
            }
        }

        protected override void OnFinish()
        {
            try
            {
                if (dimrApi != null)
                {
                    int returnCode = dimrApi.Finish();
                    if (returnCode != 0)
                    {
                        throw new DimrErrorCodeException(Status, returnCode);
                    }
                }

                if (DoDimrRun())
                {
                    List<IDimrModel> dimrModels = currentWorkflow.GetActivitiesOfType<IDimrModel>().ToList();
                    dimrModels.ForEach(m => m.OnFinishIntegratedModelRun(WorkingDirectoryPath));
                }
                else
                {
                    CurrentWorkflow?.Finish();
                }
            }
            catch (DimrErrorCodeException e)
            {
                HandleDimrErrorCodeException(e);
                throw;
            }
        }

        #endregion

        #region Region

        public virtual IHydroRegion Region
        {
            get
            {
                return (IHydroRegion)GetDataItemByTag(HydroRegionTag).Value;
            }
            set
            {
                IHydroRegion hydroRegion = Region;
                if (hydroRegion != null)
                {
                    ((INotifyCollectionChange)hydroRegion).CollectionChanged -= OnHydroRegionCollectionChanged;
                }

                IDataItem regionDataItem = GetDataItemByTag(HydroRegionTag);
                regionDataItem.Value = value;

                if (value != null)
                {
                    ((INotifyCollectionChange)value).CollectionChanged += OnHydroRegionCollectionChanged;
                }

                AddChildRegionDataItems(regionDataItem);
            }
        }

        private void OnHydroRegionCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            var subRegions = sender as IEventedList<IRegion>;
            var subRegion = e.GetRemovedOrAddedItem() as IHydroRegion;

            if (subRegions == null || subRegion == null || subRegion.Parent == null)
            {
                return;
            }

            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                IDataItem parentRegionDataItem = GetDataItemByValue(subRegion.Parent);
                AddChildRegionDataItems(parentRegionDataItem);
                return;
            }

            if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                IDataItem parentRegionDataItem = GetDataItemByValue(subRegion.Parent);
                IDataItem regionDataItem = parentRegionDataItem.Children.FirstOrDefault(di => Equals(di.Value, subRegion));
                if (regionDataItem != null)
                {
                    parentRegionDataItem.Children.Remove(regionDataItem);
                }

                return;
            }

            throw new NotSupportedException(e.Action + " is not supported on the hydro region collection");
        }

        private static void AddChildRegionDataItems(IDataItem regionDataItem)
        {
            var region = (IHydroRegion)regionDataItem.Value;
            foreach (IHydroRegion subRegion in region.SubRegions.OfType<IHydroRegion>())
            {
                IDataItem existingDataItem =
                    regionDataItem.Children.FirstOrDefault(childDataItem => childDataItem.Value == subRegion);
                if (existingDataItem == null)
                {
                    existingDataItem = CreateDataItemForSubRegion(subRegion, regionDataItem);
                    regionDataItem.Children.Add(existingDataItem);
                }

                AddChildRegionDataItems(existingDataItem);
            }
        }

        private static IDataItem CreateDataItemForSubRegion(IHydroRegion subRegion, IDataItem parent)
        {
            return new DataItem
            {
                Name = subRegion.Name,
                Parent = parent,
                ValueType = typeof(IHydroRegion),
                ValueConverter = new AggregationValueConverter(subRegion),
                Owner = parent.Owner
            };
        }

        #endregion

        #region HydroModelBuilder

        private static readonly HydroModelBuilder builder = new HydroModelBuilder();
        private IDimrApi dimrApi;
        private readonly DimrApiFactory dimrApiFactory = new DimrApiFactory();

        public virtual void RefreshDefaultModelWorkflows()
        {
            builder.RefreshDefaultModelWorkflows(this);
        }

        public virtual void SetDefaultActivityName(IActivity activity)
        {
            builder.SetDefaultActivityName(activity);
        }

        public virtual void AutoAddRequiredLinks(IActivity activity, bool relinking = false)
        {
            builder.AutoAddRequiredLinks(this, activity, relinking);
        }

        public static HydroModel BuildModel(ModelGroup modelGroup)
        {
            return builder.BuildModel(modelGroup);
        }

        public static bool CanBuildModel(ModelGroup modelGroup)
        {
            return builder.CanBuildModel(modelGroup);
        }

        #endregion

        #region IModelMerge

        public virtual ValidationReport ValidateMerge(IModelMerge sourceModel)
        {
            var validationReports = new List<ValidationReport>();
            var validationReport = new ValidationReport("HydroModel Merge", validationReports);
            if (!CanMerge(sourceModel))
            {
                return validationReport;
            }

            var sourceHydroModel = sourceModel as HydroModel;
            if (sourceHydroModel == null)
            {
                return validationReport;
            }

            foreach (IModelMerge destinationChildModel in Activities.OfType<IModelMerge>())
            {
                foreach (IModelMerge sourceChildModel in GetMergeableChildModels(sourceHydroModel, destinationChildModel))
                {
                    ValidationReport childModelValReport = destinationChildModel.ValidateMerge(sourceChildModel);
                    if (childModelValReport != null)
                    {
                        validationReports.Add(childModelValReport);
                    }
                }
            }

            return new ValidationReport("HydroModel Merge", validationReports);
        }

        private static IEnumerable<IModelMerge> GetMergeableChildModels(ICompositeActivity sourceHydroModel, IModelMerge destinationChildModel)
        {
            return sourceHydroModel.Activities.OfType<IModelMerge>().Where(destinationChildModel.CanMerge);
        }

        private void ProcessModelMergeWithDependencies(HydroModel sourceHydroModel, Action<IModelMerge, IModelMerge, Dictionary<IModelMerge, IModelMerge>> mergeAction = null)
        {
            mergeAction = mergeAction ?? ((d, s, lu) => { });

            List<IModelMerge> modelsToMerge = sourceHydroModel.Activities.OfType<IModelMerge>().ToList();
            Dictionary<IModelMerge, IModelMerge> modelToMergeLookup = modelsToMerge.ToDictionary(m => m, m => Activities.OfType<IModelMerge>().First(a => a.CanMerge(m)));

            var mergedModels = new List<IModelMerge>();
            var mergeDictionary = new Dictionary<IModelMerge, IModelMerge>();
            while (modelsToMerge.Count != 0)
            {
                int previousmergedModelsCount = mergedModels.Count;
                var copyModelsToMerge = new List<IModelMerge>(modelsToMerge);
                foreach (IModelMerge modelToMerge in copyModelsToMerge)
                {
                    if (modelToMerge.DependentModels.Except(mergedModels).Any())
                    {
                        continue;
                    }

                    IModelMerge desModel = modelToMergeLookup[modelToMerge];

                    mergeAction(desModel, modelToMerge, mergeDictionary);

                    modelsToMerge.Remove(modelToMerge);
                    mergedModels.Add(modelToMerge);
                    mergeDictionary.Add(modelToMerge, desModel);
                }

                if (mergedModels.Count == previousmergedModelsCount)
                {
                    string message =
                        string.Format("While merging or checking dependent submodels of model {0} something went wrong. It appears a dependent subModel couldn't be merged.",
                                      sourceHydroModel.Name);
                    throw new Exception(message);
                }
            }
        }

        public virtual bool Merge(IModelMerge sourceModel, IDictionary<IModelMerge, IModelMerge> mergedDependentModelsLookup)
        {
            if (!CanMerge(sourceModel))
            {
                return false;
            }

            var sourceHydroModel = sourceModel as HydroModel;
            if (sourceHydroModel == null)
            {
                return false;
            }

            var clone_sourceHydroModel = sourceHydroModel.DeepClone() as HydroModel;
            if (clone_sourceHydroModel == null)
            {
                return false;
            }

            using (clone_sourceHydroModel)
            {
                ProcessModelMergeWithDependencies(clone_sourceHydroModel, (desModel, modelToMerge, mergedDependendChildModelsLookup) => desModel.Merge(modelToMerge, mergedDependendChildModelsLookup));
            }

            return true;
        }

        public virtual bool CanMerge(object sourceModel)
        {
            var sourceHydroModel = sourceModel as HydroModel;
            if (sourceHydroModel == null)
            {
                return false;
            }

            // Of all source model check if they can be merged with at least 1 model of the destination child models
            IEnumerable<IModelMerge> sourceChildModels = sourceHydroModel.Activities.OfType<IModelMerge>();
            if (!sourceChildModels
                    .All(sourceChildModel => Activities.OfType<IModelMerge>() // destinationChildModels
                                                       .Any(destinationModel => destinationModel.CanMerge(sourceChildModel))))
            {
                return false;
            }

            try
            {
                ProcessModelMergeWithDependencies(sourceHydroModel);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public virtual IEnumerable<IModelMerge> DependentModels
        {
            get
            {
                yield break;
            }
        }

        #endregion

        #region Other

        protected override void OnClearOutput()
        {
            Activities.OfType<IModel>().Plus(currentWorkflow as IModel)
                      .Where(m => m != null)
                      .ForEach(m => m.ClearOutput(true));

            RemoveOutputTextDocumentDataItem();
        }

        private void RemoveOutputTextDocumentDataItem()
        {
            IList<IDataItem> textDocumentDataItems = dataItems.Where(di => di.Role.HasFlag(DataItemRole.Output)
                                                                           && di.ValueType == typeof(TextDocument))
                                                              .ToList();

            foreach (IDataItem dataItem in textDocumentDataItems)
            {
                dataItems.Remove(dataItem);
            }
        }

        public override IEnumerable<object> GetDirectChildren()
        {
            IEnumerable<object> objects = base.GetDirectChildren().Concat(Activities);
            foreach (object o in objects)
            {
                yield return o;
            }

            var hydroModelWorkFlow = CurrentWorkflow as IHydroModelWorkFlow;
            if (hydroModelWorkFlow?.Data == null)
            {
                yield break;
            }

            foreach (IDataItem dataItem in hydroModelWorkFlow.Data.OutputDataItems)
            {
                yield return dataItem;
            }
        }

        public override IProjectItem DeepClone()
        {
            var clonedHydroModel = (HydroModel)base.DeepClone();

            clonedHydroModel.Activities = new EventedList<IActivity>(Activities.Select(a => (IActivity)a.DeepClone()));

            clonedHydroModel.RelinkInternalDataItemLinks(this);
            clonedHydroModel.RelinkExternalDataItemLinks(this);

            clonedHydroModel.creating = true;
            clonedHydroModel.RefreshDefaultModelWorkflows();
            if (CurrentWorkflow != null)
            {
                clonedHydroModel.CurrentWorkflow = clonedHydroModel.Workflows[Workflows.IndexOf(CurrentWorkflow)];
            }

            clonedHydroModel.creating = false;

            return clonedHydroModel;
        }

        public override bool IsLinkAllowed(IDataItem source, IDataItem target)
        {
            // allow linking of region to any of the sub-models
            if (source.Value is IHydroRegion sourceRegion &&
                Region.AllRegions.Contains(sourceRegion) &&
                target.Owner is IActivity activity &&
                Activities.Contains(activity))
            {
                return true;
            }

            return base.IsLinkAllowed(source, target);
        }

        #endregion

        protected override void OnInputPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (Status != ActivityStatus.Failed &&
                Status != ActivityStatus.Cleaned &&
                Status != ActivityStatus.Cancelled &&
                Status != ActivityStatus.Finished)
            {
                return;
            }

            MarkOutputOutOfSync();

            foreach (IModel activity in Activities.OfType<IModel>().Plus(currentWorkflow as IModel))
            {
                activity?.MarkOutputOutOfSync();
            }
        }
    }
}