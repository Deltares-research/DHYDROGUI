using System.Collections.Generic;
using System.IO.Abstractions.TestingHelpers;
using DelftTools.Hydro;
using DeltaShell.Plugins.FMSuite.FlowFM.IO;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.InitialField;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using DHYDRO.Common.IO.InitialField;
using NSubstitute;
using NUnit.Framework;
using SharpMap.Api.SpatialOperations;
using SharpMap.SpatialOperations;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.IO.InitialField
{
    [TestFixture]
    public class InitialFieldFileWriterTest
    {
        private MockFileSystem fileSystem;
        private InitialFieldFileContext context;
        private ISpatialDataFileWriter spatialDataWriter;
        private IHydroNetwork hydroNetwork;
        
        [SetUp]
        public void SetUp()
        {
            fileSystem = new MockFileSystem();
            context = new InitialFieldFileContext();
            spatialDataWriter = Substitute.For<ISpatialDataFileWriter>();
            hydroNetwork = Substitute.For<IHydroNetwork>();
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
            void Call() => writer.ShouldWrite(null, hydroNetwork);

            // Assert
            Assert.That(Call, Throws.ArgumentNullException);
        }
        
        [Test]
        public void ShouldWrite_HydroNetworkNull_ThrowsArgumentNullException()
        {
            // Arrange
            InitialFieldFileWriter writer = CreateWriter();
            WaterFlowFMModelDefinition modelDefinition = CreateModelDefinition();

            // Act
            void Call() => writer.ShouldWrite(modelDefinition, null);

            // Assert
            Assert.That(Call, Throws.ArgumentNullException);
        }

        [Test]
        public void ShouldWrite_ModelDefinitionDoesNotContainRelevantSpatialOperation_ReturnsFalse()
        {
            // Arrange
            InitialFieldFileWriter writer = CreateWriter();
            WaterFlowFMModelDefinition modelDefinition = CreateModelDefinition();

            SetupHydroNetworkIsEmpty(true);

            // Act
            bool result = writer.ShouldWrite(modelDefinition, hydroNetwork);

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
            WaterFlowFMModelDefinition modelDefinition = CreateModelDefinitionWithOperations();
            
            SetupHydroNetworkIsEmpty(true);

            // Act
            bool result = writer.ShouldWrite(modelDefinition, hydroNetwork);

            // Assert
            Assert.That(result, Is.True);
        }
        
        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void ShouldWrite_HydroNetworkIsEmpty_ReturnsExpected(bool isEmpty)
        {
            // Arrange
            InitialFieldFileWriter writer = CreateWriter();
            WaterFlowFMModelDefinition modelDefinition = CreateModelDefinition();

            SetupHydroNetworkIsEmpty(isEmpty);

            // Act
            bool result = writer.ShouldWrite(modelDefinition, hydroNetwork);

            // Assert
            Assert.That(result, Is.EqualTo(!isEmpty));
        }

        [Test]
        [TestCase(null)]
        [TestCase("")]
        [TestCase(" ")]
        public void Write_FilePathNullOrEmpty_ThrowsArgumentException(string filePath)
        {
            // Arrange
            InitialFieldFileWriter writer = CreateWriter();
            WaterFlowFMModelDefinition modelDefinition = CreateModelDefinition();

            // Act
            void Call() => writer.Write(filePath, "initialFields.ini", false, modelDefinition);

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
            WaterFlowFMModelDefinition modelDefinition = CreateModelDefinition();

            // Act
            void Call() => writer.Write("initialFields.ini", filePath, false, modelDefinition);

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
            WaterFlowFMModelDefinition modelDefinition = CreateModelDefinitionWithOperations();

            const string fileName = "data/initialFields.ini";

            // Act
            writer.Write(fileName, fileName, false, modelDefinition);

            // Assert
            spatialDataWriter.Received(1).Write("data", false, Arg.Any<InitialFieldFileData>(), modelDefinition);
        }

        [Test]
        public void Write_ModelDefinitionWithSeveralSpatialOperations_WritesInitialFieldFile(
            [Values] InitialConditionQuantity globalQuantity)
        {
            // Arrange
            InitialFieldFileWriter writer = CreateWriter();
            WaterFlowFMModelDefinition modelDefinition = CreateModelDefinitionWithOperations(globalQuantity);

            string globalQuantityName = globalQuantity.ToString();
            const string fileName = "initialFields.ini";

            // Act
            writer.Write(fileName, fileName, false, modelDefinition);

            // Assert
            var fileContentExpected = $@"[General]
    fileVersion           = 2.00                
    fileType              = iniField            

[Initial]
    quantity              = {globalQuantityName}          
    dataFile              = Initial{globalQuantityName}.ini
    dataFileType          = 1dField             

[Initial]
    quantity              = {globalQuantityName}          
    dataFile              = {globalQuantityName}_set_value.pol
    dataFileType          = polygon             
    interpolationMethod   = constant            
    operand               = *                   
    extrapolationMethod   = no                  
    locationType          = 2d                  
    value                 = 1.2300000e+000      

[Initial]
    quantity              = InfiltrationCapacity
    dataFile              = InfiltrationCapacity.xyz
    dataFileType          = sample              
    interpolationMethod   = averaging           
    operand               = O                   
    averagingType         = nearestNb           
    averagingRelSize      = 1.0000000e+000      
    averagingNumMin       = 1                   
    averagingPercentile   = 0.0000000e+000      
    extrapolationMethod   = no                  
    locationType          = 2d                  

[Parameter]
    quantity              = frictioncoefficient 
    dataFile              = samples.xyz         
    dataFileType          = sample              
    interpolationMethod   = triangulation       
    operand               = O                   
    extrapolationMethod   = no                  
    locationType          = 2d                  
    ifrctyp               = 1                   

";

            MockFileData fileData = fileSystem.GetFile(fileName);
            
            Assert.That(fileData.TextContents, Is.EqualTo(fileContentExpected).IgnoreCase);
        }

        [Test]
        public void Write_WithOriginalDataFileNamesStored_WritesInitialFieldFileWithOriginalDataFiles()
        {
            // Arrange
            InitialFieldFileWriter writer = CreateWriter();
            WaterFlowFMModelDefinition modelDefinition = CreateModelDefinitionWithOperations();
            
            const string fileName = "initialFields.ini";
            
            context.StoreDataFileName(new InitialFieldData
            {
                DataFile = "original_set_value.pol",
                SpatialOperationName = "set_value",
                SpatialOperationQuantity = WaterFlowFMModelDefinition.InitialWaterLevelDataItemName
            });
            context.StoreDataFileName(new InitialFieldData
            {
                DataFile = "original_add_samples.xyz",
                SpatialOperationName = "add_samples",
                SpatialOperationQuantity = WaterFlowFMModelDefinition.InfiltrationDataItemName
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
    dataFile              = InitialWaterLevel.ini
    dataFileType          = 1dField             

[Initial]
    quantity              = waterlevel          
    dataFile              = original_set_value.pol
    dataFileType          = polygon             
    interpolationMethod   = constant            
    operand               = *                   
    extrapolationMethod   = no                  
    locationType          = 2d                  
    value                 = 1.2300000e+000      

[Initial]
    quantity              = InfiltrationCapacity
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

[Parameter]
    quantity              = frictioncoefficient 
    dataFile              = original_import_samples.xyz
    dataFileType          = sample              
    interpolationMethod   = triangulation       
    operand               = O                   
    extrapolationMethod   = no                  
    locationType          = 2d                  
    ifrctyp               = 1                   

";

            MockFileData fileData = fileSystem.GetFile(fileName);
            
            Assert.That(fileData.TextContents, Is.EqualTo(fileContentExpected).IgnoreCase);
        }

        private InitialFieldFileWriter CreateWriter()
        {
            return new InitialFieldFileWriter(context, spatialDataWriter, fileSystem);
        }
        
        private static WaterFlowFMModelDefinition CreateModelDefinition()
        {
            return new WaterFlowFMModelDefinition();
        }
        
        private static WaterFlowFMModelDefinition CreateModelDefinitionWithOperations(
            InitialConditionQuantity globalQuantity = InitialConditionQuantity.WaterLevel)
        {
            var modelDefinition = new WaterFlowFMModelDefinition
            {
                SpatialOperations =
                {
                    [WaterFlowFMModelDefinition.InitialWaterLevelDataItemName] =
                        new List<ISpatialOperation> { CreateSetValueOperation() },
                    [WaterFlowFMModelDefinition.InitialWaterDepthDataItemName] =
                        new List<ISpatialOperation> { CreateSetValueOperation() },
                    [WaterFlowFMModelDefinition.InfiltrationDataItemName] =
                        new List<ISpatialOperation> { CreateAddSamplesOperation() },
                    [WaterFlowFMModelDefinition.RoughnessDataItemName] =
                        new List<ISpatialOperation> { CreateImportSamplesSpatialOperation() }
                }
            };
            
            modelDefinition.SetModelProperty(GuiProperties.InitialConditionGlobalQuantity1D, ((int) globalQuantity).ToString());
            modelDefinition.SetModelProperty(GuiProperties.InitialConditionGlobalQuantity2D, ((int) globalQuantity).ToString());

            return modelDefinition;
        }

        private static ImportSamplesOperationImportData CreateImportSamplesSpatialOperation()
        {
            return new ImportSamplesOperationImportData
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

        private void SetupHydroNetworkIsEmpty(bool isEmpty)
        {
            hydroNetwork.IsVerticesEmpty.Returns(isEmpty);
            hydroNetwork.IsEdgesEmpty.Returns(isEmpty);
        }
    }
}