using System.Collections.Generic;
using System.IO.Abstractions.TestingHelpers;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Files.Helpers;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Files.InitialField;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Files.InitialField.Data;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using NSubstitute;
using NUnit.Framework;
using SharpMap.Api.SpatialOperations;
using SharpMap.SpatialOperations;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.IO.Files.InitialField
{
    [TestFixture]
    public class InitialFieldFileWriterTest
    {
        private MockFileSystem fileSystem;
        private InitialFieldFileContext context;
        private ISpatialDataFileWriter spatialDataWriter;
        
        [SetUp]
        public void SetUp()
        {
            fileSystem = new MockFileSystem();
            context = new InitialFieldFileContext();
            spatialDataWriter = Substitute.For<ISpatialDataFileWriter>();
        }
        
        [Test]
        public void Constructor_InitialFieldFileContextNull_ThrowsArgumentNullException()
        {
            // Act
            void Call() => _ = new InitialFieldFileWriter(null, spatialDataWriter, fileSystem);

            // Assert
            Assert.That(Call, Throws.ArgumentNullException);
        }

        [Test]
        public void Constructor_SpatialDataFileWriterNull_ThrowsArgumentNullException()
        {
            // Act
            void Call() => _ = new InitialFieldFileWriter(context, null, fileSystem);

            // Assert
            Assert.That(Call, Throws.ArgumentNullException);
        }
        
        [Test]
        public void Constructor_FileSystemNull_ThrowsArgumentNullException()
        {
            // Act
            void Call() => _ = new InitialFieldFileWriter(context, spatialDataWriter, null);

            // Assert
            Assert.That(Call, Throws.ArgumentNullException);
        }

        [Test]
        public void ShouldWrite_ModelDefinitionNull_ThrowsArgumentNullException()
        {
            // Arrange
            InitialFieldFileWriter writer = CreateWriter();

            // Act
            void Call() => writer.ShouldWrite(null);

            // Assert
            Assert.That(Call, Throws.ArgumentNullException);
        }

        [Test]
        public void ShouldWrite_ModelDefinitionDoesNotContainRelevantSpatialOperation_ReturnsFalse()
        {
            // Arrange
            InitialFieldFileWriter writer = CreateWriter();
            var modelDefinition = new WaterFlowFMModelDefinition();

            // Act
            bool result = writer.ShouldWrite(modelDefinition);

            // Assert
            Assert.That(result, Is.False);
        }

        [Test]
        [TestCase(WaterFlowFMModelDefinition.InitialWaterLevelDataItemName)]
        [TestCase(WaterFlowFMModelDefinition.RoughnessDataItemName)]
        public void ShouldWrite_ModelDefinitionContainsRelevantSpatialOperation_ReturnsTrue(string spatialOperationName)
        {
            // Arrange
            InitialFieldFileWriter writer = CreateWriter();
            var modelDefinition = new WaterFlowFMModelDefinition();

            modelDefinition.SpatialOperations[spatialOperationName] =
                new List<ISpatialOperation> { CreateSetValueOperation() };

            // Act
            bool result = writer.ShouldWrite(modelDefinition);

            // Assert
            Assert.That(result, Is.True);
        }

        [Test]
        [TestCase(null)]
        [TestCase("")]
        [TestCase(" ")]
        public void Write_FilePathNullOrEmpty_ThrowsArgumentException(string filePath)
        {
            // Arrange
            InitialFieldFileWriter writer = CreateWriter();

            // Act
            void Call() => writer.Write(filePath, "initialFields.ini", false, new WaterFlowFMModelDefinition());

            // Assert
            Assert.That(Call, Throws.ArgumentException);
        }
        
        [Test]
        [TestCase(null)]
        [TestCase("")]
        [TestCase(" ")]
        public void Write_ParentPathNullOrEmpty_ThrowsArgumentException(string filePath)
        {
            // Arrange
            InitialFieldFileWriter writer = CreateWriter();

            // Act
            void Call() => writer.Write("initialFields.ini", filePath, false, new WaterFlowFMModelDefinition());

            // Assert
            Assert.That(Call, Throws.ArgumentException);
        }

        [Test]
        public void Write_ModelDefinitionNull_ThrowsArgumentNullException()
        {
            // Arrange
            InitialFieldFileWriter writer = CreateWriter();

            const string fileName = "initialFields.ini";
            
            // Act
            void Call() => writer.Write(fileName, fileName, false, null);

            // Assert
            Assert.That(Call, Throws.ArgumentNullException);
        }

        [Test]
        public void Write_ModelDefinitionWithSeveralSpatialOperations_WritesSpatialData()
        {
            // Arrange
            InitialFieldFileWriter writer = CreateWriter();
            WaterFlowFMModelDefinition modelDefinition = SetupModelDefinition();

            const string fileName = "data/initialFields.ini";

            // Act
            writer.Write(fileName, fileName, false, modelDefinition);

            // Assert
            spatialDataWriter.Received(1).Write("data", false, Arg.Any<InitialFieldFileData>(), modelDefinition);
        }

        [Test]
        public void Write_ModelDefinitionWithSeveralSpatialOperations_WritesInitialFieldFile()
        {
            // Arrange
            InitialFieldFileWriter writer = CreateWriter();
            WaterFlowFMModelDefinition modelDefinition = SetupModelDefinition();

            const string fileName = "initialFields.ini";

            // Act
            writer.Write(fileName, fileName, false, modelDefinition);

            // Assert
            const string fileContentExpected = @"[General]
    fileVersion           = 2.00                
    fileType              = iniField            

[Initial]
    quantity              = waterlevel          
    dataFile              = waterlevel.xyz      
    dataFileType          = sample              
    interpolationMethod   = averaging           
    operand               = O                   
    averagingType         = nearestNb           
    averagingRelSize      = 1.0000000e+000      
    averagingNumMin       = 1                   
    averagingPercentile   = 0.0000000e+000      
    extrapolationMethod   = no                  
    locationType          = 2d                  

[Initial]
    quantity              = waterlevel          
    dataFile              = waterlevel_set_value.pol
    dataFileType          = polygon             
    interpolationMethod   = constant            
    operand               = *                   
    extrapolationMethod   = no                  
    locationType          = 2d                  
    value                 = 1.2300000e+000      

[Parameter]
    quantity              = frictioncoefficient 
    dataFile              = samples.xyz         
    dataFileType          = sample              
    interpolationMethod   = triangulation       
    operand               = O                   
    extrapolationMethod   = no                  
    locationType          = 2d                  

";

            MockFileData fileData = fileSystem.GetFile(fileName);
            
            Assert.That(fileData.TextContents, Is.EqualTo(fileContentExpected));
        }

        [Test]
        public void Write_WithOriginalDataFileNamesStored_WritesInitialFieldFileWithOriginalDataFiles()
        {
            // Arrange
            InitialFieldFileWriter writer = CreateWriter();
            WaterFlowFMModelDefinition modelDefinition = SetupModelDefinition();
            
            const string fileName = "initialFields.ini";

            context.StoreDataFileName(new InitialFieldData
            {
                DataFile = "original_add_samples.xyz",
                SpatialOperationName = "add_samples",
                SpatialOperationQuantity = WaterFlowFMModelDefinition.InitialWaterLevelDataItemName
            });
            context.StoreDataFileName(new InitialFieldData
            {
                DataFile = "original_set_value.pol",
                SpatialOperationName = "set_value",
                SpatialOperationQuantity = WaterFlowFMModelDefinition.InitialWaterLevelDataItemName
            });
            context.StoreDataFileName(new InitialFieldData
            {
                DataFile = "original_import_samples.xyz",
                SpatialOperationName = "import_samples",
                SpatialOperationQuantity = WaterFlowFMModelDefinition.RoughnessDataItemName
            });
            
            // Act
            writer.Write(fileName, fileName, false, modelDefinition);

            // Assert
            const string fileContentExpected = @"[General]
    fileVersion           = 2.00                
    fileType              = iniField            

[Initial]
    quantity              = waterlevel          
    dataFile              = original_add_samples.xyz
    dataFileType          = sample              
    interpolationMethod   = averaging           
    operand               = O                   
    averagingType         = nearestNb           
    averagingRelSize      = 1.0000000e+000      
    averagingNumMin       = 1                   
    averagingPercentile   = 0.0000000e+000      
    extrapolationMethod   = no                  
    locationType          = 2d                  

[Initial]
    quantity              = waterlevel          
    dataFile              = original_set_value.pol
    dataFileType          = polygon             
    interpolationMethod   = constant            
    operand               = *                   
    extrapolationMethod   = no                  
    locationType          = 2d                  
    value                 = 1.2300000e+000      

[Parameter]
    quantity              = frictioncoefficient 
    dataFile              = original_import_samples.xyz
    dataFileType          = sample              
    interpolationMethod   = triangulation       
    operand               = O                   
    extrapolationMethod   = no                  
    locationType          = 2d                  

";

            MockFileData fileData = fileSystem.GetFile(fileName);
            
            Assert.That(fileData.TextContents, Is.EqualTo(fileContentExpected));
        }

        private InitialFieldFileWriter CreateWriter()
        {
            return new InitialFieldFileWriter(context, spatialDataWriter, fileSystem);
        }

        private static WaterFlowFMModelDefinition SetupModelDefinition()
        {
            return new WaterFlowFMModelDefinition
            {
                SpatialOperations =
                {
                    [WaterFlowFMModelDefinition.InitialWaterLevelDataItemName] =
                        new List<ISpatialOperation>
                        {
                            CreateAddSamplesOperation(),
                            CreateSetValueOperation()
                        },
                    [WaterFlowFMModelDefinition.RoughnessDataItemName] =
                        new List<ISpatialOperation> { CreateImportSamplesSpatialOperation() }
                }
            };
        }

        private static ImportSamplesSpatialOperation CreateImportSamplesSpatialOperation()
        {
            return new ImportSamplesSpatialOperation
            {
                Name = "import_samples",
                FilePath = "samples.xyz",
                InterpolationMethod = SpatialInterpolationMethod.Triangulation,
                Operand = PointwiseOperationType.Overwrite
            };
        }

        private static SetValueOperation CreateSetValueOperation()
        {
            return new SetValueOperation
            {
                Name = "set_value",
                Value = 1.23,
                OperationType = PointwiseOperationType.Multiply
            };
        }

        private static AddSamplesOperation CreateAddSamplesOperation()
        {
            return new AddSamplesOperation(true) { Name = "add_samples" };
        }
    }
}