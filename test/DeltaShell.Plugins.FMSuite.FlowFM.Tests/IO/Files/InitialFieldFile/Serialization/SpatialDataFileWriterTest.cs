using System;
using System.IO;
using DelftTools.TestUtils;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Files.Helpers;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Files.InitialFieldFile.Data;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Files.InitialFieldFile.Serialization;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using NUnit.Framework;
using SharpMap.Api.SpatialOperations;
using SharpMap.SpatialOperations;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.IO.Files.InitialFieldFile.Serialization
{
    [TestFixture]
    public class SpatialDataFileWriterTest
    {
        private SpatialDataFileWriter writer;
        private WaterFlowFMModelDefinition definition;
        private InitialFieldFileData data;

        [SetUp]
        public void SetUp()
        {
            writer = new SpatialDataFileWriter();
            definition = new WaterFlowFMModelDefinition();
            data = new InitialFieldFileData();
        }

        [Test]
        [TestCase("")]
        [TestCase(" ")]
        [TestCase(null)]
        public void Write_DirectoryIsNullOrEmptyOrWhiteSpace_ThrowsArgumentException(string directory)
        {
            Assert.Throws<ArgumentException>(() => writer.Write(directory, false, data, definition));
        }

        [Test]
        public void Write_InitialFieldFileDataIsNull_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => writer.Write("output", false, null, definition));
        }

        [Test]
        public void Write_ModelDefinitionIsNull_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => writer.Write("output", false, data, null));
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void Write_InitialConditionsAndParametersAreEmpty_DoesNotWriteAnyFile()
        {
            using (var tempDir = new TemporaryDirectory())
            {
                writer.Write(tempDir.Path, false, data, definition);

                Assert.That(new DirectoryInfo(tempDir.Path), Is.Empty);
            }
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        [Category(TestCategory.DataAccess)]
        public void Write_ImportSamplesSpatialOperation_FileExists(bool switchTo)
        {
            ImportSamplesSpatialOperation spatialOperation = CreateImportSamplesSpatialOperation();
            InitialField initialField = CreateImportSamplesInitialField(spatialOperation);

            SetupSpatialData(spatialOperation, initialField);

            using (var tempDir = new TemporaryDirectory())
            {
                string expectedDataFilePath = Path.Combine(tempDir.Path, initialField.DataFile);
                string expectedOperationFilePath = switchTo ? expectedDataFilePath : spatialOperation.FilePath;
                
                writer.Write(tempDir.Path, switchTo, data, definition);

                Assert.That(expectedDataFilePath, Does.Exist);
                Assert.That(spatialOperation.FilePath, Is.EqualTo(expectedOperationFilePath));
            }
        }

        private void SetupSpatialData(ISpatialOperation operation, InitialField initialField)
        {
            definition.SpatialOperations.Add(initialField.SpatialOperationQuantity, new[] { operation });
            data.AddInitialCondition(initialField);
        }

        private static ImportSamplesSpatialOperation CreateImportSamplesSpatialOperation()
        {
            return new ImportSamplesSpatialOperation
            {
                Name = "chezy",
                FilePath = TestHelper.GetTestFilePath(@"chezy_samples\chezy.xyz")
            };
        }

        private static InitialField CreateImportSamplesInitialField(ImportSamplesOperation operation)
        {
            return new InitialField
            {
                DataFile = Path.GetFileName(operation.FilePath),
                SpatialOperationName = operation.Name,
                SpatialOperationQuantity = WaterFlowFMModelDefinition.RoughnessDataItemName
            };
        }
    }
}