using DelftTools.Hydro;
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
using DelftTools.Utils.Editing;
using DelftTools.Utils.IO;
using DeltaShell.Plugins.DelftModels.HydroModel.ValueConverters;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading;
using DelftTools.Hydro.Helpers;
using DelftTools.Utils.Validation;
using DeltaShell.Dimr;
using DeltaShell.Plugins.DelftModels.HydroModel.Export;
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
    public partial class HydroModel : TimeDependentModelBase, IHydroModel, ICompositeActivity, IFileBased, IModelMerge, IDisposable
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(HydroModel));
        
        private const string HydroRegionTag = "RootHydroRegion";
        
        #region Fields and properties

        private IEventedList<IActivity> activities;
        private IEventedList<ICompositeActivity> workflows;

        private bool creating;
        private bool updating; // updating property values which can trigger other changes

        private bool migrating; // moving existing models into an integrated model (as opposed to adding a new model)

        private bool overrideStartTime;
        private bool overrideStopTime;
        private bool overrideTimeStep;

        private ICompositeActivity currentWorkflow;
        private CompositeHydroModelWorkFlowData currentWorkFlowData;

        public virtual bool ReadOnly { get; set; }

        public virtual bool Migrating
        {
            get { return migrating; }
            set { migrating = value; }
        }

        #endregion

        #region Constructor and dispose

        public HydroModel()
        {
            Sobek2CompareTest = false;
            Name = "Integrated Model";

            creating = true;

            Workflows = new EventedList<ICompositeActivity>();

            Activities = new EventedList<IActivity>();
            RefreshDefaultModelWorkflows();

            // add hydro region
            var hydroRegion = new HydroRegion();
            ((INotifyCollectionChange)hydroRegion).CollectionChanged += OnHydroRegionCollectionChanged;
            DataItems.Add(new DataItem { ValueType = typeof(HydroRegion), Value = hydroRegion, Name = "Region", Tag = HydroRegionTag });

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
            foreach (var activity in Activities.GetActivitiesOfType<IDisposable>())
            {
                activity.Dispose();
            }
        }

        #endregion

        #region Time settings

        public virtual bool OverrideStartTime
        {
            get { return overrideStartTime; }
            set
            {
                overrideStartTime = value;
                SetChildActivitiesStartTime();
            }
        }

        public virtual bool OverrideStopTime
        {
            get { return overrideStopTime; }
            set
            {
                overrideStopTime = value;
                SetChildActivitiesStopTime();
            }
        }

        public virtual bool OverrideTimeStep
        {
            get { return overrideTimeStep; }
            set
            {
                overrideTimeStep = value;
                SetChildActivitiesTimeStep();
            }
        }

        [NoNotifyPropertyChange]
        public override DateTime StartTime
        {
            get { return base.StartTime; }
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
            get { return base.StopTime; }
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
            get { return base.TimeStep; }
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
                return
                    GetProgressTextCore(Activities.GetActivitiesOfType<TimeDependentModelBase>().Average(m => m.ProgressPercentage));
            }
        }

        [EditAction]
        private void SetChildActivitiesStartTime()
        {
            if (!OverrideStartTime)
            {
                return;
            }

            updating = true;
            foreach (var subModel in Activities.GetActivitiesOfType<ITimeDependentModel>().Where(a => a.StartTime != StartTime))
            {
                subModel.StartTime = StartTime;
            }
            updating = false;
        }

        [EditAction]
        private void SetChildActivitiesStopTime()
        {
            if (!OverrideStopTime)
            {
                return;
            }

            updating = true;
            foreach (var subModel in Activities.GetActivitiesOfType<ITimeDependentModel>().Where(a => a.StopTime != StopTime))
            {
                subModel.StopTime = StopTime;
            }
            updating = false;
        }

        [EditAction]
        private void SetChildActivitiesTimeStep()
        {
            if (!OverrideTimeStep)
            {
                return;
            }

            updating = true;
            foreach (var subModel in Activities.GetActivitiesOfType<ITimeDependentModel>().Where(a => a.TimeStep != TimeStep))
            {
                subModel.TimeStep = TimeStep;
            }
            updating = false;
        }

        #endregion

        #region Activities

        public virtual IEventedList<IActivity> Activities
        {
            get { return activities; }
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
                    foreach (var model in activities.OfType<IModel>())
                    {
                        model.Owner = this;
                    }

                    activities.CollectionChanging += ActivitiesCollectionChanging;
                    activities.CollectionChanged += ActivitiesCollectionChanged;
                    ((INotifyPropertyChanged)activities).PropertyChanged += OnActivitiesPropertyChanged;
                }
            }
        }

        public override bool OutputIsEmpty
        {
            get
            {
                if (activities == null) return true; // Can be set null

                // Only ModelBase has syncing logic about output:
                return activities.OfType<ModelBase>().All(m => m.OutputIsEmpty);
            }
            protected set
            {
                // Do nothing, property completely depends on children
            }
        }

        [EditAction]
        private void OnActivitiesPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (updating)
            {
                return;
            }

            var parameter = sender as Parameter;

            if (parameter == null || e.PropertyName != "Value")
            {
                return;
            }

            // underlying model parameter has been changed - reset override flag

            var timeDependentModels = Activities.OfType<ITimeDependentModel>();
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

        [EditAction]
        private void OnActivitiesCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            // activity was removed / added
            if (Equals(sender, activities))
            {
                var model = e.GetRemovedOrAddedItem() as IModel;
                var timeDependentModel = e.GetRemovedOrAddedItem() as ITimeDependentModel;
                switch (e.Action)
                {
                    case NotifyCollectionChangedAction.Add:
                        if (model != null)
                        {
                            model.Owner = this;

                            if (migrating)
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
                        }

                        break;
                    default:
                        throw new NotImplementedException();
                }

                RefreshDefaultModelWorkflows();
                //SetDefaultActivityName((IActivity)e.Item);
                CurrentWorkflow = Workflows.FirstOrDefault();
            }
        }

        public virtual IEnumerable<IModel> Models
        {
            get { return Activities.OfType<IModel>(); }
        }

        #endregion

        #region Workflows

        public virtual IEventedList<ICompositeActivity> Workflows
        {
            get { return workflows; }
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
            var activity = e.GetRemovedOrAddedItem() as ICompositeActivity;
            if (activity != null)
            {
                activity.GetActivitiesOfType<IHydroModelWorkFlow>().ForEach(lfc => HydroModelWorkFlowHandler(lfc, e.Action));
            }

            var disposable = e.GetRemovedOrAddedItem() as IDisposable;
            if (disposable != null && e.Action == NotifyCollectionChangedAction.Remove)
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
            get { return currentWorkflow; }
            set
            {
                if (currentWorkflow == value)
                    return;

                if (currentWorkflow != null)
                {
                    var coupler = currentWorkflow as Iterative1D2DCoupler;
                    if (coupler != null)
                    {
                        var dimrModel = coupler.Flow2DModel as IDimrModel;
                        if (dimrModel != null)
                        {
                            dimrModel.SetVar(new[] {false}, Iterative1D2DCoupler.IsPartOf1D2DModelPropertyName);
                            dimrModel.SetVar(new[] {false}, Iterative1D2DCoupler.DisableFlowNodeRenumberingPropertyName);
                        }
                    }
                    currentWorkflow.StatusChanged -= CurrentWorkflowOnStatusChanged;
                }

                currentWorkflow = value;

                if (currentWorkflow != null)
                {
                    currentWorkflow.StatusChanged += CurrentWorkflowOnStatusChanged;
                    
                    var coupler = currentWorkflow as Iterative1D2DCoupler;
                    if (coupler != null)
                    {
                        var dimrModel = coupler.Flow2DModel as IDimrModel;
                        if (dimrModel != null)
                        {
                            dimrModel.SetVar(new[] {true}, Iterative1D2DCoupler.IsPartOf1D2DModelPropertyName);
                            dimrModel.SetVar(new[] {true}, Iterative1D2DCoupler.DisableFlowNodeRenumberingPropertyName);
                        }
                    }
                }

                currentWorkFlowData = null;

                if (!creating)
                    RebuildModelLinks();
            }
        }

        [InvokeRequired]
        private void LogInvalidWorkflow()
        {
            Log.ErrorFormat(Resources.HydroModel_LogErrorsWhenUnsupportedWorkflow_The_workflow___0___is_currently_not_supported_in_DeltaShell, CurrentWorkflow.Name);
        }

        [InvokeRequired]
        private void LogInvalidActivities()
        {
            Log.ErrorFormat(Resources.HydroModel_LogInvalidActivities_The_integrated_model___0___could_not_initialize__Please_check_the_validation_report_, Name);
        }
        
        [EditAction]
        private void CurrentWorkflowOnStatusChanged(object sender,
            ActivityStatusChangedEventArgs activityStatusChangedEventArgs)
        {
            Status = currentWorkflow.Status;
        }

        [EditAction]
        private void RebuildModelLinks()
        {
            builder.RebuildAllModelLinks(this);
        }

        //nhibernate
        [NoNotifyPropertyChange]
        protected virtual int CurrentWorkflowIndex
        {
            get { return Workflows.IndexOf(CurrentWorkflow); }
            [EditAction]
            set
            {
                if (Workflows.Count <= 1) //only default workflow: can happen on load
                    RefreshDefaultModelWorkflows();
                if (value > -1 && value < Workflows.Count)
                    CurrentWorkflow = Workflows[value];
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
                        HydroModelWorkFlowDataLookUp = GetActivitiesLevelIndices(CurrentWorkflow, new int[] { })
                            .Where(t => t.Item1 != null)
                            .ToDictionary(t => t.Item1, t => t.Item2)
                    };
                }
                return currentWorkFlowData;
            }
            set
            {
                currentWorkFlowData = value;

                if (currentWorkFlowData == null) return;
                currentWorkFlowData.TryRestoreData(CurrentWorkflow);
            }
        }

        private IEnumerable<System.Tuple<IHydroModelWorkFlowData, IList<int>>> GetActivitiesLevelIndices(IActivity activity, int[] indicesList)
        {
            var hydroModelWorkFlow = activity as IHydroModelWorkFlow;
            if (hydroModelWorkFlow != null)
            {
                yield return new System.Tuple<IHydroModelWorkFlowData, IList<int>>(hydroModelWorkFlow.Data, new List<int>(indicesList));
            }

            var compositeActivity = activity as ICompositeActivity;
            if (compositeActivity == null) yield break;

            for (int i = 0; i < compositeActivity.Activities.Count; i++)
            {
                var newLevelIndices = new List<int>(indicesList.Concat(new[] { i })).ToArray();

                foreach (var tuple in GetActivitiesLevelIndices(compositeActivity.Activities[i], newLevelIndices))
                {
                    yield return tuple;
                }
            }
        }

        #endregion

        #region Model run

        private bool DoDimrRun()
        {
            if (currentWorkflow == null) return false;
            return currentWorkflow.Activities.GetActivitiesOfType<IModel>().Count() ==
                   currentWorkflow.Activities.GetActivitiesOfType<IDimrModel>().Count();
        }

        protected override void OnExecute()
        {
            if (CurrentWorkflow == null) return;
            if (DoDimrRun())
            {
                try
                {
                    dimrApi.Update(dimrApi.TimeStep.TotalSeconds);
                    CurrentTime = dimrApi.CurrentTime;
                    currentWorkflow.Activities.GetActivitiesOfType<IDimrModel>().ForEach(m => m.CurrentTime = CurrentTime);
                    OnProgressChanged();
                    if (dimrApi.StopTime.Subtract(dimrApi.CurrentTime).TotalSeconds <= 0)
                    {
                        Status = ActivityStatus.Done;
                    }
                    if (Status != ActivityStatus.Done)
                    {
                        currentWorkflow.Activities.GetActivitiesOfType<IDimrStateAwareModel>().ForEach(m => m.WriteRestartFiles());
                    }

                }
                catch (Exception e)
                {
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
            get { return base.Status; }
            protected set
            {
                base.Status = value;
                if (!DoDimrRun()) return;
                Activities.GetActivitiesOfType<IDimrModel>().ForEach(dimrModel => dimrModel.Status = Status);
            }
        }
        
        protected override void OnInitialize()
        {
            if (CurrentWorkflow == null) return;
            if (!WorkFlowTypeValidatorFactory.GetWorkFlowTypeValidator(CurrentWorkflow).Valid())
            {
                LogInvalidWorkflow();
                Status = ActivityStatus.Failed;
                return;
            }

            var validationReport = new HydroModelValidator().Validate(this);
            if (validationReport.ErrorCount > 0)
            {
                LogInvalidActivities();
                Status = ActivityStatus.Failed;
                return;
            }

            if (DoDimrRun())
            {
                PrepareWorkDirectory();

                var dimrModels = CurrentWorkflow.Activities.GetActivitiesOfType<IDimrModel>()
                    .Plus(CurrentWorkflow as IDimrModel).Where(dm => dm != null).ToList();

                dimrModels.ForEach(m =>
                {
                    m.ExplicitWorkingDirectory = Path.Combine(ExplicitWorkingDirectory, m.DirectoryName);
                    m.RunsInIntegratedModel = true;
                    m.DisconnectOutput();
                });

                var kernelDirectories = GetKernelDirectories(dimrModels);
                if (kernelDirectories == null) return;

                var dHydroConfigXmlExporter = new DHydroConfigXmlExporter();
                if (!dHydroConfigXmlExporter.Export(this, Path.Combine(ExplicitWorkingDirectory, "dimr.xml")))
                {
                    Status = ActivityStatus.Failed;
                    return;
                }

                // use message buffering when running in Main thread 
                // dimrExe (using process.WaitForExit) blocks main thread, so messages (that are marshaled to MainThread) that 
                // are send from async output handlers will cause deadlock
                var runningInMainThread = Thread.CurrentThread.ManagedThreadId ==
                                          HydroModelApplicationPlugin.MainThreadId;
                dimrApi = DimrApiFactory.CreateNew( /*runningInMainThread*/ /*runRemote:false*/);

                if (dimrApi == null)
                {
                    throw new ArgumentNullException("Could not load the Dimr api.");
                }

                //run dimr

                dimrApi.KernelDirs = kernelDirectories;
                dimrApi.DimrRefDate = StartTime;
                dimrApi.Initialize(Path.Combine(ExplicitWorkingDirectory, "dimr.xml"));
                CurrentTime = StartTime;

                currentWorkflow.Activities.GetActivitiesOfType<IDimrModel>().ForEach(m => m.CurrentTime = CurrentTime);
                OnProgressChanged();
                currentWorkflow.Activities.GetActivitiesOfType<IDimrStateAwareModel>().ForEach(m => m.PrepareRestart());
            }
            else
            {
                CurrentWorkflow.Initialize();
            }
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

        private void PrepareWorkDirectory()
        {
            var workDirectory = ExplicitWorkingDirectory;
            if (ExplicitWorkingDirectory == null)
            {
                var dirPath = Path.GetDirectoryName(path) ?? Environment.CurrentDirectory;
                workDirectory = Path.Combine(dirPath, Name.Replace(' ', '_') + "_output");
                ExplicitWorkingDirectory = workDirectory;
            }

            FileUtils.CreateDirectoryIfNotExists(workDirectory, !Sobek2CompareTest);
        }

        public virtual ValidationReport Validate()
        {
            return new HydroModelValidator().Validate(this);
        }

        public virtual bool Sobek2CompareTest { get; set; }
        
        protected override void OnProgressChanged()
        {
            if (dimrApi != null) dimrApi.ProcessMessages();
            base.OnProgressChanged();
        }
        protected override void OnCancel()
        {
            if (CurrentWorkflow == null) return;
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
                var validPath = ExplicitWorkingDirectory;
                if (!Directory.Exists(validPath)) return;
                
                var dimrModels = currentWorkflow.GetActivitiesOfType<IDimrModel>().ToList();
                dimrModels.ForEach(m => m.RunsInIntegratedModel = false);

                foreach (var dimrModel in dimrModels)
                {
                    var outputDirectory = Path.Combine(validPath, dimrModel.DirectoryName);
                    dimrModel.ConnectOutput(outputDirectory);
                }
                var CurrentWorkflowIsDimr = CurrentWorkflow as IDimrModel;
                if (CurrentWorkflowIsDimr != null)
                {
                    CurrentWorkflowIsDimr.ConnectOutput(validPath);
                    CurrentWorkflowIsDimr.RunsInIntegratedModel = false;
                }
                DimrRunner.ConnectDimrRunLogFile(this);
            }
            else
            {
                if (CurrentWorkflow != null) CurrentWorkflow.Cleanup();
            }
        }

        protected override void OnFinish()
        {
            if (DoDimrRun() && dimrApi != null)
            {
                dimrApi.Finish();
                currentWorkflow.Activities.GetActivitiesOfType<IDimrStateAwareModel>().ForEach(m => m.FinalizeRestart());
            }
            else
            {
                if (CurrentWorkflow != null) CurrentWorkflow.Finish();
            }
        }

        #endregion

        #region Region

        public virtual IHydroRegion Region
        {
            get { return (IHydroRegion)GetDataItemByTag(HydroRegionTag).Value; }
            set
            {
                var hydroRegion = Region;
                if (hydroRegion != null)
                {
                    ((INotifyCollectionChange)hydroRegion).CollectionChanged -= OnHydroRegionCollectionChanged;
                }

                var regionDataItem = GetDataItemByTag(HydroRegionTag);
                regionDataItem.Value = value;

                if (value != null)
                {
                    ((INotifyCollectionChange)value).CollectionChanged += OnHydroRegionCollectionChanged;
                }

                AddChildRegionDataItems(regionDataItem);
            }
        }

        public virtual Type SupportedRegionType { get { return typeof(HydroRegion); } }

        [EditAction]
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
                var parentRegionDataItem = GetDataItemByValue(subRegion.Parent);
                AddChildRegionDataItems(parentRegionDataItem);
                return;
            }

            if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                var parentRegionDataItem = GetDataItemByValue(subRegion.Parent);
                var regionDataItem = parentRegionDataItem.Children.FirstOrDefault(di => Equals(di.Value, subRegion));
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
            foreach (var subRegion in region.SubRegions.OfType<IHydroRegion>())
            {
                var existingDataItem =
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

        static HydroModelBuilder builder = new HydroModelBuilder();
        private IDimrApi dimrApi;
        
        [EditAction]
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
            if (!CanMerge(sourceModel)) return validationReport;
            var sourceHydroModel = sourceModel as HydroModel;
            if (sourceHydroModel == null) return validationReport;
            foreach (var destinationChildModel in Activities.OfType<IModelMerge>())
            {
                foreach (var sourceChildModel in sourceHydroModel.Activities.OfType<IModelMerge>())
                {
                    if (destinationChildModel.CanMerge(sourceChildModel))
                    {
                        var childModelValReport = destinationChildModel.ValidateMerge(sourceChildModel);
                        if (childModelValReport != null)
                        {
                            validationReports.Add(childModelValReport);
                        }
                    }
                }

            }
            return new ValidationReport("HydroModel Merge", validationReports);
        }

        private void ProcessModelMergeWithDependencies(HydroModel sourceHydroModel, Action<IModelMerge, IModelMerge, Dictionary<IModelMerge, IModelMerge>> mergeAction = null)
        {
            mergeAction = mergeAction ?? ((d, s, lu) => { });

            var modelsToMerge = sourceHydroModel.Activities.OfType<IModelMerge>().ToList();
            var modelToMergeLookup = modelsToMerge.ToDictionary(m => m, m => Activities.OfType<IModelMerge>().First(a => a.CanMerge(m)));

            var mergedModels = new List<IModelMerge>();
            var mergeDictionary = new Dictionary<IModelMerge, IModelMerge>();
            while (modelsToMerge.Count != 0)
            {
                var previousmergedModelsCount = mergedModels.Count;
                var copyModelsToMerge = new List<IModelMerge>(modelsToMerge);
                foreach (var modelToMerge in copyModelsToMerge)
                {
                    if (modelToMerge.DependendModels.Except(mergedModels).Any()) continue;

                    var desModel = modelToMergeLookup[modelToMerge];

                    mergeAction(desModel, modelToMerge, mergeDictionary);

                    modelsToMerge.Remove(modelToMerge);
                    mergedModels.Add(modelToMerge);
                    mergeDictionary.Add(modelToMerge, desModel);
                }

                if (mergedModels.Count == previousmergedModelsCount)
                {
                    var message =
                        string.Format("While merging or checking dependent submodels of model {0} something went wrong. It appears a dependent subModel couldn't be merged.",
                        sourceHydroModel.Name);
                    throw new Exception(message);
                }
            }
        }

        public virtual bool Merge(IModelMerge sourceModel, IDictionary<IModelMerge, IModelMerge> mergedDependendModelsLookup)
        {
            if (!CanMerge(sourceModel)) return false;

            var sourceHydroModel = sourceModel as HydroModel;
            if (sourceHydroModel == null) return false;

            var clone_sourceHydroModel = sourceHydroModel.DeepClone() as HydroModel;
            if (clone_sourceHydroModel == null) return false;

            using (clone_sourceHydroModel)
            {
                ProcessModelMergeWithDependencies(clone_sourceHydroModel, (desModel, modelToMerge, mergedDependendChildModelsLookup) => desModel.Merge(modelToMerge, mergedDependendChildModelsLookup));
            }

            return true;
        }

        public virtual bool CanMerge(object sourceModel)
        {
            var sourceHydroModel = sourceModel as HydroModel;
            if (sourceHydroModel == null) return false;

            // Of all source model check if they can be merged with at least 1 model of the destination child models
            var sourceChildModels = sourceHydroModel.Activities.OfType<IModelMerge>();
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

        public virtual IEnumerable<IModelMerge> DependendModels { get { yield break; } }

        #endregion

        #region Other

        protected override void OnClearOutput()
        {
            base.OnClearOutput();

            var models = Activities.OfType<IModel>()
                .Plus(currentWorkflow as IModel)
                .Where(m => m != null);

            foreach (var model in models)
            {
                model.ClearOutput();
            }
        }

        public override IEnumerable<object> GetDirectChildren()
        {
            var objects = base.GetDirectChildren().Concat(Activities);
            foreach (var o in objects)
            {
                yield return o;
            }

            var hydroModelWorkFlow = CurrentWorkflow as IHydroModelWorkFlow;
            if (hydroModelWorkFlow == null || hydroModelWorkFlow.Data == null) yield break;

            foreach (var dataItem in hydroModelWorkFlow.Data.OutputDataItems)
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

            RewireHydroRegionValueConverters(this, clonedHydroModel);

            clonedHydroModel.creating = true;
            clonedHydroModel.RefreshDefaultModelWorkflows();
            if (CurrentWorkflow != null)
            {
                clonedHydroModel.CurrentWorkflow = clonedHydroModel.Workflows[Workflows.IndexOf(CurrentWorkflow)];
            }
            clonedHydroModel.creating = false;

            return clonedHydroModel;
        }

        private void RewireHydroRegionValueConverters(HydroModel hydroModel, HydroModel clonedHydroModel)
        {
            var sourceItems = hydroModel.GetAllItemsRecursive().ToList();
            var clonedItems = clonedHydroModel.GetAllItemsRecursive().ToList();

            var sourceDataItems = sourceItems.OfType<IDataItem>().Where(di => di.ValueConverter is IHydroRegionValueConverter).ToList();
            var clonedDataItems = clonedItems.OfType<IDataItem>().Where(di => di.ValueConverter is IHydroRegionValueConverter).ToList();

            if (sourceDataItems.Count != clonedDataItems.Count)
            {
                throw new InvalidOperationException("DataItems with HydroRegion value converters count does not match after clone");
            }

            var sourceRegions = sourceItems.OfType<IHydroRegion>().ToList();
            var clonedRegions = clonedItems.OfType<IHydroRegion>().ToList();

            if (sourceRegions.Count != clonedRegions.Count)
            {
                throw new InvalidOperationException("Number of regions does not match after clone");
            }

            for (int i = 0; i < sourceDataItems.Count; i++)
            {
                var source = sourceDataItems[i];
                var clone = clonedDataItems[i];

                var sourceRegion = ((IHydroRegionValueConverter)source.ValueConverter).HydroRegion;
                var indexInSource = sourceRegions.IndexOf(sourceRegion);

                var clonedConverter = ((IHydroRegionValueConverter)clone.ValueConverter);
                clonedConverter.HydroRegion = clonedRegions[indexInSource];
            }
        }

        public override bool IsLinkAllowed(IDataItem source, IDataItem target)
        {
            // allow linking of region to any of the sub-models
            var sourceRegion = source.Value as IHydroRegion;
            if (sourceRegion != null && Region.AllRegions.Contains(sourceRegion) && target.Owner is IActivity && Activities.Contains((IActivity)target.Owner))
            {
                return true;
            }

            return base.IsLinkAllowed(source, target);
        }


        #endregion

        public bool CopyFromWorkingDirectory { get; } = false;
    }
}