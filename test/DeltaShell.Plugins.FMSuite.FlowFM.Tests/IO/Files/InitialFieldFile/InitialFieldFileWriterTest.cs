using System.Collections.Generic;
using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Files.Helpers;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Files.InitialFieldFile;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Files.InitialFieldFile.Data;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Files.InitialFieldFile.Serialization;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using DHYDRO.Common.IO.Ini;
using NSubstitute;
using NUnit.Framework;
using SharpMap.Api.SpatialOperations;
using SharpMap.SpatialOperations;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.IO.Files.InitialFieldFile
{
    [TestFixture]
    public class InitialFieldFileWriterTest
    {
        [Test]
        public void Constructor_FileSystemNull_ThrowsArgumentNullException()
        {
            // Arrange
            var spatialDataFileWriter = Substitute.For<ISpatialDataFileWriter>();

            // Act
            void Call()
            {
                _ = new InitialFieldFileWriter(null, spatialDataFileWriter);
            }

            // Assert
            Assert.That(Call, Throws.ArgumentNullException);
        }

        [Test]
        public void Constructor_SpatialDataFileWriterNull_ThrowsArgumentNullException()
        {
            // Arrange
            var fileSystem = new MockFileSystem();

            // Act
            void Call()
            {
                _ = new InitialFieldFileWriter(fileSystem, null);
            }

            // Assert
            Assert.That(Call, Throws.ArgumentNullException);
        }

        [Test]
        public void ShouldWrite_ModelDefinitionNull_ThrowsArgumentNullException()
        {
            // Arrange
            var fileSystem = new MockFileSystem();
            var spatialDataFileWriter = Substitute.For<ISpatialDataFileWriter>();
            var writer = new InitialFieldFileWriter(fileSystem, spatialDataFileWriter);

            // Act
            void Call()
            {
                writer.ShouldWrite(null);
            }

            // Assert
            Assert.That(Call, Throws.ArgumentNullException);
        }

        [Test]
        public void ShouldWrite_ModelDefinitionDoesNotContainRelevantSpatialOperation_ReturnsFalse()
        {
            // Arrange
            var fileSystem = new MockFileSystem();
            var spatialDataFileWriter = Substitute.For<ISpatialDataFileWriter>();
            var writer = new InitialFieldFileWriter(fileSystem, spatialDataFileWriter);
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
            var fileSystem = new MockFileSystem();
            var spatialDataFileWriter = Substitute.For<ISpatialDataFileWriter>();
            var writer = new InitialFieldFileWriter(fileSystem, spatialDataFileWriter);
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
            var fileSystem = new MockFileSystem();
            var spatialDataFileWriter = Substitute.For<ISpatialDataFileWriter>();
            var writer = new InitialFieldFileWriter(fileSystem, spatialDataFileWriter);

            // Act
            void Call()
            {
                writer.Write(filePath, new WaterFlowFMModelDefinition());
            }

            // Assert
            Assert.That(Call, Throws.ArgumentException);
        }

        [Test]
        public void Write_ModelDefinitionNull_ThrowsArgumentNullException()
        {
            // Arrange
            var fileSystem = new MockFileSystem();
            var spatialDataFileWriter = Substitute.For<ISpatialDataFileWriter>();
            var writer = new InitialFieldFileWriter(fileSystem, spatialDataFileWriter);

            // Act
            void Call()
            {
                writer.Write("initialFields.ini", null);
            }

            // Assert
            Assert.That(Call, Throws.ArgumentNullException);
        }

        [Test]
        public void Write_ModelDefinitionWithSeveralSpatialOperations_WritesCorrectFile()
        {
            // Arrange
            var fileSystem = new MockFileSystem();
            fileSystem.AddDirectory("input");
            var spatialDataFileWriter = Substitute.For<ISpatialDataFileWriter>();
            var writer = new InitialFieldFileWriter(fileSystem, spatialDataFileWriter);
            var modelDefinition = new WaterFlowFMModelDefinition
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

            const string fileName = "input/initialFields.ini";

            // Act
            writer.Write(fileName, modelDefinition);

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
    dataFile              = waterlevel_some_operation.pol
    dataFileType          = polygon             
    interpolationMethod   = constant            
    operand               = *                   
    extrapolationMethod   = no                  
    locationType          = 2d                
    value                 = 1.2300000e+000      

[Parameter]
    quantity              = frictioncoefficient 
    dataFile              = some_operation.xyz  
    dataFileType          = sample              
    interpolationMethod   = triangulation       
    operand               = O                   
    extrapolationMethod   = no                  
    locationType          = 2d";

            AssertEqualIniData(fileSystem, fileName, fileContentExpected);
            spatialDataFileWriter.Received(1).Write("input", Arg.Any<InitialFieldFileData>(), modelDefinition);
        }

        private static void AssertEqualIniData(IFileSystem fileSystem, string fileName, string fileContentExpected)
        {
            string fileContentActual = fileSystem.File.ReadAllText(fileName);
            var iniParser = new IniParser();
            IniData iniActual = iniParser.Parse(fileContentActual);
            IniData iniExpected = iniParser.Parse(fileContentExpected);

            Assert.That(iniActual.Equals(iniExpected));
        }

        private static ImportSamplesSpatialOperation CreateImportSamplesSpatialOperation()
        {
            return new ImportSamplesSpatialOperation
            {
                FilePath = "some_operation.xyz",
                InterpolationMethod = SpatialInterpolationMethod.Triangulation,
                Operand = PointwiseOperationType.Overwrite
            };
        }

        private static SetValueOperation CreateSetValueOperation()
        {
            return new SetValueOperation
            {
                Name = "some_operation",
                Value = 1.23,
                OperationType = PointwiseOperationType.Multiply
            };
        }

        private static AddSamplesOperation CreateAddSamplesOperation()
        {
            return new AddSamplesOperation(true);
        }
    }
}