using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using DelftTools.Utils.Guards;
using Deltares.Infrastructure.IO;
using DeltaShell.Plugins.FMSuite.Common.IO.Files;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Files.Helpers;
using DHYDRO.Common.Extensions;
using DHYDRO.Common.IO.InitialField;
using NetTopologySuite.Extensions.Features;
using SharpMap.Api.SpatialOperations;
using SharpMap.Data.Providers;
using SharpMap.SpatialOperations;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO.Files.InitialField
{
    /// <summary>
    /// Factory class for <see cref="ISpatialOperation"/>.
    /// </summary>
    public sealed class SpatialOperationFactory
    {
        private readonly IFileSystem fileSystem;

        /// <summary>
        /// Initializes a new instance of the <see cref="SpatialOperationFactory"/> class.
        /// </summary>
        /// <param name="fileSystem">Provides access to the file system.</param>
        /// <exception cref="System.ArgumentNullException">Thrown when <paramref name="fileSystem"/> is <c>null</c>.</exception>
        public SpatialOperationFactory(IFileSystem fileSystem)
        {
            Ensure.NotNull(fileSystem, nameof(fileSystem));
            
            this.fileSystem = fileSystem;
        }

        /// <summary>
        /// Create a spatial operation from the given initial field data.
        /// </summary>
        /// <param name="initialFieldData"> The initial field. </param>
        /// <returns>A newly constructed spatial operation. </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="initialFieldData"/> is <c>null</c>.
        /// </exception>
        public ISpatialOperation CreateFromInitialFieldData(InitialFieldData initialFieldData)
        {
            Ensure.NotNull(initialFieldData, nameof(initialFieldData));

            switch (initialFieldData.DataFileType)
            {
                case InitialFieldDataFileType.GeoTIFF:
                case InitialFieldDataFileType.ArcInfo:
                    return CreateSamplesOperation<ImportRasterSamplesOperationImportData>(initialFieldData);
                case InitialFieldDataFileType.Sample:
                    return CreateSamplesOperation<ImportSamplesSpatialOperation>(initialFieldData);
                case InitialFieldDataFileType.Polygon:
                    return CreatePolygonOperation(initialFieldData);
                default:
                    throw new ArgumentException(
                        $"Cannot construct spatial operation for file {initialFieldData.DataFile} with file type {initialFieldData.DataFileType.GetDescription()}");
            }
        }

        private ISpatialOperation CreatePolygonOperation(InitialFieldData initialFieldData)
        {
            var polFile = new PolFile<Feature2DPolygon>();
            string dataFilePath = GetDataFilePath(initialFieldData);

            IEnumerable<Feature2DPolygon> polygons = polFile.Read(dataFilePath);
            IEnumerable<Feature> features = polygons.Select(f => new Feature
            {
                Geometry = f.Geometry,
                Attributes = f.Attributes
            });

            string operationName = Path.GetFileNameWithoutExtension(initialFieldData.DataFile)
                                       .ReplaceCaseInsensitive(initialFieldData.Quantity.GetDescription() + "_", string.Empty);
            
            var operation = new SetValueOperation
            {
                Name = operationName,
                Value = initialFieldData.Value,
                OperationType = GetOperand(initialFieldData.Operand),
                Mask = { Provider = new FeatureCollection(features.ToList(), typeof(Feature)) }
            };

            return operation;
        }

        private T CreateSamplesOperation<T>(InitialFieldData initialFieldData) where T : ImportSamplesSpatialOperation, new()
        {
            string operationName = Path.GetFileNameWithoutExtension(initialFieldData.DataFile);

            var operation = new T
            {
                Name = operationName,
                FilePath = GetDataFilePath(initialFieldData),
                Operand = GetOperand(initialFieldData.Operand),
                InterpolationMethod = GetInterpolationMethod(initialFieldData.InterpolationMethod)
            };

            if (initialFieldData.InterpolationMethod == InitialFieldInterpolationMethod.Averaging)
            {
                operation.AveragingMethod = GetAveragingType(initialFieldData.AveragingType);
                operation.RelativeSearchCellSize = initialFieldData.AveragingRelSize;
                operation.MinSamplePoints = initialFieldData.AveragingNumMin;
            }

            return operation;
        }

        private string GetDataFilePath(InitialFieldData initialFieldData)
        {
            if (string.IsNullOrWhiteSpace(initialFieldData.ParentDataDirectory))
            {
                return initialFieldData.DataFile;
            }
            
            return fileSystem.GetAbsolutePath(initialFieldData.ParentDataDirectory, initialFieldData.DataFile);
        }
        
        private static SpatialInterpolationMethod GetInterpolationMethod(InitialFieldInterpolationMethod initialFieldInterpolationMethod)
        {
            switch (initialFieldInterpolationMethod)
            {
                case InitialFieldInterpolationMethod.Triangulation:
                    return SpatialInterpolationMethod.Triangulation;
                case InitialFieldInterpolationMethod.Averaging:
                    return SpatialInterpolationMethod.Averaging;
                default:
                    throw new ArgumentOutOfRangeException(nameof(initialFieldInterpolationMethod), initialFieldInterpolationMethod, null);
            }
        }

        private static GridCellAveragingMethod GetAveragingType(InitialFieldAveragingType averagingType)
        {
            switch (averagingType)
            {
                case InitialFieldAveragingType.Mean:
                    return GridCellAveragingMethod.SimpleAveraging;
                case InitialFieldAveragingType.NearestNb:
                    return GridCellAveragingMethod.ClosestPoint;
                case InitialFieldAveragingType.Max:
                    return GridCellAveragingMethod.MaximumValue;
                case InitialFieldAveragingType.Min:
                    return GridCellAveragingMethod.MinimumValue;
                case InitialFieldAveragingType.InverseDistance:
                    return GridCellAveragingMethod.InverseWeightedDistance;
                case InitialFieldAveragingType.MinAbsolute:
                    return GridCellAveragingMethod.MinAbs;
                default:
                    throw new ArgumentOutOfRangeException(nameof(averagingType), averagingType, null);
            }
        }

        private static PointwiseOperationType GetOperand(InitialFieldOperand operand)
        {
            switch (operand)
            {
                case InitialFieldOperand.Override:
                    return PointwiseOperationType.Overwrite;
                case InitialFieldOperand.Append:
                    return PointwiseOperationType.OverwriteWhereMissing;
                case InitialFieldOperand.Add:
                    return PointwiseOperationType.Add;
                case InitialFieldOperand.Multiply:
                    return PointwiseOperationType.Multiply;
                case InitialFieldOperand.Maximum:
                    return PointwiseOperationType.Maximum;
                case InitialFieldOperand.Minimum:
                    return PointwiseOperationType.Minimum;
                default:
                    throw new ArgumentOutOfRangeException(nameof(operand), operand, null);
            }
        }
    }
}