using System;
using DelftTools.Utils;
using DelftTools.Utils.Guards;

namespace DeltaShell.Plugins.FMSuite.Wave.IO.Helpers.Boundaries
{
    /// <summary>
    /// Represents a boundary category block with data from the .mdw file
    /// </summary>
    public class BoundaryMdwBlock : INameable
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BoundaryMdwBlock" /> class.
        /// </summary>
        /// <param name="name"> The name of the wave boundary. </param>
        public BoundaryMdwBlock(string name)
        {
            Ensure.NotNull(name, nameof(name));
            if (name == string.Empty)
            {
                throw new ArgumentException("Argument cannot be empty.", nameof(name));
            }

            Name = name;
        }

        /// <summary>
        /// Gets or sets the name of the wave boundary.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the definition.
        /// </summary>
        public string Definition { get; set; }

        /// <summary>
        /// Gets or sets the x of the start coordinate.
        /// </summary>
        public double XStartCoordinate { get; set; }

        /// <summary>
        /// Gets or sets the y of the start coordinate.
        /// </summary>
        public double YStartCoordinate { get; set; }

        /// <summary>
        /// Gets or sets the x of the end coordinate.
        /// </summary>
        public double XEndCoordinate { get; set; }

        /// <summary>
        /// Gets or sets the y of the end coordinate.
        /// </summary>
        public double YEndCoordinate { get; set; }

        /// <summary>
        /// Gets or sets the spectrum type.
        /// </summary>
        public SpectrumType SpectrumType { get; set; }

        /// <summary>
        /// Gets or sets the shape type
        /// </summary>
        public ShapeType ShapeType { get; set; }

        /// <summary>
        /// Gets or sets the period type.
        /// </summary>
        public PeriodType PeriodType { get; set; }

        /// <summary>
        /// Gets or sets the spreading type.
        /// </summary>
        public SpreadingType SpreadingType { get; set; }

        /// <summary>
        /// Gets or sets the peak enhancement factor.
        /// </summary>
        public double PeakEnhancementFactor { get; set; }

        /// <summary>
        /// Gets or sets the spreading.
        /// </summary>
        public double Spreading { get; set; }

        /// <summary>
        /// Gets or sets the support point distances.
        /// </summary>
        public double[] Distances { get; set; }

        /// <summary>
        /// Gets or sets the wave heights at the support points.
        /// </summary>
        public double[] WaveHeights { get; set; }

        /// <summary>
        /// Gets or sets the periods at the support points.
        /// </summary>
        public double[] Periods { get; set; }

        /// <summary>
        /// Gets or sets the directions at the support points.
        /// </summary>
        public double[] Directions { get; set; }

        /// <summary>
        /// Gets or sets the directional spreadings at the support points.
        /// </summary>
        public double[] DirectionalSpreadings { get; set; }
    }
}