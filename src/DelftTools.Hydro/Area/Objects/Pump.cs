using System.ComponentModel;
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
        public IGeometry Geometry { get; set; }

        [DisplayName("Group name")]
        [FeatureAttribute(Order = 1)]
        public string GroupName
        {
            get => groupName;
            set => groupName = GroupableFeatureHelper.SetGroupableFeatureGroupName(value);
        }

        public bool IsDefaultGroup { get; set; } = false;

        [DisplayName("Name")]
        [FeatureAttribute(Order = 2)]

        public string Name { get; set; } = "Pump";

        public bool UseCapacityTimeSeries { get; set; } = false;

        [DisplayName("Capacity")]
        [FeatureAttribute(Order = 3)]
        public double Capacity { get; set; }

        public TimeSeries CapacityTimeSeries { get; }

        [NoNotifyPropertyChange]
        public IFeatureAttributeCollection Attributes { get; set; }
    }
}