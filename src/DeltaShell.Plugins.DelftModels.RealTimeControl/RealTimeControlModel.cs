using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using BasicModelInterface;
using DelftTools.Functions;
using DelftTools.Hydro;
using DelftTools.Shell.Core;
using DelftTools.Shell.Core.Extensions;
using DelftTools.Shell.Core.Workflow;
using DelftTools.Shell.Core.Workflow.DataItems;
using DelftTools.Shell.Core.Workflow.DataItems.ValueConverters;
using DelftTools.Utils;
using DelftTools.Utils.Aop;
using DelftTools.Utils.Collections;
using DelftTools.Utils.Collections.Generic;
using DelftTools.Utils.Guards;
using DelftTools.Utils.IO;
using DelftTools.Utils.Reflection;
using DelftTools.Utils.Validation;
using DeltaShell.Dimr;
using DeltaShell.NGHS.Common;
using DeltaShell.NGHS.IO;
using DeltaShell.Plugins.CommonTools.TextData;
using DeltaShell.Plugins.DelftModels.HydroModel.Export;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Domain;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Domain.Restart;
using DeltaShell.Plugins.DelftModels.RealTimeControl.IO;
using DeltaShell.Plugins.DelftModels.RealTimeControl.IO.Export;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Properties;
using DeltaShell.Plugins.DelftModels.RealTimeControl.rtc_kernel;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Validation;
using DHYDRO.Common.Logging;
using GeoAPI.Extensions.CoordinateSystems;
using GeoAPI.Extensions.Coverages;
using GeoAPI.Extensions.Feature;
using log4net;
using NetTopologySuite.Extensions.Coverages;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl
{
    /// <summary>
    /// NotifyPropertyChange attribute should not be necessary because base class
    /// already has it applied. Project explorer does not function correctly when left out.
    /// </summary>
    [Entity]
    public class RealTimeControlModel : TimeDependentModelBase, IRealTimeControlModel, IModelMerge, IDisposable, IDimrModel, IControllingModel, IFileBased
    {
        public const string InputPostFix = ".input";
        public const string OutputPostFix = ".output";

        private static readonly ILog Log = LogManager.GetLogger(typeof(RealTimeControlModel));
        private readonly DimrRunner runner;

        private readonly IList<IDataItem> linkedDataItemsOriginalValues;

        private string communicationRtcToFmFileName = "rtc_to_flow.nc";
        private string communicationFmToRtcFileName = "flow_to_rtc.nc";
        private ICoordinateSystem coordinateSystem;
        private RealTimeControlOutputFileFunctionStore outputFileFunctionStore;
        private bool disposed;

        private ICompositeActivity oldOwner;

        private bool cloning;

        private IEventedList<ControlGroup> controlGroups;

        private IEventedList<IModel> internalControlledModelsList;

        private bool suspendUpdateFeatureAndParameter;

        private RealTimeControlRestartFile restartInput;

        public RealTimeControlModel() : this("RTC Model") { }

        public RealTimeControlModel(string name)
            : base(name)
        {
            ControlGroups = new EventedList<ControlGroup>();
            linkedDataItemsOriginalValues = new List<IDataItem>();

            LogLevel = 0;
            FlushLogEveryStep = false;

            LimitMemory = true;

            InternalControlledModelsList = new EventedList<IModel>();

            SaveStateStartTime = StopTime;
            SaveStateStopTime = StopTime;
            SaveStateTimeStep = TimeStep;

            restartInput = new RealTimeControlRestartFile();
            ListOfOutputRestartFiles = new List<RealTimeControlRestartFile>();
            OutputDocuments = new EventedList<ReadOnlyTextFileData>();

            runner = new DimrRunner(this, new DimrApiFactory());
            DimrConfigModelCouplerFactory.CouplerProviders.Add(new RealTimeControlDimrConfigModelCouplerProvider());

            if (outputFileFunctionStore != null)
            {
                ReconnectRtcToFmOutputFile(outputFileFunctionStore.Path);
            }

            SuspendClearOutputOnInputChange = true;
        }

        public virtual RealTimeControlOutputFileFunctionStore OutputFileFunctionStore
        {
            get => outputFileFunctionStore;
            set
            {
                outputFileFunctionStore = value;
                if (outputFileFunctionStore != null)
                {
                    outputFileFunctionStore.CoordinateSystem = CoordinateSystem;
                    outputFileFunctionStore.Features = GetChildDataItemLocationsFromControlledModels(DataItemRole.Output).ToList();
                }
            }
        }

        /// <summary>
        /// Property for storing the last exported paths.
        /// This will be used for determining which files
        /// are input (created by the exporter) and which
        /// files are output during the Finish step of a
        /// run <see cref="OnFinishIntegratedModelRun"/>.
        /// </summary>
        public virtual string[] LastExportedPaths { get; set; } = new string[0];

        public virtual int LogLevel { get; set; }

        //set this to true when running the model..so the output won't be removed during the run
        public virtual bool FlushLogEveryStep { get; set; }

        public virtual ITimeDependentModel TimeProvider
        {
            get
            {
                if (ControlledModels.Any())
                {
                    return (ITimeDependentModel)ControlledModels.First();
                }

                //locally defined
                return null;
            }
        }

        public virtual bool LimitMemory { get; set; }

        public virtual string LastWorkingDirectory { get; protected set; }

        public virtual IEnumerable<IFeatureCoverage> OutputFeatureCoverages =>
            outputFileFunctionStore != null && outputFileFunctionStore.Functions != null
                ? outputFileFunctionStore.Functions.OfType<IFeatureCoverage>()
                : Enumerable.Empty<IFeatureCoverage>();

        /// <summary>
        /// Gets or sets the output text documents.
        /// </summary>
        public virtual IEventedList<ReadOnlyTextFileData> OutputDocuments { get; protected set; }
        
        public override bool CanRun => false;

        [NoNotifyPropertyChange]
        public override DateTime StartTime
        {
            get => TimeProvider?.StartTime ?? base.StartTime;
            set
            {
                if (base.StartTime == value)
                {
                    return;
                }

                if (TimeProvider != null)
                {
                    TimeProvider.StartTime = value;
                }

                base.StartTime = value;
                MarkOutputOutOfSync();
            }
        }

        [NoNotifyPropertyChange]
        public override DateTime StopTime
        {
            get => TimeProvider?.StopTime ?? base.StopTime;
            set
            {
                if (base.StopTime == value)
                {
                    return;
                }

                if (TimeProvider != null)
                {
                    TimeProvider.StopTime = value;
                }

                base.StopTime = value;
                MarkOutputOutOfSync();
            }
        }

        [NoNotifyPropertyChange]
        public override TimeSpan TimeStep
        {
            get => TimeProvider?.TimeStep ?? base.TimeStep;
            set
            {
                if (base.TimeStep == value)
                {
                    return;
                }

                if (TimeProvider != null)
                {
                    TimeProvider.TimeStep = value;
                }

                base.TimeStep = value;
                MarkOutputOutOfSync();
            }
        }

        [NoNotifyPropertyChange]
        [Aggregation]
        public override object Owner
        {
            get => base.Owner;
            set
            {
                base.Owner = value;
                ResubscribeToOwner();
            }
        }

        public virtual IEventedList<ControlGroup> ControlGroups
        {
            get => controlGroups;
            set
            {
                if (controlGroups != null)
                {
                    controlGroups.CollectionChanged -= ControlGroupsCollectionChanged;
                    ((INotifyPropertyChange)controlGroups).PropertyChanged -= SetOutputOutOfSync;
                    ((INotifyPropertyChanging)controlGroups).PropertyChanging -= ControlGroupsPropertyChanging;
                    ((INotifyPropertyChange)controlGroups).PropertyChanged -= ControlGroupsPropertyChanged;
                }

                controlGroups = value;

                if (controlGroups != null)
                {
                    controlGroups.CollectionChanged += ControlGroupsCollectionChanged;
                    ((INotifyPropertyChange)controlGroups).PropertyChanged += SetOutputOutOfSync;
                    ((INotifyPropertyChanging)controlGroups).PropertyChanging += ControlGroupsPropertyChanging;
                    ((INotifyPropertyChange)controlGroups).PropertyChanged += ControlGroupsPropertyChanged;
                }
            }
        }

        public virtual ICoordinateSystem CoordinateSystem
        {
            get => coordinateSystem;
            set
            {
                coordinateSystem = value;
                if (outputFileFunctionStore != null)
                {
                    outputFileFunctionStore.CoordinateSystem = coordinateSystem;
                }
            }
        }

        public virtual IEnumerable<IModel> ControlledModels => internalControlledModelsList;

        public override string KernelVersions
        {
            get
            {
                if (!File.Exists(DimrApiDataSet.RtcToolsDllPath))
                {
                    return "";
                }

                return "Kernel: " + RealTimeControlModelDll.RTCTOOLS_DLL_NAME + "  " + FileVersionInfo.GetVersionInfo(DimrApiDataSet.RtcToolsDllPath).FileVersion;
            }
        }

        public virtual void RefreshInitialState()
        {
            //#$*(# dataitems #$*&#@(
            //we revert the output/data item to its original state here
            foreach (ControlGroup controlGroup in ControlGroups)
            {
                foreach (Output output in controlGroup.Outputs)
                {
                    IDataItem outputDataItem = GetDataItemByValue(output);
                    if (outputDataItem != null && outputDataItem.LinkedBy.Count > 0)
                    {
                        output.Value = (double)outputDataItem.LinkedBy[0].Value;
                    }
                }
            }
        }

        public virtual void SetTimeLagHydraulicRulesToTimeSteps(IEnumerable<ControlGroup> controlGroupsToUpdate, TimeSpan timeStep)
        {
            foreach (HydraulicRule r in controlGroupsToUpdate.SelectMany(controlGroup => controlGroup.Rules.OfType<HydraulicRule>()))
            {
                r.SetTimeLagToTimeSteps(timeStep);
            }
        }

        /// <exception cref="NotSupportedException">
        /// When a <see cref="DataItem"/> (either in this model or it's child-models) is
        /// unlinked and the <see cref="DataItem.Value"/> either does not inherit from <see cref="ICloneable"/>, is not null, or is
        /// not a value type.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// When attempting to perform deep clone a <see cref="DataItemSet"/> (either in this model or it's child-models) for which
        /// a <see cref="IDataItem"/>s <see cref="IDataItem.Owner"/> is not the data item set.
        /// </exception>
        public override IProjectItem DeepClone()
        {
            var clonedModel = new RealTimeControlModel { Name = Name };
            // with rewiring of links between models the origin is changed as well as the clone.
            cloning = true;
            suspendUpdateFeatureAndParameter = true;

            clonedModel.cloning = true;
            clonedModel.OutputOutOfSync = OutputOutOfSync;
            clonedModel.LimitMemory = LimitMemory;

            clonedModel.DataItems.Clear(); // re-clone all data items
            foreach (IDataItem dataItem in DataItems)
            {
                clonedModel.DataItems.Add((IDataItem)dataItem.DeepClone());
            }

            // add control groups from the cloned data items, otherwise they are cloned twice
            foreach (ControlGroup controlGroup in ControlGroups)
            {
                IDataItem controlGroupDataItem = GetDataItemByValue(controlGroup);
                int controlGroupDataItemIndex = DataItems.IndexOf(controlGroupDataItem);
                IDataItem controlGroupDataItemClone = clonedModel.DataItems[controlGroupDataItemIndex];

                // restore links to Inputs / Outputs in child data items
                List<object> controlGroupDataItemObjects = controlGroupDataItem.GetAllItemsRecursive().ToList();
                List<object> controlGroupDataItemCloneObjects = controlGroupDataItemClone.GetAllItemsRecursive().ToList();

                foreach (IDataItem childDataItem in controlGroupDataItemClone.Children.Where(di => di.ValueConverter is PropertyValueConverter))
                {
                    var propertyValueConverterClone = (PropertyValueConverter)childDataItem.ValueConverter;
                    var propertyValueConverter = (PropertyValueConverter)controlGroupDataItemObjects[controlGroupDataItemCloneObjects.IndexOf(propertyValueConverterClone)];

                    int originalValueIndex = controlGroupDataItemObjects.IndexOf(propertyValueConverter.OriginalValue);
                    propertyValueConverterClone.OriginalValue = controlGroupDataItemCloneObjects[originalValueIndex];
                }

                var clonedControlGroup = (ControlGroup)controlGroupDataItemClone.Value;
                clonedControlGroup.Name = controlGroup.Name;
                clonedModel.ControlGroups.Add(clonedControlGroup);
            }

            cloning = false;
            suspendUpdateFeatureAndParameter = false;

            clonedModel.RelinkInternalDataItemLinks(this); // should reconnect all data items

            clonedModel.cloning = false;

            if (outputFileFunctionStore != null && File.Exists(outputFileFunctionStore.Path))
            {
                clonedModel.OutputFileFunctionStore = new RealTimeControlOutputFileFunctionStore { Path = outputFileFunctionStore.Path };
            }

            return clonedModel;
        }

        IEnumerable<IDataItem> ICoupledModel.GetDataItemsUsedForCouplingModel(DataItemRole role)
        {
            return AllDataItems.Where(di => di.Role == role);
        }

        public virtual string GetUpToDateDataItemName(string oldDataItemName) => oldDataItemName;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public override bool IsLinkAllowed(IDataItem source, IDataItem target)
        {
            return false;
        }

        protected virtual IList<ExplicitValueConverterLookupItem> explicitValueConverterLookupItems { get; set; }

        protected virtual IEventedList<IModel> InternalControlledModelsList
        {
            get => internalControlledModelsList;
            set
            {
                if (internalControlledModelsList != null)
                {
                    ((INotifyPropertyChanged)internalControlledModelsList).PropertyChanged -= ModelsPropertyChanged;
                    internalControlledModelsList.CollectionChanged -= ControlledModelsCollectionChanged;
                }

                internalControlledModelsList = value;

                if (internalControlledModelsList != null)
                {
                    ((INotifyPropertyChanged)internalControlledModelsList).PropertyChanged += ModelsPropertyChanged;
                    internalControlledModelsList.CollectionChanged += ControlledModelsCollectionChanged;
                }
            }
        }

        protected override void OnInputPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Value" || e.PropertyName == "ConvertedValue")
            {
                return; //shouldn't trigger clearing of output (happens when flow does final execute step after rtc has finished)
            }

            if (Status == ActivityStatus.Failed || Status == ActivityStatus.Cleaned ||
                Status == ActivityStatus.Cancelled || Status == ActivityStatus.Finished)
            {
                MarkOutputOutOfSync();
            }
        }

        protected override void OnDataItemLinking(object sender, LinkingUnlinkingEventArgs<IDataItem> e)
        {
            base.OnDataItemLinking(sender, e);

            if (e.Source.ValueConverter?.OriginalValue is Output)
            {
                e.UseValueFromTarget = true; // makes sure that initial value used by Output is set
            }
        }

        protected override void OnDataItemAdded(IDataItem item)
        {
            base.OnDataItemAdded(item);
            UpdateFeatureAndParameter(item);
        }

        protected override void OnDataItemLinked(object sender, LinkedUnlinkedEventArgs<IDataItem> e)
        {
            base.OnDataItemLinked(sender, e);

            // update feature
            if (e.Source.ValueConverter?.OriginalValue is ConnectionPoint)
            {
                UpdateFeatureAndParameter(e.Source);
            }
            else if (e.Target.ValueConverter?.OriginalValue is ConnectionPoint)
            {
                UpdateFeatureAndParameter(e.Target);
            }
        }

        protected override void OnDataItemUnlinked(object sender, LinkedUnlinkedEventArgs<IDataItem> e)
        {
            base.OnDataItemUnlinked(sender, e);

            // update feature
            ConnectionPoint connection = null;
            if (e.Source.ValueConverter?.OriginalValue is ConnectionPoint connectionPoint)
            {
                connection = connectionPoint;
            }
            else if (e.Target.ValueConverter?.OriginalValue is ConnectionPoint connectionPoint2)
            {
                connection = connectionPoint2;
            }

            if (connection != null)
            {
                lastRelinkedFeature = e.Relinking ? connection.Feature : null;
                connection.Feature = null;
                connection.ParameterName = string.Empty;
                connection.UnitName = string.Empty;
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
            {
                return;
            }
            // Ensure all stores are closed

            if (disposing)
            {
                IEnumerable<IFileBased> fileStores = AllDataItems
                                                     .Where(di => di.LinkedTo == null &&
                                                                  di.ValueType.Implements(typeof(IFunction)))
                                                     .Select(di => di.Value).OfType<IFunction>()
                                                     .Select(nc => nc.Store).OfType<IFileBased>();

                foreach (IFileBased fileStore in fileStores)
                {
                    fileStore.Close();
                }

                runner?.Dispose();
            }

            disposed = true;
        }

        // This is no edit action...
        private void ResubscribeToOwner()
        {
            if (oldOwner != null)
            {
                oldOwner.Activities.CollectionChanged -= OwnerModelsCollectionChanged;
                InternalControlledModelsList.Clear();
                oldOwner = null;
            }

            oldOwner = Owner as ICompositeActivity;
            if (oldOwner != null)
            {
                oldOwner.Activities.CollectionChanged += OwnerModelsCollectionChanged;
                foreach (IModel model in oldOwner.Activities.OfType<IModel>())
                {
                    if (model is RealTimeControlModel)
                    {
                        continue;
                    }

                    InternalControlledModelsList.Add(model);
                }
            }
        }

        private void OwnerModelsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (!(e.GetRemovedOrAddedItem() is IModel model) || model is RealTimeControlModel)
            {
                return;
            }

            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    InternalControlledModelsList.Add(model);
                    break;
                case NotifyCollectionChangedAction.Remove:
                    InternalControlledModelsList.Remove(model);
                    OnRemoveModel();
                    break;
                default:
                    throw new InvalidEnumArgumentException(nameof(e.Action), (int)e.Action, typeof(NotifyCollectionChangeAction));
            }
        }

        private void OnRemoveModel()
        {
            ClearOutput();
        }

        private void ConnectionPointsCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            // add/remove data items for control groups and their inputs/outputs
            var connectionPoint = (ConnectionPoint)e.GetRemovedOrAddedItem();
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    IDataItem controlGroupDataItem = DataItems.FirstOrDefault(
                        di =>
                        {
                            var controlGroup = di.Value as ControlGroup;
                            return controlGroup != null && controlGroup.Inputs.Cast<ConnectionPoint>().Concat(controlGroup.Outputs).Contains(connectionPoint);
                        });

                    if (controlGroupDataItem != null)
                    {
                        AddConnectionDataItem(controlGroupDataItem, connectionPoint, connectionPoint is Input ? DataItemRole.Input : DataItemRole.Output);
                    }

                    break;
                case NotifyCollectionChangedAction.Remove:
                    foreach (IDataItem connectionPointDataItem in GetConnectionPointDataItems(dataItems, connectionPoint))
                    {
                        connectionPointDataItem.Unlink();
                        connectionPointDataItem.Parent.Children.Remove(connectionPointDataItem);
                    }

                    break;
                default:
                    throw new NotSupportedException();
            }
        }

        private static IEnumerable<IDataItem> GetConnectionPointDataItems(IEnumerable<IDataItem> modelDataItems, ConnectionPoint connectionPoint)
        {
            return modelDataItems.Where(di => di.ValueType == typeof(ControlGroup))
                                 .Select(d => d.Children.FirstOrDefault(di => Equals(di.ValueConverter?.OriginalValue, connectionPoint)))
                                 .Where(c => c != null);
        }

        private void ControlGroupsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (cloning)
            {
                return;
            }

            if (e.GetRemovedOrAddedItem() is ConnectionPoint && !IsAggregationList(sender)) //breaks if other collections are added
            {
                ConnectionPointsCollectionChanged(e);
            }

            AfterControlGroupsCollectionChanged(sender, e);
        }

        private void AfterControlGroupsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            MarkOutputOutOfSync();

            if (Equals(sender, ControlGroups))
            {
                var controlGroup = (ControlGroup)e.GetRemovedOrAddedItem();
                // add/remove data items for control groups and their inputs/outputs
                switch (e.Action)
                {
                    case NotifyCollectionChangedAction.Add:
                        AddDataItemsForControlGroup(controlGroup);
                        break;
                    case NotifyCollectionChangedAction.Remove:
                        IDataItem controlGroupDataItem = GetDataItemByValue(controlGroup);

                        if (controlGroupDataItem != null)
                        {
                            foreach (IDataItem dataItem in controlGroupDataItem.Children)
                            {
                                dataItem.Unlink();
                            }

                            controlGroupDataItem.Children.Clear();
                            DataItems.Remove(controlGroupDataItem);
                        }

                        break;
                    default:
                        throw new NotSupportedException();
                }
            }
        }

        private void AddDataItemsForControlGroup(ControlGroup controlGroup)
        {
            var controlGroupDataItem = new DataItem(controlGroup, DataItemRole.Input) { ValueType = typeof(ControlGroup) };

            // add control group inputs/outputs
            foreach (Input input in controlGroup.Inputs)
            {
                AddConnectionDataItem(controlGroupDataItem, input, DataItemRole.Input);
            }

            foreach (Output output in controlGroup.Outputs)
            {
                AddConnectionDataItem(controlGroupDataItem, output, DataItemRole.Output);
            }

            DataItems.Add(controlGroupDataItem);
        }

        private static void AddConnectionDataItem(IDataItem controlGroupDataItem, ConnectionPoint connectionPoint, DataItemRole role)
        {
            string name = DataItem.DefaultName;

            if ((role & DataItemRole.Input) == DataItemRole.Input)
            {
                int count = controlGroupDataItem.Children.Count(di => (di.Role & DataItemRole.Input) == DataItemRole.Input);
                name = ((IControlGroup)controlGroupDataItem.Value).Name + InputPostFix + count;
            }

            if ((role & DataItemRole.Output) == DataItemRole.Output)
            {
                int count = controlGroupDataItem.Children.Count(di => (di.Role & DataItemRole.Output) == DataItemRole.Output);
                name = ((IControlGroup)controlGroupDataItem.Value).Name + OutputPostFix + count;
            }

            var dataItem = new DataItem
            {
                Name = name,
                Role = role,
                Parent = controlGroupDataItem,
                ValueType = typeof(double),
                ValueConverter = new PropertyValueConverter(connectionPoint, "Value")
            };

            controlGroupDataItem.Children.Add(dataItem);
        }

        private void SetOutputOutOfSync(object sender, PropertyChangedEventArgs e)
        {
            MarkOutputOutOfSync();
        }

        private void ControlledModelsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            // required for project load
            if (!(e?.GetRemovedOrAddedItem() is IModel))
            {
                return;
            }

            if (outputFileFunctionStore == null)
            {
                return;
            }

            ReconnectRtcToFmOutputFile(outputFileFunctionStore.Path);
        }

        private void ModelsPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var model = sender as IModel;
            if (model == null)
            {
                return;
            }

            if (e.PropertyName == "OutputOutOfSync" && model.OutputOutOfSync)
            {
                // this is another hack, fix the model state machine to handle lower level exception
                MarkOutputOutOfSync();
            }
        }

        /// <summary>
        /// Update Feature, ParameterName and UnitName in ConnectionPoint based on information on the other side.
        /// </summary>
        /// <param name="dataItem"></param>
        private void UpdateFeatureAndParameter(IDataItem dataItem)
        {
            if (suspendUpdateFeatureAndParameter)
            {
                return;
            }

            ConnectionPoint connection;
            if (dataItem.LinkedTo != null)
            {
                if (!(dataItem.ValueConverter?.OriginalValue is ConnectionPoint))
                {
                    return;
                }

                connection = (ConnectionPoint)dataItem.ValueConverter.OriginalValue;
                if (connection.Feature != null)
                {
                    lastRelinkedFeature = connection.Feature;
                }

                connection.Feature = dataItem.LinkedTo.GetFeature();
                connection.ParameterName = dataItem.LinkedTo.GetParameterName();
                connection.UnitName = dataItem.LinkedTo.GetUnitName();

                ReplaceSingleFeatureInOutputFeatureCoverages(lastRelinkedFeature, connection.Feature); //(part of clone logic)
            }

            if (dataItem.LinkedBy.Count == 0 || !(dataItem.ValueConverter?.OriginalValue is ConnectionPoint))
            {
                return;
            }

            if (dataItem.LinkedBy.Count > 1)
            {
                throw new NotSupportedException("Use of RTC output in more than one consumer is not supported yet");
            }

            connection = (ConnectionPoint)dataItem.ValueConverter.OriginalValue;
            if (connection.Feature != null)
            {
                lastRelinkedFeature = connection.Feature;
            }

            connection.Feature = dataItem.LinkedBy.First().GetFeature();
            connection.ParameterName = dataItem.LinkedBy.First().GetParameterName();
            connection.UnitName = dataItem.LinkedBy.First().GetUnitName();

            ReplaceSingleFeatureInOutputFeatureCoverages(lastRelinkedFeature, connection.Feature); //(part of clone logic)
        }

        private void ReplaceSingleFeatureInOutputFeatureCoverages(IFeature before, IFeature after)
        {
            if (before == null || after == null)
            {
                return;
            }

            foreach (IFeatureCoverage outputCoverage in OutputFeatureCoverages)
            {
                IEventedList<IFeature> featuresBefore = outputCoverage.Features;
                var featureInCoverage = false;
                var featuresAfter = new List<IFeature>();

                foreach (IFeature f in featuresBefore)
                {
                    if (Equals(f, before))
                    {
                        featureInCoverage = true;
                        featuresAfter.Add(after);
                    }
                    else
                    {
                        featuresAfter.Add(f);
                    }
                }

                if (featureInCoverage)
                {
                    FeatureCoverage.RefreshAfterClone(outputCoverage, featuresBefore, featuresAfter);
                }
            }
        }

        #region IRestartModel
      
        /// <inheritdoc cref="IRestartModel{TRestartFile}.UseRestart"/>
        public virtual bool UseRestart => !RestartInput.IsEmpty;

        /// <inheritdoc cref="IRestartModel{TRestartFile}.WriteRestart"/>
        public virtual bool WriteRestart { get; set; }

        /// <inheritdoc cref="IRestartModel{TRestartFile}.RestartInput"/>
        public virtual RealTimeControlRestartFile RestartInput
        {
            get => restartInput;
            set
            {
                Ensure.NotNull(value,nameof(value));
                restartInput = value;
            }
        }

        /// <summary>
        /// non-public property for manipulating the list of output restart files
        /// </summary>
        protected List<RealTimeControlRestartFile> ListOfOutputRestartFiles { get; set; }

        /// <inheritdoc cref="IRestartModel{TRestartFile}.RestartOutput"/>
        public virtual IEnumerable<RealTimeControlRestartFile> RestartOutput => ListOfOutputRestartFiles;

        /// <inheritdoc cref="IRestartModel{TRestartFile}.SetRestartInputToDuplicateOf"/>
        public virtual void SetRestartInputToDuplicateOf(RealTimeControlRestartFile source)
        {
            Ensure.NotNull(source,nameof(source));
            RestartInput = new RealTimeControlRestartFile(source);
        }
        #endregion

        #region IDimrModel

        #region Overrides of TimeDependentModelBase

        public override IBasicModelInterface BMIEngine => runner.Api;

        #endregion

        public virtual string LibraryName => "FBCTools_BMI";

        public virtual string InputFile => ".";

        public virtual string DirectoryName => "rtc";

        public virtual bool IsMasterTimeStep => false;

        public virtual string ShortName => "rtc";

        public virtual string GetItemString(IDataItem dataItem)
        {
            var propertyValueConverter = dataItem.ValueConverter as PropertyValueConverter;
            if (propertyValueConverter != null)
            {
                var connectionPoint = propertyValueConverter.OriginalValue as ConnectionPoint;

                switch (connectionPoint)
                {
                    case Input input:
                        var inputSerializer = new InputSerializer(input);
                        return inputSerializer.GetXmlName(string.Empty);
                    case Output output:
                        var outputSerializer = new OutputSerializer(output);
                        return outputSerializer.GetXmlName();
                }
            }

            throw new ArgumentException($"Could not serialize data item {dataItem} to d-hydro xml");
        }

        /// <inheritdoc/>
        /// <exception cref="NotSupportedException">
        /// If the string does not start with <see cref="RtcXmlTag.Input"/> or
        /// <see cref="RtcXmlTag.Output"/>
        /// </exception>
        public virtual IEnumerable<IDataItem> GetDataItemsByItemString(string itemString)
        {
            bool isOutput = itemString.StartsWith(RtcXmlTag.Output);
            bool isInput = itemString.StartsWith(RtcXmlTag.Input);

            if (!isOutput && !isInput)
            {
                throw new NotSupportedException($"{itemString} does not start with {RtcXmlTag.Input} or {RtcXmlTag.Output}");
            }

            IEnumerable<IDataItem> dataItem = AllDataItems.Where(di => (di.ValueConverter?.OriginalValue as ConnectionPoint)?.Name == itemString).ToArray();
            if (!dataItem.Any())
            {
                throw new NotSupportedException($"Could not find {itemString} on {Name}");
            }

            return dataItem;
        }

        /// <summary>
        /// Cleans up model after model coupling at the end of a
        /// DIMR import. All input and output points
        /// set by the RTC importer should be reset, if
        /// coupling failed.
        /// </summary>
        public virtual void CleanUpModelAfterModelCoupling()
        {
            foreach (IControlGroup controlGroup in ControlGroups)
            {
                foreach (Input input in controlGroup.Inputs)
                {
                    ResetConnectionPointIfUnlinked(input);
                }

                foreach (Output output in controlGroup.Outputs)
                {
                    ResetConnectionPointIfUnlinked(output);
                }
            }
        }

        private static void ResetConnectionPointIfUnlinked(ConnectionPoint connectionPoint)
        {
            if (!connectionPoint.IsConnected)
            {
                connectionPoint.Reset();
            }
        }

        public virtual Type ExporterType => typeof(RealTimeControlModelExporter);

        public virtual string GetExporterPath(string directoryName)
        {
            return directoryName;
        }

        public virtual string KernelDirectoryLocation => DimrApiDataSet.RtcToolsDllPath;

        public virtual void DisconnectOutput()
        {
            DisconnectOutputFileFunctionStore();
            ListOfOutputRestartFiles.Clear();
        }

        protected override void OnClearOutput()
        {
            BeginEdit("Clearing all real time control output");

            DisconnectOutputFileFunctionStore();
            ListOfOutputRestartFiles.Clear();
            ClearOutputDocuments();

            EndEdit();

            MarkDirty();
        }

        private void ClearOutputDocuments() => OutputDocuments.Clear();

        private void DisconnectOutputFileFunctionStore()
        {
            if (outputFileFunctionStore != null)
            {
                outputFileFunctionStore.Functions?.Clear();
                outputFileFunctionStore.Features?.Clear();
                outputFileFunctionStore.Close();
                outputFileFunctionStore = null;
            }
        }

        public virtual void ConnectOutput(string outputPath)
        {
            if (!RetrieveOutputData(outputPath, out DirectoryInfo dirInfo, out string[] newOutputFiles))
            {
                return;
            }

            IList<string> restartFiles = RetrieveRestartFiles(newOutputFiles);
            SetRestartOutputFiles(restartFiles);

            IEnumerable<string> outputDocumentFilePaths = newOutputFiles.Where(f => f.EndsWith(".xml") || f.EndsWith(".csv")).Except(restartFiles);
            ReconnectOutputDocuments(outputDocumentFilePaths);

            string rtcToFlowFilePath = Path.Combine(dirInfo.FullName, CommunicationRtcToFmFileName);
            ReconnectRtcToFmOutputFile(rtcToFlowFilePath);

            OutputIsEmpty = false;
        }

        private void UpdateOutputFilePaths(string outputPath)
        {
            if (!RetrieveOutputData(outputPath, out DirectoryInfo dirInfo, out string[] newOutputFiles))
            {
                return;
            }

            IList<string> restartFiles = RetrieveRestartFiles(newOutputFiles);
            SetRestartOutputFiles(restartFiles);

            string rtcToFlowFilePath = Path.Combine(dirInfo.FullName, CommunicationRtcToFmFileName);
            UpdateRtcToFmOutputFilePath(rtcToFlowFilePath);

            OutputIsEmpty = false;
        }
        private void UpdateRtcToFmOutputFilePath(string rtcToFlowFilePath)
        {
            if (!File.Exists(rtcToFlowFilePath))
            {
                DisconnectOutputFileFunctionStore();
                return;
            }

            outputFileFunctionStore.Path = rtcToFlowFilePath;
        }

        private static IList<string> RetrieveRestartFiles(string[] newOutputFiles)
        {
            var matchRestartFile = new Regex(@"rtc_\d{8}_\d{6}.xml$");
            IList<string> restartFiles = newOutputFiles.Where(p => matchRestartFile.IsMatch(Path.GetFileName(p))).ToList();
            return restartFiles;
        }

        private static bool RetrieveOutputData(string outputPath, out DirectoryInfo dirInfo, out string[] newOutputFiles)
        {
            dirInfo = null;
            newOutputFiles = null;

            if (string.IsNullOrEmpty(outputPath))
            {
                return false;
            }

            dirInfo = new DirectoryInfo(outputPath);
            if (string.IsNullOrEmpty(dirInfo.Parent?.FullName))
            {
                return false;
            }

            if (!dirInfo.Exists)
            {
                return false;
            }

            newOutputFiles = Directory.GetFiles(outputPath);

            return newOutputFiles.Length != 0;
        }

        private void ReconnectOutputDocuments(IEnumerable<string> outputDocumentFilePaths)
        {
            ClearOutputDocuments();

            var logHandler = new LogHandler("Reconnecting output documents", Log);
            OutputDocuments.AddRange(outputDocumentFilePaths
                                     .Select(p => ReadTextDocument(p, logHandler))
                                     .Where(x => x != null));
            logHandler.LogReport();
        }

        private static ReadOnlyTextFileData ReadTextDocument(string textFilePath, ILogHandler logHandler)
        {
            var textFileInfo = new FileInfo(textFilePath);

            try
            {
                return new ReadOnlyTextFileData(textFileInfo.Name,
                                                File.ReadAllText(textFileInfo.FullName),
                                                GetReadOnlyTextFileDataType(textFileInfo));
            }
            catch (Exception e)
            {
                logHandler.ReportErrorFormat("Error while reading {0}: {1}",
                                             textFileInfo.FullName,
                                             e.Message);
            }

            return null;
        }

        private static ReadOnlyTextFileDataType GetReadOnlyTextFileDataType(FileInfo fileInfo)
        {
            switch (fileInfo.Extension)
            {
                case ".xml":
                    return ReadOnlyTextFileDataType.Xml;
                case ".csv":
                    return ReadOnlyTextFileDataType.Table;
                default:
                    return ReadOnlyTextFileDataType.Default;
            }
        }

        private void SetRestartOutputFiles(IEnumerable<string> restartFilePaths)
        {
            ListOfOutputRestartFiles.Clear();
            ListOfOutputRestartFiles.AddRange( restartFilePaths.Select(RealTimeControlRestartFile.CreateFromFile) );
        }

        private void ReconnectRtcToFmOutputFile(string outputFilePath)
        {
            DisconnectOutputFileFunctionStore();

            if (!File.Exists(outputFilePath))
            {
                return;
            }

            List<IFeature> features = GetChildDataItemLocationsFromControlledModels(DataItemRole.Output).ToList();
            outputFileFunctionStore = new RealTimeControlOutputFileFunctionStore
            {
                Features = features,
                CoordinateSystem = CoordinateSystem,
                Path = outputFilePath
            };
        }

        public virtual ValidationReport Validate() // NOTE: Do not re
        {
            return new RealTimeControlModelValidator().Validate(this);
        }

        public new virtual ActivityStatus Status
        {
            get => base.Status;
            set => base.Status = value;
        }

        public virtual bool RunsInIntegratedModel { get; set; }

        /// <summary>
        /// DimrExportDirectoryPath should only be used if the
        /// model runs stand-alone by using the DimrRunner.
        /// However, RTC cannot run stand-alone, but it is
        /// possible to press the run button. The run will be
        /// cancelled, but the CleanUp is still using this property
        /// for trying to connect output... Exception will be thrown,
        /// but caught. Code should be improved for this case
        /// </summary>
        public virtual string DimrExportDirectoryPath
        {
            get => throw new NotSupportedException();
            set => throw new NotSupportedException();
        }

        public virtual string DimrModelRelativeOutputDirectory => Path.Combine(DirectoryName, DirectoryNameConstants.OutputDirectoryName);

        [NoNotifyPropertyChange]
        public new virtual DateTime CurrentTime
        {
            get => base.CurrentTime;
            set => base.CurrentTime = value;
        }

        public virtual Array GetVar(string category, string itemName = null, string parameter = null)
        {
            return runner.GetVar($"{Name}/{category}/{itemName}/{parameter}");
        }

        public virtual void SetVar(Array values, string category, string itemName = null, string parameter = null)
        {
            runner.SetVar($"{Name}/{category}/{itemName}/{parameter}", values);
        }

        public virtual void OnFinishIntegratedModelRun(string hydroModelWorkingDirectoryPath)
        {
            // Actions, which should be done in the IDimrModel after a successful integrated model
            // run.
            string runRtcDirectory = Path.Combine(hydroModelWorkingDirectoryPath, DirectoryName);

            string[] allNonRecursivePaths = FileBasedUtils.CollectNonRecursivePaths(runRtcDirectory);
            IList<string> allOutputPaths = allNonRecursivePaths.Where(p => !LastExportedPaths.Contains(p)).ToList();

            string rtcToFlowNc = Path.Combine(hydroModelWorkingDirectoryPath, CommunicationRtcToFmFileName);
            string fmToRtcNc = Path.Combine(hydroModelWorkingDirectoryPath, CommunicationFmToRtcFileName);

            if (File.Exists(rtcToFlowNc))
            {
                allOutputPaths.Add(rtcToFlowNc);
            }

            if (File.Exists(fmToRtcNc))
            {
                allOutputPaths.Add(fmToRtcNc);
            }

            Directory.CreateDirectory(Path.Combine(runRtcDirectory, DirectoryNameConstants.OutputDirectoryName));

            foreach (string outputPath in allOutputPaths)
            {
                string outputName = Path.GetFileName(outputPath);
                string destinationOutputPath = Path.Combine(runRtcDirectory, DirectoryNameConstants.OutputDirectoryName, outputName);
                Directory.Move(outputPath, destinationOutputPath);
            }

            currentOutputDirectoryPath = Path.Combine(runRtcDirectory, DirectoryNameConstants.OutputDirectoryName);

            MarkDirty();
        }

        public virtual ISet<string> IgnoredFilePathsWhenCleaningWorkingDirectory =>
            new HashSet<string>();

        #endregion

        #region HandleControlGroupRenaming

        private string previousControlGroupName;

        private void ControlGroupsPropertyChanging(object sender, PropertyChangingEventArgs e)
        {
            if (e.PropertyName != "Name")
            {
                return;
            }

            var controlGroup = sender as ControlGroup;
            if (controlGroup == null)
            {
                return;
            }

            previousControlGroupName = controlGroup.Name;
        }

        private void ControlGroupsPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName != "Name")
            {
                return;
            }

            var controlGroup = sender as ControlGroup;
            if (controlGroup == null)
            {
                return;
            }

            // DELFT3DFM-1441: ControlGroups must have unique names!
            if (ControlGroups.Where(cg => !ReferenceEquals(cg, controlGroup)).Any(cg => cg.Name == controlGroup.Name))
            {
                Log.WarnFormat(Resources.RealTimeControlModel_ControlGroupsPropertyChanged_Unable_to_update_ControlGroup_name__all_ControlGroup_names_must_be_unique__0___1___has_been_reverted_back_to___2__,
                               Environment.NewLine, controlGroup.Name, previousControlGroupName);

                if (controlGroup.Name != previousControlGroupName)
                {
                    controlGroup.Name = previousControlGroupName;
                }

                return;
            }

            // DELFT3DFM-1441: ControlGroup ChildDataItems with should have the ControlGroup DataItem Name as a prefix
            this.SyncControlGroupChildDataItemNames(controlGroup);
        }

        #endregion

        #region IRealTimeControlModel

        public override IEnumerable<object> GetDirectChildren()
        {
            return base.GetDirectChildren().Concat(OutputFeatureCoverages).Concat(OutputDocuments);
        }

        /// <summary>
        /// Query connectable locations from controlled models.
        /// </summary>
        /// <param name="role"></param>
        /// <returns></returns>
        public virtual IEnumerable<IFeature> GetChildDataItemLocationsFromControlledModels(DataItemRole role)
        {
            IEnumerable<IFeature> childDataItemLocationsFromControlledModels = ControlledModels.SelectMany(m => m.GetChildDataItemLocations(role)).Distinct();
            // The childDataItemLocationsFromControlledModels list may contain features that are wrapped in data-items that
            // provide/consuming value types other that typeof(double), e.g. Flow1D's network-coverages for water levels,
            // discharges, etc (Flow1D exposes these network-coverages data items for e.g. the OpenMI wrapper).
            // RTC only can handle values on one single location, so return only the single value locations
            // (i.e. data item value type is double, see GetChildDataItemsFromControlledModelsForLocation(...) below).
            return
                childDataItemLocationsFromControlledModels.Where(
                    loc => GetChildDataItemsFromControlledModelsForLocation(loc).Any());
        }

        public virtual IEnumerable<IDataItem> GetChildDataItemsFromControlledModelsForLocation(IFeature feature)
        {
            // RTC only can handle values on one single location, so return only the data items that have
            // value type double (see also GetChildDataItemLocationsFromControlledModels(...) above).
            return ControlledModels.SelectMany(m => m.GetChildDataItems(feature)).Where(di => di.ValueType == typeof(double));
        }

        public virtual void ResetOrphanedControlGroupInputsAndOutputs(IControlGroup controlGroup)
        {
            // SOBEK3-562: Existing projects can have ControlGroups with locations at inputs/outputs but no underlying data item links

            controlGroup.Inputs.Where(input => input.Feature != null).ForEach(input => ResetOrphanedInput(controlGroup, input));
            controlGroup.Outputs.Where(output => output.Feature != null).ForEach(output => ResetOrphanedOutput(controlGroup, output));
        }

        private void ResetOrphanedInput(IControlGroup controlGroup, Input input)
        {
            IDataItem inputDataItem = GetDataItemByValue(input);
            if (inputDataItem == null || inputDataItem.LinkedTo != null)
            {
                return;
            }
            // else Input is Orphaned

            RtcBaseObject[] rtcInputConnections = controlGroup.Rules.Where(r => r.Inputs.Contains(input)).Cast<RtcBaseObject>()
                                                              .Concat(controlGroup.Conditions.Where(c => c.Input == input))
                                                              .Concat(controlGroup.Signals.Where(s => s.Inputs.Contains(input)))
                                                              .ToArray();

            ResetOrphanedConnectionPoint(controlGroup.Name, rtcInputConnections, input);
        }

        private void ResetOrphanedOutput(IControlGroup controlGroup, Output output)
        {
            IDataItem outputDataItem = GetDataItemByValue(output);
            if (outputDataItem == null || outputDataItem.LinkedBy.Any())
            {
                return;
            }
            // else Output is orphaned

            RtcBaseObject[] rtcOutputConnections = controlGroup.Rules.Where(r => r.Outputs.Contains(output)).Cast<RtcBaseObject>().ToArray();

            ResetOrphanedConnectionPoint(controlGroup.Name, rtcOutputConnections, output);
        }

        private void ResetOrphanedConnectionPoint(string controlGroupName, RtcBaseObject[] connections, ConnectionPoint connectionPoint)
        {
            string connectionTypeName = connectionPoint is Input ? "Input" : "Output"; // Can only be an Input or Output
            string connectionsString = connections.Any() ? string.Join(", ", connections.Select(ic => ic.Name)) : "None";

            Log.WarnFormat(Resources.RealTimeControlModel_BrokenDataItemLinkDetected,
                           Name, controlGroupName, connectionTypeName, connectionsString,
                           Environment.NewLine, connectionPoint.Name);

            connectionPoint.Reset();
        }

        #endregion

        #region State Aware Model

        private IFeature lastRelinkedFeature;

        protected virtual Queue<DateTime> outputWriteTimesQueue { get; set; }

        #region Save State: Time Range

        public virtual DateTime SaveStateStartTime { get; set; }

        public virtual DateTime SaveStateStopTime { get; set; }

        public virtual TimeSpan SaveStateTimeStep { get; set; }

        #endregion

        #endregion

        #region IModelMerge

        public virtual ValidationReport ValidateMerge(IModelMerge sourceModel)
        {
            if (!CanMerge(sourceModel))
            {
                return new ValidationReport(Name + " (Real Time Control)", new[]
                {
                    new ValidationReport("Model", new[]
                    {
                        new ValidationIssue(sourceModel, ValidationSeverity.Error, $"sourceModel {sourceModel.Name} (of type {sourceModel.GetType()}) can't be merged with this model {Name} (of type {GetType()})")
                    })
                });
            }

            return new RealTimeControlModelMergeValidator().Validate(this, (RealTimeControlModel)sourceModel);
        }

        public virtual bool Merge(IModelMerge sourceModel, IDictionary<IModelMerge, IModelMerge> mergedDependentModelsLookup)
        {
            if (!CanMerge(sourceModel))
            {
                return false;
            }

            var srcModel = sourceModel as RealTimeControlModel;
            if (srcModel == null)
            {
                return false;
            }

            List<string> existingControlGroupNames = ControlGroups.Select(cg => cg.Name).ToList();
            foreach (ControlGroup controlGroup in srcModel.ControlGroups)
            {
                var clonedControlGroup = (ControlGroup)controlGroup.Clone();
                if (existingControlGroupNames.Contains(clonedControlGroup.Name))
                {
                    string uniqueName = NamingHelper.GenerateUniqueNameFromList(controlGroup.Name + "{0}", true, existingControlGroupNames);
                    Log.InfoFormat(Resources.RealTimeControlModel_Merge_There_already_exists_a_ControlGroup_named__0__in_Model__1___ControlGroup__0__will_be_renamed_to__2_,
                                   clonedControlGroup.Name, Name, uniqueName);

                    clonedControlGroup.Name = uniqueName;
                }

                ControlGroups.Add(clonedControlGroup);
                existingControlGroupNames.Add(clonedControlGroup.Name);
            }

            if (mergedDependentModelsLookup == null)
            {
                return true;
            }

            foreach (IModelMerge sourceDependentModel in mergedDependentModelsLookup.Keys)
            {
                var mergedDependentModel = mergedDependentModelsLookup[sourceDependentModel] as IModel;

                // check input items LinkedTo
                sourceModel.AllDataItems.Where(di => di.Role == DataItemRole.Input && di.LinkedTo != null && di.ValueConverter != null)
                           .ForEach(dataItem => RelinkDataItemsForMergedInputs(dataItem, mergedDependentModel));

                // check output items LinkedBy
                sourceModel.AllDataItems.Where(di => di.Role == DataItemRole.Output && di.LinkedBy != null && di.ValueConverter != null)
                           .ForEach(dataItem => RelinkDataItemsForMergedOutputs(dataItem, mergedDependentModel));
            }

            return true;
        }

        private void RelinkDataItemsForMergedInputs(IDataItem sourceModelInput, IModel mergedDependentModel)
        {
            var sourceModelOriginalValue = sourceModelInput.ValueConverter.OriginalValue as Input;
            if (sourceModelOriginalValue == null)
            {
                return; // Shouldn't ever happen
            }

            IDataItem matchingRtcModelDataItem = AllDataItems.FirstOrDefault(
                di => di.LinkedTo == null
                      && di.ValueConverter?.OriginalValue is Input input
                      && input.Name == sourceModelOriginalValue.Name);

            if (matchingRtcModelDataItem == null)
            {
                return;
            }

            IDataItem dependentModelDataItemToLink = GetMatchingDataItemToLink(sourceModelInput.LinkedTo, DataItemRole.Output, sourceModelOriginalValue.ParameterName, mergedDependentModel);

            if (dependentModelDataItemToLink != null) // ok, lets relink!
            {
                matchingRtcModelDataItem.LinkTo(dependentModelDataItemToLink);
            }
        }

        private void RelinkDataItemsForMergedOutputs(IDataItem sourceModelOutput, IModel mergedDependentModel)
        {
            var sourceModelOriginalValue = sourceModelOutput.ValueConverter.OriginalValue as Output;
            if (sourceModelOriginalValue == null)
            {
                return; // Shouldn't ever happen
            }

            IDataItem matchingRtcModelDataItem = AllDataItems.FirstOrDefault(
                di => (di.LinkedBy == null || di.LinkedBy.Count != sourceModelOutput.LinkedBy.Count)
                      && di.ValueConverter?.OriginalValue is Output output
                      && output.Name == sourceModelOriginalValue.Name);

            if (matchingRtcModelDataItem == null)
            {
                return;
            }

            foreach (IDataItem sourceLinkedDataItem in sourceModelOutput.LinkedBy)
            {
                IDataItem dependentModelDataItemToLink = GetMatchingDataItemToLink(sourceLinkedDataItem, DataItemRole.Input, sourceModelOriginalValue.ParameterName, mergedDependentModel);

                if (dependentModelDataItemToLink != null) // ok, lets relink!
                {
                    // unlink any existing outputs connected to this item
                    dependentModelDataItemToLink.LinkedBy.Where(linkee => linkee.Role == DataItemRole.Output).ForEach(linkee => linkee.Unlink());

                    dependentModelDataItemToLink.LinkTo(matchingRtcModelDataItem);
                }
            }
        }

        private static IDataItem GetMatchingDataItemToLink(IDataItem sourceLinkedDataItem, DataItemRole role, string parameterName, IModel mergedDependentModel)
        {
            IFeature matchingFeature = mergedDependentModel.GetChildDataItemLocations(role)
                                                           .FirstOrDefault(di => sourceLinkedDataItem.ValueConverter != null && di.Equals(sourceLinkedDataItem.ValueConverter.OriginalValue));

            if (matchingFeature == null)
            {
                return null;
            }

            IDataItem dataItemToLink = mergedDependentModel.GetChildDataItems(matchingFeature)
                                                           .FirstOrDefault(di => di.Name.Contains(parameterName));

            return dataItemToLink;
        }

        public virtual bool CanMerge(object sourceModel)
        {
            var rtcModel = sourceModel as RealTimeControlModel;
            if (rtcModel == null)
            {
                return false;
            }

            return rtcModel.DependentModels.All(m => DependentModels.Any(dm => dm.CanMerge(m)));
        }

        public virtual IEnumerable<IModelMerge> DependentModels => ControlledModels.OfType<IModelMerge>();

        public virtual string CommunicationRtcToFmFileName
        {
            get => communicationRtcToFmFileName;
            set
            {
                Ensure.NotNull(value, nameof(value));

                if (!string.IsNullOrEmpty(value))
                {
                    communicationRtcToFmFileName = value + ".nc";
                }
            }
        }

        public virtual string CommunicationFmToRtcFileName
        {
            get => communicationFmToRtcFileName;
            set
            {
                Ensure.NotNull(value, nameof(value));

                if (!string.IsNullOrEmpty(value))
                {
                    communicationFmToRtcFileName = value + ".nc";
                }
            }
        }

        #endregion

        #region TimeDependentModelBase

        protected override void OnProgressChanged()
        {
            runner.OnProgressChanged();
        }

        protected override void OnFinish()
        {
            runner.OnFinish();
        }

        protected override void OnInitialize()
        {
            if (RunsInIntegratedModel)
            {
                return;
            }

            Log.Info("You cannot run RTC models standalone");
            Status = ActivityStatus.Cancelled;
        }

        /// <summary>
        /// Implements logic required to run model, process input items and updates output items.
        /// </summary>
        protected override void OnExecute()
        {
            if (RunsInIntegratedModel)
            {
                return;
            }

            OutputOutOfSync = false;
            runner.OnExecute();
        }

        protected override void OnCleanup()
        {
            // Restore linked data items to their original values
            // Needs to be done here as RestartState is written in TimeDependentModel.Finish()
            foreach (IDataItem dataItem in linkedDataItemsOriginalValues)
            {
                IDataItem currentDataItem = AllDataItems.FirstOrDefault(
                    di => di.LinkedBy.Count > 0 &&
                          di.LinkedBy[0].Name.Equals(dataItem.Name) &&
                          di.LinkedBy[0].Tag.Equals(dataItem.Tag));

                if (currentDataItem != null)
                {
                    currentDataItem.Value = dataItem.Value;
                }
            }

            linkedDataItemsOriginalValues.Clear();

            outputWriteTimesQueue = null;

            // Clear the explicit value converter lookup if relevant
            explicitValueConverterLookupItems?.Clear();

            base.OnCleanup();
            runner.OnCleanup();
        }

        #endregion

        #region IFileBased

        private bool isOpen;
        private int dirtyCounter; // tells NHibernate we need to be saved
        private string path;
        private string currentOutputDirectoryPath;
        private string persistentOutputDirectory;
        private string oldPersistentOutputDirectory = string.Empty;
        private bool removeSourceOutputFolder;

        /// <summary>
        /// The persistent output directory to which output files
        /// should be copied.
        /// </summary>
        /// <remarks>
        /// Can be outdated when asked due to a rename model action and
        /// save (not outdated during save-as). Therefore
        /// <see cref="UpdatePersistentOutputDirectoryIfNeeded"/> should be called.
        /// </remarks>
        protected virtual string PersistentOutputDirectory
        {
            get
            {
                UpdatePersistentOutputDirectoryIfNeeded();
                return persistentOutputDirectory;
            }
            set => persistentOutputDirectory = value;
        }

        /// <summary>
        /// Check if model has been renamed followed by a save (not a save-as).
        /// During normal save <see cref="persistentOutputDirectory"/> contains
        /// old model name and therefore if condition is true. Resulting in
        /// correcting <see cref="persistentOutputDirectory"/> and removing
        /// source folder in <see cref="CopyOutputFolderTo"/> method called from
        /// <see cref="IFileBased.Path"/> setter. During Save As first
        /// <see cref="IFileBased.CopyTo"/> and <see cref="IFileBased.SwitchTo"/>
        /// will be called with an argument, which is the new path. Due to this
        /// <see cref="persistentOutputDirectory"/> is up to date when setting
        /// <see cref="IFileBased.Path"/> property afterwards. Resulting in unnecessary
        /// second copy action without removal of source folder and second
        /// <see cref="IFileBased.SwitchTo"/>.
        /// </summary>
        private void UpdatePersistentOutputDirectoryIfNeeded()
        {
            var dirInfo = new DirectoryInfo(persistentOutputDirectory);
            DirectoryInfo modelDirectory = dirInfo.Parent;
            if (modelDirectory.Name != Name)
            {
                removeSourceOutputFolder = true;
                oldPersistentOutputDirectory = persistentOutputDirectory;
                persistentOutputDirectory = Path.Combine(modelDirectory.Parent.FullName, Name, DirectoryNameConstants.OutputDirectoryName);
            }
        }

        // Used for Import model
        // Creating new model
        void IFileBased.CreateNew(string path)
        {
            Ensure.NotNull(path, nameof(path));

            PersistentOutputDirectory = GetOutputFolderFromDeltaShellPath(path);
            this.path = path;
            isOpen = true;
        }

        void IFileBased.Close()
        {
            isOpen = false;
        }

        // Never called by DeltaShell Framework
        void IFileBased.Open(string path)
        {
            Ensure.NotNull(path, nameof(path));
            isOpen = true;
        }

        // Used for Save As, called by ProjectFileBasedItemRepository.
        void IFileBased.CopyTo(string destinationPath)
        {
            Ensure.NotNull(destinationPath, nameof(destinationPath));

            string targetOutputDirectory = GetOutputFolderFromDeltaShellPath(destinationPath);
            CopyOutputFolderTo(targetOutputDirectory);
        }

        // Used for Open Project, called by FileBasedDataAccessListener.

        // Save, called by FileBasedDataAccessListener, so
        // dirtyCounter change needed for activating.

        // Used for Save As, called by ProjectFileBasedItemRepository.

        // Used for first Save As (Moving from temp folder,
        // since project name will be adjusted during Save As,
        // which trigger database update,
        // which results in a second Save of files due to FileBasedDataAccessListener)
        void IFileBased.SwitchTo(string newPath)
        {
            Ensure.NotNull(newPath, nameof(newPath));

            string expectedOutputPath = GetOutputFolderFromDeltaShellPath(newPath);

            // Open project, Save  As, Save
            path = newPath;
            PersistentOutputDirectory = expectedOutputPath;

            currentOutputDirectoryPath = expectedOutputPath;

            // Open project
            if (!isOpen)
            {
                isOpen = true;

                if (Directory.Exists(expectedOutputPath))
                {
                    bool originalOutputOutOfSync = OutputOutOfSync;
                    ConnectOutput(expectedOutputPath);
                    OutputOutOfSync = originalOutputOutOfSync;
                    return;
                }
            }

            if (Directory.Exists(expectedOutputPath) && IsRtcOutputPresent)
            {
                bool originalOutputOutOfSync = OutputOutOfSync;
                UpdateOutputFilePaths(expectedOutputPath);
                OutputOutOfSync = originalOutputOutOfSync;
            }
        }

        void IFileBased.Delete()
        {
            // Can be used in the future for deleting
            // files when model has been deleted by user.
        }

        // Used for Open Project, called by FileBasedDataAccessListener.

        // Save, called by FileBasedDataAccessListener, so
        // dirtyCounter change needed for activating.

        // Used for first Save As (Moving from temp folder,
        // since project name will be adjusted during Save As,
        // which trigger database update,
        // which results in a second Save of files due to Data Access Listener)
        string IFileBased.Path
        {
            get => path;
            set
            {
                if (path == value)
                {
                    return;
                }

                path = value;

                // isOpen check needed for saving project,
                // otherwise during opening an export will be done.
                if (path.StartsWith("$") && isOpen)
                {
                    CopyOutputFolderTo(PersistentOutputDirectory);
                }
            }
        }

        IEnumerable<string> IFileBased.Paths
        {
            get
            {
                yield return ((IFileBased)this).Path;
            }
        }

        bool IFileBased.IsFileCritical => true;

        bool IFileBased.IsOpen => isOpen;

        bool IFileBased.CopyFromWorkingDirectory => false;

        #region FileBased Helper Methods

        private string GetOutputFolderFromDeltaShellPath(string filePath)
        {
            string projectDirectory = Path.GetDirectoryName(filePath);

            Ensure.NotNull(projectDirectory, nameof(projectDirectory));

            return Path.Combine(projectDirectory, Name, "output");
        }

        private void CopyOutputFolderTo(string targetDirectory)
        {
            if (!CanOutputBeCopied())
            {
                return;
            }

            if (!IsRtcOutputPresent)
            {
                RemoveOutputDirectory(targetDirectory);
            }
            else
            {
                var dirInfoSource = new DirectoryInfo(currentOutputDirectoryPath);
                var dirInfoTarget = new DirectoryInfo(targetDirectory);

                if (dirInfoSource.FullName == dirInfoTarget.FullName)
                {
                    return;
                }

                DirectoryCopy(dirInfoSource, dirInfoTarget);

                if (removeSourceOutputFolder)
                {
                    RemoveOutputFolderInOldDirectory();
                }
            }
        }

        private bool CanOutputBeCopied()
        {
            return !string.IsNullOrEmpty(currentOutputDirectoryPath) && Directory.Exists(currentOutputDirectoryPath);
        }

        private void RemoveOutputFolderInOldDirectory()
        {
            DisconnectOutput();
            FileUtils.DeleteIfExists(Directory.GetParent(oldPersistentOutputDirectory).FullName);
            oldPersistentOutputDirectory = string.Empty;
            removeSourceOutputFolder = false;
        }

        private static void RemoveOutputDirectory(string targetDirectory)
        {
            FileUtils.DeleteIfExists(targetDirectory);
        }

        /// <summary>
        /// Gets whether the RTC model output is present.
        /// </summary>
        private bool IsRtcOutputPresent => outputFileFunctionStore != null || OutputDocuments.Any() || RestartOutput.Any();

        private static void DirectoryCopy(DirectoryInfo sourceDir, DirectoryInfo destDir)
        {
            DirectoryInfo[] sourceSubDirs = sourceDir.GetDirectories();

            FileUtils.CreateDirectoryIfNotExists(destDir.FullName, true);

            FileInfo[] sourceFiles = sourceDir.GetFiles();
            foreach (FileInfo file in sourceFiles)
            {
                string destFilePath = Path.Combine(destDir.FullName, file.Name);
                file.CopyTo(destFilePath, true);
            }

            foreach (DirectoryInfo subDir in sourceSubDirs)
            {
                string newDestDirPath = Path.Combine(destDir.FullName, subDir.Name);
                var newDestDirInfo = new DirectoryInfo(newDestDirPath);
                DirectoryCopy(subDir, newDestDirInfo);
            }
        }

        // Needed for NHibernate.
        private void MarkDirty()
        {
            unchecked
            {
                dirtyCounter++;
            }
        }

        #endregion

        #endregion
    }
}
