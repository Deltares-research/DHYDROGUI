using DelftTools.Utils.Collections.Generic;

namespace DeltaShell.Plugins.FMSuite.Wave.Boundaries.GeometricDefinitions
{
    /// <summary>
    /// <see cref="IWaveBoundaryGeometricDefinition"/> defines the geometric
    /// attributes of a <see cref="IWaveBoundary"/>.
    /// </summary>
    /// <remarks>
    /// The following invariants are be enforced:
    /// * <see cref="StartingIndex"/> >= 0;
    /// * <see cref="EndingIndex"/> > <see cref="StartingIndex"/>;
    /// * <see cref="GridSide"/> is a valid enum value;
    /// * <see cref="SupportPoints"/> is not <c>null</c>;
    /// </remarks>
    public interface IWaveBoundaryGeometricDefinition
    {
        /// <summary>
        /// Gets the index of the first <see cref="GridCoordinate"/>
        /// of this <see cref="IWaveBoundary"/>.
        /// </summary>
        /// <value>
        /// The index of the first <see cref="GridCoordinate"/> of this
        /// <see cref="IWaveBoundary"/>.
        /// </value>
        int StartingIndex { get; }

        /// <summary>
        /// Gets the index of the last <see cref="GridCoordinate"/>
        /// of this <see cref="IWaveBoundary"/>.
        /// </summary>
        /// <value>
        /// The index of the last <see cref="GridCoordinate"/> of this
        /// <see cref="IWaveBoundary"/>.
        /// </value>
        int EndingIndex { get; }

        /// <summary>
        /// The side of the grid this <see cref="IWaveBoundary"/> is located on.
        /// </summary>
        GridSide GridSide { get; }

        /// <summary>
        /// Gets the length of this <see cref="IWaveBoundary"/>.
        /// </summary>
        double Length { get; }

        /// <summary>
        /// Gets the support points defined on this <see cref="IWaveBoundaryGeometricDefinition"/>.
        /// </summary>
        /// <value>
        /// The support points defined on this <see cref="IWaveBoundaryGeometricDefinition"/>.
        /// </value>
        IEventedList<SupportPoint> SupportPoints { get; }
    }
}