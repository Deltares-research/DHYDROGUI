using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using DelftTools.Functions;
using DelftTools.Functions.Filters;
using DelftTools.Functions.Generic;
using DelftTools.Hydro;
using DelftTools.Hydro.SewerFeatures;
using DelftTools.Shell.Core.Workflow.DataItems;
using DelftTools.Units;
using DelftTools.Utils;
using DelftTools.Utils.Aop;
using DelftTools.Utils.ComponentModel;
using DeltaShell.NGHS.IO.Properties;
using GeoAPI.Extensions.Feature;
using GeoAPI.Extensions.Networks;
using GeoAPI.Geometries;
using NetTopologySuite.Extensions.Features.Generic;

namespace DeltaShell.NGHS.IO.DataObjects
{
    [Entity(FireOnCollectionChange=false)]
    public class Model1DBoundaryNodeData : FeatureData<IFunction, INode>, IItemContainer, IFeature
    {
        private Model1DBoundaryNodeDataType dataType;
        private IDataItem flowConstantDataItem;
        private IDataItem seriesDataItem;
        private bool useSalt;
        private bool useTemperature;
        private IDataItem waterLevelConstantDataItem;
        private bool waterLevelOnly;

        // Functions used to ensure constant Q or H is visible via FeatureData<IFunction, INode>.Data interface
        private readonly Variable<double> flowConstantFunction;
        private readonly Variable<double> waterLevelConstantFunction;

        public Model1DBoundaryNodeData()
        {
            // data item to hold function if boundary is not a constant
            SeriesDataItem = new DataItem
                                 {
                                     Name = "water flow model source data",
                                     ValueType = typeof (Function),
                                     Role = DataItemRole.Input
                                 };

            // data item to hold constant flow
            FlowConstantDataItem = new DataItem
                                       {
                                           Name = "water flow value",
                                           ValueType = typeof (FlowParameter),
                                           Role = DataItemRole.Input,
                                           Value = new FlowParameter()
                                       };

            flowConstantFunction = new Variable<double> { Name = "water flow value", Values = { 0 } };
            flowConstantFunction.Attributes[FunctionAttributes.StandardName] = FunctionAttributes.StandardNames.WaterDischarge;

            // data item to hold constant water level
            WaterLevelConstantDataItem = new DataItem
                                             {
                                                 Name = "Water level value",
                                                 ValueType = typeof (WaterLevelParameter),
                                                 Role = DataItemRole.Input,
                                                 Value = new WaterLevelParameter()
                                             };

            waterLevelConstantFunction = new Variable<double> { Name = "water level value", Values = { 0 } };
            waterLevelConstantFunction.Attributes[FunctionAttributes.StandardName] = FunctionAttributes.StandardNames.WaterLevel;

            DataType = Model1DBoundaryNodeDataType.None;
        }

        /// <summary>
        /// Function to hold data if boundary is not a constant. So (Q(h),Q(t) or H(q))
        /// </summary>
        [Aggregation]
        public override IFunction Data
        {
            get
            {
                // Bit of a HACK, needs a cleanup when this class will be refactored!
                switch (DataType)
                {
                    case Model1DBoundaryNodeDataType.None:
                        return null;

                    case Model1DBoundaryNodeDataType.FlowConstant:
                            flowConstantFunction.Values[0] = Flow;
                            return flowConstantFunction;

                    case Model1DBoundaryNodeDataType.WaterLevelConstant:
                            waterLevelConstantFunction.Values[0] = WaterLevel;
                            return waterLevelConstantFunction;

                    case Model1DBoundaryNodeDataType.FlowWaterLevelTable:
                    case Model1DBoundaryNodeDataType.FlowTimeSeries:
                    case Model1DBoundaryNodeDataType.WaterLevelTimeSeries:
                        return (IFunction)SeriesDataItem.Value;
                }

                return null;
            }
            set
            {
                SwitchToAppropriateType(value);
                SetSeriesDataItemValue(value);
                base.Data = value;
            }
        }

        [EditAction]
        private void SetSeriesDataItemValue(IFunction value)
        {
            SeriesDataItem.Value = value;
        }

