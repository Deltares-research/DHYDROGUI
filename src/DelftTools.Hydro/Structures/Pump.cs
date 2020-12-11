using System;
using System.ComponentModel;
using DelftTools.Functions;
using DelftTools.Utils.Aop;
using GeoAPI.Extensions.Feature;
using GeoAPI.Extensions.Networks;
using NetTopologySuite.Extensions.Networks;

namespace DelftTools.Hydro.Structures
{
    /// <summary>
    /// Implements a 1d pump.
    /// Both the Sobek Pump (type 9) and River Pump (type 3) are implemented by this class.
    /// todo: add support for reduction table = combine with implementation triggers
    /// </summary>
    [Entity(FireOnCollectionChange = false)]
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class Pump : BranchFeature, IPump
    {
        private bool canBeTimedependent;
        private bool useCapacityTimeSeries;

        public Pump() : this("Pump") {}

        public Pump(bool canBeTimeDependent) : this("Pump", canBeTimeDependent) {}

        public Pump(string name, bool canBeTimeDependent = false)
        {
            Name = name;
            Capacity = 1.0;
            StartDelivery = 0;
            StopDelivery = 0;
            StartSuction = 3.0;
            StopSuction = 2.0;
            DirectionIsPositive = true;
            ControlDirection = PumpControlDirection.SuctionAndDeliverySideControl;
            ReductionTable = FunctionHelper.Get1DFunction<double, double>("reduction", "difference", "factor");

            CanBeTimedependent = canBeTimeDependent;
        }

        public virtual bool CanBeTimedependent
        {
            get => canBeTimedependent;
            set
            {
                canBeTimedependent = value;
                OnCanBeTimeDependentSet();
            }
        }

        [DisplayName("Positive direction")]
        [FeatureAttribute(Order = 5, ExportName = "PosDir")]
        public virtual bool DirectionIsPositive { get; set; }

        [FeatureAttribute(Order = 6)]
        public virtual double Capacity { get; set; }

        public virtual bool UseCapacityTimeSeries
        {
            get => useCapacityTimeSeries;
            set
            {
                if (!canBeTimedependent && value)
                {
                    throw new InvalidOperationException(
                        "Cannot use time series for capacity when time varying data is not allowed.");
                }

                useCapacityTimeSeries = value;
            }
        }

        public virtual TimeSeries CapacityTimeSeries { get; protected set; }

        [DisplayName("Switch-on delivery")]
        [FeatureAttribute(Order = 7, ExportName = "OnDelivery")]
        public virtual double StartDelivery { get; set; }

        [DisplayName("Switch-off delivery")]
        [FeatureAttribute(Order = 8, ExportName = "OffDelivery")]
        public virtual double StopDelivery { get; set; }

        [DisplayName("Switch-on suction")]
        [FeatureAttribute(Order = 9, ExportName = "OnSuction")]
        public virtual double StartSuction { get; set; }

        [DisplayName("Switch-off suction")]
        [FeatureAttribute(Order = 10, ExportName = "OffSuction")]
        public virtual double StopSuction { get; set; }

        [DisplayName("Control on")]
        [FeatureAttribute(Order = 11, ExportName = "ControlDir")]
        public virtual PumpControlDirection ControlDirection { get; set; }

        /// <summary>
        /// reduction table
        /// </summary>
        public virtual IFunction ReductionTable { get; set; }

        public override void CopyFrom(object source)
        {
            base.CopyFrom(source);

            var pump = (Pump) source;

            LongName = pump.LongName;
            Attributes = (IFeatureAttributeCollection) pump.Attributes.Clone();
            Capacity = pump.Capacity;
            StopDelivery = pump.StopDelivery;
            StartDelivery = pump.StartDelivery;
            StartSuction = pump.StartSuction;
            StopSuction = pump.StopSuction;
            ControlDirection = pump.ControlDirection;
            ReductionTable = (IFunction) pump.ReductionTable.Clone(true);
            DirectionIsPositive = pump.DirectionIsPositive;

            CanBeTimedependent = pump.CanBeTimedependent;
            if (!pump.CanBeTimedependent)
            {
                return;
            }

            UseCapacityTimeSeries = pump.UseCapacityTimeSeries;
            CapacityTimeSeries = (TimeSeries) pump.CapacityTimeSeries.Clone(true);
        }

        [EditAction]
        private void OnCanBeTimeDependentSet()
        {
            if (canBeTimedependent)
            {
                if (CapacityTimeSeries == null)
                {
                    CapacityTimeSeries = HydroTimeSeriesFactory.CreateTimeSeries("Capacity", "Capacity", "TODO");
                }
            }
            else
            {
                UseCapacityTimeSeries = false;
                CapacityTimeSeries = null;
            }
        }

        public virtual string Description { get; set; }

        [DisplayName("Long name")]
        [FeatureAttribute(Order = 1)]
        public virtual string LongName { get; set; }

        public virtual int CompareTo(Pump other)
        {
            return CompareTo((IBranchFeature) other);
        }

        public override int CompareTo(object obj)
        {
            var other = (Pump) obj;
            return CompareTo(other);
        }

        public virtual IHydroRegion Region { get; }
    }
}