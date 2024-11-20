using System.IO.Abstractions.TestingHelpers;
using DelftTools.TestUtils;
using DeltaShell.Plugins.FMSuite.FlowFM.IO;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Files.Helpers;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Files.InitialField;
using DHYDRO.Common.IO.InitialField;
using DHYDRO.Common.TestUtils.IO.InitialField;
using NUnit.Framework;
using SharpMap.Api.SpatialOperations;
using SharpMap.SpatialOperations;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.IO.Files.InitialField
{
    [TestFixture]
    public class SpatialOperationFactoryTest
    {
        private MockFileSystem fileSystem;

        [SetUp]
        public void SetUp()
        {
            fileSystem = new MockFileSystem();
        }
        
        [Test]
        public void Constructor_FileSystemNull_ThrowsArgumentNullException()
        {
            // Act
            void Call() => _ = new SpatialOperationFactory(null);

            // Assert
            Assert.That(Call, Throws.ArgumentNullException);
        }
        
        [Test]
        public void CreateFromInitialFieldData_InitialFieldDataNull_ThrowsArgumentNullException()
        {
            // Arrange
            SpatialOperationFactory factory = CreateFactory();

            // Act
            void Call() => factory.CreateFromInitialFieldData(null);

            // Assert
            Assert.That(Call, Throws.ArgumentNullException);
        }

        [Test]
        [TestCase(InitialFieldDataFileType.ArcInfo)]
        [TestCase(InitialFieldDataFileType.GeoTIFF)]
        public void CreateFromInitialFieldData_FromArcInfoOrGeoTiffData_CreatesImportRasterSamplesOperationImportData(InitialFieldDataFileType dataFileType)
        {
            // Arrange
            SpatialOperationFactory factory = CreateFactory();
            InitialFieldData initialFieldData = InitialFieldDataBuilder.Start().AddRequiredValues().Build();
            initialFieldData.DataFileType = dataFileType;

            // Act
            ISpatialOperation result = factory.CreateFromInitialFieldData(initialFieldData);

            // Assert
            var spatialOperation = result as ImportRasterSamplesOperationImportData;
            Assert.That(spatialOperation, Is.Not.Null);
            Assert.That(spatialOperation.Name, Is.EqualTo("water_level"));
            Assert.That(spatialOperation.FilePath, Is.EqualTo(initialFieldData.DataFile));
            Assert.That(spatialOperation.Operand, Is.EqualTo(PointwiseOperationType.Overwrite));
            Assert.That(spatialOperation.InterpolationMethod, Is.EqualTo(SpatialInterpolationMethod.Triangulation));
        }

        [Test]
        [TestCase(InitialFieldOperand.Override, PointwiseOperationType.Overwrite)]
        [TestCase(InitialFieldOperand.Append, PointwiseOperationType.OverwriteWhereMissing)]
        [TestCase(InitialFieldOperand.Add, PointwiseOperationType.Add)]
        [TestCase(InitialFieldOperand.Multiply, PointwiseOperationType.Multiply)]
        [TestCase(InitialFieldOperand.Maximum, PointwiseOperationType.Maximum)]
        [TestCase(InitialFieldOperand.Minimum, PointwiseOperationType.Minimum)]
        public void CreateFromInitialFieldData_FromSamplesData_CreatesImportSamplesSpatialOperationWithCorrectOperand(
            InitialFieldOperand operand,
            PointwiseOperationType expOperand)
        {
            // Arrange
            SpatialOperationFactory factory = CreateFactory();
            InitialFieldData initialFieldData = InitialFieldDataBuilder.Start().AddRequiredValues().Build();
            initialFieldData.Operand = operand;

            // Act
            ISpatialOperation result = factory.CreateFromInitialFieldData(initialFieldData);

            // Assert
            var spatialOperation = result as ImportSamplesSpatialOperation;
            Assert.That(spatialOperation, Is.Not.Null);
            Assert.That(spatialOperation.Name, Is.EqualTo("water_level"));
            Assert.That(spatialOperation.FilePath, Is.EqualTo(initialFieldData.DataFile));
            Assert.That(spatialOperation.Operand, Is.EqualTo(expOperand));
            Assert.That(spatialOperation.InterpolationMethod, Is.EqualTo(SpatialInterpolationMethod.Triangulation));
        }

        [TestCase(InitialFieldAveragingType.Mean, GridCellAveragingMethod.SimpleAveraging)]
        [TestCase(InitialFieldAveragingType.NearestNb, GridCellAveragingMethod.ClosestPoint)]
        [TestCase(InitialFieldAveragingType.Max, GridCellAveragingMethod.MaximumValue)]
        [TestCase(InitialFieldAveragingType.Min, GridCellAveragingMethod.MinimumValue)]
        [TestCase(InitialFieldAveragingType.InverseDistance, GridCellAveragingMethod.InverseWeightedDistance)]
        [TestCase(InitialFieldAveragingType.MinAbsolute, GridCellAveragingMethod.MinAbs)]
        public void CreateFromInitialFieldData_FromSamplesData_WithAveragingInterpolation_CreatesCorrectImportSamplesSpatialOperation(
            InitialFieldAveragingType averagingType,
            GridCellAveragingMethod expAveragingType)
        {
            // Arrange
            SpatialOperationFactory factory = CreateFactory();
            InitialFieldData initialFieldData = InitialFieldDataBuilder.Start().AddRequiredValues().AddAveragingInterpolation().Build();
            initialFieldData.AveragingType = averagingType;

            // Act
            ISpatialOperation result = factory.CreateFromInitialFieldData(initialFieldData);

            // Assert
            var spatialOperation = result as ImportSamplesSpatialOperation;
            Assert.That(spatialOperation, Is.Not.Null);
            Assert.That(spatialOperation.Name, Is.EqualTo("water_level"));
            Assert.That(spatialOperation.FilePath, Is.EqualTo(initialFieldData.DataFile));
            Assert.That(spatialOperation.Operand, Is.EqualTo(PointwiseOperationType.Overwrite));
            Assert.That(spatialOperation.InterpolationMethod, Is.EqualTo(SpatialInterpolationMethod.Averaging));
            Assert.That(spatialOperation.AveragingMethod, Is.EqualTo(expAveragingType));
            Assert.That(spatialOperation.RelativeSearchCellSize, Is.EqualTo(1.23));
            Assert.That(spatialOperation.MinSamplePoints, Is.EqualTo(2));
        }

        [Test]
        public void CreateFromInitialFieldData_FromSamplesData_WithTriangulationInterpolation_CreatesCorrectImportSamplesSpatialOperation()
        {
            // Arrange
            SpatialOperationFactory factory = CreateFactory();
            InitialFieldData initialFieldData = InitialFieldDataBuilder.Start().AddRequiredValues().Build();

            // Act
            ISpatialOperation result = factory.CreateFromInitialFieldData(initialFieldData);

            // Assert
            var spatialOperation = result as ImportSamplesSpatialOperation;
            Assert.That(spatialOperation, Is.Not.Null);
            Assert.That(spatialOperation.Name, Is.EqualTo("water_level"));
            Assert.That(spatialOperation.FilePath, Is.EqualTo(initialFieldData.DataFile));
            Assert.That(spatialOperation.Operand, Is.EqualTo(PointwiseOperationType.Overwrite));
            Assert.That(spatialOperation.InterpolationMethod, Is.EqualTo(SpatialInterpolationMethod.Triangulation));
        }

        [Test]
        [TestCase(InitialFieldOperand.Override, PointwiseOperationType.Overwrite)]
        [TestCase(InitialFieldOperand.Append, PointwiseOperationType.OverwriteWhereMissing)]
        [TestCase(InitialFieldOperand.Add, PointwiseOperationType.Add)]
        [TestCase(InitialFieldOperand.Multiply, PointwiseOperationType.Multiply)]
        [TestCase(InitialFieldOperand.Maximum, PointwiseOperationType.Maximum)]
        [TestCase(InitialFieldOperand.Minimum, PointwiseOperationType.Minimum)]
        public void CreateFromInitialFieldData_FromPolygonData_CreatesCorrectSetValueOperation(InitialFieldOperand operand,
                                                                                               PointwiseOperationType expOperand)
        {
            using (var temp = new TemporaryDirectory())
            {
                // Arrange
                SpatialOperationFactory factory = CreateFactory();
                InitialFieldData initialFieldData = InitialFieldDataBuilder.Start().AddRequiredValues().AddPolygonDataFileType().Build();
                initialFieldData.Operand = operand;
                initialFieldData.DataFile = temp.CreateFile("water_level.pol", GetPolFileContent());
                initialFieldData.ParentDataDirectory = temp.Path;

                // Act
                ISpatialOperation result = factory.CreateFromInitialFieldData(initialFieldData);

                // Assert
                var spatialOperation = result as SetValueOperation;
                Assert.That(spatialOperation, Is.Not.Null);
                Assert.That(spatialOperation.Name, Is.EqualTo("water_level"));
                Assert.That(spatialOperation.Value, Is.EqualTo(7));
                Assert.That(spatialOperation.OperationType, Is.EqualTo(expOperand));
            }
        }

        private SpatialOperationFactory CreateFactory()
        {
            return new SpatialOperationFactory(fileSystem);
        }

        private static string GetPolFileContent()
        {
            return @"
water_level
3       2
1.23    2.34
3.45    4.56
5.67    6.78";
        }
    }
}