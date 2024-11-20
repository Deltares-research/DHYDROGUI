using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using DelftTools.Functions;
using DelftTools.Functions.Generic;
using DelftTools.Hydro;
using DelftTools.Hydro.SewerFeatures;
using DelftTools.Hydro.Structures;
using DelftTools.Shell.Core.Workflow.DataItems;
using DelftTools.Units;
using DelftTools.Utils;
using DelftTools.Utils.Aop;
using DelftTools.Utils.Collections;
using DelftTools.Utils.ComponentModel;
using GeoAPI.Extensions.Feature;
using GeoAPI.Geometries;
using NetTopologySuite.Extensions.Features.Generic;
using NetTopologySuite.Geometries;
using NetTopologySuite.LinearReferencing;

namespace DeltaShell.NGHS.IO.DataObjects
{
    [Entity(FireOnCollectionChange=false)]
    public class Model1DLateralSourceData : FeatureData<IFunction, LateralSource>, IItemContainer, IFeature
    {
        public const double DefaultSalinity = 0.0;

        private Model1DLateralDataType dataType;
        private IDataItem flowConstantDataItem;
        private IDataItem seriesDataItem;
        private bool useSalt;
        private bool useTemperature;

        public Model1DLateralSourceData()
        {
            SeriesDataItem = new DataItem
                                 {
                                     Name = "water flow model source data",
                                     ValueType = typeof (Function),
                                     Role = DataItemRole.Input
                                 };
            FlowConstantDataItem = new DataItem
                                       {
                                           Name = "water flow value",
                                           ValueType = typeof (FlowParameter),
                                           Role = DataItemRole.Input,
                                           Value = new FlowParameter()
                                       };

            Data = HydroTimeSeriesFactory.CreateFlowTimeSeries();
            DataType = Model1DLateralDataType.FlowConstant;
        }

        public override IFunction Data
        {
            get { return (IFunction) SeriesDataItem.Value; }
            set
            {
                SwitchToAppropriateType(value);
                SetSeriesDataItemValue(value);
                base.Data = value;
            }
        }

        private void SetSeriesDataItemValue(IFunction value)
        {
            SeriesDataItem.Value = value;
        }

        public override LateralSource Feature
        {
            get { return base.Feature; }
            set
            {
                if (null != base.Feature)
                {
                    ((INotifyPropertyChanged)base.Feature).PropertyChanged -= FeatureDataPropertyChanged;
                    ((INotifyCollectionChanged)base.Feature).CollectionChanged -= FeatureCollectionChanged;
                }
                base.Feature = value;
                
                if (null != base.Feature)
                {
                    ((INotifyPropertyChanged)base.Feature).PropertyChanged += FeatureDataPropertyChanged;
                    ((INotifyCollectionChanged)base.Feature).CollectionChanged += FeatureCollectionChanged;
                }
            }
        }
        
        [FeatureAttribute(Order = 1)]
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
        [FeatureAttribute(Order = 2)]
        public virtual Model1DLateralDataType DataType
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
        [DynamicReadOnly]
        [NoNotifyPropertyChange]
        public ICompartment Compartment { get; set; }

        [DynamicReadOnlyValidationMethod]
        public virtual bool DynamicReadOnlyValidationMethod(string propertyName)
        {
            if (propertyName == nameof(Compartment))
            {
                var node = Math.Abs(Feature.Branch.Length - Feature.Chainage) < 0.001 ? Feature.Branch.Target :
                    Math.Abs(Feature.Chainage) < 0.001 ? Feature.Branch.Source : null;
                if (node is Manhole manhole && manhole.Compartments.Any()) return false;
            }
            return true;
        }

        private void AfterSetDataType()
        {
            switch (dataType)
            {
                case Model1DLateralDataType.FlowTimeSeries:
                    Data = HydroTimeSeriesFactory.CreateFlowTimeSeries();
                    Name = Feature + " - " + Data;
                    // set data if it the function is completely defined to avoid side effects
                    break;
                case Model1DLateralDataType.FlowWaterLevelTable:
                    Data = new FlowWaterLevelTable();
                    Name = Feature + " - " + Data;
                    break;
                case Model1DLateralDataType.FlowRealTime:
                    Name = Feature.Name + " - Realtime from RR";
                    break;
                default:
                    Name = Feature + " - " + Data;
                    break;
            }
            
        }
        
        public IDataItem SeriesDataItem
        {
            get { return seriesDataItem; }
            set
            {
                UnSubcribeDataItemEvents(seriesDataItem);
                seriesDataItem = value;
                SubscribeDataItemEvents(seriesDataItem);
            }
        }

        public IDataItem FlowConstantDataItem
        {
            get { return flowConstantDataItem; }
            set
            {
                UnSubcribeDataItemEvents(flowConstantDataItem);
                flowConstantDataItem = value;
                SubscribeDataItemEvents(flowConstantDataItem);
            }
        }

        /// <summary>
        /// Constant flow used if DataType is ConstantFlow
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

        public bool UseSalt
        {
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
            get { return useSalt; }
        }

