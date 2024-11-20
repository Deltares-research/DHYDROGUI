using DelftTools.Functions;
using DelftTools.Hydro.SewerFeatures;

namespace DelftTools.Hydro.Structures
{
    /// <summary>
    /// <see cref="IPump"/> defines a one-dimensional pump which can be placed on a
    /// <see cref="GeoAPI.Extensions.Networks.IBranch"/>.
    /// </summary>
    public interface IPump : IStructure1D, ISewerFeature
    {
        /// <summary>
        /// Gets whether time-varying data can be used to drive the <see cref="IPump.Capacity"/>.
        /// </summary>
        bool CanBeTimedependent { get; }

        /// <summary>
        /// Gets or sets whether the direction is positive, <c>true</c>, or negative <c>false</c>.
        /// </summary>
        bool DirectionIsPositive { get; set; }

        /// <summary>
        /// Gets or sets if the <see cref="IPump.CapacityTimeSeries"/> is used, <c>true</c>, or
        /// the <see cref="Capacity"/>, <c>false</c>.
        /// </summary>
        /// 
        bool UseCapacityTimeSeries { get; set; }

        /// <summary>
        /// Gets or sets the constant capacity in [m3/s] of this <see cref="IPump"/>
        /// </summary>
        /// <remarks>
        /// This should be used if <see cref="UseCapacityTimeSeries"/> is <c>false</c>
        /// </remarks>
        double Capacity { get; set; }

        /// <summary>
        /// Gets the capacity as a function of the time.
        /// </summary>
        /// <remarks>
        /// This should be used if <see cref="UseCapacityTimeSeries"/> is <c>true</c>
        /// </remarks>
        TimeSeries CapacityTimeSeries { get; }

        /// <summary>
        /// Gets or sets the start delivery value of this <see cref="IPump"/>
        /// </summary>
        double StartDelivery { get; set; }

        /// <summary>
        /// Gets or sets the stop delivery value of this <see cref="IPump"/>
        /// </summary>
        double StopDelivery { get; set; }

        /// <summary>
        /// Gets or sets the start suction value of this <see cref="IPump"/>
        /// </summary>
        double StartSuction { get; set; }

        /// <summary>
        /// Gets or sets the stop suction value of this <see cref="IPump"/>
        /// </summary>
        double StopSuction { get; set; }

        /// <summary>
        /// Gets or sets the control direction of this <see cref="IPump"/>
        /// </summary>
        PumpControlDirection ControlDirection { get; set; }

        // Designer properties
        /// <summary>
        /// Y offset relative in the profile. This value is used by the structure view to display
        /// the pump in the structure designer. It is not used by the 1d model engine.
        /// </summary>
        double OffsetY { get; set; }
        /// <summary>
        /// Y offset relative in the profile. Calculated based on levels. For visualization only
        /// </summary>
        double OffsetZ { get; }

        /// <summary>
        /// Gets or sets the reduction table of this <see cref="IPump"/>
        /// </summary>
        IFunction ReductionTable { get; set; }
    }
}