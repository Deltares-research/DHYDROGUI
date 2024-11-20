using DelftTools.Functions;
using DelftTools.Hydro.GroupableFeatures;
using DelftTools.Utils.Aop;
using DelftTools.Utils.Data;
using Deltares.Infrastructure.API.Guards;
using GeoAPI.Extensions.Feature;
using GeoAPI.Geometries;

namespace DelftTools.Hydro.Area.Objects.StructureObjects
{
    /// <summary>
    /// <see cref="Pump"/> implements a 2D pump which is
    /// contained in the <see cref="HydroArea"/>.
    /// </summary>
    /// <seealso cref="Unique{T}" />
    /// <seealso cref="IPump" />
    [Entity]
    public sealed class Pump : Unique<long>, IPump
    {
        private string groupName;

        /// <summary>
        /// Creates a new <see cref="Pump"/>.
        /// </summary>
        public Pump() : this(HydroTimeSeriesFactory.CreateTimeSeries("Capacity", 
                                                                     "Capacity", 
                                                                     "m3/s")) { }

        private Pump(TimeSeries capacityTimeSeries)
        {
            Ensure.NotNull(capacityTimeSeries, nameof(capacityTimeSeries));
            CapacityTimeSeries = capacityTimeSeries;
        }

        public IGeometry Geometry { get; set; }

        [FeatureAttribute]
        public string GroupName
        {
            get => groupName;
            set => groupName = GroupableFeatureHelper.SetGroupableFeatureGroupName(value);
        }

        public bool IsDefaultGroup { get; set; } = false;

        [FeatureAttribute]
        public string Name { get; set; } = "Pump";

        public bool UseCapacityTimeSeries { get; set; } = false;

        [FeatureAttribute]
        public double Capacity { get; set; } = 1.0;

        public TimeSeries CapacityTimeSeries { get; }

        [NoNotifyPropertyChange]
        public IFeatureAttributeCollection Attributes { get; set; }

        public object Clone()
        {
            return new Pump((TimeSeries) CapacityTimeSeries.Clone())
            {
                GroupName = GroupName,
                Geometry = (IGeometry) Geometry?.Clone(),
                Attributes = (IFeatureAttributeCollection) Attributes?.Clone(),
                Name = Name,
                IsDefaultGroup = IsDefaultGroup,
                UseCapacityTimeSeries = UseCapacityTimeSeries,
                Capacity = Capacity,
            };
        }

        // As part of WaterFlowFMModel.Eventing.GetDataItemListForFeature
        // uses the `feature.ToString()` method to generate a name. In order
        // to ensure the name does not get overwritten, we need to overwrite
        // the `ToString` method to return the Name. This is legacy behaviour 
        // from the previous Pump implementation unfortunately.
        public override string ToString() => 
            !string.IsNullOrEmpty(Name) ? Name : "Unnamed Pump";
    }
}