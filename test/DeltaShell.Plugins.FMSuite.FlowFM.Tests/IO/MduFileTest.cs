using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.Structures;
using DelftTools.TestUtils;
using DelftTools.Utils.IO;
using DelftTools.Utils.Reflection;
using DeltaShell.NGHS.IO.Grid;
using DeltaShell.Plugins.FMSuite.Common.FeatureData;
using DeltaShell.Plugins.FMSuite.FlowFM.FeatureData;
using DeltaShell.Plugins.FMSuite.FlowFM.IO;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using DeltaShell.Plugins.FMSuite.FlowFM.Properties;
using GeoAPI.Geometries;
using log4net.Core;
using NetTopologySuite.Extensions.Features;
using NetTopologySuite.Extensions.Geometries;
using NetTopologySuite.Geometries;
using NUnit.Framework;
using SharpMap;
using SharpMap.Extensions.CoordinateSystems;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.IO
{
    [TestFixture]
    public class MduFileTest
    {
        [SetUp]
        public void Setup()
        {
            Map.CoordinateSystemFactory = new OgrCoordinateSystemFactory();
        }
        private string mduFilePath;
        private string mduDir;
        private string modelName;
        private string saveDirectory;
        private string savePath;
        private string newMduDir;

        private static void CheckAttributeCollection(DictionaryFeatureAttributeCollection attributes,  string columnName, List<double> valueList)
        {
            Assert.IsNotNull(valueList);
            object setValues;
            Assert.IsTrue(attributes.TryGetValue(columnName, out setValues));
            var geometryPointsSyncedList = (setValues as GeometryPointsSyncedList<double>);

            Assert.IsNotNull(geometryPointsSyncedList);

            var idx = 0;
            foreach (var point in geometryPointsSyncedList)
            {
                Assert.AreEqual(point, valueList[idx]);
                idx++;
            }
        }

        [Test]
        public void WriteExternalForcingsFiles()
        {

            TestHelper.PerformActionInTemporaryDirectory(tempDir =>
            {
                var testFile = Path.Combine(tempDir,"ModelWithMeteo.mdu");
                var mduFile = new MduFile();
                var hydroArea = new HydroArea();
                var modelDefinition = new WaterFlowFMModelDefinition();
                var meteoPrecipitationSeries = FmMeteoField.CreateMeteoPrecipitationSeries(FmMeteoLocationType.Global);
                var start = new DateTime(1981, 8, 31, 12, 30, 0);
                var times = new[]
                    {start, start.AddDays(1), start.AddDays(2), start.AddDays(3), start.AddDays(4), start.AddDays(5)};
                var rainFallValues = new[] {5, 5.5, 7.5, 8.3, 11.2, 20.8};

                meteoPrecipitationSeries.Data.Arguments[0].SetValues(times);
                meteoPrecipitationSeries.Data.Components[0].SetValues(rainFallValues);

                var propertyDefinition = new WaterFlowFMPropertyDefinition
                {
                    MduPropertyName = KnownProperties.BndExtForceFile,
                    DataType = typeof(IList<string>),
                    IsMultipleFile = true,
                    FileCategoryName = "TestCategory"
                };
                modelDefinition.AddProperty(new WaterFlowFMProperty(propertyDefinition, string.Empty));
                modelDefinition.FmMeteoFields.Add(meteoPrecipitationSeries);
                modelDefinition.FmMeteoFields.Add(meteoPrecipitationSeries);
                modelDefinition.ModelName = "ModelWithMeteo";
                mduFile.Write(testFile, modelDefinition, hydroArea, null, null,null, null, null, null,null, false, true, false);

                Assert.IsTrue(File.Exists(testFile));
            });

        }

        [Test]
        public void WriteMorphologyAndSedimentFiles()
        {
            var testFile = "ModelWithMorphology.mdu";
            var mduFile = new MduFile();
            var hydroArea = new HydroArea();
            var model = new WaterFlowFMModel();
            var sedimentData = model as ISedimentModelData;
            var modelDefinition = model.ModelDefinition;
            modelDefinition.UseMorphologySediment = true;

            mduFile.Write(testFile, modelDefinition, hydroArea, null, null, null, null, null, null, null, false,false, true, false, sedimentData);

            Assert.IsTrue(File.Exists(testFile));
            var lines = File.ReadLines(testFile);
            Assert.IsTrue(lines.Any(l => l.Contains("ModelWithMorphology.mor")));
            Assert.IsTrue(lines.Any(l => l.Contains("ModelWithMorphology.sed")));
        }

        [Category(TestCategory.DataAccess)]
        [Category(TestCategory.Integration)]
        [Test]
        public void Test_MduFile_Read_Loads_BridgePillars()
        {
            var testPath = TestHelper.GetTestFilePath(@"ImportMDUFile\bridge-1.mdu");
            testPath = TestHelper.CreateLocalCopy(testPath);
            Assert.IsNotNull(testPath);
            Assert.IsTrue(File.Exists(testPath));

            var mduFile = new MduFile();
            var area = new HydroArea();
            var network = new HydroNetwork();

            mduFile.Read(testPath,new WaterFlowFMModelDefinition(), area, network, null, null, null, null);
            Assert.IsTrue(area.BridgePillars.Any());

            var pillar = area.BridgePillars.First();
            Assert.IsNotNull(pillar);


            var expectedName = "BridgePillar01";
            var expectedDiameters = new List<double>(){-599,-599,-999,-999};
            var expectedCoeff = new List<double>(){-999,-999,-499,-499};
            Assert.AreEqual(expectedName, pillar.Name);

            //Check if now they are present.
            Assert.AreEqual(pillar.Attributes.Count, 2);
            var attributes = pillar.Attributes as DictionaryFeatureAttributeCollection;
            Assert.IsNotNull(attributes);

            CheckAttributeCollection(attributes, "Column3", expectedDiameters);
            CheckAttributeCollection(attributes, "Column4", expectedCoeff);
        }

        [Test]
        public void Test_MduFile_Write_Writes_BridgePillars_Entry()
        {
            var testFile = "mduBridgePillars.mdu";
            var mduFile = new MduFile();
            var modelDefinition = new WaterFlowFMModelDefinition();
            var hydroArea = new HydroArea();

            try
            {
                mduFile.Write(testFile, modelDefinition, hydroArea, null,null, null, null, null, null, null);
            }
            catch (Exception e )
            {
                Assert.Fail($"Test crashed. {e.Message}");
            }
            
            Assert.IsTrue(File.Exists(testFile));
            var lines = File.ReadLines(testFile);
            Assert.IsTrue( lines.Any( l => l.Contains("PillarFile")));
        }

        [Test]
        public void Test_MduFile_Write_WithBridgePillars_Writes_BridgePillars_Entry_AndFile()
        {
            var tempFileName = Path.GetTempFileName();
            var testFile = string.Concat(tempFileName, ".mdu");

            var mduFile = new MduFile();
            var modelDefinition = new WaterFlowFMModelDefinition();
            var hydroArea = new HydroArea();
            var pillar = new BridgePillar()
            {
                Name = "BridgePillar2Test",
                Geometry =
                    new LineString(new[]
                    {
                        new Coordinate(20.0, 60.0, 0),
                        new Coordinate(140.0, 8.0, 1.0),
                        new Coordinate(180.0, 4.0, 2.0),
                        new Coordinate(260.0, 0.0, 3.0)
                    }),
            };
            hydroArea.BridgePillars.Add(pillar);
            
            try
            {
                mduFile.Write(testFile, modelDefinition, hydroArea, null, null, null, null,null, null, null);
            }
            catch (Exception e)
            {
                Assert.Fail($"Test crashed. {e.Message}");
            }

            Assert.IsTrue(File.Exists(testFile));
            var lines = File.ReadLines(testFile);
            var expectedLine = $"PillarFile        = {Path.GetFileName(tempFileName)}.pliz";
            Assert.IsTrue(lines.Any(l => l.Contains(expectedLine)));

            //The contents of th efile are checked at the PliZ Exporter and WaterFlowFM export level.
            Assert.IsTrue(File.Exists(tempFileName.Replace(".mdu", ".pliz")));
        }

        [Test]
        public void MduFile_SetBridgePillarAttributes_Updates_BridgePillar_ModelData_WithGiven_Columns()
        {
            var bp = new BridgePillar()
            {
                Geometry =
                    new LineString(new[]
                    {
                        new Coordinate(0.0, 160.0, 0),
                        new Coordinate(40.0, 80.0, 10.0),
                        new Coordinate(80.0, 40.0, 20.0),
                        new Coordinate(160.0, 0.0, 30.0)
                    }),
            };
            var modelFeatureCoordinateDatas = new List<ModelFeatureCoordinateData<BridgePillar>>();
            
            //Create values for the DataColumns.
            var modelFeatureCoordinateData = new ModelFeatureCoordinateData<BridgePillar>();
            modelFeatureCoordinateData.UpdateDataColumns();
            modelFeatureCoordinateData.Feature = bp;
            modelFeatureCoordinateData.DataColumns[0].ValueList = new List<double> { 1.0, 2.5, 5.0, 10.0 };
            modelFeatureCoordinateData.DataColumns[1].ValueList = new List<double> { 10.0, 5.0, 2.5, 1.0 };

            modelFeatureCoordinateDatas.Add(modelFeatureCoordinateData);

            MduFile.SetBridgePillarAttributes(new List<BridgePillar> {bp}, modelFeatureCoordinateDatas);

            Assert.AreEqual(bp.Attributes.Count, 2);
            var attributes = bp.Attributes as DictionaryFeatureAttributeCollection;
            Assert.IsNotNull(attributes);

            CheckAttributeCollection(attributes, "Column3", modelFeatureCoordinateData.DataColumns[0].ValueList as List<double>);
            CheckAttributeCollection(attributes, "Column4", modelFeatureCoordinateData.DataColumns[1].ValueList as List<double>);
        }

        [Test]
        public void MduFile_CleanBridgePillarAttributes_RemovesAll_AttributesFromFeature()
        {
            var dictionaryFeatureAttributeCollection = new DictionaryFeatureAttributeCollection();
            dictionaryFeatureAttributeCollection.Add("testAttr", 23);
            var bp = new BridgePillar{Attributes = dictionaryFeatureAttributeCollection};

            Assert.IsTrue(bp.Attributes.Any());
            MduFile.CleanBridgePillarAttributes(new List<BridgePillar>{bp});
            Assert.IsFalse(bp.Attributes.Any());
        }

        [Test]
        public void MduFile_CleanBridgePillarAttributes_NullArgument_DoesNot_Crash()
        {
            try
            {
                MduFile.CleanBridgePillarAttributes(null);
            }
            catch (Exception)
            {
                Assert.Fail("Should not crash.");
            }
            Assert.Pass("Test did not crash.");
        }

        [Test]
        public void MduFile_SetBridgePillarDataModel_Arguments_Null_DoesNotCrash()
        {
            try
            {
                MduFile.SetBridgePillarDataModel(null, null, null);
            }
            catch (Exception)
            {
                Assert.Fail("Should not crash.");
            }
            Assert.Pass("Test did not crash.");
        }

        [Test]
        public void MduFile_SetBridgePillarDataModel_Attributes_AreSetTo_BridgePillar()
        {
            var bp = new BridgePillar()
            {
                Geometry =
                    new LineString(new[]
                    {
                        new Coordinate(0.0, 160.0, 0),
                        new Coordinate(40.0, 80.0, 10.0),
                        new Coordinate(80.0, 40.0, 20.0),
                        new Coordinate(160.0, 0.0, 30.0)
                    }),
            };

            var listofDataModel = new List<ModelFeatureCoordinateData<BridgePillar>>();
            #region set Attribute values to bridge pillar
            /*We were not able to set the Attributes property for Bridge pillar, so we use the following code to di for us*/
            //Create values for the DataColumns
            var modelFeatureCoordinateData = new ModelFeatureCoordinateData<BridgePillar>();
            modelFeatureCoordinateData.UpdateDataColumns();
            modelFeatureCoordinateData.Feature = bp;
            modelFeatureCoordinateData.DataColumns[0].ValueList = new List<double> { 1.0, 2.5, 5.0, 10.0 };
            modelFeatureCoordinateData.DataColumns[1].ValueList = new List<double> { 10.0, 5.0, 2.5, 1.0 };

            listofDataModel.Add(modelFeatureCoordinateData);

            MduFile.SetBridgePillarAttributes(new List<BridgePillar> { bp }, listofDataModel);

            Assert.IsNotNull(bp.Attributes);
            Assert.AreEqual(2, bp.Attributes.Count);
            #endregion

            listofDataModel.Clear();
            var bpDataModel = new ModelFeatureCoordinateData<BridgePillar>(){Feature = bp};
            bpDataModel.UpdateDataColumns();

            listofDataModel.Add(bpDataModel);

            Assert.IsNotNull(bpDataModel.DataColumns);
            Assert.AreEqual(2, bpDataModel.DataColumns.Count);

            var diameterList = new List<double> { 1.0, 2.5, 5.0, 10.0 };
            var coeffList = new List<double> { 10.0, 5.0, 2.5, 1.0 };

            Assert.AreNotEqual(diameterList, bpDataModel.DataColumns[0].ValueList as List<double> );
            Assert.AreNotEqual(coeffList, bpDataModel.DataColumns[1].ValueList as List<double>);

            //Run method
            MduFile.SetBridgePillarDataModel(listofDataModel, bpDataModel, bp);

            Assert.IsNotNull(bpDataModel.DataColumns);
            Assert.AreEqual(2, bpDataModel.DataColumns.Count);

            //Check if now they are present.
            Assert.AreEqual(diameterList, bpDataModel.DataColumns[0].ValueList as List<double>);
            Assert.AreEqual(coeffList, bpDataModel.DataColumns[1].ValueList as List<double>);
        }

        [Test]
        public void MduFile_SetBridgePillarDataModel_WithTooManyColumns_DoesNotCrash()
        {
            var testPath = TestHelper.GetTestFilePath(@"ImportMDUFile\IncorrectPlizFile\bridge-1.mdu");
            testPath = TestHelper.CreateLocalCopy(testPath);
            Assert.IsNotNull(testPath);
            Assert.IsTrue(File.Exists(testPath));

            var mduFile = new MduFile();
            var area = new HydroArea();
            var network = new HydroNetwork();

            Assert.DoesNotThrow(() => mduFile.Read(testPath, new WaterFlowFMModelDefinition(), area, network, null, null,null,null, allBridgePillarsAndCorrespondingProperties: new List<ModelFeatureCoordinateData<BridgePillar>>()), "It Crashed");
            
        }

        [Test]
        public void Test_MduFile_Read_BridgePillar_WithTooManyColumns_IsImported_AndMessageIsGiven()
        {
            var testPath = TestHelper.GetTestFilePath(@"ImportMDUFile\IncorrectPlizFile\bridge-1.mdu");
            testPath = TestHelper.CreateLocalCopy(testPath);
            Assert.IsNotNull(testPath);
            Assert.IsTrue(File.Exists(testPath));

            var mduFile = new MduFile();
            var area = new HydroArea();
            var network = new HydroNetwork();

            var expectedMsg = string.Format(
                Resources.MduFile_Read_Based_on_the_Bridge_Pillar_file__0___there_are_too_many_column_s__defined_for__1___The_last__2__column_s__have_been_ignored, 
                "bridge-1.pliz", "BridgePillar01", 1);
            TestHelper.AssertAtLeastOneLogMessagesContains(
                () => mduFile.Read(testPath, new WaterFlowFMModelDefinition(), area, network, null, null,null,null, allBridgePillarsAndCorrespondingProperties: new List<ModelFeatureCoordinateData<BridgePillar>>()),
                expectedMsg
                );          
        }

        [Test]
        public void Test_MduFile_Read_BridgePillar_WithTooFewColumns_IsImported_AndMessageIsGiven()
        {
            var testPath = TestHelper.GetTestFilePath(@"ImportMDUFile\IncorrectPlizFile\bridge-2.mdu");
            testPath = TestHelper.CreateLocalCopy(testPath);
            Assert.IsNotNull(testPath);
            Assert.IsTrue(File.Exists(testPath));

            var mduFile = new MduFile();
            var area = new HydroArea();
            var network = new HydroNetwork();

            var expectedMsg = string.Format(
                Resources.MduFile_Read_Based_on_the_Bridge_Pillar_file__0___there_are_not_enough_column_s__defined_for__1___The_last__2__column_s__have_been_generated_using_default_values,
                "bridge-2.pliz", "BridgePillar02", 1);

            TestHelper.AssertAtLeastOneLogMessagesContains(
                () => mduFile.Read(testPath, new WaterFlowFMModelDefinition(), area, network, null, null,null,null, allBridgePillarsAndCorrespondingProperties: new List<ModelFeatureCoordinateData<BridgePillar>>()),
                expectedMsg
            );
        }

        [Category(TestCategory.DataAccess)]
        [Category(TestCategory.Integration)]
        [TestCase(@"TestModelWithNcInSubFolder\trynet.mdu", "Sub\\gridtry.nc")]
        [TestCase(@"TestModelWithoutNcInSubFolder\trynet.mdu", "gridtry.nc")]
        [TestCase(@"TestModelWithNcInSubFolderAndDefaultNames\trynet.mdu", "Sub\\trynet_net.nc")]
        public void GivenModelForImporting_WhenNcFileIsInSubFolder_ThenNcFileShouldBeAgainInSubFolder(string relativeMduFilePath, string relativeNcFilePath)
        {
            mduFilePath = TestHelper.GetTestFilePath(relativeMduFilePath);
            mduFilePath = TestHelper.CreateLocalCopy(mduFilePath);
            mduDir = Path.GetDirectoryName(mduFilePath);
            Assert.NotNull(mduDir);
            modelName = Path.GetFileName(mduFilePath);

            saveDirectory = Path.Combine(mduDir, "MduFileReadsAndWritesTest");
            Directory.CreateDirectory(saveDirectory);
            savePath = Path.Combine(saveDirectory, "trynet.mdu");
            newMduDir = Path.GetDirectoryName(savePath);
            Assert.NotNull(newMduDir);
            try
            {
                var mduFile = new MduFile();

                var originalArea = new HydroArea();
                var network = new HydroNetwork();
                var originalMD = new WaterFlowFMModelDefinition(mduDir, modelName);
                var allFixedWeirsAndCorrespondingProperties = new List<ModelFeatureCoordinateData<FixedWeir>>();
                mduFile.Read(mduFilePath, originalMD, originalArea, network, null, null, null, allFixedWeirsAndCorrespondingProperties);
                mduFile.Write(savePath, originalMD, originalArea, null,null,null, null,null,null, allFixedWeirsAndCorrespondingProperties, switchTo: false);
                
                var netFileLocationShouldBe = Path.Combine(newMduDir, relativeNcFilePath);

                Assert.IsTrue(File.Exists(netFileLocationShouldBe));
            }
            finally
            {
                FileUtils.DeleteIfExists(mduDir);
            }
        }

        [Category(TestCategory.DataAccess)]
        [Category(TestCategory.Integration)]
        [TestCase(@"cs_after_save\before_save_AmersfoortRDNew_net.nc", 28992, "Amersfoort / RD New")]
        [TestCase(@"cs_after_save\before_save_AmersfoortRDOld_net.nc", 28991, "Amersfoort / RD Old")]
        [TestCase(@"cs_after_save\before_save_UTMzone30N_net.nc", 32630, "WGS 84 / UTM zone 30N")]
        public void SetCoordinateSystemNameNetfileWithModelCoordinateSystemNameTest(string netFile, int espgModel, string expectedCoordinateSystemName)
        {
            var workingDirectory = FileUtils.CreateTempDirectory();
            var coordinateSystem = new OgrCoordinateSystemFactory().CreateFromEPSG(espgModel);

            var netFilePath = TestHelper.GetTestFilePath(netFile);
            var netFileInfo = new FileInfo(netFilePath);
            Assert.IsTrue(netFileInfo.Exists);

            var workingNetFilePath = Path.Combine(workingDirectory, netFileInfo.Name);
            var workingNetFileInfo = new FileInfo(workingNetFilePath);
            FileUtils.CopyFile(netFileInfo.FullName, workingNetFilePath);
            Assert.IsTrue(workingNetFileInfo.Exists);

            var mduFile = new MduFile();
            //var fileProjectedName = TypeUtils.CallPrivateMethod<string>(mduFile, "GetProjectedCoordinateSystemNameFromNetFile", workingNetFilePath);
            var fileProjectedName = TypeUtils.CallPrivateStaticMethod(typeof(ICoordinateSystemExtensions), "GetProjectedCoordinateSystemNameFromNetFile", workingNetFilePath) as string;
            Assert.That(fileProjectedName, Is.EqualTo("Unknown projected"));

            UGridFileHelper.WriteCoordinateSystem(workingNetFilePath, coordinateSystem);

            var editedFileProjectedName = TypeUtils.CallPrivateStaticMethod(typeof(ICoordinateSystemExtensions), "GetProjectedCoordinateSystemNameFromNetFile", workingNetFilePath) as string;
            Assert.IsTrue(editedFileProjectedName.Equals(expectedCoordinateSystemName));

            FileUtils.DeleteIfExists(workingDirectory);
        }

        /*[TestCase(false, @"update_CS_netfile\amersfoortRDNew_net.nc", 28991, true)]
        [TestCase(false, @"update_CS_netfile\unknown_projected_net.nc", 28991, true)]
*/
        /*[TestCase(false, @"update_CS_netfile\amersfoortRDNew_net.nc", 28992, true)]
        [TestCase(false, @"update_CS_netfile\unknown_projected_net.nc", 28992, true)]
        [TestCase(false, @"update_CS_netfile\wgs84_net.nc", 4326, true)]
*/
        [Category(TestCategory.DataAccess)]
        [Category(TestCategory.Integration)]
        [TestCase(true, @"update_CS_netfile\amersfoortRDNew_net.nc", 28992, true)]
        [TestCase(true, @"update_CS_netfile\unknown_projected_net.nc", 28992, false)]
        [TestCase(true, @"update_CS_netfile\wgs84_net.nc", 4326 , true)]
        [TestCase(true, @"update_CS_netfile\amersfoortRDNew_net.nc", 28991, false)]
        [TestCase(true, @"update_CS_netfile\unknown_projected_net.nc", 28991, false)]
        [TestCase(true, @"update_CS_netfile\wgs84_net.nc", 4326, true)]
        [TestCase(true, @"update_CS_netfile\wgs84_net.nc", 28992, false)]
        [TestCase(true, @"update_CS_netfile\amersfoortRDNew_net.nc", 4326, false)]

        public void ShouldUpdateNetfileCoordinateSystemTest(bool hasCoordinateSystem, string targetFile, int epsgModelDefinition, bool expected)
        {
            var netFilePath = TestHelper.GetTestFilePath(targetFile);
            var netFileInfo = new FileInfo(netFilePath);
            Assert.IsTrue(netFileInfo.Exists);

            var modelDefinition = new WaterFlowFMModelDefinition();
            var mduFile = new MduFile();
            if (hasCoordinateSystem == true)
                modelDefinition.CoordinateSystem = new OgrCoordinateSystemFactory().CreateFromEPSG(epsgModelDefinition);
      
            //var result = TypeUtils.CallPrivateMethod<bool>(mduFile, "IsNetfileCoordinateSystemUpToDate", modelDefinition, netFilePath);
            var result = modelDefinition.CoordinateSystem.IsNetfileCoordinateSystemUpToDate(netFilePath);

            Assert.That(result, Is.EqualTo(expected));
        }

        [Category(TestCategory.DataAccess)]
        [Category(TestCategory.Integration)]
        [Test]
        public void GivenAnMduToReadWithFixedWeirs_WhenTheSchemeNumbersRequiresMoreColumnsThanGivenInPlizFile_ThenAllMissingPropertiesShouldBeCreatedUsingTheDefaultValues()
        {
            mduFilePath = TestHelper.GetTestFilePath(@"HydroAreaCollection\FlowFMFixedWeirs\FlowFM2.mdu");
            mduFilePath = TestHelper.CreateLocalCopy(mduFilePath);
            mduDir = Path.GetDirectoryName(mduFilePath);
            Assert.NotNull(mduDir);
            modelName = Path.GetFileName(mduFilePath);

            saveDirectory = Path.Combine(mduDir, "MduFileReadsAndWritesTest");
            Directory.CreateDirectory(saveDirectory);
            savePath = Path.Combine(saveDirectory, "FlowFM2.mdu");
            newMduDir = Path.GetDirectoryName(savePath);
            Assert.NotNull(newMduDir);
            try
            {
                var mduFile = new MduFile();

                var originalArea = new HydroArea();
                var network = new HydroNetwork();
                var originalMD = new WaterFlowFMModelDefinition(mduDir, modelName);
                var allFixedWeirsAndCorrespondingProperties = new List<ModelFeatureCoordinateData<FixedWeir>>();
                mduFile.Read(mduFilePath, originalMD, originalArea, network, null,null,null, allFixedWeirsAndCorrespondingProperties);

                Assert.AreEqual(7, allFixedWeirsAndCorrespondingProperties[0].DataColumns.Count);

                //CrestLevel
                Assert.AreEqual(0, allFixedWeirsAndCorrespondingProperties[0].DataColumns[0].ValueList[0]);
                Assert.AreEqual(0, allFixedWeirsAndCorrespondingProperties[0].DataColumns[0].ValueList[1]);

                //Ground Height Left
                Assert.AreEqual(0, allFixedWeirsAndCorrespondingProperties[0].DataColumns[1].ValueList[0]);
                Assert.AreEqual(0, allFixedWeirsAndCorrespondingProperties[0].DataColumns[1].ValueList[1]);

                //Ground Height Right
                Assert.AreEqual(0, allFixedWeirsAndCorrespondingProperties[0].DataColumns[2].ValueList[0]);
                Assert.AreEqual(0, allFixedWeirsAndCorrespondingProperties[0].DataColumns[2].ValueList[1]);

                //Crest Width
                Assert.AreEqual(3, allFixedWeirsAndCorrespondingProperties[0].DataColumns[3].ValueList[0]);
                Assert.AreEqual(3, allFixedWeirsAndCorrespondingProperties[0].DataColumns[3].ValueList[1]);

                //Slope Left
                Assert.AreEqual(4, allFixedWeirsAndCorrespondingProperties[0].DataColumns[4].ValueList[0]);
                Assert.AreEqual(4, allFixedWeirsAndCorrespondingProperties[0].DataColumns[4].ValueList[1]);

                //Slope Right
                Assert.AreEqual(4, allFixedWeirsAndCorrespondingProperties[0].DataColumns[5].ValueList[0]);
                Assert.AreEqual(4, allFixedWeirsAndCorrespondingProperties[0].DataColumns[5].ValueList[1]);

                //Roughness Code
                Assert.AreEqual(0, allFixedWeirsAndCorrespondingProperties[0].DataColumns[6].ValueList[0]);
                Assert.AreEqual(0, allFixedWeirsAndCorrespondingProperties[0].DataColumns[6].ValueList[1]);

                mduFile.Write(savePath, originalMD, originalArea, null, null,null, null, null, null, allFixedWeirsAndCorrespondingProperties,switchTo: false);

                var generatedResultsContent = File.ReadAllLines(@"TestOutput\FlowFMFixedWeirs\MduFileReadsAndWritesTest\TwoFixedWeirs_fxw2_fxw.pliz");

                var expectedResultsContent = 
                    new string[]{
                        "Weir01",
                        "    2    9",
                        "5.400000000000000E+000  4.600000000000000E+000  0.000000000000000E+000  0.000000000000000E+000  0.000000000000000E+000  3.000000000000000E+000  4.000000000000000E+000  4.000000000000000E+000  0.000000000000000E+000",
                        "1.200000000000000E+000  1.000000000000000E+001  0.000000000000000E+000  0.000000000000000E+000  0.000000000000000E+000  3.000000000000000E+000  4.000000000000000E+000  4.000000000000000E+000  0.000000000000000E+000",
                        "Weir02",
                        "    2    9",
                        "2.000000000000000E+000  7.000000000000000E-001  0.000000000000000E+000  0.000000000000000E+000  0.000000000000000E+000  3.000000000000000E+000  4.000000000000000E+000  4.000000000000000E+000  0.000000000000000E+000",
                        "3.900000000000000E+000  3.900000000000000E+000  0.000000000000000E+000  0.000000000000000E+000  0.000000000000000E+000  3.000000000000000E+000  4.000000000000000E+000  4.000000000000000E+000  0.000000000000000E+000"
                        };

                for (int i =0; i < 8; i++)
                {
                    Assert.AreEqual(expectedResultsContent[i], generatedResultsContent[i],
                        "Line " + (i + 1) + " of generated file " + savePath +
                        " differs from expected result");
                }

            }
            finally
            {
                FileUtils.DeleteIfExists(mduDir);
            }
        }

        [Category(TestCategory.DataAccess)]
        [Category(TestCategory.Integration)]
        [Test]
        public void GivenAnMduToReadWithFixedWeirs_WhenTheSchemeNumbersRequiresLessColumnsThanGivenInPlizFile_ThenOnlyTheNeededPropertiesShouldBeCreatedAfterReadingThePlizFile()
            {
                mduFilePath = TestHelper.GetTestFilePath(@"HydroAreaCollection\FlowFMFixedWeirs\FlowFM3.mdu");
                mduFilePath = TestHelper.CreateLocalCopy(mduFilePath);
                mduDir = Path.GetDirectoryName(mduFilePath);
                Assert.NotNull(mduDir);
                modelName = Path.GetFileName(mduFilePath);

            saveDirectory = Path.Combine(mduDir, "MduFileReadsAndWritesTest");
            Directory.CreateDirectory(saveDirectory);
            savePath = Path.Combine(saveDirectory, "FlowFM3.mdu");
            newMduDir = Path.GetDirectoryName(savePath);
            Assert.NotNull(newMduDir);

            try
                {
                    var mduFile = new MduFile();

                    var originalArea = new HydroArea();
                    var network = new HydroNetwork();
                    var originalMD = new WaterFlowFMModelDefinition(mduDir, modelName);
                    var allFixedWeirsAndCorrespondingProperties = new List<ModelFeatureCoordinateData<FixedWeir>>();

                    

                    mduFile.Read(mduFilePath, originalMD, originalArea, network, null, null, null, allFixedWeirsAndCorrespondingProperties);

                    Assert.AreEqual(3, allFixedWeirsAndCorrespondingProperties[0].DataColumns.Count);

                    //CrestLevel
                    Assert.AreEqual(1.2, allFixedWeirsAndCorrespondingProperties[0].DataColumns[0].ValueList[0]);
                    Assert.AreEqual(6.4, allFixedWeirsAndCorrespondingProperties[0].DataColumns[0].ValueList[1]);

                    //Ground Height Left
                    Assert.AreEqual(3.5, allFixedWeirsAndCorrespondingProperties[0].DataColumns[1].ValueList[0]);
                    Assert.AreEqual(3.0, allFixedWeirsAndCorrespondingProperties[0].DataColumns[1].ValueList[1]);

                    //Ground Height Right
                    Assert.AreEqual(3.2, allFixedWeirsAndCorrespondingProperties[0].DataColumns[2].ValueList[0]);
                    Assert.AreEqual(3.3, allFixedWeirsAndCorrespondingProperties[0].DataColumns[2].ValueList[1]);

                    mduFile.Write(savePath, originalMD, originalArea, null, null,null, null, null, null, allFixedWeirsAndCorrespondingProperties, switchTo: false);

                    var generatedResultsContent = File.ReadAllLines(@"TestOutput\FlowFMFixedWeirs\MduFileReadsAndWritesTest\TwoFixedWeirs_fxw.pliz");

                    var expectedResultsContent =
                        new string[]{
                            "Weir01",
                            "    2    5",
                            "5.400000000000000E+000  4.600000000000000E+000  1.200000000000000E+000  3.500000000000000E+000  3.200000000000000E+000",
                            "1.200000000000000E+000  1.000000000000000E+001  6.400000000000000E+000  3.000000000000000E+000  3.300000000000000E+000",
                            "Weir02",
                            "    2    5",
                            "2.000000000000000E+000  7.000000000000000E-001  1.700000000000000E+000  4.500000000000000E+000  4.200000000000000E+000",
                            "3.900000000000000E+000  3.900000000000000E+000  6.100000000000000E+000  4.000000000000000E+000  4.300000000000000E+000"
                        };

                    for (int i = 0; i < 8; i++)
                    {
                        Assert.AreEqual(expectedResultsContent[i], generatedResultsContent[i],
                            "Line " + (i + 1) + " of generated file " + savePath +
                            " differs from expected result");
                    }
            }
                finally
                {
                    FileUtils.DeleteIfExists(mduDir);
                }
            }

        [Category(TestCategory.DataAccess)]
        [Test]
        public void GivenAnMDUWithTheOldNameForEnclosureFile_WhenImportingIt_ThenThisNameShouldBeChangedToGridEnclosureFile()
        {
            mduFilePath = TestHelper.GetTestFilePath(@"harlingen\HarlingenModelWithOldEnclosureFileMduPropertyName\har.mdu");
            mduFilePath = TestHelper.CreateLocalCopy(mduFilePath);
            mduDir = Path.GetDirectoryName(mduFilePath);
            Assert.NotNull(mduDir);
            modelName = Path.GetFileName(mduFilePath);

            saveDirectory = Path.Combine(mduDir, "SaveLocation");
            Directory.CreateDirectory(saveDirectory);
            savePath = Path.Combine(saveDirectory, "har.mdu");
            newMduDir = Path.GetDirectoryName(savePath);
            Assert.NotNull(newMduDir);

            try
            {
                var mduFile = new MduFile();

                var originalArea = new HydroArea();
                var network = new HydroNetwork();
                var originalMD = new WaterFlowFMModelDefinition(mduDir, modelName);
                var allFixedWeirsAndCorrespondingProperties = new List<ModelFeatureCoordinateData<FixedWeir>>();

                mduFile.Read(mduFilePath, originalMD, originalArea, network, null, null, null, allFixedWeirsAndCorrespondingProperties);

                //Check if the enclosure file is in memory under the new mdu property name in the model definition.
                var newModelProperty = originalMD.GetModelProperty(KnownProperties.EnclosureFile);
                Assert.NotNull(newModelProperty);

                //Check that the old mdu property name is not existing anymore in the model definition.
                var oldModelProperty = originalMD.GetModelProperty("enclosurefile");
                Assert.IsNull(oldModelProperty);

                mduFile.Write(savePath, originalMD, originalArea, null, null,null, null,null, null, allFixedWeirsAndCorrespondingProperties);

                var generatedInputContent =
                    File.ReadAllLines(mduFilePath);

                Assert.IsFalse(generatedInputContent.Any(x => x.ToLower().Contains("gridenclosurefile")));
                Assert.IsTrue(generatedInputContent.Any(x => x.ToLower().Contains("enclosurefile")));

                var generatedResultsContent =
                    File.ReadAllLines(savePath);

                Assert.IsTrue(generatedResultsContent.Any(x => x.ToLower().Contains("gridenclosurefile")));
            }
            finally
            {
                FileUtils.DeleteIfExists(mduDir);
            }
        }

        [Category(TestCategory.DataAccess)]
        [Test]
        public void GivenAnMDUWithTheNewNameForEnclosureFile_WhenImportingIt_ThenTheImportShouldBeCorrectForThisMduProperty()
        {
            mduFilePath = TestHelper.GetTestFilePath(@"harlingen\HarlingenModelWithNewEnclosureFileMduPropertyName\har.mdu");
            mduFilePath = TestHelper.CreateLocalCopy(mduFilePath);
            mduDir = Path.GetDirectoryName(mduFilePath);
            Assert.NotNull(mduDir);
            modelName = Path.GetFileName(mduFilePath);

            saveDirectory = Path.Combine(mduDir, "SaveLocation");
            Directory.CreateDirectory(saveDirectory);
            savePath = Path.Combine(saveDirectory, "har.mdu");
            newMduDir = Path.GetDirectoryName(savePath);
            Assert.NotNull(newMduDir);

            try
            {
                var mduFile = new MduFile();

                var originalArea = new HydroArea();
                var network = new HydroNetwork();
                var originalMD = new WaterFlowFMModelDefinition(mduDir, modelName);
                var allFixedWeirsAndCorrespondingProperties = new List<ModelFeatureCoordinateData<FixedWeir>>();

                mduFile.Read(mduFilePath, originalMD, originalArea, network, null, null, null, allFixedWeirsAndCorrespondingProperties);

                //Check if the enclosure file is in memory under the new mdu property name in the model definition.
                var newModelProperty = originalMD.GetModelProperty(KnownProperties.EnclosureFile);
                Assert.NotNull(newModelProperty);

                //Check that the old mdu property name is not existing anymore in the model definition.
                var oldModelProperty = originalMD.GetModelProperty("enclosurefile");
                Assert.IsNull(oldModelProperty);

                mduFile.Write(savePath, originalMD, originalArea, null, null,null, null, null, null, allFixedWeirsAndCorrespondingProperties);

                var generatedInputContent =
                    File.ReadAllLines(mduFilePath);

                Assert.IsTrue(generatedInputContent.Any(x => x.ToLower().Contains("gridenclosurefile")));
                
                var generatedResultsContent =
                    File.ReadAllLines(savePath);

                Assert.IsTrue(generatedResultsContent.Any(x => x.ToLower().Contains("gridenclosurefile")));
            }
            finally
            {
                FileUtils.DeleteIfExists(mduDir);
            }
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void Read_WithRoofArea_ImportsRoofArea()
        {
            using (var temp = new TemporaryDirectory())
            {
                // Setup
                string mduFileContent =
                    "[geometry]                        " + Environment.NewLine +
                    "    RoofsFile     = some_roofs.pol";

                string roofFileContent =
                    "SomeRoof                          " + Environment.NewLine +
                    "    5    2                        " + Environment.NewLine +
                    "        0.0        0.0            " + Environment.NewLine +
                    "        1.0        0.0            " + Environment.NewLine +
                    "        1.0        1.0            " + Environment.NewLine +
                    "        0.0        1.0            " + Environment.NewLine +
                    "        0.0        0.0            ";


                temp.CreateFile("some_roofs.pol", roofFileContent);
                var filePath = temp.CreateFile("some_name.mdu", mduFileContent);
                var mduFile = new MduFile();
                var modelDefinition = new WaterFlowFMModelDefinition();
                var area = new HydroArea();

                // Call
                mduFile.Read(filePath, modelDefinition, area, null, null, null, null, null);

                // Assert
                Assert.That(modelDefinition.GetModelProperty("roofsfile").Value, Is.EqualTo("some_roofs.pol"));

                GroupableFeature2DPolygon roof = area.RoofAreas.Single();
                Assert.That(roof.Name, Is.EqualTo("SomeRoof"));

                IGeometry geometry = roof.Geometry;
                Assert.That(geometry, Is.TypeOf<Polygon>());
                Assert.That(geometry.Coordinates, Has.Length.EqualTo(5));
                Assert.That(geometry.Coordinates[0].X, Is.EqualTo(0.0));
                Assert.That(geometry.Coordinates[0].Y, Is.EqualTo(0.0));
                Assert.That(geometry.Coordinates[1].X, Is.EqualTo(1.0));
                Assert.That(geometry.Coordinates[1].Y, Is.EqualTo(0.0));
                Assert.That(geometry.Coordinates[2].X, Is.EqualTo(1.0));
                Assert.That(geometry.Coordinates[2].Y, Is.EqualTo(1.0));
                Assert.That(geometry.Coordinates[3].X, Is.EqualTo(0.0));
                Assert.That(geometry.Coordinates[3].Y, Is.EqualTo(1.0));
                Assert.That(geometry.Coordinates[4].X, Is.EqualTo(0.0));
                Assert.That(geometry.Coordinates[4].Y, Is.EqualTo(0.0));
            }
        }
        
        [Test]
        [Category(TestCategory.DataAccess)]
        public void Read_RoofAreaFileDoesNotExist_ReportsWarning()
        {
            using (var temp = new TemporaryDirectory())
            {
                // Setup
                string mduFileContent =
                    "[geometry]                        " + Environment.NewLine +
                    "    RoofsFile     = some_roofs.pol";
                
                var filePath = temp.CreateFile("some_name.mdu", mduFileContent);
                var mduFile = new MduFile();
                var modelDefinition = new WaterFlowFMModelDefinition {ModelName = "some_name"};
                var area = new HydroArea();

                // Call
                void Call()=> mduFile.Read(filePath, modelDefinition, area, null, null, null, null, null);
                
                // Assert
                string[] warnings = TestHelper.GetAllRenderedMessages(Call, Level.Warn).ToArray();

                string expWarning = $@"File at location '{Path.Combine(temp.Path, "some_roofs.pol")}' does not exist, but is defined in MDU file at '{Path.Combine(temp.Path, "some_name.mdu")}'" + "\r\n" +
                                    "at property 'RoofAreaFile'. Model information may have been lost while loading model 'some_name'.";
                
                Assert.That(warnings, Does.Contain(expWarning));
                Assert.That(modelDefinition.GetModelProperty("roofsfile").Value, Is.Empty);
                Assert.That(area.RoofAreas, Is.Empty);
            }
        }
    }
}