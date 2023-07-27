using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Functions.Generic;
using DelftTools.Hydro.GroupableFeatures;
using DelftTools.Shell.Core.Workflow.DataItems;
using DelftTools.TestUtils;
using DelftTools.Utils.Collections.Generic;
using DelftTools.Utils.IO;
using DeltaShell.NGHS.IO;
using DeltaShell.NGHS.IO.DelftIniObjects;
using DeltaShell.Plugins.FMSuite.FlowFM.IO;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Files;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Files.Helpers;
using DeltaShell.Plugins.FMSuite.FlowFM.Model;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using DeltaShell.Plugins.FMSuite.FlowFM.Properties;
using DeltaShell.Plugins.FMSuite.FlowFM.Sediment;
using DeltaShell.Plugins.SharpMapGis.SpatialOperations;
using DHYDRO.Common.Logging;
using GeoAPI.Extensions.Coverages;
using log4net.Core;
using NetTopologySuite.Extensions.Coverages;
using NetTopologySuite.Extensions.Grids;
using NUnit.Framework;
using Rhino.Mocks;
using SharpMap;
using SharpMap.Data.Providers;
using SharpMap.SpatialOperations;
using SharpMapTestUtils;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.IO
{
    [TestFixture]
    [Category(TestCategory.DataAccess)]
    public class SedimentFileTest
    {
        private static readonly string[] iniPropertyNamesOverall =
        {
            "Bros",
            "Bounty"
        };

        private static readonly string[] iniPropertyNamesSedimentFraction =
        {
            "MilkyWay",
            "Snickers"
        };

        private static readonly string[] iniPropertyNamesUnknown =
        {
            "Toblerone",
            "Twix"
        };

        [Test]
        // The way the sediment reader it's developed forces a model to be created in order to import the .sed file properties
        public void GivenAnMduWithSedimentFileWithUnknownProperties_WhenReadingAndWriting_ThenTheCorrectPropertiesAreCreatedAndCorrectlyWrittenToTheFile()
        {
            #region Load

            // Given
            string mduFilePath = TestHelper.GetTestFilePath(@"sedmor\FlowFMCustomProperties\FlowFMCustomPropertiesSedMor.mdu");

            // When
            var importedModel = new WaterFlowFMModel();
            importedModel.ImportFromMdu(mduFilePath);

            // Then
            Assert.NotNull(importedModel, "Model was not imported.");
            WaterFlowFMModelDefinition modelDefinition = importedModel.ModelDefinition;
            ValidateAllUnknownProperties(modelDefinition);

            #endregion

            #region Save

            TestHelper.PerformActionInTemporaryDirectory(tempDir =>
            {
                // Given
                mduFilePath = Path.Combine(tempDir, "FlowFMWithCustomProperties.mdu");

                // When
                new MduFile().Write(mduFilePath, modelDefinition, importedModel.Area, importedModel.FixedWeirsProperties, sedimentModelData: importedModel);
                importedModel = new WaterFlowFMModel();
                importedModel.ImportFromMdu(mduFilePath);

                // Then
                Assert.NotNull(importedModel);
                modelDefinition = importedModel.ModelDefinition;
                ValidateAllUnknownProperties(modelDefinition);
            });

            #endregion
        }

        [Test]
        public void GivenASedimentFileWithUnknownProperties_WhenReading_ThenOnlyUnknownAndCorrectSedimentPropertiesAreAddedToModelDefinition()
        {
            // Given
            var model = new WaterFlowFMModel();
            IEventedList<WaterFlowFMProperty> properties = model.ModelDefinition.Properties;
            int originalNumberOfProperties = properties.Count;
            string sedFilePath = TestHelper.GetTestFilePath(@"sedmor\FlowFMCustomProperties\SedCustomProperties.sed");

            // When
            SedimentFile.LoadSediments(sedFilePath, model);

            // Then
            Assert.AreEqual(originalNumberOfProperties + 12, properties.Count,
                            "Unexpected number of properties in model definition: exactly and only 12 unknown properties should have been added to the original properties.");
            ValidateAllUnknownProperties(model.ModelDefinition);
        }

        [Test]
        public void SaveAndReadSedFileWithCustomProperties()
        {
            string sedFile = Path.GetTempFileName();
            try
            {
                /* Define new model */
                var fmModel = new WaterFlowFMModel();
                fmModel.ModelDefinition.UseMorphologySediment = true;

                /*  Definition of properties   */
                var intProp = new SedimentProperty<int>("MyIntProp", 0, 0, false, 0, false, "liter", "MyIntDescription", false);
                intProp.Value = 27;

                var boolProp = new SedimentProperty<bool>("MyBoolProp", false, false, false, false, false, "sec",
                                                          "MyBoolDescription", false);
                boolProp.Value = true;

                var doubleProp = new SedimentProperty<double>("MyDoubleProp", 0, 0, false, 0, false, "cc",
                                                              "MyDoubleDescription", false);
                doubleProp.Value = 11.2;

                var formulaProp = new SedimentProperty<int>("TraFrm", -1, -2, false, 18, false, string.Empty, "Integer selecting the transport formula", true);

                /* Set sediment and formula properties */
                var testSedimentType = new SedimentType();
                testSedimentType.Key = "MySedType";
                testSedimentType.Properties = new EventedList<ISedimentProperty>()
                {
                    intProp,
                    doubleProp,
                    boolProp,
                    formulaProp
                };

                var testFormulaType = new SedimentFormulaType();
                testFormulaType.Properties = new EventedList<ISedimentProperty>() {formulaProp};

                var overallProp = new SedimentProperty<double>("MyOverallProp", 0, 0, true, 0, false, "km", "MyOverallDescription", false);
                overallProp.Value = 80.1;
                fmModel.SedimentOverallProperties = new EventedList<ISedimentProperty>() {overallProp};

                /*Add the fraction to the model*/
                var fraction = new SedimentFraction();
                fraction.Name = "MySedimentName";
                fraction.CurrentSedimentType = testSedimentType;
                fraction.CurrentFormulaType = testFormulaType;

                fmModel.SedimentFractions = new EventedList<ISedimentFraction>() {fraction};
                WaterFlowFMModelDefinition modelDefinition = fmModel.ModelDefinition;

                /* Test */
                SedimentFile.Save(sedFile, modelDefinition, fmModel);
                string sedWritten = File.ReadAllText(sedFile);
                Assert.That(sedWritten, Does.Contain(SedimentFile.GeneralHeader));
                Assert.That(sedWritten, Does.Contain(SedimentFile.OverallHeader));
                Assert.That(sedWritten, Does.Contain(SedimentFile.Header));
                Assert.That(sedWritten, Does.Contain("MyIntProp"));
                Assert.That(sedWritten, Does.Contain("MyBoolProp"));
                Assert.That(sedWritten, Does.Contain("MyDoubleProp"));
                Assert.That(sedWritten, Does.Contain("MyOverallProp"));
                Assert.That(sedWritten, Does.Contain("TraFrm"));
                Assert.That(sedWritten, Does.Contain("MySedimentName"));
                Assert.That(sedWritten, Does.Contain("MySedType"));
            }
            finally
            {
                FileUtils.DeleteIfExists(sedFile);
            }
        }

        [Test]
        public void SaveAndLoadSedFileWithInValidFractionNameShouldThrowException()
        {
            string sedFile = Path.GetTempFileName();
            try
            {
                /* Define new model */
                var fmModel = new WaterFlowFMModel();
                fmModel.ModelDefinition.UseMorphologySediment = true;

                /*Add the fraction to the model*/
                var fraction = new SedimentFraction();
                fraction.Name = "MySedimen*tName";
                ISpatiallyVaryingSedimentProperty spatiallyVaryingProperty =
                    fraction.CurrentSedimentType.Properties.OfType<ISpatiallyVaryingSedimentProperty>().FirstOrDefault();
                Assert.IsNotNull(spatiallyVaryingProperty);
                Assert.AreEqual("SedConc", spatiallyVaryingProperty.Name);
                Assert.IsFalse(spatiallyVaryingProperty.IsVisible);
                Assert.IsFalse(spatiallyVaryingProperty.IsEnabled);
                Assert.IsNull(spatiallyVaryingProperty.SpatiallyVaryingName);
                fmModel.SedimentFractions = new EventedList<ISedimentFraction>();
                fmModel.SedimentFractions.Add(fraction);
                spatiallyVaryingProperty = fraction.CurrentSedimentType.Properties.OfType<ISpatiallyVaryingSedimentProperty>().FirstOrDefault();
                Assert.IsNotNull(spatiallyVaryingProperty);
                Assert.AreEqual("SedConc", spatiallyVaryingProperty.Name);
                Assert.IsTrue(spatiallyVaryingProperty.IsVisible);
                Assert.IsTrue(spatiallyVaryingProperty.IsEnabled); //sedconc is always disabled
                Assert.IsNotNull(spatiallyVaryingProperty.SpatiallyVaryingName);

                WaterFlowFMModelDefinition modelDefinition = fmModel.ModelDefinition;
                SedimentFile.Save(sedFile, modelDefinition, fmModel);

                var model = new WaterFlowFMModel();
                LogHelper.ConfigureLogging(Level.Error);
                try
                {
                    TestHelper.AssertLogMessageIsGenerated(() => { SedimentFile.LoadSediments(sedFile, model); }, string.Format(@"Could not read sediment file because : Value cannot be null.{0}Parameter name: Sediment name MySedimen*tName in sediment file {1} is invalid to deltashell", Environment.NewLine, sedFile));
                }
                finally
                {
                    LogHelper.ResetLogging();
                }
            }
            finally
            {
                FileUtils.DeleteIfExists(sedFile);
            }
        }

        [Test]
        public void SaveAndLoadSedFileWithModifiedPropertiesValues()
        {
            string sedFile = Path.GetTempFileName();
            try
            {
                /* Define new model */
                var fmModel = new WaterFlowFMModel();
                fmModel.ModelDefinition.UseMorphologySediment = true;

                /* Define test properties */
                var intProp = new SedimentProperty<int>("IopSus", 0, 0, false, 0, false, "liter", "myintdescription", false);
                intProp.Value = 27;

                var boolProp = new SedimentProperty<bool>("EpsPar", false, false, false, false, false, "sec",
                                                          "mybooldescription", false);
                boolProp.Value = true;

                var doubleProp = new SedimentProperty<double>("SedDia", 0, 0, false, 0, false, "cc", "mydoubledescription", false);
                doubleProp.Value = 11.2;

                var sedConcProp = new SpatiallyVaryingSedimentProperty<double>("SedConc", 0, 0, false, 0, false, "cc", "mydoubledescription", false, false);
                sedConcProp.Value = 33.4;

                /* Use this variable to check if regardless of being a SpatiallyVaryingSedimentProperty it will NOT be stored as
                 spatially varying property if we set the value to false. */
                var doubleValuePropertyProp = new SpatiallyVaryingSedimentProperty<double>("IniSedThick", 5, 0, false, 0, false, "cc", "mydoubledescription", false, false);
                doubleValuePropertyProp.Value = 13.0;

                var formulaProp = new SedimentProperty<int>("TraFrm", -1, -2, false, 18, false, string.Empty, "Integer selecting the transport formula", true);
                formulaProp.Value = -2;

                /* Set sediment and formula properties */
                var testSedimentType = new SedimentType();
                testSedimentType.Key = "sand";
                testSedimentType.Properties = new EventedList<ISedimentProperty>()
                {
                    intProp,
                    doubleProp,
                    boolProp,
                    sedConcProp,
                    formulaProp,
                    doubleValuePropertyProp
                };

                var overallProp = new SedimentProperty<double>("Cref", 0, 0, true, 0, false, "km", "myoveralldescription", false);
                overallProp.Value = 80.1;
                fmModel.SedimentOverallProperties = new EventedList<ISedimentProperty>() {overallProp};

                /*Add the fraction to the model*/
                var fraction = new SedimentFraction();
                fraction.Name = "MySedimentName";
                fraction.CurrentSedimentType = testSedimentType;
                fraction.CurrentFormulaType = fraction.SupportedFormulaTypes.FirstOrDefault(sft => sft.TraFrm == -2);

                fmModel.SedimentFractions = new EventedList<ISedimentFraction>() {fraction};
                WaterFlowFMModelDefinition modelDefinition = fmModel.ModelDefinition;

                /* Test */
                SedimentFile.Save(sedFile, modelDefinition, fmModel);
                var model = new WaterFlowFMModel();
                SedimentFile.LoadSediments(sedFile, model);

                var loadedOverallProp = model.SedimentOverallProperties.FirstOrDefault() as ISedimentProperty<double>;
                Assert.IsNotNull(loadedOverallProp);
                Assert.That(loadedOverallProp.Name, Does.Contain("Cref"));
                Assert.That(loadedOverallProp.Value, Is.EqualTo(80.1).Within(0.01));

                ISedimentFraction loadedSedimentFraction = model.SedimentFractions.FirstOrDefault();
                Assert.IsNotNull(loadedSedimentFraction);
                Assert.That(loadedSedimentFraction.Name, Does.Contain("MySedimentName"));
                Assert.That(loadedSedimentFraction.CurrentSedimentType.Key, Does.Contain("sand"));

                var loadedFormulaProp = loadedSedimentFraction.CurrentSedimentType.Properties.FirstOrDefault(st => st.Name == "TraFrm") as ISedimentProperty<int>;
                Assert.IsNotNull(loadedFormulaProp);
                Assert.That(loadedFormulaProp.Value, Is.EqualTo(-2));

                var loadedIntProp = loadedSedimentFraction.CurrentFormulaType.Properties.FirstOrDefault(st => st.Name == "IopSus") as ISedimentProperty<int>;
                Assert.IsNotNull(loadedIntProp);
                Assert.That(loadedIntProp.Value, Is.EqualTo(27));

                var loadedBoolProp = loadedSedimentFraction.CurrentFormulaType.Properties.FirstOrDefault(st => st.Name == "EpsPar") as ISedimentProperty<bool>;
                Assert.IsNotNull(loadedBoolProp);
                Assert.That(loadedBoolProp.Value, Is.EqualTo(true));

                var loadedDoubleProp = loadedSedimentFraction.CurrentSedimentType.Properties.FirstOrDefault(st => st.Name == "SedDia") as ISedimentProperty<double>;
                Assert.IsNotNull(loadedDoubleProp);
                Assert.That(loadedDoubleProp.Value, Is.EqualTo(11.2).Within(0.01));

                var loadedSedConcProp = loadedSedimentFraction.CurrentSedimentType.Properties.FirstOrDefault(st => st.Name == "SedConc") as ISpatiallyVaryingSedimentProperty<double>;
                Assert.IsNotNull(loadedSedConcProp);
                /* By definition SedConc will be (until future changes) SpatiallyVarying
                  regardles of its initial declaration. */
                Assert.IsTrue(loadedSedConcProp.IsSpatiallyVarying);
                Assert.That(loadedSedConcProp.Value, Is.EqualTo(0.0));

                var loadedSpatDoubleProp = loadedSedimentFraction.CurrentSedimentType.Properties.FirstOrDefault(st => st.Name == "IniSedThick") as ISpatiallyVaryingSedimentProperty<double>;
                Assert.IsNotNull(loadedSpatDoubleProp);
                Assert.IsFalse(loadedSpatDoubleProp.IsSpatiallyVarying);
                Assert.That(loadedSpatDoubleProp.Value, Is.EqualTo(13.0).Within(0.01));
            }
            finally
            {
                FileUtils.DeleteIfExists(sedFile);
            }
        }

        [Test]
        public void SaveSedFileWithSpatiallyVaryingProperties()
        {
            string sedFile = Path.GetTempFileName();
            string generatedXyzFile = Path.Combine(Path.GetDirectoryName(sedFile), "mysedimentName_SedConc." + XyzFile.Extension);

            try
            {
                /* Define new model */
                var fmModel = new WaterFlowFMModel();
                fmModel.ImportFromMdu(sedFile);

                fmModel.ModelDefinition.UseMorphologySediment = true;

                //Area 
                // Import dry points
                const string baseFolderPath = @"HydroAreaCollection/MduFileProjects";

                fmModel.Area.DryAreas.Add(new GroupableFeature2DPolygon() {GroupName = Path.Combine(Directory.GetCurrentDirectory(), baseFolderPath, @"MyDryAreas_dry.pol")});

                /* Define test properties */
                var doubleSpatProp = new SpatiallyVaryingSedimentProperty<double>("SedConc", 0, 0, false, 0, true, "cc", "mydoubledescription", true, false);
                doubleSpatProp.SpatiallyVaryingName = "mysedimentName_SedConc";
                doubleSpatProp.Value = 12.3;

                var doubleSpatProp2 = new SpatiallyVaryingSedimentProperty<double>("IniSedThick", 5, 0, false, 0, true, "Joule", "mydoubledescription", true, false);
                doubleSpatProp2.SpatiallyVaryingName = "mysedimentName_IniSedThick";
                doubleSpatProp2.Value = 80.1;

                /* Set sediment and formula properties */
                var testSedimentType = new SedimentType();
                testSedimentType.Key = "sand";
                testSedimentType.Properties = new EventedList<ISedimentProperty>()
                {
                    doubleSpatProp,
                    doubleSpatProp2
                };

                var overallProp = new SedimentProperty<double>("Cref", 0, 0, true, 0, false, "km", "myoveralldescription", false);
                overallProp.Value = 80.1;
                fmModel.SedimentOverallProperties = new EventedList<ISedimentProperty>() {overallProp};

                /*Add the fraction to the model*/
                var fraction = new SedimentFraction();
                fraction.Name = "mysedimentName";
                fraction.CurrentSedimentType = testSedimentType;

                /*  Test    */
                UnstructuredGrid grid = UnstructuredGridTestHelper.GenerateRegularGrid(2, 2, 2, 2);

                fmModel.Grid = grid;

                /*SedThick coverage*/
                var covSedThick = new UnstructuredGridCellCoverage(grid, false);
                covSedThick[0] = 0.1;
                covSedThick[1] = 3.2;
                covSedThick[2] = 5.4;
                covSedThick[3] = 7.6;

                covSedThick.Name = "mysedimentName_IniSedThick";

                covSedThick.Components[0].NoDataValue = -999;

                var dataSedThickItem = new DataItem();
                dataSedThickItem.Name = "mysedimentName_IniSedThick";
                dataSedThickItem.Value = covSedThick;
                dataSedThickItem.Role = DataItemRole.Input;

                /*SedCon coverage*/
                var covSedConc = new UnstructuredGridCellCoverage(grid, false);
                covSedConc[0] = 0.1;
                covSedConc[1] = 3.2;
                covSedConc[2] = 5.4;
                covSedConc[3] = 7.6;

                covSedConc.Name = "mysedimentName_SedConc";

                covSedConc.Components[0].NoDataValue = -999;

                var dataSedConcItem = new DataItem();
                dataSedConcItem.Name = "mysedimentName_SedConc";
                dataSedConcItem.Value = covSedConc;
                dataSedConcItem.Role = DataItemRole.Input;

                /*Add coverages to fraction and model.*/
                fmModel.DataItems = new EventedList<IDataItem>
                {
                    dataSedThickItem,
                    dataSedConcItem
                };
                fmModel.SedimentOverallProperties = new EventedList<ISedimentProperty>();
                fmModel.SedimentFractions = new EventedList<ISedimentFraction>();
                fmModel.SedimentFractions.Add(fraction);

                WaterFlowFMModelDefinition modelDefinition = fmModel.ModelDefinition;
                /* 
                 * SedimentFile.Save(sedFile, fmModel);
                 * Spatially varying operations no longer get saved through this method but through ExtForceFile.cs
                 */
                SedimentFile.Save(sedFile, modelDefinition, fmModel);

                string sedWritten = File.ReadAllText(sedFile);
                Assert.That(sedWritten, Does.Contain(SedimentFile.GeneralHeader));
                Assert.That(sedWritten, Is.Not.StringContaining("SedConc"));
                Assert.That(sedWritten, Does.Contain("#mysedimentName#"));
                Assert.That(sedWritten, Is.Not.StringContaining("#mysedimentName_SedConc#"));
                Assert.That(sedWritten, Is.Not.StringContaining("12.3"));

                Assert.That(sedWritten, Does.Contain("IniSedThick"));
                Assert.That(sedWritten, Does.Contain("#mysedimentName_IniSedThick.xyz#"));
                Assert.That(sedWritten, Is.Not.StringContaining("80.1"));

                Assert.IsFalse(File.Exists(generatedXyzFile));

                /* Check the ExtFile */
                // update model definition (called during export)
                var initialSpatialOps = new List<string>()
                {
                    doubleSpatProp.SpatiallyVaryingName,
                    doubleSpatProp2.SpatiallyVaryingName
                };
                fmModel.ModelDefinition.SelectSpatialOperations(fmModel.AllDataItems.ToList(), fmModel.TracerDefinitions, initialSpatialOps);
                var extFile = new ExtForceFile();
                extFile.Write(sedFile, fmModel.ModelDefinition, false, false);
                Assert.IsTrue(File.Exists(generatedXyzFile));

                List<IPointValue> xyzFileValues = XyzFile.Read(generatedXyzFile).ToList();
                Assert.That(xyzFileValues.ElementAt(0).X, Is.EqualTo(covSedThick.Coordinates.ElementAt(0).X));
                Assert.That(xyzFileValues.ElementAt(0).Y, Is.EqualTo(covSedThick.Coordinates.ElementAt(0).Y));
                Assert.That(xyzFileValues.ElementAt(0).Value, Is.EqualTo(covSedThick.GetValues<double>().ElementAt(0)));

                Assert.That(xyzFileValues.ElementAt(1).X, Is.EqualTo(covSedThick.Coordinates.ElementAt(1).X));
                Assert.That(xyzFileValues.ElementAt(1).Y, Is.EqualTo(covSedThick.Coordinates.ElementAt(1).Y));
                Assert.That(xyzFileValues.ElementAt(1).Value, Is.EqualTo(covSedThick.GetValues<double>().ElementAt(1)));

                Assert.That(xyzFileValues.ElementAt(2).X, Is.EqualTo(covSedThick.Coordinates.ElementAt(2).X));
                Assert.That(xyzFileValues.ElementAt(2).Y, Is.EqualTo(covSedThick.Coordinates.ElementAt(2).Y));
                Assert.That(xyzFileValues.ElementAt(2).Value, Is.EqualTo(covSedThick.GetValues<double>().ElementAt(2)));

                Assert.That(xyzFileValues.ElementAt(3).X, Is.EqualTo(covSedThick.Coordinates.ElementAt(3).X));
                Assert.That(xyzFileValues.ElementAt(3).Y, Is.EqualTo(covSedThick.Coordinates.ElementAt(3).Y));
                Assert.That(xyzFileValues.ElementAt(3).Value, Is.EqualTo(covSedThick.GetValues<double>().ElementAt(3)));
            }
            finally
            {
                FileUtils.DeleteIfExists(sedFile);
                FileUtils.DeleteIfExists(generatedXyzFile);
            }
        }

        [Test]
        [Category(TestCategory.Slow)]
        public void SaveSedFileWithSpatiallyVaryingPropertiesAndNoOperationsGeneratesWarningMessages()
        {
            string sedFile = Path.GetTempFileName();
            string generatedXyzFile = Path.Combine(Path.GetDirectoryName(sedFile), "mysedimentName_SedConc." + XyzFile.Extension);

            try
            {
                /* Define new model */
                var fmModel = new WaterFlowFMModel();
                fmModel.ImportFromMdu(sedFile);

                fmModel.ModelDefinition.UseMorphologySediment = true;
                UnstructuredGrid grid = UnstructuredGridTestHelper.GenerateRegularGrid(2, 2, 2, 2);
                fmModel.Grid = grid;

                var fraction = new SedimentFraction() {Name = "Frac1"};
                fmModel.SedimentFractions.Add(fraction);

                WaterFlowFMModelDefinition modelDefinition = fmModel.ModelDefinition;
                // Save SedFile with no spatially varying properties. No warnings should be given.
                TestHelper.AssertLogMessagesCount(() => SedimentFile.Save(sedFile, modelDefinition, fmModel), 0);

                // Add a spatially varying prop 
                ISpatiallyVaryingSedimentProperty randomSVProp = fraction.CurrentSedimentType.Properties.OfType<ISpatiallyVaryingSedimentProperty>().FirstOrDefault(p => p.Name != "SedConc");
                Assert.NotNull(randomSVProp);
                randomSVProp.IsSpatiallyVarying = true;
                // Warning should be given.
                TestHelper.AssertAtLeastOneLogMessagesContains(
                    () => SedimentFile.Save(sedFile, fmModel.ModelDefinition, fmModel),
                    string.Format(Resources.SedimentFile_WriteSpatiallyVaryingSedimentPropertySubFiles_No_spatial_operations_of_type_Import__Add_or_Value_found_for_spatially_varying_property__0___Remember_to_interpolate_them_to_generate_the_xyz_file__Otherwise_the_model_might_not_run_as_expected_, randomSVProp.SpatiallyVaryingName));

                //Add a 'value' operation, another warning should be given.
                IDataItem dataItem = fmModel.AllDataItems.FirstOrDefault(di => di.Name == randomSVProp.SpatiallyVaryingName);

                // retrieve / create value converter for mysedimentName_SedConc dataitem
                SpatialOperationSetValueConverter valueConverter = SpatialOperationValueConverterFactory.GetOrCreateSpatialOperationValueConverter(dataItem, randomSVProp.SpatiallyVaryingName);
                var samples = new SetValueOperation();
                samples.SetInputData(SpatialOperation.MainInputName, new PointCloudFeatureProvider
                {
                    PointCloud = new PointCloud
                    {
                        PointValues = new List<IPointValue>
                        {
                            new PointValue
                            {
                                X = fmModel.Grid.Cells[0].CenterX,
                                Y = fmModel.Grid.Cells[0].CenterY,
                                Value = 12
                            },
                            new PointValue
                            {
                                X = fmModel.Grid.Cells[1].CenterX,
                                Y = fmModel.Grid.Cells[1].CenterY,
                                Value = 30
                            },
                            new PointValue
                            {
                                X = fmModel.Grid.Cells[2].CenterX,
                                Y = fmModel.Grid.Cells[2].CenterY,
                                Value = 31
                            }
                        }
                    }
                });
                valueConverter.SpatialOperationSet.AddOperation(samples);
                valueConverter.SpatialOperationSet.Execute();

                // update model definition (called during export)
                var initialSpatialOps = new List<string>() {randomSVProp.SpatiallyVaryingName};
                fmModel.ModelDefinition.SelectSpatialOperations(fmModel.AllDataItems.ToList(), fmModel.TracerDefinitions, initialSpatialOps);

                // New Warning should be given.

                TestHelper.AssertAtLeastOneLogMessagesContains(
                    () => SedimentFile.Save(sedFile, modelDefinition, fmModel),
                    string.Format(Resources.SedimentFile_WriteSpatiallyVaryingSedimentPropertySubFiles_Cannot_create_xyz_file_for_spatial_varying_initial_condition__0__because_it_is_a_value_spatial_operation__please_interpolate_the_operation_to_the_grid_or, randomSVProp.SpatiallyVaryingName));
            }
            finally
            {
                FileUtils.DeleteIfExists(sedFile);
                FileUtils.DeleteIfExists(generatedXyzFile);
            }
        }

        [Test]
        [Category(TestCategory.Slow)]
        public void SaveSedFileWithSpatiallyVaryingPropertiesAndAddValuesOperation()
        {
            string sedFile = Path.GetTempFileName();
            string generatedXyzFile = Path.Combine(Path.GetDirectoryName(sedFile), "mysedimentName_SedConc." + XyzFile.Extension);
            var fileCopyName = "";
            try
            {
                /* Define new model */
                var fmModel = new WaterFlowFMModel();
                fmModel.ImportFromMdu(sedFile);

                fmModel.ModelDefinition.UseMorphologySediment = true;
                UnstructuredGrid grid = UnstructuredGridTestHelper.GenerateRegularGrid(2, 2, 2, 2);
                fmModel.Grid = grid;

                /* Define test properties */
                var doubleSpatProp = new SpatiallyVaryingSedimentProperty<double>("SedConc", 0, 0, false, 0, true, "cc", "mydoubledescription", true, false)
                {
                    SpatiallyVaryingName = "mysedimentName_SedConc",
                    Value = 12.3
                };

                /* Set sediment and formula properties */
                var testSedimentType = new SedimentType
                {
                    Key = "sand",
                    Properties = new EventedList<ISedimentProperty> {doubleSpatProp}
                };

                var overallProp = new SedimentProperty<double>("Cref", 0, 0, true, 0, false, "km", "myoveralldescription", false) {Value = 80.1};

                fmModel.SedimentOverallProperties = new EventedList<ISedimentProperty>() {overallProp};

                /*Add the fraction to the model*/
                var fraction = new SedimentFraction
                {
                    Name = "mysedimentName",
                    CurrentSedimentType = testSedimentType
                };

                fileCopyName = TestHelper.CreateLocalCopy(TestHelper.GetTestFilePath(@"harlingen_model_3d\har_V3.xyz"));

                fmModel.SedimentOverallProperties = new EventedList<ISedimentProperty>();
                fmModel.SedimentFractions = new EventedList<ISedimentFraction>();
                fmModel.SedimentFractions.Add(fraction);

                IDataItem dataItem = fmModel.AllDataItems.FirstOrDefault(di => di.Name == "mysedimentName_SedConc");

                // retrieve / create value converter for mysedimentName_SedConc dataitem
                SpatialOperationSetValueConverter valueConverter = SpatialOperationValueConverterFactory.GetOrCreateSpatialOperationValueConverter(dataItem, "mysedimentName_SedConc");
                var samples = new AddSamplesOperation(false);
                samples.SetInputData(AddSamplesOperation.SamplesInputName, new PointCloudFeatureProvider
                {
                    PointCloud = new PointCloud
                    {
                        PointValues = new List<IPointValue>
                        {
                            new PointValue
                            {
                                X = fmModel.Grid.Cells[0].CenterX,
                                Y = fmModel.Grid.Cells[0].CenterY,
                                Value = 12
                            },
                            new PointValue
                            {
                                X = fmModel.Grid.Cells[1].CenterX,
                                Y = fmModel.Grid.Cells[1].CenterY,
                                Value = 30
                            },
                            new PointValue
                            {
                                X = fmModel.Grid.Cells[2].CenterX,
                                Y = fmModel.Grid.Cells[2].CenterY,
                                Value = 31
                            }
                        }
                    }
                });
                valueConverter.SpatialOperationSet.AddOperation(samples);
                valueConverter.SpatialOperationSet.Execute();

                // update model definition (called during export)
                var initialSpatialOps = new List<string>() {doubleSpatProp.SpatiallyVaryingName};
                fmModel.ModelDefinition.SelectSpatialOperations(fmModel.DataItems, fmModel.TracerDefinitions, initialSpatialOps);
                // create an interpolate operation using the samples added earlier
                var interpolateOperation = new InterpolateOperation();
                interpolateOperation.SetInputData(InterpolateOperation.InputSamplesName, samples.Output.Provider);
                Assert.IsNotNull(valueConverter.SpatialOperationSet.AddOperation(interpolateOperation));

                // update model definition (called during export)
                fmModel.ModelDefinition.SelectSpatialOperations(fmModel.AllDataItems.ToList(), fmModel.TracerDefinitions, initialSpatialOps);

                WaterFlowFMModelDefinition modelDefinition = fmModel.ModelDefinition;
                SedimentFile.Save(sedFile, modelDefinition, fmModel);

                string sedWritten = File.ReadAllText(sedFile);
                Assert.That(sedWritten, Does.Contain(SedimentFile.GeneralHeader));
                Assert.That(sedWritten, Is.Not.StringContaining("SedConc"));
                Assert.That(sedWritten, Does.Contain("#mysedimentName#"));
                Assert.That(sedWritten, Is.Not.StringContaining("#mysedimentName_SedConc#"));
                Assert.That(sedWritten, Is.Not.StringContaining("12.3"));

                /* 
                 * SedimentFile.Save(sedFile, fmModel);
                 * Spatially varying operations no longer get saved through this method but through ExtForceFile.cs
                 */
                Assert.IsFalse(File.Exists(generatedXyzFile));
                var extFile = new ExtForceFile();
                extFile.Write(sedFile, fmModel.ModelDefinition, false, false);
                Assert.IsTrue(File.Exists(generatedXyzFile));
            }
            finally
            {
                FileUtils.DeleteIfExists(sedFile);
                FileUtils.DeleteIfExists(fileCopyName);
                FileUtils.DeleteIfExists(generatedXyzFile);
            }
        }

        [Test]
        [Category(TestCategory.Slow)]
        public void LoadSedFileWithSpatiallyVaryingProperties_MudFraction()
        {
            string mduPath = TestHelper.GetTestFilePath(@"SpatiallyVarying_MudFraction\FlowFM.mdu");
            string localCopy = TestHelper.CreateLocalCopy(mduPath);

            using (var model = new WaterFlowFMModel())
            {
                model.ImportFromMdu(localCopy);

                ISedimentFraction fraction = model.SedimentFractions.FirstOrDefault(sf => sf.Name == "mudFraction");
                Assert.IsNotNull(fraction);
                var spatvaryingProp = fraction.CurrentSedimentType.Properties.FirstOrDefault(p => p.Name == "IniSedThick") as ISpatiallyVaryingSedimentProperty;
                Assert.IsNotNull(spatvaryingProp);
                Assert.IsTrue(spatvaryingProp.IsSpatiallyVarying);
                IDataItem dataItem = model.AllDataItems.FirstOrDefault(di => di.Name == "mudFraction_IniSedThick");
                Assert.IsNotNull(dataItem);
                var coverage = dataItem.Value as UnstructuredGridCellCoverage;
                Assert.IsNotNull(coverage);
                IMultiDimensionalArray<double> values = coverage.GetValues<double>();

                Assert.That(values.Count == 9);
                Assert.That(values[0], Is.EqualTo(-999.0).Within(0.1));
                Assert.That(values[1], Is.EqualTo(-999.0).Within(0.1));
                Assert.That(values[2], Is.EqualTo(-999.0).Within(0.1));
                Assert.That(values[3], Is.EqualTo(10.0).Within(0.1));
                Assert.That(values[4], Is.EqualTo(10.0).Within(0.1));
                Assert.That(values[5], Is.EqualTo(-999.0).Within(0.1));
                Assert.That(values[6], Is.EqualTo(10.0).Within(0.1));
                Assert.That(values[7], Is.EqualTo(10.0).Within(0.1));
                Assert.That(values[8], Is.EqualTo(-999.0).Within(0.1));

                spatvaryingProp = fraction.CurrentFormulaType.Properties.FirstOrDefault(p => p.Name == "TcrSed") as ISpatiallyVaryingSedimentProperty;
                Assert.IsNotNull(spatvaryingProp);
                Assert.IsTrue(spatvaryingProp.IsSpatiallyVarying);
                dataItem = model.AllDataItems.FirstOrDefault(di => di.Name == "mudFraction_TcrSed");
                Assert.IsNotNull(dataItem);
                coverage = dataItem.Value as UnstructuredGridCellCoverage;
                Assert.IsNotNull(coverage);
                values = coverage.GetValues<double>();

                Assert.That(values.Count == 9);
                Assert.That(values[0], Is.EqualTo(-999.0).Within(0.1));
                Assert.That(values[1], Is.EqualTo(-999.0).Within(0.1));
                Assert.That(values[2], Is.EqualTo(-999.0).Within(0.1));
                Assert.That(values[3], Is.EqualTo(6.0).Within(0.1));
                Assert.That(values[4], Is.EqualTo(6.0).Within(0.1));
                Assert.That(values[5], Is.EqualTo(-999.0).Within(0.1));
                Assert.That(values[6], Is.EqualTo(6.0).Within(0.1));
                Assert.That(values[7], Is.EqualTo(6.0).Within(0.1));
                Assert.That(values[8], Is.EqualTo(-999.0).Within(0.1));

                spatvaryingProp = fraction.CurrentFormulaType.Properties.FirstOrDefault(p => p.Name == "TcrEro") as ISpatiallyVaryingSedimentProperty;
                Assert.IsNotNull(spatvaryingProp);
                Assert.IsTrue(spatvaryingProp.IsSpatiallyVarying);
                dataItem = model.AllDataItems.FirstOrDefault(di => di.Name == "mudFraction_TcrEro");
                Assert.IsNotNull(dataItem);
                coverage = dataItem.Value as UnstructuredGridCellCoverage;
                Assert.IsNotNull(coverage);
                values = coverage.GetValues<double>();

                Assert.That(values.Count == 9);
                Assert.That(values[0], Is.EqualTo(-999.0).Within(0.1));
                Assert.That(values[1], Is.EqualTo(-999.0).Within(0.1));
                Assert.That(values[2], Is.EqualTo(-999.0).Within(0.1));
                Assert.That(values[3], Is.EqualTo(8.0).Within(0.1));
                Assert.That(values[4], Is.EqualTo(8.0).Within(0.1));
                Assert.That(values[5], Is.EqualTo(-999.0).Within(0.1));
                Assert.That(values[6], Is.EqualTo(8.0).Within(0.1));
                Assert.That(values[7], Is.EqualTo(8.0).Within(0.1));
                Assert.That(values[8], Is.EqualTo(-999.0).Within(0.1));
            }
        }

        [Test]
        [Category(TestCategory.Slow)]
        public void LoadSedFileWithSpatiallyVaryingProperties()
        {
            string mduPath = TestHelper.GetTestFilePath(@"spatially_varying_sediment_properties_in_model\FlowFM.mdu");
            string localCopy = TestHelper.CreateLocalCopy(mduPath);

            using (var model = new WaterFlowFMModel())
            {
                model.ImportFromMdu(localCopy);

                ISedimentFraction fraction = model.SedimentFractions.FirstOrDefault(sf => sf.Name == "gouwe");
                Assert.IsNotNull(fraction);
                var spatvaryingProp = fraction.CurrentSedimentType.Properties.FirstOrDefault(p => p.Name == "IniSedThick") as
                                          ISpatiallyVaryingSedimentProperty;
                Assert.IsNotNull(spatvaryingProp);
                Assert.IsTrue(spatvaryingProp.IsSpatiallyVarying);
                IDataItem dataItem = model.AllDataItems.FirstOrDefault(di => di.Name == "gouwe_IniSedThick");
                Assert.IsNotNull(dataItem);
                var coverage = dataItem.Value as UnstructuredGridCellCoverage;
                Assert.IsNotNull(coverage);
                IMultiDimensionalArray<double> values = coverage.GetValues<double>();

                Assert.That(values.Count == 9);
                Assert.That(values[0], Is.EqualTo(-999.0).Within(0.1));
                Assert.That(values[1], Is.EqualTo(-999.0).Within(0.1));
                Assert.That(values[2], Is.EqualTo(-999.0).Within(0.1));
                Assert.That(values[3], Is.EqualTo(10.0).Within(0.1));
                Assert.That(values[4], Is.EqualTo(10.0).Within(0.1));
                Assert.That(values[5], Is.EqualTo(-999.0).Within(0.1));
                Assert.That(values[6], Is.EqualTo(10.0).Within(0.1));
                Assert.That(values[7], Is.EqualTo(10.0).Within(0.1));
                Assert.That(values[8], Is.EqualTo(-999.0).Within(0.1));
            }
        }

        [Test]
        public void SaveSedFileWithSpatiallyVaryingPropertiesAndImportOperation()
        {
            string sedFile = Path.GetTempFileName();
            string generatedXyzFile = Path.Combine(Path.GetDirectoryName(sedFile),
                                                   "mysedimentName_IniSedThick." + XyzFile.Extension);

            try
            {
                /* Define new model */
                var fmModel = new WaterFlowFMModel();
                fmModel.ImportFromMdu(sedFile);

                fmModel.ModelDefinition.UseMorphologySediment = true;

                /* Define test properties */
                var doubleSpatProp = new SpatiallyVaryingSedimentProperty<double>("IniSed", 0, 0, false, 0, true, "cc",
                                                                                  "mydoubledescription", true, false);
                doubleSpatProp.SpatiallyVaryingName = "mysedimentName_IniSedThick";
                doubleSpatProp.Value = 12.3;

                /* Set sediment and formula properties */
                var testSedimentType = new SedimentType();
                testSedimentType.Key = "sand";
                testSedimentType.Properties = new EventedList<ISedimentProperty>() {doubleSpatProp};

                var overallProp =
                    new SedimentProperty<double>("Cref", 0, 0, true, 0, false, "km", "myoveralldescription", false);
                overallProp.Value = 80.1;
                fmModel.SedimentOverallProperties = new EventedList<ISedimentProperty>() {overallProp};

                /*Add the fraction to the model*/
                var fraction = new SedimentFraction();
                fraction.Name = "mysedimentName";
                fraction.CurrentSedimentType = testSedimentType;

                string fileName = TestHelper.GetTestFilePath(@"harlingen_model_3d\har_V3.xyz");
                fileName = TestHelper.CreateLocalCopy(fileName);

                fmModel.SedimentOverallProperties = new EventedList<ISedimentProperty>();
                fmModel.SedimentFractions = new EventedList<ISedimentFraction>();
                fmModel.SedimentFractions.Add(fraction);

                IDataItem dataItem = fmModel.AllDataItems.FirstOrDefault(di => di.Name == "mysedimentName_IniSedThick");

                // retrieve / create value converter for mysedimentName_SedConc dataitem
                SpatialOperationSetValueConverter valueConverter =
                    SpatialOperationValueConverterFactory.GetOrCreateSpatialOperationValueConverter(dataItem,
                                                                                                    "mysedimentName_IniSedThick");

                valueConverter.SpatialOperationSet.AddOperation(new ImportSamplesSpatialOperation()
                {
                    Name = "mysedimentName_IniSedThick",
                    FilePath = Path.GetFullPath(fileName)
                });
                valueConverter.SpatialOperationSet.Execute();
                WaterFlowFMModelDefinition modelDefinition = fmModel.ModelDefinition;
                SedimentFile.Save(sedFile, modelDefinition, fmModel);
                string sedWritten = File.ReadAllText(sedFile);
                Assert.That(sedWritten, Does.Contain(SedimentFile.GeneralHeader));
                Assert.That(sedWritten, Does.Contain("#mysedimentName#"));

                /* Sed conc is in ExtForceFile */
                Assert.That(sedWritten, Is.Not.StringContaining("SedConc"));
                Assert.That(sedWritten, Is.Not.StringContaining("#mysedimentName_SedConc#"));

                /* Custom property */
                Assert.That(sedWritten, Does.Contain("IniSed"));
                Assert.That(sedWritten, Does.Contain("#mysedimentName_IniSedThick.xyz#"));

                Assert.That(sedWritten, Is.Not.StringContaining("12.3"));

                Assert.IsTrue(File.Exists(generatedXyzFile));
            }
            finally
            {
                FileUtils.DeleteIfExists(sedFile);
                FileUtils.DeleteIfExists(generatedXyzFile);
            }
        }

        [Test]
        [Category(TestCategory.Slow)]
        public void CloneLoadedSedFileWithSpatiallyVaryingProperties()
        {
            string mduPath =
                TestHelper.GetTestFilePath(@"spatially_varying_sediment_properties_in_model\FlowFM.mdu");
            string localCopy = TestHelper.CreateLocalCopy(mduPath);

            using (var orgModel = new WaterFlowFMModel())
            {
                orgModel.ImportFromMdu(localCopy);

                CheckModelCoverageValues(orgModel);
                using (var model = (WaterFlowFMModel) orgModel.DeepClone())
                {
                    CheckModelCoverageValues(model);
                }
            }
        }

        /// <summary>
        /// GIVEN a SedFile without unknown features
        /// AND an FM Model
        /// AND a logHandler
        /// WHEN LoadSediments is called with these parameters
        /// THEN no warning messages are logged
        /// </summary>
        [Test]
        public void GivenASedFileWithoutUnknownFeaturesAndAnFMModelAndALogHandler_WhenLoadSedimentsIsCalledWithTheseParameters_ThenNoWarningMessagesAreLogged()
        {
            using (var tempDir = new TemporaryDirectory())
            using (var model = new WaterFlowFMModel()) // :(
            {
                // Given
                string sedPath = Path.Combine(tempDir.Path, "sedfile.sed");

                CreateSedFile(sedPath, 0, 0, 0);

                var logHandlerMock = MockRepository.GenerateStrictMock<ILogHandler>();
                logHandlerMock.Expect(lh => lh.LogReport()).Repeat.Any();
                logHandlerMock.Expect(lh => lh.ReportError(null)).IgnoreArguments().Repeat.Never();
                logHandlerMock.Expect(lh => lh.ReportInfo(null)).IgnoreArguments().Repeat.Never();
                logHandlerMock.Expect(lh => lh.ReportWarning(null)).IgnoreArguments().Repeat.Never();
                logHandlerMock.Expect(lh => lh.ReportErrorFormat(null)).IgnoreArguments().Repeat.Never();
                logHandlerMock.Expect(lh => lh.ReportInfoFormat(null)).IgnoreArguments().Repeat.Never();
                logHandlerMock.Expect(lh => lh.ReportWarningFormat(null)).IgnoreArguments().Repeat.Never();

                logHandlerMock.Replay();
                // When
                SedimentFile.LoadSediments(sedPath, model, logHandlerMock);

                // Then
                logHandlerMock.VerifyAllExpectations();
            }
        }

        private static void ValidateAllUnknownProperties(WaterFlowFMModelDefinition modelDefinition)
        {
            IEventedList<WaterFlowFMProperty> properties = modelDefinition.Properties;

            const string sedimentFraction1Name = "sed1";
            List<WaterFlowFMProperty> unknownPropertiesForSed1 = properties.Where(p => p.PropertyDefinition.Category.Equals(sedimentFraction1Name)).ToList();
            ValidatePropertiesCategory(unknownPropertiesForSed1, SedimentFile.Header, sedimentFraction1Name);

            const string sedimentFraction2Name = "sed2";
            List<WaterFlowFMProperty> unknownPropertiesForSed2 = properties.Where(p => p.PropertyDefinition.Category.Equals(sedimentFraction2Name)).ToList();
            ValidatePropertiesCategory(unknownPropertiesForSed2, SedimentFile.Header, sedimentFraction2Name);

            const string customCategoryName = "MyCustomCategory";
            List<WaterFlowFMProperty> propertiesUnknownCategory = properties.Where(p => p.PropertyDefinition.FileCategoryName == customCategoryName).ToList();
            ValidatePropertiesCategory(propertiesUnknownCategory, customCategoryName, customCategoryName);
        }

        private static void ValidatePropertiesCategory(List<WaterFlowFMProperty> properties, string fileCategoryName, string categoryName)
        {
            Assert.IsTrue(properties.All(p => p.PropertyDefinition.UnknownPropertySource.Equals(PropertySource.SedimentFile)));
            Assert.IsTrue(properties.All(p => p.PropertyDefinition.FileCategoryName.Equals(fileCategoryName)));
            Assert.IsTrue(properties.All(p => p.PropertyDefinition.Category.Equals(categoryName)));

            ValidateProperty(properties, "MyCustomStringProp", "\"777\"");
            ValidateProperty(properties, "MyCustomBoolProp", "1");
            ValidateProperty(properties, "MyCustomDoubleProp", "7.77");
            ValidateProperty(properties, "MyCustomIntProp", "777");
        }

        private static void ValidateProperty(IEnumerable<WaterFlowFMProperty> properties, string propertyName, string propertyValue)
        {
            WaterFlowFMProperty customStringProperty = properties.FirstOrDefault(p => p.PropertyDefinition.FilePropertyName.Equals(propertyName));
            Assert.NotNull(customStringProperty);
            Assert.AreEqual(propertyValue, customStringProperty.Value);
        }

        private static void CheckModelCoverageValues(WaterFlowFMModel model)
        {
            ISedimentFraction fraction = model.SedimentFractions.FirstOrDefault(sf => sf.Name == "gouwe");
            Assert.IsNotNull(fraction);
            var spatvaryingProp =
                fraction.CurrentSedimentType.Properties.FirstOrDefault(p => p.Name == "IniSedThick") as
                    ISpatiallyVaryingSedimentProperty;
            Assert.IsNotNull(spatvaryingProp);
            Assert.IsTrue(spatvaryingProp.IsSpatiallyVarying);
            IDataItem dataItem = model.AllDataItems.FirstOrDefault(di => di.Name == "gouwe_IniSedThick");
            Assert.IsNotNull(dataItem);
            var coverage = dataItem.Value as UnstructuredGridCellCoverage;
            Assert.IsNotNull(coverage);
            IMultiDimensionalArray<double> values = coverage.GetValues<double>();

            Assert.That(values.Count == 9);
            Assert.That(values[0], Is.EqualTo(-999.0).Within(0.1));
            Assert.That(values[1], Is.EqualTo(-999.0).Within(0.1));
            Assert.That(values[2], Is.EqualTo(-999.0).Within(0.1));
            Assert.That(values[3], Is.EqualTo(10.0).Within(0.1));
            Assert.That(values[4], Is.EqualTo(10.0).Within(0.1));
            Assert.That(values[5], Is.EqualTo(-999.0).Within(0.1));
            Assert.That(values[6], Is.EqualTo(10.0).Within(0.1));
            Assert.That(values[7], Is.EqualTo(10.0).Within(0.1));
            Assert.That(values[8], Is.EqualTo(-999.0).Within(0.1));
        }

        /// <summary>
        /// GIVEN a SedFile with unknown features
        /// AND an FM Model
        /// AND a logHandler
        /// WHEN LoadSediments is called with these parameters
        /// THEN the correct warning message is logged
        /// </summary>
        [TestCase(0, 0, 1)]
        [TestCase(0, 0, 2)]
        [TestCase(0, 1, 0)]
        [TestCase(0, 1, 1)]
        [TestCase(0, 1, 2)]
        [TestCase(0, 2, 0)]
        [TestCase(0, 2, 1)]
        [TestCase(0, 2, 2)]
        [TestCase(1, 0, 0)]
        [TestCase(1, 0, 1)]
        [TestCase(1, 0, 2)]
        [TestCase(1, 1, 0)]
        [TestCase(1, 1, 1)]
        [TestCase(1, 1, 2)]
        [TestCase(1, 2, 0)]
        [TestCase(1, 2, 1)]
        [TestCase(1, 2, 2)]
        [TestCase(2, 0, 0)]
        [TestCase(2, 0, 1)]
        [TestCase(2, 0, 2)]
        [TestCase(2, 1, 0)]
        [TestCase(2, 1, 1)]
        [TestCase(2, 1, 2)]
        [TestCase(2, 2, 0)]
        [TestCase(2, 2, 1)]
        [TestCase(2, 2, 2)]
        public void GivenASedFileWithUnknownFeaturesAndAnFMModelAndALogHandler_WhenLoadSedimentsIsCalledWithTheseParameters_ThenTheCorrectWarningMessageIsLogged(int nUnknownOverall, int nUnknownSediment, int nUnknownUnknown)
        {
            using (var tempDir = new TemporaryDirectory())
            using (var model = new WaterFlowFMModel())
            {
                // Given
                string sedPath = Path.Combine(tempDir.Path, "sedfile.sed");

                CreateSedFile(sedPath, nUnknownOverall, nUnknownSediment, nUnknownUnknown);

                var logHandlerMock = MockRepository.GenerateStrictMock<ILogHandler>();
                logHandlerMock.Expect(lh => lh.LogReport()).Repeat.Once();

                setUpExpectationsReportWarning(logHandlerMock, nUnknownOverall, iniPropertyNamesOverall);
                setUpExpectationsReportWarning(logHandlerMock, nUnknownSediment, iniPropertyNamesSedimentFraction);
                setUpExpectationsReportWarning(logHandlerMock, nUnknownUnknown, iniPropertyNamesUnknown);

                logHandlerMock.Replay();
                // When
                SedimentFile.LoadSediments(sedPath, model, logHandlerMock);

                // Then
                logHandlerMock.VerifyAllExpectations();
            }
        }

        private static void setUpExpectationsReportWarning(ILogHandler mock, int nUnknown, IReadOnlyList<string> names)
        {
            for (var i = 0; i < nUnknown; i++)
            {
                string propName = names[i];

                mock.Expect(lh => lh.ReportWarningFormat(
                                Arg<string>.Matches(m => m.Equals(Resources.MorphologySediment_ReadCategoryProperties_Unsupported_keyword___0___at_line___1___detected_and_will_be_passed_to_the_computational_core__Note_that_some_data_or_the_connection_to_linked_files_may_be_lost_)),
                                Arg<object[]>.Matches(o => o.Length == 2 && (o[0] as string).Equals(propName) && o[1] is int)))
                    .Repeat.Once();
            }
        }

        private static void CreateSedFile(string path,
                                          int nUnknownOverall,
                                          int nUnknownSediment,
                                          int nUnknownUnknown)
        {
            var iniCategories = new List<DelftIniCategory>();

            // General Category
            var sedimentFileInformationCategory = new DelftIniCategory(SedimentFile.GeneralHeader);
            sedimentFileInformationCategory.AddProperty(SedimentFile.FileCreatedBy, "This sexy test helper.");
            sedimentFileInformationCategory.AddProperty(SedimentFile.FileCreationDate, "Wed Jan 24 1852, 10:58:04", "Gee that is old");
            sedimentFileInformationCategory.AddProperty(SedimentFile.FileVersion, "02.00");

            iniCategories.Add(sedimentFileInformationCategory);

            // Overall Category
            var sedimentOverall = new DelftIniCategory(SedimentFile.OverallHeader);
            sedimentOverall.AddSedimentProperty("Cref", "1600", "kg/m³", "Reference density for hindered settling calculations");

            for (var i = 0; i < nUnknownOverall; i++)
            {
                sedimentOverall.AddProperty(iniPropertyNamesOverall[i], "Lekker", "#Toch?");
            }

            iniCategories.Add(sedimentOverall);

            // Sediment Category

            // Sediment Category
            var sedimentFracCat = new DelftIniCategory(SedimentFile.Header);

            ISedimentType sedType = SedimentFractionHelper.GetSedimentationTypes().First();

            sedimentFracCat.AddSedimentProperty("Name", sedType.Name, "", "Name of sediment fraction");
            sedimentFracCat.AddSedimentProperty("SedTyp", sedType.Key, "", "Must be \"sand\", \"mud\" or \"bedload\"");

            foreach (ISedimentProperty prop in sedType.Properties)
            {
                prop.SedimentPropertyWrite(sedimentFracCat);
            }

            for (var i = 0; i < nUnknownSediment; i++)
            {
                sedimentFracCat.AddProperty(iniPropertyNamesSedimentFraction[i], "Lekker", "#Toch");
            }

            iniCategories.Add(sedimentFracCat);

            // Optional unknown category
            if (nUnknownUnknown > 0)
            {
                var unknownCategory = new DelftIniCategory("I_am_an_unknown_category");

                for (var i = 0; i < nUnknownUnknown; i++)
                {
                    unknownCategory.AddProperty(iniPropertyNamesUnknown[i], "Lekker", "Toch");
                }

                iniCategories.Add(unknownCategory);
            }

            new DelftIniWriter().WriteDelftIniFile(iniCategories, path, false);
        }
    }
}