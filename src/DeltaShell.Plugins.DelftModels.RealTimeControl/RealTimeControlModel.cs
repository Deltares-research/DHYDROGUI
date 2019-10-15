using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using BasicModelInterface;
using DelftTools.Functions;
using DelftTools.Hydro;
using DelftTools.Shell.Core;
using DelftTools.Shell.Core.Extensions;
using DelftTools.Shell.Core.Workflow;
using DelftTools.Shell.Core.Workflow.DataItems;
using DelftTools.Shell.Core.Workflow.DataItems.ValueConverters;
using DelftTools.Shell.Core.Workflow.Restart;
using DelftTools.Utils;
using DelftTools.Utils.Aop;
using DelftTools.Utils.Collections;
using DelftTools.Utils.Collections.Generic;
using DelftTools.Utils.IO;
using DelftTools.Utils.Reflection;
using DelftTools.Utils.Validation;
using DeltaShell.Dimr;
using DeltaShell.NGHS.Common;
using DeltaShell.Plugins.DelftModels.HydroModel.Export;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Domain;
using DeltaShell.Plugins.DelftModels.RealTimeControl.ImportExport;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Properties;
using DeltaShell.Plugins.DelftModels.RealTimeControl.rtc_kernel;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Validation;
using GeoAPI.Extensions.CoordinateSystems;
using GeoAPI.Extensions.Coverages;
using GeoAPI.Extensions.Feature;
using log4net;
using NetTopologySuite.Extensions.Coverages;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl
{
    /// <summary>
    /// NotifyPropertyChange attribute should not be necessary because base class 
    /// already has it applied. Projectexplorer does not function correctly when left out.
    /// </summary>
    [Entity(FireOnCollectionChange=false)]
    public class RealTimeControlModel : TimeDependentModelBase, IRealTimeControlModel, IDimrStateAwareModel, IModelMerge, IDisposable, IDimrModel, ICoupledModel
    {
        public const string InputPostFix = ".input";
        public const string OutputPostFix = ".output";

        private string outputFileName = "rtcOutput.nc";

        private static readonly ILog Log = LogManager.GetLogger(typeof(RealTimeControlModel));
        private readonly DimrRunner runner;

        private readonly IList<IDataItem> linkedDataItemsOriginalValues;
        private ICoordinateSystem coordinateSystem;
        private RealTimeControlOutputFileFunctionStore outputFileFunctionStore;

        protected virtual IList<ExplicitValueConverterLookupItem> explicitValueConverterLookupItems { get; set; }

        public virtual RealTimeControlOutputFileFunctionStore OutputFileFunctionStore
        {
            get { return outputFileFunctionStore; }
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

        public virtual int LogLevel { get; set; }
        //set this to true when running the model..so the output won't be removed during the run
        public virtual bool FlushLogEveryStep { get; set; }

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

            runner = new DimrRunner(this);
            DimrConfigModelCouplerFactory.CouplerProviders.Add(new RealTimeControlDimrConfigModelCouplerProvider());

            if(outputFileFunctionStore != null)
                ReconnectOutputFiles(outputFileFunctionStore.Path);
        }
        
        private ICompositeActivity oldOwner;

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
                foreach(var model in oldOwner.Activities.OfType<IModel>())
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
            //todo: test aggregation of list
            var model = e.GetRemovedOrAddedItem() as IModel;
            if (model == null || model is RealTimeControlModel)
            {
                return;
            }
            switch(e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    InternalControlledModelsList.Add(model);
                    break;
                case NotifyCollectionChangedAction.Remove:
                    InternalControlledModelsList.Remove(model);
                    OnRemoveModel();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        [EditAction]
        private void OnRemoveModel()
        {
            OutputIsEmpty = false; // hack to make ClearOutput fire appropriately. 
            ClearOutput();
        }

        public override IDataItem GetDataItemByTag(string tag)
        {
            return base.GetDataItemByTag(tag) ?? CreateDataItemNotAvailableInPreviousVersion(tag);
        }

        /// <summary>
        /// Incredibly ugly construct, but this is used for backward compatibility reasons
        /// </summary>
        /// <param name="tag"></param>
        private IDataItem CreateDataItemNotAvailableInPreviousVersion(string tag)
        {
            if (tag == RestartInputStateTag || tag == UseRestartTag || tag == WriteRestartTag)
            {
                AddRestartDataItems();
                return GetDataItemByTag(tag);
            }
            return null;
        }

        [EditAction]
        private void ConnectionPointsCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            // add/remove data items for control groups and their inputs/outputs
            var connectionPoint = (ConnectionPoint) e.GetRemovedOrAddedItem();
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    var controlGroupDataItem = DataItems.FirstOrDefault(
                        di =>
                            {
                                var controlGroup = di.Value as ControlGroup;
                                if (controlGroup != null)
                                {
                                    if (controlGroup.Inputs.Cast<ConnectionPoint>().Concat(controlGroup.Outputs).Contains(connectionPoint))
                                    {
                                        return true;
                                    }
                                }
                                return false;
                            });

                    if (controlGroupDataItem != null)
                    {
                        AddConnectionDataItem(controlGroupDataItem, connectionPoint, connectionPoint is Input ? DataItemRole.Input : DataItemRole.Output);
                    }
                    break;
                case NotifyCollectionChangedAction.Remove:
                    foreach (var dataItem in DataItems.Where(di => di.ValueType == typeof (ControlGroup)))
                    {
                        var connectionPointDataItem = dataItem.Children.FirstOrDefault(di => di.ValueConverter != null && Equals(di.ValueConverter.OriginalValue,connectionPoint));
                        if (connectionPointDataItem != null)
                        {
                            connectionPointDataItem.Unlink();
                            dataItem.Children.Remove(connectionPointDataItem);
                        }
                    }
                    break;
                default:
                    throw new NotSupportedException();
            }
        }

        private bool cloning;

        void ControlGroupsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
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

        [EditAction]
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
                        var controlGroupDataItem = GetDataItemByValue(controlGroup);

                        if (controlGroupDataItem != null)
                        {
                            foreach (var dataItem in controlGroupDataItem.Children)
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
            var controlGroupDataItem = new DataItem(controlGroup, DataItemRole.Input)
                                           {
                                               ValueType = typeof(ControlGroup)
                                           };

            // add control group inputs/outputs
            foreach (var input in controlGroup.Inputs)
            {
                AddConnectionDataItem(controlGroupDataItem, input, DataItemRole.Input);
            }
            foreach (var output in controlGroup.Outputs)
            {
                AddConnectionDataItem(controlGroupDataItem, output, DataItemRole.Output);
            }

            DataItems.Add(controlGroupDataItem);
        }

        private static void AddConnectionDataItem(IDataItem controlGroupDataItem, ConnectionPoint connectionPoint, DataItemRole role)
        {
            var name = DataItem.DefaultName;

            if ((role & DataItemRole.Input) == DataItemRole.Input)
            {
                var count = controlGroupDataItem.Children.Count(di => (di.Role & DataItemRole.Input) == DataItemRole.Input);
                name = ((IControlGroup) controlGroupDataItem.Value).Name + InputPostFix + count;
            }
            if ((role & DataItemRole.Output) == DataItemRole.Output)
            {
                var count = controlGroupDataItem.Children.Count(di => (di.Role & DataItemRole.Output) == DataItemRole.Output);
                name = ((IControlGroup)controlGroupDataItem.Value).Name + OutputPostFix + count;
            }            
            
            var dataItem = new DataItem
            {
                Name = name,
                Role = role,
                Parent = controlGroupDataItem,
                ValueType = typeof (double),
                ValueConverter = new PropertyValueConverter(connectionPoint, "Value")
            };

            controlGroupDataItem.Children.Add(dataItem);
        }
        
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

        //HOW can we overcome this duplication?
        [NoNotifyPropertyChange]
        public override DateTime StartTime
        {
            get
            {
                return (TimeProvider != null) ? TimeProvider.StartTime : base.StartTime;
            }
            set
            {
                if (TimeProvider != null)
                {
                    TimeProvider.StartTime = value;
                }
                // This base model setting is made to make the base logic right
                base.StartTime = value;
            }
        }

        [NoNotifyPropertyChange]
        public override DateTime StopTime
        {
            get
            {
                return (TimeProvider != null) ? TimeProvider.StopTime : base.StopTime;
            }
            set
            {
                if (TimeProvider != null)
                {
                    TimeProvider.StopTime = value;
                }
                // This base model setting is made to make the base logic right
                base.StopTime = value;
            }
        }

        [NoNotifyPropertyChange]
        public override TimeSpan TimeStep
        {
            get
            {
                return (TimeProvider != null) ? TimeProvider.TimeStep : base.TimeStep;
            }
            set
            {
                if (TimeProvider != null)
                {
                    TimeProvider.TimeStep = value;
                }
                // This base model setting is made to make the base logic right
                base.TimeStep = value;
            }
        }

        [NoNotifyPropertyChange]
        [Aggregation]
        public override object Owner
        {
            get
            {
                return base.Owner;
            }
            set
            {
                base.Owner = value;
                ResubscribeToOwner();
            }
        }
        private string workDirectory;

        public virtual bool LimitMemory { get; set; }
        
        public virtual string LastWorkingDirectory { get; protected set; }

        #region IDimrModel

        #region Overrides of TimeDependentModelBase

        public override IBasicModelInterface BMIEngine {
            get { return runner.Api; }
        }

        #endregion

        public virtual string LibraryName
        {
            get { return "FBCTools_BMI"; }
        }

        public virtual string InputFile
        {
            get { return "."; }
        }

        public virtual string DirectoryName
        {
            get { return "rtc"; }
        }

        public virtual bool IsMasterTimeStep
        {
            get { return false; }
        }

        public virtual string ShortName
        {
            get { return "rtc"; }
        }

        public virtual string GetItemString(IDataItem dataItem)
        {
            var propertyValueConverter = dataItem.ValueConverter as PropertyValueConverter;
            if (propertyValueConverter != null)
            {
                var connectionPoint = propertyValueConverter.OriginalValue as ConnectionPoint;

                if (connectionPoint != null)
                {
                    return connectionPoint.XmlName;
                }
            }

            throw new ArgumentException(string.Format("Could not serialize data item {0} to d-hydro xml", dataItem));
        }

        /// <summary>
        /// Gets the data item by item string.
        /// </summary>
        /// <param name="itemString">The item string.</param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException">If the string does not start with <see cref="RtcXmlTag.Input"/> or <see cref="RtcXmlTag.Output"/></exception>
        public virtual IDataItem GetDataItemByItemString(string itemString)
        {
            //[Output]Maeslant_drempel/Crest width (s)
            var isOutput = itemString.StartsWith(RtcXmlTag.Output);
            var isInput = itemString.StartsWith(RtcXmlTag.Input);

            if (!isOutput && !isInput)
            {
                throw new NotImplementedException($"{itemString} does not start with {RtcXmlTag.Input} or {RtcXmlTag.Output}");
            }

            var dataItem = AllDataItems.FirstOrDefault(di => (di.ValueConverter?.OriginalValue as ConnectionPoint)?.Name == itemString);
            if (dataItem == null)
            {
                throw new NotImplementedException($"Could not find {itemString} on {Name}");
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

        public virtual Type ExporterType
        {
            get { return typeof(RealTimeControlModelExporter); }
        }

        public virtual string GetExporterPath(string directoryName)
        {
            return directoryName;
        }

        public virtual string KernelDirectoryLocation
        {
            get { return DimrApiDataSet.RtcToolsDllPath; }
        }

        public virtual void DisconnectOutput()
        {
            if (outputFileFunctionStore == null) return;

            outputFileFunctionStore.Functions?.Clear();
            outputFileFunctionStore.Features?.Clear();
            outputFileFunctionStore.Close();
            outputFileFunctionStore = null;
        }

        public virtual void ConnectOutput(string outputPath)
        {
            if (string.IsNullOrEmpty(outputPath)) return;

            var dirInfo = new DirectoryInfo(outputPath);
            if (dirInfo.Parent == null) return;

            var outputFilePath = Path.Combine(dirInfo.Parent.FullName, OutputFileName);
            ReconnectOutputFiles(outputFilePath);
        }

        private void ReconnectOutputFiles(string outputFilePath)
        {
            DisconnectOutput();

            if (!File.Exists(outputFilePath)) return;
            
            var features = GetChildDataItemLocationsFromControlledModels(DataItemRole.Output).ToList();
            outputFileFunctionStore = new RealTimeControlOutputFileFunctionStore()
            {
                Features = features,
                CoordinateSystem = this.CoordinateSystem, 
                Path = outputFilePath
            };
        }

        public virtual ValidationReport Validate() // NOTE: Do not re
        {
            return new RealTimeControlModelValidator().Validate(this);
        }
        public new virtual ActivityStatus Status
        {
            get { return base.Status; }
            set { base.Status = value; }
        }

        [EditAction]
        public virtual bool RunsInIntegratedModel { get; set; }

        [EditAction]
        public virtual string DimrExportDirectoryPath
        {
            get { return ExplicitWorkingDirectory; }
            set { ExplicitWorkingDirectory = value; }
        }

        public virtual string DimrModelRelativeWorkingDirectory
        {
            get { return DirectoryName; }
        }

        public virtual string DimrModelRelativeOutputDirectory
        {
            get { return DirectoryName; }
        }

        [NoNotifyPropertyChange]
        public new virtual DateTime CurrentTime
        {
            get { return base.CurrentTime; }
            set { base.CurrentTime = value; }
        }
        public virtual Array GetVar(string category, string itemName = null, string parameter = null)
        {
            return runner.GetVar(string.Format("{0}/{1}/{2}/{3}", Name, category, itemName, parameter));
        }
        public virtual void SetVar(Array values, string category, string itemName = null, string parameter = null)
        {
            runner.SetVar(string.Format("{0}/{1}/{2}/{3}", Name, category, itemName, parameter), values);
        }
        public virtual bool CanRunParallel { get { return false; } }
        public virtual string MpiCommunicatorString { get { return null; } }

        #endregion
        
        
        public virtual void RefreshInitialState()
        {
            //#$*(# dataitems #$*&#@(
            //we revert the output/dataitem to its original state here
            foreach(var controlGroup in ControlGroups)
            {
                foreach(var output in controlGroup.Outputs)
                {
                    var outputDataItem = GetDataItemByValue(output);
                    if (outputDataItem != null && outputDataItem.LinkedBy.Count > 0)
                    {
                        output.Value = (double) outputDataItem.LinkedBy[0].Value;
                    }
                }
            }
        }

        public virtual void SetTimeLagHydraulicRulesToTimeSteps(IEnumerable<ControlGroup> controlGroupsToUpdate, TimeSpan timeStep)
        {
            foreach (var r in controlGroupsToUpdate.SelectMany(controlGroup => controlGroup.Rules.OfType<HydraulicRule>()))
            {
                r.SetTimeLagToTimeSteps(timeStep);
            }
        }
        
        private IEventedList<ControlGroup> controlGroups;

        public virtual IEventedList<ControlGroup> ControlGroups
        {
            get
            {
                return controlGroups;
            }
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
            get { return coordinateSystem; }
            set
            {
                coordinateSystem = value;
                if (outputFileFunctionStore != null)
                    outputFileFunctionStore.CoordinateSystem = coordinateSystem;
            }
        }

        #region HandleControlGroupRenaming

        private string previousControlGroupName;

        private void ControlGroupsPropertyChanging(object sender, PropertyChangingEventArgs e)
        {
            if (e.PropertyName != "Name") return;
            var controlGroup = sender as ControlGroup;
            if (controlGroup == null) return;

            previousControlGroupName = controlGroup.Name;
        }

        private void ControlGroupsPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName != "Name") return;
            var controlGroup = sender as ControlGroup;
            if (controlGroup == null) return;

            // DELFT3DFM-1441: ControlGroups must have unique names!
            if (ControlGroups.Where(cg => !ReferenceEquals(cg, controlGroup)).Any(cg => cg.Name == controlGroup.Name))
            {
                Log.WarnFormat(Resources.RealTimeControlModel_ControlGroupsPropertyChanged_Unable_to_update_ControlGroup_name__all_ControlGroup_names_must_be_unique__0___1___has_been_reverted_back_to___2__,
                    Environment.NewLine, controlGroup.Name, previousControlGroupName);

                if(controlGroup.Name != previousControlGroupName)
                    controlGroup.Name = previousControlGroupName;

                return;
            }

            // DELFT3DFM-1441: ControlGroup ChildDataItems with should have the ControlGroup DataItem Name as a prefix
            this.SyncControlGroupChildDataItemNames(controlGroup);
        }

        #endregion

        private void SetOutputOutOfSync(object sender, PropertyChangedEventArgs e)
        {
            MarkOutputOutOfSync();
        }
        
        private IEventedList<IModel> internalControlledModelsList;

        protected virtual IEventedList<IModel> InternalControlledModelsList
        {
            get { return internalControlledModelsList; }
            set
            {
                if (internalControlledModelsList != null)
                {
                    ((INotifyPropertyChanged) internalControlledModelsList).PropertyChanged -= ModelsPropertyChanged;
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

        private void ControlledModelsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            // required for project load
            var model = e?.GetRemovedOrAddedItem() as IModel;
            if (model == null) return;

            if (outputFileFunctionStore == null) return;

            ReconnectOutputFiles(outputFileFunctionStore.Path);
        }

        public virtual IEnumerable<IModel> ControlledModels
        {
            get { return internalControlledModelsList; }
        }
        
        private void ModelsPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var model = sender as IModel;
            if (model == null) return;

            if (e.PropertyName == "OutputOutOfSync" && model.OutputOutOfSync)
            {
                // this is another hack, fix the model state machine to handle lower level exception
                MarkOutputOutOfSync();
            }

            if (RunsInIntegratedModel) return;
        }

        protected override void OnInputPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Value" || e.PropertyName == "ConvertedValue")
                return; //shouldn't trigger clearing of output (happens when flow does final execute step after rtc has finished)

            if (Status == ActivityStatus.Failed || Status == ActivityStatus.Cleaned || 
                Status == ActivityStatus.Cancelled || Status == ActivityStatus.Finished)
            {
                MarkOutputOutOfSync();
            }
        }
        

        public override string KernelVersions
        {
            get
            {
                if (!File.Exists(DimrApiDataSet.RtcToolsDllPath))
                    return "";

                return "Kernel: " + RealTimeControlModelDll.RTCTOOLS_DLL_NAME + "  " + FileVersionInfo.GetVersionInfo(DimrApiDataSet.RtcToolsDllPath).FileVersion;
            }
        }

        public virtual IEnumerable<IFeatureCoverage> OutputFeatureCoverages
        {
            get
            {
                return outputFileFunctionStore != null && outputFileFunctionStore.Functions != null
                ? outputFileFunctionStore.Functions.OfType<IFeatureCoverage>()
                : Enumerable.Empty<IFeatureCoverage>();
            }
        }

        public override bool CanCopy(IDataItem item)
        {
            if (item.Value is FileBasedRestartState)
            {
                return true;
            }
            return base.CanCopy(item);
        }
        ///<exception cref="NotSupportedException">When a <see cref="DataItem"/> (either in this model or it's child-models) is unlinked and the <see cref="DataItem.Value"/> either does not inherit from <see cref="ICloneable"/>, is not null, or is not a value type.</exception>
        ///<exception cref="InvalidOperationException">
        /// When attempting to perform deep clone a <see cref="DataItemSet"/> (either in this model or it's child-models) for which a <see cref="IDataItem"/>s <see cref="IDataItem.Owner"/> is not the data item set.</exception>
        public override IProjectItem DeepClone()
        {
            var clonedModel = new RealTimeControlModel { Name = Name };
            // with rewiring of links between models the origin is changed as well as the clone.
            cloning = true;
            suspendUpdateFeatureAndParameter = true;

            clonedModel.cloning = true;
            clonedModel.SuspendClearOutputOnInputChange = true;
            clonedModel.OutputOutOfSync = OutputOutOfSync;
            clonedModel.LimitMemory = LimitMemory;

            clonedModel.DataItems.Clear(); // re-clone all data items
            foreach (var dataItem in DataItems)
            {
                clonedModel.DataItems.Add((IDataItem)dataItem.DeepClone());
            }
            
            // add control groups from the cloned data items, otherwise they are cloned twice
            foreach (var controlGroup in ControlGroups)
            {
                var controlGroupDataItem = GetDataItemByValue(controlGroup);
                var controlGroupDataItemIndex = DataItems.IndexOf(controlGroupDataItem);
                var controlGroupDataItemClone = clonedModel.DataItems[controlGroupDataItemIndex];

                // restore links to Inputs / Outputs in child data items
                var controlGroupDataItemObjects = controlGroupDataItem.GetAllItemsRecursive().ToList();
                var controlGroupDataItemCloneObjects = controlGroupDataItemClone.GetAllItemsRecursive().ToList();

                foreach (var childDataItem in controlGroupDataItemClone.Children.Where(di => di.ValueConverter is PropertyValueConverter))
                {
                    var propertyValueConverterClone = (PropertyValueConverter)childDataItem.ValueConverter;
                    var propertyValueConverter = (PropertyValueConverter)controlGroupDataItemObjects[controlGroupDataItemCloneObjects.IndexOf(propertyValueConverterClone)];

                    var originalValueIndex = controlGroupDataItemObjects.IndexOf(propertyValueConverter.OriginalValue);
                    propertyValueConverterClone.OriginalValue = controlGroupDataItemCloneObjects[originalValueIndex];
                }

                var clonedControlGroup = (ControlGroup) controlGroupDataItemClone.Value;
                clonedControlGroup.Name = controlGroup.Name;
                clonedModel.ControlGroups.Add(clonedControlGroup);
            }

            cloning = false;
            suspendUpdateFeatureAndParameter = false;

            clonedModel.RelinkInternalDataItemLinks(this); // should reconnect all data items

            clonedModel.cloning = false;
            clonedModel.SuspendClearOutputOnInputChange = false;

            foreach (var model in clonedModel.ControlledModels)
            {
                model.SuspendClearOutputOnInputChange = false;
            }
            
            if (outputFileFunctionStore != null && File.Exists(outputFileFunctionStore.Path))
            {
                clonedModel.OutputFileFunctionStore = new RealTimeControlOutputFileFunctionStore()
                {
                    Path = outputFileFunctionStore.Path
                };
            }
            
            return clonedModel;
        }

        #region IRealTimeControlModel
        
        public override IEnumerable<object> GetDirectChildren()
        {
            return base.GetDirectChildren().Concat(OutputFeatureCoverages);
        }
        
        /// <summary>
        /// Query connectable locations from controlled models.
        /// </summary>
        /// <param name="role"></param>
        /// <returns></returns>
        public virtual IEnumerable<IFeature> GetChildDataItemLocationsFromControlledModels(DataItemRole role)
        {
            var childDataItemLocationsFromControlledModels = ControlledModels.SelectMany(m => m.GetChildDataItemLocations(role)).Distinct();
            // The childDataItemLocationsFromControlledModels list may contain features that are wrapped in data-items that
            // provide/consuming value types other that typeif(double), e.g. Flow1D's network-coverages for waterlevel's,
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
            // SOBEK3-562: Existing projects can have ControlGroups with locations at inputs/outputs but no underlying dataitem links

            controlGroup.Inputs.Where(input => input.Feature != null).ForEach(input => ResetOrphanedInput(controlGroup, input));
            controlGroup.Outputs.Where(output => output.Feature != null).ForEach(output => ResetOrphanedOutput(controlGroup, output));
        }

        private void ResetOrphanedInput(IControlGroup controlGroup, Input input)
        {
            var inputDataItem = GetDataItemByValue(input);
            if (inputDataItem == null || inputDataItem.LinkedTo != null) return;
            // else Input is Orphaned

            var rtcInputConnections = controlGroup.Rules.Where(r => r.Inputs.Contains(input)).Cast<RtcBaseObject>()
                        .Concat(controlGroup.Conditions.Where(c => c.Input == input))
                        .Concat(controlGroup.Signals.Where(s => s.Inputs.Contains(input)))
                        .ToArray();

            ResetOrphanedConnectionPoint(controlGroup.Name, rtcInputConnections, input);
        }

        private void ResetOrphanedOutput(IControlGroup controlGroup, Output output)
        {
            var outputDataItem = GetDataItemByValue(output);
            if (outputDataItem == null || outputDataItem.LinkedBy.Any()) return;
            // else Output is orphaned

            var rtcOutputConnections = controlGroup.Rules.Where(r => r.Outputs.Contains(output)).Cast<RtcBaseObject>().ToArray();

            ResetOrphanedConnectionPoint(controlGroup.Name, rtcOutputConnections, output);
        }

        private void ResetOrphanedConnectionPoint(string controlGroupName, RtcBaseObject[] connections, ConnectionPoint connectionPoint)
        {
            var connectionTypeName = connectionPoint is Input ? "Input" : "Output"; // Can only be an Input or Output
            var connectionsString = connections.Any() ? string.Join(", ", connections.Select(ic => ic.Name)) : "None";

            Log.WarnFormat(Resources.RealTimeControlModel_BrokenDataItemLinkDetected,
                           Name, controlGroupName, connectionTypeName, connectionsString, 
                           Environment.NewLine, connectionPoint.Name);

            connectionPoint.Reset(); 
        }

        #endregion

        protected override void OnDataItemLinking(object sender, LinkingUnlinkingEventArgs<IDataItem> e)
        {
            base.OnDataItemLinking(sender, e);

            if (e.Source.ValueConverter != null && e.Source.ValueConverter.OriginalValue is Output)
            {
                e.UseValueFromTarget = true; // makes sure that initial value used by Output is set
            }
        }

        [EditAction]
        protected override void OnDataItemAdded(IDataItem item)
        {
            base.OnDataItemAdded(item);
            UpdateFeatureAndParameter(item);
        }

        protected override void OnDataItemLinked(object sender, LinkedUnlinkedEventArgs<IDataItem> e)
        {
            base.OnDataItemLinked(sender, e);

            // update feature
            if (e.Source.ValueConverter != null && e.Source.ValueConverter.OriginalValue is ConnectionPoint)
            {
                UpdateFeatureAndParameter(e.Source);
            }
            else if (e.Target.ValueConverter != null && e.Target.ValueConverter.OriginalValue is ConnectionPoint)
            {
                UpdateFeatureAndParameter(e.Target);
            }
        }

        private bool suspendUpdateFeatureAndParameter;

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
                if (dataItem.ValueConverter == null || !(dataItem.ValueConverter.OriginalValue is ConnectionPoint))
                {
                    return;
                }


                connection = (ConnectionPoint) dataItem.ValueConverter.OriginalValue;
                if (connection.Feature != null)
                    lastRelinkedFeature = connection.Feature;
                connection.Feature = dataItem.LinkedTo.GetFeature();
                connection.ParameterName = dataItem.LinkedTo.GetParameterName();
                connection.UnitName = dataItem.LinkedTo.GetUnitName();

                ReplaceSingleFeatureInOutputFeatureCoverages(lastRelinkedFeature, connection.Feature); //(part of clone logic)
            }

            if (dataItem.LinkedBy.Count == 0 || dataItem.ValueConverter == null || !(dataItem.ValueConverter.OriginalValue is ConnectionPoint))
            {
                return;
            }

            if (dataItem.LinkedBy.Count > 1)
            {
                throw new NotSupportedException("Use of RTC output in more than one consumer is not supported yet");
            }

            connection = (ConnectionPoint) dataItem.ValueConverter.OriginalValue;
            if (connection.Feature != null)
                lastRelinkedFeature = connection.Feature;
            connection.Feature = dataItem.LinkedBy.First().GetFeature();
            connection.ParameterName = dataItem.LinkedBy.First().GetParameterName();
            connection.UnitName = dataItem.LinkedBy.First().GetUnitName();

            ReplaceSingleFeatureInOutputFeatureCoverages(lastRelinkedFeature, connection.Feature); //(part of clone logic)
        }

        private void ReplaceSingleFeatureInOutputFeatureCoverages(IFeature before, IFeature after)
        {
            if (before == null || after == null)
                return;

            foreach (var outputCoverage in OutputFeatureCoverages)
            {
                var featuresBefore = outputCoverage.Features;
                var featureInCoverage = false;
                var featuresAfter = featuresBefore.Select(f =>
                    {
                        if (Equals(f, before))
                        {
                            featureInCoverage = true;
                            return after;
                        }
                        return f;
                    }).ToList();

                if (featureInCoverage)
                {
                    FeatureCoverage.RefreshAfterClone(outputCoverage, featuresBefore, featuresAfter);
                }
            }
        }

        protected override void OnDataItemUnlinked(object sender, LinkedUnlinkedEventArgs<IDataItem> e)
        {
            base.OnDataItemUnlinked(sender, e);

            // update feature
            ConnectionPoint connection = null;
            if (e.Source.ValueConverter != null && e.Source.ValueConverter.OriginalValue is ConnectionPoint)
            {
                connection = (ConnectionPoint) e.Source.ValueConverter.OriginalValue;
            }
            else if (e.Target.ValueConverter != null && e.Target.ValueConverter.OriginalValue is ConnectionPoint)
            {
                connection = (ConnectionPoint) e.Target.ValueConverter.OriginalValue;
            }

            if (connection != null)
            {
                lastRelinkedFeature = e.Relinking ? connection.Feature : null;
                connection.Feature = null;
                connection.ParameterName = string.Empty;
                connection.UnitName = string.Empty;
            }
        }

        public override bool IsLinkAllowed(IDataItem source, IDataItem target)
        {
            return false;
        }

        #region State Aware Model

        private ModelFileBasedStateHandler modelStateHandler;
        private IFeature lastRelinkedFeature;
        private static readonly int[] SupportedMetaDataVersions = new[] { 1 };
        protected virtual Queue<DateTime> outputWriteTimesQueue { get; set; }

        IModelState IStateAwareModelEngine.GetCopyOfCurrentState()
        {
            return ModelStateHandler.GetState();
        }

        void IStateAwareModelEngine.SetState(IModelState modelState)
        {
            ModelStateHandler.FeedStateToModel(modelState);
        }

        void IStateAwareModelEngine.ReleaseState(IModelState modelState)
        {
            ModelStateHandler.ReleaseState(modelState);
        }

        IModelState IStateAwareModelEngine.CreateStateFromFile(string persistentStateFilePath)
        {
            return ModelStateHandler.CreateStateFromFile(Name, persistentStateFilePath);
        }

        #region Save State: Time Range

        public virtual DateTime SaveStateStartTime { get; set; }

        public virtual DateTime SaveStateStopTime { get; set; }

        public virtual TimeSpan SaveStateTimeStep { get; set; }

        #endregion

        public virtual IEnumerable<DateTime> GetRestartWriteTimes()
        {
                var time = SaveStateStartTime;
                while (time <= SaveStateStopTime)
                {
                    yield return time;

                    time += SaveStateTimeStep;
                }
        }

        void IStateAwareModelEngine.SaveStateToFile(IModelState modelState, string persistentStateFilePath)
        {
            modelState.MetaData = new ModelStateMetaData
            {
                ModelTypeId = "RealTimeControlModel",
                Version = SupportedMetaDataVersions.Last(),
                Attributes = GetMetaDataRequirements(SupportedMetaDataVersions.Last())
            };
            ModelStateHandler.SaveStateToFile(modelState, persistentStateFilePath);
        }

        public virtual void ValidateInputState(out IEnumerable<string> errors, out IEnumerable<string> warnings)
        {
            try
            {
                var modelState = (ModelStateFilesImpl)ModelStateHandler.CreateStateFromFile("validate", RestartInput.Path);
                errors = ModelStateValidator.ValidateInputState(modelState, SupportedMetaDataVersions, GetMetaDataRequirements, "RealTimeControlModel");
                warnings = Enumerable.Empty<string>();
            }
            catch (ArgumentException e)
            {
                errors = new[] { e.Message };
                warnings = Enumerable.Empty<string>();
            }
        }

        private Dictionary<string, string> GetMetaDataRequirements(int version)
        {
            if (version == 1)
            {
                var ruleTypesPerControlGroup = "";
                foreach (var controlGroup in ControlGroups.OrderBy(cg => cg.Name))
                {
                    ruleTypesPerControlGroup += controlGroup.Rules.Aggregate("(", (current, rule) => current + rule.GetType().Name + ",");
                    ruleTypesPerControlGroup += "),";
                }

                var conditionTypesPerControlGroup = "";
                foreach (var controlGroup in ControlGroups.OrderBy(cg => cg.Name))
                {
                    conditionTypesPerControlGroup += controlGroup.Conditions.Aggregate("(", (current, condition) => current + condition.GetType().Name + ",");
                    conditionTypesPerControlGroup += "),";
                }
                return new Dictionary<string, string>
                    {
                        {"NrOfControlGroups", ControlGroups.Count.ToString(CultureInfo.InvariantCulture)},
                        {"NrOfRulesPerControlGroups", ControlGroups.OrderBy(cg => cg.Name)
                                     .Select(cg => cg.Rules.Count)
                                     .Aggregate("", (current, rulesCount) => current + rulesCount+ ",")},
                        {"RuleTypesPerControlGroup", ruleTypesPerControlGroup},
                        {"NrOfConditionsPerControlGroups", ControlGroups.OrderBy(cg => cg.Name)
                                     .Select(cg => cg.Conditions.Count)
                                     .Aggregate("", (current, conditionsCount) => current + conditionsCount + ",")},
                        {"ConditionTypesPerControlGroup", conditionTypesPerControlGroup},
                    };
            }

            throw new NotImplementedException(string.Format("Meta data version {0} for model type {1} is not supported",
                                                            version, "RealTimeControlModel"));
        }

        public virtual ModelFileBasedStateHandler ModelStateHandler
        {
            get
            {
                if (modelStateHandler == null)
                {
                    IList<DelftTools.Utils.Tuple<string, string>> outAndInFileNames = new List<DelftTools.Utils.Tuple<string, string>>();
                    outAndInFileNames.Add(new DelftTools.Utils.Tuple<string, string>(RealTimeControlXMLFiles.XmlExportState, RealTimeControlXMLFiles.XmlImportState));
                    modelStateHandler = new ModelFileBasedStateHandler(Name, outAndInFileNames);
                }
                return modelStateHandler;
            }
        }

        #endregion
        
        #region IModelMerge
        public virtual ValidationReport ValidateMerge(IModelMerge sourceModel)
        {
            if (!CanMerge(sourceModel))
            {
                return new ValidationReport(Name + " (Real Time Control)", new[]
                                                                   {
                                                                       new ValidationReport("Model", new [] { new ValidationIssue(sourceModel, ValidationSeverity.Error, string.Format("sourceModel {0} (of type {1}) can't be merged with this model {2} (of type {3})",sourceModel.Name, sourceModel.GetType(),Name,GetType())) })
                                                                   });
            }

            return new RealTimeControlModelMergeValidator().Validate(this, (RealTimeControlModel)sourceModel);
        }

        public virtual bool Merge(IModelMerge sourceModel, IDictionary<IModelMerge, IModelMerge> mergedDependendModelsLookup)
        {
            if (!CanMerge(sourceModel))
            {
                return false;
            }
            var srcModel = sourceModel as RealTimeControlModel;
            if (srcModel == null) return false;

            var existingControlGroupNames = ControlGroups.Select(cg => cg.Name).ToList();
            foreach (var controlGroup in srcModel.ControlGroups)
            {
                var clonedControlGroup = (ControlGroup) controlGroup.Clone();
                if (existingControlGroupNames.Contains(clonedControlGroup.Name))
                {
                    var uniqueName = NamingHelper.GenerateUniqueNameFromList(controlGroup.Name + "{0}", true, existingControlGroupNames);
                    Log.InfoFormat(Resources.RealTimeControlModel_Merge_There_already_exists_a_ControlGroup_named__0__in_Model__1___ControlGroup__0__will_be_renamed_to__2_,
                        clonedControlGroup.Name, this.Name, uniqueName);

                    clonedControlGroup.Name = uniqueName;
                }
                ControlGroups.Add(clonedControlGroup);
                existingControlGroupNames.Add(clonedControlGroup.Name);
            }

            if (mergedDependendModelsLookup == null) return true;
            
            foreach (var sourceDependentModel in mergedDependendModelsLookup.Keys)
            {
                var mergedDependentModel = mergedDependendModelsLookup[sourceDependentModel] as IModel;
                    
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
            if (sourceModelOriginalValue == null) return; // Shouldn't ever happen

            var matchingRtcModelDataItem = AllDataItems.FirstOrDefault(
                di => di.LinkedTo == null &&
                      di.ValueConverter != null &&
                      di.ValueConverter.OriginalValue is Input &&
                      ((Input)di.ValueConverter.OriginalValue).Name == sourceModelOriginalValue.Name);

            if (matchingRtcModelDataItem == null) return;

            var dependentModelDataItemToLink = GetMatchingDataItemToLink(sourceModelInput.LinkedTo, DataItemRole.Output, sourceModelOriginalValue.ParameterName, mergedDependentModel);

            if (dependentModelDataItemToLink != null) // ok, lets relink!
                matchingRtcModelDataItem.LinkTo(dependentModelDataItemToLink);
        }

        private void RelinkDataItemsForMergedOutputs(IDataItem sourceModelOutput, IModel mergedDependentModel)
        {
            var sourceModelOriginalValue = sourceModelOutput.ValueConverter.OriginalValue as Output;
            if (sourceModelOriginalValue == null) return; // Shouldn't ever happen

            var matchingRtcModelDataItem = AllDataItems.FirstOrDefault(
                di => (di.LinkedBy == null || (di.LinkedBy.Count != sourceModelOutput.LinkedBy.Count)) &&
                       di.ValueConverter != null &&
                       di.ValueConverter.OriginalValue is Output &&
                       ((Output)di.ValueConverter.OriginalValue).Name == sourceModelOriginalValue.Name);

            if (matchingRtcModelDataItem == null) return;

            foreach (var sourceLinkedDataItem in sourceModelOutput.LinkedBy)
            {
                var dependentModelDataItemToLink = GetMatchingDataItemToLink(sourceLinkedDataItem, DataItemRole.Input, sourceModelOriginalValue.ParameterName, mergedDependentModel);

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
            var matchingFeature = mergedDependentModel.GetChildDataItemLocations(role)
                .FirstOrDefault(di => sourceLinkedDataItem.ValueConverter != null && di.Equals(sourceLinkedDataItem.ValueConverter.OriginalValue));

            if (matchingFeature == null) return null;
            
            var dataItemToLink = mergedDependentModel.GetChildDataItems(matchingFeature)
                .FirstOrDefault(di => di.Name.Contains(parameterName));

            return dataItemToLink;
        }

        public virtual bool CanMerge(object sourceModel)
        {
            //return sourceModel is RealTimeControlModel;
            var rtcModel = sourceModel as RealTimeControlModel;
            if (rtcModel == null) return false;

            return rtcModel.DependendModels.All(m => DependendModels.Any(dm => dm.CanMerge(m)));
        }

        public virtual IEnumerable<IModelMerge> DependendModels { get { return ControlledModels.OfType<IModelMerge>(); }}

        public virtual string OutputFileName
        {
            get { return outputFileName; }
            set
            {
                if (!string.IsNullOrEmpty(value))
                {
                    outputFileName = value + ".nc";
                }
            }
        }

        #endregion

        public void Dispose()
        {
            // Ensure all stores are closed
            var fileStores = AllDataItems.Where(di => di.LinkedTo == null && di.ValueType.Implements(typeof(IFunction)))
                    .Select(di => di.Value).OfType<IFunction>()
                    .Select(nc => nc.Store).OfType<IFileBased>();

            foreach (var fileStore in fileStores)
            {
                fileStore.Close();
            }
        }

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
                ModelStateHandler.ModelWorkingDirectory = ExplicitWorkingDirectory;
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
            if (RunsInIntegratedModel) return;

            OutputOutOfSync = false;
            runner.OnExecute();
        }

        protected override void OnCleanup()
        {
            
            // Restore linked dataitems to their original values
            // Needs to be done here as RestartState is written in TimeDependentModel.Finish()
            foreach (var dataItem in linkedDataItemsOriginalValues)
            {
                var currentDataItem = AllDataItems.FirstOrDefault(
                    di => di.LinkedBy.Count > 0 &&
                            di.LinkedBy[0].Name.Equals(dataItem.Name) &&
                            di.LinkedBy[0].Tag.Equals(dataItem.Tag));

                if (currentDataItem != null) currentDataItem.Value = dataItem.Value;
            }

            linkedDataItemsOriginalValues.Clear();

            SuspendClearOutputOnInputChange = false;
            outputWriteTimesQueue = null;

            // Clear the explicit value converter lookup if relevant
            if (explicitValueConverterLookupItems != null)
            {
                explicitValueConverterLookupItems.Clear();
            }

            base.OnCleanup();
            runner.OnCleanup();
        }
        #endregion

        #region Implementation of IDimrStateAwareModel

        public virtual void PrepareRestart()
        {
            ModelStateHandler.ModelWorkingDirectory = ExplicitWorkingDirectory;
            if (UseRestart)
            {
                if (RestartInput.IsEmpty)
                {
                    throw new InvalidOperationException("Cannot use restart; restart empty!");
                }
                ModelStateHandler.FeedStateToModel(ModelStateHandler.CreateStateFromFile(Name, RestartInput.Path));
            }
            ClearStatesIfRequired();
        }

        public virtual void WriteRestartFiles()
        {
            WriteRestartIfRequired(false);
        }

        public virtual void FinalizeRestart()
        {
            WriteRestartIfRequired(true);
        }

        #endregion
    }
}