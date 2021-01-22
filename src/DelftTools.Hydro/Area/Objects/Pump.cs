using DelftTools.Functions;
using DelftTools.Hydro.Structures;
using DelftTools.Utils.Aop;
using DelftTools.Utils.Data;
using DelftTools.Utils.Guards;
using GeoAPI.Extensions.Feature;
using GeoAPI.Geometries;

namespace DelftTools.Hydro.Area.Objects
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

        public string GroupName
        {
            get => groupName;
            set => groupName = GroupableFeatureHelper.SetGroupableFeatureGroupName(value);
        }

        public bool IsDefaultGroup { get; set; } = false;


        public string Name { get; set; } = "Pump";

        public bool UseCapacityTimeSeries { get; set; } = false;

        public double Capacity { get; set; }

        public TimeSeries CapacityTimeSeries { get; }

        [NoNotifyPropertyChange]
        public IFeatureAttributeCollection Attributes { get; set; }

        public object Clone()
        {
            return new Pump((TimeSeries) CapacityTimeSeries.Clone())
            {
                GroupName = GroupName,
                Geometry = (IGeometry) Geometry.Clone(),
                Attributes = (IFeatureAttributeCollection) Attributes.Clone(),
                Name = Name,
                IsDefaultGroup = IsDefaultGroup,
                UseCapacityTimeSeries = UseCapacityTimeSeries,
                Capacity = Capacity,
            };
        }
    }
}