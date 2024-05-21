using System;
using System.IO;
using DelftTools.TestUtils;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Files.Helpers;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Files.InitialField;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Files.InitialField.Data;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using NUnit.Framework;
using SharpMap.Api.SpatialOperations;
using SharpMap.SpatialOperations;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.IO.Files.InitialField
{
    [TestFixture]
    public class SpatialDataFileWriterTest
    {
        private WaterFlowFMModelDefinition definition;
        private InitialFieldFileData data;

        [SetUp]
        public void SetUp()
        {
            definition = new WaterFlowFMModelDefinition();
            data = new InitialFieldFileData();
        }

        [Test]
        [TestCase("")]
        [TestCase(" ")]
        [TestCase(null)]
        public void Write_DirectoryIsNullOrEmptyOrWhiteSpace_ThrowsArgumentException(string directory)
        {
            SpatialDataFileWriter writer = CreateWriter();
            
            Assert.Throws<ArgumentException>(() => writer.Write(directory, false, data, definition));
        }

        [Test]
        public void Write_InitialFieldFileDataIsNull_ThrowsArgumentNullException()
        {
            SpatialDataFileWriter writer = CreateWriter();

            Assert.Throws<ArgumentNullException>(() => writer.Write("output", false, null, definition));
        }

        [Test]
        public void Write_ModelDefinitionIsNull_ThrowsArgumentNullException()
        {
            SpatialDataFileWriter writer = CreateWriter();

            Assert.Throws<ArgumentNullException>(() => writer.Write("output", false, data, null));
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void Write_InitialConditionsAndParametersAreEmpty_DoesNotWriteAnyFile()
        {
            SpatialDataFileWriter writer = CreateWriter();

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
            SpatialDataFileWriter writer = CreateWriter();

            ImportSamplesSpatialOperation spatialOperation = CreateImportSamplesSpatialOperation();
            InitialFieldData initialFieldData = CreateImportSamplesInitialFieldData(spatialOperation);

            SetupSpatialData(spatialOperation, initialFieldData);

            using (var tempDir = new TemporaryDirectory())
            {
                string expectedDataFilePath = Path.Combine(tempDir.Path, initialFieldData.DataFile);
                string expectedOperationFilePath = switchTo ? expectedDataFilePath : spatialOperation.FilePath;
                
                writer.Write(tempDir.Path, switchTo, data, definition);

                Assert.That(expectedDataFilePath, Does.Exist);
                Assert.That(spatialOperation.FilePath, Is.EqualTo(expectedOperationFilePath));
            }
        }

        private static SpatialDataFileWriter CreateWriter()
        {
            return new SpatialDataFileWriter();
        }

        private void SetupSpatialData(ISpatialOperation operation, InitialFieldData initialFieldData)
        {
            definition.SpatialOperations.Add(initialFieldData.SpatialOperationQuantity, new[] { operation });
            data.AddInitialCondition(initialFieldData);
        }

        private static ImportSamplesSpatialOperation CreateImportSamplesSpatialOperation()
        {
            return new ImportSamplesSpatialOperation
            {
                Name = "chezy",
                FilePath = TestHelper.GetTestFilePath(@"chezy_samples\chezy.xyz")
            };
        }

        private static InitialFieldData CreateImportSamplesInitialFieldData(ImportSamplesOperation operation)
        {
            return new InitialFieldData
            {
                DataFile = Path.GetFileName(operation.FilePath),
                SpatialOperationName = operation.Name,
                SpatialOperationQuantity = WaterFlowFMModelDefinition.RoughnessDataItemName
            };
        }
    }
}