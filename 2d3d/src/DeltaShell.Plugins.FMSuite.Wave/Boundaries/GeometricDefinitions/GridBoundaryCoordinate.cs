using System;
using System.ComponentModel;

namespace DeltaShell.Plugins.FMSuite.Wave.Boundaries.GeometricDefinitions
{
    /// <summary>
    /// <see cref="GridBoundaryCoordinate"/> describes a single boundary coordinate
    /// as an index and a <see cref="GridSide"/>
    /// </summary>
    public class GridBoundaryCoordinate
    {
        /// <summary>
        /// Creates a new see cref="GridBoundaryCoordinate"/>.
        /// </summary>
        /// <param name="gridSide">The grid side.</param>
        /// <param name="index">The index.</param>
        /// <exception cref="InvalidEnumArgumentException">
        /// Thrown when <paramref name="gridSide"/> is not defined.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when <paramref name="index"/> is not defined.
        /// </exception>
        public GridBoundaryCoordinate(GridSide gridSide,
                                      int index)
        {
            if (!Enum.IsDefined(typeof(GridSide), gridSide))
            {
                throw new InvalidEnumArgumentException();
            }

            if (index < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            GridSide = gridSide;
            Index = index;
        }

        /// <summary>
        /// Gets the grid side of this <see cref="GridBoundaryCoordinate"/>.
        /// </summary>
        /// <value>
        /// The grid side.
        /// </value>
        public GridSide GridSide { get; }

        /// <summary>
        /// Gets the index of this <see cref="GridBoundaryCoordinate"/> on the
        /// <see cref="GridSide"/> of the <see cref="IGridBoundary"/> it was
        /// defined on.
        /// </summary>
        /// <value>
        /// The index.
        /// </value>
        public int Index { get; }

        /// <summary>
        /// Deconstructs this <see cref="GridBoundaryCoordinate"/> to its
        /// <see cref="GridSide"/> and <see cref="Index"/>
        /// </summary>
        /// <param name="gridSide">The grid side.</param>
        /// <param name="index">The index.</param>
        public void Deconstruct(out GridSide gridSide, out int index)
        {
            gridSide = GridSide;
            index = Index;
        }

        public override bool Equals(object obj) =>
            obj is GridBoundaryCoordinate otherCoordinate &&
            otherCoordinate.Index == Index &&
            otherCoordinate.GridSide == GridSide;

        public override int GetHashCode() => (Index << 2) ^ (int) GridSide;
    }
}