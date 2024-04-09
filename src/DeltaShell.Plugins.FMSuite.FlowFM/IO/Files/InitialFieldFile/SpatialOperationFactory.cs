using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Utils.Guards;
using DeltaShell.Plugins.FMSuite.Common.IO.Files;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Files.Helpers;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Files.InitialFieldFile.Data;
using DHYDRO.Common.Extensions;
using NetTopologySuite.Extensions.Features;
using SharpMap.Api.SpatialOperations;
using SharpMap.Data.Providers;
using SharpMap.SpatialOperations;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO.Files.InitialFieldFile
{
    /// <summary>
    /// Factory class for <see cref="ISpatialOperation"/>.
    /// </summary>
    public sealed class SpatialOperationFactory
    {
        /// <summary>
        /// Create a spatial operation from the given initial field.
        /// </summary>
        /// <param name="initialField"> The initial field. </param>
        /// <returns>A newly constructed spatial operation. </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="initialField"/> is <c>null</c>.
        /// </exception>
        public ISpatialOperation CreateFromInitialField(InitialField initialField)
        {
            Ensure.NotNull(initialField, nameof(initialField));

            switch (initialField.DataFileType)
            {
                case InitialFieldDataFileType.GeoTIFF:
                case InitialFieldDataFileType.ArcInfo:
                    return CreateSamplesOperation<ImportRasterSamplesOperationImportData>(initialField);
                case InitialFieldDataFileType.Sample:
                    return CreateSamplesOperation<ImportSamplesSpatialOperation>(initialField);
                case InitialFieldDataFileType.Polygon:
                    return CreatePolygonOperation(initialField);
                default:
                    throw new ArgumentException(
                        $"Cannot construct spatial operation for file {initialField.DataFile} with file type {initialField.DataFileType.GetDescription()}");
            }
        }

        private static ISpatialOperation CreatePolygonOperation(InitialField initialField)
        {
            IEnumerable<Feature> features = new PolFile<Feature2DPolygon>().Read(initialField.DataFile).Select(f => new Feature
            {
                Geometry = f.Geometry,
                Attributes = f.Attributes
            });

            string operationName = Path.GetFileNameWithoutExtension(initialField.DataFile)
                                       .ReplaceCaseInsensitive(initialField.Quantity.GetDescription() + "_", string.Empty);
            var operation = new SetValueOperation
            {
                Value = initialField.Value,
                OperationType = GetOperand(initialField.Operand),
                Name = operationName,
                Mask = { Provider = new FeatureCollection(features.ToList(), typeof(Feature)) }
            };

            return operation;
        }

        private static T CreateSamplesOperation<T>(InitialField initialField) where T : ImportSamplesSpatialOperation, new()
        {
            string operationName = Path.GetFileNameWithoutExtension(initialField.DataFile);

            var operation = new T
            {
                Name = operationName,
                FilePath = initialField.DataFile,
                Operand = GetOperand(initialField.Operand),
                InterpolationMethod = GetInterpolationMethod(initialField.InterpolationMethod)
            };

            if (initialField.InterpolationMethod == InitialFieldInterpolationMethod.Averaging)
            {
                operation.AveragingMethod = GetAveragingType(initialField.AveragingType);
                operation.RelativeSearchCellSize = initialField.AveragingRelSize;
                operation.MinSamplePoints = initialField.AveragingNumMin;
            }

            return operation;
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