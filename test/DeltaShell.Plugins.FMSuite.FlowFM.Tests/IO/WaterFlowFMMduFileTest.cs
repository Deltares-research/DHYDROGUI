using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using DelftTools.Hydro;
using DelftTools.Hydro.Area.Objects;
using DelftTools.Hydro.Area.Objects.StructureObjects;
using DelftTools.Hydro.GroupableFeatures;
using DelftTools.TestUtils;
using DelftTools.Utils.Collections;
using DelftTools.Utils.Collections.Generic;
using DelftTools.Utils.IO;
using DelftTools.Utils.Reflection;
using DeltaShell.Plugins.FMSuite.Common.IO;
using DeltaShell.Plugins.FMSuite.FlowFM.FeatureData;
using DeltaShell.Plugins.FMSuite.FlowFM.IO;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Files;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Files.Helpers;
using DeltaShell.Plugins.FMSuite.FlowFM.Model;
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
            string mduFilePath = string.Concat(Path.GetTempFileName(), ".mdu");

            try
            {
                var mduFile = new MduFile();
                var modelDefinition = new WaterFlowFMModelDefinition();
                modelDefinition.WriteSnappedFeatures = true;

                //Write

                var mduFileWriteConfig = new MduFileWriteConfig
                {
                    WriteExtForcings = false,
                    WriteFeatures = false
                };

                mduFile.WriteProperties(mduFilePath,
                                        modelDefinition.Properties,
                                        mduFileWriteConfig);
                string readAllText = File.ReadAllText(mduFilePath);

                foreach (string prop in modelDefinition.KnownWriteOutputSnappedFeatures)
                {
                    WaterFlowFMMduFileTestHelper.AssertContainsMduLine(readAllText, prop, "1");
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

            IList<string> subFileList = MduFileHelper.GetMultipleSubfilePath("mduTestPath", property);
            Assert.AreEqual(2, subFileList.Count);
        }

        [Test]
        [Category(TestCategory.Jira)] // See issue D3DFMIQ-1462, only second test fails.
        [TestCase("geometry", "MultipleLinePropertiesTestFile", "Test1 Test2", "# Test comment 1", "= Test1 Test2 # Test comment 1")]
        [TestCase("geometry", "MultipleLinePropertiesTestFile", "Test1 Test2", "# Test comment 1 Test comment 2", "=Test1 \\ # Test comment 1\r\nTest2 # Test comment 2")] /* Slash separated */
        public void MduFileReadsAndWritesMultipleLinePropertiesIncludingComments(string sectionName, string propertyName, string expectedValues, string expectedOutputComments, string rawValuesAndComments)
        {
            string nameWithoutExtension = Path.GetTempFileName();
            string mduFilePath = string.Concat(nameWithoutExtension, ".mdu");

            string mduFileText = string.Concat(propertyName, rawValuesAndComments);
            mduFileText = string.Concat("[", sectionName, "]", "\n", mduFileText);
            File.WriteAllText(mduFilePath, mduFileText);
            try
            {
                var mduFile = new MduFile();
                var modelDefinition = new WaterFlowFMModelDefinition();
                var allFixedWeirsAndCorrespondingProperties = new Dictionary<FixedWeir, ModelFeatureCoordinateData<FixedWeir>>();

                mduFile.Read(mduFilePath, modelDefinition, new HydroArea(), allFixedWeirsAndCorrespondingProperties);

                //Write
                var mduFileWriteConfig = new MduFileWriteConfig
                {
                    WriteExtForcings = false,
                    WriteFeatures = false
                };

                mduFile.WriteProperties(mduFilePath,
                                        modelDefinition.Properties,
                                        mduFileWriteConfig);

                string readAllText = File.ReadAllText(mduFilePath);

                WaterFlowFMMduFileTestHelper.AssertContainsMduLine(readAllText, propertyName, expectedValues, expectedOutputComments);
            }
            finally
            {
                FileUtils.DeleteIfExists(mduFilePath);
            }
        }

        [Test]
        [Category(TestCategory.Jira)] // See issue D3DFMIQ-1462
        [TestCase("geometry", "CustomProperty1", "CustomProperty2", "Test1 Test2", "Test3", true, false)]
        public void MduFileHandlesWrongDeclarationsOfMultipleLineProperties(string sectionName, string property1Name, string property2Name, string property1Value, string property2Value, bool multipleLineProp1, bool multipleLineProp2)
        {
            string nameWithoutExtension = Path.GetTempFileName();
            string mduFilePath = string.Concat(nameWithoutExtension, ".mdu");
            string property1Text = string.Concat(property1Name, "=", property1Value,
                                                 multipleLineProp1 ? @"\" : string.Empty);
            string property2Text = string.Concat(property2Name, "=", property2Value,
                                                 multipleLineProp2 ? @"\" : string.Empty);
            string mduFileText = string.Concat(property1Text, "\n", property2Text);
            mduFileText = string.Concat("[", sectionName, "]", "\n", mduFileText);
            File.WriteAllText(mduFilePath, mduFileText);
            try
            {
                var mduFile = new MduFile();
                var modelDefinition = new WaterFlowFMModelDefinition();

                WaterFlowFMProperty property1 = AddCustomMultipleFilePropertyToModelDefinition(modelDefinition, property1Name, sectionName);
                WaterFlowFMProperty property2 = AddCustomMultipleFilePropertyToModelDefinition(modelDefinition, property2Name, sectionName);
                //Read
                mduFile.Read(mduFilePath, modelDefinition, new HydroArea(), new Dictionary<FixedWeir, ModelFeatureCoordinateData<FixedWeir>>());
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
            string pathWithoutExtension = Path.GetTempFileName();
            string mduFilePath = string.Concat(pathWithoutExtension, ".mdu");
            var mduFile = new MduFile();
            string mduDir = Path.GetDirectoryName(mduFilePath);
            string modelName = Path.GetFileName(mduFilePath);

            Assert.NotNull(mduDir);

            string defaultNameWE = string.Concat(Path.GetFileName(pathWithoutExtension), FileConstants.ObsPointFileExtension);
            string group1NameWE = string.Concat("Group1", FileConstants.ObsPointFileExtension);
            string fileObsPointsDefault = Path.Combine(mduDir, defaultNameWE);
            string fileObsPointsGroup1 = Path.Combine(mduDir, group1NameWE);
            using (var model = new WaterFlowFMModel())
            {
                model.ImportFromMdu(mduFilePath);

                model.Area = new HydroArea();

                HydroArea area = model.Area;
                /*Observation points, we create 2 with keys and one default. Thus, three expected output files*/
                area.ObservationPoints.AddRange(
                    new[]
                    {
                        WaterFlowFMMduFileTestHelper.GetNewGroupableFeature2DPoint(string.Empty, "Feature1") /*Default group expected*/
                    }
                );
                GroupableFeature2DPoint defaultFeature = area.ObservationPoints.FirstOrDefault(o => o.Name == "Feature1");
                Assert.IsNotNull(defaultFeature);

                var modelDefinition = new WaterFlowFMModelDefinition(mduDir, modelName);
                var allFixedWeirsAndCorrespondingProperties = new List<ModelFeatureCoordinateData<FixedWeir>>();
                mduFile.Write(mduFilePath, modelDefinition, area, allFixedWeirsAndCorrespondingProperties);
                //After writing the default groups get updated.
                Assert.IsTrue(defaultFeature.IsDefaultGroup);
                //Now rename the group name and save again.
                defaultFeature.GroupName = group1NameWE;
                mduFile.Write(mduFilePath, modelDefinition, area, allFixedWeirsAndCorrespondingProperties);

                Assert.AreEqual(group1NameWE, defaultFeature.GroupName);
                Assert.IsTrue(File.Exists(mduFilePath));
                string readAllText = File.ReadAllText(mduFilePath);
                WaterFlowFMMduFileTestHelper.AssertContainsMduLine(readAllText, "ObsFile", string.Join(" ", group1NameWE));
                Assert.IsTrue(File.Exists(fileObsPointsDefault));
                Assert.IsTrue(File.Exists(fileObsPointsGroup1));
            }

            FileUtils.DeleteIfExists(mduFilePath);
            FileUtils.DeleteIfExists(fileObsPointsDefault);
            FileUtils.DeleteIfExists(fileObsPointsGroup1);
        }

        [Test]                        /* Extension of the one above but directly loading an MDU File. */
        [Category(TestCategory.Jira)] // See issue D3DFMIQ-1462
        [TestCase(KnownProperties.EnclosureFile, "Value1 Value2", "CustomPropertyTest", "Value3")]
        public void WhenMduExpectsANewMultipleLinePropertyButItIsANewPropertyItKeepsReading(string hydroAreaFileProperty, string expectedCompositeValue, string customPropertyName, string expectedSimpleValue)
        {
            string mduFilePath = TestHelper.GetTestFilePath(@"HydroAreaCollection\FlowFMPropertyWithSlashAndNoNewLine.mdu");
            mduFilePath = TestHelper.CreateLocalCopy(mduFilePath);
            string mduDir = Path.GetDirectoryName(mduFilePath);
            string modelName = Path.GetFileName(mduFilePath);

            var area = new HydroArea();
            var modelDefinition = new WaterFlowFMModelDefinition(mduDir, modelName);
            var allFixedWeirsAndCorrespondingProperties = new Dictionary<FixedWeir, ModelFeatureCoordinateData<FixedWeir>>();
            var mduFile = new MduFile();

            mduFile.Read(mduFilePath, modelDefinition, area, allFixedWeirsAndCorrespondingProperties);

            WaterFlowFMProperty property = modelDefinition.GetModelProperty(hydroAreaFileProperty);
            Assert.IsNotNull(property);
            Assert.AreEqual(expectedCompositeValue, property.GetValueAsString());

            WaterFlowFMProperty customProperty = modelDefinition.GetModelProperty(customPropertyName);
            Assert.IsNotNull(customProperty);
            Assert.AreEqual(expectedSimpleValue, customProperty.GetValueAsString());

            FileUtils.DeleteIfExists(mduFilePath);
        }

        [Test]
        [Category(TestCategory.Jira)] // See issue D3DFMIQ-1462
        public void MduFileReadsFromMultipleFilesAnAssignsGroupNamesToIGroupableFeatures()
        {
            string mduFilePath = TestHelper.GetTestFilePath(@"HydroAreaCollection\FlowFM\FlowFM.mdu");
            mduFilePath = TestHelper.CreateLocalCopy(mduFilePath);
            string mduDir = Path.GetDirectoryName(mduFilePath);
            string modelName = Path.GetFileName(mduFilePath);
            try
            {
                var area = new HydroArea();
                var modelDefinition = new WaterFlowFMModelDefinition(mduDir, modelName);
                var allFixedWeirsAndCorrespondingProperties = new Dictionary<FixedWeir, ModelFeatureCoordinateData<FixedWeir>>();
                var mduFile = new MduFile();

                mduFile.Read(mduFilePath, modelDefinition, area, allFixedWeirsAndCorrespondingProperties);

                //  2 groups per each feature
                CheckFeatureWasReadCorrectly(area.ObservationPoints, "ObservationPoints", new List<string>
                {
                    "ObsGroup1_obs.xyn",
                    "ObsGroup2_obs.xyn"
                });
                CheckFeatureWasReadCorrectly(area.Enclosures, "Enclosures", new List<string>
                {
                    "EncGroup1_enc.pol",
                    "EncGroup2_enc.pol"
                });
                /* Dry Points and dry areas are exclusive  XyzFile dryPointFile;*/
                CheckFeatureWasReadCorrectly(area.DryAreas, "DryAreas", new List<string>
                {
                    "DryGroup1_dry.pol",
                    "DryGroup2_dry.pol"
                });
                CheckFeatureWasReadCorrectly(area.ThinDams, "ThinDams", new List<string>
                {
                    "ThdGroup1_thd.pli",
                    "ThdGroup2_thd.pli"
                });
                CheckFeatureWasReadCorrectly(area.FixedWeirs, "FixedWeirs", new List<string>
                {
                    "FxwGroup1_fxw.pli",
                    "FxwGroup2_fxw.pli"
                });
                CheckFeatureWasReadCorrectly(area.ObservationCrossSections, "ObservationCrossSections", new List<string>
                {
                    "CrsGroup1_crs.pli",
                    "CrsGroup2_crs.pli"
                });
                CheckFeatureWasReadCorrectly(area.LandBoundaries, "LandBoundaries", new List<string>
                {
                    "LdbGroup1.ldb",
                    "LdbGroup2.ldb"
                });

                /* StructuresFile 
                 * We CAN read from multiple structures files, however these structures will not get the GroupName due to its implementation nature.
                 */
                var structuresGroupNames =
                    new List<string>
                    {
                        "StructuresGroup1_structures.ini",
                        "StructuresGroup2_structures.ini"
                    };

                CheckFeatureWasReadCorrectly(area.Structures, "Weirs", structuresGroupNames);
                CheckFeatureWasReadCorrectly(area.Pumps, "Pumps", structuresGroupNames);
            }
            finally
            {
                FileUtils.DeleteIfExists(mduFilePath);
            }
        }

        [Test]
        public void MduFileReadsFromMultipleFilesAnAssignsGroupNamesToXyZFileDryPointFeature()
        {
            string mduFilePath = TestHelper.GetTestFilePath(@"HydroAreaCollection\FlowFM\dryPointsMdu.mdu");
            mduFilePath = TestHelper.CreateLocalCopy(mduFilePath);
            string mduDir = Path.GetDirectoryName(mduFilePath);
            string modelName = Path.GetFileName(mduFilePath);
            try
            {
                var area = new HydroArea();
                var modelDefinition = new WaterFlowFMModelDefinition(mduDir, modelName);
                var allFixedWeirsAndCorrespondingProperties = new Dictionary<FixedWeir, ModelFeatureCoordinateData<FixedWeir>>();
                var mduFile = new MduFile();

                mduFile.Read(mduFilePath, modelDefinition, area, allFixedWeirsAndCorrespondingProperties);
                CheckFeatureWasReadCorrectly(area.DryPoints, "DryAreas (points)", new List<string>
                {
                    "dryGroup1_dry.xyz",
                    "dryGroup2_dry.xyz"
                });
            }
            finally
            {
                FileUtils.DeleteIfExists(mduFilePath);
            }
        }

        [Test]
        public void WritePropertyWhenMultipleFileIsTrue()
        {
            string nameWithoutExtension = Path.GetTempFileName();
            string mduFilePath = string.Concat(nameWithoutExtension, ".mdu");
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

            var mduFileWriteConfig = new MduFileWriteConfig
            {
                WriteExtForcings = false,
                WriteFeatures = false
            };

            mduFile.WriteProperties(mduFilePath,
                                    new List<WaterFlowFMProperty>() { property },
                                    mduFileWriteConfig);

            Assert.IsTrue(File.Exists(mduFilePath));
            string readAllText = File.ReadAllText(mduFilePath);
            WaterFlowFMMduFileTestHelper.AssertContainsMduLine(readAllText, property.PropertyDefinition.MduPropertyName, "Test1 Test2");

            FileUtils.DeleteIfExists(mduFilePath);
        }

        [Test]
        public void MduFileWritesOneFilePerGroupDeclaredInTheMduAndAMultipleValueInTheMduProperty()
        {
            string pathWithoutExtension = Path.GetTempFileName();
            string mduFilePath = string.Concat(pathWithoutExtension, ".mdu");
            var mduFile = new MduFile();
            string mduDir = Path.GetDirectoryName(mduFilePath);
            string modelName = Path.GetFileName(mduFilePath);

            Assert.NotNull(mduDir);

            var ObsExtension = "_obs.xyn";
            string defaultNameWE = string.Concat(Path.GetFileName(pathWithoutExtension), ObsExtension);
            string group1NameWE = string.Concat("Group1", ObsExtension);
            string group2NameWE = string.Concat("Group2", ObsExtension);
            string fileObsPointsDefault = Path.Combine(mduDir, defaultNameWE);
            string fileObsPointsGroup1 = Path.Combine(mduDir, group1NameWE);
            string fileObsPointsGroup2 = Path.Combine(mduDir, group2NameWE);
            try
            {
                var area = new HydroArea();
                /*Observation points, we create 2 with keys and one default. Thus, three expected output files*/
                area.ObservationPoints.AddRange(
                    new[]
                    {
                        WaterFlowFMMduFileTestHelper.GetNewGroupableFeature2DPoint("", "Feature1"), /*Default group expected*/
                        WaterFlowFMMduFileTestHelper.GetNewGroupableFeature2DPoint("Group1", "Feature2"),
                        WaterFlowFMMduFileTestHelper.GetNewGroupableFeature2DPoint("Group1", "Feature3"),
                        WaterFlowFMMduFileTestHelper.GetNewGroupableFeature2DPoint("Group2", "Feature4")
                    }
                );
                GroupableFeature2DPoint defaultFeature = area.ObservationPoints.FirstOrDefault(o => o.Name == "Feature1");
                Assert.IsNotNull(defaultFeature);

                var modelDefinition = new WaterFlowFMModelDefinition(mduDir, modelName);
                var allFixedWeirsAndCorrespondingProperties = new List<ModelFeatureCoordinateData<FixedWeir>>();
                mduFile.Write(mduFilePath, modelDefinition, area, allFixedWeirsAndCorrespondingProperties);
                //After writing the default groups get updated.
                Assert.IsTrue(defaultFeature.IsDefaultGroup);

                Assert.IsTrue(File.Exists(mduFilePath));
                string readAllText = File.ReadAllText(mduFilePath);
                WaterFlowFMMduFileTestHelper.AssertContainsMduLine(readAllText, "ObsFile", string.Join(" ", defaultNameWE, group1NameWE, group2NameWE));
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
        public void GivenMduFileWithDryPointsAndDryAreasFilesBothOnDryPointsFileSection_WhenReadingMdu_ThenBothFilesAreCorrectlyRead()
        {
            string mduFilePath = TestHelper.GetTestFilePath(@"HydroAreaCollection\FlowFM\dryPointsAndAreasInModel.mdu");
            mduFilePath = TestHelper.CreateLocalCopy(mduFilePath);
            string mduDir = Path.GetDirectoryName(mduFilePath);
            Assert.NotNull(mduDir);
            string modelName = Path.GetFileName(mduFilePath);

            var mduFile = new MduFile();
            var area = new HydroArea();
            var modelDefinition = new WaterFlowFMModelDefinition(mduDir, modelName);
            var allFixedWeirsAndCorrespondingProperties = new Dictionary<FixedWeir, ModelFeatureCoordinateData<FixedWeir>>();
            mduFile.Read(mduFilePath, modelDefinition, area, allFixedWeirsAndCorrespondingProperties);

            Assert.That(area.DryPoints.Count, Is.EqualTo(6));
            Assert.That(area.DryAreas.Count, Is.EqualTo(2));
        }

        [Test]
        public void GivenMduFileWithOnlyOneDryPointsFileReferenceInMdu_WhenReadingMdu_ThenFileIsCorrectlyRead()
        {
            string mduFilePath = TestHelper.GetTestFilePath(@"HydroAreaCollection\FlowFM\OnlyDryPointsInModel.mdu");
            mduFilePath = TestHelper.CreateLocalCopy(mduFilePath);
            string mduDir = Path.GetDirectoryName(mduFilePath);
            Assert.NotNull(mduDir);
            string modelName = Path.GetFileName(mduFilePath);

            var mduFile = new MduFile();
            var area = new HydroArea();
            var modelDefinition = new WaterFlowFMModelDefinition(mduDir, modelName);
            var allFixedWeirsAndCorrespondingProperties = new Dictionary<FixedWeir, ModelFeatureCoordinateData<FixedWeir>>();
            mduFile.Read(mduFilePath, modelDefinition, area, allFixedWeirsAndCorrespondingProperties);

            Assert.That(area.DryPoints.Count, Is.EqualTo(3));
            Assert.IsEmpty(area.DryAreas); //Check this, because dry areas and dry points are read in the same method (MduFile.ReadDryPointsAndDryAreas)
        }

        [Test]
        public void GivenMduFileWithOnlyOneDryAreasFileReferenceInMdu_WhenReadingMdu_ThenFileIsCorrectlyRead()
        {
            string mduFilePath = TestHelper.GetTestFilePath(@"HydroAreaCollection\FlowFM\OnlyDryAreasInModel.mdu");
            mduFilePath = TestHelper.CreateLocalCopy(mduFilePath);
            string mduDir = Path.GetDirectoryName(mduFilePath);
            Assert.NotNull(mduDir);
            string modelName = Path.GetFileName(mduFilePath);

            var mduFile = new MduFile();
            var area = new HydroArea();
            var modelDefinition = new WaterFlowFMModelDefinition(mduDir, modelName);
            var allFixedWeirsAndCorrespondingProperties = new Dictionary<FixedWeir, ModelFeatureCoordinateData<FixedWeir>>();
            mduFile.Read(mduFilePath, modelDefinition, area, allFixedWeirsAndCorrespondingProperties);

            Assert.IsEmpty(area.DryPoints); //Check this, because dry areas and dry points are read in the same method (MduFile.ReadDryPointsAndDryAreas)
            Assert.That(area.DryAreas.Count, Is.EqualTo(1));
        }

        [Test]                        /* Roundtrip test */
        [Category(TestCategory.Jira)] // See issue D3DFMIQ-1462
        public void MduFileReadsAndWritesIGroupableFeatures()
        {
            string mduFilePath = TestHelper.GetTestFilePath(@"HydroAreaCollection\FlowFM\FlowFM.mdu");
            mduFilePath = TestHelper.CreateLocalCopy(mduFilePath);
            string mduDir = Path.GetDirectoryName(mduFilePath);
            Assert.NotNull(mduDir);
            string modelName = Path.GetFileName(mduFilePath);

            string saveDirectory = Path.Combine(mduDir, "MduFileReadsAndWritesTest");
            Directory.CreateDirectory(saveDirectory);
            string savePath = Path.Combine(saveDirectory, "SaveFlowFM.mdu");
            string newMduDir = Path.GetDirectoryName(savePath);
            string newMduName = Path.GetFileName(savePath);
            try
            {
                var mduFile = new MduFile();

                var originalArea = new HydroArea();
                var originalMD = new WaterFlowFMModelDefinition(mduDir, modelName);
                var allFixedWeirsAndCorrespondingProperties = new Dictionary<FixedWeir, ModelFeatureCoordinateData<FixedWeir>>();
                mduFile.Read(mduFilePath, originalMD, originalArea, allFixedWeirsAndCorrespondingProperties);
                mduFile.Write(savePath, originalMD, originalArea, allFixedWeirsAndCorrespondingProperties.Values, switchTo: false);

                var savedArea = new HydroArea();
                var savedMD = new WaterFlowFMModelDefinition(newMduDir, newMduName);
                mduFile.Read(savePath, savedMD, savedArea, allFixedWeirsAndCorrespondingProperties);

                //Check MDU property.
                var listOfProperties = new List<string>()
                {
                    KnownProperties.EnclosureFile,
                    KnownProperties.ObsFile,
                    KnownProperties.LandBoundaryFile,
                    KnownProperties.ThinDamFile,
                    KnownProperties.FixedWeirFile,
                    KnownProperties.StructuresFile,
                    KnownProperties.ObsCrsFile,
                    KnownProperties.DryPointsFile,
                    KnownProperties.StructuresFile
                };

                foreach (string property in listOfProperties)
                {
                    CompareHydroAreaModelProperties(property, savePath, originalMD, savedMD);
                }

                CompareHydroAreaFeatures(originalArea, savedArea);
            }
            finally
            {
                FileUtils.DeleteIfExists(mduFilePath);
                FileUtils.DeleteIfExists(savePath);
                FileUtils.DeleteIfExists(saveDirectory);
            }
        }

        [Test]
        public void MduFileReadsAndWritesXyzFileDryPointFeature()
        {
            string mduFilePath = TestHelper.GetTestFilePath(@"HydroAreaCollection\FlowFM\dryPointsMdu.mdu");
            mduFilePath = TestHelper.CreateLocalCopy(mduFilePath);
            string mduDir = Path.GetDirectoryName(mduFilePath);
            Assert.NotNull(mduDir);
            string modelName = Path.GetFileName(mduFilePath);

            string saveDirectory = Path.Combine(mduDir, "MduFileReadsAndWritesTest");
            Directory.CreateDirectory(saveDirectory);
            string savePath = Path.Combine(saveDirectory, "SaveDryPoint.mdu");
            string newMduDir = Path.GetDirectoryName(savePath);
            string newMduName = Path.GetFileName(savePath);
            try
            {
                var mduFile = new MduFile();

                var originalArea = new HydroArea();
                var originalMd = new WaterFlowFMModelDefinition(mduDir, modelName);
                var allFixedWeirsAndCorrespondingProperties = new Dictionary<FixedWeir, ModelFeatureCoordinateData<FixedWeir>>();
                mduFile.Read(mduFilePath, originalMd, originalArea, allFixedWeirsAndCorrespondingProperties);

                var mduFileWriteConfig = new MduFileWriteConfig { WriteExtForcings = false };

                mduFile.Write(savePath,
                              originalMd,
                              originalArea,
                              allFixedWeirsAndCorrespondingProperties.Values,
                              mduFileWriteConfig,
                              false);

                var savedArea = new HydroArea();
                var savedMd = new WaterFlowFMModelDefinition(newMduDir, newMduName);
                mduFile.Read(savePath, savedMd, savedArea, allFixedWeirsAndCorrespondingProperties);

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

        [Test]                        /* Roundtrip test */
        [Category(TestCategory.Jira)] // See issue D3DFMIQ-1462
        public void MduFileWritesDefaultValueForIGroupableFeatures()
        {
            string mduFilePath = TestHelper.GetTestFilePath(@"HydroAreaCollection\FlowFM\FlowFM.mdu");
            mduFilePath = TestHelper.CreateLocalCopy(mduFilePath);
            string mduDir = Path.GetDirectoryName(mduFilePath);
            Assert.NotNull(mduDir);
            string modelName = Path.GetFileName(mduFilePath);

            string saveDirectory = Path.Combine(mduDir, "MduFileReadsAndWritesTest");
            Directory.CreateDirectory(saveDirectory);
            string savePath = Path.Combine(saveDirectory, "SaveFlowFM.mdu");
            string newMduDir = Path.GetDirectoryName(savePath);
            string newMduName = Path.GetFileName(savePath);
            try
            {
                var mduFile = new MduFile();

                var originalArea = new HydroArea();
                var originalMD = new WaterFlowFMModelDefinition(mduDir, modelName);
                var allFixedWeirsAndCorrespondingProperties = new Dictionary<FixedWeir, ModelFeatureCoordinateData<FixedWeir>>();
                mduFile.Read(mduFilePath, originalMD, originalArea, allFixedWeirsAndCorrespondingProperties);

                //Remove one of the selected group names to make it ' default' .
                RemoveGroupNameFromGroupableFeature(originalArea.ObservationPoints);
                RemoveGroupNameFromGroupableFeature(originalArea.Enclosures);
                RemoveGroupNameFromGroupableFeature(originalArea.DryAreas);
                RemoveGroupNameFromGroupableFeature(originalArea.ThinDams);
                RemoveGroupNameFromGroupableFeature(originalArea.FixedWeirs);
                RemoveGroupNameFromGroupableFeature(originalArea.ObservationCrossSections);
                RemoveGroupNameFromGroupableFeature(originalArea.LandBoundaries);
                RemoveGroupNameFromGroupableFeature(originalArea.Pumps);
                RemoveGroupNameFromGroupableFeature(originalArea.Structures);

                mduFile.Write(savePath, originalMD, originalArea, allFixedWeirsAndCorrespondingProperties.Values, switchTo: false);

                var savedArea = new HydroArea();
                var savedMD = new WaterFlowFMModelDefinition(newMduDir, newMduName);
                mduFile.Read(savePath, savedMD, savedArea, allFixedWeirsAndCorrespondingProperties);

                CompareHydroAreaFeatures(originalArea, savedArea);
                //Check default group was created.
                string mduPathName = Path.GetFileNameWithoutExtension(savePath);
                CheckDefaultGroupIsInFeature("LandBoundaries", originalArea.LandBoundaries, mduPathName, FileConstants.LandBoundaryFileExtension);
                CheckDefaultGroupIsInFeature("FixedWeirs", originalArea.FixedWeirs, mduPathName, FileConstants.FixedWeirPlizFileExtension);
                CheckDefaultGroupIsInFeature("ObservationPoints", originalArea.ObservationPoints, mduPathName, FileConstants.ObsPointFileExtension);
                CheckDefaultGroupIsInFeature("ObservationCrossSections", originalArea.ObservationCrossSections, mduPathName, FileConstants.ObsCrossSectionPliFileExtension);
                CheckDefaultGroupIsInFeature("DryAreas", originalArea.DryAreas, mduPathName, FileConstants.DryAreaFileExtension);
                CheckDefaultGroupIsInFeature("Enclosures", originalArea.Enclosures, mduPathName, FileConstants.EnclosureExtension);
                CheckDefaultGroupIsInFeature("Pumps", originalArea.Pumps, mduPathName, FileConstants.StructuresFileExtension);
                CheckDefaultGroupIsInFeature("Weirs", originalArea.Structures, mduPathName, FileConstants.StructuresFileExtension);
            }
            finally
            {
                FileUtils.DeleteIfExists(mduFilePath);
                FileUtils.DeleteIfExists(savePath);
                FileUtils.DeleteIfExists(saveDirectory);
            }
        }

        [Test]
        [Category(TestCategory.Jira)] // See issue D3DFMIQ-1462. Only slashSeparated is failing for now.
        [TestCase("HydroAreaCollection\\FlowFM.mdu", 2)]
        [TestCase("HydroAreaCollection\\repeatedProperty.mdu", 1)]
        [TestCase("HydroAreaCollection\\slashSeparated.mdu", 2)]
        [TestCase("HydroAreaCollection\\spaceSeparated.mdu", 2)]
        public void ReadHydroAreaCollectionIntoModelDefinitionTest(string filePath, int expectedEnclosures)
        {
            string mduFilePath = TestHelper.GetTestFilePath(filePath);
            mduFilePath = TestHelper.CreateLocalCopy(mduFilePath);
            string mduDir = Path.GetDirectoryName(mduFilePath);
            string modelName = Path.GetFileName(mduFilePath);

            try
            {
                var area = new HydroArea();
                var modelDefinition = new WaterFlowFMModelDefinition(mduDir, modelName);
                var allFixedWeirsAndCorrespondingProperties = new Dictionary<FixedWeir, ModelFeatureCoordinateData<FixedWeir>>();
                var mduFile = new MduFile();

                mduFile.Read(mduFilePath, modelDefinition, area, allFixedWeirsAndCorrespondingProperties);

                Assert.AreEqual(expectedEnclosures, area.Enclosures.Count);
            }
            finally
            {
                FileUtils.DeleteIfExists(mduFilePath);
            }
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        [TestCase(@"HydroAreaCollection\MduFileProjects\FilesOutsideMduFolderProject.dsproj_data\PathsRelativeToMdu")]
        [TestCase(@"HydroAreaCollection\MduFileProjects\FilesOutsideMduFolderProject.dsproj_data\PathsRelativeToParent")]
        public void GivenAProjectFolderWithFeatureFilesOutsideOfTheMduFolder_WhenReadingMdu_ThenAllFilesExceptStructuresAreCopiedAndEverythingShouldBeRead(string modelDirectory)
        {
            // Preparations
            string localPath = TestHelper.CreateLocalCopy(TestHelper.GetTestFilePath(modelDirectory));
            string mduFilePath = Path.Combine(localPath, @"FlowFM\FlowFM.mdu");

            string featureFileDirectory = Path.GetDirectoryName(Path.GetDirectoryName(mduFilePath));
            string mduFileFolder = Path.GetDirectoryName(mduFilePath);

            string modelName = Path.GetFileNameWithoutExtension(mduFilePath);
            var area = new HydroArea();
            var modelDefinition = new WaterFlowFMModelDefinition(mduFilePath, modelName);
            var allFixedWeirsAndCorrespondingProperties = new Dictionary<FixedWeir, ModelFeatureCoordinateData<FixedWeir>>();
            var mduFile = new MduFile();

            mduFile.Read(mduFilePath, modelDefinition, area, allFixedWeirsAndCorrespondingProperties);

            // Check if all features were read
            Assert.That(area.DryPoints.Count, Is.EqualTo(2));
            Assert.That(area.Enclosures.Count, Is.EqualTo(1));
            Assert.That(area.FixedWeirs.Count, Is.EqualTo(1));
            Assert.That(area.ObservationPoints.Count, Is.EqualTo(2));
            Assert.That(area.Pumps.Count, Is.EqualTo(1));
            Assert.That(area.ThinDams.Count, Is.EqualTo(1));
            Assert.That(area.Structures.Count, Is.EqualTo(2));
            Assert.That(area.ObservationCrossSections.Count, Is.EqualTo(1));
            Assert.That(area.LandBoundaries.Count, Is.EqualTo(1));

            CheckIfFilesWereCopied(featureFileDirectory, mduFileFolder, true);
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
            var area = new HydroArea();
            var modelDefinition = new WaterFlowFMModelDefinition(mduFilePath, modelName);
            var allFixedWeirsAndCorrespondingProperties = new Dictionary<FixedWeir, ModelFeatureCoordinateData<FixedWeir>>();
            var mduFile = new MduFile();

            mduFile.Read(mduFilePath, modelDefinition, area, allFixedWeirsAndCorrespondingProperties);

            // Check if all features were read
            Assert.That(area.DryPoints.Count, Is.EqualTo(2));
            Assert.That(area.Enclosures.Count, Is.EqualTo(1));
            Assert.That(area.FixedWeirs.Count, Is.EqualTo(1));
            Assert.That(area.ObservationPoints.Count, Is.EqualTo(2));
            Assert.That(area.Pumps.Count, Is.EqualTo(1));
            Assert.That(area.ThinDams.Count, Is.EqualTo(1));
            Assert.That(area.Structures.Count, Is.EqualTo(2));
            Assert.That(area.ObservationCrossSections.Count, Is.EqualTo(1));
            Assert.That(area.LandBoundaries.Count, Is.EqualTo(1));

            CheckIfFilesWereCopied(featureFileDirectory, Path.GetDirectoryName(mduFilePath), false);
        }

        [Test]
        public void GivenAnMduFileWithFileNamePropertyStartingWithSlashes_WhenReadingMduFile_ThenSlashesAreIgnoredAndFileIsRead()
        {
            // Preparations
            string localPath = TestHelper.CreateLocalCopy(TestHelper.GetTestFilePath(@"HydroAreaCollection\MduFileProjects"));
            string mduFilePath = Path.Combine(localPath, @"LeadingSlashesMdu\FlowFM.mdu");

            string modelName = Path.GetFileNameWithoutExtension(mduFilePath);
            var area = new HydroArea();
            var modelDefinition = new WaterFlowFMModelDefinition(mduFilePath, modelName);
            var allFixedWeirsAndCorrespondingProperties = new Dictionary<FixedWeir, ModelFeatureCoordinateData<FixedWeir>>();
            var mduFile = new MduFile();

            mduFile.Read(mduFilePath, modelDefinition, area, allFixedWeirsAndCorrespondingProperties);

            // Check if all features were read
            Assert.That(area.ObservationPoints.Count, Is.EqualTo(1));
        }

        [Test]
        public void GivenAnMduFileWithNonExistentReferenceToFile_WhenReadingMduFile_ThenTheUserGetsAnErrorMessage()
        {
            // Preparations
            string localPath = TestHelper.CreateLocalCopy(TestHelper.GetTestFilePath(@"HydroAreaCollection\MduFileProjects"));
            string mduFilePath = Path.Combine(localPath, @"MissingFileMdu\FlowFM.mdu");
            string modelName = Path.GetFileNameWithoutExtension(mduFilePath);
            
            var modelDefinition = new WaterFlowFMModelDefinition(mduFilePath, modelName);
            var allFixedWeirsAndCorrespondingProperties = new Dictionary<FixedWeir, ModelFeatureCoordinateData<FixedWeir>>();
            
            var area = new HydroArea();
            var mduFile = new MduFile();

            // Call
            void Call() => mduFile.Read(mduFilePath, modelDefinition, area, allFixedWeirsAndCorrespondingProperties);
            
            // Assert
            List<string> messages = TestHelper.GetAllRenderedMessages(Call, Level.Error).ToList();

            string[] expectedErrorMessages = new []
            {
                string.Format(Resources.MduFileReferenceDoesNotExist, "FlowFM_net.nc", mduFilePath, "NetFile", modelName),
                string.Format(Resources.MduFileReferenceDoesNotExist, "MyObservationPoints_obs.xyn", mduFilePath, "ObsFile", modelName)
            };
            
            Assert.That(messages, Is.EqualTo(expectedErrorMessages));
            Assert.That(area.ObservationPoints, Is.Empty);
        }

        [Test]
        [TestCase(@"MissingFeatureFilesProject.dsproj_data\FlowFM.mdu", 0)]
        [TestCase(@"DuplicateFilesProject.dsproj_data\FlowFM\FlowFM.mdu", 1)]
        public void GivenMduFileWithReferencesToNonExistentFilesOrFileNamesThatAlreadyExistInTheMduFolder_WhenReadingMdu_ThenTheseFeaturesAreNotReadExceptStructuresIfPresent(
            string mduProjectFilePath,
            int nExpectedFeatures)
        {
            // Given
            string localPath = TestHelper.CreateLocalCopy(TestHelper.GetTestFilePath(@"HydroAreaCollection\MduFileProjects"));
            string mduFilePath = Path.Combine(localPath, mduProjectFilePath);
            string modelName = Path.GetFileNameWithoutExtension(mduFilePath);
            var area = new HydroArea();
            var modelDefinition = new WaterFlowFMModelDefinition(mduFilePath, modelName);
            var mduFile = new MduFile();
            var allFixedWeirsAndCorrespondingProperties = new Dictionary<FixedWeir, ModelFeatureCoordinateData<FixedWeir>>();

            // When
            mduFile.Read(mduFilePath, modelDefinition, area, allFixedWeirsAndCorrespondingProperties);

            // Then
            const string errorMessage = "Expected a different number of {0}:";
            Assert.That(area.ObservationPoints, Has.Count.EqualTo(2),
                        string.Format(errorMessage, "ObservationPoints"));
            Assert.That(area.Enclosures, Has.Count.EqualTo(1),
                        string.Format(errorMessage, "Enclosures"));
            Assert.That(area.DryPoints, Has.Count.EqualTo(nExpectedFeatures),
                        string.Format(errorMessage, "DryPoints"));
            Assert.That(area.FixedWeirs, Has.Count.EqualTo(nExpectedFeatures),
                        string.Format(errorMessage, "FixedWeirs"));
            Assert.That(area.Pumps, Has.Count.EqualTo(nExpectedFeatures),
                        string.Format(errorMessage, "Pumps"));
            Assert.That(area.Structures, Has.Count.EqualTo(nExpectedFeatures),
                        string.Format(errorMessage, "Weirs"));
            Assert.That(area.ThinDams, Has.Count.EqualTo(nExpectedFeatures),
                        string.Format(errorMessage, "ThinDams"));
            Assert.That(area.ObservationCrossSections, Has.Count.EqualTo(nExpectedFeatures),
                        string.Format(errorMessage, "ObservationCrossSections"));
            Assert.That(area.LandBoundaries, Has.Count.EqualTo(nExpectedFeatures),
                        string.Format(errorMessage, "LandBoundaries"));
        }

        [Test]
        [TestCase("obspoints", "OBSPOINTS", true)]
        [TestCase("obspoints", "OBSPOINTS", false)]
        [TestCase("OBSPOINTS", "obspoints", true)]
        [TestCase("OBSPOINTS", "obspoints", false)]
        public void GivenTwoFeaturesWithNameThatDifferByACapitalLetter_WhenWritingMduFile_ThenBothAreWrittenToTheSameFile(string firstGroupName, string secondGroupName, bool fileShouldAlreadyExists)
        {
            string groupName1 = firstGroupName + FileConstants.ObsPointFileExtension;
            string groupName2 = secondGroupName + FileConstants.ObsPointFileExtension;
            string existingGroupName = "ObSpOiNtS" + FileConstants.ObsPointFileExtension;

            string mduFilePath = string.Concat(Path.GetTempFileName(), ".mdu");
            var mduFile = new MduFile();
            string mduDir = Path.GetDirectoryName(mduFilePath);
            string modelName = Path.GetFileName(Path.GetFileNameWithoutExtension(mduFilePath));

            File.Delete(Path.Combine(mduDir, groupName1));
            if (fileShouldAlreadyExists)
            {
                FileStream fileStream = File.Create(Path.Combine(mduDir, existingGroupName));
                fileStream.Close();
            }

            Assert.NotNull(mduDir);

            var area = new HydroArea();
            var name1 = "ObsPoint01";
            var name2 = "ObsPoint02";
            area.ObservationPoints.AddRange(
                new[]
                {
                    WaterFlowFMMduFileTestHelper.GetNewGroupableFeature2DPoint(groupName1, name1),
                    WaterFlowFMMduFileTestHelper.GetNewGroupableFeature2DPoint(groupName2, name2)
                }
            );

            var modelDefinition = new WaterFlowFMModelDefinition(mduDir, modelName);
            var allFixedWeirsAndCorrespondingProperties = new Dictionary<FixedWeir, ModelFeatureCoordinateData<FixedWeir>>();
            TestHelper.AssertAtLeastOneLogMessagesContains(() => mduFile.Write(mduFilePath, modelDefinition, area, allFixedWeirsAndCorrespondingProperties.Values),
                                                           fileShouldAlreadyExists ? "already exists in the project folder. Features in group" : "Features with group name");

            string[] files = Directory.GetFiles(mduDir);
            int groupName1FileCount = files.Count(fp => fp.Contains(groupName1));
            int groupName2FileCount = files.Count(fp => fp.Contains(groupName2));
            int existingFileCount = files.Count(fp => fp.Contains(existingGroupName));

            Assert.That(existingFileCount, Is.EqualTo(fileShouldAlreadyExists ? 1 : 0));
            Assert.That(groupName1FileCount, Is.EqualTo(fileShouldAlreadyExists ? 0 : 1));
            Assert.That(groupName2FileCount, Is.EqualTo(0));

            area = new HydroArea();
            mduFile.Read(mduFilePath, modelDefinition, area, allFixedWeirsAndCorrespondingProperties);
            Assert.That(area.ObservationPoints.Count, Is.EqualTo(2));
            Assert.IsTrue(area.ObservationPoints.Select(o => o.Name).Contains(name1));
            Assert.IsTrue(area.ObservationPoints.Select(o => o.Name).Contains(name2));

            // Delete all files that were created during this test
            files.Where(fp => fp.Contains(groupName1) || fp.Contains(groupName2) || fp.Contains(existingGroupName)).ForEach(File.Delete);
        }

        [Test]
        public void GivenModelWithOneDryAreaAndOneDryPoint_WhenWritingAndReadingMduFile_ThenBothFeaturesArePresent()
        {
            string mduFilePath = string.Concat(Path.GetTempFileName(), ".mdu");
            var mduFile = new MduFile();
            string mduDir = Path.GetDirectoryName(mduFilePath);
            string modelName = Path.GetFileName(Path.GetFileNameWithoutExtension(mduFilePath));
            Assert.IsNotNull(mduDir);

            var area = new HydroArea();
            var dryPointsGroupName = @"featureFiles/myDryPoints";
            area.DryPoints.Add(WaterFlowFMMduFileTestHelper.GetNewGroupablePointFeature(dryPointsGroupName));
            var dryAreasGroupName = @"featureFiles/myDryAreas";
            area.DryAreas.Add(WaterFlowFMMduFileTestHelper.GetNewGroupableFeature2DPolygon(dryAreasGroupName, "Polygon01"));

            var modelDefinition = new WaterFlowFMModelDefinition(mduDir, modelName);
            var allFixedWeirsAndCorrespondingProperties = new Dictionary<FixedWeir, ModelFeatureCoordinateData<FixedWeir>>();
            mduFile.Write(mduFilePath, modelDefinition, area, allFixedWeirsAndCorrespondingProperties.Values);

            var newArea = new HydroArea();
            mduFile.Read(mduFilePath, modelDefinition, newArea, allFixedWeirsAndCorrespondingProperties);
            Assert.That(newArea.DryAreas.Count, Is.EqualTo(1));
            Assert.That(newArea.DryPoints.Count, Is.EqualTo(1));

            string dryPointsFileNameWithExtension = dryPointsGroupName + FileConstants.DryPointFileExtension;

            FileUtils.DeleteIfExists(mduFilePath);
            FileUtils.DeleteIfExists(mduFilePath.Replace(".mdu", string.Empty));
            DeleteAllFilesAndFoldersInSubDirectory(new DirectoryInfo(Path.GetDirectoryName(Path.Combine(mduDir, dryPointsFileNameWithExtension))));
        }

        [Test]
        public void GivenModelWithOneDryAreaAndOneDryPoint_WhenWritingMduFile_ThenBothFeatureFileReferencesAreWrittenToMduFile()
        {
            string mduFilePath = string.Concat(Path.GetTempFileName(), ".mdu");
            var mduFile = new MduFile();
            string mduDir = Path.GetDirectoryName(mduFilePath);
            string modelName = Path.GetFileName(Path.GetFileNameWithoutExtension(mduFilePath));
            Assert.IsNotNull(mduDir);

            var area = new HydroArea();
            var dryPointsGroupName = @"featureFiles/myDryPoints";
            area.DryPoints.Add(WaterFlowFMMduFileTestHelper.GetNewGroupablePointFeature(dryPointsGroupName));
            var dryAreasGroupName = @"featureFiles/myDryAreas";
            area.DryAreas.Add(WaterFlowFMMduFileTestHelper.GetNewGroupableFeature2DPolygon(dryAreasGroupName, "Polygon01"));

            var modelDefinition = new WaterFlowFMModelDefinition(mduDir, modelName);
            var allFixedWeirsAndCorrespondingProperties = new List<ModelFeatureCoordinateData<FixedWeir>>();
            mduFile.Write(mduFilePath, modelDefinition, area, allFixedWeirsAndCorrespondingProperties);

            string readAllText = File.ReadAllText(mduFilePath);

            string dryPointsFileNameWithExtension = dryPointsGroupName + FileConstants.DryPointFileExtension;
            string dryAreasFileNameWithExtension = dryAreasGroupName + FileConstants.DryAreaFileExtension;

            WaterFlowFMMduFileTestHelper.AssertContainsMduLine(readAllText, "DryPointsFile", dryPointsFileNameWithExtension + " " + dryAreasFileNameWithExtension);

            Assert.IsTrue(File.Exists(Path.Combine(mduDir, dryPointsFileNameWithExtension)));
            Assert.IsTrue(File.Exists(Path.Combine(mduDir, dryAreasFileNameWithExtension)));

            FileUtils.DeleteIfExists(mduFilePath);
            FileUtils.DeleteIfExists(mduFilePath.Replace(".mdu", string.Empty));
            DeleteAllFilesAndFoldersInSubDirectory(new DirectoryInfo(Path.GetDirectoryName(Path.Combine(mduDir, dryPointsFileNameWithExtension))));
        }

        [Test]
        public void GivenFeaturesWithGroupNamesThatPointToSubFolders_WhenWriting_ThenMduFileAndFeatureFilesAreBeingWritten()
        {
            string mduFilePath = string.Concat(Path.GetTempFileName(), ".mdu");
            var mduFile = new MduFile();
            string mduDir = Path.GetDirectoryName(mduFilePath);
            string modelName = Path.GetFileName(Path.GetFileNameWithoutExtension(mduFilePath));

            Assert.NotNull(mduDir);

            var area = new HydroArea();

            var pointGroupName = @"featureFiles/myObsPoints";
            area.ObservationPoints.AddRange(
                new[]
                {
                    WaterFlowFMMduFileTestHelper.GetNewGroupableFeature2DPoint(pointGroupName, "ObsPoint01"),
                    WaterFlowFMMduFileTestHelper.GetNewGroupableFeature2DPoint(pointGroupName, "ObsPoint02")
                }
            );

            var enclosureGroupName = @"featureFiles/myPolygons";
            area.Enclosures.AddRange(
                new[]
                {
                    WaterFlowFMMduFileTestHelper.GetNewGroupableFeature2DPolygon(enclosureGroupName, "Polygon01"),
                    WaterFlowFMMduFileTestHelper.GetNewGroupableFeature2DPolygon(enclosureGroupName, "Polygon02")
                }
            );

            var dryPointsGroupName = @"featureFiles/myDryPoints";
            area.DryPoints.AddRange(
                new[]
                {
                    WaterFlowFMMduFileTestHelper.GetNewGroupablePointFeature(dryPointsGroupName),
                    WaterFlowFMMduFileTestHelper.GetNewGroupablePointFeature(dryPointsGroupName)
                }
            );

            var landBoundariesGroupName = @"featureFiles/myLandBoundaries";
            area.LandBoundaries.AddRange(
                new[]
                {
                    WaterFlowFMMduFileTestHelper.GetNewLandBoundary2D(landBoundariesGroupName, "LandBoundary01"),
                    WaterFlowFMMduFileTestHelper.GetNewLandBoundary2D(landBoundariesGroupName, "LandBoundary02")
                }
            );

            var structureGroupName = @"featureFiles/myGates";
            area.Structures.AddRange(
                new[]
                {
                    WaterFlowFMMduFileTestHelper.GetNewWeir2DWithGateFormula(structureGroupName, "Gate01"),
                    WaterFlowFMMduFileTestHelper.GetNewWeir2DWithGateFormula(structureGroupName, "Gate02")
                }
            );

            var modelDefinition = new WaterFlowFMModelDefinition(mduDir, modelName);
            var allFixedWeirsAndCorrespondingProperties = new List<ModelFeatureCoordinateData<FixedWeir>>();
            mduFile.Write(mduFilePath, modelDefinition, area, allFixedWeirsAndCorrespondingProperties);

            string readAllText = File.ReadAllText(mduFilePath);

            string obsFileNameWithExtension = pointGroupName + FileConstants.ObsPointFileExtension;
            string enclosureFileNameWithExtension = enclosureGroupName + FileConstants.EnclosureExtension;
            string dryPointsFileNameWithExtension = dryPointsGroupName + FileConstants.DryPointFileExtension;
            string landBoundariesFileNameWithExtension = landBoundariesGroupName + FileConstants.LandBoundaryFileExtension;
            string structuresFileNameWithExtension = structureGroupName + FileConstants.StructuresFileExtension;

            WaterFlowFMMduFileTestHelper.AssertContainsMduLine(readAllText, modelDefinition.GetModelProperty(KnownProperties.ObsFile).PropertyDefinition.Caption, obsFileNameWithExtension);
            WaterFlowFMMduFileTestHelper.AssertContainsMduLine(readAllText, modelDefinition.GetModelProperty(KnownProperties.EnclosureFile).PropertyDefinition.Caption, enclosureFileNameWithExtension);
            WaterFlowFMMduFileTestHelper.AssertContainsMduLine(readAllText, modelDefinition.GetModelProperty(KnownProperties.DryPointsFile).PropertyDefinition.Caption, dryPointsFileNameWithExtension);
            WaterFlowFMMduFileTestHelper.AssertContainsMduLine(readAllText, modelDefinition.GetModelProperty(KnownProperties.LandBoundaryFile).PropertyDefinition.Caption, landBoundariesFileNameWithExtension);
            WaterFlowFMMduFileTestHelper.AssertContainsMduLine(readAllText, modelDefinition.GetModelProperty(KnownProperties.StructuresFile).PropertyDefinition.Caption, structuresFileNameWithExtension);

            Assert.IsTrue(File.Exists(Path.Combine(mduDir, obsFileNameWithExtension)));
            Assert.IsTrue(File.Exists(Path.Combine(mduDir, enclosureFileNameWithExtension)));
            Assert.IsTrue(File.Exists(Path.Combine(mduDir, dryPointsFileNameWithExtension)));
            Assert.IsTrue(File.Exists(Path.Combine(mduDir, landBoundariesFileNameWithExtension)));
            Assert.IsTrue(File.Exists(Path.Combine(mduDir, structuresFileNameWithExtension)));

            DeleteAllFilesAndFoldersInSubDirectory(new DirectoryInfo(Path.GetDirectoryName(Path.Combine(mduDir, obsFileNameWithExtension))));
        }

        [Test]
        public void GivenFeaturesWithInvalidGroupNames_WhenWriting_ThenTheseFeaturesAreNotSaved()
        {
            string mduFilePath = string.Concat(Path.GetTempFileName(), ".mdu");
            var mduFile = new MduFile();
            string mduDir = Path.GetDirectoryName(mduFilePath);
            string modelName = Path.GetFileName(Path.GetFileNameWithoutExtension(mduFilePath));

            Assert.NotNull(mduDir);

            var area = new HydroArea();

            var pointGroupName = @"..\myObsPoints";
            area.ObservationPoints.AddRange(
                new[]
                {
                    WaterFlowFMMduFileTestHelper.GetNewGroupableFeature2DPoint(pointGroupName, "ObsPoint01"),
                    WaterFlowFMMduFileTestHelper.GetNewGroupableFeature2DPoint(pointGroupName, "ObsPoint02")
                }
            );

            var dryPointsGroupName = @"featureFiles/myDryPoints";
            area.DryPoints.AddRange(
                new[]
                {
                    WaterFlowFMMduFileTestHelper.GetNewGroupablePointFeature(dryPointsGroupName),
                    WaterFlowFMMduFileTestHelper.GetNewGroupablePointFeature(dryPointsGroupName)
                }
            );

            string obsFileNameWithExtension = pointGroupName + FileConstants.ObsPointFileExtension;
            string dryPointsFileNameWithExtension = dryPointsGroupName + FileConstants.DryPointFileExtension;
            DeleteAllFilesAndFoldersInSubDirectory(new DirectoryInfo(Path.GetDirectoryName(Path.Combine(mduDir, dryPointsFileNameWithExtension))));

            var modelDefinition = new WaterFlowFMModelDefinition(mduDir, modelName);
            var allFixedWeirsAndCorrespondingProperties = new List<ModelFeatureCoordinateData<FixedWeir>>();
            mduFile.Write(mduFilePath, modelDefinition, area, allFixedWeirsAndCorrespondingProperties);

            string readAllText = File.ReadAllText(mduFilePath);

            Regex expectedObsFileRegex = GetRegularExpressionForTextWithEmptyValue("ObsFile");
            Assert.IsTrue(expectedObsFileRegex.IsMatch(readAllText), "File did not contain expected text.");

            WaterFlowFMMduFileTestHelper.AssertContainsMduLine(readAllText, "DryPointsFile", dryPointsFileNameWithExtension);
            Assert.IsFalse(File.Exists(Path.Combine(mduDir, obsFileNameWithExtension)));
            Assert.IsTrue(File.Exists(Path.Combine(mduDir, dryPointsFileNameWithExtension)));

            DeleteAllFilesAndFoldersInSubDirectory(new DirectoryInfo(Path.GetDirectoryName(Path.Combine(mduDir, dryPointsFileNameWithExtension))));
        }

        [Test]
        public void GivenMduFileReferencingAnExistingFeatureFile_WhenLoadingAndRenamingTheFeatureWithARelativePath_ThenReferenceInMduFileIsDeleted()
        {
            var initialGroupName = "FlowFM_thd.pli";
            string newGroupName = "../" + initialGroupName;

            // Preparations
            string localPath = TestHelper.CreateLocalCopy(TestHelper.GetTestFilePath(@"HydroAreaCollection\MduFileProjects"));
            string mduFilePath = Path.Combine(localPath, @"ChangeFeatureGroupNameMduTest\FlowFM.mdu");
            string mduDir = Path.GetDirectoryName(mduFilePath);

            string modelName = Path.GetFileNameWithoutExtension(mduFilePath);
            var area = new HydroArea();
            var modelDefinition = new WaterFlowFMModelDefinition(mduFilePath, modelName);
            var allFixedWeirsAndCorrespondingProperties = new Dictionary<FixedWeir, ModelFeatureCoordinateData<FixedWeir>>();
            var mduFile = new MduFile();

            mduFile.Read(mduFilePath, modelDefinition, area, allFixedWeirsAndCorrespondingProperties);

            // Check initial settings
            IEventedList<ThinDam2D> thinDams = area.ThinDams;
            Assert.IsNotNull(thinDams);
            Assert.That(thinDams.Count, Is.EqualTo(1));
            Assert.That(thinDams.FirstOrDefault().GroupName, Is.EqualTo(initialGroupName));

            // Change group name and write to mdu file
            area.ThinDams.ForEach(td => td.GroupName = newGroupName);
            mduFile.Write(mduFilePath, modelDefinition, area, allFixedWeirsAndCorrespondingProperties.Values);

            string readAllText = File.ReadAllText(mduFilePath);
            Regex expectedThinDamFileRegex = GetRegularExpressionForTextWithEmptyValue("ThinDamFile");
            Assert.IsTrue(expectedThinDamFileRegex.IsMatch(readAllText), "File did not contain expected text.");

            DeleteAllFilesAndFoldersInSubDirectory(new DirectoryInfo(Path.GetDirectoryName(Path.Combine(mduDir, initialGroupName))));
        }

        [Test]
        public void GivenAbsolutePathNameForFeatures_WhenWriting_ThenWarnWhenPathIsNotInSubFolderOfMduFolder()
        {
            string mduFilePath = string.Concat(Path.GetTempFileName(), ".mdu");
            var mduFile = new MduFile();
            string mduDir = Path.GetDirectoryName(mduFilePath);
            string modelName = Path.GetFileName(Path.GetFileNameWithoutExtension(mduFilePath));

            Assert.NotNull(mduDir);

            var area = new HydroArea();

            string absolutePathPointGroupName = Path.Combine(Directory.GetParent(mduDir).FullName, "myObsPoints");
            area.ObservationPoints.AddRange(
                new[]
                {
                    WaterFlowFMMduFileTestHelper.GetNewGroupableFeature2DPoint(absolutePathPointGroupName, "ObsPoint01"),
                    WaterFlowFMMduFileTestHelper.GetNewGroupableFeature2DPoint(absolutePathPointGroupName, "ObsPoint02")
                }
            );

            var modelDefinition = new WaterFlowFMModelDefinition(mduDir, modelName);
            var allFixedWeirsAndCorrespondingProperties = new List<ModelFeatureCoordinateData<FixedWeir>>();
            TestHelper.AssertAtLeastOneLogMessagesContains(() => mduFile.Write(mduFilePath, modelDefinition, area, allFixedWeirsAndCorrespondingProperties), ", because the group name is invalid. Remove any occurences of");
            Assert.IsFalse(File.Exists(absolutePathPointGroupName));
        }

        [Test]
        public void GivenPolylineFeaturesWithPlizExtension_WhenWritingMduFile_ThenFilesAreSavedAsPlizFile()
        {
            string mduFilePath = string.Concat(Path.GetTempFileName(), ".mdu");
            string mduDir = Path.GetDirectoryName(mduFilePath);
            string modelName = Path.GetFileName(Path.GetFileNameWithoutExtension(mduFilePath));
            Assert.NotNull(mduDir);

            var obsCrsGroupName = "myObsCrossSection_crs.pliz";
            string obsCrsFileName = Path.Combine(mduDir, obsCrsGroupName);
            var fixedWeirGroupName = "myFixedWeir_fxw.pliz";
            string fixedWeirFileName = Path.Combine(mduDir, obsCrsGroupName);
            var thinDamGroupName = "myThinDam_thd.pliz";
            string thinDamFileName = Path.Combine(mduDir, obsCrsGroupName);
            try
            {
                var mduFile = new MduFile();
                var area = new HydroArea();
                area.ObservationCrossSections.Add(new ObservationCrossSection2D
                {
                    GroupName = obsCrsGroupName,
                    Geometry = new LineString(new[]
                    {
                        new Coordinate(0, 0),
                        new Coordinate(0, 100)
                    })
                });
                var fixedWeir = new FixedWeir
                {
                    GroupName = fixedWeirGroupName,
                    Geometry = new LineString(new[]
                    {
                        new Coordinate(0, 0),
                        new Coordinate(0, 100)
                    })
                };
                area.FixedWeirs.Add(fixedWeir);
                area.ThinDams.Add(new ThinDam2D
                {
                    GroupName = thinDamGroupName,
                    Geometry = new LineString(new[]
                    {
                        new Coordinate(0, 0),
                        new Coordinate(0, 100)
                    })
                });

                var modelDefinition = new WaterFlowFMModelDefinition(mduDir, modelName);
                var allFixedWeirsAndCorrespondingProperties = new List<ModelFeatureCoordinateData<FixedWeir>> { new ModelFeatureCoordinateData<FixedWeir> { Feature = fixedWeir } };
                mduFile.Write(mduFilePath, modelDefinition, area, allFixedWeirsAndCorrespondingProperties);
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
            string mduFilePath = string.Concat(Path.GetTempFileName(), ".mdu");
            string mduDir = Path.GetDirectoryName(mduFilePath);
            string modelName = Path.GetFileName(Path.GetFileNameWithoutExtension(mduFilePath));
            Assert.NotNull(mduDir);

            var obsCrsGroupName = "myObsCrossSection_crs.pli";
            string obsCrsFileName = Path.Combine(mduDir, obsCrsGroupName);
            var fixedWeirGroupName = "myFixedWeir_fxw.pli";
            string fixedWeirFileName = Path.Combine(mduDir, obsCrsGroupName);
            var thinDamGroupName = "myThinDam_thd.pli";
            string thinDamFileName = Path.Combine(mduDir, obsCrsGroupName);
            try
            {
                var mduFile = new MduFile();
                var area = new HydroArea();
                area.ObservationCrossSections.Add(new ObservationCrossSection2D
                {
                    GroupName = obsCrsGroupName,
                    Geometry = new LineString(new[]
                    {
                        new Coordinate(0, 0),
                        new Coordinate(0, 100)
                    })
                });
                var fixedWeir = new FixedWeir
                {
                    GroupName = fixedWeirGroupName,
                    Geometry = new LineString(new[]
                    {
                        new Coordinate(0, 0),
                        new Coordinate(0, 100)
                    })
                };
                area.FixedWeirs.Add(fixedWeir);
                area.ThinDams.Add(new ThinDam2D
                {
                    GroupName = thinDamGroupName,
                    Geometry = new LineString(new[]
                    {
                        new Coordinate(0, 0),
                        new Coordinate(0, 100)
                    })
                });

                var modelDefinition = new WaterFlowFMModelDefinition(mduDir, modelName);
                var allFixedWeirsAndCorrespondingProperties = new List<ModelFeatureCoordinateData<FixedWeir>>() { new ModelFeatureCoordinateData<FixedWeir>() { Feature = fixedWeir } };

                mduFile.Write(mduFilePath, modelDefinition, area, allFixedWeirsAndCorrespondingProperties);
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
            string mduFilePath = string.Concat(Path.GetTempFileName(), ".mdu");
            var mduFile = new MduFile();
            string mduDir = Path.GetDirectoryName(mduFilePath);
            string modelName = Path.GetFileName(Path.GetFileNameWithoutExtension(mduFilePath));

            Assert.NotNull(mduDir);

            var area = new HydroArea();

            area.ObservationPoints.Add(WaterFlowFMMduFileTestHelper.GetNewGroupableFeature2DPoint("MyObsPoints.xyn", "ObsPoint01"));
            area.DryPoints.Add(WaterFlowFMMduFileTestHelper.GetNewGroupablePointFeature("MyDryPoints.xyz"));
            area.ObservationCrossSections.Add(new ObservationCrossSection2D
            {
                GroupName = modelName + ".pli",
                Name = "MyObsCrossSection",
                Geometry = new LineString(new[]
                {
                    new Coordinate(0, 0),
                    new Coordinate(0, 100)
                }),
                IsDefaultGroup = true
            });
            area.Structures.Add(new Structure()
            {
                GroupName = "MyStructures.ini",
                Name = "MyGate",
                IsDefaultGroup = false
            });

            var obsGroupName = "MyObsPoints_obs.xyn";
            var dryGroupName = "MyDryPoints_dry.xyz";
            string crsGroupName = modelName + "_crs.pli";
            var gateGroupName = "MyStructures_structures.ini";

            string obsFilePath = Path.Combine(mduDir, obsGroupName);
            string dryFilePath = Path.Combine(mduDir, dryGroupName);
            string crsFilePath = Path.Combine(mduDir, crsGroupName);
            string gateFilePath = Path.Combine(mduDir, gateGroupName);
            FileUtils.DeleteIfExists(obsFilePath);
            FileUtils.DeleteIfExists(dryFilePath);
            FileUtils.DeleteIfExists(crsFilePath);
            FileUtils.DeleteIfExists(gateFilePath);

            var modelDefinition = new WaterFlowFMModelDefinition(mduDir, modelName);
            var allFixedWeirsAndCorrespondingProperties = new List<ModelFeatureCoordinateData<FixedWeir>>();

            mduFile.Write(mduFilePath, modelDefinition, area, allFixedWeirsAndCorrespondingProperties);

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

            var structuresFileEntries = modelDefinition.GetModelProperty(KnownProperties.StructuresFile).Value as List<string>;
            Assert.NotNull(structuresFileEntries);
            Assert.That(structuresFileEntries.Count, Is.EqualTo(1));
            Assert.That(area.Structures.FirstOrDefault().GroupName, Is.EqualTo("MyStructures.ini"));
            Assert.That(structuresFileEntries.Contains(gateGroupName));
            Assert.That(File.Exists(gateFilePath));

            FileUtils.DeleteIfExists(mduFilePath);
        }

        [Test]
        public void GivenStructuresFileWithReferenceToNonExsistentFile_WhenReadingMdu_ThenStructuresFileIsIgnoredAndDeltaShellGivesWarning()
        {
            // Preparations
            string localPath = TestHelper.CreateLocalCopy(TestHelper.GetTestFilePath(@"HydroAreaCollection\MduFileProjects"));
            string mduFilePath = Path.Combine(localPath, @"StructuresFileWithoutReferences\FlowFM\FlowFM.mdu");
            string mduDir = Path.GetDirectoryName(mduFilePath);

            var area = new HydroArea();
            var modelDefinition = new WaterFlowFMModelDefinition(mduFilePath, Path.GetFileNameWithoutExtension(mduFilePath));
            var mduFile = new MduFile();
            var allFixedWeirsAndCorrespondingProperties = new Dictionary<FixedWeir, ModelFeatureCoordinateData<FixedWeir>>();

            TestHelper.AssertAtLeastOneLogMessagesContains(() => mduFile.Read(mduFilePath, modelDefinition, area, allFixedWeirsAndCorrespondingProperties), "' is referenced in structures file '");
            Assert.IsFalse(File.Exists(Path.Combine(mduDir, "FlowFM_structures.ini")));
            Assert.That(modelDefinition.GetModelProperty(KnownProperties.StructuresFile).GetValueAsString(), Is.EqualTo(""));
        }

        [Test]
        public void GivenMduFileWithReferencesThatIsSituatedInAFolderWithSpacesInItsName_WhenReadingMduFile_ThenNoProblemsOccur()
        {
            // Preparations
            string localPath = TestHelper.CreateLocalCopy(TestHelper.GetTestFilePath(@"HydroAreaCollection\MduFileProjects"));
            string mduFilePath = Path.Combine(localPath, @"MduFileInFolderWith - SpacesInName\FlowFM.mdu");

            new MduFile().Read(mduFilePath,
                               new WaterFlowFMModelDefinition(mduFilePath, Path.GetFileNameWithoutExtension(mduFilePath)),
                               new HydroArea(), new Dictionary<FixedWeir, ModelFeatureCoordinateData<FixedWeir>>());
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void GivenPointSourceFileThatContainsOnePoint_WhenReadingMduFile_ThenSourceHasBeenImported()
        {
            // Preparations
            string localPath = TestHelper.CreateLocalCopy(TestHelper.GetTestFilePath(@"HydroAreaCollection\MduFileProjects"));
            string mduFilePath = Path.Combine(localPath, @"MduFileWithSourceSinkFile\FlowFM.mdu");

            var area = new HydroArea();
            var modelDefinition = new WaterFlowFMModelDefinition(mduFilePath, Path.GetFileNameWithoutExtension(mduFilePath));
            var allFixedWeirsAndCorrespondingProperties = new Dictionary<FixedWeir, ModelFeatureCoordinateData<FixedWeir>>();

            Assert.That(modelDefinition.SourcesAndSinks.Count, Is.EqualTo(0));
            new MduFile().Read(mduFilePath, modelDefinition, area, allFixedWeirsAndCorrespondingProperties);
            Assert.That(modelDefinition.SourcesAndSinks.Count, Is.EqualTo(1));
        }

        #region TestHelpers

        private void CompareHydroAreaFeatures(HydroArea originalArea, HydroArea savedArea)
        {
            //Check features
            CompareHydroAreaFeatureCollections("ObservationPoints", originalArea.ObservationPoints,
                                               originalArea.ObservationPoints.Select(op => op.Name), savedArea.ObservationPoints,
                                               savedArea.ObservationPoints.Select(op => op.Name));
            CompareHydroAreaFeatureCollections("Enclosures", originalArea.Enclosures,
                                               originalArea.Enclosures.Select(op => op.Name), savedArea.Enclosures,
                                               savedArea.Enclosures.Select(op => op.Name));
            CompareHydroAreaFeatureCollections("DryAreas", originalArea.DryAreas,
                                               originalArea.DryAreas.Select(op => op.Name), savedArea.DryAreas,
                                               savedArea.DryAreas.Select(op => op.Name));
            CompareHydroAreaFeatureCollections("ThinDams", originalArea.ThinDams,
                                               originalArea.ThinDams.Select(op => op.Name), savedArea.ThinDams,
                                               savedArea.ThinDams.Select(op => op.Name));
            CompareHydroAreaFeatureCollections("FixedWeirs", originalArea.FixedWeirs,
                                               originalArea.FixedWeirs.Select(op => op.Name), savedArea.FixedWeirs,
                                               savedArea.FixedWeirs.Select(op => op.Name));
            CompareHydroAreaFeatureCollections("ObservationCrossSections", originalArea.ObservationCrossSections,
                                               originalArea.ObservationCrossSections.Select(op => op.Name), savedArea.ObservationCrossSections,
                                               savedArea.ObservationCrossSections.Select(op => op.Name));
            CompareHydroAreaFeatureCollections("LandBoundaries", originalArea.LandBoundaries,
                                               originalArea.LandBoundaries.Select(op => op.Name), savedArea.LandBoundaries,
                                               savedArea.LandBoundaries.Select(op => op.Name));

            /* Check structures */
            CompareHydroAreaFeatureCollections("Pumps", originalArea.Pumps,
                                               originalArea.Pumps.Select(op => op.Name), savedArea.Pumps,
                                               savedArea.Pumps.Select(op => op.Name));
            CompareHydroAreaFeatureCollections("Weirs", originalArea.Structures,
                                               originalArea.Structures.Select(op => op.Name), savedArea.Structures,
                                               savedArea.Structures.Select(op => op.Name));
        }

        private void CompareHydroAreaModelProperties(string propertyName, string saveMduFilePath, WaterFlowFMModelDefinition expectedMD,
                                                     WaterFlowFMModelDefinition savedMD)
        {
            WaterFlowFMProperty expectedProp = expectedMD.GetModelProperty(propertyName);
            WaterFlowFMProperty savedProp = savedMD.GetModelProperty(propertyName);
            Assert.IsNotNull(expectedProp, "Wrong property name? {0}", propertyName);
            Assert.IsNotNull(savedProp, "Wrong property name? {0}", propertyName);

            Assert.AreEqual(expectedProp.GetValueAsString(), savedProp.GetValueAsString());
            CheckFeatureFilesWereCreated(saveMduFilePath, propertyName, savedMD);
        }

        private void CompareHydroAreaFeatureCollections(string featureName, IEnumerable<IGroupableFeature> expectedFeature, IEnumerable<string> expectedAreaFeatureNames, IEnumerable<IGroupableFeature> savedFeature, IEnumerable<string> savedAreaFeatureNames)
        {
            List<IGroupableFeature> expectedList = expectedFeature.ToList();
            List<IGroupableFeature> savedList = savedFeature.ToList();
            Assert.AreEqual(expectedList.Count, savedList.Count,
                            "{0} Saved features differ from the original read ones.", featureName);

            List<IGrouping<string, IGroupableFeature>> expectedGroups = expectedList.GroupBy(g => g.GroupName).ToList();
            List<string> expectedGroupNames = expectedGroups.Select(g => g.Key).ToList();
            List<IGrouping<string, IGroupableFeature>> savedGroups = savedList.GroupBy(g => g.GroupName).ToList();
            List<string> savedGroupNames = savedGroups.Select(g => g.Key).ToList();
            Assert.AreEqual(expectedGroupNames.Count, savedGroupNames.Count,
                            "{0} Group names differ from the original read ones. Original: {1}, Saved {2}", featureName, expectedGroupNames, savedGroupNames);

            Assert.IsTrue(Enumerable.SequenceEqual(
                              expectedAreaFeatureNames.OrderBy(fElement => fElement),
                              savedAreaFeatureNames.OrderBy(sElement => sElement)));
        }

        private void CheckFeatureFilesWereCreated(string mduFilePath, string propertyName, WaterFlowFMModelDefinition modelDefinition)
        {
            IList<string> files = MduFileHelper.GetMultipleSubfilePath(mduFilePath, modelDefinition.GetModelProperty(propertyName));
            IEnumerable<string> notCreatedFiles = files.Where(f => !File.Exists(f) || string.IsNullOrEmpty(File.ReadAllText(f)));
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

            List<IGrouping<string, IGroupableFeature>> featureGrouped = asGroupable.GroupBy(g => g.GroupName).ToList();
            List<string> readGroups = featureGrouped.Select(g => g.Key).ToList();
            Assert.AreEqual(expectedGroupNames.Count, featureGrouped.Count,
                            string.Format("Feature {0}. Expected groupNames {1}, generated {2}", featureName, expectedGroupNames, readGroups));

            foreach (string expectedGroupName in expectedGroupNames)
            {
                Assert.IsTrue(readGroups.Contains(expectedGroupName),
                              "Feature {0}, expected group: {1} but not found in {2}", featureName, expectedGroupName, readGroups);
            }
        }

        private static void CheckDefaultGroupIsInFeature(string featureName, IEnumerable<IGroupableFeature> feature, string savePath, string featureExtension)
        {
            List<IGrouping<string, IGroupableFeature>> grouped = feature.GroupBy(g => g.GroupName).ToList();
            List<string> groupNames = grouped.Select(g => g.Key).ToList();
            Assert.IsTrue(groupNames.Any(g => g.Replace(featureExtension, string.Empty).Trim().Equals(savePath)),
                          "Feature {0} did not save default group {1}, instead: {2}", featureName, savePath, groupNames.ToList());
        }

        private static void CheckDryPointsFeature(HydroArea originalArea, HydroArea savedArea)
        {
            List<GroupablePointFeature> expectedList = originalArea.DryPoints.ToList();
            List<GroupablePointFeature> savedList = savedArea.DryPoints.ToList();
            Assert.AreEqual(expectedList.Count, savedList.Count,
                            "Expected dry points {0}, saved {1}", expectedList.Count, savedList.Count);

            List<IGrouping<string, GroupablePointFeature>> expectedGroups = expectedList.GroupBy(g => g.GroupName).ToList();
            List<string> expectedGroupNames = expectedGroups.Select(g => g.Key).ToList();
            List<IGrouping<string, GroupablePointFeature>> savedGroups = savedList.GroupBy(g => g.GroupName).ToList();
            List<string> savedGroupNames = savedGroups.Select(g => g.Key).ToList();
            Assert.AreEqual(expectedGroupNames.Count, savedGroups.Select(g => g.Key).Count(),
                            "Group names differ from the original read ones. Original: {0}, Saved {1}", expectedGroupNames,
                            savedGroupNames);

            Assert.IsFalse(expectedList.Any(dryPoint => savedList.Contains(dryPoint)));
            Assert.IsFalse(savedList.Any(dryPoint => expectedList.Contains(dryPoint)));
        }

        private static WaterFlowFMProperty AddCustomMultipleFilePropertyToModelDefinition(
            WaterFlowFMModelDefinition modelDefinition, string propertyName, string fileSectionName)
        {
            var propertyDefinition = new WaterFlowFMPropertyDefinition
            {
                MduPropertyName = propertyName,
                FileSectionName = fileSectionName,
                DataType = typeof(IList<string>),
                IsMultipleFile = true
            };
            modelDefinition.AddProperty(new WaterFlowFMProperty(propertyDefinition, string.Empty));
            Assert.IsTrue(modelDefinition.ContainsProperty(propertyDefinition.MduPropertyName.ToLower()));

            WaterFlowFMProperty property = modelDefinition.GetModelProperty(propertyName);
            Assert.IsNotNull(property);

            return property;
        }

        private static void RemoveGroupNameFromGroupableFeature(IEnumerable<IGroupableFeature> feature)
        {
            IEnumerable<IGrouping<string, IGroupableFeature>> grouped = feature.GroupBy(e => e.GroupName);
            grouped.First().ForEach(g => g.GroupName = string.Empty);
        }

        private static void DeleteAllFilesAndFoldersInSubDirectory(DirectoryInfo directoryInfo)
        {
            foreach (FileInfo file in directoryInfo.GetFiles())
            {
                file.Delete();
            }

            foreach (DirectoryInfo dir in directoryInfo.GetDirectories())
            {
                DeleteAllFilesAndFoldersInSubDirectory(dir);
                dir.Delete();
            }
        }

        private static void CheckIfFilesWereCopied(string fromFolder, string toFolder, bool checkIfTrue)
        {
            IEnumerable<string> toFolderFileNames = new DirectoryInfo(toFolder).GetFiles().Select(f => f.Name);
            IEnumerable<string> fromFolderFileNames = new DirectoryInfo(fromFolder)
                                                      .GetFiles().Select(f => f.Name)
                                                      .Where(n => !n.EndsWith("_structures.ini") && !n.Contains("gate") &&
                                                                  !n.Contains("pump") && !n.Contains("weir"));
            foreach (string fileName in fromFolderFileNames)
            {
                Assert.That(toFolderFileNames.Contains(fileName), Is.EqualTo(checkIfTrue));
            }
        }

        private static Regex GetRegularExpressionForTextWithEmptyValue(string mduPropertyName)
        {
            return new Regex($@"{mduPropertyName}\s*=\s*\#");
        }

        #endregion
    }
}