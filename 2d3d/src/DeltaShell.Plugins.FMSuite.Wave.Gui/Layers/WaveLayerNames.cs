using System;

namespace DeltaShell.Plugins.FMSuite.Wave.Gui.Layers
{
    /// <summary>
    /// <see cref="WaveLayerNames"/> defines the names of the different wave layers.
    /// </summary>
    public static class WaveLayerNames
    {
        /// <summary>
        /// The obstacle layer name
        /// </summary>
        public const string ObstacleLayerName = "Obstacles";

        /// <summary>
        /// The boundary layer name
        /// </summary>
        public const string BoundaryLayerName = "Boundaries";

        /// <summary>
        /// The boundary line layer name
        /// </summary>
        public const string BoundaryLineLayerName = "Boundary Lines";

        /// <summary>
        /// The boundary end points layer name
        /// </summary>
        public const string BoundaryEndPointsLayerName = "End Points";

        /// <summary>
        /// The boundary end points layer name
        /// </summary>
        public const string BoundaryStartPointsLayerName = "Start Points";

        /// <summary>
        /// The boundary support points layer name
        /// </summary>
        public const string BoundarySupportPointsLayerName = "Support points";

        /// <summary>
        /// The active support points layer name
        /// </summary>
        public const string ActiveSupportPointsLayerName = "Active support points";

        /// <summary>
        /// The inactive support points layer name
        /// </summary>
        public const string InactiveSupportPointsLayerName = "Inactive support points";

        /// <summary>
        /// The selected support point layer name
        /// </summary>
        public const string SelectedSupportPointLayerName = "Selected support point";

        /// <summary>
        /// The observation point layer name
        /// </summary>
        public const string ObservationPointLayerName = "Observation Points";

        /// <summary>
        /// The observation cross section layer name
        /// </summary>
        public const string ObservationCrossSectionLayerName = "Observation Cross-Sections";

        /// <summary>
        /// The wave output data layer name
        /// </summary>
        public const string WaveOutputDataLayerName = "Output";

        /// <summary>
        /// The wavm function group layer name
        /// </summary>
        public const string WavmFunctionGroupLayerName = "Map Files";

        /// <summary>
        /// The wavh function group layer name
        /// </summary>
        public const string WavhFunctionGroupLayerName = "His Files";

        /// <summary>
        /// Gets the name of the domain layer given a <paramref name="domainName"/>.
        /// </summary>
        /// <param name="domainName">Name of the domain.</param>
        /// <returns>
        /// The name of the domain layer given a <paramref name="domainName"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="domainName"/> is <c>null</c>.
        /// </exception>
        public static string GetDomainLayerName(string domainName)
        {
            if (domainName == null)
            {
                throw new ArgumentNullException(nameof(domainName));
            }

            return $"Domain ({domainName})";
        }

        /// <summary>
        /// Gets the name of the output layer given a <paramref name="domainName"/>.
        /// </summary>
        /// <param name="domainName">Name of the domain.</param>
        /// <returns>
        /// The name of the output layer given a <paramref name="domainName"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="domainName"/> is <c>null</c>.
        /// </exception>
        public static string GetOutputLayerName(string domainName)
        {
            if (domainName == null)
            {
                throw new ArgumentNullException(nameof(domainName));
            }

            return $"Output ({domainName})";
        }
    }
}