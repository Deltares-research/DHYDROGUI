using DelftTools.Utils;

namespace DeltaShell.Plugins.FMSuite.Wave.DataAccess.Helpers.Boundaries
{
    /// <summary>
    /// Represents a boundary category block with data from the .mdw file
    /// </summary>
    public class BoundaryMdwBlock : INameable
    {
        /// <summary>
        /// Gets or sets the definition type.
        /// </summary>
        public DefinitionImportType DefinitionType { get; set; }

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
        public SpectrumImportExportType SpectrumType { get; set; }

        /// <summary>
        /// Gets or sets the shape type
        /// </summary>
        public ShapeImportType ShapeType { get; set; }

        /// <summary>
        /// Gets or sets the period type.
        /// </summary>
        public PeriodImportExportType PeriodType { get; set; }

        /// <summary>
        /// Gets or sets the spreading type.
        /// </summary>
        public SpreadingImportType SpreadingType { get; set; }

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

        /// <summary>
        /// Gets or sets the spectrum file paths at the support points.
        /// </summary>
        public string[] SpectrumFiles { get; set; }

        /// <summary>
        /// Gets or sets the the orientation.
        /// </summary>
        public BoundaryOrientationType? OrientationType { get; set; } = null;

        /// <summary>
        /// Gets or sets the type of the distance dir.
        /// </summary>
        public DistanceDirType? DistanceDirType { get; set; } = null;

        /// <summary>
        /// Gets or sets the name of the wave boundary.
        /// </summary>
        public string Name { get; set; }
    }
}