using System.Collections.Generic;
using System.IO.Abstractions.TestingHelpers;
using System.Linq;
using DelftTools.TestUtils;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Files.InitialFieldFile;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using log4net.Core;
using NUnit.Framework;
using SharpMap.Api.SpatialOperations;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.IO.Files.InitialFieldFile
{
    [TestFixture]
    public class InitialFieldFileReaderTest
    {
        [Test]
        public void Constructor_FileSystemNull_ThrowsArgumentNullException()
        {
            // Act
            void Call()
            {
                _ = new InitialFieldFileReader(null);
            }

            // Assert
            Assert.That(Call, Throws.ArgumentNullException);
        }

        [Test]
        public void Read_ModelDefinitionNull_ThrowsArgumentNullException()
        {
            // Arrange
            var fileSystem = new MockFileSystem();
            var reader = new InitialFieldFileReader(fileSystem);
            const string fileName = "initialFields.ini";

            // Act
            void Call()
            {
                reader.Read(fileName, fileName, null);
            }

            // Assert
            Assert.That(Call, Throws.ArgumentNullException);
        }

        [Test]
        [TestCase(null)]
        [TestCase("")]
        [TestCase(" ")]
        public void Read_FilePathNullOrEmpty_ThrowsArgumentException(string filePath)
        {
            // Arrange
            var fileSystem = new MockFileSystem();
            var reader = new InitialFieldFileReader(fileSystem);

            // Act
            void Call()
            {
                reader.Read(filePath, "initialFields.ini", new WaterFlowFMModelDefinition());
            }

            // Assert
            Assert.That(Call, Throws.ArgumentException);
        }

        [Test]
        [TestCase(null)]
        [TestCase("")]
        [TestCase(" ")]
        public void Read_RelativeParentPathNullOrEmpty_ThrowsArgumentException(string filePath)
        {
            // Arrange
            var fileSystem = new MockFileSystem();
            var reader = new InitialFieldFileReader(fileSystem);

            // Act
            void Call()
            {
                reader.Read("initialFields.ini", filePath, new WaterFlowFMModelDefinition());
            }

            // Assert
            Assert.That(Call, Throws.ArgumentException);
        }

        [Test]
        public void Read_FileDoesNotExist_LogsError()
        {
            // Arrange
            var fileSystem = new MockFileSystem();
            var reader = new InitialFieldFileReader(fileSystem);
            const string filePath = "initialFields.ini";

            // Act
            void Call()
            {
                reader.Read(filePath, filePath, new WaterFlowFMModelDefinition());
            }

            // Assert
            string error = TestHelper.GetAllRenderedMessages(Call, Level.Error).Single();
            Assert.That(error, Is.EqualTo($"Initial field file does not exist: {filePath}"));
        }

        [Test]
        public void Read_FromInitialFieldFile_AddsDataToModelDefinition()
        {
            // Arrange
            var fileSystem = new MockFileSystem();
            var reader = new InitialFieldFileReader(fileSystem);

            const string fileName = "initialFields.ini";
            fileSystem.AddFile(fileName, new MockFileData(GetIniData()));
            fileSystem.AddEmptyFile("bedlevel.xyz");
            fileSystem.AddEmptyFile("initialwaterlevel_samples.xyz");
            fileSystem.AddEmptyFile("frictioncoefficient_samples.xyz");
            fileSystem.AddEmptyFile("frictioncoefficient_samples_(1).xyz");

            var modelDefinition = new WaterFlowFMModelDefinition();

            // Act
            reader.Read(fileName, fileName, modelDefinition);

            // Assert
            IList<ISpatialOperation> bathymetry = modelDefinition.SpatialOperations[WaterFlowFMModelDefinition.BathymetryDataItemName];
            Assert.That(bathymetry, Has.Count.EqualTo(1));
            IList<ISpatialOperation> waterLevel = modelDefinition.SpatialOperations[WaterFlowFMModelDefinition.InitialWaterLevelDataItemName];
            Assert.That(waterLevel, Has.Count.EqualTo(1));
            IList<ISpatialOperation> frictionCoefficient = modelDefinition.SpatialOperations[WaterFlowFMModelDefinition.RoughnessDataItemName];
            Assert.That(frictionCoefficient, Has.Count.EqualTo(2));
        }

        [Test]
        public void Read_FromInitialFieldFile_WithRelativeDataFilePaths_AddsDataToModelDefinition()
        {
            // Arrange
            var fileSystem = new MockFileSystem();
            var reader = new InitialFieldFileReader(fileSystem);

            const string fileName = "initialFields.ini";
            fileSystem.AddFile(fileName, new MockFileData(GetIniData("data/")));
            fileSystem.AddEmptyFile("data/bedlevel.xyz");
            fileSystem.AddEmptyFile("data/initialwaterlevel_samples.xyz");
            fileSystem.AddEmptyFile("data/frictioncoefficient_samples.xyz");
            fileSystem.AddEmptyFile("data/frictioncoefficient_samples_(1).xyz");

            var modelDefinition = new WaterFlowFMModelDefinition();

            // Act
            reader.Read(fileName, fileName, modelDefinition);

            // Assert
            IList<ISpatialOperation> bathymetry = modelDefinition.SpatialOperations[WaterFlowFMModelDefinition.BathymetryDataItemName];
            Assert.That(bathymetry, Has.Count.EqualTo(1));
            IList<ISpatialOperation> waterLevel = modelDefinition.SpatialOperations[WaterFlowFMModelDefinition.InitialWaterLevelDataItemName];
            Assert.That(waterLevel, Has.Count.EqualTo(1));
            IList<ISpatialOperation> frictionCoefficient = modelDefinition.SpatialOperations[WaterFlowFMModelDefinition.RoughnessDataItemName];
            Assert.That(frictionCoefficient, Has.Count.EqualTo(2));
        }

        [Test]
        public void Read_FromInitialFieldFile_WithWrongFileReference_LogsWarning()
        {
            // Arrange
            var fileSystem = new MockFileSystem();
            var reader = new InitialFieldFileReader(fileSystem);

            const string fileName = "initialFields.ini";
            fileSystem.AddFile(fileName, new MockFileData(GetIniData()));

            // Add files, except the bedlevel.xyz
            fileSystem.AddEmptyFile("initialwaterlevel_samples.xyz");
            fileSystem.AddEmptyFile("frictioncoefficient_samples.xyz");
            fileSystem.AddEmptyFile("frictioncoefficient_samples_(1).xyz");

            var modelDefinition = new WaterFlowFMModelDefinition();

            // Act
            void Call()
            {
                reader.Read(fileName, fileName, modelDefinition);
            }

            // Assert
            string error = TestHelper.GetAllRenderedMessages(Call, Level.Error).Single();
            Assert.That(error, Does.Contain("Initial field data file does not exist: bedlevel.xyz"));
            Assert.That(modelDefinition.SpatialOperations, Does.Not.ContainKey(WaterFlowFMModelDefinition.BathymetryDataItemName));
        }

        [Test]
        public void Read_FromEmptyInitialFieldFile_AddsDataToModelDefinition()
        {
            // Arrange
            var fileSystem = new MockFileSystem();
            var reader = new InitialFieldFileReader(fileSystem);

            const string fileName = "initialFields.ini";
            fileSystem.AddFile(fileName, new MockFileData(GetEmptyIniData()));

            var modelDefinition = new WaterFlowFMModelDefinition();

            // Act
            reader.Read(fileName, fileName, modelDefinition);

            // Assert
            Assert.That(modelDefinition.SpatialOperations, Does.Not.ContainKey(WaterFlowFMModelDefinition.BathymetryDataItemName));
            Assert.That(modelDefinition.SpatialOperations, Does.Not.ContainKey(WaterFlowFMModelDefinition.InitialWaterLevelDataItemName));
            Assert.That(modelDefinition.SpatialOperations, Does.Not.ContainKey(WaterFlowFMModelDefinition.RoughnessDataItemName));
        }

        private static string GetIniData(string folder = "")
        {
            return $@"
[General]
    fileVersion           = 2.00                
    fileType              = initialField            

[Initial]
    quantity              = bedlevel            
    dataFile              = {folder}bedlevel.xyz        
    dataFileType          = sample              
    interpolationMethod   = averaging           
    operand               = O                   
    averagingType         = nearestNb           
    averagingRelSize      = 1.0000000e+000      
    averagingNumMin       = 1                   
    locationType          = 2d                  

[Initial]
    quantity              = waterlevel          
    dataFile              = {folder}initialwaterlevel_samples.xyz
    dataFileType          = sample              
    interpolationMethod   = averaging           
    operand               = O                   
    averagingType         = nearestNb           
    averagingRelSize      = 1.0100000e+000      
    averagingNumMin       = 1                   
    locationType          = 2d                       

[Parameter]
    quantity              = frictioncoefficient 
    dataFile              = {folder}frictioncoefficient_samples.xyz
    dataFileType          = sample              
    interpolationMethod   = averaging           
    operand               = O                   
    averagingType         = nearestNb           
    averagingRelSize      = 1.0100000e+000      
    averagingNumMin       = 1                   
    locationType          = 2d   

[Parameter]
    quantity              = frictioncoefficient 
    dataFile              = {folder}frictioncoefficient_samples_(1).xyz
    dataFileType          = sample              
    interpolationMethod   = triangulation           
    operand               = O                  
";
        }

        private static string GetEmptyIniData()
        {
            return @"
[General]
    fileVersion           = 2.00                
    fileType              = initialField                
";
        }
    }
}