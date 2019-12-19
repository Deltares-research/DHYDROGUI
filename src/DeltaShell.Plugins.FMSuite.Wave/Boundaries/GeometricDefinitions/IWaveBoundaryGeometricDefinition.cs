using System;
using System.ComponentModel;
using DelftTools.Utils.Collections.Generic;

namespace DeltaShell.Plugins.FMSuite.Wave.Boundaries.GeometricDefinitions
{
    /// <summary>
    /// <see cref="IWaveBoundaryGeometricDefinition"/> defines the geometric
    /// attributes of a <see cref="IWaveBoundary"/>.
    /// </summary>
    /// <remarks>
    /// The following invariants should be enforced:
    /// * <see cref="StartingIndex"/> >= 0;
    /// * <see cref="EndingIndex"/> > <see cref="StartingIndex"/>;
    /// * <see cref="GridSide"/> is a valid enum value;
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
        /// <exception cref="ArgumentException">
        /// Thrown when <paramref name="value"/> is smaller than zero.
        /// Thrown when <paramref name="value"/> is greater or equal to <see cref="EndingIndex"/>.
        /// </exception>
        int StartingIndex { get; }

        /// <summary>
        /// Gets the index of the last <see cref="GridCoordinate"/>
        /// of this <see cref="IWaveBoundary"/>.
        /// </summary>
        /// <value>
        /// The index of the last <see cref="GridCoordinate"/> of this
        /// <see cref="IWaveBoundary"/>.
        /// </value>
        /// <exception cref="ArgumentException">
        /// Thrown when <paramref name="value"/> is smaller or equal to <see cref="StartingIndex"/>.
        /// </exception>
        int EndingIndex { get; }

        /// <summary>
        /// The side of the grid this <see cref="IWaveBoundary"/> is located on.
        /// </summary>
        /// <exception cref="InvalidEnumArgumentException">
        /// Thrown when <see cref="GridSide"/> is set to an invalid enum value.
        /// </exception>
        GridSide GridSide { get; }

        /// <summary>
        /// Gets the support points defined on this <see cref="IWaveBoundaryGeometricDefinition"/>.
        /// </summary>
        /// <value>
        /// The support points defined on this <see cref="IWaveBoundaryGeometricDefinition"/>.
        /// </value>
        IEventedList<SupportPoint> SupportPoints { get; }
    }
}