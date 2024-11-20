using System;
using System.ComponentModel;
using DelftTools.Functions;
using DelftTools.Hydro.SewerFeatures;
using DelftTools.Hydro.Structures.SteerableProperties;

namespace DelftTools.Hydro.Structures
{
    public enum CrestShape
    {
        Sharp,
        Round,
        Triangular,
        Broad,
        [Description("User defined")]
        UserDefined
    }

    /// <summary>
    /// <see cref="IWeir"/> defines a one-dimensional weir/structure
    /// which can be placed on a <see cref="GeoAPI.Extensions.Networks.IBranch"/>.
    /// </summary>
    public interface IWeir : IStructure1D, ISewerFeature, IHasSteerableProperties
    {
        /// <summary>
        /// Indicates if time dependent parameters can be used.
        /// </summary>
        bool CanBeTimedependent { get; }

        /// <summary>
        /// Formula for sobek
        /// </summary>
        IWeirFormula WeirFormula { get; set; }

        /// <summary>
        /// Rectangle or Free Form
        /// </summary>
        bool IsRectangle { get; }

        /// <summary>
        /// Gated or not. Maybe remove..is equal to WeirFormula is IGatedWeirFormula
        /// </summary>
        bool IsGated { get; }

        /// <summary>
        /// Crest width (-1 : look at profile)
        /// </summary>
        double CrestWidth { get; set; }

        /// <summary>
        /// When true, use <see cref="CrestLevelTimeSeries"/> otherwise use <see cref="CrestLevel"/>.
        /// </summary>
        /// <exception cref="InvalidOperationException">When setting to true while <see cref="CanBeTimedependent"/> is false.</exception>
        bool UseCrestLevelTimeSeries { get; set; }
        
        /// <summary>
        /// Crest level
        /// </summary>
        double CrestLevel { get; set; }
        
        /// <summary>
        /// Time varying crest level. Will be null when <see cref="CanBeTimedependent"/> is false.
        /// </summary>
        TimeSeries CrestLevelTimeSeries { get; }

        /// <summary>
        /// Is flow possible in the negative direction of the channel
        /// </summary>
        bool AllowNegativeFlow { get; set; }

        /// <summary>
        /// Is flow possible in the positive direction of the channel
        /// </summary>
        bool AllowPositiveFlow { get; set; }

        /// <summary>
        /// The shape of the crest of the weir. This is a helper for getting/setting the ContractionCoefficient
        /// </summary>
        CrestShape CrestShape { get; set; }

        FlowDirection FlowDirection { get; set; }

        /// <summary>
        /// Flag indicates whether the velocity height is to be calculated or not.
        /// </summary>
        bool UseVelocityHeight { get; set; }

        /// <summary>
        /// Determine whether or not a time series is being used for the crest level.
        /// </summary>
        /// <returns>Returns <c>true</c> if currently using a time series for the crest level. <c>False</c> otherwise.</returns>
        bool IsUsingTimeSeriesForCrestLevel();
    }
}