using System;
using System.IO;
using System.Linq;
using DelftTools.Shell.Core.Workflow.DataItems;
using DelftTools.TestUtils;
using DelftTools.Utils.Collections.Generic;
using DelftTools.Utils.IO;
using DeltaShell.Plugins.FMSuite.FlowFM.IO;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using log4net.Core;
using NetTopologySuite.Extensions.Coverages;
using NUnit.Framework;
using SharpMap;
using SharpMapTestUtils;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.IO
{
    [TestFixture]
    [Category(TestCategory.DataAccess)]
    public class SedimentFileTest
    {
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
            mduFile.Write(mduFileSaveToPath, flowFM.ModelDefinition, flowFM.Area);

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
                p => p.PropertyDefinition.FileCategoryName.Equals(SedimentFile.SedimentUnknownProperty) &&
                     p.PropertyDefinition.Category.Equals("sed1") &&
                     p.PropertyDefinition.FilePropertyName.Equals("MyCustomStringProp") &&
                     p.Value.Equals("\"123\"")));

            Assert.True(modelDefinition.Properties.Any(
                p => p.PropertyDefinition.FileCategoryName.Equals(SedimentFile.SedimentUnknownProperty) &&
                     p.PropertyDefinition.Category.Equals("sed1") &&
                     p.PropertyDefinition.FilePropertyName.Equals("MyCustomBoolProp") &&
                     p.Value.Equals("1")));

            Assert.True(modelDefinition.Properties.Any(
                p => p.PropertyDefinition.FileCategoryName.Equals(SedimentFile.SedimentUnknownProperty) &&
                     p.PropertyDefinition.Category.Equals("sed1") &&
                     p.PropertyDefinition.FilePropertyName.Equals("MyCustomDoubleProp") &&
                     p.Value.Equals("1.23")));

            Assert.True(modelDefinition.Properties.Any(
                p => p.PropertyDefinition.FileCategoryName.Equals(SedimentFile.SedimentUnknownProperty) &&
                     p.PropertyDefinition.Category.Equals("sed1") &&
                     p.PropertyDefinition.FilePropertyName.Equals("MyCustomIntProp") &&
                     p.Value.Equals("123")));

            #endregion
            
            #region Sed 2

            Assert.True(modelDefinition.Properties.Any(
                p => p.PropertyDefinition.FileCategoryName.Equals(SedimentFile.SedimentUnknownProperty) &&
                     p.PropertyDefinition.Category.Equals("sed2") &&
                     p.PropertyDefinition.FilePropertyName.Equals("MyCustomStringProp") &&
                     p.Value.Equals("\"231\"")));

            Assert.True(modelDefinition.Properties.Any(
                p => p.PropertyDefinition.FileCategoryName.Equals(SedimentFile.SedimentUnknownProperty) &&
                     p.PropertyDefinition.Category.Equals("sed2") &&
                     p.PropertyDefinition.FilePropertyName.Equals("MyCustomBoolProp") &&
                     p.Value.Equals("0")));

            Assert.True(modelDefinition.Properties.Any(
                p => p.PropertyDefinition.FileCategoryName.Equals(SedimentFile.SedimentUnknownProperty) &&
                     p.PropertyDefinition.Category.Equals("sed2") &&
                     p.PropertyDefinition.FilePropertyName.Equals("MyCustomDoubleProp") &&
                     p.Value.Equals("2.31")));

            Assert.True(modelDefinition.Properties.Any(
                p => p.PropertyDefinition.FileCategoryName.Equals(SedimentFile.SedimentUnknownProperty) &&
                     p.PropertyDefinition.Category.Equals("sed2") &&
                     p.PropertyDefinition.FilePropertyName.Equals("MyCustomIntProp") &&
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

            Assert.IsTrue(unknownPropertiesForSed1.Select(p => p.PropertyDefinition).All(pd => pd.FileCategoryName == SedimentFile.SedimentUnknownProperty));
            var unknownPropertyNamesSed1 = unknownPropertiesForSed1.Select(p => p.PropertyDefinition.FilePropertyName).ToList();
            Assert.IsTrue(unknownPropertyNamesSed1.Contains("MyCustomStringProp"));
            Assert.IsTrue(unknownPropertyNamesSed1.Contains("MyCustomBoolProp"));
            Assert.IsTrue(unknownPropertyNamesSed1.Contains("MyCustomDoubleProp"));
            Assert.IsTrue(unknownPropertyNamesSed1.Contains("MyCustomIntProp"));

            Assert.IsTrue(unknownPropertiesForSed2.Select(p => p.PropertyDefinition).All(pd => pd.FileCategoryName == SedimentFile.SedimentUnknownProperty));
            var unknownPropertyNamesSed2 = unknownPropertiesForSed2.Select(p => p.PropertyDefinition.FilePropertyName).ToList();
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
                fmModel.ModelDefinition.GetModelProperty(GuiProperties.UseMorSed).Value = true;

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

                /* Test */
                SedimentFile.Save(sedFile, fmModel);
                var sedWritten = File.ReadAllText(sedFile);
                Assert.That(sedWritten, Is.StringContaining(SedimentFile.GeneralHeader)); 
                Assert.That(sedWritten, Is.StringContaining(SedimentFile.OverallHeader)); 
                Assert.That(sedWritten, Is.StringContaining(SedimentFile.Header)); 
                Assert.That(sedWritten, Is.StringContaining("MyIntProp")); 
                Assert.That(sedWritten, Is.StringContaining("MyBoolProp")); 
                Assert.That(sedWritten, Is.StringContaining("MyDoubleProp")); 
                Assert.That(sedWritten, Is.StringContaining("MyOverallProp")); 
                Assert.That(sedWritten, Is.StringContaining("TraFrm")); 
                Assert.That(sedWritten, Is.StringContaining("MySedimentName")); 
                Assert.That(sedWritten, Is.StringContaining("MySedType")); 
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
                fmModel.ModelDefinition.GetModelProperty(GuiProperties.UseMorSed).Value = true;


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
                Assert.IsFalse(spatiallyVaryingProperty.IsEnabled);//sedconc is always disabled
                Assert.IsNotNull(spatiallyVaryingProperty.SpatiallyVaryingName);

                /* Test */
                SedimentFile.Save(sedFile, fmModel);
                var model = new WaterFlowFMModel();
                LogHelper.ConfigureLogging(Level.Error);
                try
                {
                    TestHelper.AssertLogMessageIsGenerated(() =>
                    {
                        SedimentFile.LoadSediments(sedFile, model);
                    }, string.Format(@"Could not read sediment file because : Value cannot be null.{0}Parameter name: Sediment name MySedimen*tName in sediment file {1} is invalid to deltashell", Environment.NewLine, sedFile));
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

            var sedFile = Path.GetTempFileName();
            try
            {
                /* Define new model */
                var fmModel = new WaterFlowFMModel();
                fmModel.ModelDefinition.GetModelProperty(GuiProperties.UseMorSed).Value = true;

                /* Define test properties */
                var intProp = new SedimentProperty<int>("IopSus", 0, 0, false, 0, false, "liter", "myintdescription", false);
                intProp.Value = 27;
                
                var boolProp = new SedimentProperty<bool>("EpsPar", false, false, false, false, false, "sec",
                    "mybooldescription", false);
                boolProp.Value = true;

                var doubleProp = new SedimentProperty<double>("SedDia", 0, 0, false, 0, false, "cc", "mydoubledescription", false);
                doubleProp.Value = 11.2;

                var doubleSpatProp = new SpatiallyVaryingSedimentProperty<double>("SedConc", 0, 0, false, 0, false, "cc", "mydoubledescription", false, false);
                doubleSpatProp.Value = 33.4;

                var formulaProp = new SedimentProperty<int>("TraFrm", -1, -2, false, 18, false, string.Empty, "Integer selecting the transport formula", true);
                formulaProp.Value = -2;

                /* Set sediment and formula properties */
                var testSedimentType = new SedimentType();
                testSedimentType.Key = "sand";
                testSedimentType.Properties = new EventedList<ISedimentProperty>() { intProp, doubleProp, boolProp, doubleSpatProp, formulaProp };
                
                var overallProp = new SedimentProperty<double>("Cref", 0, 0, true, 0, false, "km", "myoveralldescription", false);
                overallProp.Value = 80.1;
                fmModel.SedimentOverallProperties = new EventedList<ISedimentProperty>(){ overallProp};

                /*Add the fraction to the model*/
                var fraction = new SedimentFraction();
                fraction.Name = "MySedimentName";
                fraction.CurrentSedimentType = testSedimentType;
                fraction.CurrentFormulaType = fraction.SupportedFormulaTypes.FirstOrDefault(sft => sft.TraFrm == -2);

                fmModel.SedimentFractions = new EventedList<ISedimentFraction>() { fraction };

                /* Test */
                SedimentFile.Save(sedFile, fmModel);
                var model = new WaterFlowFMModel();
                SedimentFile.LoadSediments(sedFile, model);

                var loadedOverallProp = model.SedimentOverallProperties.FirstOrDefault() as ISedimentProperty<double>;
                Assert.IsNotNull(loadedOverallProp);
                Assert.That(loadedOverallProp.Name, Is.StringContaining("Cref")); 
                Assert.That(loadedOverallProp.Value, Is.EqualTo(80.1).Within(0.01));

                var loadedSedimentFraction = model.SedimentFractions.FirstOrDefault();
                Assert.IsNotNull(loadedSedimentFraction);
                Assert.That(loadedSedimentFraction.Name, Is.StringContaining("MySedimentName"));
                Assert.That(loadedSedimentFraction.CurrentSedimentType.Key, Is.StringContaining("sand"));

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

                var loadedSpatDoubleProp = loadedSedimentFraction.CurrentSedimentType.Properties.FirstOrDefault(st => st.Name == "SedConc") as ISpatiallyVaryingSedimentProperty<double>;
                Assert.IsNotNull(loadedSpatDoubleProp);
                Assert.That(loadedSpatDoubleProp.Value, Is.EqualTo(33.4).Within(0.01));
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
                fmModel.ModelDefinition.GetModelProperty(GuiProperties.UseMorSed).Value = true;

                /* Define test properties */
                var doubleSpatProp = new SpatiallyVaryingSedimentProperty<double>("SedConc", 0, 0, false, 0, true, "cc", "mydoubledescription", true, false);
                doubleSpatProp.SpatiallyVaryingName = "mysedimentName_SedConc";
                doubleSpatProp.Value = 12.3;

                var doubleSpatProp2 = new SpatiallyVaryingSedimentProperty<double>("IniSedThick", 0, 0, false, 0, true, "Joule", "mydoubledescription", false, false);
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
                var coverage = new UnstructuredGridCellCoverage(grid, false);
                coverage[0] = 0.1;
                coverage[1] = 3.2;
                coverage[2] = 5.4;
                coverage[3] = 7.6;

                coverage.Name = "mysedimentName_SedConc";

                coverage.Components[0].NoDataValue = -999;
                
                var dataItem = new DataItem();
                dataItem.Name = "mysedimentName_SedConc";
                dataItem.Value = coverage;
                dataItem.Role = DataItemRole.Input;

                fmModel.DataItems = new EventedList<IDataItem> { dataItem };
                fmModel.SedimentOverallProperties = new EventedList<ISedimentProperty>();
                fmModel.SedimentFractions = new EventedList<ISedimentFraction>();
                fmModel.SedimentFractions.Add(fraction);
                
                SedimentFile.Save(sedFile, fmModel);
                var sedWritten = File.ReadAllText(sedFile);
                Assert.That(sedWritten, Is.StringContaining(SedimentFile.GeneralHeader));
                Assert.That(sedWritten, Is.StringContaining("SedConc"));
                Assert.That(sedWritten, Is.StringContaining("#mysedimentName#"));
                Assert.That(sedWritten, Is.StringContaining("#mysedimentName_SedConc#"));
                Assert.That(sedWritten, Is.Not.StringContaining("12.3"));

                Assert.That(sedWritten, Is.StringContaining("IniSedThick"));
                Assert.That(sedWritten, Is.Not.StringContaining("mysedimentName_IniSedThick"));
                Assert.That(sedWritten, Is.StringContaining("80.1"));

                Assert.IsTrue(File.Exists(generatedXyzFile));
                var xyzFileValues = new XyzFile().Read(generatedXyzFile).ToList();
                Assert.That(xyzFileValues.ElementAt(0).X, Is.EqualTo(coverage.Coordinates.ElementAt(0).X));
                Assert.That(xyzFileValues.ElementAt(0).Y, Is.EqualTo(coverage.Coordinates.ElementAt(0).Y));
                Assert.That(xyzFileValues.ElementAt(0).Value, Is.EqualTo(coverage.GetValues<double>().ElementAt(0)));

                Assert.That(xyzFileValues.ElementAt(1).X, Is.EqualTo(coverage.Coordinates.ElementAt(1).X));
                Assert.That(xyzFileValues.ElementAt(1).Y, Is.EqualTo(coverage.Coordinates.ElementAt(1).Y));
                Assert.That(xyzFileValues.ElementAt(1).Value, Is.EqualTo(coverage.GetValues<double>().ElementAt(1)));

                Assert.That(xyzFileValues.ElementAt(2).X, Is.EqualTo(coverage.Coordinates.ElementAt(2).X));
                Assert.That(xyzFileValues.ElementAt(2).Y, Is.EqualTo(coverage.Coordinates.ElementAt(2).Y));
                Assert.That(xyzFileValues.ElementAt(2).Value, Is.EqualTo(coverage.GetValues<double>().ElementAt(2)));

                Assert.That(xyzFileValues.ElementAt(3).X, Is.EqualTo(coverage.Coordinates.ElementAt(3).X));
                Assert.That(xyzFileValues.ElementAt(3).Y, Is.EqualTo(coverage.Coordinates.ElementAt(3).Y));
                Assert.That(xyzFileValues.ElementAt(3).Value, Is.EqualTo(coverage.GetValues<double>().ElementAt(3)));
            }
            finally
            {
                FileUtils.DeleteIfExists(sedFile);
                FileUtils.DeleteIfExists(generatedXyzFile);
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
        public void CloneLoadedSedFileWithSpatiallyVaryingProperties()
        {
            var mduPath =
            TestHelper.GetTestFilePath(@"spatially_varying_sediment_properties_in_model\FlowFM.mdu");
            var localCopy = TestHelper.CreateLocalCopy(mduPath);

            using (var orgmodel = new WaterFlowFMModel(localCopy))
            {
                using (var model = (WaterFlowFMModel)orgmodel.DeepClone())
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
    }
}