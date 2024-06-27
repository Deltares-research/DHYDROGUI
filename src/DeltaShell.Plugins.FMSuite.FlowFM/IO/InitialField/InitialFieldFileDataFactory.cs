using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Hydro;
using Deltares.Infrastructure.API.Guards;
using Deltares.Infrastructure.Extensions;
using DeltaShell.NGHS.Common.Utils;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using DHYDRO.Common.IO.InitialField;
using SharpMap.Api.SpatialOperations;
using SharpMap.SpatialOperations;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO.InitialField
{
    /// <summary>
    /// Factory class for <see cref="ISpatialOperation"/>.
    /// </summary>
    public sealed class InitialFieldFileDataFactory
    {
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

            InitialFieldData initialFieldDataOneD = CreateInitialFieldFromGlobalQuantity(modelDefinition);
            AddInitialField(initialFieldFileData, initialFieldDataOneD);

            foreach (InitialFieldQuantity quantity in InitialFieldFileQuantities.SupportedQuantities.Keys)
            {
                if (!CanWriteSpatialOperationsForQuantity(quantity, modelDefinition))
                {
                    continue;
                }

                IEnumerable<ISpatialOperation> spatialOperations = GetSpatialOperations(modelDefinition, quantity);

                foreach (ISpatialOperation spatialOperation in spatialOperations)
                {
                    InitialFieldData initialFieldData = CreateInitialFieldFromSpatialOperation(spatialOperation, quantity, uniqueFileNameProvider);
                    AddInitialField(initialFieldFileData, initialFieldData);
                }
            }

            return initialFieldFileData;
        }

        private static bool CanWriteSpatialOperationsForQuantity(InitialFieldQuantity quantity, WaterFlowFMModelDefinition modelDefinition)
        {
            InitialConditionQuantity globalQuantity = GetInitialConditionGlobalQuantity2D(modelDefinition);

            if ((quantity == InitialFieldQuantity.WaterLevel && globalQuantity != InitialConditionQuantity.WaterLevel) ||
                (quantity == InitialFieldQuantity.WaterDepth && globalQuantity != InitialConditionQuantity.WaterDepth))
            {
                return false;
            }

            return true;
        }
        
        private static InitialConditionQuantity GetInitialConditionGlobalQuantity1D(WaterFlowFMModelDefinition modelDefinition)
        {
            return (InitialConditionQuantity)modelDefinition.GetModelProperty(GuiProperties.InitialConditionGlobalQuantity1D).Value;
        }

        private static InitialConditionQuantity GetInitialConditionGlobalQuantity2D(WaterFlowFMModelDefinition modelDefinition)
        {
            return (InitialConditionQuantity)modelDefinition.GetModelProperty(GuiProperties.InitialConditionGlobalQuantity2D).Value;
        }
        
        private static IEnumerable<ISpatialOperation> GetSpatialOperations(WaterFlowFMModelDefinition modelDefinition, InitialFieldQuantity quantity)
        {
            string spatialOperationQuantity = GetSpatialOperationQuantityName(quantity);

            IList<ISpatialOperation> spatialOperations = modelDefinition.GetSpatialOperations(spatialOperationQuantity);
            return spatialOperations ?? Enumerable.Empty<ISpatialOperation>();
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

        private static InitialFieldData CreateInitialFieldFromGlobalQuantity(WaterFlowFMModelDefinition modelDefinition)
        {
            InitialConditionQuantity globalQuantity = GetInitialConditionGlobalQuantity1D(modelDefinition);
            
            return new InitialFieldData
            {
                Quantity = GetInitialFieldQuantity(globalQuantity),
                DataFile = GetOneDFieldFileName(globalQuantity),
                DataFileType = InitialFieldDataFileType.OneDField,
                LocationType = InitialFieldLocationType.OneD
            };
        }

        private static InitialFieldData CreateInitialFieldFromSpatialOperation(
            ISpatialOperation spatialOperation,
            InitialFieldQuantity quantity,
            UniqueFileNameProvider uniqueFileNameProvider)
        {
            var importSamplesOperation = spatialOperation as ImportSamplesOperationImportData;
            if (importSamplesOperation != null)
            {
                return CreateFromImportSamplesSpatialOperation(quantity, importSamplesOperation);
            }

            var polygonOperation = spatialOperation as SetValueOperation;
            if (polygonOperation != null)
            {
                return CreateFromSetValueOperation(quantity, polygonOperation, uniqueFileNameProvider);
            }

            var addSamplesOperation = spatialOperation as AddSamplesOperation;
            if (addSamplesOperation != null)
            {
                return CreateFromAddSamplesOperation(quantity, addSamplesOperation);
            }

            throw new NotSupportedException(
                $"Cannot serialize operation of type {spatialOperation.GetType()} to initial field file");
        }

        private static InitialFieldData CreateFromImportSamplesSpatialOperation(InitialFieldQuantity quantity,
                                                                                ImportSamplesOperationImportData operation)
        {
            InitialFieldData initialFieldData = CreateFromSpatialOperation(quantity, operation);
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

        private static InitialFieldData CreateFromSetValueOperation(InitialFieldQuantity quantity,
                                                                    SetValueOperation operation,
                                                                    UniqueFileNameProvider uniqueFileNameProvider)
        {
            InitialFieldData initialFieldData = CreateFromSpatialOperation(quantity, operation);
            initialFieldData.DataFile = GetPolFileName(operation, quantity, uniqueFileNameProvider);
            initialFieldData.DataFileType = InitialFieldDataFileType.Polygon;
            initialFieldData.InterpolationMethod = InitialFieldInterpolationMethod.Constant;
            initialFieldData.Value = operation.Value;
            initialFieldData.Operand = GetInitialFieldOperand(operation.OperationType);

            return initialFieldData;
        }

        private static InitialFieldData CreateFromAddSamplesOperation(InitialFieldQuantity quantity,
                                                                      AddSamplesOperation operation)
        {
            InitialFieldData initialFieldData = CreateFromSpatialOperation(quantity, operation);
            initialFieldData.DataFile = GetXyzFileName(quantity);
            initialFieldData.DataFileType = InitialFieldDataFileType.Sample;
            initialFieldData.InterpolationMethod = InitialFieldInterpolationMethod.Averaging;
            initialFieldData.Operand = InitialFieldOperand.Override;
            initialFieldData.AveragingType = InitialFieldAveragingType.NearestNb;
            initialFieldData.AveragingRelSize = 1.0;
            initialFieldData.AveragingNumMin = 1;

            return initialFieldData;
        }

        private static InitialFieldData CreateFromSpatialOperation(InitialFieldQuantity quantity, ISpatialOperation operation)
        {
            return new InitialFieldData
            {
                Quantity = quantity,
                LocationType = InitialFieldLocationType.TwoD,
                SpatialOperationName = operation.Name,
                SpatialOperationQuantity = GetSpatialOperationQuantityName(quantity)
            };
        }

        private static string GetSpatialOperationQuantityName(InitialFieldQuantity quantity)
            => InitialFieldFileQuantities.SupportedQuantities[quantity];

        private static string GetPolFileName(ISpatialOperation operation, InitialFieldQuantity quantity, UniqueFileNameProvider uniqueFileNameProvider)
        {
            string fileOperationName = ReplaceWhiteSpaceWith(operation.Name, '_');
            string fileQuantityName = quantity.GetDescription();
            string fileName = $"{fileQuantityName}_{fileOperationName}" + FileConstants.PolylineFileExtension;
            return uniqueFileNameProvider.GetUniqueFileNameFor(fileName);
        }

        private static string GetXyzFileName(InitialFieldQuantity quantity) 
            => quantity.GetDescription() + FileConstants.XyzFileExtension;
        
        private static string GetOneDFieldFileName(InitialConditionQuantity quantity)
            => $"Initial{quantity}.ini";

        private static InitialFieldQuantity GetInitialFieldQuantity(InitialConditionQuantity globalQuantity)
        {
            switch (globalQuantity)
            {
                case InitialConditionQuantity.WaterLevel:
                    return InitialFieldQuantity.WaterLevel;
                case InitialConditionQuantity.WaterDepth:
                    return InitialFieldQuantity.WaterDepth;
                default:
                    throw new ArgumentOutOfRangeException(nameof(globalQuantity), globalQuantity, null);
            }
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

        private static InitialFieldInterpolationMethod GetSpatialOperationMethod(ImportSamplesOperationImportData operation)
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