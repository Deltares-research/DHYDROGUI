using DelftTools.Hydro;
using DelftTools.Shell.Core.Workflow;
using DelftTools.Utils.Collections.Generic;
using DelftTools.Utils.Editing;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries;
using DeltaShell.Plugins.FMSuite.Wave.OutputData;
using NetTopologySuite.Extensions.Features;

namespace DeltaShell.Plugins.FMSuite.Wave
{
    /// <summary>
    /// <see cref="IWaveModel"/> describes the content of a wave model.
    /// </summary>
    /// <seealso cref="ITimeDependentModel"/>
    /// <seealso cref="IHasCoordinateSystem"/>
    /// <seealso cref="IEditableObject"/>
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

        /// <summary>
        /// Gets the <see cref="IWaveOutputData"/> of this <see cref="WaveModel"/>
        /// </summary>
        IWaveOutputData WaveOutputData { get; }
    }
}