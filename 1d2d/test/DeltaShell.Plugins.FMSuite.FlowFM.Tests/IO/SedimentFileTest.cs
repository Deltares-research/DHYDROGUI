using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Shell.Core.Workflow.DataItems;
using DelftTools.TestUtils;
using DelftTools.Utils.Collections.Generic;
using DelftTools.Utils.IO;
using DeltaShell.Plugins.FMSuite.FlowFM.IO;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using DeltaShell.Plugins.FMSuite.FlowFM.Properties;
using DeltaShell.Plugins.SharpMapGis.SpatialOperations;
using GeoAPI.Extensions.Coverages;
using log4net.Core;
using NetTopologySuite.Extensions.Coverages;
using NUnit.Framework;
using SharpMap;
using SharpMap.Data.Providers;
using SharpMap.Extensions.CoordinateSystems;
using SharpMap.SpatialOperations;
using SharpMapTestUtils;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.IO
{
    [TestFixture]
    [Category(TestCategory.DataAccess)]
    public class SedimentFileTest
    {
        [SetUp]
        public void Setup()
        {
            Map.CoordinateSystemFactory = new OgrCoordinateSystemFactory();
        }

        [Test]
        public void LoadAndSaveSedFlowFMWithCustomProperties()
        {
            var mduFilePath = TestHelper.GetTestFilePath(@"sedmor\FlowFMCustomProperties\FlowFMCustomPropertiesSedMor.mdu");
            /*The way the sediment reader it's developed forces a model to be created in order to import the .sed file properties */
            var flowFM = new WaterFlowFMModel(mduFilePath);
            Assert.NotNull(flowFM);
            var modelDefinition = flowFM.ModelDefinition;
            TestSedimentsContainAllUnknownProperties(modelDefinition);

            /* Write properties in a new mdu file */
            var mduFile = new MduFile();
            const string saveToDir = "LoadAndSaveSedFlowFM";
            Directory.CreateDirectory(saveToDir);
            var mduFileSaveToPath = Path.Combine(saveToDir, "FlowFMWithCustomProperties.mdu");
            mduFile.Write(mduFileSaveToPath, modelDefinition, flowFM.Area, null,null, null, null, null, null, flowFM.FixedWeirsProperties);

            /* Check if properties have been written again. */
            var newFlowFM = new WaterFlowFMModel(mduFileSaveToPath);
            Assert.NotNull(newFlowFM);
            var newModelDefinition = flowFM.ModelDefinition;
            TestSedimentsContainAllUnknownProperties(newModelDefinition);

        }

        private static void TestSedimentsContainAllUnknownProperties(WaterFlowFMModelDefinition modelDefinition)
        {
            Assert.NotNull( modelDefinition );
            #region Sed 1
            Assert.True(modelDefinition.Properties.Any(
                p => p.PropertyDefinition.FileSectionName.Equals(SedimentFile.SedimentUnknownProperty) &&
                     p.PropertyDefinition.Category.Equals("sed1") &&
                     p.PropertyDefinition.FilePropertyKey.Equals("MyCustomStringProp") &&
                     p.Value.Equals("\"123\"")));

            Assert.True(modelDefinition.Properties.Any(
                p => p.PropertyDefinition.FileSectionName.Equals(SedimentFile.SedimentUnknownProperty) &&
                     p.PropertyDefinition.Category.Equals("sed1") &&
                     p.PropertyDefinition.FilePropertyKey.Equals("MyCustomBoolProp") &&
                     p.Value.Equals("1")));

            Assert.True(modelDefinition.Properties.Any(
                p => p.PropertyDefinition.FileSectionName.Equals(SedimentFile.SedimentUnknownProperty) &&
                     p.PropertyDefinition.Category.Equals("sed1") &&
                     p.PropertyDefinition.FilePropertyKey.Equals("MyCustomDoubleProp") &&
                     p.Value.Equals("1.23")));

            Assert.True(modelDefinition.Properties.Any(
                p => p.PropertyDefinition.FileSectionName.Equals(SedimentFile.SedimentUnknownProperty) &&
                     p.PropertyDefinition.Category.Equals("sed1") &&
                     p.PropertyDefinition.FilePropertyKey.Equals("MyCustomIntProp") &&
                     p.Value.Equals("123")));

            #endregion
            
            #region Sed 2

            Assert.True(modelDefinition.Properties.Any(
                p => p.PropertyDefinition.FileSectionName.Equals(SedimentFile.SedimentUnknownProperty) &&
                     p.PropertyDefinition.Category.Equals("sed2") &&
                     p.PropertyDefinition.FilePropertyKey.Equals("MyCustomStringProp") &&
                     p.Value.Equals("\"231\"")));

            Assert.True(modelDefinition.Properties.Any(
                p => p.PropertyDefinition.FileSectionName.Equals(SedimentFile.SedimentUnknownProperty) &&
                     p.PropertyDefinition.Category.Equals("sed2") &&
                     p.PropertyDefinition.FilePropertyKey.Equals("MyCustomBoolProp") &&
                     p.Value.Equals("0")));

            Assert.True(modelDefinition.Properties.Any(
                p => p.PropertyDefinition.FileSectionName.Equals(SedimentFile.SedimentUnknownProperty) &&
                     p.PropertyDefinition.Category.Equals("sed2") &&
                     p.PropertyDefinition.FilePropertyKey.Equals("MyCustomDoubleProp") &&
                     p.Value.Equals("2.31")));

            Assert.True(modelDefinition.Properties.Any(
                p => p.PropertyDefinition.FileSectionName.Equals(SedimentFile.SedimentUnknownProperty) &&
                     p.PropertyDefinition.Category.Equals("sed2") &&
                     p.PropertyDefinition.FilePropertyKey.Equals("MyCustomIntProp") &&
                     p.Value.Equals("231")));

            #endregion
        }

        [Test]
        public void TestLoadSediments_OnlyUnknownSedimentPropertiesAreAddedToModelDefinition()
        {
            var fmModel = new WaterFlowFMModel();
            var modelDefinitionProperties = fmModel.ModelDefinition.Properties;

            var originalNumberOfProperties = modelDefinitionProperties.Count;

            var sedFilePath = TestHelper.GetTestFilePath(@"sedmor\FlowFMCustomProperties\SedCustomProperties.sed");
            SedimentFile.LoadSediments(sedFilePath, fmModel);

            var unknownPropertiesForSed1 = modelDefinitionProperties.Where(p => p.PropertyDefinition.Category == "sed1").ToList();
            var unknownPropertiesForSed2 = modelDefinitionProperties.Where(p => p.PropertyDefinition.Category == "sed2").ToList();
            
            var finalNumberOfProperties = modelDefinitionProperties.Count;

            Assert.AreEqual(originalNumberOfProperties + unknownPropertiesForSed1.Count + unknownPropertiesForSed2.Count, finalNumberOfProperties,
                "Unexpected number of properties in Model Definition");

            Assert.IsTrue(unknownPropertiesForSed1.Select(p => p.PropertyDefinition).All(pd => pd.FileSectionName == SedimentFile.SedimentUnknownProperty));
            var unknownPropertyNamesSed1 = unknownPropertiesForSed1.Select(p => p.PropertyDefinition.FilePropertyKey).ToList();
            Assert.IsTrue(unknownPropertyNamesSed1.Contains("MyCustomStringProp"));
            Assert.IsTrue(unknownPropertyNamesSed1.Contains("MyCustomBoolProp"));
            Assert.IsTrue(unknownPropertyNamesSed1.Contains("MyCustomDoubleProp"));
            Assert.IsTrue(unknownPropertyNamesSed1.Contains("MyCustomIntProp"));

            Assert.IsTrue(unknownPropertiesForSed2.Select(p => p.PropertyDefinition).All(pd => pd.FileSectionName == SedimentFile.SedimentUnknownProperty));
            var unknownPropertyNamesSed2 = unknownPropertiesForSed2.Select(p => p.PropertyDefinition.FilePropertyKey).ToList();
            Assert.IsTrue(unknownPropertyNamesSed2.Contains("MyCustomStringProp"));
            Assert.IsTrue(unknownPropertyNamesSed2.Contains("MyCustomBoolProp"));
            Assert.IsTrue(unknownPropertyNamesSed2.Contains("MyCustomDoubleProp"));
            Assert.IsTrue(unknownPropertyNamesSed2.Contains("MyCustomIntProp"));
        }

        [Test]
        public void SaveAndReadSedFileWithCustomProperties()
        {
            
            var sedFile = Path.GetTempFileName();
            try
            {
                /* Define new model */
                var fmModel = new WaterFlowFMModel();
                fmModel.ModelDefinition.UseMorphologySediment = true;

                /*  Definition of properties   */
                var intProp = new SedimentProperty<int>("MyIntProp", 0, 0,false, 0, false, "liter", "MyIntDescription", false);
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
                testSedimentType.Properties = new EventedList<ISedimentProperty>() { intProp, doubleProp, boolProp, formulaProp };

                var testFormulaType = new SedimentFormulaType();
                testFormulaType.Properties = new EventedList<ISedimentProperty>() { formulaProp };

                var overallProp = new SedimentProperty<double>("MyOverallProp", 0, 0, true, 0, false, "km", "MyOverallDescription", false);
                overallProp.Value = 80.1;
                fmModel.SedimentOverallProperties = new EventedList<ISedimentProperty>() { overallProp };

                /*Add the fraction to the model*/
                var fraction = new SedimentFraction();
                fraction.Name = "MySedimentName";
                fraction.CurrentSedimentType = testSedimentType;
                fraction.CurrentFormulaType = testFormulaType;

                fmModel.SedimentFractions = new EventedList<ISedimentFraction>() { fraction };
                var modelDefinition = fmModel.ModelDefinition;

                /* Test */
                SedimentFile.Save(sedFile, modelDefinition, fmModel);
                var sedWritten = File.ReadAllText(sedFile);
                Assert.IsTrue(sedWritten.Contains(SedimentFile.GeneralHeader)); 
                Assert.IsTrue(sedWritten.Contains(SedimentFile.OverallHeader)); 
                Assert.IsTrue(sedWritten.Contains(SedimentFile.Header)); 
                Assert.IsTrue(sedWritten.Contains("MyIntProp")); 
                Assert.IsTrue(sedWritten.Contains("MyBoolProp")); 
                Assert.IsTrue(sedWritten.Contains("MyDoubleProp")); 
                Assert.IsTrue(sedWritten.Contains("MyOverallProp")); 
                Assert.IsTrue(sedWritten.Contains("TraFrm")); 
                Assert.IsTrue(sedWritten.Contains("MySedimentName")); 
                Assert.IsTrue(sedWritten.Contains("MySedType")); 
            }
            finally
            {
                FileUtils.DeleteIfExists(sedFile);
            }
        }

        [Test]
        public void SaveAndLoadSedFileWithInValidFractionNameShouldThrowException()
        {
            
            var sedFile = Path.GetTempFileName();
            try
            {
                /* Define new model */
                var fmModel = new WaterFlowFMModel();
                fmModel.ModelDefinition.UseMorphologySediment = true;

                /*Add the fraction to the model*/
                var fraction = new SedimentFraction();
                fraction.Name = "MySedimen*tName";
                var spatiallyVaryingProperty =
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
                Assert.IsTrue(spatiallyVaryingProperty.IsEnabled);//sedconc is always disabled
                Assert.IsNotNull(spatiallyVaryingProperty.SpatiallyVaryingName);

                var modelDefinition = fmModel.ModelDefinition;
                SedimentFile.Save(sedFile, modelDefinition, fmModel);

                var model = new WaterFlowFMModel();

                var message = $@"Could not read sediment file because : Value cannot be null.{Environment.NewLine}Parameter name: Sediment name MySedimen*tName in sediment file {sedFile} is invalid to deltashell";

                TestHelper.AssertLogMessageIsGenerated(() => SedimentFile.LoadSediments(sedFile, model), message, Level.Error);
            }
            finally
            {
                FileUtils.DeleteIfExists(sedFile);
            }
        }

        [Test]
        public void SaveAndLoadSedFileWithModifiedPropertiesValues()
        {

            var sedFile = Path.GetTempFileName();
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
                testSedimentType.Properties = new EventedList<ISedimentProperty>() { intProp, doubleProp, boolProp, sedConcProp, formulaProp, doubleValuePropertyProp };
                
                var overallProp = new SedimentProperty<double>("Cref", 0, 0, true, 0, false, "km", "myoveralldescription", false);
                overallProp.Value = 80.1;
                fmModel.SedimentOverallProperties = new EventedList<ISedimentProperty>(){ overallProp};

                /*Add the fraction to the model*/
                var fraction = new SedimentFraction();
                fraction.Name = "MySedimentName";
                fraction.CurrentSedimentType = testSedimentType;
                fraction.CurrentFormulaType = fraction.SupportedFormulaTypes.FirstOrDefault(sft => sft.TraFrm == -2);

                fmModel.SedimentFractions = new EventedList<ISedimentFraction>() { fraction };
                var modelDefinition = fmModel.ModelDefinition;

                /* Test */
                SedimentFile.Save(sedFile, modelDefinition, fmModel);
                var model = new WaterFlowFMModel();
                SedimentFile.LoadSediments(sedFile, model);

                var loadedOverallProp = model.SedimentOverallProperties.FirstOrDefault() as ISedimentProperty<double>;
                Assert.IsNotNull(loadedOverallProp);
                Assert.That(loadedOverallProp.Name.Contains("Cref")); 
                Assert.That(loadedOverallProp.Value, Is.EqualTo(80.1).Within(0.01));

                var loadedSedimentFraction = model.SedimentFractions.FirstOrDefault();
                Assert.IsNotNull(loadedSedimentFraction);
                Assert.That(loadedSedimentFraction.Name.Contains("MySedimentName"));
                Assert.That(loadedSedimentFraction.CurrentSedimentType.Key.Contains("sand"));

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
            var sedFile = Path.GetTempFileName();
            var generatedXyzFile = Path.Combine(Path.GetDirectoryName(sedFile), "mysedimentName_SedConc." + XyzFile.Extension);

            try
            {
                /* Define new model */
                var fmModel = new WaterFlowFMModel(sedFile);
                fmModel.ModelDefinition.UseMorphologySediment = true;

                //Area 
                // Import dry points
                const string baseFolderPath = @"HydroAreaCollection/MduFileProjects";

                fmModel.Area.DryAreas.Add(new GroupableFeature2DPolygon()
                {
                    GroupName = Path.Combine(Directory.GetCurrentDirectory(), baseFolderPath, @"MyDryAreas_dry.pol")
                });

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
                testSedimentType.Properties = new EventedList<ISedimentProperty>() { doubleSpatProp, doubleSpatProp2 };
                
                var overallProp = new SedimentProperty<double>("Cref", 0, 0, true, 0, false, "km", "myoveralldescription", false);
                overallProp.Value = 80.1;
                fmModel.SedimentOverallProperties = new EventedList<ISedimentProperty>() { overallProp };

                /*Add the fraction to the model*/
                var fraction = new SedimentFraction();
                fraction.Name = "mysedimentName";
                fraction.CurrentSedimentType = testSedimentType;
               
                /*  Test    */
                var grid = UnstructuredGridTestHelper.GenerateRegularGrid(2, 2, 2, 2);
                
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
                fmModel.DataItems = new EventedList<IDataItem> { dataSedThickItem, dataSedConcItem };
                fmModel.SedimentOverallProperties = new EventedList<ISedimentProperty>();
                fmModel.SedimentFractions = new EventedList<ISedimentFraction>();
                fmModel.SedimentFractions.Add(fraction);

                var modelDefinition = fmModel.ModelDefinition;
                /* 
                 * SedimentFile.Save(sedFile, fmModel);
                 * Spatially varying operations no longer get saved through this method but through ExtForceFile.cs
                 */
                SedimentFile.Save(sedFile, modelDefinition, fmModel);

                var sedWritten = File.ReadAllText(sedFile);
                Assert.That(sedWritten.Contains(SedimentFile.GeneralHeader));
                Assert.That(sedWritten, Is.Not.Contains("SedConc"));
                Assert.That(sedWritten.Contains("#mysedimentName#"));
                Assert.That(sedWritten, Is.Not.Contains("#mysedimentName_SedConc#"));
                Assert.That(sedWritten, Is.Not.Contains("12.3"));

                Assert.That(sedWritten.Contains("IniSedThick"));
                Assert.That(sedWritten.Contains("#mysedimentName_IniSedThick.xyz#"));
                Assert.That(sedWritten, Is.Not.Contains("80.1"));

                Assert.IsFalse(File.Exists(generatedXyzFile));
                
                /* Check the ExtFile */
                // update model definition (called during export)
                var initialSpatialOps = new List<string>() { doubleSpatProp.SpatiallyVaryingName, doubleSpatProp2.SpatiallyVaryingName };
                fmModel.ModelDefinition.SelectSpatialOperations(fmModel.DataItems, fmModel.TracerDefinitions, initialSpatialOps);
                var extFile = new ExtForceFile();
                extFile.WriteExtForceFileSubFiles(sedFile, fmModel.ModelDefinition, false);
                Assert.IsTrue(File.Exists(generatedXyzFile));

                var xyzFileValues = XyzFile.Read(generatedXyzFile).ToList();
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
        public void SaveSedFileWithSpatiallyVaryingPropertiesAndNoOperationsGeneratesWarningMessages()
        {
            var sedFile = Path.GetTempFileName();
            var generatedXyzFile = Path.Combine(Path.GetDirectoryName(sedFile), "mysedimentName_SedConc." + XyzFile.Extension);

            try
            {
                /* Define new model */
                var fmModel = new WaterFlowFMModel(sedFile);
                fmModel.ModelDefinition.UseMorphologySediment = true;
                var grid = UnstructuredGridTestHelper.GenerateRegularGrid(2, 2, 2, 2);
                fmModel.Grid = grid;

                var fraction = new SedimentFraction() { Name = "Frac1" };
                fmModel.SedimentFractions.Add(fraction);

                var modelDefinition = fmModel.ModelDefinition;
                // Save SedFile with no spatially varying properties. No warnings should be given.
                TestHelper.AssertLogMessagesCount( () => SedimentFile.Save(sedFile, modelDefinition, fmModel), 0);
                
                // Add a spatially varying prop 
                var randomSVProp = fraction.CurrentSedimentType.Properties.OfType<ISpatiallyVaryingSedimentProperty>().FirstOrDefault(p => p.Name != "SedConc");
                Assert.NotNull(randomSVProp);
                randomSVProp.IsSpatiallyVarying = true;
                // Warning should be given.
                TestHelper.AssertAtLeastOneLogMessagesContains(
                    () => SedimentFile.Save(sedFile, fmModel.ModelDefinition, fmModel),
                    String.Format(Resources.SedimentFile_WriteSpatiallyVaryingSedimentPropertySubFiles_No_spatial_operations_of_type_Import__Add_or_Value_found_for_spatially_varying_property__0___Remember_to_interpolate_them_to_generate_the_xyz_file__Otherwise_the_model_might_not_run_as_expected_, randomSVProp.SpatiallyVaryingName));

                //Add a 'value' operation, another warning should be given.
                var dataItem = fmModel.AllDataItems.FirstOrDefault(di => di.Name == randomSVProp.SpatiallyVaryingName);

                // retrieve / create value converter for mysedimentName_SedConc dataitem
                var valueConverter = SpatialOperationValueConverterFactory.GetOrCreateSpatialOperationValueConverter(dataItem, randomSVProp.SpatiallyVaryingName);
                var samples = new SetValueOperation();
                samples.SetInputData(ValueOperationBase.MainInputName, new PointCloudFeatureProvider
                {
                    PointCloud = new PointCloud
                    {
                        PointValues = new List<IPointValue>
                        {
                            new PointValue { X = fmModel.Grid.Cells[0].CenterX, Y = fmModel.Grid.Cells[0].CenterY, Value = 12},
                            new PointValue { X = fmModel.Grid.Cells[1].CenterX, Y = fmModel.Grid.Cells[1].CenterY, Value = 30},
                            new PointValue { X = fmModel.Grid.Cells[2].CenterX, Y = fmModel.Grid.Cells[2].CenterY, Value = 31},
                        },
                    },

                });
                var sp = valueConverter.SpatialOperationSet.AddOperation(samples);
                valueConverter.SpatialOperationSet.Execute();

                // update model definition (called during export)
                var initialSpatialOps = new List<string>() { randomSVProp.SpatiallyVaryingName };
                fmModel.ModelDefinition.SelectSpatialOperations(fmModel.DataItems, fmModel.TracerDefinitions, initialSpatialOps);

                // New Warning should be given.
            
                TestHelper.AssertAtLeastOneLogMessagesContains(
                    () => SedimentFile.Save(sedFile, modelDefinition, fmModel),
                    String.Format(Resources.SedimentFile_WriteSpatiallyVaryingSedimentPropertySubFiles_Cannot_create_xyz_file_for_spatial_varying_initial_condition__0__because_it_is_a_value_spatial_operation__please_interpolate_the_operation_to_the_grid_or, randomSVProp.SpatiallyVaryingName));
            }
            finally
            {
                FileUtils.DeleteIfExists(sedFile);
                FileUtils.DeleteIfExists(generatedXyzFile);
            }
        }

        [Test]
        public void SaveSedFileWithSpatiallyVaryingPropertiesAndAddValuesOperation()
        {
            var sedFile = Path.GetTempFileName();
            var generatedXyzFile = Path.Combine(Path.GetDirectoryName(sedFile), "mysedimentName_SedConc." + XyzFile.Extension);
            string fileCopyName = "";
            try
            {
                /* Define new model */
                var fmModel = new WaterFlowFMModel(sedFile);
                fmModel.ModelDefinition.UseMorphologySediment = true;
                var grid = UnstructuredGridTestHelper.GenerateRegularGrid(2, 2, 2, 2);
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

                var overallProp = new SedimentProperty<double>("Cref", 0, 0, true, 0, false, "km", "myoveralldescription", false)
                {
                    Value = 80.1
                };

                fmModel.SedimentOverallProperties = new EventedList<ISedimentProperty>() { overallProp };

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

                var dataItem = fmModel.AllDataItems.FirstOrDefault(di => di.Name == "mysedimentName_SedConc");
                
                // retrieve / create value converter for mysedimentName_SedConc dataitem
                var valueConverter = SpatialOperationValueConverterFactory.GetOrCreateSpatialOperationValueConverter(dataItem, "mysedimentName_SedConc");
                var samples = new AddSamplesOperation(false);
                samples.SetInputData(AddSamplesOperation.SamplesInputName, new PointCloudFeatureProvider
                {
                    PointCloud = new PointCloud
                    {
                        PointValues = new List<IPointValue>
                            {
                                    new PointValue { X = fmModel.Grid.Cells[0].CenterX, Y = fmModel.Grid.Cells[0].CenterY, Value = 12},
                                    new PointValue { X = fmModel.Grid.Cells[1].CenterX, Y = fmModel.Grid.Cells[1].CenterY, Value = 30},
                                    new PointValue { X = fmModel.Grid.Cells[2].CenterX, Y = fmModel.Grid.Cells[2].CenterY, Value = 31},
                                },
                    },
                    
                });
                valueConverter.SpatialOperationSet.AddOperation(samples);
                valueConverter.SpatialOperationSet.Execute();

                // update model definition (called during export)
                var initialSpatialOps = new List<string>() { doubleSpatProp.SpatiallyVaryingName };
                fmModel.ModelDefinition.SelectSpatialOperations(fmModel.DataItems, fmModel.TracerDefinitions, initialSpatialOps);
                // create an interpolate operation using the samples added earlier
                var interpolateOperation = new InterpolateOperation();
                interpolateOperation.SetInputData(InterpolateOperation.InputSamplesName, samples.Output.Provider);
                Assert.IsNotNull(valueConverter.SpatialOperationSet.AddOperation(interpolateOperation));

                // update model definition (called during export)
                fmModel.ModelDefinition.SelectSpatialOperations(fmModel.DataItems, fmModel.TracerDefinitions, initialSpatialOps);

                var modelDefinition = fmModel.ModelDefinition;
                SedimentFile.Save(sedFile, modelDefinition, fmModel);
                
                var sedWritten = File.ReadAllText(sedFile);
                Assert.That(sedWritten.Contains(SedimentFile.GeneralHeader));
                Assert.That(sedWritten, Is.Not.Contains("SedConc"));
                Assert.That(sedWritten.Contains("#mysedimentName#"));
                Assert.That(sedWritten, Is.Not.Contains("#mysedimentName_SedConc#"));
                Assert.That(sedWritten, Is.Not.Contains("12.3"));

                /* 
                 * SedimentFile.Save(sedFile, fmModel);
                 * Spatially varying operations no longer get saved through this method but through ExtForceFile.cs
                 */
                Assert.IsFalse(File.Exists(generatedXyzFile));
                var extFile = new ExtForceFile();
                extFile.WriteExtForceFileSubFiles(sedFile, fmModel.ModelDefinition, false);
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
        public void LoadSedFileWithSpatiallyVaryingProperties_MudFraction()
        {
            var mduPath = TestHelper.GetTestFilePath(@"SpatiallyVarying_MudFraction\FlowFM.mdu");
            var localCopy = TestHelper.CreateLocalCopy(mduPath);

            using (var model = new WaterFlowFMModel(localCopy))
            {
                var fraction = model.SedimentFractions.FirstOrDefault(sf => sf.Name == "mudFraction");
                Assert.IsNotNull(fraction);
                var spatvaryingProp = fraction.CurrentSedimentType.Properties.FirstOrDefault(p => p.Name == "IniSedThick") as ISpatiallyVaryingSedimentProperty;
                Assert.IsNotNull(spatvaryingProp);
                Assert.IsTrue(spatvaryingProp.IsSpatiallyVarying);
                var dataItem = model.DataItems.FirstOrDefault(di => di.Name == "mudFraction_IniSedThick");
                Assert.IsNotNull(dataItem);
                var coverage = dataItem.Value as UnstructuredGridCellCoverage;
                Assert.IsNotNull(coverage);
                var values = coverage.GetValues<double>();

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
                dataItem = model.DataItems.FirstOrDefault(di => di.Name == "mudFraction_TcrSed");
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
                dataItem = model.DataItems.FirstOrDefault(di => di.Name == "mudFraction_TcrEro");
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
        public void LoadSedFileWithSpatiallyVaryingProperties()
        {
            var mduPath = TestHelper.GetTestFilePath(@"spatially_varying_sediment_properties_in_model\FlowFM.mdu");
            var localCopy = TestHelper.CreateLocalCopy(mduPath);

            using (var model = new WaterFlowFMModel(localCopy))
            {
                var fraction = model.SedimentFractions.FirstOrDefault(sf => sf.Name == "gouwe");
                Assert.IsNotNull(fraction);
                var spatvaryingProp = fraction.CurrentSedimentType.Properties.FirstOrDefault(p => p.Name == "IniSedThick") as
                        ISpatiallyVaryingSedimentProperty;
                Assert.IsNotNull(spatvaryingProp);
                Assert.IsTrue(spatvaryingProp.IsSpatiallyVarying);
                var dataItem = model.DataItems.FirstOrDefault(di => di.Name == "gouwe_IniSedThick");
                Assert.IsNotNull(dataItem);
                var coverage = dataItem.Value as UnstructuredGridCellCoverage;
                Assert.IsNotNull(coverage);
                var values = coverage.GetValues<double>();

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
            var sedFile = Path.GetTempFileName();
            var generatedXyzFile = Path.Combine(Path.GetDirectoryName(sedFile),
                "mysedimentName_IniSedThick." + XyzFile.Extension);

            try
            {
                /* Define new model */
                var fmModel = new WaterFlowFMModel(sedFile);
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

                var fileName = TestHelper.GetTestFilePath(@"harlingen_model_3d\har_V3.xyz");
                fileName = TestHelper.CreateLocalCopy(fileName);

                fmModel.SedimentOverallProperties = new EventedList<ISedimentProperty>();
                fmModel.SedimentFractions = new EventedList<ISedimentFraction>();
                fmModel.SedimentFractions.Add(fraction);

                var dataItem = fmModel.AllDataItems.FirstOrDefault(di => di.Name == "mysedimentName_IniSedThick");

                // retrieve / create value converter for mysedimentName_SedConc dataitem
                var valueConverter =
                    SpatialOperationValueConverterFactory.GetOrCreateSpatialOperationValueConverter(dataItem,
                        "mysedimentName_IniSedThick");

                valueConverter.SpatialOperationSet.AddOperation(new ImportSamplesOperationImportData()
                {
                    Name = "mysedimentName_IniSedThick",
                    FilePath = Path.GetFullPath(fileName)
                });
                valueConverter.SpatialOperationSet.Execute();
                var modelDefinition = fmModel.ModelDefinition;
                SedimentFile.Save(sedFile, modelDefinition, fmModel);
                var sedWritten = File.ReadAllText(sedFile);
                Assert.That(sedWritten.Contains(SedimentFile.GeneralHeader));
                Assert.That(sedWritten.Contains("#mysedimentName#"));
                
                /* Sed conc is in ExtForceFile */
                Assert.That(sedWritten, Is.Not.Contains("SedConc"));
                Assert.That(sedWritten, Is.Not.Contains("#mysedimentName_SedConc#"));
                
                /* Custom property */
                Assert.That(sedWritten.Contains("IniSed"));
                Assert.That(sedWritten.Contains("#mysedimentName_IniSedThick.xyz#"));

                Assert.That(sedWritten, Is.Not.Contains("12.3"));

                Assert.IsTrue(File.Exists(generatedXyzFile));
            }
            finally
            {
                FileUtils.DeleteIfExists(sedFile);
                FileUtils.DeleteIfExists(generatedXyzFile);
            }
        }

        [Test]
        public void CloneLoadedSedFileWithSpatiallyVaryingProperties()
        {
            var mduPath =
            TestHelper.GetTestFilePath(@"spatially_varying_sediment_properties_in_model\FlowFM.mdu");
            var localCopy = TestHelper.CreateLocalCopy(mduPath);

            using (var orgModel = new WaterFlowFMModel(localCopy))
            {
                CheckModelCoverageValues(orgModel);
                using (var model = (WaterFlowFMModel)orgModel.DeepClone())
                {
                    CheckModelCoverageValues(model);
                }
            }
        }

        private static void CheckModelCoverageValues(WaterFlowFMModel model)
        {
            var fraction = model.SedimentFractions.FirstOrDefault(sf => sf.Name == "gouwe");
            Assert.IsNotNull(fraction);
            var spatvaryingProp =
                fraction.CurrentSedimentType.Properties.FirstOrDefault(p => p.Name == "IniSedThick") as
                    ISpatiallyVaryingSedimentProperty;
            Assert.IsNotNull(spatvaryingProp);
            Assert.IsTrue(spatvaryingProp.IsSpatiallyVarying);
            var dataItem = model.DataItems.FirstOrDefault(di => di.Name == "gouwe_IniSedThick");
            Assert.IsNotNull(dataItem);
            var coverage = dataItem.Value as UnstructuredGridCellCoverage;
            Assert.IsNotNull(coverage);
            var values = coverage.GetValues<double>();

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
}