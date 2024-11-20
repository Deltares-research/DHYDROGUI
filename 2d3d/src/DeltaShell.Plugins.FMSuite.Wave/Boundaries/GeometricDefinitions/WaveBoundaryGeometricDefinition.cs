using System;
using System.ComponentModel;
using DelftTools.Utils.Collections.Generic;

namespace DeltaShell.Plugins.FMSuite.Wave.Boundaries.GeometricDefinitions
{
    /// <summary>
    /// <see cref="WaveBoundaryGeometricDefinition"/> implements the geometric
    /// attributes of a <see cref="IWaveBoundary"/>.
    /// </summary>
    /// <seealso cref="IWaveBoundaryGeometricDefinition"/>
    public class WaveBoundaryGeometricDefinition : IWaveBoundaryGeometricDefinition
    {
        private int startingIndex;

        private int endingIndex;

        private GridSide gridSide = GridSide.North;

        /// <summary>
        /// Creates a new instance of the <see cref="WaveBoundaryGeometricDefinition"/>.
        /// </summary>
        /// <param name="startingIndex">Index of the starting.</param>
        /// <param name="endingIndex">Index of the ending.</param>
        /// <param name="gridSide">The grid side.</param>
        /// <param name="length">The length of the wave boundary.</param>
        /// <exception cref="InvalidEnumArgumentException">
        /// Thrown when <see cref="GridSide"/> is an invalid enum value.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when <paramref name="startingIndex"/> is smaller than zero
        /// or larger than or equal to <paramref name="endingIndex"/>.
        /// </exception>
        public WaveBoundaryGeometricDefinition(int startingIndex,
                                               int endingIndex,
                                               GridSide gridSide,
                                               double length)
        {
            if (startingIndex < 0)
            {
                throw new ArgumentException($"StartingIndex: '{startingIndex}' should be larger or equal to zero.");
            }

            if (startingIndex >= endingIndex)
            {
                throw new ArgumentException($"StartingIndex: '{startingIndex}' should be smaller than EndingIndex: {endingIndex}.");
            }

            if (length <= 0)
            {
                throw new ArgumentException($"Length: '{length}' should be larger than zero.");
            }

            GridSide = gridSide;
            this.startingIndex = startingIndex;
            this.endingIndex = endingIndex;
            Length = length;

            SupportPoints = new EventedList<SupportPoint>
            {
                new SupportPoint(0, this),
                new SupportPoint(Length, this)
            };
        }

        public int StartingIndex
        {
            get => startingIndex;
            internal set
            {
                ValidateStartingIndex(value);
                startingIndex = value;
            }
        }

        public int EndingIndex
        {
            get => endingIndex;
            internal set
            {
                ValidateEndingIndex(value);
                endingIndex = value;
            }
        }

        public GridSide GridSide
        {
            get => gridSide;
            internal set
            {
                if (!Enum.IsDefined(typeof(GridSide), value))
                {
                    throw new InvalidEnumArgumentException($"Value '{value}' is not a defined GridSide enum.");
                }

                gridSide = value;
            }
        }

        public double Length { get; }

        public IEventedList<SupportPoint> SupportPoints { get; }

        private void ValidateStartingIndex(int value)
        {
            if (value < 0)
            {
                throw new ArgumentException($"{nameof(StartingIndex)} should be greater or equal to zero.");
            }

            if (value >= EndingIndex)
            {
                throw new ArgumentException($"{nameof(StartingIndex)} should be smaller than {nameof(EndingIndex)}.");
            }
        }

        private void ValidateEndingIndex(int value)
        {
            if (value <= StartingIndex)
            {
                throw new ArgumentException($"{nameof(EndingIndex)} should be greater than {nameof(StartingIndex)}.");
            }
        }
    }
}