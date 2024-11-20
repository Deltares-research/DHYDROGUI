using System.Collections.Generic;
using System.IO.Abstractions.TestingHelpers;
using System.Linq;
using DelftTools.TestUtils;
using DeltaShell.NGHS.TestUtils;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Files.InitialField;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using DHYDRO.Common.IO.InitialField;
using log4net.Core;
using NUnit.Framework;
using SharpMap.Api.SpatialOperations;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.IO.Files.InitialField
{
    [TestFixture]
    public class InitialFieldFileReaderTest
    {
        private MockFileSystem fileSystem;
        private InitialFieldFileContext context;

        [SetUp]
        public void SetUp()
        {
            fileSystem = new MockFileSystem();
            context = new InitialFieldFileContext();
        }
        
        [Test]
        public void Constructor_InitialFieldFileContextNull_ThrowsArgumentNullException()
        {
            // Act
            void Call() => _ = new InitialFieldFileReader(null, fileSystem);

            // Assert
            Assert.That(Call, Throws.ArgumentNullException);
        }
        
        [Test]
        public void Constructor_FileSystemNull_ThrowsArgumentNullException()
        {
            // Act
            void Call() => _ = new InitialFieldFileReader(context, null);

            // Assert
            Assert.That(Call, Throws.ArgumentNullException);
        }

        [Test]
        public void Read_ModelDefinitionNull_ThrowsArgumentNullException()
        {
            // Arrange
            InitialFieldFileReader reader = CreateReader();
            
            const string fileName = "initialFields.ini";

            // Act
            void Call() => reader.Read(fileName, fileName, null);

            // Assert
            Assert.That(Call, Throws.ArgumentNullException);
        }

        [Test]
        [TestCaseSource(typeof(CommonTestCaseSource), nameof(CommonTestCaseSource.NullOrWhiteSpace))]
        public void Read_FilePathNullOrEmpty_ThrowsArgumentException(string filePath)
        {
            // Arrange
            InitialFieldFileReader reader = CreateReader();

            // Act
            void Call() => reader.Read(filePath, "initialFields.ini", new WaterFlowFMModelDefinition());

            // Assert
            Assert.That(Call, Throws.ArgumentException);
        }

        [Test]
        [TestCaseSource(typeof(CommonTestCaseSource), nameof(CommonTestCaseSource.NullOrWhiteSpace))]
        public void Read_ParentFilePathNullOrEmpty_ThrowsArgumentException(string filePath)
        {
            // Arrange
            InitialFieldFileReader reader = CreateReader();

            // Act
            void Call() => reader.Read("initialFields.ini", filePath, new WaterFlowFMModelDefinition());

            // Assert
            Assert.That(Call, Throws.ArgumentException);
        }

        [Test]
        public void Read_FileDoesNotExist_LogsError()
        {
            // Arrange
            InitialFieldFileReader reader = CreateReader();

            const string filePath = "initialFields.ini";

            // Act
            void Call() => reader.Read(filePath, filePath, new WaterFlowFMModelDefinition());

            // Assert
            string error = TestHelper.GetAllRenderedMessages(Call, Level.Error).Single();
            Assert.That(error, Is.EqualTo($"Initial field file does not exist: {filePath}"));
        }
        
        [Test]
        [TestCase("")]
        [TestCase("data/")]
        public void Read_FromInitialFieldFile_StoresDataFileNames(string spatialDataDir)
        {
            // Arrange
            InitialFieldFileReader reader = CreateReader();
            var modelDefinition = new WaterFlowFMModelDefinition();

            const string fileName = "initialFields.ini";
            CreateInitialFieldFile(fileName, spatialDataDir);

            // Act
            reader.Read(fileName, fileName, modelDefinition);

            // Assert
            var expected = new[]
            {
                $"{spatialDataDir}bedlevel.xyz",
                $"{spatialDataDir}initialwaterlevel_samples.xyz",
                $"{spatialDataDir}frictioncoefficient_samples.xyz",
                $"{spatialDataDir}frictioncoefficient_samples_(1).xyz"
            };
            Assert.That(context.DataFileNames, Is.EqualTo(expected));
        }
        
        [Test]
        public void Read_FromInitialFieldFileAndThenEmptyInitialFieldFile_ClearsStoredDataFileNames()
        {
            // Arrange
            InitialFieldFileReader reader = CreateReader();
            var modelDefinition = new WaterFlowFMModelDefinition();

            const string fileName = "initialFields.ini";

            // Act
            CreateInitialFieldFile(fileName);
            reader.Read(fileName, fileName, modelDefinition);

            CreateEmptyInitialFieldFile(fileName);
            reader.Read(fileName, fileName, modelDefinition);
            
            // Assert
            Assert.That(context.DataFileNames, Is.Empty);
        }

        [Test]
        [TestCase("")]
        [TestCase("data/")]
        public void Read_FromInitialFieldFile_AddsDataToModelDefinition(string spatialDataDir)
        {
            // Arrange
            InitialFieldFileReader reader = CreateReader();
            var modelDefinition = new WaterFlowFMModelDefinition();

            const string fileName = "initialFields.ini";
            CreateInitialFieldFile(fileName, spatialDataDir);

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
        public void Read_FromInitialFieldFile_WithWrongFileReference_LogsError()
        {
            // Arrange
            InitialFieldFileReader reader = CreateReader();
            var modelDefinition = new WaterFlowFMModelDefinition();

            const string fileName = "initialFields.ini";
            CreateInitialFieldFile(fileName);
            
            fileSystem.RemoveFile("bedlevel.xyz");

            // Act
            void Call() => reader.Read(fileName, fileName, modelDefinition);

            // Assert
            string error = TestHelper.GetAllRenderedMessages(Call, Level.Error).Single();
            Assert.That(error, Does.Contain("Initial field data file does not exist: bedlevel.xyz"));
            Assert.That(modelDefinition.SpatialOperations, Does.Not.ContainKey(WaterFlowFMModelDefinition.BathymetryDataItemName));
        }

        [Test]
        public void Read_FromEmptyInitialFieldFile_AddsNoDataToModelDefinition()
        {
            // Arrange
            InitialFieldFileReader reader = CreateReader();
            var modelDefinition = new WaterFlowFMModelDefinition();

            const string fileName = "initialFields.ini";
            CreateEmptyInitialFieldFile(fileName);

            // Act
            reader.Read(fileName, fileName, modelDefinition);

            // Assert
            Assert.That(modelDefinition.SpatialOperations, Does.Not.ContainKey(WaterFlowFMModelDefinition.BathymetryDataItemName));
            Assert.That(modelDefinition.SpatialOperations, Does.Not.ContainKey(WaterFlowFMModelDefinition.InitialWaterLevelDataItemName));
            Assert.That(modelDefinition.SpatialOperations, Does.Not.ContainKey(WaterFlowFMModelDefinition.RoughnessDataItemName));
        }

        private void CreateInitialFieldFile(string fileName, string spatialDataDir = "")
        {
            fileSystem.AddFile(fileName, new MockFileData(GetIniData(spatialDataDir)));
            fileSystem.AddEmptyFile($"{spatialDataDir}bedlevel.xyz");
            fileSystem.AddEmptyFile($"{spatialDataDir}initialwaterlevel_samples.xyz");
            fileSystem.AddEmptyFile($"{spatialDataDir}frictioncoefficient_samples.xyz");
            fileSystem.AddEmptyFile($"{spatialDataDir}frictioncoefficient_samples_(1).xyz");
        }

        private void CreateEmptyInitialFieldFile(string fileName)
        {
            fileSystem.AddFile(fileName, new MockFileData(GetEmptyIniData()));
        }

        private InitialFieldFileReader CreateReader()
        {
            return new InitialFieldFileReader(context, fileSystem);
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