        [Aggregation]
        public override INode Feature
        {
            get { return base.Feature; }
            set
            {
                if (null != base.Feature)
                {
                    ((INotifyPropertyChange) Feature).PropertyChanged -= FeatureDataPropertyChanged;
                    Feature.IncomingBranches.CollectionChanged -= BranchCollectionChanged;
                    Feature.OutgoingBranches.CollectionChanged -= BranchCollectionChanged;
                    var hydroObject = Feature as IHydroObject;
                    if (hydroObject != null)
                    {
                        hydroObject.Links.CollectionChanged -= LinkCollectionChanged; 
                    }
                }

                base.Feature = value;

                waterLevelOnly = Feature != null && Feature.IsConnectedToMultipleBranches;

                if (null != base.Feature)
                {
                    ((INotifyPropertyChange) Feature).PropertyChanged += FeatureDataPropertyChanged;
                    Feature.IncomingBranches.CollectionChanged += BranchCollectionChanged;
                    Feature.OutgoingBranches.CollectionChanged += BranchCollectionChanged;
                    var hydroObject = Feature as IHydroObject;
                    if (hydroObject != null)
                    {
                        hydroObject.Links.CollectionChanged += LinkCollectionChanged;
                    }
                }
            }
        }

        private void LinkCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (DataType == Model1DBoundaryNodeDataType.None)
            {
                DataType = Model1DBoundaryNodeDataType.FlowConstant;
                FlowParameter fp = FlowConstantDataItem.Value as FlowParameter;
                fp.Value = 0.0; 
            }
        }

        public bool WaterLevelOnly
        {
            get { return waterLevelOnly; }
            set
            {
                waterLevelOnly = value;

                AfterSetWaterLevelOnly();
            }
        }

        [FeatureAttribute]
        public override string Name
        {
            get
            {
                string name = GetNameForDataType();
                if (IsLinked)
                {
                    name += " (" + GetLinkedDataItemName() + ")";
                }
                return name;
            }
        }

        [FeatureAttribute]
        [DynamicReadOnly]
        public virtual Model1DBoundaryNodeDataType DataType
        {
            get { return dataType; }
            set
            {
                if (dataType == value)
                {
                    // No type change: keep function intact
                    return;
                }

                //unlink as soon as type changes. Don't want to mess up linked data.
                UnlinkDataItems();

                dataType = value;

                AfterSetDataType();
            }
        }

        [FeatureAttribute]
        [DisplayName("Outlet Compartment")]
        [ReadOnly(true)]
        public virtual OutletCompartment OutletCompartment { get; set; }

        [DynamicReadOnlyValidationMethod]
        public virtual bool DynamicReadOnlyValidationMethod(string propertyName)
        {
            return propertyName != nameof(DataType) ||
                   ((!(Node is Manhole manhole) || !manhole.Compartments.OfType<OutletCompartment>().Any()) &&
                    Node is Manhole);
        }

        public virtual InterpolationType InterpolationType
        {
            get
            {
                if (DataType == Model1DBoundaryNodeDataType.FlowConstant ||
                    DataType == Model1DBoundaryNodeDataType.WaterLevelConstant ||
                    DataType == Model1DBoundaryNodeDataType.None)
                {
                    return InterpolationType.None;
                }
                return Data.Arguments[0].InterpolationType;
            }
        }

        public virtual InterpolationType SaltInterpolationType
        {
            get
            {
                return SaltConditionType == SaltBoundaryConditionType.TimeDependent
                           ? SaltConcentrationTimeSeries.Arguments[0].InterpolationType
                           : InterpolationType.Linear;
            }
        }

