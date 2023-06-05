using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.Link1d2d;
using DelftTools.Hydro.Structures;
using DelftTools.TestUtils;
using DelftTools.Utils.Collections;
using DelftTools.Utils.Collections.Generic;
using DelftTools.Utils.IO;
using DelftTools.Utils.Reflection;
using DeltaShell.NGHS.IO.DataObjects;
using DeltaShell.Plugins.FMSuite.FlowFM.FeatureData;
using DeltaShell.Plugins.FMSuite.FlowFM.IO;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using GeoAPI.Geometries;
using NetTopologySuite.Algorithm;
using NetTopologySuite.Extensions.Coverages;
using NetTopologySuite.Extensions.Grids;
using NetTopologySuite.Extensions.Networks;
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
        public void MduFileReadsAndWritesMultipleLinePropertiesIncludingComments(string fileCategoryName, string propertyName, string expectedValues, string expectedOutputComments, string rawValuesAndComments)
        {
            var nameWithoutExtension = Path.GetTempFileName();
            var mduFilePath = string.Concat(nameWithoutExtension, ".mdu");

            var mduFileText = string.Concat(propertyName, rawValuesAndComments);
            mduFileText = string.Concat("[",fileCategoryName, "]", "\n", mduFileText);
            File.WriteAllText(mduFilePath, mduFileText);
            try
            {
                var mduFile = new MduFile();
                var modelDefinition = new WaterFlowFMModelDefinition();
                var allFixedWeirsAndCorrespondingProperties = new List<ModelFeatureCoordinateData<FixedWeir>>();
                var property = AddCustomMultipleFilePropertyToModelDefinition(modelDefinition, propertyName, fileCategoryName);
                
                var convertedFileObjectsForFMModel = new ConvertedFileObjectsForFMModel
                {
                    ModelDefinition = modelDefinition,
                    HydroArea = new HydroArea(),
                    Grid = new UnstructuredGrid(),
                    HydroNetwork = new HydroNetwork(),
                    AllFixedWeirsAndCorrespondingProperties = allFixedWeirsAndCorrespondingProperties
                };

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
        public void MduFileHandlesWrongDeclarationsOfMultipleLineProperties(string fileCategoryName, string property1Name, string property2Name, string property1Value, string property2Value, bool multipleLineProp1, bool multipleLineProp2)
        {
            var nameWithoutExtension = Path.GetTempFileName();
            var mduFilePath = string.Concat(nameWithoutExtension, ".mdu");
            var property1Text = string.Concat(property1Name, "=", property1Value,
                multipleLineProp1 ? @"\" : string.Empty);
            var property2Text = string.Concat(property2Name, "=", property2Value,
                multipleLineProp2 ? @"\" : string.Empty);
            var mduFileText = string.Concat(property1Text, "\n", property2Text);
            mduFileText = string.Concat("[", fileCategoryName, "]", "\n", mduFileText);
            File.WriteAllText(mduFilePath, mduFileText);
            try
            {
                var mduFile = new MduFile();
                var modelDefinition = new WaterFlowFMModelDefinition();
                var convertedFileObjectsForFMModel = new ConvertedFileObjectsForFMModel
                {
                    ModelDefinition = modelDefinition,
                    HydroArea = new HydroArea(),
                    Grid = new UnstructuredGrid(),
                    HydroNetwork = new HydroNetwork(),
                    AllFixedWeirsAndCorrespondingProperties = new List<ModelFeatureCoordinateData<FixedWeir>>()
                };
                var property1 = AddCustomMultipleFilePropertyToModelDefinition(modelDefinition, property1Name, fileCategoryName);
                var property2 = AddCustomMultipleFilePropertyToModelDefinition(modelDefinition, property2Name, fileCategoryName);
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
                        WaterFlowFMMduFileTestHelper.GetNewGroupableFeature2DPoint(string.Empty, "Feature1"), /*Default group expected*/
                    }
                );
                var defaultFeature = area.ObservationPoints.FirstOrDefault(o => o.Name == "Feature1");
                Assert.IsNotNull(defaultFeature);

                var modelDefinition = new WaterFlowFMModelDefinition(mduDir, modelName);
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
        [TestCase(KnownProperties.EnclosureFile, "Value1 Value2", "CustomPropertyTest", "Value3")]
        public void WhenMduExpectsANewMultipleLinePropertyButItIsANewPropertyItKeepsReading(string hydroAreaFileProperty, string expectedCompositeValue, string customPropertyName, string expectedSimpleValue)
        {
            var mduFilePath = TestHelper.GetTestFilePath(@"HydroAreaCollection\FlowFMPropertyWithSlashAndNoNewLine.mdu");
            mduFilePath = TestHelper.CreateLocalCopy(mduFilePath);
            var mduDir = Path.GetDirectoryName(mduFilePath);
            var modelName = Path.GetFileName(mduFilePath);

            var area = new HydroArea();
            var network = new HydroNetwork();
            var modelDefinition = new WaterFlowFMModelDefinition(mduDir, modelName);
            var allFixedWeirsAndCorrespondingProperties = new List<ModelFeatureCoordinateData<FixedWeir>>();
            var mduFile = new MduFile();
            var convertedFileObjectsForFMModel = new ConvertedFileObjectsForFMModel
            {
                ModelDefinition = modelDefinition,
                HydroArea = area,
                HydroNetwork = network,
                AllFixedWeirsAndCorrespondingProperties = allFixedWeirsAndCorrespondingProperties
            };

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
        [Category("Quarantine")]
        public void MduFileReadsFromMultipleFilesAnAssignsGroupNamesToIGroupableFeatures()
        {
            var mduFilePath = TestHelper.GetTestFilePath(@"HydroAreaCollection\FlowFM\FlowFM.mdu");
            mduFilePath = TestHelper.CreateLocalCopy(mduFilePath);
            var mduDir = Path.GetDirectoryName(mduFilePath);
            var modelName = Path.GetFileName(mduFilePath);
            try
            {
                var area = new HydroArea();
                var network = new HydroNetwork();
                var modelDefinition = new WaterFlowFMModelDefinition(mduDir, modelName);
                var allFixedWeirsAndCorrespondingProperties = new List<ModelFeatureCoordinateData<FixedWeir>>();
                var mduFile = new MduFile();
                var convertedFileObjectsForFMModel = new ConvertedFileObjectsForFMModel
                {
                    ModelDefinition = modelDefinition,
                    HydroArea = area,
                    HydroNetwork = network,
                    AllFixedWeirsAndCorrespondingProperties = allFixedWeirsAndCorrespondingProperties
                };

                mduFile.Read(mduFilePath, convertedFileObjectsForFMModel);

                //  2 groups per each feature
                CheckFeatureWasReadCorrectly(area.ObservationPoints, "ObservationPoints", new List<string> { "ObsGroup1_obs.xyn", "ObsGroup2_obs.xyn" });
                CheckFeatureWasReadCorrectly(area.Enclosures, "Enclosures", new List<string> { "EncGroup1_enc.pol", "EncGroup2_enc.pol" });
                /* Dry Points and dry areas are exclusive  XyzFile dryPointFile;*/
                CheckFeatureWasReadCorrectly(area.DryAreas, "DryAreas", new List<string> { "DryGroup1_dry.pol", "DryGroup2_dry.pol" });
                CheckFeatureWasReadCorrectly(area.ThinDams, "ThinDams", new List<string> { "ThdGroup1_thd.pli", "ThdGroup2_thd.pli" });
                CheckFeatureWasReadCorrectly(area.FixedWeirs, "FixedWeirs", new List<string> { "FxwGroup1_fxw.pli", "FxwGroup2_fxw.pli" });
                CheckFeatureWasReadCorrectly(area.ObservationCrossSections, "ObservationCrossSections", new List<string> { "CrsGroup1_crs.pli", "CrsGroup2_crs.pli" });
                CheckFeatureWasReadCorrectly(area.LandBoundaries, "LandBoundaries", new List<string> { "LdbGroup1.ldb", "LdbGroup2.ldb" });

                Assert.AreEqual(2, area.Embankments.Count);
                var embankmentsList = area.Embankments.Select(e => e.Name).ToList();
                var expectedEmbankments = new List<string> {"Embankment01", "Embankment02"};
                Assert.IsFalse(embankmentsList.Any( e => !expectedEmbankments.Contains(e)));
                Assert.IsFalse(expectedEmbankments.Any(e => !embankmentsList.Contains(e)));
               
                /* StructuresFile 
                 * We CAN read from multiple structures files, however these structures will not get the GroupName due to its implementation nature.
                 */
                var structuresGroupNames =
                    new List<string> { "StructuresGroup1_structures.ini", "StructuresGroup2_structures.ini" };
                CheckFeatureWasReadCorrectly(area.Gates, "Gates", structuresGroupNames);
                CheckFeatureWasReadCorrectly(area.Pumps, "Pumps", structuresGroupNames);
                CheckFeatureWasReadCorrectly(area.Weirs, "Weirs", structuresGroupNames);
            }
            finally
            {
                FileUtils.DeleteIfExists(mduFilePath);
            }
        }

        [Test]
        public void MduFileReadsFromMultipleFilesAnAssignsGroupNamesToXyZFileDryPointFeature()
        {
            var mduFilePath = TestHelper.GetTestFilePath(@"HydroAreaCollection\FlowFM\dryPointsMdu.mdu");
            mduFilePath = TestHelper.CreateLocalCopy(mduFilePath);
            var mduDir = Path.GetDirectoryName(mduFilePath);
            var modelName = Path.GetFileName(mduFilePath);
            try
            {
                var area = new HydroArea();
                var network = new HydroNetwork();
                var modelDefinition = new WaterFlowFMModelDefinition(mduDir, modelName);
                var allFixedWeirsAndCorrespondingProperties = new List<ModelFeatureCoordinateData<FixedWeir>>();
                var mduFile = new MduFile();
                
                var convertedFileObjectsForFMModel = new ConvertedFileObjectsForFMModel
                {
                    ModelDefinition = modelDefinition,
                    HydroArea = area,
                    HydroNetwork = network,
                    AllFixedWeirsAndCorrespondingProperties = allFixedWeirsAndCorrespondingProperties
                };

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
                FileCategoryName = "TestCategory",
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
                        WaterFlowFMMduFileTestHelper.GetNewGroupableFeature2DPoint("", "Feature1"), /*Default group expected*/ WaterFlowFMMduFileTestHelper.GetNewGroupableFeature2DPoint("Group1", "Feature2"), WaterFlowFMMduFileTestHelper.GetNewGroupableFeature2DPoint("Group1", "Feature3"), WaterFlowFMMduFileTestHelper.GetNewGroupableFeature2DPoint("Group2", "Feature4")
                    }
                );
                var defaultFeature = area.ObservationPoints.FirstOrDefault(o => o.Name == "Feature1");
                Assert.IsNotNull(defaultFeature);

                var modelDefinition = new WaterFlowFMModelDefinition(mduDir, modelName);
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
        public void GivenMduFileWithDryPointsAndDryAreasFilesBothOnDryPointsFileCategory_WhenReadingMdu_ThenBothFilesAreCorrectlyRead()
        {
            var mduFilePath = TestHelper.GetTestFilePath(@"HydroAreaCollection\FlowFM\dryPointsAndAreasInModel.mdu");
            mduFilePath = TestHelper.CreateLocalCopy(mduFilePath);
            var mduDir = Path.GetDirectoryName(mduFilePath);
            Assert.NotNull(mduDir);
            var modelName = Path.GetFileName(mduFilePath);

            var mduFile = new MduFile();
            var area = new HydroArea();
            var network = new HydroNetwork();
            var modelDefinition = new WaterFlowFMModelDefinition(mduDir, modelName);
            var allFixedWeirsAndCorrespondingProperties = new List<ModelFeatureCoordinateData<FixedWeir>>();

            var convertedFileObjectsForFMModel = new ConvertedFileObjectsForFMModel
            {
                ModelDefinition = modelDefinition,
                HydroArea = area,
                HydroNetwork = network,
                AllFixedWeirsAndCorrespondingProperties = allFixedWeirsAndCorrespondingProperties
            };

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
            var area = new HydroArea();
            var network = new HydroNetwork();
            var modelDefinition = new WaterFlowFMModelDefinition(mduDir, modelName);
            var allFixedWeirsAndCorrespondingProperties = new List<ModelFeatureCoordinateData<FixedWeir>>();

            var convertedFileObjectsForFMModel = new ConvertedFileObjectsForFMModel
            {
                ModelDefinition = modelDefinition,
                HydroArea = area,
                HydroNetwork = network,
                AllFixedWeirsAndCorrespondingProperties = allFixedWeirsAndCorrespondingProperties
            };

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
            var area = new HydroArea();
            var network = new HydroNetwork();
            var modelDefinition = new WaterFlowFMModelDefinition(mduDir, modelName);
            var allFixedWeirsAndCorrespondingProperties = new List<ModelFeatureCoordinateData<FixedWeir>>();

            var convertedFileObjectsForFMModel = new ConvertedFileObjectsForFMModel
            {
                ModelDefinition = modelDefinition,
                HydroArea = area,
                HydroNetwork = network,
                AllFixedWeirsAndCorrespondingProperties = allFixedWeirsAndCorrespondingProperties
            };

            mduFile.Read(mduFilePath, convertedFileObjectsForFMModel);

            Assert.IsEmpty(area.DryPoints); //Check this, because dry areas and dry points are read in the same method (MduFile.ReadDryPointsAndDryAreas)
            Assert.That(area.DryAreas.Count, Is.EqualTo(1));
        }

        [Test, Ignore("We are not able to read 1D2D-models at the moment, so this test will fail at the moment.")] /* Roundtrip test */
        [Category("ToCheck")]
        public void MduFileReadsAndWritesIGroupableFeatures()
        {
            var mduFilePath = TestHelper.GetTestFilePath(@"HydroAreaCollection\FlowFM\FlowFM.mdu");
            mduFilePath = TestHelper.CreateLocalCopy(mduFilePath);
            var mduDir = Path.GetDirectoryName(mduFilePath);
            Assert.NotNull(mduDir);
            var modelName = Path.GetFileName(mduFilePath);

            var saveDirectory = Path.Combine(mduDir, "MduFileReadsAndWritesTest");
            Directory.CreateDirectory(saveDirectory);
            var savePath = Path.Combine(saveDirectory, "SaveFlowFM.mdu");
            var newMduDir = Path.GetDirectoryName(savePath);
            var newMduName = Path.GetFileName(savePath);
            try
            {
                var mduFile = new MduFile();

                var originalArea = new HydroArea();
                var originalNetwork = new HydroNetwork();
                var originalMD = new WaterFlowFMModelDefinition(mduDir, modelName);
                var allFixedWeirsAndCorrespondingProperties = new List<ModelFeatureCoordinateData<FixedWeir>>();
                var convertedFileObjectsForFMModelOriginal = new ConvertedFileObjectsForFMModel
                {
                    ModelDefinition = originalMD,
                    HydroArea = originalArea,
                    HydroNetwork = originalNetwork,
                    AllFixedWeirsAndCorrespondingProperties = allFixedWeirsAndCorrespondingProperties
                };

                mduFile.Read(mduFilePath, convertedFileObjectsForFMModelOriginal);
                mduFile.Write(savePath, originalMD, originalArea, null, null, null, null, null, null, allFixedWeirsAndCorrespondingProperties, switchTo: false);

                var savedArea = new HydroArea();
                var savedNetwork = new HydroNetwork();
                var savedMD = new WaterFlowFMModelDefinition(newMduDir, newMduName);

                var convertedFileObjectsForFMModelSaved = new ConvertedFileObjectsForFMModel
                {
                    ModelDefinition = savedMD,
                    HydroArea = savedArea,
                    HydroNetwork = savedNetwork,
                    AllFixedWeirsAndCorrespondingProperties = allFixedWeirsAndCorrespondingProperties
                };

                mduFile.Read(savePath, convertedFileObjectsForFMModelSaved);

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

                foreach (var property in listOfProperties)
                {
                    CompareHydroAreaModelProperties(property, savePath, originalMD, savedMD);
                }

                CompareHydroAreaFeatures(originalArea, savedArea);

                /* Embankments */
                Assert.AreEqual(originalArea.Embankments.Count, savedArea.Embankments.Count);
                var expectedEmbankments = originalArea.Embankments.Select(e => e.Name).ToList();
                var savedEmbankments = savedArea.Embankments.Select(e => e.Name).ToList();
                Assert.IsFalse(savedEmbankments.Any(e => !expectedEmbankments.Contains(e)));
                Assert.IsFalse(expectedEmbankments.Any(e => !savedEmbankments.Contains(e)));

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
            var mduFilePath = TestHelper.GetTestFilePath(@"HydroAreaCollection\FlowFM\dryPointsMdu.mdu");
            mduFilePath = TestHelper.CreateLocalCopy(mduFilePath);
            var mduDir = Path.GetDirectoryName(mduFilePath);
            Assert.NotNull(mduDir);
            var modelName = Path.GetFileName(mduFilePath);

            var saveDirectory = Path.Combine(mduDir, "MduFileReadsAndWritesTest");
            Directory.CreateDirectory(saveDirectory);
            var savePath = Path.Combine(saveDirectory, "SaveDryPoint.mdu");
            var newMduDir = Path.GetDirectoryName(savePath);
            var newMduName = Path.GetFileName(savePath);
            try
            {
                var mduFile = new MduFile();

                var originalArea = new HydroArea();
                var network = new HydroNetwork();
                var originalMd = new WaterFlowFMModelDefinition(mduDir, modelName);
                var allFixedWeirsAndCorrespondingProperties = new List<ModelFeatureCoordinateData<FixedWeir>>();
                var convertedFileObjectsForFMModelOriginal = new ConvertedFileObjectsForFMModel
                {
                    ModelDefinition = originalMd,
                    HydroArea = originalArea,
                    HydroNetwork = new HydroNetwork(),
                    AllFixedWeirsAndCorrespondingProperties = allFixedWeirsAndCorrespondingProperties
                };
                mduFile.Read(mduFilePath, convertedFileObjectsForFMModelOriginal);
                mduFile.Write(savePath, originalMd, originalArea, null, null, null, null, null, null, allFixedWeirsAndCorrespondingProperties, switchTo: false, writeExtForcings: false);

                var savedArea = new HydroArea();
                var savedNetwork = new HydroNetwork();
                var savedMd = new WaterFlowFMModelDefinition(newMduDir, newMduName);
                var convertedFileObjectsForFMModelSaved = new ConvertedFileObjectsForFMModel
                {
                    ModelDefinition = savedMd,
                    HydroArea = savedArea,
                    HydroNetwork = savedNetwork,
                    AllFixedWeirsAndCorrespondingProperties = allFixedWeirsAndCorrespondingProperties
                };
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

        [Test, Ignore("We are not able to read 1D2D-models at the moment, so this test will fail at the moment.")] /* Roundtrip test */
        [Category("ToCheck")]
        public void MduFileWritesDefaultValueForIGroupableFeatures()
        {
            var mduFilePath = TestHelper.GetTestFilePath(@"HydroAreaCollection\FlowFM\FlowFM.mdu");
            mduFilePath = TestHelper.CreateLocalCopy(mduFilePath);
            var mduDir = Path.GetDirectoryName(mduFilePath);
            Assert.NotNull(mduDir);
            var modelName = Path.GetFileName(mduFilePath);

            var saveDirectory = Path.Combine(mduDir, "MduFileReadsAndWritesTest");
            Directory.CreateDirectory(saveDirectory);
            var savePath = Path.Combine(saveDirectory, "SaveFlowFM.mdu");
            var newMduDir = Path.GetDirectoryName(savePath);
            var newMduName = Path.GetFileName(savePath);
            try
            {
                var mduFile = new MduFile();

                var originalArea = new HydroArea();
                var network = new HydroNetwork();
                var originalMD = new WaterFlowFMModelDefinition(mduDir, modelName);
                var allFixedWeirsAndCorrespondingProperties = new List<ModelFeatureCoordinateData<FixedWeir>>();
                var convertedFileObjectsForFMModelOriginal = new ConvertedFileObjectsForFMModel
                {
                    ModelDefinition = originalMD,
                    HydroArea = originalArea,
                    HydroNetwork = network,
                    AllFixedWeirsAndCorrespondingProperties = allFixedWeirsAndCorrespondingProperties
                };
                mduFile.Read(mduFilePath, convertedFileObjectsForFMModelOriginal);

                //Remove one of the selected group names to make it ' default' .
                RemoveGroupNameFromGroupableFeature(originalArea.ObservationPoints);
                RemoveGroupNameFromGroupableFeature(originalArea.Enclosures);
                RemoveGroupNameFromGroupableFeature(originalArea.DryAreas);
                RemoveGroupNameFromGroupableFeature(originalArea.ThinDams);
                RemoveGroupNameFromGroupableFeature(originalArea.FixedWeirs);
                RemoveGroupNameFromGroupableFeature(originalArea.ObservationCrossSections);
                RemoveGroupNameFromGroupableFeature(originalArea.LandBoundaries);
                RemoveGroupNameFromGroupableFeature(originalArea.Gates);
                RemoveGroupNameFromGroupableFeature(originalArea.Pumps);
                RemoveGroupNameFromGroupableFeature(originalArea.Weirs);

                mduFile.Write(savePath, originalMD, originalArea, null, null, null, null,null, null, allFixedWeirsAndCorrespondingProperties, switchTo: false);

                var savedArea = new HydroArea();
                var savedNetwork = new HydroNetwork();
                var savedMD = new WaterFlowFMModelDefinition(newMduDir, newMduName);
                var convertedFileObjectsForFMModelSaved = new ConvertedFileObjectsForFMModel
                {
                    ModelDefinition = savedMD,
                    HydroArea = savedArea,
                    HydroNetwork = savedNetwork,
                    AllFixedWeirsAndCorrespondingProperties = allFixedWeirsAndCorrespondingProperties
                }; 
                mduFile.Read(savePath, convertedFileObjectsForFMModelSaved);

                CompareHydroAreaFeatures(originalArea, savedArea);
                //Check default group was created.
                var mduPathName = Path.GetFileNameWithoutExtension(savePath);
                CheckDefaultGroupIsInFeature("LandBoundaries", originalArea.LandBoundaries, mduPathName, MduFile.LandBoundariesExtension);
                CheckDefaultGroupIsInFeature("FixedWeirs", originalArea.FixedWeirs, mduPathName, MduFile.FixedWeirExtension);
                CheckDefaultGroupIsInFeature("ObservationPoints", originalArea.ObservationPoints, mduPathName, MduFile.ObsExtension);
                CheckDefaultGroupIsInFeature("ObservationCrossSections", originalArea.ObservationCrossSections, mduPathName, MduFile.ObsCrossExtension);
                CheckDefaultGroupIsInFeature("DryAreas", originalArea.DryAreas, mduPathName, MduFile.DryAreaExtension);
                CheckDefaultGroupIsInFeature("Enclosures", originalArea.Enclosures, mduPathName, MduFile.EnclosureExtension);
                CheckDefaultGroupIsInFeature("Gates", originalArea.Gates, mduPathName, MduFile.StructuresExtension);
                CheckDefaultGroupIsInFeature("Pumps", originalArea.Pumps, mduPathName, MduFile.StructuresExtension);
                CheckDefaultGroupIsInFeature("Weirs", originalArea.Weirs, mduPathName, MduFile.StructuresExtension);
            }
            finally
            {
                FileUtils.DeleteIfExists(mduFilePath);
                FileUtils.DeleteIfExists(savePath);
                FileUtils.DeleteIfExists(saveDirectory);
            }
        }

        [Test]
        [Category(TestCategory.WorkInProgress)]
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
            var newMduDir = Path.GetDirectoryName(savePath);
            var newMduName = Path.GetFileName(savePath);
            try
            {
                var mduFile = new MduFile();

                var originalArea = new HydroArea();
                var originalNetwork = new HydroNetwork();
                var originalMd = new WaterFlowFMModelDefinition(mduDir, modelName);
                var allFixedWeirsAndCorrespondingProperties = new List<ModelFeatureCoordinateData<FixedWeir>>();
                var convertedFileObjectsForFMModelOriginal = new ConvertedFileObjectsForFMModel
                {
                    ModelDefinition = originalMd,
                    HydroArea = originalArea,
                    HydroNetwork = originalNetwork,
                    AllFixedWeirsAndCorrespondingProperties = allFixedWeirsAndCorrespondingProperties
                };
                mduFile.Read(mduFilePath, convertedFileObjectsForFMModelOriginal);

                RemoveGroupNameFromGroupableFeature(originalArea.DryPoints);
                mduFile.Write(savePath, originalMd, originalArea, null, null, null, null,null, null, allFixedWeirsAndCorrespondingProperties, switchTo: false, writeExtForcings: false);

                var savedArea = new HydroArea();
                var savedNetwork = new HydroNetwork();
                var savedMd = new WaterFlowFMModelDefinition(newMduDir, newMduName);
                var convertedFileObjectsForFMModelSaved = new ConvertedFileObjectsForFMModel
                {
                    ModelDefinition = savedMd,
                    HydroArea = savedArea,
                    HydroNetwork = savedNetwork,
                    AllFixedWeirsAndCorrespondingProperties = allFixedWeirsAndCorrespondingProperties
                };

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
            var mduDir = Path.GetDirectoryName(mduFilePath);
            var modelName = Path.GetFileName(mduFilePath);

            try
            {
                var area = new HydroArea();
                var network = new HydroNetwork();
                var modelDefinition = new WaterFlowFMModelDefinition(mduDir, modelName);
                var allFixedWeirsAndCorrespondingProperties = new List<ModelFeatureCoordinateData<FixedWeir>>();
                var mduFile = new MduFile();
                var convertedFileObjectsForFMModel = new ConvertedFileObjectsForFMModel
                {
                    ModelDefinition = modelDefinition,
                    HydroArea = area,
                    HydroNetwork = network,
                    AllFixedWeirsAndCorrespondingProperties = allFixedWeirsAndCorrespondingProperties
                };

                mduFile.Read(mduFilePath, convertedFileObjectsForFMModel);

                Assert.AreEqual(expectedEnclosures, area.Enclosures.Count);
            }
            finally
            {
                FileUtils.DeleteIfExists(mduFilePath);
            }
        }

        [Test]
        [Category("Quarantine")]
        public void GivenAProjectFolderWithFeatureFilesOutsideOfTheMduFolder_WhenReadingMdu_ThenAllFilesAreCopiedAndRead()
        {
            // Preparations
            const string baseFolderPath = @"HydroAreaCollection\MduFileProjects";
            var relativeMduFilePath = @"FilesOutsideMduFolderProject.dsproj_data\FlowFM\FlowFM.mdu";
            var testDir = TestHelper.GetTestDataDirectoryPathForAssembly(GetType().Assembly);

            var originalFolderPath = Path.Combine(testDir, baseFolderPath);
            
            var testWorkingFolder = TestHelper.CreateLocalCopy(originalFolderPath);
            var mduPath = Path.Combine(testWorkingFolder, relativeMduFilePath);

            CheckIfFilesWereCopied(originalFolderPath, testWorkingFolder);

            var modelName = Path.GetFileNameWithoutExtension(mduPath);
            var area = new HydroArea();
            var network = new HydroNetwork();
            var modelDefinition = new WaterFlowFMModelDefinition(mduPath, modelName);
            var allFixedWeirsAndCorrespondingProperties = new List<ModelFeatureCoordinateData<FixedWeir>>();
            var mduFile = new MduFile();
            var convertedFileObjectsForFMModel = new ConvertedFileObjectsForFMModel
            {
                ModelDefinition = modelDefinition,
                HydroArea = area,
                HydroNetwork = network,
                AllFixedWeirsAndCorrespondingProperties = allFixedWeirsAndCorrespondingProperties
            };

            mduFile.Read(mduPath, convertedFileObjectsForFMModel);

            // Check if all features were read
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

            CheckIfFilesWereCopied(originalFolderPath, testWorkingFolder);
        }

        [Test]
        [TestCase(@"FilesInsideMduSubFolderProject.dsproj_data\FlowFM\FlowFM.mdu")]
        [TestCase(@"FilesInsideMduSubFolderButWithRelativePathsProject.dsproj_data\FlowFM\FlowFM.mdu")]
        [Category("Quarantine")]
        public void GivenAProjectFolderWithFeatureFilesInsideOfAnMduSubFolder_WhenReadingMdu_ThenAllFilesAreRead(string mduProjectFilePath)
        {
            // Preparations
            const string baseFolderPath = @"HydroAreaCollection\MduFileProjects";
            var testDir = TestHelper.GetTestDataDirectoryPathForAssembly(GetType().Assembly);

            var originalFolderPath = Path.Combine(testDir, baseFolderPath);
            var mduPath = Path.Combine(originalFolderPath, mduProjectFilePath);
            var featureFileDirectory = Path.Combine(testDir, baseFolderPath, @"FeatureFiles");

            var modelName = Path.GetFileNameWithoutExtension(mduPath);
            var area = new HydroArea();
            var network = new HydroNetwork();
            var modelDefinition = new WaterFlowFMModelDefinition(mduPath, modelName);
            var allFixedWeirsAndCorrespondingProperties = new List<ModelFeatureCoordinateData<FixedWeir>>();
            var mduFile = new MduFile();
            var convertedFileObjectsForFMModel = new ConvertedFileObjectsForFMModel
            {
                ModelDefinition = modelDefinition,
                HydroArea = area,
                HydroNetwork = network,
                AllFixedWeirsAndCorrespondingProperties = allFixedWeirsAndCorrespondingProperties
            };

            mduFile.Read(mduPath, convertedFileObjectsForFMModel);

            // Check if all features were read
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

            CheckIfFilesWereCopied(featureFileDirectory, Path.GetDirectoryName(mduPath));
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
            var area = new HydroArea();
            var network = new HydroNetwork();
            var modelDefinition = new WaterFlowFMModelDefinition(mduPath, modelName);
            var allFixedWeirsAndCorrespondingProperties = new List<ModelFeatureCoordinateData<FixedWeir>>();
            var mduFile = new MduFile();
            var convertedFileObjectsForFMModel = new ConvertedFileObjectsForFMModel
            {
                ModelDefinition = modelDefinition,
                HydroArea = area,
                HydroNetwork = network,
                AllFixedWeirsAndCorrespondingProperties = allFixedWeirsAndCorrespondingProperties
            };

            mduFile.Read(mduPath, convertedFileObjectsForFMModel);

            // Check if all features were read
            Assert.That(area.ObservationPoints.Count, Is.EqualTo(1));
        }

        [Test]
        public void GivenAnMduFileWithNonExistentReferenceToFile_WhenReadingMduFile_ThenTheUserGetsAWarningMessage()
        {
            // Preparations
            const string baseFolderPath = @"HydroAreaCollection\MduFileProjects";
            var filePath = @"MissingFileMdu\FlowFM.mdu";
            var testDir = TestHelper.GetTestDataDirectoryPathForAssembly(GetType().Assembly);

            var originalFolderPath = Path.Combine(testDir, baseFolderPath);
            var mduPath = Path.Combine(originalFolderPath, filePath);
            
            var modelName = Path.GetFileNameWithoutExtension(filePath);
            var area = new HydroArea();
            var network = new HydroNetwork();
            var modelDefinition = new WaterFlowFMModelDefinition(mduPath, modelName);
            var allFixedWeirsAndCorrespondingProperties = new List<ModelFeatureCoordinateData<FixedWeir>>();
            var mduFile = new MduFile();
            var convertedFileObjectsForFMModel = new ConvertedFileObjectsForFMModel
            {
                ModelDefinition = modelDefinition,
                HydroArea = area,
                HydroNetwork = network,
                AllFixedWeirsAndCorrespondingProperties = allFixedWeirsAndCorrespondingProperties
            };

            TestHelper.AssertAtLeastOneLogMessagesContains(() =>
            {
                mduFile.Read(mduPath, convertedFileObjectsForFMModel);
            }, "' does not exist, but is defined in MDU file at '");

            // Check if all features were read
            Assert.That(area.ObservationPoints.Count, Is.EqualTo(0));
        }

        [Test]
        [TestCase(@"MissingFeatureFilesProject.dsproj_data\FlowFM.mdu")]
        [TestCase(@"DuplicateFilesProject.dsproj_data\FlowFM\FlowFM.mdu")]
        public void GivenMduFileWithReferencesToNonExistentFilesOrFileNamesThatAlreadyExistInTheMduFolder_WhenReadingMdu_ThenTheseFeaturesAreNotRead(string mduProjectFilePath)
        {
            // Preparations
            const string baseFolderPath = @"HydroAreaCollection\MduFileProjects";
            var testDir = TestHelper.GetTestDataDirectoryPathForAssembly(GetType().Assembly);

            var originalFolderPath = Path.Combine(testDir, baseFolderPath);
            
            var testWorkingFolder = TestHelper.CreateLocalCopy(originalFolderPath);
            var mduPath = Path.Combine(testWorkingFolder, mduProjectFilePath);

            var modelName = Path.GetFileNameWithoutExtension(mduProjectFilePath);
            var area = new HydroArea();
            var network = new HydroNetwork();
            var modelDefinition = new WaterFlowFMModelDefinition(mduPath, modelName);
            var mduFile = new MduFile();
            var allFixedWeirsAndCorrespondingProperties = new List<ModelFeatureCoordinateData<FixedWeir>>();
            var convertedFileObjectsForFMModel = new ConvertedFileObjectsForFMModel
            {
                ModelDefinition = modelDefinition,
                HydroArea = area,
                HydroNetwork = network,
                AllFixedWeirsAndCorrespondingProperties = allFixedWeirsAndCorrespondingProperties
            };

            mduFile.Read(mduPath, convertedFileObjectsForFMModel);

            // Check if all features were read
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
            var network = new HydroNetwork();
            var name1 = "ObsPoint01";
            var name2 = "ObsPoint02";
            area.ObservationPoints.AddRange(
                new[]
                {
                    WaterFlowFMMduFileTestHelper.GetNewGroupableFeature2DPoint(groupName1, name1), WaterFlowFMMduFileTestHelper.GetNewGroupableFeature2DPoint(groupName2, name2)
                }
            );

            var modelDefinition = new WaterFlowFMModelDefinition(mduDir, modelName);
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

            area = new HydroArea();
            var convertedFileObjectsForFMModel = new ConvertedFileObjectsForFMModel
            {
                ModelDefinition = modelDefinition,
                HydroArea = area,
                HydroNetwork = network,
                AllFixedWeirsAndCorrespondingProperties = allFixedWeirsAndCorrespondingProperties
            };

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

            var modelDefinition = new WaterFlowFMModelDefinition(mduDir, modelName);
            var allFixedWeirsAndCorrespondingProperties = new List<ModelFeatureCoordinateData<FixedWeir>>();
            mduFile.Write(mduFilePath, modelDefinition, area, null, null, null, null, null,null, allFixedWeirsAndCorrespondingProperties);

            var newArea = new HydroArea();
            var network = new HydroNetwork();
            var convertedFileObjectsForFMModel = new ConvertedFileObjectsForFMModel
            {
                ModelDefinition = modelDefinition,
                HydroArea = newArea,
                HydroNetwork = network,
                AllFixedWeirsAndCorrespondingProperties = allFixedWeirsAndCorrespondingProperties
            };
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

            var modelDefinition = new WaterFlowFMModelDefinition(mduDir, modelName);
            var allFixedWeirsAndCorrespondingProperties = new List<ModelFeatureCoordinateData<FixedWeir>>();
            mduFile.Write(mduFilePath, modelDefinition, area, null, null, null, null,null, null, allFixedWeirsAndCorrespondingProperties);

            var readAllText = File.ReadAllText(mduFilePath);

            var dryPointsFileNameWithExtension = dryPointsGroupName + MduFile.DryPointExtension;
            var dryAreasFileNameWithExtension = dryAreasGroupName + MduFile.DryAreaExtension;

            var expectedDryPointsFileText = GetExpectedFileText("DryPointsFile", dryPointsFileNameWithExtension + " " + dryAreasFileNameWithExtension);

            Assert.IsTrue(readAllText.Contains(expectedDryPointsFileText), "Expected {0} \n Generated: {1}", expectedDryPointsFileText, readAllText);

            Assert.IsTrue(File.Exists(Path.Combine(mduDir, dryPointsFileNameWithExtension)));
            Assert.IsTrue(File.Exists(Path.Combine(mduDir, dryAreasFileNameWithExtension)));

            FileUtils.DeleteIfExists(mduFilePath);
            FileUtils.DeleteIfExists(mduFilePath.Replace(".mdu", string.Empty));
            DeleteAllFilesAndFoldersInSubDirectory(new DirectoryInfo(Path.GetDirectoryName(Path.Combine(mduDir, dryPointsFileNameWithExtension))));
        }

        [Test]
        public void GivenFeaturesWithGroupNamesThatPointToSubFolders_WhenWriting_ThenMduFileAndFeatureFilesAreBeingWritten()
        {
            var mduFilePath = string.Concat(Path.GetTempFileName(), ".mdu");
            var mduFile = new MduFile();
            var mduDir = Path.GetDirectoryName(mduFilePath);
            var modelName = Path.GetFileName(Path.GetFileNameWithoutExtension(mduFilePath));

            Assert.NotNull(mduDir);

            var area = new HydroArea();

            var pointGroupName = @"featureFiles/myObsPoints";
            area.ObservationPoints.AddRange(
                new[]
                    {
                        WaterFlowFMMduFileTestHelper.GetNewGroupableFeature2DPoint(pointGroupName, "ObsPoint01"), WaterFlowFMMduFileTestHelper.GetNewGroupableFeature2DPoint(pointGroupName, "ObsPoint02")
                    }
                );

            var enclosureGroupName = @"featureFiles/myPolygons";
            area.Enclosures.AddRange(
                new []
                    {
                        WaterFlowFMMduFileTestHelper.GetNewGroupableFeature2DPolygon(enclosureGroupName, "Polygon01"), WaterFlowFMMduFileTestHelper.GetNewGroupableFeature2DPolygon(enclosureGroupName, "Polygon02")
                    }
                );

            var dryPointsGroupName = @"featureFiles/myDryPoints";
            area.DryPoints.AddRange(
                new[]
                {
                    WaterFlowFMMduFileTestHelper.GetNewGroupablePointFeature(dryPointsGroupName), WaterFlowFMMduFileTestHelper.GetNewGroupablePointFeature(dryPointsGroupName)
                }
            );

            var landBoundariesGroupName = @"featureFiles/myLandBoundaries";
            area.LandBoundaries.AddRange(
                new[]
                {
                    WaterFlowFMMduFileTestHelper.GetNewLandBoundary2D(landBoundariesGroupName, "LandBoundary01"), WaterFlowFMMduFileTestHelper.GetNewLandBoundary2D(landBoundariesGroupName, "LandBoundary02")
                }
            );

            var structureGroupName = @"featureFiles/myGates";
            area.Gates.AddRange(
                new[]
                {
                    WaterFlowFMMduFileTestHelper.GetNewGate2D(structureGroupName, "Gate01"), WaterFlowFMMduFileTestHelper.GetNewGate2D(structureGroupName, "Gate02")
                }
            );

            var modelDefinition = new WaterFlowFMModelDefinition(mduDir, modelName);
            var allFixedWeirsAndCorrespondingProperties = new List<ModelFeatureCoordinateData<FixedWeir>>();
            mduFile.Write(mduFilePath, modelDefinition, area, null, null, null, null, null,null, allFixedWeirsAndCorrespondingProperties);
                
            var readAllText = File.ReadAllText(mduFilePath);

            var obsFileNameWithExtension = pointGroupName + MduFile.ObsExtension;
            var enclosureFileNameWithExtension = enclosureGroupName + MduFile.EnclosureExtension;
            var dryPointsFileNameWithExtension = dryPointsGroupName + MduFile.DryPointExtension;
            var landBoundariesFileNameWithExtension = landBoundariesGroupName + MduFile.LandBoundariesExtension;

            var expectedObsFileText = GetExpectedFileText(modelDefinition.GetModelProperty(KnownProperties.ObsFile).PropertyDefinition.Caption, obsFileNameWithExtension);
            var expectedEnclosureFileText = GetExpectedFileText(modelDefinition.GetModelProperty(KnownProperties.EnclosureFile).PropertyDefinition.Caption, enclosureFileNameWithExtension);
            var expectedDryPointsFileText = GetExpectedFileText(modelDefinition.GetModelProperty(KnownProperties.DryPointsFile).PropertyDefinition.Caption, dryPointsFileNameWithExtension);
            var expectedLandBoundariesFileText = GetExpectedFileText(modelDefinition.GetModelProperty(KnownProperties.LandBoundaryFile).PropertyDefinition.Caption, landBoundariesFileNameWithExtension);
            
            Assert.IsTrue(readAllText.Contains(expectedObsFileText), "Expected {0} \n Generated: {1}", expectedObsFileText, readAllText);
            Assert.IsTrue(readAllText.Contains(expectedEnclosureFileText), "Expected {0} \n Generated: {1}", expectedEnclosureFileText, readAllText);
            Assert.IsTrue(readAllText.Contains(expectedDryPointsFileText), "Expected {0} \n Generated: {1}", expectedDryPointsFileText, readAllText);
            Assert.IsTrue(readAllText.Contains(expectedLandBoundariesFileText), "Expected {0} \n Generated: {1}", expectedLandBoundariesFileText, readAllText);

            Assert.IsTrue(File.Exists(Path.Combine(mduDir, obsFileNameWithExtension)));
            Assert.IsTrue(File.Exists(Path.Combine(mduDir, enclosureFileNameWithExtension)));
            Assert.IsTrue(File.Exists(Path.Combine(mduDir, dryPointsFileNameWithExtension)));
            Assert.IsTrue(File.Exists(Path.Combine(mduDir, landBoundariesFileNameWithExtension)));

            DeleteAllFilesAndFoldersInSubDirectory(new DirectoryInfo(Path.GetDirectoryName(Path.Combine(mduDir, obsFileNameWithExtension))));
        }

        [Test]
        public void GivenFeaturesWithInvalidGroupNames_WhenWriting_ThenTheseFeaturesAreNotSaved()
        {
            var mduFilePath = string.Concat(Path.GetTempFileName(), ".mdu");
            var mduFile = new MduFile();
            var mduDir = Path.GetDirectoryName(mduFilePath);
            var modelName = Path.GetFileName(Path.GetFileNameWithoutExtension(mduFilePath));

            Assert.NotNull(mduDir);

            var area = new HydroArea();

            var pointGroupName = @"..\myObsPoints";
            area.ObservationPoints.AddRange(
                new[]
                {
                    WaterFlowFMMduFileTestHelper.GetNewGroupableFeature2DPoint(pointGroupName, "ObsPoint01"), WaterFlowFMMduFileTestHelper.GetNewGroupableFeature2DPoint(pointGroupName, "ObsPoint02")
                }
            );

            var dryPointsGroupName = @"featureFiles/myDryPoints";
            area.DryPoints.AddRange(
                new[]
                {
                    WaterFlowFMMduFileTestHelper.GetNewGroupablePointFeature(dryPointsGroupName), WaterFlowFMMduFileTestHelper.GetNewGroupablePointFeature(dryPointsGroupName)
                }
            );

            var obsFileNameWithExtension = pointGroupName + MduFile.ObsExtension;
            var dryPointsFileNameWithExtension = dryPointsGroupName + MduFile.DryPointExtension;
            DeleteAllFilesAndFoldersInSubDirectory(new DirectoryInfo(Path.GetDirectoryName(Path.Combine(mduDir, dryPointsFileNameWithExtension))));

            var modelDefinition = new WaterFlowFMModelDefinition(mduDir, modelName);
            var allFixedWeirsAndCorrespondingProperties = new List<ModelFeatureCoordinateData<FixedWeir>>();
            mduFile.Write(mduFilePath, modelDefinition, area, null, null, null, null, null,null, allFixedWeirsAndCorrespondingProperties);

            var readAllText = File.ReadAllText(mduFilePath);

            var expectedObsFileText = GetExpectedFileTextWithEmptyValue("ObsFile");
            var expectedDryPointsFileText = GetExpectedFileText("DryPointsFile", dryPointsFileNameWithExtension);

            Assert.IsTrue(readAllText.Contains(expectedObsFileText), "Expected {0} \n Generated: {1}", expectedObsFileText, readAllText);
            Assert.IsTrue(readAllText.Contains(expectedDryPointsFileText), "Expected {0} \n Generated: {1}", expectedDryPointsFileText, readAllText);

            Assert.IsFalse(File.Exists(Path.Combine(mduDir, obsFileNameWithExtension)));
            Assert.IsTrue(File.Exists(Path.Combine(mduDir, dryPointsFileNameWithExtension)));

            DeleteAllFilesAndFoldersInSubDirectory(new DirectoryInfo(Path.GetDirectoryName(Path.Combine(mduDir, dryPointsFileNameWithExtension))));
        }

        [Test]
        public void GivenMduFileReferencingAnExistingFeatureFile_WhenLoadingAndRenamingTheFeatureWithARelativePath_ThenReferenceInMduFileIsDeleted()
        {
            var initialGroupName = "FlowFM_thd.pli";
            var newGroupName = "../" + initialGroupName;

            // Preparations
            const string baseFolderPath = @"HydroAreaCollection\MduFileProjects";
            var testDir = TestHelper.GetTestDataDirectoryPathForAssembly(GetType().Assembly);

            var originalFolderPath = Path.Combine(testDir, baseFolderPath);
            var relativePath = @"ChangeFeatureGroupNameMduTest\FlowFM.mdu";

            var testWorkingFolder = TestHelper.CreateLocalCopy(originalFolderPath);
            var mduPath = Path.Combine(testWorkingFolder, relativePath);

            var modelName = Path.GetFileNameWithoutExtension(mduPath);
            var area = new HydroArea();
            var network = new HydroNetwork();
            var modelDefinition = new WaterFlowFMModelDefinition(mduPath, modelName);
            var allFixedWeirsAndCorrespondingProperties = new List<ModelFeatureCoordinateData<FixedWeir>>();
            var mduFile = new MduFile();
            var convertedFileObjectsForFMModel = new ConvertedFileObjectsForFMModel
            {
                ModelDefinition = modelDefinition,
                HydroArea = area,
                HydroNetwork = network,
                AllFixedWeirsAndCorrespondingProperties = allFixedWeirsAndCorrespondingProperties
            };

            mduFile.Read(mduPath, convertedFileObjectsForFMModel);

            // Check initial settings
            var thinDams = area.ThinDams;
            Assert.IsNotNull(thinDams);
            Assert.That(thinDams.Count, Is.EqualTo(1));
            Assert.That(thinDams.FirstOrDefault().GroupName, Is.EqualTo(initialGroupName));

            // Change group name and write to mdu file
            area.ThinDams.ForEach(td => td.GroupName = newGroupName);
            mduFile.Write(mduPath, modelDefinition, area, null, null, null,null, null, null, allFixedWeirsAndCorrespondingProperties);

            var readAllText = File.ReadAllText(mduPath);
            var expectedThinDamFileText = GetExpectedFileTextWithEmptyValue("ThinDamFile");
            Assert.IsTrue(readAllText.Contains(expectedThinDamFileText), "Expected {0} \n Generated: {1}", expectedThinDamFileText, readAllText);

            DeleteAllFilesAndFoldersInSubDirectory(new DirectoryInfo(Path.GetDirectoryName(Path.Combine(testWorkingFolder, initialGroupName))));
        }

        [Test]
        public void GivenAbsolutePathNameForFeatures_WhenWriting_ThenWarnWhenPathIsNotInSubFolderOfMduFolder()
        {
            var mduFilePath = string.Concat(Path.GetTempFileName(), ".mdu");
            var mduFile = new MduFile();
            var mduDir = Path.GetDirectoryName(mduFilePath);
            var modelName = Path.GetFileName(Path.GetFileNameWithoutExtension(mduFilePath));

            Assert.NotNull(mduDir);

            var area = new HydroArea();

            var absolutePathPointGroupName = Path.Combine(Directory.GetParent(mduDir).FullName, "myObsPoints");
            area.ObservationPoints.AddRange(
                new[]
                {
                    WaterFlowFMMduFileTestHelper.GetNewGroupableFeature2DPoint(absolutePathPointGroupName, "ObsPoint01"), WaterFlowFMMduFileTestHelper.GetNewGroupableFeature2DPoint(absolutePathPointGroupName, "ObsPoint02")
                }
            );
            
            var modelDefinition = new WaterFlowFMModelDefinition(mduDir, modelName);
            var allFixedWeirsAndCorrespondingProperties = new List<ModelFeatureCoordinateData<FixedWeir>>();
            TestHelper.AssertAtLeastOneLogMessagesContains(() => mduFile.Write(mduFilePath, modelDefinition, area, null, null,null, null, null, null, allFixedWeirsAndCorrespondingProperties), ", because the group name is invalid. Remove any occurences of");
            Assert.IsFalse(File.Exists(absolutePathPointGroupName));
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
                area.FixedWeirs.Add(new FixedWeir
                {
                    GroupName = fixedWeirGroupName,
                    Geometry = new LineString(new[] { new Coordinate(0, 0), new Coordinate(0, 100) })
                });
                area.ThinDams.Add(new ThinDam2D
                {
                    GroupName = thinDamGroupName,
                    Geometry = new LineString(new[] { new Coordinate(0, 0), new Coordinate(0, 100) })
                });

                var modelDefinition = new WaterFlowFMModelDefinition(mduDir, modelName);
                var allFixedWeirsAndCorrespondingProperties = new List<ModelFeatureCoordinateData<FixedWeir>>();
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
                area.FixedWeirs.Add(new FixedWeir
                {
                    GroupName = fixedWeirGroupName,
                    Geometry = new LineString(new[] { new Coordinate(0, 0), new Coordinate(0, 100) })
                });
                area.ThinDams.Add(new ThinDam2D
                {
                    GroupName = thinDamGroupName,
                    Geometry = new LineString(new[] { new Coordinate(0, 0), new Coordinate(0, 100) })
                });

                var modelDefinition = new WaterFlowFMModelDefinition(mduDir, modelName);
                var allFixedWeirsAndCorrespondingProperties = new List<ModelFeatureCoordinateData<FixedWeir>>();

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

            area.ObservationPoints.Add(WaterFlowFMMduFileTestHelper.GetNewGroupableFeature2DPoint("MyObsPoints.xyn", "ObsPoint01"));
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

            var modelDefinition = new WaterFlowFMModelDefinition(mduDir, modelName);
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
        [Category("Quarantine")]
        public void GivenStructuresFileWithReferenceToNonExsistentFile_WhenReadingMdu_ThenStructuresFileIsIgnoredAndDeltaShellGivesWarning()
        {
            // Preparations
            const string baseFolderPath = @"HydroAreaCollection\MduFileProjects";
            var testDir = TestHelper.GetTestDataDirectoryPathForAssembly(GetType().Assembly);

            var originalFolderPath = Path.Combine(testDir, baseFolderPath);
            var relativePath = @"StructuresFileWithoutReferences\FlowFM\FlowFM.mdu";

            var testWorkingFolder = TestHelper.CreateLocalCopy(originalFolderPath);
            var mduPath = Path.Combine(testWorkingFolder, relativePath);

            var area = new HydroArea();
            var network = new HydroNetwork();
            var modelDefinition = new WaterFlowFMModelDefinition(mduPath, Path.GetFileNameWithoutExtension(mduPath));
            var mduFile = new MduFile();
            var allFixedWeirsAndCorrespondingProperties = new List<ModelFeatureCoordinateData<FixedWeir>>();
            var convertedFileObjectsForFMModel = new ConvertedFileObjectsForFMModel
            {
                ModelDefinition = modelDefinition,
                HydroArea = area,
                HydroNetwork = network,
                AllFixedWeirsAndCorrespondingProperties = allFixedWeirsAndCorrespondingProperties
            };
            TestHelper.AssertAtLeastOneLogMessagesContains(() => mduFile.Read(mduPath, convertedFileObjectsForFMModel), "' is referenced in structures file '");
            Assert.IsFalse(File.Exists(Path.Combine(testWorkingFolder, "FlowFM_structures.ini")));
            Assert.That(modelDefinition.GetModelProperty(KnownProperties.StructuresFile).GetValueAsString(), Is.EqualTo(""));
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
            var convertedFileObjectsForFMModel = new ConvertedFileObjectsForFMModel
            {
                ModelDefinition = new WaterFlowFMModelDefinition(mduPath, Path.GetFileNameWithoutExtension(mduPath)),
                HydroArea = new HydroArea(),
                Grid = new UnstructuredGrid(),
                HydroNetwork = new HydroNetwork(),
                AllFixedWeirsAndCorrespondingProperties = new List<ModelFeatureCoordinateData<FixedWeir>>()
            };
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

            var area = new HydroArea();
            var network = new HydroNetwork();
            var modelDefinition = new WaterFlowFMModelDefinition(mduPath, Path.GetFileNameWithoutExtension(mduPath));
            var allFixedWeirsAndCorrespondingProperties = new List<ModelFeatureCoordinateData<FixedWeir>>();
            var convertedFileObjectsForFMModel = new ConvertedFileObjectsForFMModel
            {
                ModelDefinition = modelDefinition,
                HydroArea = area,
                HydroNetwork = network,
                AllFixedWeirsAndCorrespondingProperties = allFixedWeirsAndCorrespondingProperties
            };
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

                var modelDefinition = new WaterFlowFMModelDefinition(mduDir, modelName);
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
                using (var newModel = new WaterFlowFMModel(mduFilePath) { Area = area })
                {
                    var newModelDefinition = new WaterFlowFMModelDefinition(mduDir, modelName);
                    var convertedFileObjectsForFMModel = new ConvertedFileObjectsForFMModel
                    {
                        ModelDefinition = newModelDefinition,
                        HydroArea = area,
                        AllFixedWeirsAndCorrespondingProperties = allFixedWeirsAndCorrespondingProperties
                    };
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
            CompareHydroAreaFeatureCollections("Gates", originalArea.Gates,
                originalArea.Gates.Select(op => op.Name), savedArea.Gates,
                savedArea.Gates.Select(op => op.Name));
            CompareHydroAreaFeatureCollections("Pumps", originalArea.Pumps,
                originalArea.Pumps.Select(op => op.Name), savedArea.Pumps,
                savedArea.Pumps.Select(op => op.Name));
            CompareHydroAreaFeatureCollections("Weirs", originalArea.Weirs,
                originalArea.Weirs.Select(op => op.Name), savedArea.Weirs,
                savedArea.Weirs.Select(op => op.Name));
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

        private void CompareHydroAreaFeatureCollections(string featureName, IEnumerable<IGroupableFeature> expectedFeature, IEnumerable<string> expectedAreaFeatureNames, IEnumerable<IGroupableFeature> savedFeature, IEnumerable<string> savedAreaFeatureNames)
        {
            var expectedList = expectedFeature.ToList();
            var savedList = savedFeature.ToList();
            Assert.AreEqual(expectedList.Count, savedList.Count, 
                "{0} Saved features differ from the original read ones.", featureName);

            var expectedGroups = expectedList.GroupBy(g => g.GroupName).ToList();
            var expectedGroupNames = expectedGroups.Select(g => g.Key).ToList();
            var savedGroups = savedList.GroupBy(g => g.GroupName).ToList();
            var savedGroupNames = savedGroups.Select(g => g.Key).ToList();
            Assert.AreEqual(expectedGroupNames.Count, savedGroupNames.Count,
                "{0} Group names differ from the original read ones. Original: {1}, Saved {2}", featureName, expectedGroupNames, savedGroupNames);

            Assert.IsTrue(Enumerable.SequenceEqual(
                expectedAreaFeatureNames.OrderBy(fElement => fElement),
                savedAreaFeatureNames.OrderBy(sElement => sElement)));
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
            WaterFlowFMModelDefinition modelDefinition, string propertyName, string fileCategoryName)
        {
            var propertyDefinition = new WaterFlowFMPropertyDefinition
            {
                MduPropertyName = propertyName,
                FileCategoryName = fileCategoryName,
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

        private static void CheckIfFilesWereCopied(string fromFolder, string toFolder)
        {
            var toFolderFileNames = new DirectoryInfo(toFolder).GetFiles().Select(f => f.Name);
            var fromFolderFileNames = new DirectoryInfo(fromFolder).GetFiles().Select(f => f.Name);

            var missing = fromFolderFileNames.Except(toFolderFileNames).ToArray();
            if (missing.Any())
            {
                Assert.Fail($"Missing files {string.Join(",", missing)}");
            }
        }

        private static string GetExpectedFileText(string mduPropertyName, string fileNameWithExtension)
        {
            return string.Format("{0,-18}= {1,-20}", mduPropertyName, string.Join(" ", fileNameWithExtension)).Trim();
        }

        private static string GetExpectedFileTextWithEmptyValue(string mduPropertyName)
        {
            return string.Format("{0,-18}=\r\n", mduPropertyName);
        }

        #endregion
    }
}