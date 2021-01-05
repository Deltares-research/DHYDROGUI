using DelftTools.Functions;

namespace DelftTools.Hydro.Structures
{
    public interface IPump : IStructure
    {
        string Name { get; set; }
        string LongName { get; set; }

        /// <summary>
        /// Indicates if <see cref="CapacityTimeSeries"/> can be used.
        /// </summary>
        bool CanBeTimedependent { get; set; }

        bool DirectionIsPositive { get; set; }

        /// <summary>
        /// If true, <see cref="CapacityTimeSeries"/> is used. Otherwise use <see cref="Capacity"/>.
        /// </summary>
        bool UseCapacityTimeSeries { get; set; }

        double Capacity { get; set; }
        TimeSeries CapacityTimeSeries { get; }
        double StartDelivery { get; set; }
        double StopDelivery { get; set; }
        double StartSuction { get; set; }
        double StopSuction { get; set; }
        PumpControlDirection ControlDirection { get; set; }

        /// <summary>
        /// reduction table
        /// </summary>
        IFunction ReductionTable { get; set; }
    }
}