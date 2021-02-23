using DelftTools.Functions;

namespace DelftTools.Hydro.Area.Objects.StructureObjects
{
    /// <summary>
    /// <see cref="IPump"/> defines a single pump located in an <see cref="HydroArea"/>.
    /// </summary>
    /// <remarks>
    /// Deriving classes of <see cref="IPump"/> are expected to have
    /// a <see cref="Utils.Aop.EntityAttribute"/>.
    /// </remarks>
    public interface IPump : IStructureObject
    {
        /// <summary>
        /// If true, <see cref="CapacityTimeSeries"/> is used. Otherwise <see cref="Capacity"/> is used.
        /// </summary>
        bool UseCapacityTimeSeries { get; set; }

        /// <summary>
        /// Gets or sets the capacity of this <see cref="IPump"/>.
        /// </summary>
        double Capacity { get; set; }

        /// <summary>
        /// Gets the capacity time series of this <see cref="IPump"/>.
        /// </summary>
        TimeSeries CapacityTimeSeries { get; }
    }
}