        public bool UseTemperature
        {
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
            get { return useTemperature; }
        }

        public double SaltMassDischargeConstant { get; set; }
        public double SaltConcentrationDischargeConstant { get; set; }

        public double TemperatureConstant { get; set; }

        public ITimeSeries SaltMassTimeSeries { get; set; }

        public SaltLateralDischargeType SaltLateralDischargeType { get; set; }

        public TemperatureLateralDischargeType TemperatureLateralDischargeType { get; set; }

        public ITimeSeries SaltConcentrationTimeSeries { get; set; }

        public ITimeSeries TemperatureTimeSeries { get; set; }

        #region ICloneable Members

        public object Clone()
        {
            var clonedWaterFlowModel1DLateralSourceData = new Model1DLateralSourceData
                                                              {
                                                                  DataType = DataType,
                                                                  SeriesDataItem =
                                                                      (IDataItem) SeriesDataItem.DeepClone(),
                                                                  FlowConstantDataItem =
                                                                      (IDataItem) FlowConstantDataItem.DeepClone()
                                                              };
            if (UseSalt)
            {
                clonedWaterFlowModel1DLateralSourceData.UseSalt = UseSalt;
                clonedWaterFlowModel1DLateralSourceData.SaltConcentrationDischargeConstant =
                    SaltConcentrationDischargeConstant;
                clonedWaterFlowModel1DLateralSourceData.SaltConcentrationTimeSeries =
                    (ITimeSeries) SaltConcentrationTimeSeries.Clone(true);
                clonedWaterFlowModel1DLateralSourceData.SaltLateralDischargeType = SaltLateralDischargeType;
                clonedWaterFlowModel1DLateralSourceData.SaltMassDischargeConstant = SaltMassDischargeConstant;
                clonedWaterFlowModel1DLateralSourceData.SaltMassTimeSeries =
                    (ITimeSeries) SaltMassTimeSeries.Clone(true);
            }

            if (UseTemperature)
            {
                clonedWaterFlowModel1DLateralSourceData.UseTemperature = UseTemperature;
                clonedWaterFlowModel1DLateralSourceData.TemperatureConstant = TemperatureConstant;
                clonedWaterFlowModel1DLateralSourceData.TemperatureTimeSeries =
                    (ITimeSeries)TemperatureTimeSeries.Clone(true);
                clonedWaterFlowModel1DLateralSourceData.TemperatureLateralDischargeType = TemperatureLateralDischargeType;
            }

            return clonedWaterFlowModel1DLateralSourceData;
        }

        #endregion

        private void HandleDataItemChanges(object sender, PropertyChangedEventArgs e)
        {
            if ((e.PropertyName == "Name") || (e.PropertyName == "LinkedTo" || e.PropertyName == "Value"))
            {
                //force a property changed on name like this.
                UpdateName();
            }
        }

        private void SwitchToAppropriateType(IFunction value)
        {
            if (value == null)
            {
                return;
            }

            Model1DLateralDataType targetType;

            if (value is TimeSeries)
            {
                targetType = Model1DLateralDataType.FlowTimeSeries;
            }
            else if (value.Arguments.Count > 0 && value.Arguments[0].ValueType != typeof (DateTime))
            {
                targetType = Model1DLateralDataType.FlowWaterLevelTable;
            }
            else
            {
                throw new ArgumentException("Unable to infer function data type. Cannot set function to lateral data");
            }

            //do the check: unnecessary set leads to property changed and unwanted side effects
            if (DataType != targetType)
            {
                DataType = targetType;
            }
        }

        private void FeatureDataPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Name")
            {
                UpdateName();
            }
        }

        private void UnlinkDataItems()
        {
            SeriesDataItem.Unlink();
            FlowConstantDataItem.Unlink();
        }

        private void SubscribeDataItemEvents(IDataItem dataItem)
        {
            if (dataItem != null)
            {
                ((INotifyPropertyChanged) dataItem).PropertyChanged += HandleDataItemChanges;
                dataItem.Linking += SeriesDataItemLinking;
                dataItem.Unlinked += SeriesDataItemUnlinked;
            }
        }

        private void UnSubcribeDataItemEvents(IDataItem dataItem)
        {
            if (dataItem != null)
            {
                ((INotifyPropertyChanged) dataItem).PropertyChanged -= HandleDataItemChanges;
                dataItem.Linking -= SeriesDataItemLinking;
                dataItem.Unlinked -= SeriesDataItemUnlinked;

            }
        }

        private void SeriesDataItemLinking(object sender, LinkingUnlinkingEventArgs<IDataItem> e)
        {
            if (e.Source.Value is TimeSeries)
            {
                DataType = Model1DLateralDataType.FlowTimeSeries;
            }
            else if (e.Source.Value is FlowWaterLevelTable)
            {
                DataType = Model1DLateralDataType.FlowWaterLevelTable;
            }
        }

        private void SeriesDataItemUnlinked(object sender, LinkedUnlinkedEventArgs<IDataItem> e)
        {
            // restore time series
            AfterSetDataType();
        }