        public virtual InterpolationType TemperatureInterpolationType
        {
            get
            {
                return TemperatureConditionType == TemperatureBoundaryConditionType.TimeDependent
                           ? TemperatureTimeSeries.Arguments[0].InterpolationType
                           : InterpolationType.Linear;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public IDataItem SeriesDataItem
        {
            get { return seriesDataItem; }
            set
            {
                UnSubcribeDataItemEvents(seriesDataItem);
                seriesDataItem = value;
                SubscribeDataItemEvents(value);
            }
        }

        public IDataItem FlowConstantDataItem
        {
            get { return flowConstantDataItem; }
            set
            {
                UnSubcribeDataItemEvents(flowConstantDataItem);
                flowConstantDataItem = value;
                SubscribeDataItemEvents(value);
            }
        }

        public IDataItem WaterLevelConstantDataItem
        {
            get { return waterLevelConstantDataItem; }
            set
            {
                UnSubcribeDataItemEvents(waterLevelConstantDataItem);
                waterLevelConstantDataItem = value;
                SubscribeDataItemEvents(value);
            }
        }

        /// <summary>
        /// Constant waterlevel used if DataType is ConstantWaterLevel
        /// </summary>
        [NoNotifyPropertyChange]
        public double WaterLevel
        {
            get { return ((WaterLevelParameter) WaterLevelConstantDataItem.Value).Value; }
            set { ((WaterLevelParameter) WaterLevelConstantDataItem.Value).Value = value; }
        }

        /// <summary>
        /// Constant flow used if DataType is ConstantWaterFlow
        /// </summary>
        [NoNotifyPropertyChange]
        public double Flow
        {
            get { return ((FlowParameter) FlowConstantDataItem.Value).Value; }
            set { ((FlowParameter) FlowConstantDataItem.Value).Value = value; }
        }

        /// <summary>
        /// IsLinked is true if the current value (or series) is determined by a linked item
        /// </summary>
        public bool IsLinked
        {
            get { return SeriesDataItem.IsLinked; }
        }

        /// <summary>
        /// Determines whether the BC CAN have salinity data.
        /// </summary>
        public bool UseSalt
        {
            get { return useSalt; }
            set
            {
                //don't enable if not changed..
                if (useSalt == value)
                    return;

                useSalt = value;

                //a change has come..disable or enable
                if (useSalt)
                    EnableSalt();
                else
                    DisableSalt();
            }
        }

        /// <summary>
        /// Determines whether the BC CAN have temperature data.
        /// </summary>
        public bool UseTemperature
        {
            get { return useTemperature; }
            set
            {
                //don't enable if not changed..
                if (useTemperature == value)
                    return;

                useTemperature = value;

                //a change has come..disable or enable
                if (useTemperature)
                    EnableTemperature();
                else
                    DisableTemperature();
            }
        }

        /// <summary>
        /// Type of salt condition. Can be none (default)
        /// </summary>
        public SaltBoundaryConditionType SaltConditionType { get; set; }

        /// <summary>
        /// Type of temperature condition. Can be none (default)
        /// </summary>
        public TemperatureBoundaryConditionType TemperatureConditionType { get; set; }

        //change to DataItems and Parameters when we are going to support linking these things..

        /// <summary>
        /// Concentration used if salt is constant.
        /// </summary>
        public double SaltConcentrationConstant { get; set; }

        /// <summary>
        /// Value used if temperature is constant.
        /// </summary>
        public double TemperatureConstant { get; set; }

        /// <summary>
        /// = return time
        /// </summary>
        public double ThatcherHarlemannCoefficient { get; set; }

        public ITimeSeries SaltConcentrationTimeSeries { get; set; }

        public ITimeSeries TemperatureTimeSeries { get; set; }

        public INode Node
        {
            get { return Feature as INode; }
        }

        #region ICloneable Members

        public object Clone()
        {
            var clonedWaterFlowModel1DBoundaryNodeData =
                (Model1DBoundaryNodeData) Activator.CreateInstance(GetType());
            clonedWaterFlowModel1DBoundaryNodeData.DataType = DataType;
            clonedWaterFlowModel1DBoundaryNodeData.seriesDataItem = (IDataItem) seriesDataItem.DeepClone();
            clonedWaterFlowModel1DBoundaryNodeData.flowConstantDataItem = (IDataItem) FlowConstantDataItem.DeepClone();
            clonedWaterFlowModel1DBoundaryNodeData.waterLevelConstantDataItem =
                (IDataItem) WaterLevelConstantDataItem.DeepClone();

            clonedWaterFlowModel1DBoundaryNodeData.UseSalt = UseSalt;
            clonedWaterFlowModel1DBoundaryNodeData.SaltConditionType = SaltConditionType;
            clonedWaterFlowModel1DBoundaryNodeData.ThatcherHarlemannCoefficient = ThatcherHarlemannCoefficient;
            clonedWaterFlowModel1DBoundaryNodeData.SaltConcentrationConstant = SaltConcentrationConstant;
            if (SaltConcentrationTimeSeries != null)
            {
                clonedWaterFlowModel1DBoundaryNodeData.SaltConcentrationTimeSeries =
                    (ITimeSeries) SaltConcentrationTimeSeries.Clone(true);
            }

            clonedWaterFlowModel1DBoundaryNodeData.UseTemperature = UseTemperature;
            clonedWaterFlowModel1DBoundaryNodeData.TemperatureConditionType = TemperatureConditionType;
            clonedWaterFlowModel1DBoundaryNodeData.TemperatureConstant = TemperatureConstant;
            if (TemperatureTimeSeries != null)
            {
                clonedWaterFlowModel1DBoundaryNodeData.TemperatureTimeSeries =
                    (ITimeSeries)TemperatureTimeSeries.Clone(true);
            }
            
            return clonedWaterFlowModel1DBoundaryNodeData;
        }

        #endregion

        [EditAction]
        private void HandleDataItemChanges(object sender, PropertyChangedEventArgs e)
        {
            if ((e.PropertyName == "Name") || (e.PropertyName == "LinkedTo" || e.PropertyName == "Value"))
            {
                //force a property changed on name like this.
                UpdateName();
            }
        }

        [EditAction]
        private void SwitchToAppropriateType(IFunction value)
        {
            if (value == null)
            {
                return;
            }
            
            Model1DBoundaryNodeDataType targetType;
            if (value is TimeSeries)
            {
                targetType = Enumerable.FirstOrDefault<Model1DBoundaryNodeDataType>(Helper1D.GetTimeSeriesDataTypes(value));
            }
            else if (value.Arguments.Count > 0 && value.Arguments[0].ValueType != typeof (DateTime))
            {
                targetType = Model1DBoundaryNodeDataType.FlowWaterLevelTable;
            }
            else
            {
                throw new ArgumentException("Unable to infer function data type. Cannot set function to boundary data");
            }

            if (DataType != targetType)//unnecessary set leads to property changed and unwanted side effects
            {
                DataType = targetType;
            }
        }

        [EditAction]
        private void FeatureDataPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Name")
            {
                UpdateName();
            }
        }

        [EditAction]
        private void BranchCollectionChanged(object sender, NotifyCollectionChangedEventArgs NotifyCollectionChangedEventArgs)
        {
            if (WaterLevelOnly != Feature.IsConnectedToMultipleBranches)
            {
                WaterLevelOnly = Feature.IsConnectedToMultipleBranches;
            }
        }

        [EditAction]
        private void AfterSetWaterLevelOnly()
        {
            if (waterLevelOnly
                && !(DataType == Model1DBoundaryNodeDataType.WaterLevelConstant
                     || DataType == Model1DBoundaryNodeDataType.WaterLevelTimeSeries
                     || DataType == Model1DBoundaryNodeDataType.None))
            {
                DataType = Model1DBoundaryNodeDataType.WaterLevelConstant; // Reset to water level
            }
        }

        [EditAction]
        private void AfterSetDataType()
        {
            if (Node is Manhole manhole)
                OutletCompartment = manhole.Compartments.OfType<OutletCompartment>().FirstOrDefault();

            switch (dataType)
            {
                case Model1DBoundaryNodeDataType.WaterLevelTimeSeries:
                    Data = HydroTimeSeriesFactory.CreateWaterLevelTimeSeries();
                    break;
                case Model1DBoundaryNodeDataType.FlowTimeSeries:
                    Data = HydroTimeSeriesFactory.CreateFlowTimeSeries();
                    // set data . The function is completely defined to avoid side effects
                    break;
                case Model1DBoundaryNodeDataType.FlowWaterLevelTable:
                    Data = new FlowWaterLevelTable();
                    break;
                case Model1DBoundaryNodeDataType.None:
                    Data = null;
                    OutletCompartment = null;
                    break;
                case Model1DBoundaryNodeDataType.FlowConstant:
                    Data = null; // To fire PropertyChange for Data, as value to be returned from Data has changed
                    Flow = 0;
                    break;
                case Model1DBoundaryNodeDataType.WaterLevelConstant:
                    Data = null; // To fire PropertyChange for Data, as value to be returned from Data has changed
                    WaterLevel = 0;
                    break;
            }
            Name = Feature + " - " + Data;
        }

        [EditAction]
        private void UnlinkDataItems()
        {
            SeriesDataItem.Unlink();
            FlowConstantDataItem.Unlink();
            WaterLevelConstantDataItem.Unlink();
        }

        private void SubscribeDataItemEvents(IDataItem dataItem)
        {
            if (dataItem == null) return;
            ((INotifyPropertyChanged) dataItem).PropertyChanged += HandleDataItemChanges;
            dataItem.Linking += SeriesDataItemLinking;
            dataItem.Unlinked += SeriesDataItemUnlinked;
        }

        private void UnSubcribeDataItemEvents(IDataItem dataItem)
        {
            if (dataItem == null) return;
            ((INotifyPropertyChanged) dataItem).PropertyChanged -= HandleDataItemChanges;
            dataItem.Linking += SeriesDataItemLinking;
            dataItem.Unlinked -= SeriesDataItemUnlinked;
        }

        [EditAction]
        private void SeriesDataItemLinking(object sender, LinkingUnlinkingEventArgs<IDataItem> e)
        {
            // match current series data item function to the source function
            var sourceTimeSeries = e.Source.Value as TimeSeries;
            if(sourceTimeSeries != null)
            {
                DataType = Enumerable.FirstOrDefault<Model1DBoundaryNodeDataType>(Helper1D.GetTimeSeriesDataTypes(sourceTimeSeries));
            }
            else if (e.Source.Value is FlowWaterLevelTable)
            {
                DataType = Model1DBoundaryNodeDataType.FlowWaterLevelTable;
            }
        }

        private void SeriesDataItemUnlinked(object senderd, LinkedUnlinkedEventArgs<IDataItem> e)
        {
            // restore time series
            AfterSetDataType();
        }

        /// <summary>
        /// Returns the value (Q or H) for a given time. Returns constant if DataType is constant
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        public virtual double GetValue(DateTime time)
        {
            if ((DataType == Model1DBoundaryNodeDataType.WaterLevelTimeSeries) ||
                (DataType == Model1DBoundaryNodeDataType.FlowTimeSeries))
            {
                return Data.Evaluate<double>(time);
            }
            if (DataType == Model1DBoundaryNodeDataType.WaterLevelConstant)
            {
                return WaterLevel;
            }
            if (DataType == Model1DBoundaryNodeDataType.FlowConstant)
            {
                return Flow;
            }
            throw new NotImplementedException("BoundaryNodeDataType not supported.");
        }

        public virtual double GetSaltValue(DateTime time)
        {
            switch (SaltConditionType)
            {
                case SaltBoundaryConditionType.Constant:
                    return SaltConcentrationConstant;
                case SaltBoundaryConditionType.TimeDependent:
                    IVariable timeArgument = SaltConcentrationTimeSeries.Arguments[0];
                    var variableValueFilter = new VariableValueFilter<DateTime>(timeArgument, time);
                    return
                        SaltConcentrationTimeSeries.Evaluate<double>(variableValueFilter);
                default:
                    throw new ArgumentOutOfRangeException("time", "No boundary condition data defined");
            }
        }

        public virtual double GetTemperatureValue(DateTime time)
        {
            switch (TemperatureConditionType)
            {
                case TemperatureBoundaryConditionType.Constant:
                    return TemperatureConstant;
                case TemperatureBoundaryConditionType.TimeDependent:
                    IVariable timeArgument = TemperatureTimeSeries.Arguments[0];
                    return
                        TemperatureTimeSeries.Evaluate<double>(new VariableValueFilter<DateTime>(timeArgument,
                                                                                                       time));
                default:
                    throw new ArgumentOutOfRangeException("time", "No boundary condition data defined");
            }
        }

        private string GetNameForDataType()
        {
            switch (DataType)
            {
                case Model1DBoundaryNodeDataType.FlowTimeSeries:
                    return Feature + " - Q(t)";
                case Model1DBoundaryNodeDataType.WaterLevelTimeSeries:
                    return Feature + " - H(t)";
                case Model1DBoundaryNodeDataType.FlowWaterLevelTable:
                    return Feature + " - Q(h)";
                case Model1DBoundaryNodeDataType.FlowConstant:
                    return Feature + " - Q: " + Flow + " m^3/s";
                case Model1DBoundaryNodeDataType.WaterLevelConstant:
                    return Feature + " - H: " + WaterLevel + " m";
                case Model1DBoundaryNodeDataType.None:
                    return Feature + " - None";
            }
            throw new NotImplementedException("Should not get here");
        }

        public string GetLinkedDataItemName()
        {
            switch (DataType)
            {
                case Model1DBoundaryNodeDataType.FlowConstant:
                    return FlowConstantDataItem.LinkedTo.Name;
                case Model1DBoundaryNodeDataType.WaterLevelConstant:
                    return WaterLevelConstantDataItem.LinkedTo.Name;
                case Model1DBoundaryNodeDataType.FlowTimeSeries:
                case Model1DBoundaryNodeDataType.WaterLevelTimeSeries:
                case Model1DBoundaryNodeDataType.FlowWaterLevelTable:
                    return SeriesDataItem.LinkedTo.Name;
            }
            throw new NotImplementedException("Should never get here");
        }

        [EditAction]
        private void DisableSalt()
        {
            // change the salt condition type
            SaltConcentrationTimeSeries = null;
            SaltConditionType = SaltBoundaryConditionType.None;
        }

        [EditAction]
        private void DisableTemperature()
        {
            // change the temperature condition type
            TemperatureTimeSeries = null;
            TemperatureConditionType = TemperatureBoundaryConditionType.None;
        }

        [EditAction]
        private void EnableSalt()
        {
            //add the dataitems and change the salt condition type
            SaltConcentrationConstant = 0;

            //add TH
            ThatcherHarlemannCoefficient = 0; //some default..


            //add a timeseries 
            SaltConcentrationTimeSeries = new TimeSeries();
            SaltConcentrationTimeSeries.Arguments[0].DefaultValue = new DateTime(2000, 1, 1);
            SaltConcentrationTimeSeries.Arguments[0].InterpolationType = InterpolationType.Linear;
            SaltConcentrationTimeSeries.Arguments[0].ExtrapolationType = ExtrapolationType.Constant;
            SaltConcentrationTimeSeries.Components.Add(new Variable<double>("concentration", new Unit("ppt", "ppt")));
            SaltConcentrationTimeSeries.Name = "Salinity concentration time series";
            SaltConcentrationTimeSeries.Components[0].Attributes[FunctionAttributes.StandardName] =
                FunctionAttributes.StandardNames.WaterSalinity;

            SaltConditionType = SaltBoundaryConditionType.Constant;
        }

        [EditAction]
        private void EnableTemperature()
        {
            //add the dataitems and change the temperature condition type
            TemperatureConstant = 0;

            //add a timeseries 
            TemperatureTimeSeries = new TimeSeries();
            TemperatureTimeSeries.Arguments[0].DefaultValue = new DateTime(2000, 1, 1);
            TemperatureTimeSeries.Arguments[0].InterpolationType = InterpolationType.Linear;
            TemperatureTimeSeries.Arguments[0].ExtrapolationType = ExtrapolationType.Constant;
            TemperatureTimeSeries.Components.Add(new Variable<double>("Temperature", new Unit("degreesCelcius", Resources.degreeCelcius)));
            TemperatureTimeSeries.Name = "Temperature time series";

            //TODO: create standard name
            TemperatureTimeSeries.Components[0].Attributes[FunctionAttributes.StandardName] = "water_temperature";

            TemperatureConditionType = TemperatureBoundaryConditionType.Constant;
        }

        public override string ToString()
        {
            return Name;
        }

        public IEnumerable<object> GetDirectChildren()
        {
            yield return SeriesDataItem;
        }

        # region Implementation of IFeature

        public IGeometry Geometry
        {
            get { return Feature != null ? Feature.Geometry : null; }
            set
            {
                if (Feature != null)
                {
                    Feature.Geometry = value;
                }
            }
        }

        public IFeatureAttributeCollection Attributes
        {
            get { return Feature != null ? Feature.Attributes : null; }
            set
            {
                if (Feature != null)
                {
                    Feature.Attributes = value;
                }
            }
        }

        # endregion
    }
}