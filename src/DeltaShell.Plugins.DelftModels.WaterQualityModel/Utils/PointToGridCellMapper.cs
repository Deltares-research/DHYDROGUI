using System;
using System.Globalization;
using System.Linq;
using DelftTools.Utils;
using GeoAPI.Geometries;
using NetTopologySuite.Extensions.Grids;

namespace DeltaShell.Plugins.DelftModels.WaterQualityModel.Utils
{
    /// <summary>
    /// Class able to map a 3D location to a <see cref="UnstructuredGrid"/> cell index,
    /// taking into account sigma or Z-layer discretization.
    /// </summary>
    public class PointToGridCellMapper
    {
        private double[] sigmaLayerThicknesses;
        private double topLevel;
        private double bottomLevel;

        /// <summary>
        /// The schematization grid.
        /// </summary>
        public UnstructuredGrid Grid { get; set; }

        /// <summary>
        /// Sets the mapper up for mapping to sigma layers.
        /// </summary>
        /// <param name="relativeThicknesses"> The relative thicknesses of the layer. </param>
        /// <remarks>
        /// A height of 0 corresponds to the surface, where a height of 1 corresponds
        /// to the bottom.
        /// </remarks>
        /// <exception cref="ArgumentException">
        /// When the sum of all thicknesses in
        /// <paramref name="relativeThicknesses"/> to not add up to ~1.0.
        /// </exception>
        public void SetSigmaLayers(double[] relativeThicknesses)
        {
            SetLayersCore(relativeThicknesses, 0.0, 1.0, "Sigma");
        }

        /// <summary>
        /// Sets the mapper up for mapping to z-layers.
        /// </summary>
        /// <param name="relativeThicknesses"> The relative thicknesses of the layers. </param>
        /// <param name="top"> The top level (surface). </param>
        /// <param name="bottom"> The bottom level (bottom). </param>
        /// <exception cref="ArgumentException">
        /// When the sum of all thicknesses in
        /// <paramref name="relativeThicknesses"/> to not add up to ~1.0.
        /// </exception>
        public void SetZLayers(double[] relativeThicknesses, double top, double bottom)
        {
            SetLayersCore(relativeThicknesses, top, bottom, "Z");
        }

        /// <summary>
        /// Gets the cell index for <see cref="Grid"/> and the initialized layers, for a
        /// 3D location.
        /// </summary>
        /// <param name="x"> The x coordinate. </param>
        /// <param name="y"> The y coordinate. </param>
        /// <param name="z"> The z coordinate. </param>
        /// <returns> </returns>
        /// <exception cref="System.InvalidOperationException">
        /// When <see cref="Grid"/> is null or when either <see cref="SetSigmaLayers"/> or
        /// <see cref="SetZLayers"/> hasn't been called before to initialize the model layers.
        /// </exception>
        /// <exception cref="System.ArgumentOutOfRangeException">
        /// When <paramref name="z"/>
        /// falls outside the valid range as defined when calling either <see cref="SetSigmaLayers"/>
        /// or <see cref="SetZLayers"/>.
        /// </exception>
        /// <exception cref="System.ArgumentException">
        /// When evaluating at a location outside
        /// the grid or when it is ambiguous in which cell it falls.
        /// </exception>
        public int GetWaqSegmentIndex(double x, double y, double z)
        {
            if (sigmaLayerThicknesses == null)
            {
                throw new InvalidOperationException("Cannot determine cell index as no layer data was provided.");
            }

            double relativeZ = MapToRelativeValue(z);
            if (!relativeZ.IsInRange(0.0, 1.0))
            {
                string message = string.Format(
                    "Height of point must be in range [{0}, {1}] for {2} models, but was {3}.",
                    topLevel.ToString(CultureInfo.InvariantCulture), bottomLevel.ToString(CultureInfo.InvariantCulture),
                    GetModelType(), z);
                throw new ArgumentOutOfRangeException(nameof(z), message);
            }

            int indexIn2DSpace = GetWaqSegmentIndex2D(x, y);
            if (indexIn2DSpace == 0)
            {
                throw new ArgumentException(string.Format(
                                                "Point ({0}, {1}, {2}) is not within grid or has ambiguous location (on a grid edge or grid vertex).",
                                                x, y, z));
            }

            int index = GetLayerIndex(relativeZ);

            return indexIn2DSpace + (index * Grid.Cells.Count);
        }

        /// <summary>
        /// Gets the cell index of a coordinate, neglecting the z coordinate.
        /// </summary>
        public int GetWaqSegmentIndex2D(double x, double y)
        {
            if (Grid == null)
            {
                throw new InvalidOperationException("Cannot determine cell index as no grid was set.");
            }

            // do not include edges when checking if a coordinate is within a segment (cell)
            // if the coordinate is exactly on an edge it is unclear to which segment the point belongs
            return Grid.GetCellIndexForCoordinate(new Coordinate(x, y), includeEdges: false) + 1 ?? 0; // + 1, waq is one based.

        }

        public Cell GetCellFromWaqSegmentId(int segmentIndex)
        {
            int singleLayerGridCellCount = Grid.Cells.Count;
            return Grid.Cells[(segmentIndex - 1) % singleLayerGridCellCount];
        }

        private string GetModelType()
        {
            return topLevel == 0.0 && bottomLevel == 1.0 ? "sigma" : "Z-layer";
        }

        private int GetLayerIndex(double z)
        {
            // Method Preconditions:
            // * z in range [0,1]
            // * sigmaLayerThicknesses sums up to ~1.0
            var distance = 0.0;
            for (var i = 0; i < sigmaLayerThicknesses.Length; i++)
            {
                distance += sigmaLayerThicknesses[i];
                if (z <= distance)
                {
                    return i;
                }
            }

            // Cover case where sigmaLayerThicknesses.Sum() nears 1 but is not exactly that:
            if (z <= 1.0)
            {
                return sigmaLayerThicknesses.Length - 1;
            }

            // This code is unreachable code under the given preconditions:
            throw new InvalidOperationException(
                "For z in range [0,1] and valid sigmaLayerThicknesses spanning to 1.0, a layer match should have been found.");
        }

        private void SetLayersCore(double[] relativeThicknesses, double topLevelValue, double bottomLevelValue,
                                   string layerType)
        {
            double sum = relativeThicknesses.Sum();
            if (Math.Abs(1.0 - sum) > 1.01e-3)
            {
                string message = string.Format("{0} layers should add up to ~1.0, but was adding up to {1}.", layerType,
                                               sum);
                throw new ArgumentException(message, nameof(relativeThicknesses));
            }

            sigmaLayerThicknesses = relativeThicknesses;

            topLevel = topLevelValue;
            bottomLevel = bottomLevelValue;
        }

        private double MapToRelativeValue(double z)
        {
            return (z - topLevel) / (bottomLevel - topLevel);
        }
    }
}