        private string GetNameForDataType()
        {
            switch (DataType)
            {
                case Model1DLateralDataType.FlowTimeSeries:
                    return Feature + " - Q(t)";
                case Model1DLateralDataType.FlowConstant:
                    return Feature + " - Q: " + Flow + " m³/s";
                case Model1DLateralDataType.FlowWaterLevelTable:
                    return Feature + " - Q(h)";
                case Model1DLateralDataType.FlowRealTime:
                    return Feature.ToString();
            }
            return "";
        }

        private void DisableSalt()
        {
            SaltConcentrationTimeSeries = null;
            SaltMassTimeSeries = null;
            SaltLateralDischargeType = SaltLateralDischargeType.Default;
        }

        private void DisableTemperature()
        {
            TemperatureTimeSeries = null;
            TemperatureLateralDischargeType = TemperatureLateralDischargeType.None;
        }

        private void EnableSalt()
        {
            SaltMassDischargeConstant = 0; //get some defaults here
            SaltConcentrationDischargeConstant = 0;

            SaltConcentrationTimeSeries = new TimeSeries();
            SaltConcentrationTimeSeries.Arguments[0].DefaultValue = new DateTime(2000, 1, 1);
            SaltConcentrationTimeSeries.Arguments[0].InterpolationType = InterpolationType.Linear;
            SaltConcentrationTimeSeries.Arguments[0].ExtrapolationType = ExtrapolationType.Constant;
            SaltConcentrationTimeSeries.Components.Add(new Variable<double>("discharge concentration",
                                                                            new Unit("ppt", "ppt")));
            SaltConcentrationTimeSeries.Name = "Salt discharge concentration time series";
            SaltConcentrationTimeSeries.Components[0].Attributes[FunctionAttributes.StandardName] =
                FunctionAttributes.StandardNames.WaterSalinity;

            SaltMassTimeSeries = new TimeSeries();
            SaltMassTimeSeries.Arguments[0].DefaultValue = new DateTime(2000, 1, 1);
            SaltMassTimeSeries.Arguments[0].InterpolationType = InterpolationType.Linear;
            SaltMassTimeSeries.Arguments[0].ExtrapolationType = ExtrapolationType.Constant;
            SaltMassTimeSeries.Components.Add(new Variable<double>("discharge", new Unit("kg/s", "kg/s")));
            SaltMassTimeSeries.Name = "Salt discharge load time series";

            SaltMassTimeSeries.Components[0].Attributes[FunctionAttributes.StandardName] = "salt_discharge";

            SaltLateralDischargeType = SaltLateralDischargeType.Default;
        }

        private void EnableTemperature()
        {
            //add the dataitems and change the temperature condition type
            TemperatureConstant = 0;

            //add a timeseries 
            TemperatureTimeSeries = new TimeSeries();
            TemperatureTimeSeries.Arguments[0].DefaultValue = new DateTime(2000, 1, 1);
            TemperatureTimeSeries.Arguments[0].InterpolationType = InterpolationType.Linear;
            TemperatureTimeSeries.Arguments[0].ExtrapolationType = ExtrapolationType.Constant;
            TemperatureTimeSeries.Components.Add(new Variable<double>("Temperature", new Unit("degreesCelcius", "°C")));
            TemperatureTimeSeries.Name = "Temperature time series";
            
            TemperatureTimeSeries.Components[0].Attributes[FunctionAttributes.StandardName] = "water_temperature";

            TemperatureLateralDischargeType = TemperatureLateralDischargeType.Constant;
        }
        
        public string GetLinkedDataItemName()
        {
            switch (DataType)
            {
                case Model1DLateralDataType.FlowConstant:
                    return FlowConstantDataItem.LinkedTo.Name;
                case Model1DLateralDataType.FlowTimeSeries:
                case Model1DLateralDataType.FlowWaterLevelTable:
                    return SeriesDataItem.LinkedTo.Name;
            }
            return "";
        }

        public IEnumerable<object> GetDirectChildren()
        {
            yield return SeriesDataItem;
        }

        # region Implementation of IFeature

        public IGeometry Geometry
        {
            get
            {
                if (Feature == null) return null;

                var geometry = Feature.Geometry;

                if (geometry is ILineString)
                {
                    var location = LengthLocationMap.GetLocation(geometry, geometry.Length / 2);
                    geometry = new Point(location.GetCoordinate(geometry));
                }

                return geometry;
            }
            set 
            {
                // Geometry is readonly
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
        
        private void FeatureCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            UpdateDataType(sender, e);
        }

        private void UpdateDataType(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (DataType == Model1DLateralDataType.FlowRealTime ||
                !sender.Equals(Feature.Links) ||
                e.Action != NotifyCollectionChangedAction.Add)
            {
                return;
            }

            var addedLink = (HydroLink) e.GetRemovedOrAddedItem();
            if (addedLink.Source is Catchment)
            {
                DataType = Model1DLateralDataType.FlowRealTime;
            }
        }
    }
}