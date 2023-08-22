using System.Collections.Generic;
using System.Linq;
using DelftTools.Utils.Aop;
using DelftTools.Utils.Guards;
using GeoAPI.Extensions.Coverages;
using NetTopologySuite.Extensions.Coverages;
using SharpMap.SpatialOperations;

namespace DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition
{
    /// <summary>
    /// <see cref="Samples"/> is a class that holds point cloud data and properties controlling how the point values should be
    /// interpolated onto the grid.
    /// </summary>
    [Entity]
    public sealed class Samples
    {
        private readonly IPointCloud pointCloud;

        /// <summary>
        /// Initialize a new instance of the <see cref="Samples"/> class.
        /// </summary>
        /// <param name="name"> The samples name. </param>
        /// <exception cref="System.ArgumentException">
        /// Thrown when <paramref name="name"/> is <c>null</c> or white space.
        /// </exception>
        public Samples(string name)
        {
            Ensure.NotNullOrWhiteSpace(name, nameof(name));

            pointCloud = new PointCloud { Name = name };
        }

        /// <summary>
        /// The name for the data.
        /// </summary>
        public string Name => pointCloud.Name;

        /// <summary>
        /// The operand for how the data is set onto the grid.
        /// </summary>
        public PointwiseOperationType Operand { get; set; }

        /// <summary>
        /// The method for how the data is interpolated onto the grid.
        /// </summary>
        public SpatialInterpolationMethod InterpolationMethod { get; set; }

        /// <summary>
        /// The averaging method when <see cref="SpatialInterpolationMethod.Averaging"/> is used.
        /// </summary>
        public GridCellAveragingMethod AveragingMethod { get; set; }

        /// <summary>
        /// The relative search cell size when <see cref="SpatialInterpolationMethod.Averaging"/> is used.
        /// </summary>
        public double RelativeSearchCellSize { get; set; }

        /// <summary>
        /// The extrapolation tolerance when <see cref="SpatialInterpolationMethod.Triangulation"/> is used.
        /// </summary>
        public double ExtrapolationTolerance { get; set; }

        /// <summary>
        /// The point values of the samples.
        /// </summary>
        public IEnumerable<IPointValue> PointValues => pointCloud.PointValues;

        /// <summary>
        /// The name of the source file with the samples.
        /// </summary>
        public string SourceFileName { get; set; }

        /// <summary>
        /// Whether or not this instance contains data.
        /// </summary>
        public bool HasData => PointValues.Any();

        /// <summary>
        /// Set the point values.
        /// </summary>
        /// <param name="pointValues"> The new point values. </param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when <paramref name="pointValues"/> is <c>null</c>.
        /// </exception>
        public void SetPointValues(IEnumerable<IPointValue> pointValues)
        {
            pointCloud.PointValues = pointValues.ToList();
        }

        /// <summary>
        /// Get the point values as a <see cref="IPointCloud"/>.
        /// </summary>
        /// <returns>
        /// The underlying <see cref="PointCloud"/>.
        /// </returns>
        public IPointCloud AsPointCloud()
        {
            return pointCloud;
        }
    }
}