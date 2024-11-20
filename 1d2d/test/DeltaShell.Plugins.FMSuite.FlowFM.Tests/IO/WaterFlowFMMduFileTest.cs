using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.Roughness;
using DelftTools.Hydro.Structures;
using DelftTools.TestUtils;
using DelftTools.Utils.Collections;
using DelftTools.Utils.Collections.Generic;
using DelftTools.Utils.IO;
using DelftTools.Utils.Reflection;
using DeltaShell.NGHS.IO.DataObjects;
using DeltaShell.NGHS.IO.DataObjects.Friction;
using DeltaShell.NGHS.IO.DataObjects.InitialConditions;
using DeltaShell.Plugins.FMSuite.FlowFM.FeatureData;
using DeltaShell.Plugins.FMSuite.FlowFM.IO;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using DeltaShell.Plugins.FMSuite.FlowFM.Properties;
using GeoAPI.Geometries;
using log4net.Core;
using NetTopologySuite.Geometries;
using NUnit.Framework;
using Rhino.Mocks;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.IO
{
    [TestFixture]
    public class WaterFlowFMMduFileTest
    {
        private MockRepository mocks;

        [SetUp]
        public void Setup()
        {
            mocks = new MockRepository();
        }

        [TearDown]
        public void TearDown()
        {
            mocks.VerifyAll();
        }

        [Test]
        public void WriteSnappedFeaturesThenReadThemTest()
        {
            var mduFilePath = string.Concat(Path.GetTempFileName(), ".mdu");

            try
            {
                var mduFile = new MduFile();
                var modelDefinition = new WaterFlowFMModelDefinition();
                modelDefinition.WriteSnappedFeatures = true;
                
                //Write
                mduFile.WriteProperties(mduFilePath, modelDefinition.Properties, false, false);
                var readAllText = File.ReadAllText(mduFilePath);

                foreach (var prop in modelDefinition.KnownWriteOutputSnappedFeatures)
                {
                    var expectedText = String.Format("{0,-18}= {1,-20}", prop, 1).Trim();
                    Assert.IsTrue(readAllText.Contains(expectedText), "Expected: {0} not found in: {1}", expectedText, readAllText);
                }
            }
            finally
            {
                FileUtils.DeleteIfExists(mduFilePath);
            }
        }

        [Test]
        public void MduFileHelperReadSubFilePathCollectionTest()
        {
            /* Dummy property, we are only interested in the reading of subfilepath*/
            var propertyDefinition = new WaterFlowFMPropertyDefinition
            {
                MduPropertyName = "CollectionPropertyTestFile",
                DataType = typeof(IList<string>),
                IsMultipleFile = true
            };
            var property = new WaterFlowFMProperty(propertyDefinition, "Test1 Test2");
            Assert.IsNotNull(property);

            var subFileList = MduFileHelper.GetMultipleSubfilePath("mduTestPath", property);
            Assert.AreEqual(2, subFileList.Count);
        }

        [Test]
        [TestCase("geometry", "MultipleLinePropertiesTestFile", "Test1 Test2", "# Test comment 1", "= Test1 Test2 # Test comment 1")]
        [TestCase("geometry", "MultipleLinePropertiesTestFile", "Test1 Test2", "# Test comment 1 Test comment 2", "=Test1 \\ # Test comment 1\r\nTest2 # Test comment 2")] /* Slash separated */
        public void MduFileReadsAndWritesMultipleLinePropertiesIncludingComments(string fileIniSectionName, string propertyName, string expectedValues, string expectedOutputComments, string rawValuesAndComments)
        {
            var nameWithoutExtension = Path.GetTempFileName();
            var mduFilePath = string.Concat(nameWithoutExtension, ".mdu");

            var mduFileText = string.Concat(propertyName, rawValuesAndComments);
            mduFileText = string.Concat("[",fileIniSectionName, "]", "\n", mduFileText);
            File.WriteAllText(mduFilePath, mduFileText);
            try
            {
                var mduFile = new MduFile();
                var modelDefinition = new WaterFlowFMModelDefinition();
                var property = AddCustomMultipleFilePropertyToModelDefinition(modelDefinition, propertyName, fileIniSectionName);

                var convertedFileObjectsForFMModel = CreateConvertedFileObjectsForFMModel();
                convertedFileObjectsForFMModel.ModelDefinition = modelDefinition;

                //Read
                mduFile.Read(mduFilePath, convertedFileObjectsForFMModel);
                Assert.AreEqual(expectedValues, property.GetValueAsString());

                //Write
                mduFile.WriteProperties(mduFilePath, modelDefinition.Properties, false, false);
                var readAllText = File.ReadAllText(mduFilePath);

                var expectedText = String.Format("{0,-18}= {1,-20}{2}", propertyName, expectedValues, expectedOutputComments).Trim();
                Assert.IsTrue( readAllText.Contains(expectedText), "Expected: {0} not found in: {1}", expectedText, readAllText );
            }
            finally 
            {
                FileUtils.DeleteIfExists(mduFilePath);
            }
        }

        [Test]
        [TestCase("geometry", "CustomProperty1", "CustomProperty2", "Test1 Test2", "Test3", true, false)]
        public void MduFileHandlesWrongDeclarationsOfMultipleLineProperties(string fileIniSectionName, string property1Name, string property2Name, string property1Value, string property2Value, bool multipleLineProp1, bool multipleLineProp2)
        {
            var nameWithoutExtension = Path.GetTempFileName();
            var mduFilePath = string.Concat(nameWithoutExtension, ".mdu");
            var property1Text = string.Concat(property1Name, "=", property1Value,
                multipleLineProp1 ? @"\" : string.Empty);
            var property2Text = string.Concat(property2Name, "=", property2Value,
                multipleLineProp2 ? @"\" : string.Empty);
            var mduFileText = string.Concat(property1Text, "\n", property2Text);
            mduFileText = string.Concat("[", fileIniSectionName, "]", "\n", mduFileText);
            File.WriteAllText(mduFilePath, mduFileText);
            try
            {
                var mduFile = new MduFile();
                var convertedFileObjectsForFMModel = CreateConvertedFileObjectsForFMModel();
                var modelDefinition = convertedFileObjectsForFMModel.ModelDefinition;
                var property1 = AddCustomMultipleFilePropertyToModelDefinition(modelDefinition, property1Name, fileIniSectionName);
                var property2 = AddCustomMultipleFilePropertyToModelDefinition(modelDefinition, property2Name, fileIniSectionName);
                modelDefinition.GetModelProperty(KnownProperties.NetFile).Value = string.Concat(nameWithoutExtension, "_net.nc");
                //Read
                mduFile.Read(mduFilePath, convertedFileObjectsForFMModel);
                Assert.AreEqual(property1Value, property1.GetValueAsString());
                Assert.AreEqual(property2Value, property2.GetValueAsString());
            }
            finally
            {
                FileUtils.DeleteIfExists(mduFilePath);
            }
        }

        [Test]
        public void DefaultPropertyFileGetsRenamedIfGroupNameChanges()
        {
            var pathWithoutExtension = Path.GetTempFileName();
            var mduFilePath = string.Concat(pathWithoutExtension, ".mdu");
            var mduFile = new MduFile();
            var mduDir = Path.GetDirectoryName(mduFilePath);
            var modelName = Path.GetFileName(mduFilePath);

            Assert.NotNull(mduDir);
            
            var defaultNameWE = String.Concat(Path.GetFileName(pathWithoutExtension), MduFile.ObsExtension);
            var group1NameWE = String.Concat("Group1", MduFile.ObsExtension);
            var fileObsPointsDefault = Path.Combine(mduDir, defaultNameWE);
            var fileObsPointsGroup1 = Path.Combine(mduDir, group1NameWE);
            using (var model = new WaterFlowFMModel(mduFilePath){ Area = new HydroArea()})
            {
                var area = model.Area;
                /*Observation points, we create 2 with keys and one default. Thus, three expected output files*/
                area.ObservationPoints.AddRange(
                    new[]
                    {
                        WaterFlowFMMduFileTestHelper.GetNewObservationPoint2D(string.Empty, "Feature1"), /*Default group expected*/
                    }
                );
                var defaultFeature = area.ObservationPoints.FirstOrDefault(o => o.Name == "Feature1");
                Assert.IsNotNull(defaultFeature);

                var modelDefinition = new WaterFlowFMModelDefinition(modelName);
                var allFixedWeirsAndCorrespondingProperties = new List<ModelFeatureCoordinateData<FixedWeir>>();
                mduFile.Write(mduFilePath, modelDefinition, area, null, null,null, null, null, null, allFixedWeirsAndCorrespondingProperties);
                //After writing the default groups get updated.
                Assert.IsTrue(defaultFeature.IsDefaultGroup);
                //Now rename the group name and save again.
                defaultFeature.GroupName = group1NameWE;
                mduFile.Write(mduFilePath, modelDefinition, area, null, null,null, null, null, null, allFixedWeirsAndCorrespondingProperties);

                Assert.AreEqual(group1NameWE, defaultFeature.GroupName);
                Assert.IsTrue(File.Exists(mduFilePath));
                var readAllText = File.ReadAllText(mduFilePath);
                var expectedText = String.Format("{0,-18}= {1,-20}", "ObsFile", string.Join(" ", group1NameWE)).Trim();
                Assert.IsTrue(readAllText.Contains(expectedText), "Expected {0} \n Generated: {1}", expectedText, readAllText);
                Assert.IsTrue(File.Exists(fileObsPointsDefault));
                Assert.IsTrue(File.Exists(fileObsPointsGroup1));
            }
            FileUtils.DeleteIfExists(mduFilePath);
            FileUtils.DeleteIfExists(fileObsPointsDefault);
            FileUtils.DeleteIfExists(fileObsPointsGroup1);
        }

        [Test] /* Extension of the one above but directly loading an MDU File. */
        [TestCase(KnownProperties.EnclosureFile, "Enc1_enc.pol Enc2_enc.pol", "CustomPropertyTest", "CustomPropertyValue")]
        public void WhenMduExpectsANewMultipleLinePropertyButItIsANewPropertyItKeepsReading(string hydroAreaFileProperty, string expectedCompositeValue, string customPropertyName, string expectedSimpleValue)
        {
            var mduFilePath = TestHelper.GetTestFilePath(@"HydroAreaCollection\FlowFMPropertyWithSlashAndNoNewLine.mdu");
            mduFilePath = TestHelper.CreateLocalCopy(mduFilePath);

            var mduFile = new MduFile();
            var convertedFileObjectsForFMModel = CreateConvertedFileObjectsForFMModel();
            var modelDefinition = convertedFileObjectsForFMModel.ModelDefinition;
            
            mduFile.Read(mduFilePath, convertedFileObjectsForFMModel);

            var property = modelDefinition.GetModelProperty(hydroAreaFileProperty);
            Assert.IsNotNull(property);
            Assert.AreEqual(expectedCompositeValue, property.GetValueAsString());

            var customProperty = modelDefinition.GetModelProperty(customPropertyName);
            Assert.IsNotNull(customProperty);
            Assert.AreEqual(expectedSimpleValue, customProperty.GetValueAsString());

            FileUtils.DeleteIfExists(mduFilePath);
        }

        [Test]
        public void MduFileReadsFromMultipleFilesAnAssignsGroupNamesToXyZFileDryPointFeature()
        {
            var mduFilePath = TestHelper.GetTestFilePath(@"HydroAreaCollection\FlowFM\dryPointsMdu.mdu");
            mduFilePath = TestHelper.CreateLocalCopy(mduFilePath);
            var modelName = Path.GetFileName(mduFilePath);
            try
            {
                var mduFile = new MduFile();
                var convertedFileObjectsForFMModel = CreateConvertedFileObjectsForFMModel(modelName);
                var area = convertedFileObjectsForFMModel.HydroArea;
                
                mduFile.Read(mduFilePath, convertedFileObjectsForFMModel);
                CheckFeatureWasReadCorrectly(area.DryPoints, "DryAreas (points)", new List<string> { "dryGroup1_dry.xyz", "dryGroup2_dry.xyz" });                
            }
            finally
            {
                FileUtils.DeleteIfExists(mduFilePath);
            }
        }

        [Test]
        public void WritePropertyWhenMultipleFileIsTrue()
        {
            var nameWithoutExtension = Path.GetTempFileName();
            var mduFilePath = String.Concat(nameWithoutExtension, ".mdu");
            var mduFile = new MduFile();

            /* Dummy property, we are only interested in the reading of subfilepath*/
            var propertyDefinition = new WaterFlowFMPropertyDefinition
            {
                MduPropertyName = "CollectionPropertyTestFile",
                FileSectionName = "TestSection",
                DataType = typeof(IList<string>),
                IsMultipleFile = true
            };
            var property = new WaterFlowFMProperty(propertyDefinition, "Test1 Test2");
            mduFile.WriteProperties(mduFilePath, new List<WaterFlowFMProperty>() { property }, false, false);

            Assert.IsTrue(File.Exists(mduFilePath));
            var readAllText = File.ReadAllText(mduFilePath);
            var expectedTest = String.Format("{0,-18}= {1,-20}{2}", property.PropertyDefinition.MduPropertyName, "Test1 Test2", "").Trim();
            Assert.IsTrue(readAllText.Contains(expectedTest));

            FileUtils.DeleteIfExists(mduFilePath);
        }

        [Test]
        public void MduFileWritesOneFilePerGroupDeclaredInTheMduAndAMultipleValueInTheMduProperty()
        {
            var pathWithoutExtension = Path.GetTempFileName();
            var mduFilePath = String.Concat(pathWithoutExtension, ".mdu");
            var mduFile = new MduFile();
            var mduDir = Path.GetDirectoryName(mduFilePath);
            var modelName = Path.GetFileName(mduFilePath);

            Assert.NotNull(mduDir);

            string ObsExtension = "_obs.xyn";
            var defaultNameWE = String.Concat(Path.GetFileName(pathWithoutExtension), ObsExtension);
            var group1NameWE = String.Concat("Group1", ObsExtension);
            var group2NameWE = String.Concat("Group2", ObsExtension);
            var fileObsPointsDefault = Path.Combine(mduDir, defaultNameWE);
            var fileObsPointsGroup1 = Path.Combine(mduDir, group1NameWE);
            var fileObsPointsGroup2 = Path.Combine(mduDir, group2NameWE);
            try
            {
                var area = new HydroArea();
                /*Observation points, we create 2 with keys and one default. Thus, three expected output files*/
                area.ObservationPoints.AddRange(
                    new[]
                    {
                        WaterFlowFMMduFileTestHelper.GetNewObservationPoint2D("", "Feature1"), /*Default group expected*/
                        WaterFlowFMMduFileTestHelper.GetNewObservationPoint2D("Group1", "Feature2"),
                        WaterFlowFMMduFileTestHelper.GetNewObservationPoint2D("Group1", "Feature3"),
                        WaterFlowFMMduFileTestHelper.GetNewObservationPoint2D("Group2", "Feature4")
                    }
                );
                var defaultFeature = area.ObservationPoints.FirstOrDefault(o => o.Name == "Feature1");
                Assert.IsNotNull(defaultFeature);

                var modelDefinition = new WaterFlowFMModelDefinition(modelName);
                var allFixedWeirsAndCorrespondingProperties = new List<ModelFeatureCoordinateData<FixedWeir>>();
                mduFile.Write(mduFilePath, modelDefinition, area, null, null,null, null, null, null, allFixedWeirsAndCorrespondingProperties);
                //After writing the default groups get updated.
                Assert.IsTrue(defaultFeature.IsDefaultGroup);

                Assert.IsTrue(File.Exists(mduFilePath));
                var readAllText = File.ReadAllText(mduFilePath);
                var expectedText = String.Format("{0,-18}= {1,-20}", "ObsFile", string.Join(" ", defaultNameWE, group1NameWE, group2NameWE)).Trim();
                Assert.IsTrue(readAllText.Contains(expectedText), "Expected {0} \n Generated: {1}", expectedText, readAllText);
                Assert.IsTrue(File.Exists(fileObsPointsDefault));
                Assert.IsTrue(File.Exists(fileObsPointsGroup1));
                Assert.IsTrue(File.Exists(fileObsPointsGroup2));
            }
            finally
            {
                FileUtils.DeleteIfExists(mduFilePath);
                FileUtils.DeleteIfExists(fileObsPointsDefault);
                FileUtils.DeleteIfExists(fileObsPointsGroup1);
                FileUtils.DeleteIfExists(fileObsPointsGroup2);
            }
        }

        [Test]
        public void GivenMduFileWithDryPointsAndDryAreasFilesBothOnDryPointsFileIniSection_WhenReadingMdu_ThenBothFilesAreCorrectlyRead()
        {
            var mduFilePath = TestHelper.GetTestFilePath(@"HydroAreaCollection\FlowFM\dryPointsAndAreasInModel.mdu");
            mduFilePath = TestHelper.CreateLocalCopy(mduFilePath);
            var mduDir = Path.GetDirectoryName(mduFilePath);
            Assert.NotNull(mduDir);
            var modelName = Path.GetFileName(mduFilePath);

            var mduFile = new MduFile();
            var convertedFileObjectsForFMModel = CreateConvertedFileObjectsForFMModel(modelName);
            var area = convertedFileObjectsForFMModel.HydroArea;
            
            mduFile.Read(mduFilePath, convertedFileObjectsForFMModel);

            Assert.That(area.DryPoints.Count, Is.EqualTo(6));
            Assert.That(area.DryAreas.Count, Is.EqualTo(2));
        }

        [Test]
        public void GivenMduFileWithOnlyOneDryPointsFileReferenceInMdu_WhenReadingMdu_ThenFileIsCorrectlyRead()
        {
            var mduFilePath = TestHelper.GetTestFilePath(@"HydroAreaCollection\FlowFM\OnlyDryPointsInModel.mdu");
            mduFilePath = TestHelper.CreateLocalCopy(mduFilePath);
            var mduDir = Path.GetDirectoryName(mduFilePath);
            Assert.NotNull(mduDir);
            var modelName = Path.GetFileName(mduFilePath);

            var mduFile = new MduFile();
            var convertedFileObjectsForFMModel = CreateConvertedFileObjectsForFMModel(modelName);
            var area = convertedFileObjectsForFMModel.HydroArea;
            
            mduFile.Read(mduFilePath, convertedFileObjectsForFMModel);

            Assert.That(area.DryPoints.Count, Is.EqualTo(3));
            Assert.IsEmpty(area.DryAreas); //Check this, because dry areas and dry points are read in the same method (MduFile.ReadDryPointsAndDryAreas)
        }

        [Test]
        public void GivenMduFileWithOnlyOneDryAreasFileReferenceInMdu_WhenReadingMdu_ThenFileIsCorrectlyRead()
        {
            var mduFilePath = TestHelper.GetTestFilePath(@"HydroAreaCollection\FlowFM\OnlyDryAreasInModel.mdu");
            mduFilePath = TestHelper.CreateLocalCopy(mduFilePath);
            var mduDir = Path.GetDirectoryName(mduFilePath);
            Assert.NotNull(mduDir);
            var modelName = Path.GetFileName(mduFilePath);

            var mduFile = new MduFile();
            var convertedFileObjectsForFMModel = CreateConvertedFileObjectsForFMModel(modelName);
            var area = convertedFileObjectsForFMModel.HydroArea;
            
            mduFile.Read(mduFilePath, convertedFileObjectsForFMModel);

            Assert.IsEmpty(area.DryPoints); //Check this, because dry areas and dry points are read in the same method (MduFile.ReadDryPointsAndDryAreas)
            Assert.That(area.DryAreas.Count, Is.EqualTo(1));
        }
        
        [Test]
        public void MduFileReadsAndWritesXyzFileDryPointFeature()
        {
            var mduFilePath = TestHelper.GetTestFilePath(@"HydroAreaCollection\FlowFM\dryPointsMdu.mdu");
            mduFilePath = TestHelper.CreateLocalCopy(mduFilePath);
            var mduDir = Path.GetDirectoryName(mduFilePath);
            Assert.NotNull(mduDir);
            var modelName = Path.GetFileName(mduFilePath);

            var saveDirectory = Path.Combine(mduDir, "MduFileReadsAndWritesTest");
            Directory.CreateDirectory(saveDirectory);
            var savePath = Path.Combine(saveDirectory, "SaveDryPoint.mdu");
            var newMduName = Path.GetFileName(savePath);
            try
            {
                var mduFile = new MduFile();

                var convertedFileObjectsForFMModelOriginal = CreateConvertedFileObjectsForFMModel(modelName);
                var originalMd = convertedFileObjectsForFMModelOriginal.ModelDefinition;
                var originalArea = convertedFileObjectsForFMModelOriginal.HydroArea;
                var allFixedWeirsAndCorrespondingProperties = convertedFileObjectsForFMModelOriginal.AllFixedWeirsAndCorrespondingProperties;
                
                mduFile.Read(mduFilePath, convertedFileObjectsForFMModelOriginal);
                mduFile.Write(savePath, originalMd, originalArea, null, null, null, null, null, null, allFixedWeirsAndCorrespondingProperties, switchTo: false, writeExtForcings: false);

                var convertedFileObjectsForFMModelSaved = CreateConvertedFileObjectsForFMModel(newMduName);
                convertedFileObjectsForFMModelSaved.AllFixedWeirsAndCorrespondingProperties = allFixedWeirsAndCorrespondingProperties;
                var savedMd = convertedFileObjectsForFMModelSaved.ModelDefinition;
                var savedArea = convertedFileObjectsForFMModelSaved.HydroArea;
                
                mduFile.Read(savePath, convertedFileObjectsForFMModelSaved);

                //Check MDU property.
                CompareHydroAreaModelProperties(KnownProperties.DryPointsFile, savePath, originalMd, savedMd);

                //Check feature
                CheckDryPointsFeature(originalArea, savedArea);
            }
            finally
            {
                FileUtils.DeleteIfExists(mduFilePath);
                FileUtils.DeleteIfExists(savePath);
                FileUtils.DeleteIfExists(saveDirectory);
            }
        }

        [Test]
        public void MduFileWritesDefaultValuesForDryPointFeature()
        {
            var mduFilePath = TestHelper.GetTestFilePath(@"HydroAreaCollection\FlowFM\dryPointsMdu.mdu");
            mduFilePath = TestHelper.CreateLocalCopy(mduFilePath);
            var mduDir = Path.GetDirectoryName(mduFilePath);
            Assert.NotNull(mduDir);
            var modelName = Path.GetFileName(mduFilePath);

            var saveDirectory = Path.Combine(mduDir, "MduFileReadsAndWritesTest");
            Directory.CreateDirectory(saveDirectory);
            var savePath = Path.Combine(saveDirectory, "SaveDryPoint.mdu");
            var newMduName = Path.GetFileName(savePath);
            try
            {
                var mduFile = new MduFile();

                var convertedFileObjectsForFMModelOriginal = CreateConvertedFileObjectsForFMModel(modelName);
                var originalMd = convertedFileObjectsForFMModelOriginal.ModelDefinition;
                var originalArea = convertedFileObjectsForFMModelOriginal.HydroArea;
                var allFixedWeirsAndCorrespondingProperties = convertedFileObjectsForFMModelOriginal.AllFixedWeirsAndCorrespondingProperties;
                
                mduFile.Read(mduFilePath, convertedFileObjectsForFMModelOriginal);

                RemoveGroupNameFromGroupableFeature(originalArea.DryPoints);
                mduFile.Write(savePath, originalMd, originalArea, null, null, null, null,null, null, allFixedWeirsAndCorrespondingProperties, switchTo: false, writeExtForcings: false);

                var convertedFileObjectsForFMModelSaved = CreateConvertedFileObjectsForFMModel(newMduName);
                convertedFileObjectsForFMModelSaved.AllFixedWeirsAndCorrespondingProperties = allFixedWeirsAndCorrespondingProperties;
                var savedMd = convertedFileObjectsForFMModelSaved.ModelDefinition;
                var savedArea = convertedFileObjectsForFMModelSaved.HydroArea;

                mduFile.Read(savePath, convertedFileObjectsForFMModelSaved);

                //Check MDU property.
                CompareHydroAreaModelProperties("DryPointsFile", savePath, originalMd, savedMd);
                //Check feature
                CheckDryPointsFeature(originalArea, savedArea);
                //Check default group was created.
                var mduPathName = Path.GetFileNameWithoutExtension(savePath);
                CheckDefaultGroupIsInFeature("DryPoints", originalArea.DryPoints, mduPathName, "_dry.xyz");
            }
            finally
            {
                FileUtils.DeleteIfExists(mduFilePath);
                FileUtils.DeleteIfExists(savePath);
                FileUtils.DeleteIfExists(saveDirectory);
            }
        }


        [Test]
        [TestCase("HydroAreaCollection\\FlowFM.mdu", 2)]
        [TestCase("HydroAreaCollection\\repeatedProperty.mdu", 1)]
        [TestCase("HydroAreaCollection\\slashSeparated.mdu", 2)]
        [TestCase("HydroAreaCollection\\spaceSeparated.mdu", 2)]
        public void ReadHydroAreaCollectionIntoModelDefinitionTest(string filePath, int expectedEnclosures)
        {
            var mduFilePath = TestHelper.GetTestFilePath(filePath);
            mduFilePath = TestHelper.CreateLocalCopy(mduFilePath);
            var modelName = Path.GetFileName(mduFilePath);

            try
            {
                var mduFile = new MduFile();
                var convertedFileObjectsForFMModel = CreateConvertedFileObjectsForFMModel(modelName);
                var area = convertedFileObjectsForFMModel.HydroArea;
                
                mduFile.Read(mduFilePath, convertedFileObjectsForFMModel);

                Assert.AreEqual(expectedEnclosures, area.Enclosures.Count);
            }
            finally
            {
                FileUtils.DeleteIfExists(mduFilePath);
            }
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        [TestCase(@"FilesInsideMduSubFolderProject.dsproj_data\FlowFM\FlowFM.mdu")]
        [TestCase(@"FilesInsideMduSubFolderButWithRelativePathsProject.dsproj_data\FlowFM\FlowFM.mdu")]
        public void GivenAProjectFolderWithFeatureFilesInsideOfAnMduSubFolder_WhenReadingMdu_ThenAllFilesAreReadAndNotCopied(string mduProjectFilePath)
        {
            // Preparations
            string localPath = TestHelper.CreateLocalCopy(TestHelper.GetTestFilePath(@"HydroAreaCollection\MduFileProjects"));
            string mduFilePath = Path.Combine(localPath, mduProjectFilePath);
            string featureFileDirectory = Path.Combine(Path.GetDirectoryName(mduFilePath), @"FeatureFiles");

            string modelName = Path.GetFileNameWithoutExtension(mduFilePath);
            var convertedFileObjectsForFMModel = CreateConvertedFileObjectsForFMModel(modelName);
            var area = convertedFileObjectsForFMModel.HydroArea;
            var mduFile = new MduFile();

            mduFile.Read(mduFilePath, convertedFileObjectsForFMModel);

            // Check if all features were read
            Assert.That(area.DryPoints.Count, Is.EqualTo(2));
            Assert.That(area.Enclosures.Count, Is.EqualTo(1));
            Assert.That(area.FixedWeirs.Count, Is.EqualTo(1));
            Assert.That(area.ObservationPoints.Count, Is.EqualTo(2));
            Assert.That(area.Pumps.Count, Is.EqualTo(1));
            Assert.That(area.ThinDams.Count, Is.EqualTo(1));
            Assert.That(area.Gates.Count, Is.EqualTo(1));
            Assert.That(area.Weirs.Count, Is.EqualTo(1));
            Assert.That(area.ObservationCrossSections.Count, Is.EqualTo(1));
            Assert.That(area.LandBoundaries.Count, Is.EqualTo(1));

            CheckIfFilesAreNotCopied(featureFileDirectory, Path.GetDirectoryName(mduFilePath));
        }

        [Test]
        public void GivenAnMduFileWithFileNamePropertyStartingWithSlashes_WhenReadingMduFile_ThenSlashesAreIgnoredAndFileIsRead()
        {
            // Preparations
            const string baseFolderPath = @"HydroAreaCollection\MduFileProjects";
            var filePath = @"LeadingSlashesMdu\FlowFM.mdu";
            var testDir = TestHelper.GetTestDataDirectoryPathForAssembly(GetType().Assembly);

            var originalFolderPath = Path.Combine(testDir, baseFolderPath);
            var mduPath = Path.Combine(originalFolderPath, filePath);

            var modelName = Path.GetFileNameWithoutExtension(filePath);
            var mduFile = new MduFile();
            var convertedFileObjectsForFMModel = CreateConvertedFileObjectsForFMModel(modelName);
            var area = convertedFileObjectsForFMModel.HydroArea;
            
            mduFile.Read(mduPath, convertedFileObjectsForFMModel);

            // Check if all features were read
            Assert.That(area.ObservationPoints.Count, Is.EqualTo(1));
        }

        [Test]
        public void GivenAnMduFileWithNonExistentReferenceToFile_WhenReadingMduFile_ThenTheUserGetsAnErrorMessage()
        {
            // Preparations
            const string baseFolderPath = @"HydroAreaCollection\MduFileProjects";
            var filePath = @"MissingFileMdu\FlowFM.mdu";
            var testDir = TestHelper.GetTestDataDirectoryPathForAssembly(GetType().Assembly);

            var originalFolderPath = Path.Combine(testDir, baseFolderPath);
            var mduPath = Path.Combine(originalFolderPath, filePath);
            
            var modelName = Path.GetFileNameWithoutExtension(filePath);
            var mduFile = new MduFile();
            var convertedFileObjectsForFMModel = CreateConvertedFileObjectsForFMModel(modelName);
            var area = convertedFileObjectsForFMModel.HydroArea;
            
            // Call
            void Call() => mduFile.Read(mduPath, convertedFileObjectsForFMModel);

            // Assert
            List<string> messages = TestHelper.GetAllRenderedMessages(Call, Level.Error).ToList();

            string[] expectedErrorMessages = new []
            {
                string.Format(Resources.MduFileReferenceDoesNotExist, "FlowFM_net.nc", mduPath, "NetFile", modelName),
                string.Format(Resources.MduFileReferenceDoesNotExist, "MyObservationPoints_obs.xyn", mduPath, "ObsFile", modelName)
            };
                
            Assert.That(messages, Is.EqualTo(expectedErrorMessages));
            Assert.That(area.ObservationPoints, Is.Empty);
        }

        [Test]
        public void GivenMduFileWithReferencesToNonExistentFilesOrFileNamesThatAlreadyExistInTheMduFolder_WhenReadingMdu_ThenTheseFeaturesAreNotRead()
        {
            // Preparations
            const string baseFolderPath = @"HydroAreaCollection\MduFileProjects";
            const string mduProjectFilePath = @"MissingFeatureFilesProject.dsproj_data\FlowFM.mdu";
            
            var testDir = TestHelper.GetTestDataDirectoryPathForAssembly(GetType().Assembly);

            var originalFolderPath = Path.Combine(testDir, baseFolderPath);
            
            var testWorkingFolder = TestHelper.CreateLocalCopy(originalFolderPath);
            var mduPath = Path.Combine(testWorkingFolder, mduProjectFilePath);

            var modelName = Path.GetFileNameWithoutExtension(mduProjectFilePath);
            var mduFile = new MduFile();
            var convertedFileObjectsForFMModel = CreateConvertedFileObjectsForFMModel(modelName);
            var area = convertedFileObjectsForFMModel.HydroArea;
            
            mduFile.Read(mduPath, convertedFileObjectsForFMModel);

            Assert.That(area.DryPoints.Count, Is.EqualTo(0));
            Assert.That(area.Enclosures.Count, Is.EqualTo(1));
            Assert.That(area.FixedWeirs.Count, Is.EqualTo(0));
            Assert.That(area.Gates.Count, Is.EqualTo(0));
            Assert.That(area.ObservationPoints.Count, Is.EqualTo(2));
            Assert.That(area.Pumps.Count, Is.EqualTo(0));
            Assert.That(area.ThinDams.Count, Is.EqualTo(0));
            Assert.That(area.Weirs.Count, Is.EqualTo(0));
            Assert.That(area.ObservationCrossSections.Count, Is.EqualTo(0));
            Assert.That(area.LandBoundaries.Count, Is.EqualTo(0));
        }
        
        [Test]
        public void GivenMduFileWithFileNamesThatAlreadyExistInTheMduFolder_WhenReadingMdu_ThenTheseFeaturesAreRead()
        {
            // Preparations
            const string baseFolderPath = @"HydroAreaCollection\MduFileProjects";
            const string mduProjectFilePath = @"DuplicateFilesProject.dsproj_data\FlowFM\FlowFM.mdu";
            
            var testDir = TestHelper.GetTestDataDirectoryPathForAssembly(GetType().Assembly);

            var originalFolderPath = Path.Combine(testDir, baseFolderPath);
            
            var testWorkingFolder = TestHelper.CreateLocalCopy(originalFolderPath);
            var mduPath = Path.Combine(testWorkingFolder, mduProjectFilePath);

            var modelName = Path.GetFileNameWithoutExtension(mduProjectFilePath);
            var mduFile = new MduFile();
            var convertedFileObjectsForFMModel = CreateConvertedFileObjectsForFMModel(modelName);
            var area = convertedFileObjectsForFMModel.HydroArea;
            
            mduFile.Read(mduPath, convertedFileObjectsForFMModel);

            Assert.That(area.DryPoints.Count, Is.EqualTo(2));
            Assert.That(area.Enclosures.Count, Is.EqualTo(1));
            Assert.That(area.FixedWeirs.Count, Is.EqualTo(1));
            Assert.That(area.Gates.Count, Is.EqualTo(1));
            Assert.That(area.ObservationPoints.Count, Is.EqualTo(2));
            Assert.That(area.Pumps.Count, Is.EqualTo(1));
            Assert.That(area.ThinDams.Count, Is.EqualTo(1));
            Assert.That(area.Weirs.Count, Is.EqualTo(1));
            Assert.That(area.ObservationCrossSections.Count, Is.EqualTo(1));
            Assert.That(area.LandBoundaries.Count, Is.EqualTo(1));
        }

        [Test]
        [TestCase("obspoints", "OBSPOINTS", true)]
        [TestCase("obspoints", "OBSPOINTS", false)]
        [TestCase("OBSPOINTS", "obspoints", true)]
        [TestCase("OBSPOINTS", "obspoints", false)]
        public void GivenTwoFeaturesWithNameThatDifferByACapitalLetter_WhenWritingMduFile_ThenBothAreWrittenToTheSameFile(string firstGroupName, string secondGroupName, bool fileShouldAlreadyExists)
        {
            string groupName1 = firstGroupName + MduFile.ObsExtension;
            string groupName2 = secondGroupName + MduFile.ObsExtension;
            string existingGroupName = "ObSpOiNtS" + MduFile.ObsExtension;

            var mduFilePath = string.Concat(Path.GetTempFileName(), ".mdu");
            var mduFile = new MduFile();
            var mduDir = Path.GetDirectoryName(mduFilePath);
            var modelName = Path.GetFileName(Path.GetFileNameWithoutExtension(mduFilePath));

            File.Delete(Path.Combine(mduDir, groupName1));
            if (fileShouldAlreadyExists)
            {
                var fileStream = File.Create(Path.Combine(mduDir, existingGroupName));
                fileStream.Close();
            }
            Assert.NotNull(mduDir);

            var area = new HydroArea();
            var name1 = "ObsPoint01";
            var name2 = "ObsPoint02";
            area.ObservationPoints.AddRange(
                new[]
                {
                    WaterFlowFMMduFileTestHelper.GetNewObservationPoint2D(groupName1, name1),
                    WaterFlowFMMduFileTestHelper.GetNewObservationPoint2D(groupName2, name2)
                }
            );

            var modelDefinition = new WaterFlowFMModelDefinition(modelName);
            var allFixedWeirsAndCorrespondingProperties = new List<ModelFeatureCoordinateData<FixedWeir>>();
            TestHelper.AssertAtLeastOneLogMessagesContains(() => mduFile.Write(mduFilePath, modelDefinition, area, null, null, null, null,null, null, allFixedWeirsAndCorrespondingProperties),
                fileShouldAlreadyExists ? "already exists in the project folder. Features in group" : "Features with group name");

            var files = Directory.GetFiles(mduDir);
            var groupName1FileCount = files.Count(fp => fp.Contains(groupName1));
            var groupName2FileCount = files.Count(fp => fp.Contains(groupName2));
            var existingFileCount = files.Count(fp => fp.Contains(existingGroupName));

            Assert.That(existingFileCount, Is.EqualTo(fileShouldAlreadyExists ? 1 : 0));
            Assert.That(groupName1FileCount, Is.EqualTo(fileShouldAlreadyExists ? 0 : 1));
            Assert.That(groupName2FileCount, Is.EqualTo(0));

            var convertedFileObjectsForFMModel = CreateConvertedFileObjectsForFMModel();
            convertedFileObjectsForFMModel.ModelDefinition = modelDefinition;
            convertedFileObjectsForFMModel.AllFixedWeirsAndCorrespondingProperties = allFixedWeirsAndCorrespondingProperties;

            area = convertedFileObjectsForFMModel.HydroArea;
            
            mduFile.Read(mduFilePath, convertedFileObjectsForFMModel);
            Assert.That(area.ObservationPoints.Count, Is.EqualTo(2));
            Assert.IsTrue(area.ObservationPoints.Select(o => o.Name).Contains(name1));
            Assert.IsTrue(area.ObservationPoints.Select(o => o.Name).Contains(name2));

            // Delete all files that were created during this test
            files.Where(fp => fp.Contains(groupName1) || fp.Contains(groupName2) || fp.Contains(existingGroupName)).ForEach(File.Delete);
        }

        [Test]
        public void GivenModelWithOneDryAreaAndOneDryPoint_WhenWritingAndReadingMduFile_ThenBothFeaturesArePresent()
        {
            var mduFilePath = string.Concat(Path.GetTempFileName(), ".mdu");
            var mduFile = new MduFile();
            var mduDir = Path.GetDirectoryName(mduFilePath);
            var modelName = Path.GetFileName(Path.GetFileNameWithoutExtension(mduFilePath));
            Assert.IsNotNull(mduDir);

            var area = new HydroArea();
            var dryPointsGroupName = @"featureFiles/myDryPoints";
            area.DryPoints.Add(WaterFlowFMMduFileTestHelper.GetNewGroupablePointFeature(dryPointsGroupName));
            var dryAreasGroupName = @"featureFiles/myDryAreas";
            area.DryAreas.Add(WaterFlowFMMduFileTestHelper.GetNewGroupableFeature2DPolygon(dryAreasGroupName, "Polygon01"));

            var modelDefinition = new WaterFlowFMModelDefinition(modelName);
            var allFixedWeirsAndCorrespondingProperties = new List<ModelFeatureCoordinateData<FixedWeir>>();
            mduFile.Write(mduFilePath, modelDefinition, area, null, null, null, null, null,null, allFixedWeirsAndCorrespondingProperties);

            var convertedFileObjectsForFMModel = CreateConvertedFileObjectsForFMModel();
            convertedFileObjectsForFMModel.ModelDefinition = modelDefinition;
            convertedFileObjectsForFMModel.AllFixedWeirsAndCorrespondingProperties = allFixedWeirsAndCorrespondingProperties;
            var newArea = convertedFileObjectsForFMModel.HydroArea;
            
            mduFile.Read(mduFilePath, convertedFileObjectsForFMModel);
            Assert.That(newArea.DryAreas.Count, Is.EqualTo(1));
            Assert.That(newArea.DryPoints.Count, Is.EqualTo(1));

            var dryPointsFileNameWithExtension = dryPointsGroupName + MduFile.DryPointExtension;

            FileUtils.DeleteIfExists(mduFilePath);
            FileUtils.DeleteIfExists(mduFilePath.Replace(".mdu", string.Empty));
            DeleteAllFilesAndFoldersInSubDirectory(new DirectoryInfo(Path.GetDirectoryName(Path.Combine(mduDir, dryPointsFileNameWithExtension))));
        }

        [Test]
        public void GivenModelWithOneDryAreaAndOneDryPoint_WhenWritingMduFile_ThenBothFeatureFileReferencesAreWrittenToMduFile()
        {
            var mduFilePath = string.Concat(Path.GetTempFileName(), ".mdu");
            var mduFile = new MduFile();
            var mduDir = Path.GetDirectoryName(mduFilePath);
            var modelName = Path.GetFileName(Path.GetFileNameWithoutExtension(mduFilePath));
            Assert.IsNotNull(mduDir);

            var area = new HydroArea();
            var dryPointsGroupName = @"featureFiles/myDryPoints";
            area.DryPoints.Add(WaterFlowFMMduFileTestHelper.GetNewGroupablePointFeature(dryPointsGroupName));
            var dryAreasGroupName = @"featureFiles/myDryAreas";
            area.DryAreas.Add(WaterFlowFMMduFileTestHelper.GetNewGroupableFeature2DPolygon(dryAreasGroupName, "Polygon01"));

            var modelDefinition = new WaterFlowFMModelDefinition(modelName);
            var allFixedWeirsAndCorrespondingProperties = new List<ModelFeatureCoordinateData<FixedWeir>>();
            mduFile.Write(mduFilePath, modelDefinition, area, null, null, null, null,null, null, allFixedWeirsAndCorrespondingProperties);

            var readAllText = File.ReadAllText(mduFilePath);

            var dryPointsFileNameWithExtension = dryPointsGroupName + MduFile.DryPointExtension;
            var dryAreasFileNameWithExtension = dryAreasGroupName + MduFile.DryAreaExtension;

            WaterFlowFMMduFileTestHelper.AssertContainsMduLine(readAllText, "DryPointsFile", dryPointsFileNameWithExtension + " " + dryAreasFileNameWithExtension);

            Assert.IsTrue(File.Exists(Path.Combine(mduDir, dryPointsFileNameWithExtension)));
            Assert.IsTrue(File.Exists(Path.Combine(mduDir, dryAreasFileNameWithExtension)));

            FileUtils.DeleteIfExists(mduFilePath);
            FileUtils.DeleteIfExists(mduFilePath.Replace(".mdu", string.Empty));
            DeleteAllFilesAndFoldersInSubDirectory(new DirectoryInfo(Path.GetDirectoryName(Path.Combine(mduDir, dryPointsFileNameWithExtension))));
        }

        [Test]
        [TestCase("FeatureFiles")]
        [TestCase("../FeatureFiles")]
        public void GivenFeaturesWithGroupNamesThatPointToRelativeFolderPath_WhenWriting_ThenMduFileAndFeatureFilesAreBeingWritten(string baseDir)
        {
            using (var tempDir = new TemporaryDirectory())
            {
                string mduDir = tempDir.CreateDirectory("FlowFM/input");
                string mduFilePath = Path.Combine(mduDir, "FlowFM.mdu");

                var mduFile = new MduFile();

                var area = new HydroArea();
                
                var pointGroupName = $@"{baseDir}/myObsPoints";
                area.ObservationPoints.AddRange(
                    new[]
                    {
                        WaterFlowFMMduFileTestHelper.GetNewObservationPoint2D(pointGroupName, "ObsPoint01"),
                        WaterFlowFMMduFileTestHelper.GetNewObservationPoint2D(pointGroupName, "ObsPoint02")
                    }
                );

                var enclosureGroupName = $@"{baseDir}/myPolygons";
                area.Enclosures.AddRange(
                    new[]
                    {
                        WaterFlowFMMduFileTestHelper.GetNewGroupableFeature2DPolygon(enclosureGroupName, "Polygon01"),
                        WaterFlowFMMduFileTestHelper.GetNewGroupableFeature2DPolygon(enclosureGroupName, "Polygon02")
                    }
                );

                var dryPointsGroupName = $@"{baseDir}/myDryPoints";
                area.DryPoints.AddRange(
                    new[]
                    {
                        WaterFlowFMMduFileTestHelper.GetNewGroupablePointFeature(dryPointsGroupName),
                        WaterFlowFMMduFileTestHelper.GetNewGroupablePointFeature(dryPointsGroupName)
                    }
                );

                var landBoundariesGroupName = $@"{baseDir}/myLandBoundaries";
                area.LandBoundaries.AddRange(
                    new[]
                    {
                        WaterFlowFMMduFileTestHelper.GetNewLandBoundary2D(landBoundariesGroupName, "LandBoundary01"),
                        WaterFlowFMMduFileTestHelper.GetNewLandBoundary2D(landBoundariesGroupName, "LandBoundary02")
                    }
                );

                var structureGroupName = $@"{baseDir}/myGates";
                area.Gates.AddRange(
                    new[]
                    {
                        WaterFlowFMMduFileTestHelper.GetNewGate2D(structureGroupName, "Gate01"),
                        WaterFlowFMMduFileTestHelper.GetNewGate2D(structureGroupName, "Gate02")
                    }
                );

                var modelDefinition = new WaterFlowFMModelDefinition();
                var allFixedWeirsAndCorrespondingProperties = new List<ModelFeatureCoordinateData<FixedWeir>>();
                mduFile.Write(mduFilePath, modelDefinition, area, null, null, null, null, null,null, allFixedWeirsAndCorrespondingProperties);

                string readAllText = File.ReadAllText(mduFilePath);

                string obsFileNameWithExtension = pointGroupName + FileConstants.ObsPointFileExtension;
                string enclosureFileNameWithExtension = enclosureGroupName + FileConstants.EnclosureExtension;
                string dryPointsFileNameWithExtension = dryPointsGroupName + FileConstants.DryPointFileExtension;
                string landBoundariesFileNameWithExtension = landBoundariesGroupName + FileConstants.LandBoundaryFileExtension;

                WaterFlowFMMduFileTestHelper.AssertContainsMduLine(readAllText, modelDefinition.GetModelProperty(KnownProperties.ObsFile).PropertyDefinition.Caption, obsFileNameWithExtension);
                WaterFlowFMMduFileTestHelper.AssertContainsMduLine(readAllText, modelDefinition.GetModelProperty(KnownProperties.EnclosureFile).PropertyDefinition.Caption, enclosureFileNameWithExtension);
                WaterFlowFMMduFileTestHelper.AssertContainsMduLine(readAllText, modelDefinition.GetModelProperty(KnownProperties.DryPointsFile).PropertyDefinition.Caption, dryPointsFileNameWithExtension);
                WaterFlowFMMduFileTestHelper.AssertContainsMduLine(readAllText, modelDefinition.GetModelProperty(KnownProperties.LandBoundaryFile).PropertyDefinition.Caption, landBoundariesFileNameWithExtension);

                Assert.IsTrue(File.Exists(Path.Combine(mduDir, obsFileNameWithExtension)));
                Assert.IsTrue(File.Exists(Path.Combine(mduDir, enclosureFileNameWithExtension)));
                Assert.IsTrue(File.Exists(Path.Combine(mduDir, dryPointsFileNameWithExtension)));
                Assert.IsTrue(File.Exists(Path.Combine(mduDir, landBoundariesFileNameWithExtension)));
            }
        }

        [Test]
        public void GivenPolylineFeaturesWithPlizExtension_WhenWritingMduFile_ThenFilesAreSavedAsPlizFile()
        {
            var mduFilePath = string.Concat(Path.GetTempFileName(), ".mdu");
            var mduDir = Path.GetDirectoryName(mduFilePath);
            var modelName = Path.GetFileName(Path.GetFileNameWithoutExtension(mduFilePath));
            Assert.NotNull(mduDir);

            var obsCrsGroupName = "myObsCrossSection_crs.pliz";
            var obsCrsFileName = Path.Combine(mduDir, obsCrsGroupName);
            var fixedWeirGroupName = "myFixedWeir_fxw.pliz";
            var fixedWeirFileName = Path.Combine(mduDir, obsCrsGroupName);
            var thinDamGroupName = "myThinDam_thd.pliz";
            var thinDamFileName = Path.Combine(mduDir, obsCrsGroupName);
            try
            {
                var mduFile = new MduFile();
                var area = new HydroArea();
                area.ObservationCrossSections.Add(new ObservationCrossSection2D
                {
                    GroupName = obsCrsGroupName,
                    Geometry = new LineString(new[] {new Coordinate(0, 0), new Coordinate(0, 100)})
                });
                var fixedWeir = new FixedWeir
                {
                    GroupName = fixedWeirGroupName,
                    Geometry = new LineString(new[] { new Coordinate(0, 0), new Coordinate(0, 100) })
                };
                area.FixedWeirs.Add(fixedWeir);
                area.ThinDams.Add(new ThinDam2D
                {
                    GroupName = thinDamGroupName,
                    Geometry = new LineString(new[] { new Coordinate(0, 0), new Coordinate(0, 100) })
                });

                var modelDefinition = new WaterFlowFMModelDefinition(modelName);
                var allFixedWeirsAndCorrespondingProperties = new List<ModelFeatureCoordinateData<FixedWeir>> {new ModelFeatureCoordinateData<FixedWeir> {Feature = fixedWeir}};
                mduFile.Write(mduFilePath, modelDefinition, area, null, null, null, null, null,null, allFixedWeirsAndCorrespondingProperties);
                Assert.That(File.Exists(obsCrsFileName));
                Assert.That(File.Exists(fixedWeirFileName));
                Assert.That(File.Exists(thinDamFileName));
            }
            finally
            {
                FileUtils.DeleteIfExists(mduFilePath);
                FileUtils.DeleteIfExists(obsCrsFileName);
                FileUtils.DeleteIfExists(fixedWeirFileName);
                FileUtils.DeleteIfExists(thinDamFileName);
            }
        }

        [Test]
        public void GivenPolylineFeaturesWithPliExtension_WhenWritingMduFile_ThenFilesAreSavedAsPliFile()
        {
            var mduFilePath = string.Concat(Path.GetTempFileName(), ".mdu");
            var mduDir = Path.GetDirectoryName(mduFilePath);
            var modelName = Path.GetFileName(Path.GetFileNameWithoutExtension(mduFilePath));
            Assert.NotNull(mduDir);

            var obsCrsGroupName = "myObsCrossSection_crs.pli";
            var obsCrsFileName = Path.Combine(mduDir, obsCrsGroupName);
            var fixedWeirGroupName = "myFixedWeir_fxw.pli";
            var fixedWeirFileName = Path.Combine(mduDir, obsCrsGroupName);
            var thinDamGroupName = "myThinDam_thd.pli";
            var thinDamFileName = Path.Combine(mduDir, obsCrsGroupName);
            try
            {
                var mduFile = new MduFile();
                var area = new HydroArea();
                area.ObservationCrossSections.Add(new ObservationCrossSection2D
                {
                    GroupName = obsCrsGroupName,
                    Geometry = new LineString(new[] { new Coordinate(0, 0), new Coordinate(0, 100) })
                });
                var fixedWeir = new FixedWeir
                {
                    GroupName = fixedWeirGroupName,
                    Geometry = new LineString(new[] { new Coordinate(0, 0), new Coordinate(0, 100) })
                };
                area.FixedWeirs.Add(fixedWeir);
                area.ThinDams.Add(new ThinDam2D
                {
                    GroupName = thinDamGroupName,
                    Geometry = new LineString(new[] { new Coordinate(0, 0), new Coordinate(0, 100) })
                });

                var modelDefinition = new WaterFlowFMModelDefinition(modelName);
                var allFixedWeirsAndCorrespondingProperties = new List<ModelFeatureCoordinateData<FixedWeir>> {new ModelFeatureCoordinateData<FixedWeir> {Feature = fixedWeir}};

                mduFile.Write(mduFilePath, modelDefinition, area, null, null, null, null, null, null, allFixedWeirsAndCorrespondingProperties);
                Assert.That(File.Exists(obsCrsFileName));
                Assert.That(File.Exists(fixedWeirFileName));
                Assert.That(File.Exists(thinDamFileName));
            }
            finally
            {
                FileUtils.DeleteIfExists(mduFilePath);
                FileUtils.DeleteIfExists(obsCrsFileName);
                FileUtils.DeleteIfExists(fixedWeirFileName);
                FileUtils.DeleteIfExists(thinDamFileName);
            }
        }

        [Test]
        public void GivenFeatureGroupNameWithExtensionButNoFeatureIdentifier_WhenWriting_ThenFeatureIdentifierIsAddedToFileName()
        {
            var mduFilePath = string.Concat(Path.GetTempFileName(), ".mdu");
            var mduFile = new MduFile();
            var mduDir = Path.GetDirectoryName(mduFilePath);
            var modelName = Path.GetFileName(Path.GetFileNameWithoutExtension(mduFilePath));

            Assert.NotNull(mduDir);

            var area = new HydroArea();

            area.ObservationPoints.Add(WaterFlowFMMduFileTestHelper.GetNewObservationPoint2D("MyObsPoints.xyn", "ObsPoint01"));
            area.DryPoints.Add(WaterFlowFMMduFileTestHelper.GetNewGroupablePointFeature("MyDryPoints.xyz"));
            area.ObservationCrossSections.Add(new ObservationCrossSection2D
            {
                GroupName = modelName + ".pli",
                Name = "MyObsCrossSection",
                Geometry = new LineString(new[] { new Coordinate(0, 0), new Coordinate(0, 100) }),
                IsDefaultGroup = true
            });
            area.Gates.Add(new Gate2D
            {
                GroupName = "MyStructures.ini",
                Name = "MyGate",
                IsDefaultGroup = false
            });

            var obsGroupName = "MyObsPoints_obs.xyn";
            var dryGroupName = "MyDryPoints_dry.xyz";
            var crsGroupName = modelName + "_crs.pli";
            var gateGroupName = "MyStructures_structures.ini";

            var obsFilePath = Path.Combine(mduDir, obsGroupName);
            var dryFilePath = Path.Combine(mduDir, dryGroupName);
            var crsFilePath = Path.Combine(mduDir, crsGroupName);
            var gateFilePath = Path.Combine(mduDir, gateGroupName);
            FileUtils.DeleteIfExists(obsFilePath);
            FileUtils.DeleteIfExists(dryFilePath);
            FileUtils.DeleteIfExists(crsFilePath);
            FileUtils.DeleteIfExists(gateFilePath);

            var modelDefinition = new WaterFlowFMModelDefinition(modelName);
            var allFixedWeirsAndCorrespondingProperties = new List<ModelFeatureCoordinateData<FixedWeir>>();

            mduFile.Write(mduFilePath, modelDefinition, area, null, null, null, null, null, null, allFixedWeirsAndCorrespondingProperties);

            var obsFileEntries = modelDefinition.GetModelProperty(KnownProperties.ObsFile).Value as List<string>;
            Assert.NotNull(obsFileEntries);
            Assert.That(obsFileEntries.Count, Is.EqualTo(1));
            Assert.That(area.ObservationPoints.FirstOrDefault().GroupName, Is.EqualTo("MyObsPoints.xyn"));
            Assert.That(obsFileEntries.FirstOrDefault(), Is.EqualTo(obsGroupName));
            Assert.That(File.Exists(obsFilePath));

            var dryFileEntries = modelDefinition.GetModelProperty(KnownProperties.DryPointsFile).Value as List<string>;
            Assert.NotNull(dryFileEntries);
            Assert.That(dryFileEntries.Count, Is.EqualTo(1));
            Assert.That(area.DryPoints.FirstOrDefault().GroupName, Is.EqualTo("MyDryPoints.xyz"));
            Assert.That(dryFileEntries.FirstOrDefault(), Is.EqualTo(dryGroupName));
            Assert.That(File.Exists(dryFilePath));

            var crsFileEntries = modelDefinition.GetModelProperty(KnownProperties.ObsCrsFile).Value as List<string>;
            Assert.NotNull(crsFileEntries);
            Assert.That(crsFileEntries.Count, Is.EqualTo(1));
            Assert.That(area.ObservationCrossSections.FirstOrDefault().GroupName, Is.EqualTo(modelName + ".pli"));
            Assert.That(crsFileEntries.FirstOrDefault(), Is.EqualTo(crsGroupName));
            Assert.That(File.Exists(crsFilePath));

            FileUtils.DeleteIfExists(mduFilePath);
        }

        [Test]
        public void GivenMduFileWithReferencesThatIsSituatedInAFolderWithSpacesInItsName_WhenReadingMduFile_ThenNoProblemsOccur()
        {
            // Preparations
            const string baseFolderPath = @"HydroAreaCollection\MduFileProjects";
            var testDir = TestHelper.GetTestDataDirectoryPathForAssembly(GetType().Assembly);

            var originalFolderPath = Path.Combine(testDir, baseFolderPath);
            var relativePath = @"MduFileInFolderWith - SpacesInName\FlowFM.mdu";
            
            var testWorkingFolder = TestHelper.CreateLocalCopy(originalFolderPath);
            var mduPath = Path.Combine(testWorkingFolder, relativePath);
            var convertedFileObjectsForFMModel = CreateConvertedFileObjectsForFMModel(Path.GetFileNameWithoutExtension(mduPath));
            
            new MduFile().Read(mduPath, convertedFileObjectsForFMModel);
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void GivenPointSourceFileThatContainsOnePoint_WhenReadingMduFile_ThenSourceHasBeenImported()
        {
            // Preparations
            const string baseFolderPath = @"HydroAreaCollection\MduFileProjects";
            var testDir = TestHelper.GetTestDataDirectoryPathForAssembly(GetType().Assembly);

            var originalFolderPath = Path.Combine(testDir, baseFolderPath);
            var relativePath = @"MduFileWithSourceSinkFile\FlowFM.mdu";
            
            var testWorkingFolder = TestHelper.CreateLocalCopy(originalFolderPath);
            var mduPath = Path.Combine(testWorkingFolder, relativePath);

            var convertedFileObjectsForFMModel = CreateConvertedFileObjectsForFMModel(Path.GetFileNameWithoutExtension(mduPath));
            var modelDefinition = convertedFileObjectsForFMModel.ModelDefinition;
            
            Assert.That(modelDefinition.SourcesAndSinks.Count, Is.EqualTo(0));
            new MduFile().Read(mduPath, convertedFileObjectsForFMModel);
            Assert.That(modelDefinition.SourcesAndSinks.Count, Is.EqualTo(1));
        }
        
         [Test]
        public void GivenNonZeroStartAndStopDateTimes_StartTimeAndStopTimeRelativeToReferenceDateAreSavedAndRestoredCorrectly()
        {
            var pathWithoutExtension = Path.GetTempFileName();
            var mduFilePath = string.Concat(pathWithoutExtension, ".mdu");
            var mduFile = new MduFile();
            var mduDir = Path.GetDirectoryName(mduFilePath);
            var modelName = Path.GetFileName(mduFilePath);

            Assert.NotNull(mduDir);
            
            using (var model = new WaterFlowFMModel(mduFilePath){ Area = new HydroArea()})
            {
                var area = model.Area;

                var modelDefinition = new WaterFlowFMModelDefinition(modelName);
                var allFixedWeirsAndCorrespondingProperties = new List<ModelFeatureCoordinateData<FixedWeir>>();

                DateOnly refDate = new DateOnly(1966, 12, 10);
                DateTime startTime = new DateTime(1966,12,10,0,02,00);
                long relativeStartTimeInSeconds = 2 * 60;
                DateTime stopTime = new DateTime(1966,12,10,0,04,00);
                long relativeStopTimeInSeconds = 4 * 60;
                modelDefinition.GetModelProperty(KnownProperties.RefDate).Value = refDate;
                modelDefinition.GetModelProperty(GuiProperties.StartTime).Value = startTime;
                modelDefinition.GetModelProperty(GuiProperties.StopTime).Value = stopTime;
                
                // Mimic what MduFile.Write does to calculate the TStart and TStop
                modelDefinition.SetMduTimePropertiesFromGuiProperties();
                Assert.Multiple(() =>
                    {
                        Assert.AreEqual(relativeStartTimeInSeconds, modelDefinition.GetModelProperty(KnownProperties.TStart).Value);
                        Assert.AreEqual(relativeStopTimeInSeconds, modelDefinition.GetModelProperty(KnownProperties.TStop).Value);
                    }
                );
                
                // Write the stuff to file and read it back into another model
                mduFile.Write(mduFilePath, modelDefinition, area, null, null,null, null, null, null, allFixedWeirsAndCorrespondingProperties);

                var newMduFile = new MduFile();
                using (new WaterFlowFMModel(mduFilePath) { Area = area })
                {
                    var convertedFileObjectsForFMModel = CreateConvertedFileObjectsForFMModel(modelName);
                    var newModelDefinition = convertedFileObjectsForFMModel.ModelDefinition;
                    convertedFileObjectsForFMModel.HydroArea = area;
                    convertedFileObjectsForFMModel.AllFixedWeirsAndCorrespondingProperties = allFixedWeirsAndCorrespondingProperties;
                    
                    newMduFile.Read(mduFilePath, convertedFileObjectsForFMModel);

                    // Expect failing assertions for refDate, startTime and stopTime
                    Assert.Multiple(() =>
                    {
                        Assert.AreEqual(refDate, newModelDefinition.GetModelProperty(KnownProperties.RefDate).Value);
                        Assert.AreEqual(relativeStartTimeInSeconds, newModelDefinition.GetModelProperty(KnownProperties.TStart).Value);
                        Assert.AreEqual(relativeStopTimeInSeconds, newModelDefinition.GetModelProperty(KnownProperties.TStop).Value);
                        Assert.AreEqual(startTime,newModelDefinition.GetModelProperty(GuiProperties.StartTime).Value);
                        Assert.AreEqual(stopTime,newModelDefinition.GetModelProperty(GuiProperties.StopTime).Value);
                    });
                }
            }
            FileUtils.DeleteIfExists(mduFilePath);
        }

        #region TestHelpers
        
        private static ConvertedFileObjectsForFMModel CreateConvertedFileObjectsForFMModel(string modelName = null)
        {
            return new ConvertedFileObjectsForFMModel
            {
                HydroArea = new HydroArea(),
                HydroNetwork = new HydroNetwork(),
                ModelDefinition = new WaterFlowFMModelDefinition(modelName),
                BoundaryConditions1D = new EventedList<Model1DBoundaryNodeData>(),
                LateralSourcesData = new EventedList<Model1DLateralSourceData>(),
                AllFixedWeirsAndCorrespondingProperties = new List<ModelFeatureCoordinateData<FixedWeir>>(),
                AllBridgePillarsAndCorrespondingProperties = new List<ModelFeatureCoordinateData<BridgePillar>>(),
                RoughnessSections = new EventedList<RoughnessSection>(),
                ChannelFrictionDefinitions = new EventedList<ChannelFrictionDefinition>(),
                ChannelInitialConditionDefinitions = new EventedList<ChannelInitialConditionDefinition>()
            };
        }

        private void CompareHydroAreaModelProperties(string propertyName, string saveMduFilePath, WaterFlowFMModelDefinition expectedMD,
            WaterFlowFMModelDefinition savedMD)
        {
            var expectedProp = expectedMD.GetModelProperty(propertyName);
            var savedProp = savedMD.GetModelProperty(propertyName);
            Assert.IsNotNull(expectedProp, "Wrong property name? {0}", propertyName);
            Assert.IsNotNull(savedProp, "Wrong property name? {0}", propertyName);

            Assert.AreEqual(expectedProp.GetValueAsString(), savedProp.GetValueAsString());
            CheckFeatureFilesWereCreated(saveMduFilePath, propertyName, savedMD);
        }

        private void CheckFeatureFilesWereCreated(string mduFilePath, string propertyName, WaterFlowFMModelDefinition modelDefinition)
        {
            var files = MduFileHelper.GetMultipleSubfilePath(mduFilePath, modelDefinition.GetModelProperty(propertyName));
            var notCreatedFiles = files.Where(f => !File.Exists(f) || string.IsNullOrEmpty(File.ReadAllText(f)));
            Assert.IsEmpty(notCreatedFiles, "The following files have not been created, or were not found at their expected path {0}", notCreatedFiles);
        }

        private void CheckFeatureWasReadCorrectly<TFeature>(IEnumerable<TFeature> areaFeature, string featureName,
            List<string> expectedGroupNames)
        {
            var asGroupable = areaFeature as IEnumerable<IGroupableFeature>;
            if (!typeof(TFeature).Implements(typeof(IGroupableFeature))
                || asGroupable == null)
            {
                Assert.Fail("Feature {0} is not GroupableFeature", featureName);
            }

            var featureGrouped = asGroupable.GroupBy(g => g.GroupName).ToList();
            var readGroups = featureGrouped.Select(g => g.Key).ToList();
            Assert.AreEqual(expectedGroupNames.Count, featureGrouped.Count, 
                String.Format("Feature {0}. Expected groupNames {1}, generated {2}", featureName, expectedGroupNames, readGroups));

            foreach (var expectedGroupName in expectedGroupNames)
            {
                Assert.IsTrue(readGroups.Contains(expectedGroupName),
                    "Feature {0}, expected group: {1} but not found in {2}", featureName, expectedGroupName, readGroups);
            }
        }

        private static void CheckDefaultGroupIsInFeature(string featureName, IEnumerable<IGroupableFeature> feature, string savePath, string featureExtension)
        {
            var grouped = feature.GroupBy(g => g.GroupName).ToList();
            var groupNames = grouped.Select(g => g.Key).ToList();
            Assert.IsTrue(groupNames.Any(g => g.Replace(featureExtension, string.Empty).Trim().Equals(savePath)), 
                "Feature {0} did not save default group {1}, instead: {2}", featureName, savePath, groupNames.ToList());
        }

        private static void CheckDryPointsFeature(HydroArea originalArea, HydroArea savedArea)
        {
            var expectedList = originalArea.DryPoints.ToList();
            var savedList = savedArea.DryPoints.ToList();
            Assert.AreEqual(expectedList.Count, savedList.Count,
                "Expected dry points {0}, saved {1}", expectedList.Count, savedList.Count);

            var expectedGroups = expectedList.GroupBy(g => g.GroupName).ToList();
            var expectedGroupNames = expectedGroups.Select(g => g.Key).ToList();
            var savedGroups = savedList.GroupBy(g => g.GroupName).ToList();
            var savedGroupNames = savedGroups.Select(g => g.Key).ToList();
            Assert.AreEqual(expectedGroupNames.Count, savedGroups.Select(g => g.Key).Count(),
                "Group names differ from the original read ones. Original: {0}, Saved {1}", expectedGroupNames,
                savedGroupNames);

            Assert.IsFalse(expectedList.Any(dryPoint => savedList.Contains(dryPoint)));
            Assert.IsFalse(savedList.Any(dryPoint => expectedList.Contains(dryPoint)));
        }


        private static WaterFlowFMProperty AddCustomMultipleFilePropertyToModelDefinition(
            WaterFlowFMModelDefinition modelDefinition, string propertyName, string fileIniSectionName)
        {
            var propertyDefinition = new WaterFlowFMPropertyDefinition
            {
                MduPropertyName = propertyName,
                FileSectionName = fileIniSectionName,
                DataType = typeof(IList<string>),
                IsMultipleFile = true
            };
            modelDefinition.AddProperty(new WaterFlowFMProperty(propertyDefinition, string.Empty));
            Assert.IsTrue(modelDefinition.ContainsProperty(propertyDefinition.MduPropertyName.ToLower()));

            var property = modelDefinition.GetModelProperty(propertyName);
            Assert.IsNotNull(property);

            return property;
        }

        private static void RemoveGroupNameFromGroupableFeature(IEnumerable<IGroupableFeature> feature)
        {
            var grouped = feature.GroupBy(e => e.GroupName);
            grouped.First().ForEach(g => g.GroupName = string.Empty);
        }

        private static void DeleteAllFilesAndFoldersInSubDirectory(DirectoryInfo directoryInfo)
        {
            foreach (var file in directoryInfo.GetFiles())
            {
                file.Delete();
            }

            foreach (var dir in directoryInfo.GetDirectories())
            {
                DeleteAllFilesAndFoldersInSubDirectory(dir);
                dir.Delete();
            }
        }
        
        private static void CheckIfFilesAreNotCopied(string fromFolder, string toFolder)
        {
            IEnumerable<string> toFolderFileNames = new DirectoryInfo(toFolder).GetFiles().Select(f => f.Name);
            IEnumerable<string> fromFolderFileNames = new DirectoryInfo(fromFolder)
                                                      .GetFiles()
                                                      .Select(f => f.Name)
                                                      .Where(n => !n.EndsWith("_structures.ini") && 
                                                                  !n.Contains("gate") &&
                                                                  !n.Contains("pump") && 
                                                                  !n.Contains("weir"));
            foreach (string fileName in fromFolderFileNames)
            {
                Assert.That(toFolderFileNames.Contains(fileName), Is.False);
            }
        }

        #endregion
    }
}