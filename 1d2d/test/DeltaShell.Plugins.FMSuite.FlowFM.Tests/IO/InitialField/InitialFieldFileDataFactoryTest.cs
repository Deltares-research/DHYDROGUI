using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Hydro;
using DeltaShell.Plugins.FMSuite.FlowFM.IO;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.InitialField;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using DHYDRO.Common.IO.InitialField;
using NUnit.Framework;
using SharpMap.Api.SpatialOperations;
using SharpMap.SpatialOperations;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.IO.InitialField
{
    [TestFixture]
    public class InitialFieldFileDataFactoryTest
    {
        [Test]
        public void CreateFromModelDefinition_ModelDefinitionNull_ThrowsArgumentNullException()
        {
            // Arrange
            var factory = new InitialFieldFileDataFactory();

            // Act
            void Call() => factory.CreateFromModelDefinition(null);

            // Assert
            Assert.That(Call, Throws.ArgumentNullException);
        }

        [Test]
        public void CreateFromModelDefinition_EmptySpatialOperations_CreatesOneDFieldInitialCondition(
            [Values] InitialConditionQuantity globalQuantity)
        {
            // Arrange
            var factory = new InitialFieldFileDataFactory();
            var modelDefinition = new WaterFlowFMModelDefinition();

            SetGlobalQuantity(modelDefinition, globalQuantity);
            
            // Act
            InitialFieldFileData initialFieldFileData = factory.CreateFromModelDefinition(modelDefinition);
            
            // Act
            Assert.That(initialFieldFileData.Parameters, Is.Empty);
            Assert.That(initialFieldFileData.InitialConditions, Has.Count.EqualTo(1));
            
            InitialFieldData initialFieldData = initialFieldFileData.InitialConditions.Single();
            Assert.That(initialFieldData.Quantity, Is.EqualTo(Enum.Parse(typeof(InitialFieldQuantity), globalQuantity.ToString())));
            Assert.That(initialFieldData.DataFile, Is.EqualTo($"Initial{globalQuantity}.ini"));
            Assert.That(initialFieldData.DataFileType, Is.EqualTo(InitialFieldDataFileType.OneDField));
        }

        [Test]
        [TestCase(".asc", InitialFieldDataFileType.ArcInfo)]
        [TestCase(".tif", InitialFieldDataFileType.GeoTIFF)]
        [TestCase(".xyz", InitialFieldDataFileType.Sample)]
        public void CreateFromModelDefinition_FromImportSamplesSpatialOperationWithProvidedFileExtensionForWaterLevel_CreatesCorrectInitialConditions(string fileExtension, InitialFieldDataFileType expDataFileType)
        {
            // Arrange
            var factory = new InitialFieldFileDataFactory();

            const string spatialOperationName = "some_operation";
            var spatialOperation = new ImportSamplesOperationImportData
            {
                Name = spatialOperationName,
                FilePath = Path.ChangeExtension("some_operation", fileExtension),
                InterpolationMethod = SpatialInterpolationMethod.Triangulation,
                Operand = PointwiseOperationType.Overwrite
            };

            WaterFlowFMModelDefinition modelDefinition = GetModelDefinition(spatialOperation, WaterFlowFMModelDefinition.InitialWaterLevelDataItemName);
            SetGlobalQuantity(modelDefinition, InitialConditionQuantity.WaterLevel);
            
            // Act
            InitialFieldFileData initialFieldFileData = factory.CreateFromModelDefinition(modelDefinition);

            // Act
            Assert.That(initialFieldFileData.Parameters, Is.Empty);
            Assert.That(initialFieldFileData.InitialConditions, Has.Count.EqualTo(2));
            
            InitialFieldData initialFieldData = initialFieldFileData.InitialConditions.First();
            Assert.That(initialFieldData.Quantity, Is.EqualTo(InitialFieldQuantity.WaterLevel));
            Assert.That(initialFieldData.DataFile, Is.EqualTo("InitialWaterLevel.ini"));
            Assert.That(initialFieldData.DataFileType, Is.EqualTo(InitialFieldDataFileType.OneDField));

            initialFieldData = initialFieldFileData.InitialConditions.Last();
            Assert.That(initialFieldData.Quantity, Is.EqualTo(InitialFieldQuantity.WaterLevel));
            Assert.That(initialFieldData.DataFile, Is.EqualTo(spatialOperation.FilePath));
            Assert.That(initialFieldData.DataFileType, Is.EqualTo(expDataFileType));
            Assert.That(initialFieldData.InterpolationMethod, Is.EqualTo(InitialFieldInterpolationMethod.Triangulation));
            Assert.That(initialFieldData.LocationType, Is.EqualTo(InitialFieldLocationType.TwoD));
            Assert.That(initialFieldData.Operand, Is.EqualTo(InitialFieldOperand.Override));
            Assert.That(initialFieldData.SpatialOperationName, Is.EqualTo(spatialOperationName));
            Assert.That(initialFieldData.SpatialOperationQuantity, Is.EqualTo(WaterFlowFMModelDefinition.InitialWaterLevelDataItemName));
        }

        [Test]
        public void CreateFromModelDefinition_FromImportSamplesSpatialOperationForRoughness_CreatesCorrectParameterAndInitialCondition()
        {
            // Arrange
            var factory = new InitialFieldFileDataFactory();

            const string spatialOperationName = "some_operation";
            var spatialOperation = new ImportSamplesOperationImportData
            {
                Name = spatialOperationName,
                FilePath = "some_operation.xyz",
                InterpolationMethod = SpatialInterpolationMethod.Triangulation,
                Operand = PointwiseOperationType.Overwrite
            };

            WaterFlowFMModelDefinition modelDefinition = GetModelDefinition(spatialOperation, WaterFlowFMModelDefinition.RoughnessDataItemName);

            // Act
            InitialFieldFileData initialFieldFileData = factory.CreateFromModelDefinition(modelDefinition);

            // Assert
            Assert.That(initialFieldFileData.InitialConditions, Has.Count.EqualTo(1));
            Assert.That(initialFieldFileData.Parameters, Has.Count.EqualTo(1));

            InitialFieldData initialFieldData = initialFieldFileData.InitialConditions.Single();
            Assert.That(initialFieldData.Quantity, Is.EqualTo(InitialFieldQuantity.WaterLevel));
            Assert.That(initialFieldData.DataFile, Is.EqualTo("InitialWaterLevel.ini"));
            Assert.That(initialFieldData.DataFileType, Is.EqualTo(InitialFieldDataFileType.OneDField));
            
            initialFieldData = initialFieldFileData.Parameters.Single();
            Assert.That(initialFieldData.Quantity, Is.EqualTo(InitialFieldQuantity.FrictionCoefficient));
            Assert.That(initialFieldData.DataFile, Is.EqualTo(spatialOperation.FilePath));
            Assert.That(initialFieldData.DataFileType, Is.EqualTo(InitialFieldDataFileType.Sample));
            Assert.That(initialFieldData.InterpolationMethod, Is.EqualTo(InitialFieldInterpolationMethod.Triangulation));
            Assert.That(initialFieldData.LocationType, Is.EqualTo(InitialFieldLocationType.TwoD));
            Assert.That(initialFieldData.Operand, Is.EqualTo(InitialFieldOperand.Override));
            Assert.That(initialFieldData.SpatialOperationName, Is.EqualTo(spatialOperationName));
            Assert.That(initialFieldData.SpatialOperationQuantity, Is.EqualTo(WaterFlowFMModelDefinition.RoughnessDataItemName));
        }
        
        [Test]
        public void CreateFromModelDefinition_FromImportSamplesSpatialOperationWithNonMatchingInitialConditionGlobalQuantity_CreatesOneDFieldInitialCondition(
            [Values] InitialConditionQuantity globalQuantity)
        {
            // Arrange
            var factory = new InitialFieldFileDataFactory();

            const string spatialOperationName = "some_operation";
            var spatialOperation = new ImportSamplesOperationImportData
            {
                Name = spatialOperationName,
                FilePath = "some_operation.xyz",
                InterpolationMethod = SpatialInterpolationMethod.Triangulation,
                Operand = PointwiseOperationType.Overwrite
            };

            string nonMatchingSpatialOperationName = globalQuantity == InitialConditionQuantity.WaterDepth
                                                         ? WaterFlowFMModelDefinition.InitialWaterLevelDataItemName
                                                         : WaterFlowFMModelDefinition.InitialWaterDepthDataItemName;
            
            WaterFlowFMModelDefinition modelDefinition = GetModelDefinition(spatialOperation, nonMatchingSpatialOperationName);
            SetGlobalQuantity(modelDefinition, globalQuantity);

            // Act
            InitialFieldFileData initialFieldFileData = factory.CreateFromModelDefinition(modelDefinition);

            // Assert
            Assert.That(initialFieldFileData.Parameters, Is.Empty);
            Assert.That(initialFieldFileData.InitialConditions, Has.Count.EqualTo(1));
            
            InitialFieldData initialFieldData = initialFieldFileData.InitialConditions.Single();
            Assert.That(initialFieldData.Quantity, Is.EqualTo(Enum.Parse(typeof(InitialFieldQuantity), globalQuantity.ToString())));
            Assert.That(initialFieldData.DataFile, Is.EqualTo($"Initial{globalQuantity}.ini"));
            Assert.That(initialFieldData.DataFileType, Is.EqualTo(InitialFieldDataFileType.OneDField));
        }

        [Test]
        public void CreateFromModelDefinition_FromImportSamplesSpatialOperationWithTriangulationInterpolationMethodForWaterDepth_CreatesCorrectInitialConditions()
        {
            // Arrange
            var factory = new InitialFieldFileDataFactory();

            const string spatialOperationName = "some_operation";
            var spatialOperation = new ImportSamplesOperationImportData
            {
                Name = spatialOperationName,
                FilePath = "some_operation.xyz",
                InterpolationMethod = SpatialInterpolationMethod.Triangulation,
                Operand = PointwiseOperationType.Overwrite
            };

            WaterFlowFMModelDefinition modelDefinition = GetModelDefinition(spatialOperation, WaterFlowFMModelDefinition.InitialWaterDepthDataItemName);
            SetGlobalQuantity(modelDefinition, InitialConditionQuantity.WaterDepth);

            // Act
            InitialFieldFileData initialFieldFileData = factory.CreateFromModelDefinition(modelDefinition);

            // Assert
            Assert.That(initialFieldFileData.Parameters, Is.Empty);
            Assert.That(initialFieldFileData.InitialConditions, Has.Count.EqualTo(2));

            InitialFieldData initialFieldData = initialFieldFileData.InitialConditions.First();
            Assert.That(initialFieldData.Quantity, Is.EqualTo(InitialFieldQuantity.WaterDepth));
            Assert.That(initialFieldData.DataFile, Is.EqualTo("InitialWaterDepth.ini"));
            Assert.That(initialFieldData.DataFileType, Is.EqualTo(InitialFieldDataFileType.OneDField));
            
            initialFieldData = initialFieldFileData.InitialConditions.Last();
            Assert.That(initialFieldData.Quantity, Is.EqualTo(InitialFieldQuantity.WaterDepth));
            Assert.That(initialFieldData.DataFile, Is.EqualTo(spatialOperation.FilePath));
            Assert.That(initialFieldData.DataFileType, Is.EqualTo(InitialFieldDataFileType.Sample));
            Assert.That(initialFieldData.InterpolationMethod, Is.EqualTo(InitialFieldInterpolationMethod.Triangulation));
            Assert.That(initialFieldData.Operand, Is.EqualTo(InitialFieldOperand.Override));
            Assert.That(initialFieldData.SpatialOperationName, Is.EqualTo(spatialOperationName));
            Assert.That(initialFieldData.SpatialOperationQuantity, Is.EqualTo(WaterFlowFMModelDefinition.InitialWaterDepthDataItemName));
        }

        [Test]
        [TestCase(GridCellAveragingMethod.SimpleAveraging, InitialFieldAveragingType.Mean)]
        [TestCase(GridCellAveragingMethod.ClosestPoint, InitialFieldAveragingType.NearestNb)]
        [TestCase(GridCellAveragingMethod.MaximumValue, InitialFieldAveragingType.Max)]
        [TestCase(GridCellAveragingMethod.MinimumValue, InitialFieldAveragingType.Min)]
        [TestCase(GridCellAveragingMethod.InverseWeightedDistance, InitialFieldAveragingType.InverseDistance)]
        [TestCase(GridCellAveragingMethod.MinAbs, InitialFieldAveragingType.MinAbsolute)]
        public void CreateFromModelDefinition_FromImportSamplesSpatialOperationWithAveragingInterpolationMethodWithProvidedAveragingTypeForRoughness_CreatesCorrectParameterAndInitialCondition(
            GridCellAveragingMethod spatialOperationAveragingType,
            InitialFieldAveragingType expAveragingType)
        {
            // Arrange
            var factory = new InitialFieldFileDataFactory();

            const string spatialOperationName = "some_operation";
            var spatialOperation = new ImportSamplesOperationImportData
            {
                Name = spatialOperationName,
                FilePath = "some_operation.xyz",
                InterpolationMethod = SpatialInterpolationMethod.Averaging,
                AveragingMethod = spatialOperationAveragingType,
                RelativeSearchCellSize = 1.23,
                MinSamplePoints = 4,
                Operand = PointwiseOperationType.Overwrite
            };

            WaterFlowFMModelDefinition modelDefinition = GetModelDefinition(spatialOperation, WaterFlowFMModelDefinition.RoughnessDataItemName);

            // Act
            InitialFieldFileData initialFieldFileData = factory.CreateFromModelDefinition(modelDefinition);

            // Assert
            Assert.That(initialFieldFileData.InitialConditions, Has.Count.EqualTo(1));
            Assert.That(initialFieldFileData.Parameters, Has.Count.EqualTo(1));
            
            InitialFieldData initialFieldData = initialFieldFileData.InitialConditions.Single();
            Assert.That(initialFieldData.Quantity, Is.EqualTo(InitialFieldQuantity.WaterLevel));
            Assert.That(initialFieldData.DataFile, Is.EqualTo("InitialWaterLevel.ini"));
            Assert.That(initialFieldData.DataFileType, Is.EqualTo(InitialFieldDataFileType.OneDField));
            
            initialFieldData = initialFieldFileData.Parameters.Single();
            Assert.That(initialFieldData.Quantity, Is.EqualTo(InitialFieldQuantity.FrictionCoefficient));
            Assert.That(initialFieldData.DataFile, Is.EqualTo(spatialOperation.FilePath));
            Assert.That(initialFieldData.DataFileType, Is.EqualTo(InitialFieldDataFileType.Sample));
            Assert.That(initialFieldData.InterpolationMethod, Is.EqualTo(InitialFieldInterpolationMethod.Averaging));
            Assert.That(initialFieldData.AveragingType, Is.EqualTo(expAveragingType));
            Assert.That(initialFieldData.AveragingRelSize, Is.EqualTo(1.23));
            Assert.That(initialFieldData.AveragingNumMin, Is.EqualTo(4));
            Assert.That(initialFieldData.Operand, Is.EqualTo(InitialFieldOperand.Override));
            Assert.That(initialFieldData.SpatialOperationName, Is.EqualTo(spatialOperationName));
            Assert.That(initialFieldData.SpatialOperationQuantity, Is.EqualTo(WaterFlowFMModelDefinition.RoughnessDataItemName));
        }

        [Test]
        [TestCase(PointwiseOperationType.Overwrite, InitialFieldOperand.Override)]
        [TestCase(PointwiseOperationType.OverwriteWhereMissing, InitialFieldOperand.Append)]
        [TestCase(PointwiseOperationType.Add, InitialFieldOperand.Add)]
        [TestCase(PointwiseOperationType.Multiply, InitialFieldOperand.Multiply)]
        [TestCase(PointwiseOperationType.Maximum, InitialFieldOperand.Maximum)]
        [TestCase(PointwiseOperationType.Minimum, InitialFieldOperand.Minimum)]
        public void CreateFromModelDefinition_FromImportSamplesSpatialOperationWithProvidedOperandForWaterLevel_CreatesCorrectInitialConditions(
            PointwiseOperationType spatialOperationOperandType,
            InitialFieldOperand expOperand)
        {
            // Arrange
            var factory = new InitialFieldFileDataFactory();

            const string spatialOperationName = "some_operation";
            var spatialOperation = new ImportSamplesOperationImportData
            {
                Name = spatialOperationName,
                FilePath = "some_operation.xyz",
                InterpolationMethod = SpatialInterpolationMethod.Triangulation,
                Operand = spatialOperationOperandType
            };

            WaterFlowFMModelDefinition modelDefinition = GetModelDefinition(spatialOperation, WaterFlowFMModelDefinition.InitialWaterLevelDataItemName);
            SetGlobalQuantity(modelDefinition, InitialConditionQuantity.WaterLevel);

            // Act
            InitialFieldFileData initialFieldFileData = factory.CreateFromModelDefinition(modelDefinition);

            // Assert
            Assert.That(initialFieldFileData.Parameters, Is.Empty);
            Assert.That(initialFieldFileData.InitialConditions, Has.Count.EqualTo(2));
            
            InitialFieldData initialFieldData = initialFieldFileData.InitialConditions.First();
            Assert.That(initialFieldData.Quantity, Is.EqualTo(InitialFieldQuantity.WaterLevel));
            Assert.That(initialFieldData.DataFile, Is.EqualTo("InitialWaterLevel.ini"));
            Assert.That(initialFieldData.DataFileType, Is.EqualTo(InitialFieldDataFileType.OneDField));
            
            initialFieldData = initialFieldFileData.InitialConditions.Last();
            Assert.That(initialFieldData.Quantity, Is.EqualTo(InitialFieldQuantity.WaterLevel));
            Assert.That(initialFieldData.DataFile, Is.EqualTo(spatialOperation.FilePath));
            Assert.That(initialFieldData.DataFileType, Is.EqualTo(InitialFieldDataFileType.Sample));
            Assert.That(initialFieldData.InterpolationMethod, Is.EqualTo(InitialFieldInterpolationMethod.Triangulation));
            Assert.That(initialFieldData.LocationType, Is.EqualTo(InitialFieldLocationType.TwoD));
            Assert.That(initialFieldData.Operand, Is.EqualTo(expOperand));
            Assert.That(initialFieldData.SpatialOperationName, Is.EqualTo(spatialOperationName));
            Assert.That(initialFieldData.SpatialOperationQuantity, Is.EqualTo(WaterFlowFMModelDefinition.InitialWaterLevelDataItemName));
        }

        [Test]
        [TestCase(PointwiseOperationType.Overwrite, InitialFieldOperand.Override)]
        [TestCase(PointwiseOperationType.OverwriteWhereMissing, InitialFieldOperand.Append)]
        [TestCase(PointwiseOperationType.Add, InitialFieldOperand.Add)]
        [TestCase(PointwiseOperationType.Multiply, InitialFieldOperand.Multiply)]
        [TestCase(PointwiseOperationType.Maximum, InitialFieldOperand.Maximum)]
        [TestCase(PointwiseOperationType.Minimum, InitialFieldOperand.Minimum)]
        public void CreateFromModelDefinition_FromSetValueOperationWithProvidedOperandForInfiltration_CreatesCorrectConditions(
            PointwiseOperationType spatialOperationOperandType,
            InitialFieldOperand expOperand)
        {
            // Arrange
            var factory = new InitialFieldFileDataFactory();

            const string spatialOperationName = "some_operation";
            var spatialOperation = new SetValueOperation
            {
                Name = spatialOperationName,
                Value = 1.23,
                OperationType = spatialOperationOperandType
            };

            WaterFlowFMModelDefinition modelDefinition = GetModelDefinition(spatialOperation, WaterFlowFMModelDefinition.InfiltrationDataItemName);
            SetGlobalQuantity(modelDefinition, InitialConditionQuantity.WaterDepth);

            // Act
            InitialFieldFileData initialFieldFileData = factory.CreateFromModelDefinition(modelDefinition);

            // Assert
            Assert.That(initialFieldFileData.InitialConditions, Has.Count.EqualTo(2));
            Assert.That(initialFieldFileData.Parameters, Is.Empty);
            
            InitialFieldData initialFieldData = initialFieldFileData.InitialConditions.First();
            Assert.That(initialFieldData.Quantity, Is.EqualTo(InitialFieldQuantity.WaterDepth));
            Assert.That(initialFieldData.DataFile, Is.EqualTo("InitialWaterDepth.ini"));
            Assert.That(initialFieldData.DataFileType, Is.EqualTo(InitialFieldDataFileType.OneDField));

            initialFieldData = initialFieldFileData.InitialConditions.Last();
            Assert.That(initialFieldData.Quantity, Is.EqualTo(InitialFieldQuantity.InfiltrationCapacity));
            Assert.That(initialFieldData.DataFile, Is.EqualTo("InfiltrationCapacity_some_operation.pol"));
            Assert.That(initialFieldData.DataFileType, Is.EqualTo(InitialFieldDataFileType.Polygon));
            Assert.That(initialFieldData.InterpolationMethod, Is.EqualTo(InitialFieldInterpolationMethod.Constant));
            Assert.That(initialFieldData.LocationType, Is.EqualTo(InitialFieldLocationType.TwoD));
            Assert.That(initialFieldData.Value, Is.EqualTo(1.23));
            Assert.That(initialFieldData.Operand, Is.EqualTo(expOperand));
            Assert.That(initialFieldData.SpatialOperationName, Is.EqualTo(spatialOperationName));
            Assert.That(initialFieldData.SpatialOperationQuantity, Is.EqualTo(WaterFlowFMModelDefinition.InfiltrationDataItemName));
        }

        [Test]
        public void CreateFromModelDefinition_FromAddSamplesOperationForWaterDepth_CreatesCorrectInitialConditions()
        {
            // Arrange
            var factory = new InitialFieldFileDataFactory();
            const string spatialOperationName = "operation_name";
            var spatialOperation = new AddSamplesOperation(true) { Name = spatialOperationName };

            WaterFlowFMModelDefinition modelDefinition = GetModelDefinition(spatialOperation, WaterFlowFMModelDefinition.InitialWaterDepthDataItemName);
            SetGlobalQuantity(modelDefinition, InitialConditionQuantity.WaterDepth);
            
            // Act
            InitialFieldFileData initialFieldFileData = factory.CreateFromModelDefinition(modelDefinition);

            // Assert
            Assert.That(initialFieldFileData.Parameters, Is.Empty);
            Assert.That(initialFieldFileData.InitialConditions, Has.Count.EqualTo(2));

            InitialFieldData initialFieldData = initialFieldFileData.InitialConditions.Last();
            Assert.That(initialFieldData.Quantity, Is.EqualTo(InitialFieldQuantity.WaterDepth));
            Assert.That(initialFieldData.DataFile, Is.EqualTo("waterdepth.xyz"));
            Assert.That(initialFieldData.DataFileType, Is.EqualTo(InitialFieldDataFileType.Sample));
            Assert.That(initialFieldData.InterpolationMethod, Is.EqualTo(InitialFieldInterpolationMethod.Averaging));
            Assert.That(initialFieldData.Operand, Is.EqualTo(InitialFieldOperand.Override));
            Assert.That(initialFieldData.LocationType, Is.EqualTo(InitialFieldLocationType.TwoD));
            Assert.That(initialFieldData.AveragingType, Is.EqualTo(InitialFieldAveragingType.NearestNb));
            Assert.That(initialFieldData.AveragingRelSize, Is.EqualTo(1.0));
            Assert.That(initialFieldData.AveragingNumMin, Is.EqualTo(1));
            Assert.That(initialFieldData.SpatialOperationName, Is.EqualTo(spatialOperationName));
            Assert.That(initialFieldData.SpatialOperationQuantity, Is.EqualTo(WaterFlowFMModelDefinition.InitialWaterDepthDataItemName));
        }
        
        private static WaterFlowFMModelDefinition GetModelDefinition(ISpatialOperation spatialOperation, string name)
        {
            return new WaterFlowFMModelDefinition { SpatialOperations = { [name] = new List<ISpatialOperation> { spatialOperation } } };
        }

        private static void SetGlobalQuantity(WaterFlowFMModelDefinition modelDefinition, InitialConditionQuantity globalQuantity)
        {
            modelDefinition.SetModelProperty(GuiProperties.InitialConditionGlobalQuantity1D, ((int) globalQuantity).ToString());
            modelDefinition.SetModelProperty(GuiProperties.InitialConditionGlobalQuantity2D, ((int) globalQuantity).ToString());
        }
    }
}