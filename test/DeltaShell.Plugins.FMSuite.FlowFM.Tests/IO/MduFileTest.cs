using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using DelftTools.Hydro;
using DelftTools.Hydro.Area.Objects;
using DelftTools.Hydro.GroupableFeatures;
using DelftTools.TestUtils;
using DelftTools.Utils.Collections.Generic;
using DelftTools.Utils.IO;
using DelftTools.Utils.Reflection;
using DeltaShell.NGHS.IO.Grid;
using DeltaShell.NGHS.TestUtils;
using DeltaShell.Plugins.FMSuite.FlowFM.Api;
using DeltaShell.Plugins.FMSuite.FlowFM.FeatureData;
using DeltaShell.Plugins.FMSuite.FlowFM.IO;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.BackwardCompatibility;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Files;
using DeltaShell.Plugins.FMSuite.FlowFM.Model;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using DeltaShell.Plugins.FMSuite.FlowFM.Properties;
using DeltaShell.Plugins.FMSuite.FlowFM.Sediment;
using DHYDRO.Common.Extensions;
using GeoAPI.Extensions.CoordinateSystems;
using GeoAPI.Geometries;
using NetTopologySuite.Extensions.Features;
using NetTopologySuite.Extensions.Geometries;
using NetTopologySuite.Geometries;
using NSubstitute;
using NUnit.Framework;
using Rhino.Mocks;
using SharpMap.Extensions.CoordinateSystems;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.IO
{
    [TestFixture]
    public class MduFileTest
    {
        private string mduFilePath;
        private string mduDir;
        private string modelName;
        private string saveDirectory;
        private string savePath;
        private string newMduDir;

        /// <summary>
        /// The test case data for the GivenAMduFileWithMoreColumnsThanNeeded_WhenReadIsCalled_ThenASingleErrorMessageIsLogged.
        /// </summary>
        private static IEnumerable<TestCaseData> WeirWarningMessageTestCaseData
        {
            get
            {
                //                             mduName     |  fixedWeirPlizFileName   | weirScheme | columnDifference | expectedSubMsgFormat
                yield return new TestCaseData("FlowFM3.mdu", "TwoFixedWeirs_fxw.pliz", 6, 5, Resources.MduFile_Read_Based_on_the_Fixed_Weir_Scheme__0___there_are_too_many_column_s__defined_for__1__in_the_imported_fixed_weir_file__The_last__2__column_s__have_been_ignored);
                yield return new TestCaseData("FlowFM2.mdu", "TwoFixedWeirs_fxw2.pliz", 9, 7, Resources.MduFile_Read_Based_on_the_Fixed_Weir_Scheme__0___there_are_not_enough_column_s__defined_for__1__in_the_imported_fixed_weir_file__The_last__2__column_s__have_been_generated_using_default_values);
            }
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void Write_Always_WritesExpectedMetaDataInformation()
        {
            // Setup
            var mduFile = new MduFile();
            var modelDefinition = new WaterFlowFMModelDefinition();
            var config = MockRepository.GenerateStub<IMduFileWriteConfig>();

            using (IFlexibleMeshModelApi api = FlexibleMeshModelApiFactory.CreateNew())
            using (var tempDirectory = new TemporaryDirectory())
            {
                string writeFilePath = Path.Combine(tempDirectory.Path, "FlowFM.mdu");

                // Call
                mduFile.Write(writeFilePath,
                              modelDefinition,
                              null,
                              new List<ModelFeatureCoordinateData<FixedWeir>>(),
                              config);

                // Assert
                string[] lines = File.ReadAllLines(writeFilePath);
                Assembly waterFlowFMAssembly = typeof(WaterFlowFMModel).Assembly;

                string expectedMetaDataString = $"# Deltares, Plugin D-FLOW FM Version {waterFlowFMAssembly.GetName().Version}, " +
                                                $"D-Flow FM Version {api.GetVersionString()}";

                Assert.That(lines[1], Is.EqualTo(expectedMetaDataString));
            }
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void Write_ThenAllPropertiesWithACommentAreWrittenWithAComment()
        {
            // Setup
            var mduFile = new MduFile();
            var modelDefinition = new WaterFlowFMModelDefinition();
            var config = MockRepository.GenerateStub<IMduFileWriteConfig>();

            const string propertyName = "custom_property_name";
            const string comment = "custom_comment";

            WaterFlowFMProperty property = CreateProperty(propertyName, comment);
            modelDefinition.Properties.Add(property);

            string[] lines;
            using (var tempDirectory = new TemporaryDirectory())
            {
                string writeFilePath = Path.Combine(tempDirectory.Path, "FlowFM.mdu");

                // Call
                mduFile.Write(writeFilePath, modelDefinition, null, new List<ModelFeatureCoordinateData<FixedWeir>>(), config);

                lines = File.ReadAllLines(writeFilePath);
            }

            // Assert
            string propertyLine = lines.Single(l => l.Contains(propertyName));

            Assert.That(propertyLine.Contains($"# {comment}"),
                        $"Line '{propertyLine}' does not contain the expected comment '{comment}'.");
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void Read_FromFileWithKnownPropertyWithCustomComment_ThenOriginalCommentWillBeKept()
        {
            // Setup
            var mduFile = new MduFile();
            var modelDefinition = new WaterFlowFMModelDefinition();
            const string propertyName = KnownProperties.Temperature;
            const string originalComment = "original_comment";

            WaterFlowFMProperty property = modelDefinition.GetModelProperty(propertyName);
            property.PropertyDefinition.Description = originalComment;

            using (var tempDirectory = new TemporaryDirectory())
            {
                string filePath = Path.Combine(tempDirectory.Path, "FlowFM.mdu");
                File.WriteAllLines(filePath, new[]
                {
                    "[physics]",
                    $"{propertyName} = 1 # custom_comment"
                });

                // Call
                mduFile.Read(filePath, modelDefinition, new HydroArea(), null);
            }

            // Assert
            string comment = property.PropertyDefinition.Description;
            Assert.That(comment, Is.EqualTo(originalComment),
                        "Comments should not be altered after reading the mdu file for known properties.");
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void Read_FromFileWithUnknownPropertyWithAComment_ThenCommentFromFileIsSetProperty()
        {
            // Setup
            var mduFile = new MduFile();
            var modelDefinition = new WaterFlowFMModelDefinition();
            const string propertyName = "unknown_property";
            const string expectedComment = "COMMENT";

            using (var tempDirectory = new TemporaryDirectory())
            {
                string filePath = Path.Combine(tempDirectory.Path, "FlowFM.mdu");
                File.WriteAllLines(filePath, new[]
                {
                    "[physics]",
                    $"{propertyName} = 1 # {expectedComment}"
                });

                // Call
                mduFile.Read(filePath, modelDefinition, new HydroArea(), null);
            }

            // Assert
            WaterFlowFMProperty property = modelDefinition.GetModelProperty(propertyName);
            string comment = property.PropertyDefinition.Description;

            Assert.That(comment, Is.EqualTo(expectedComment));
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void Read_WhenFileHasOldCategoryNameThenCategoryIsRenamed()
        {
            // Setup
            var mduFile = new MduFile();
            var modelDefinition = new WaterFlowFMModelDefinition();

            const string propertyName = "property";
            const string oldCategoryName = "model";
            const string newCategoryName = "General";

            using (var tempDirectory = new TemporaryDirectory())
            {
                string filePath = Path.Combine(tempDirectory.Path, "FlowFM.mdu");
                File.WriteAllLines(filePath, new[]
                {
                    $"[{oldCategoryName}]",
                    $"{propertyName} = D-Flow FM # Program name"
                });

                // Call
                mduFile.Read(filePath, modelDefinition, new HydroArea(), null);
            }

            // Assert
            WaterFlowFMProperty property = modelDefinition.GetModelProperty(propertyName);
            Assert.That(property.PropertyDefinition.FileSectionName, Is.EqualTo(newCategoryName),
                        $"Category [{oldCategoryName}] should be renamed to [{newCategoryName}].");
        }

        [Test]
        public void WriteMorphologyAndSedimentFiles()
        {
            var testFile = "ModelWithMorphology.mdu";
            var mduFile = new MduFile();
            var hydroArea = new HydroArea();
            var model = new WaterFlowFMModel();
            var sedimentData = model as ISedimentModelData;
            WaterFlowFMModelDefinition modelDefinition = model.ModelDefinition;
            modelDefinition.UseMorphologySediment = true;
            modelDefinition.ModelName = Path.GetFileNameWithoutExtension(testFile);

            var mduFileWriteConfig = new MduFileWriteConfig
            {
                WriteExtForcings = false,
                WriteFeatures = true,
                DisableFlowNodeRenumbering = false
            };

            mduFile.Write(testFile,
                          modelDefinition,
                          hydroArea,
                          new List<ModelFeatureCoordinateData<FixedWeir>>(),
                          mduFileWriteConfig,
                          false,
                          sedimentData);

            Assert.IsTrue(File.Exists(testFile));
            List<string> lines = File.ReadLines(testFile).ToList();
            Assert.IsTrue(lines.Any(l => l.Contains("ModelWithMorphology.mor")));
            Assert.IsTrue(lines.Any(l => l.Contains("ModelWithMorphology.sed")));
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void Test_MduFile_Read_Loads_BridgePillars()
        {
            string testDataFilePath = TestHelper.GetTestFilePath(@"ImportMDUFile\bridge-1.mdu");
            string testFilePath = TestHelper.CreateLocalCopy(testDataFilePath);

            try
            {
                var mduFile = new MduFile();
                var area = new HydroArea();

                mduFile.Read(testFilePath, new WaterFlowFMModelDefinition(), area, null);

                BridgePillar pillar = area.BridgePillars.Single();
                Assert.That(pillar.Name, Is.EqualTo("BridgePillar01"));

                //Check if now they are present.
                Assert.That(pillar.Attributes.Count, Is.EqualTo(2));
                var attributes = pillar.Attributes as DictionaryFeatureAttributeCollection;
                Assert.IsNotNull(attributes);

                var expectedDiameters = new List<double>
                {
                    -599,
                    -599,
                    -999,
                    -999
                };
                var expectedCoeff = new List<double>
                {
                    -999,
                    -999,
                    -499,
                    -499
                };

                CheckAttributeCollection(attributes, "Column3", expectedDiameters);
                CheckAttributeCollection(attributes, "Column4", expectedCoeff);
            }
            finally
            {
                FileUtils.DeleteIfExists(testFilePath);
            }
        }

        [Test]
        [Category(NghsTestCategory.PerformanceDotTrace)]
        public void Read_MduFileWithBridgePillars_ShouldBeWithinExecutionTime()
        {
            string testDataFilePath = TestHelper.GetTestFilePath(@"ImportMDUFile\bridge-1.mdu");
            string testFilePath = TestHelper.CreateLocalCopy(testDataFilePath);

            try
            {
                var mduFile = new MduFile();
                var area = new HydroArea();

                TimerMethod_ReadMduFileWithBridgePillars(mduFile, testFilePath, area);
            }
            finally
            {
                FileUtils.DeleteIfExists(testFilePath);
            }
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
                mduFile.Write(testFile, modelDefinition, hydroArea, new List<ModelFeatureCoordinateData<FixedWeir>>());
            }
            catch (Exception e)
            {
                Assert.Fail($"Test crashed. {e.Message}");
            }

            Assert.IsTrue(File.Exists(testFile));
            IEnumerable<string> lines = File.ReadLines(testFile);
            Assert.IsTrue(lines.Any(l => l.Contains("PillarFile")));
        }

        [Test]
        public void Write_AllFixedWeirsAndCorrespondingPropertiesNull_ThrowsArgumentNullException()
        {
            // Setup
            var mduFile = new MduFile();

            // Call
            void Call() => mduFile.Write("path", new WaterFlowFMModelDefinition(), new HydroArea(), null);

            // Assert
            var e = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(e.ParamName, Is.EqualTo("allFixedWeirsAndCorrespondingProperties"));
        }

        [Test]
        public void Test_MduFile_Write_WithBridgePillars_Writes_BridgePillars_Entry_AndFile()
        {
            string tempFileName = Path.GetTempFileName();
            string testFile = string.Concat(tempFileName, ".mdu");

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
                    })
            };
            hydroArea.BridgePillars.Add(pillar);

            try
            {
                mduFile.Write(testFile, modelDefinition, hydroArea, new List<ModelFeatureCoordinateData<FixedWeir>>());
            }
            catch (Exception e)
            {
                Assert.Fail($"Test crashed. {e.Message}");
            }

            Assert.IsTrue(File.Exists(testFile));
            string allText = File.ReadAllText(testFile);
            WaterFlowFMMduFileTestHelper.AssertContainsMduLine(allText, "PillarFile", $"{Path.GetFileName(tempFileName)}.pliz");

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
                    })
            };
            var modelFeatureCoordinateDatas = new List<ModelFeatureCoordinateData<BridgePillar>>();

            //Create values for the DataColumns.
            var modelFeatureCoordinateData = new ModelFeatureCoordinateData<BridgePillar>();
            modelFeatureCoordinateData.UpdateDataColumns();
            modelFeatureCoordinateData.Feature = bp;
            modelFeatureCoordinateData.DataColumns[0].ValueList = new List<double>
            {
                1.0,
                2.5,
                5.0,
                10.0
            };
            modelFeatureCoordinateData.DataColumns[1].ValueList = new List<double>
            {
                10.0,
                5.0,
                2.5,
                1.0
            };

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
            var dictionaryFeatureAttributeCollection = new DictionaryFeatureAttributeCollection {{"testAttr", 23}};
            var bp = new BridgePillar {Attributes = dictionaryFeatureAttributeCollection};

            Assert.IsTrue(bp.Attributes.Any());
            MduFile.CleanBridgePillarAttributes(new List<BridgePillar> {bp});
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
                    })
            };

            var listofDataModel = new List<ModelFeatureCoordinateData<BridgePillar>>();

            #region set Attribute values to bridge pillar

            /*We were not able to set the Attributes property for Bridge pillar, so we use the following code to di for us*/
            //Create values for the DataColumns
            var modelFeatureCoordinateData = new ModelFeatureCoordinateData<BridgePillar>();
            modelFeatureCoordinateData.UpdateDataColumns();
            modelFeatureCoordinateData.Feature = bp;
            modelFeatureCoordinateData.DataColumns[0].ValueList = new List<double>
            {
                1.0,
                2.5,
                5.0,
                10.0
            };
            modelFeatureCoordinateData.DataColumns[1].ValueList = new List<double>
            {
                10.0,
                5.0,
                2.5,
                1.0
            };

            listofDataModel.Add(modelFeatureCoordinateData);

            MduFile.SetBridgePillarAttributes(new List<BridgePillar> {bp}, listofDataModel);

            Assert.IsNotNull(bp.Attributes);
            Assert.AreEqual(2, bp.Attributes.Count);

            #endregion

            listofDataModel.Clear();
            var bpDataModel = new ModelFeatureCoordinateData<BridgePillar>() {Feature = bp};
            bpDataModel.UpdateDataColumns();

            listofDataModel.Add(bpDataModel);

            Assert.IsNotNull(bpDataModel.DataColumns);
            Assert.AreEqual(2, bpDataModel.DataColumns.Count);

            var diameterList = new List<double>
            {
                1.0,
                2.5,
                5.0,
                10.0
            };
            var coeffList = new List<double>
            {
                10.0,
                5.0,
                2.5,
                1.0
            };

            Assert.AreNotEqual(diameterList, bpDataModel.DataColumns[0].ValueList as List<double>);
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
            string testPath = TestHelper.GetTestFilePath(@"ImportMDUFile\IncorrectPlizFile\bridge-1.mdu");
            testPath = TestHelper.CreateLocalCopy(testPath);
            Assert.IsNotNull(testPath);
            Assert.IsTrue(File.Exists(testPath));

            var mduFile = new MduFile();
            var area = new HydroArea();

            Assert.DoesNotThrow(() => mduFile.Read(testPath, new WaterFlowFMModelDefinition(), area, null, allBridgePillarsAndCorrespondingProperties: new List<ModelFeatureCoordinateData<BridgePillar>>()), "It Crashed");
        }

        [Test]
        public void Test_MduFile_Read_BridgePillar_WithTooManyColumns_IsImported_AndMessageIsGiven()
        {
            string testPath = TestHelper.GetTestFilePath(@"ImportMDUFile\IncorrectPlizFile\bridge-1.mdu");
            testPath = TestHelper.CreateLocalCopy(testPath);
            Assert.IsNotNull(testPath);
            Assert.IsTrue(File.Exists(testPath));

            var mduFile = new MduFile();
            var area = new HydroArea();

            string expectedMsg = string.Format(
                Resources.MduFile_Read_Based_on_the_Bridge_Pillar_file__0___there_are_too_many_column_s__defined_for__1___The_last__2__column_s__have_been_ignored,
                "bridge-1.pliz", "BridgePillar01", 1);
            TestHelper.AssertAtLeastOneLogMessagesContains(
                () => mduFile.Read(testPath, new WaterFlowFMModelDefinition(), area, null, allBridgePillarsAndCorrespondingProperties: new List<ModelFeatureCoordinateData<BridgePillar>>()),
                expectedMsg
            );
        }

        [Test]
        public void Test_MduFile_Read_BridgePillar_WithTooFewColumns_IsImported_AndMessageIsGiven()
        {
            string testPath = TestHelper.GetTestFilePath(@"ImportMDUFile\IncorrectPlizFile\bridge-2.mdu");
            testPath = TestHelper.CreateLocalCopy(testPath);
            Assert.IsNotNull(testPath);
            Assert.IsTrue(File.Exists(testPath));

            var mduFile = new MduFile();
            var area = new HydroArea();

            string expectedMsg = string.Format(
                Resources.MduFile_Read_Based_on_the_Bridge_Pillar_file__0___there_are_not_enough_column_s__defined_for__1___The_last__2__column_s__have_been_generated_using_default_values,
                "bridge-2.pliz", "BridgePillar02", 1);

            TestHelper.AssertAtLeastOneLogMessagesContains(
                () => mduFile.Read(testPath, new WaterFlowFMModelDefinition(), area, null, allBridgePillarsAndCorrespondingProperties: new List<ModelFeatureCoordinateData<BridgePillar>>()),
                expectedMsg
            );
        }

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
                var originalMd = new WaterFlowFMModelDefinition(modelName);
                var allFixedWeirsAndCorrespondingProperties = new Dictionary<FixedWeir, ModelFeatureCoordinateData<FixedWeir>>();
                mduFile.Read(mduFilePath, originalMd, originalArea, allFixedWeirsAndCorrespondingProperties);

                ModelFeatureCoordinateData<FixedWeir> coordinateData = allFixedWeirsAndCorrespondingProperties.ElementAt(0).Value;
                Assert.AreEqual(7, coordinateData.DataColumns.Count);

                //CrestLevel
                Assert.AreEqual(0, coordinateData.DataColumns[0].ValueList[0]);
                Assert.AreEqual(0, coordinateData.DataColumns[0].ValueList[1]);

                //Ground Height Left
                Assert.AreEqual(0, coordinateData.DataColumns[1].ValueList[0]);
                Assert.AreEqual(0, coordinateData.DataColumns[1].ValueList[1]);

                //Ground Height Right
                Assert.AreEqual(0, coordinateData.DataColumns[2].ValueList[0]);
                Assert.AreEqual(0, coordinateData.DataColumns[2].ValueList[1]);

                //Crest Width
                Assert.AreEqual(3, coordinateData.DataColumns[3].ValueList[0]);
                Assert.AreEqual(3, coordinateData.DataColumns[3].ValueList[1]);

                //Slope Left
                Assert.AreEqual(4, coordinateData.DataColumns[4].ValueList[0]);
                Assert.AreEqual(4, coordinateData.DataColumns[4].ValueList[1]);

                //Slope Right
                Assert.AreEqual(4, coordinateData.DataColumns[5].ValueList[0]);
                Assert.AreEqual(4, coordinateData.DataColumns[5].ValueList[1]);

                //Roughness Code
                Assert.AreEqual(0, coordinateData.DataColumns[6].ValueList[0]);
                Assert.AreEqual(0, coordinateData.DataColumns[6].ValueList[1]);

                mduFile.Write(savePath, originalMd, originalArea, allFixedWeirsAndCorrespondingProperties.Values, switchTo: false);

                var twoFixedWeirsFxwPliz = "TwoFixedWeirs_fxw2_fxw.pliz";
                string[] generatedResultsContent = File.ReadAllLines(Path.Combine(newMduDir, twoFixedWeirsFxwPliz));

                var expectedResultsContent =
                    new[]
                    {
                        "Weir01",
                        "    2    9",
                        "5.400000000000000E+000  4.600000000000000E+000  0.000000000000000E+000  0.000000000000000E+000  0.000000000000000E+000  3.000000000000000E+000  4.000000000000000E+000  4.000000000000000E+000  0.000000000000000E+000",
                        "1.200000000000000E+000  1.000000000000000E+001  0.000000000000000E+000  0.000000000000000E+000  0.000000000000000E+000  3.000000000000000E+000  4.000000000000000E+000  4.000000000000000E+000  0.000000000000000E+000",
                        "Weir02",
                        "    2    9",
                        "2.000000000000000E+000  7.000000000000000E-001  0.000000000000000E+000  0.000000000000000E+000  0.000000000000000E+000  3.000000000000000E+000  4.000000000000000E+000  4.000000000000000E+000  0.000000000000000E+000",
                        "3.900000000000000E+000  3.900000000000000E+000  0.000000000000000E+000  0.000000000000000E+000  0.000000000000000E+000  3.000000000000000E+000  4.000000000000000E+000  4.000000000000000E+000  0.000000000000000E+000"
                    };

                for (var i = 0; i < 8; i++)
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

            var expectedResultsContent =
                new[]
                {
                    "Weir01",
                    "    2    5",
                    "5.400000000000000E+000  4.600000000000000E+000  1.200000000000000E+000  3.500000000000000E+000  3.200000000000000E+000",
                    "1.200000000000000E+000  1.000000000000000E+001  6.400000000000000E+000  3.000000000000000E+000  3.300000000000000E+000",
                    "Weir02",
                    "    2    5",
                    "2.000000000000000E+000  7.000000000000000E-001  1.700000000000000E+000  4.500000000000000E+000  4.200000000000000E+000",
                    "3.900000000000000E+000  3.900000000000000E+000  6.100000000000000E+000  4.000000000000000E+000  4.300000000000000E+000"
                };
            try
            {
                var mduFile = new MduFile();

                var originalArea = new HydroArea();
                var originalMd = new WaterFlowFMModelDefinition(modelName);
                var allFixedWeirsAndCorrespondingProperties = new Dictionary<FixedWeir, ModelFeatureCoordinateData<FixedWeir>>();

                mduFile.Read(mduFilePath, originalMd, originalArea, allFixedWeirsAndCorrespondingProperties);

                ModelFeatureCoordinateData<FixedWeir> coordinateData = allFixedWeirsAndCorrespondingProperties.ElementAt(0).Value;

                Assert.AreEqual(3, coordinateData.DataColumns.Count);

                //CrestLevel
                Assert.AreEqual(1.2, coordinateData.DataColumns[0].ValueList[0]);
                Assert.AreEqual(6.4, coordinateData.DataColumns[0].ValueList[1]);

                //Ground Height Left
                Assert.AreEqual(3.5, coordinateData.DataColumns[1].ValueList[0]);
                Assert.AreEqual(3.0, coordinateData.DataColumns[1].ValueList[1]);

                //Ground Height Right
                Assert.AreEqual(3.2, coordinateData.DataColumns[2].ValueList[0]);
                Assert.AreEqual(3.3, coordinateData.DataColumns[2].ValueList[1]);

                mduFile.Write(savePath, originalMd, originalArea, allFixedWeirsAndCorrespondingProperties.Values, switchTo: false);

                const string twoFixedWeirsFxwPliz = "TwoFixedWeirs_fxw.pliz";
                string[] generatedResultsContent = File.ReadAllLines(Path.Combine(newMduDir, twoFixedWeirsFxwPliz));

                for (var i = 0; i < 8; i++)
                {
                    Assert.AreEqual(expectedResultsContent[i], generatedResultsContent[i],
                                    $"Line {i + 1} of generated file {savePath} differs from the expected result.");
                }
            }
            finally
            {
                FileUtils.DeleteIfExists(mduDir);
            }
        }

        /// <summary>
        /// GIVEN a pliz file containing fixed weirs with some number of columns
        /// AND an mdu file referencing this pliz file and fixed weir scheme 0
        /// WHEN this mdu file is imported
        /// THEN no messages concerning the the fixed weir columns are generated
        /// </summary>
        [Test]
        [Category(TestCategory.Integration)]
        public void GivenAPlizFileAndAnMduFileWithFixedWeirScheme0_WhenThisMduFileIsImported_ThenNoMessagesConcerningTheTheFixedWeirColumnsAreGenerated()
        {
            using (var tempDir = new TemporaryDirectory())
            {
                // Given
                string srcPath = TestHelper.GetTestFilePath(@"HydroAreaCollection\FlowFMFixedWeirs\");

                const string mduFileName = "FlowFM4.mdu";
                const string plizFileName = "TwoFixedweirs_fxw.pliz";

                tempDir.CopyAllTestDataToTempDirectory(Path.Combine(srcPath, mduFileName),
                                                       Path.Combine(srcPath, plizFileName));

                var mduFile = new MduFile();

                var originalArea = new HydroArea();
                var originalMd = new WaterFlowFMModelDefinition(modelName);
                var allFixedWeirsAndCorrespondingProperties = new Dictionary<FixedWeir, ModelFeatureCoordinateData<FixedWeir>>();

                // When
                void TestAction()
                {
                    mduFile.Read(Path.Combine(tempDir.Path, mduFileName),
                                 originalMd,
                                 originalArea,
                                 allFixedWeirsAndCorrespondingProperties);
                }

                IEnumerable<string> msgs = TestHelper.GetAllRenderedMessages(TestAction);

                // Then
                Assert.That(msgs, Is.Not.Null, "Expected the rendered messages not to be null.");

                const string unexpectedMsgHeader = "During reading the Fixed Weirs the following";
                Assert.That(msgs.Any(m => m.StartsWith(unexpectedMsgHeader)), Is.False,
                            "Expected no messages concerning reading of Fixed Weirs, but some exist.");
            }
        }

        /// <summary>
        /// GIVEN a MduFile
        /// AND a hydroArea
        /// AND some fixedWeirs properties
        /// AND some mdu file with fixed weirs with a different number of columns than needed
        /// WHEN Read is called
        /// THEN a single error message is logged
        /// AND the error message contains a warning for each weir
        /// </summary>
        [Test]
        [TestCaseSource(nameof(WeirWarningMessageTestCaseData))]
        [Category(TestCategory.DataAccess)]
        public void GivenAMduFileWithADifferentNumberOfColumnsThanNeeded_WhenReadIsCalled_ThenAnErrorMessageIsLogged(
            string mduName, 
            string fixedWeirPlizFileName, 
            int weirScheme, 
            int columnDifference, 
            string expectedSubMsgFormat
            )
        {
            using (var tempDir = new TemporaryDirectory())
            {
                // Given
                string srcFilePath = TestHelper.GetTestFilePath(@"HydroAreaCollection\FlowFMFixedWeirs\");

                tempDir.CopyAllTestDataToTempDirectory(Path.Combine(srcFilePath, mduName),
                                                       Path.Combine(srcFilePath, fixedWeirPlizFileName),
                                                       Path.Combine(srcFilePath, "FlowFM_net.nc"));

                var originalArea = new HydroArea();
                var originalMd = new WaterFlowFMModelDefinition(modelName);
                var allFixedWeirsAndCorrespondingProperties =
                    new Dictionary<FixedWeir, ModelFeatureCoordinateData<FixedWeir>>();

                var mduFile = new MduFile();

                string readPath = Path.Combine(tempDir.Path, mduName);

                // When
                void testAction()
                {
                    mduFile.Read(readPath, originalMd, originalArea, allFixedWeirsAndCorrespondingProperties);
                }

                List<string> msgs = TestHelper.GetAllRenderedMessages(testAction).ToList();

                // Then
                Assert.That(msgs, Has.Count.EqualTo(3), "Expected three grouped warning message:");
                
                const string expectedMsgHeader = "During reading the Fixed Weirs the following warnings were reported:";
                string msg = msgs.FirstOrDefault(m => m.StartsWith(expectedMsgHeader));
                Assert.That(msg, Is.Not.Null, "Expected the header of the message to be different:");

                List<string> subMsgs = msg.Split(new[]
                {
                    "\n- "
                }, StringSplitOptions.None).ToList();
                subMsgs.RemoveAt(0);                              // Remove header msg.
                subMsgs = subMsgs.Select(s => s.Trim()).ToList(); // Remove excessive white characters.

                Assert.That(subMsgs, Has.Count.EqualTo(2), "Expected 2 sub messages within the warning message.");
                Assert.That(subMsgs[0], Is.EqualTo(string.Format(expectedSubMsgFormat, weirScheme, "Weir01", columnDifference)),
                            "Expected a different string as first sub message.");
                Assert.That(subMsgs[1], Is.EqualTo(string.Format(expectedSubMsgFormat, weirScheme, "Weir02", columnDifference)),
                            "Expected a different string as second sub message.");
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
                var originalMd = new WaterFlowFMModelDefinition(modelName);
                var allFixedWeirsAndCorrespondingProperties = new Dictionary<FixedWeir, ModelFeatureCoordinateData<FixedWeir>>();

                mduFile.Read(mduFilePath, originalMd, originalArea, allFixedWeirsAndCorrespondingProperties);

                //Check if the enclosure file is in memory under the new mdu property name in the model definition.
                WaterFlowFMProperty newModelProperty = originalMd.GetModelProperty(KnownProperties.EnclosureFile);
                Assert.NotNull(newModelProperty);

                //Check that the old mdu property name is not existing anymore in the model definition.
                WaterFlowFMProperty oldModelProperty = originalMd.GetModelProperty("enclosurefile");
                Assert.IsNull(oldModelProperty);

                mduFile.Write(savePath, originalMd, originalArea, allFixedWeirsAndCorrespondingProperties.Values);

                string[] generatedInputContent =
                    File.ReadAllLines(mduFilePath);

                Assert.IsFalse(generatedInputContent.Any(x => x.ToLower().Contains("gridenclosurefile")));
                Assert.IsTrue(generatedInputContent.Any(x => x.ToLower().Contains("enclosurefile")));

                string[] generatedResultsContent =
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
                var originalMd = new WaterFlowFMModelDefinition(modelName);
                var allFixedWeirsAndCorrespondingProperties = new Dictionary<FixedWeir, ModelFeatureCoordinateData<FixedWeir>>();

                mduFile.Read(mduFilePath, originalMd, originalArea, allFixedWeirsAndCorrespondingProperties);

                //Check if the enclosure file is in memory under the new mdu property name in the model definition.
                WaterFlowFMProperty newModelProperty = originalMd.GetModelProperty(KnownProperties.EnclosureFile);
                Assert.NotNull(newModelProperty);

                //Check that the old mdu property name is not existing anymore in the model definition.
                WaterFlowFMProperty oldModelProperty = originalMd.GetModelProperty("enclosurefile");
                Assert.IsNull(oldModelProperty);

                mduFile.Write(savePath, originalMd, originalArea, allFixedWeirsAndCorrespondingProperties.Values);

                string[] generatedInputContent =
                    File.ReadAllLines(mduFilePath);

                Assert.IsTrue(generatedInputContent.Any(x => x.ToLower().Contains("gridenclosurefile")));

                string[] generatedResultsContent =
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
        [TestCase(@"ModelWithDrypointData\FlowFM\input_without_drysuffix\FlowFM.mdu")]
        [TestCase(@"ModelWithDrypointData\FlowFM\input_with_drysuffix\FlowFM.mdu")]
        public void GivenAnMduToReadWithDryPoints_WhenReadingTheDryPointFile_ThenAreaHasDryPoints(string mduFileName)
        {
            mduFileName = TestHelper.GetTestFilePath(mduFileName);
            mduFileName = TestHelper.CreateLocalCopy(mduFileName);
            mduDir = Path.GetDirectoryName(mduFileName);
            Assert.NotNull(mduDir);
            modelName = Path.GetFileName(mduFileName);

            var mduFile = new MduFile();
            var originalArea = new HydroArea();
            var originalModelDefinition = new WaterFlowFMModelDefinition(modelName);

            mduFile.Read(mduFileName, originalModelDefinition, originalArea, null);

            IEventedList<GroupablePointFeature> dryPointsOnArea = originalArea.DryPoints;
            Assert.AreEqual(8, dryPointsOnArea.Count);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void Test_ImportMdu_Without_DryArea_Suffix_Gets_DryAreas()
        {
            // 1. Set up test model
            const int expectedAreas = 4;
            var mduFilePath = @"ModelWithDrypointData\D3DFMIQ-1037\FlowFM.mdu";
            WaterFlowFMModelDefinition modelDefinition = null;
            var mduFile = new MduFile();
            var originalArea = new HydroArea();

            using (var temporaryDirectory = new TemporaryDirectory())
            {
                mduFilePath = temporaryDirectory.CopyTestDataFileAndDirectoryToTempDirectory(mduFilePath);
                Assert.That(File.Exists(mduFilePath), $"MDU File was not found at {mduFilePath}.");
                modelName = Path.GetFileName(mduFilePath);

                // 2. Set initial expectations.
                Action createModelDefinition = () => modelDefinition = new WaterFlowFMModelDefinition(modelName);
                Action readMduFile = () => mduFile.Read(mduFilePath, modelDefinition, originalArea, null);

                // 3. Run test (Import areas)
                Assert.DoesNotThrow(() => createModelDefinition.Invoke(), "Test fail while trying to create a model definition object.");
                Assert.DoesNotThrow(() => readMduFile.Invoke(), "Test fail while trying to read the MDU file.");

                // 4. Verify final expectations
                int importedDryAreas = originalArea.DryAreas.Count;
                Assert.That(importedDryAreas, Is.EqualTo(expectedAreas), $"Imported number of areas {importedDryAreas} does not match the expected amount ({expectedAreas}");
            }
        }

        [Test]
        [Category(NghsTestCategory.PerformanceDotTrace)]
        public void Write_ManyFixedWeirs()
        {
            // Setup
            var mduFile = new MduFile();
            HydroArea hydroArea = GetHydroAreaWithManyFixedWeirs();
            IEnumerable<ModelFeatureCoordinateData<FixedWeir>> fixedWeirData = GetFixedWeirData(hydroArea);

            using (var tempDir = new TemporaryDirectory())
            {
                string filePath = Path.Combine(tempDir.Path, "model.mdu");

                // Call
                mduFile.Write(filePath, new WaterFlowFMModelDefinition(), hydroArea, fixedWeirData);
            }
        }

        /// <summary>
        /// Method to test by dot Trace. Should be public for setting thresholds.
        /// </summary>
        /// <param name="mduFile"> The Mdu file. </param>
        /// <param name="testFilePath">The Mdu file path. </param>
        /// <param name="area"> The area of the model. </param>
        public static void TimerMethod_ReadMduFileWithBridgePillars(MduFile mduFile, string testFilePath, HydroArea area)
        {
            mduFile.Read(testFilePath, new WaterFlowFMModelDefinition(), area, null);
        }

        private static void CheckAttributeCollection(DictionaryFeatureAttributeCollection attributes, string columnName, List<double> valueList)
        {
            Assert.IsNotNull(valueList);
            object setValues;
            Assert.IsTrue(attributes.TryGetValue(columnName, out setValues));
            var geometryPointsSyncedList = setValues as GeometryPointsSyncedList<double>;

            Assert.IsNotNull(geometryPointsSyncedList);

            var idx = 0;
            foreach (double point in geometryPointsSyncedList)
            {
                Assert.AreEqual(point, valueList[idx]);
                idx++;
            }
        }

        [TestCase('=', 34)]
        [TestCase('#', 52)]
        [Category(TestCategory.DataAccess)]
        public void Write_ThenAllPropertiesAreCorrectlyAligned(char separator, int expectedIndex)
        {
            // Setup
            var mduFile = new MduFile();
            var modelDefinition = new WaterFlowFMModelDefinition();
            var config = MockRepository.GenerateStub<IMduFileWriteConfig>();

            foreach (WaterFlowFMProperty property in modelDefinition.Properties)
            {
                property.PropertyDefinition.Description = "comment";
            }

            string[] lines;
            using (var tempDirectory = new TemporaryDirectory())
            {
                string writeFilePath = Path.Combine(tempDirectory.Path, "FlowFM.mdu");

                // Call
                mduFile.Write(writeFilePath, modelDefinition, null, new List<ModelFeatureCoordinateData<FixedWeir>>(), config);

                lines = File.ReadAllLines(writeFilePath);
            }

            // Assert
            string[] relevantLines = lines.Where(l => l.Contains(separator) && l.IndexOf(separator) != 0).ToArray();

            int index = relevantLines.First().IndexOf(separator);
            Assert.That(index, Is.EqualTo(expectedIndex), $"Index of {separator} was not as expected.");

            bool areAligned = relevantLines.Select(l => l.IndexOf('=')).Distinct().Count() == 1;
            Assert.That(areAligned, $"All {separator}'s in the file should be aligned.");
        }

        private static WaterFlowFMProperty CreateProperty(string name, string comment)
        {
            var propertyDefinition = new WaterFlowFMPropertyDefinition
            {
                MduPropertyName = name,
                Description = comment,
                DataType = typeof(string),
                FileSectionName = "custom_category"
            };

            return new WaterFlowFMProperty(propertyDefinition, "custom_value");
        }

        [TestCase("enclosurefile", "GridEnclosureFile")]
        [TestCase("trtdt", "DtTrt")]
        [TestCase("botlevuni", "BedLevUni")]
        [TestCase("botlevtype", "BedLevType")]
        [TestCase("mduformatversion", "FileVersion")]
        [Category(TestCategory.DataAccess)]
        public void Read_WhenFileHasOldPropertyNameThenPropertyIsRenamed(string oldPropertyName, string newPropertyName)
        {
            // Setup
            var mduFile = new MduFile();
            var modelDefinition = new WaterFlowFMModelDefinition();

            using (var tempDirectory = new TemporaryDirectory())
            {
                string filePath = Path.Combine(tempDirectory.Path, "FlowFM.mdu");
                File.WriteAllLines(filePath, new[]
                {
                    "[category]",
                    $"{oldPropertyName} = 0"
                });

                // Call
                mduFile.Read(filePath, modelDefinition, new HydroArea(), null);
            }

            // Assert
            Assert.That(modelDefinition.ContainsProperty(oldPropertyName), Is.False,
                        $"Model definition should not contain property with name '{oldPropertyName}'");
            Assert.That(modelDefinition.ContainsProperty(newPropertyName), Is.True,
                        $"Model definition should contain property with name '{newPropertyName}'");
        }

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
                var originalMd = new WaterFlowFMModelDefinition(modelName);
                var allFixedWeirsAndCorrespondingProperties = new Dictionary<FixedWeir, ModelFeatureCoordinateData<FixedWeir>>();
                mduFile.Read(mduFilePath, originalMd, originalArea, allFixedWeirsAndCorrespondingProperties);
                mduFile.Write(savePath, originalMd, originalArea, allFixedWeirsAndCorrespondingProperties.Values, switchTo: false);

                string netFileLocationShouldBe = Path.Combine(newMduDir, relativeNcFilePath);

                Assert.IsTrue(File.Exists(netFileLocationShouldBe));
            }
            finally
            {
                FileUtils.DeleteIfExists(mduDir);
            }
        }

        [Category(TestCategory.Integration)]
        [TestCase(@"cs_after_save\before_save_AmersfoortRDNew_net.nc", 28992, "Amersfoort / RD New")]
        [TestCase(@"cs_after_save\before_save_AmersfoortRDOld_net.nc", 28991, "Amersfoort / RD Old")]
        [TestCase(@"cs_after_save\before_save_UTMzone30N_net.nc", 32630, "WGS 84 / UTM zone 30N")]
        public void SetCoordinateSystemNameNetfileWithModelCoordinateSystemNameTest(string netFile, int espgModel, string expectedCoordinateSystemName)
        {
            string workingDirectory = FileUtils.CreateTempDirectory();
            ICoordinateSystem coordinateSystem = new OgrCoordinateSystemFactory().CreateFromEPSG(espgModel);

            string netFilePath = TestHelper.GetTestFilePath(netFile);
            var netFileInfo = new FileInfo(netFilePath);
            Assert.IsTrue(netFileInfo.Exists);

            string workingNetFilePath = Path.Combine(workingDirectory, netFileInfo.Name);
            var workingNetFileInfo = new FileInfo(workingNetFilePath);
            FileUtils.CopyFile(netFileInfo.FullName, workingNetFilePath);
            Assert.IsTrue(workingNetFileInfo.Exists);

            var mduFile = new MduFile();
            var fileProjectedName = TypeUtils.CallPrivateMethod<string>(mduFile, "GetProjectedCoordinateSystemNameFromNetFile", workingNetFilePath);
            Assert.That(fileProjectedName, Is.EqualTo("Unknown projected"));

            UnstructuredGridFileHelper.SetCoordinateSystem(workingNetFilePath, coordinateSystem);

            var editedFileProjectedName = TypeUtils.CallPrivateMethod<string>(mduFile, "GetProjectedCoordinateSystemNameFromNetFile", workingNetFilePath);
            Assert.IsTrue(editedFileProjectedName.Equals(expectedCoordinateSystemName));

            FileUtils.DeleteIfExists(workingDirectory);
        }

        [Category(TestCategory.Integration)]
        [TestCase(true, @"update_CS_netfile\amersfoortRDNew_net.nc", 28992, true)]
        [TestCase(true, @"update_CS_netfile\unknown_projected_net.nc", 28992, false)]
        [TestCase(true, @"update_CS_netfile\wgs84_net.nc", 4326, true)]
        [TestCase(false, @"update_CS_netfile\amersfoortRDNew_net.nc", 28992, true)]
        [TestCase(false, @"update_CS_netfile\unknown_projected_net.nc", 28992, true)]
        [TestCase(false, @"update_CS_netfile\wgs84_net.nc", 4326, true)]
        [TestCase(true, @"update_CS_netfile\amersfoortRDNew_net.nc", 28991, false)]
        [TestCase(true, @"update_CS_netfile\unknown_projected_net.nc", 28991, false)]
        [TestCase(true, @"update_CS_netfile\wgs84_net.nc", 4326, true)]
        [TestCase(false, @"update_CS_netfile\amersfoortRDNew_net.nc", 28991, true)]
        [TestCase(false, @"update_CS_netfile\unknown_projected_net.nc", 28991, true)]
        [TestCase(true, @"update_CS_netfile\wgs84_net.nc", 28992, false)]
        [TestCase(true, @"update_CS_netfile\amersfoortRDNew_net.nc", 4326, false)]
        public void ShouldUpdateNetfileCoordinateSystemTest(bool hasCoordinateSystem, string targetFile, int epsgModelDefinition, bool expected)
        {
            string netFilePath = TestHelper.GetTestFilePath(targetFile);
            var netFileInfo = new FileInfo(netFilePath);
            Assert.IsTrue(netFileInfo.Exists);

            var modelDefinition = new WaterFlowFMModelDefinition();
            var mduFile = new MduFile();
            if (hasCoordinateSystem)
            {
                modelDefinition.CoordinateSystem = new OgrCoordinateSystemFactory().CreateFromEPSG(epsgModelDefinition);
            }

            var result = TypeUtils.CallPrivateMethod<bool>(mduFile, "IsNetfileCoordinateSystemUpToDate", modelDefinition, netFilePath);

            Assert.That(result, Is.EqualTo(expected));
        }
        
        [Test]
        [Category(TestCategory.DataAccess)]
        public void WritingAnMduFileShouldNotWriteTStartOrTStopKeywords_ButShouldWriteOnlyStartDateTimeAndStopDateTime()
        {
            // Setup
            var mduFile = new MduFile();
            var modelDefinition = new WaterFlowFMModelDefinition();
            var config = Substitute.For<IMduFileWriteConfig>();

            string[] lines;
            using (var tempDirectory = new TemporaryDirectory())
            {
                const string fileName = "FlowFM.mdu";
                string writeFilePath = Path.Combine(tempDirectory.Path, fileName);

                // Call
                mduFile.Write(writeFilePath,
                              modelDefinition,
                              null,
                              Enumerable.Empty<ModelFeatureCoordinateData<FixedWeir>>(),
                              config);

                lines = File.ReadAllLines(writeFilePath);
            }

            // Assert
            Assert.That(lines, Has.None.Matches<string>(line => LineStartsWith(line, KnownLegacyProperties.TStart)));
            Assert.That(lines, Has.None.Matches<string>(line => LineStartsWith(line, KnownLegacyProperties.TStop)));
            Assert.That(lines, Has.One.Matches<string>(line => LineStartsWith(line, KnownProperties.StartDateTime)));
            Assert.That(lines, Has.One.Matches<string>(line => LineStartsWith(line, KnownProperties.StopDateTime)));
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        [TestCaseSource(nameof(Get3DLayerPropertiesTestCases))]
        public void Writing3DLayerPropertiesShouldUseDefaultValuesIfPropertyIsDisabled(string propertyName,
                                                                                       string validNonDefaultValueAsString)
        {
            // Setup
            using (var tempDir = new TemporaryDirectory())
            {
                string mduFilepath = Path.Combine(tempDir.Path, "random.mdu");
                
                var mduFile = new MduFile();

                var modelDefinition = new WaterFlowFMModelDefinition();
                WaterFlowFMProperty property = modelDefinition.GetModelProperty(propertyName);
                string expectedDefaultValue = property.PropertyDefinition.DefaultValueAsString;
                
                // Precondition
                Assert.That(expectedDefaultValue, Is.Not.EqualTo(validNonDefaultValueAsString));
                
                property.SetValueFromString(validNonDefaultValueAsString);
                property.PropertyDefinition.IsEnabled = properties => false; // disable the property
                
                // Call
                mduFile.WriteProperties(mduFilepath, modelDefinition.Properties, null, false);
                
                // Assert
                string[] lines = File.ReadAllLines(mduFilepath);
                string value = GetValueForPropertyFromMduFile(propertyName, lines);

                Assert.That(value, Is.EqualTo(expectedDefaultValue));
            }
        }
        
        [Test]
        [Category(TestCategory.DataAccess)]
        public void WritingNumTopSigPropertyShouldUseDefaultValuesIfPropertyIsDisabled()
        {
            // Setup
            using (var tempDir = new TemporaryDirectory())
            {
                string mduFilepath = Path.Combine(tempDir.Path, "random.mdu");
                
                var mduFile = new MduFile();

                var modelDefinition = new WaterFlowFMModelDefinition();
                WaterFlowFMProperty property = modelDefinition.GetModelProperty(KnownProperties.NumTopSig);
                string expectedDefaultValue = property.PropertyDefinition.DefaultValueAsString;
                const string validNonDefaultValueAsString = "2"; // value between 0 and kmx

                modelDefinition.GetModelProperty(KnownProperties.Kmx).SetValueFromString("10"); // random valid number
                
                // Precondition
                Assert.That(expectedDefaultValue, Is.Not.EqualTo(validNonDefaultValueAsString));
                
                property.SetValueFromString(validNonDefaultValueAsString);
                property.PropertyDefinition.IsEnabled = properties => false; // disable the property
                
                // Call
                mduFile.WriteProperties(mduFilepath, modelDefinition.Properties, null, false);
                
                // Assert
                string[] lines = File.ReadAllLines(mduFilepath);
                string value = GetValueForPropertyFromMduFile(KnownProperties.NumTopSig, lines);

                Assert.That(value, Is.EqualTo(expectedDefaultValue));
            }
        }

        private static IEnumerable<TestCaseData> Get3DLayerPropertiesTestCases()
        {
            yield return new TestCaseData(KnownProperties.DzTop, "9999");
            yield return new TestCaseData(KnownProperties.FloorLevTopLay, "-9999");
            yield return new TestCaseData(KnownProperties.DzTopUniAboveZ, "-9999");
            yield return new TestCaseData(KnownProperties.SigmaGrowthFactor, "9999");
            yield return new TestCaseData(KnownProperties.NumTopSigUniform, "1");
        }

        private static string GetValueForPropertyFromMduFile(string property, IEnumerable<string> fileContent)
        {
            string line = fileContent.FirstOrDefault(l => l.TrimStart().StartsWith(property, StringComparison.InvariantCultureIgnoreCase));
            if (line is null)
            {
                Assert.Fail($"Mdu file does not contain the property `{property}`.");
            }

            string valueWithPossibleComment = line.Split('=')[1];
            if (valueWithPossibleComment.Contains("#"))
            {
                return valueWithPossibleComment.Split('#')[0].Trim();
            }

            return valueWithPossibleComment.Trim();
        }

        private static bool LineStartsWith(string line, string substring)
        {
            string key = line.Split('=')[0].Trim();
            
            return key.EqualsCaseInsensitive(substring);
        }

        private static HydroArea GetHydroAreaWithManyFixedWeirs()
        {
            var hydroArea = new HydroArea();

            for (var i = 0; i < 100000; i++)
            {
                var fixedWeir = new FixedWeir
                {
                    GroupName = "fixed_weirs.pliz",
                    Geometry = new LineString(new[]
                    {
                        new Coordinate(i, i + 1),
                        new Coordinate(i, i)
                    })
                };
                hydroArea.FixedWeirs.Add(fixedWeir);
            }

            return hydroArea;
        }

        private static IEnumerable<ModelFeatureCoordinateData<FixedWeir>> GetFixedWeirData(HydroArea hydroArea)
        {
            return hydroArea.FixedWeirs.Select(fw => new ModelFeatureCoordinateData<FixedWeir> {Feature = fw}).ToList();
        }
    }
}