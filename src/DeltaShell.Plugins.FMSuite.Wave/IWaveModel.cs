using DelftTools.Hydro;
using DelftTools.Shell.Core.Workflow;
using DelftTools.Utils.Collections.Generic;
using DelftTools.Utils.Editing;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries;
using GeoAPI.Geometries;
using NetTopologySuite.Extensions.Features;

namespace DeltaShell.Plugins.FMSuite.Wave
{
    /// <summary>
    /// <see cref="IWaveModel"/> describes the content of a wave model.
    /// </summary>
    /// <seealso cref="ITimeDependentModel" />
    /// <seealso cref="IHasCoordinateSystem" />
    /// <seealso cref="IEditableObject" />
    public interface IWaveModel : ITimeDependentModel, IHasCoordinateSystem, IEditableObject
    {
        /// <summary>
        /// Gets the observation points.
        /// </summary>
        IEventedList<Feature2DPoint> ObservationPoints { get; }

        /// <summary>
        /// Gets the observation cross sections.
        /// </summary>
        IEventedList<Feature2D> ObservationCrossSections { get; }

        /// <summary>
        /// Gets the obstacles.
        /// </summary>
        IEventedList<WaveObstacle> Obstacles { get; }

        /// <summary>
        /// Gets the boundary container of this <see cref="IWaveModel"/>.
        /// </summary>
        IBoundaryContainer BoundaryContainer { get; }

        // TODO: This will most likely be removed.
        IEventedList<Feature2D> Boundaries { get; }
    }
}