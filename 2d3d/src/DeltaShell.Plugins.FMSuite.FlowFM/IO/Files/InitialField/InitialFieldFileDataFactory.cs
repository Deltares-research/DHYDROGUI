using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Deltares.Infrastructure.API.Guards;
using Deltares.Infrastructure.Extensions;
using DeltaShell.NGHS.Common.Utils;
using DeltaShell.Plugins.FMSuite.Common.IO;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Files.Helpers;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using DHYDRO.Common.IO.InitialField;
using SharpMap.Api.SpatialOperations;
using SharpMap.SpatialOperations;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO.Files.InitialField
{
    /// <summary>
    /// Factory class for <see cref="ISpatialOperation"/>.
    /// </summary>
    public sealed class InitialFieldFileDataFactory
    {
        // Dictionary containing the quantity names stored in our domain model, with their corresponding quantity in the file.
        private static readonly IDictionary<string, InitialFieldQuantity> quantities = new Dictionary<string, InitialFieldQuantity>
        {
            { WaterFlowFMModelDefinition.InitialWaterLevelDataItemName, InitialFieldQuantity.WaterLevel },
            { WaterFlowFMModelDefinition.RoughnessDataItemName, InitialFieldQuantity.FrictionCoefficient }
        };

        /// <summary>
        /// Collection of supported quantities, represented by their names as stored in the domain model.
        /// </summary>
        public static IEnumerable<string> SupportedQuantities => quantities.Keys;

        /// <summary>
        /// Create an initial field file data from the given model definition.
        /// </summary>
        /// <param name="modelDefinition"> The model definition. </param>
        /// <returns>A newly constructed initial field file data instance. </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="modelDefinition"/> is <c>null</c>.
        /// </exception>
        public InitialFieldFileData CreateFromModelDefinition(WaterFlowFMModelDefinition modelDefinition)
        {
            Ensure.NotNull(modelDefinition, nameof(modelDefinition));

            var initialFieldFileData = new InitialFieldFileData();
            var uniqueFileNameProvider = new UniqueFileNameProvider();

            foreach (string domainQuantityName in SupportedQuantities)
            {
                IList<ISpatialOperation> spatialOperations = modelDefinition.GetSpatialOperations(domainQuantityName);
                if (spatialOperations == null)
                {
                    continue;
                }

                foreach (ISpatialOperation spatialOperation in spatialOperations)
                {
                    InitialFieldData initialFieldData = CreateInitialFieldFromSpatialOperation(spatialOperation, domainQuantityName, uniqueFileNameProvider);
                    AddInitialField(initialFieldFileData, initialFieldData);
                }
            }

            return initialFieldFileData;
        }

        private static void AddInitialField(InitialFieldFileData initialFieldFileData, InitialFieldData initialFieldData)
        {
            if (initialFieldData.Quantity == InitialFieldQuantity.FrictionCoefficient)
            {
                initialFieldFileData.AddParameter(initialFieldData);
            }
            else
            {
                initialFieldFileData.AddInitialCondition(initialFieldData);
            }
        }

        private static InitialFieldData CreateInitialFieldFromSpatialOperation(
            ISpatialOperation spatialOperation,
            string quantityName,
            UniqueFileNameProvider uniqueFileNameProvider)
        {
            var importSamplesOperation = spatialOperation as ImportSamplesSpatialOperation;
            if (importSamplesOperation != null)
            {
                return CreateFromImportSamplesSpatialOperation(quantityName, importSamplesOperation);
            }

            var polygonOperation = spatialOperation as SetValueOperation;
            if (polygonOperation != null)
            {
                return CreateFromSetValueOperation(quantityName, polygonOperation, uniqueFileNameProvider);
            }

            var addSamplesOperation = spatialOperation as AddSamplesOperation;
            if (addSamplesOperation != null)
            {
                return CreateFromAddSamplesOperation(quantityName, addSamplesOperation);
            }

            throw new NotSupportedException(
                $"Cannot serialize operation of type {spatialOperation.GetType()} to initial field file");
        }

        private static InitialFieldData CreateFromImportSamplesSpatialOperation(string quantityName,
                                                                                ImportSamplesSpatialOperation operation)
        {
            InitialFieldData initialFieldData = Create(quantityName, operation);
            initialFieldData.DataFile = Path.GetFileName(operation.FilePath);
            initialFieldData.DataFileType = GetSpatialOperationFileType(operation);
            initialFieldData.InterpolationMethod = GetSpatialOperationMethod(operation);
            initialFieldData.Operand = GetInitialFieldOperand(operation.Operand);

            if (initialFieldData.InterpolationMethod == InitialFieldInterpolationMethod.Averaging)
            {
                initialFieldData.AveragingType = GetInitialFieldAveragingType(operation.AveragingMethod);
                initialFieldData.AveragingRelSize = operation.RelativeSearchCellSize;
                initialFieldData.AveragingNumMin = operation.MinSamplePoints;
            }

            return initialFieldData;
        }

        private static InitialFieldData CreateFromSetValueOperation(string quantityName,
                                                                    SetValueOperation operation,
                                                                    UniqueFileNameProvider uniqueFileNameProvider)
        {
            InitialFieldData initialFieldData = Create(quantityName, operation);
            initialFieldData.DataFile = GetPolFileName(operation, quantityName, uniqueFileNameProvider);
            initialFieldData.DataFileType = InitialFieldDataFileType.Polygon;
            initialFieldData.InterpolationMethod = InitialFieldInterpolationMethod.Constant;
            initialFieldData.Value = operation.Value;
            initialFieldData.Operand = GetInitialFieldOperand(operation.OperationType);

            return initialFieldData;
        }

        private static InitialFieldData CreateFromAddSamplesOperation(string quantityName,
                                                                      AddSamplesOperation operation)
        {
            InitialFieldData initialFieldData = Create(quantityName, operation);
            initialFieldData.DataFile = GetXyzFileName(quantityName);
            initialFieldData.DataFileType = InitialFieldDataFileType.Sample;
            initialFieldData.InterpolationMethod = InitialFieldInterpolationMethod.Averaging;
            initialFieldData.Operand = InitialFieldOperand.Override;
            initialFieldData.AveragingType = InitialFieldAveragingType.NearestNb;
            initialFieldData.AveragingRelSize = 1.0;
            initialFieldData.AveragingNumMin = 1;

            return initialFieldData;
        }

        private static InitialFieldData Create(string quantityName, ISpatialOperation operation)
        {
            return new InitialFieldData
            {
                Quantity = quantities[quantityName],
                LocationType = InitialFieldLocationType.TwoD,
                SpatialOperationName = operation.Name,
                SpatialOperationQuantity = quantityName
            };
        }

        private static string GetPolFileName(ISpatialOperation operation, string quantityName, UniqueFileNameProvider uniqueFileNameProvider)
        {
            string fileOperationName = ReplaceWhiteSpaceWith(operation.Name, '_');
            string fileQuantityName = quantities[quantityName].GetDescription();
            string fileName = $"{fileQuantityName}_{fileOperationName}" + FileConstants.PolylineFileExtension;
            return uniqueFileNameProvider.GetUniqueFileNameFor(fileName);
        }

        private static string GetXyzFileName(string quantityName)
        {
            return quantities[quantityName].GetDescription() + FileConstants.XyzFileExtension;
        }

        private static InitialFieldOperand GetInitialFieldOperand(PointwiseOperationType pointWiseOperationType)
        {
            switch (pointWiseOperationType)
            {
                case PointwiseOperationType.Overwrite:
                    return InitialFieldOperand.Override;
                case PointwiseOperationType.OverwriteWhereMissing:
                    return InitialFieldOperand.Append;
                case PointwiseOperationType.Add:
                    return InitialFieldOperand.Add;
                case PointwiseOperationType.Multiply:
                    return InitialFieldOperand.Multiply;
                case PointwiseOperationType.Maximum:
                    return InitialFieldOperand.Maximum;
                case PointwiseOperationType.Minimum:
                    return InitialFieldOperand.Minimum;
                default:
                    throw new ArgumentOutOfRangeException(nameof(pointWiseOperationType), pointWiseOperationType, null);
            }
        }

        private static InitialFieldAveragingType GetInitialFieldAveragingType(GridCellAveragingMethod gridCellAveragingMethod)
        {
            switch (gridCellAveragingMethod)
            {
                case GridCellAveragingMethod.SimpleAveraging:
                    return InitialFieldAveragingType.Mean;
                case GridCellAveragingMethod.ClosestPoint:
                    return InitialFieldAveragingType.NearestNb;
                case GridCellAveragingMethod.MaximumValue:
                    return InitialFieldAveragingType.Max;
                case GridCellAveragingMethod.MinimumValue:
                    return InitialFieldAveragingType.Min;
                case GridCellAveragingMethod.InverseWeightedDistance:
                    return InitialFieldAveragingType.InverseDistance;
                case GridCellAveragingMethod.MinAbs:
                    return InitialFieldAveragingType.MinAbsolute;
                default:
                    throw new ArgumentOutOfRangeException(nameof(gridCellAveragingMethod), gridCellAveragingMethod, null);
            }
        }

        private static InitialFieldInterpolationMethod GetSpatialOperationMethod(ImportSamplesSpatialOperation operation)
        {
            switch (operation.InterpolationMethod)
            {
                case SpatialInterpolationMethod.Triangulation:
                    return InitialFieldInterpolationMethod.Triangulation;
                case SpatialInterpolationMethod.Averaging:
                    return InitialFieldInterpolationMethod.Averaging;
                default:
                    throw new NotSupportedException($"Type of {nameof(operation)} not supported: {operation.GetType()}");
            }
        }

        private static InitialFieldDataFileType GetSpatialOperationFileType(ImportSamplesOperation operation)
        {
            string fileExtension = Path.GetExtension(operation.FilePath);
            if (fileExtension.EqualsCaseInsensitive(".asc"))
            {
                return InitialFieldDataFileType.ArcInfo;
            }

            if (fileExtension.EqualsCaseInsensitive(".tif"))
            {
                return InitialFieldDataFileType.GeoTIFF;
            }

            return InitialFieldDataFileType.Sample;
        }

        private static string ReplaceWhiteSpaceWith(string input, char replacement)
        {
            IEnumerable<char> output = input.Select(ch => ReplaceWhiteSpaceWith(ch, replacement));
            return string.Concat(output);
        }

        private static char ReplaceWhiteSpaceWith(char input, char replacement)
        {
            return char.IsWhiteSpace(input) ? replacement : input;
        }
    }
}