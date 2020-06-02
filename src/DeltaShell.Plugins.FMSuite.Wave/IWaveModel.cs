using DelftTools.Hydro;
using DelftTools.Shell.Core.Workflow;
using DelftTools.Utils.Collections.Generic;
using DelftTools.Utils.Editing;
using GeoAPI.Geometries;
using NetTopologySuite.Extensions.Features;

namespace DeltaShell.Plugins.FMSuite.Wave
{
    /// <summary>
    /// Interface for WaveModel classes
    /// </summary>
    /// <seealso cref="DelftTools.Shell.Core.Workflow.ITimeDependentModel"/>
    /// <seealso cref="DelftTools.Hydro.IHasCoordinateSystem"/>
    /// <seealso cref="DelftTools.Utils.Editing.IEditableObject"/>
    public interface IWaveModel : ITimeDependentModel, IHasCoordinateSystem, IEditableObject
    {
        IEventedList<Feature2DPoint> ObservationPoints { get; set; }

        IEventedList<Feature2D> ObservationCrossSections { get; set; }

        IEventedList<WaveObstacle> Obstacles { get; set; }

        IEventedList<Feature2D> Boundaries { get; }

        IEventedList<Feature2D> Sp2Boundaries { get; }

        bool BoundaryIsDefinedBySpecFile { get; set; }

        IGeometry GetGridSnappedBoundary(IGeometry geometry);
    }
}