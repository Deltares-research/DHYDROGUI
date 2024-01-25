using DelftTools.TestUtils;
using DeltaShell.Plugins.FMSuite.FlowFM.IO;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Files.Helpers;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Files.InitialFieldFile;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Files.InitialFieldFile.Data;
using NUnit.Framework;
using SharpMap.Api.SpatialOperations;
using SharpMap.SpatialOperations;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.IO.Files.InitialFieldFile
{
    [TestFixture]
    public class SpatialOperationFactoryTest
    {
        [Test]
        public void CreateFromInitialField_InitialFieldNull_ThrowsArgumentNullException()
        {
            // Arrange
            var factory = new SpatialOperationFactory();

            // Act
            void Call()
            {
                factory.CreateFromInitialField(null);
            }

            // Assert
            Assert.That(Call, Throws.ArgumentNullException);
        }

        [Test]
        [TestCase(InitialFieldDataFileType.ArcInfo)]
        [TestCase(InitialFieldDataFileType.GeoTIFF)]
        public void CreateFromInitialField_FromArcInfoOrGeoTiffData_CreatesImportRasterSamplesOperationImportData(InitialFieldDataFileType dataFileType)
        {
            // Arrange
            var factory = new SpatialOperationFactory();
            InitialField initialField = InitialFieldBuilder.Start().AddRequiredValues().Build();
            initialField.DataFileType = dataFileType;

            // Act
            ISpatialOperation result = factory.CreateFromInitialField(initialField);

            // Assert
            var spatialOperation = result as ImportRasterSamplesOperationImportData;
            Assert.That(spatialOperation, Is.Not.Null);
            Assert.That(spatialOperation.Name, Is.EqualTo("water_level"));
            Assert.That(spatialOperation.FilePath, Is.EqualTo(initialField.DataFile));
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
        public void CreateFromInitialField_FromSamplesData_CreatesImportSamplesSpatialOperationWithCorrectOperand(
            InitialFieldOperand operand,
            PointwiseOperationType expOperand)
        {
            // Arrange
            var factory = new SpatialOperationFactory();
            InitialField initialField = InitialFieldBuilder.Start().AddRequiredValues().Build();
            initialField.Operand = operand;

            // Act
            ISpatialOperation result = factory.CreateFromInitialField(initialField);

            // Assert
            var spatialOperation = result as ImportSamplesSpatialOperation;
            Assert.That(spatialOperation, Is.Not.Null);
            Assert.That(spatialOperation.Name, Is.EqualTo("water_level"));
            Assert.That(spatialOperation.FilePath, Is.EqualTo(initialField.DataFile));
            Assert.That(spatialOperation.Operand, Is.EqualTo(expOperand));
            Assert.That(spatialOperation.InterpolationMethod, Is.EqualTo(SpatialInterpolationMethod.Triangulation));
        }

        [TestCase(InitialFieldAveragingType.Mean, GridCellAveragingMethod.SimpleAveraging)]
        [TestCase(InitialFieldAveragingType.NearestNb, GridCellAveragingMethod.ClosestPoint)]
        [TestCase(InitialFieldAveragingType.Max, GridCellAveragingMethod.MaximumValue)]
        [TestCase(InitialFieldAveragingType.Min, GridCellAveragingMethod.MinimumValue)]
        [TestCase(InitialFieldAveragingType.InverseDistance, GridCellAveragingMethod.InverseWeightedDistance)]
        [TestCase(InitialFieldAveragingType.MinAbsolute, GridCellAveragingMethod.MinAbs)]
        public void CreateFromInitialField_FromSamplesData_WithAveragingInterpolation_CreatesCorrectImportSamplesSpatialOperation(
            InitialFieldAveragingType averagingType,
            GridCellAveragingMethod expAveragingType)
        {
            // Arrange
            var factory = new SpatialOperationFactory();
            InitialField initialField = InitialFieldBuilder.Start().AddRequiredValues().AddAveragingInterpolation().Build();
            initialField.AveragingType = averagingType;

            // Act
            ISpatialOperation result = factory.CreateFromInitialField(initialField);

            // Assert
            var spatialOperation = result as ImportSamplesSpatialOperation;
            Assert.That(spatialOperation, Is.Not.Null);
            Assert.That(spatialOperation.Name, Is.EqualTo("water_level"));
            Assert.That(spatialOperation.FilePath, Is.EqualTo(initialField.DataFile));
            Assert.That(spatialOperation.Operand, Is.EqualTo(PointwiseOperationType.Overwrite));
            Assert.That(spatialOperation.InterpolationMethod, Is.EqualTo(SpatialInterpolationMethod.Averaging));
            Assert.That(spatialOperation.AveragingMethod, Is.EqualTo(expAveragingType));
            Assert.That(spatialOperation.RelativeSearchCellSize, Is.EqualTo(1.23));
            Assert.That(spatialOperation.MinSamplePoints, Is.EqualTo(2));
        }

        [Test]
        public void CreateFromInitialField_FromSamplesData_WithTriangulationInterpolation_CreatesCorrectImportSamplesSpatialOperation()
        {
            // Arrange
            var factory = new SpatialOperationFactory();
            InitialField initialField = InitialFieldBuilder.Start().AddRequiredValues().Build();

            // Act
            ISpatialOperation result = factory.CreateFromInitialField(initialField);

            // Assert
            var spatialOperation = result as ImportSamplesSpatialOperation;
            Assert.That(spatialOperation, Is.Not.Null);
            Assert.That(spatialOperation.Name, Is.EqualTo("water_level"));
            Assert.That(spatialOperation.FilePath, Is.EqualTo(initialField.DataFile));
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
        public void CreateFromInitialField_FromPolygonData_CreatesCorrectSetValueOperation(InitialFieldOperand operand,
                                                                                           PointwiseOperationType expOperand)
        {
            using (var temp = new TemporaryDirectory())
            {
                // Arrange
                var factory = new SpatialOperationFactory();
                InitialField initialField = InitialFieldBuilder.Start().AddRequiredValues().AddPolygonDataFileType().Build();
                initialField.Operand = operand;
                initialField.DataFile = temp.CreateFile("water_level.pol", GetPolFileContent());

                // Act
                ISpatialOperation result = factory.CreateFromInitialField(initialField);

                // Assert
                var spatialOperation = result as SetValueOperation;
                Assert.That(spatialOperation, Is.Not.Null);
                Assert.That(spatialOperation.Name, Is.EqualTo("water_level"));
                Assert.That(spatialOperation.Value, Is.EqualTo(7));
                Assert.That(spatialOperation.OperationType, Is.EqualTo(expOperand));
            }
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