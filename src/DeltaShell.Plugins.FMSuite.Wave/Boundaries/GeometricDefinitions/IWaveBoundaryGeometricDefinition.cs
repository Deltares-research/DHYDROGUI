namespace DeltaShell.Plugins.FMSuite.Wave.Boundaries.GeometricDefinitions
{
    /// <summary>
    /// <see cref="IWaveBoundaryGeometricDefinition"/> defines the geometric
    /// attributes of a <see cref="IWaveBoundary"/>.
    /// </summary>
    public interface IWaveBoundaryGeometricDefinition
    {
        /// <summary>
        /// Get or set the index of the first <see cref="GridCoordinate"/>
        /// of this <see cref="IWaveBoundary"/>.
        /// </summary>
        /// <value>
        /// The index of the first <see cref="GridCoordinate"/> of this
        /// <see cref="IWaveBoundary"/>.
        /// </value>
        int StartingIndex { get; set; }

        /// <summary>
        /// Get or set the index of the last <see cref="GridCoordinate"/>
        /// of this <see cref="IWaveBoundary"/>.
        /// </summary>
        /// <value>
        /// The index of the last <see cref="GridCoordinate"/> of this
        /// <see cref="IWaveBoundary"/>.
        /// </value>
        int EndingIndex { get; set; }
    }
}