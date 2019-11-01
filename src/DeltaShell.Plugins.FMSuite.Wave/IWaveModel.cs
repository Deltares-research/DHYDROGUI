using DelftTools.Hydro;
using DelftTools.Shell.Core.Workflow;
using DelftTools.Utils.Collections.Generic;
using DelftTools.Utils.Editing;
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
        /// Get or set the observation points.
        /// </summary>
        IEventedList<Feature2DPoint> ObservationPoints { get; }

        /// <summary>
        /// Get or set the observation cross sections.
        /// </summary>
        IEventedList<Feature2D> ObservationCrossSections { get; }

        /// <summary>
        /// Get or set the obstacles.
        /// </summary>
        IEventedList<WaveObstacle> Obstacles { get; }

        // TODO: This will most likely be removed.
        IEventedList<Feature2D> Boundaries { get; }

        // TODO: This will most likely be removed.
        IEventedList<Feature2D> Sp2Boundaries { get; }

        // TODO: This will most likely be removed.
        bool BoundaryIsDefinedBySpecFile { get; set; }

        // TODO: This will most likely be removed.
        IGeometry GetGridSnappedBoundary(IGeometry geometry);
    }
}