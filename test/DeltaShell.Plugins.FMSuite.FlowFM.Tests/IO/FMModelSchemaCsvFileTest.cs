using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.TestUtils;
using DelftTools.Utils.IO;
using DelftTools.Utils.Reflection;
using DeltaShell.Plugins.FMSuite.Common;
using DeltaShell.Plugins.FMSuite.Common.IO.Files;
using DeltaShell.Plugins.FMSuite.Common.ModelSchema;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.IO
{
    [TestFixture]
    public class FMModelSchemaCsvFileTest
    {
        private string dflowfmPropertiesCsvFilePath = Path.Combine("plugins",
                                                                   "DeltaShell.Plugins.FMSuite.FlowFM",
                                                                   "CsvFiles",
                                                                   "dflowfm-properties.csv");

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            // trigger creation here, to make sure it's not triggered after these tests have ran.
            new WaterFlowFMModelDefinition();
        }

        [Test]
        public void LoadCharEnums()
        {
            ModelPropertySchema<WaterFlowFMPropertyDefinition> modelPropertySchema = new ModelSchemaCsvFile().ReadModelSchema<WaterFlowFMPropertyDefinition>(dflowfmPropertiesCsvFilePath, "MduGroup");
            Assert.Greater(modelPropertySchema.PropertyDefinitions.Count, 50);

            WaterFlowFMPropertyDefinition tunitProperty = modelPropertySchema.PropertyDefinitions[KnownProperties.Tunit];
            Type tunitEnum = tunitProperty.DataType;
            IList<Enum> values = GetEnumValues(tunitEnum);

            Assert.AreEqual("H", values[0].GetDisplayName());
            Assert.AreEqual("S", tunitProperty.DefaultValueAsString);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void Read_FMModelSchema_DoesNotSplitCommentWithCommas()
        {
            //01. Load test file
            string csvTestFile = TestHelper.GetTestFilePath(@"CsvFile\properties_test.csv");
            Assert.IsNotNull(csvTestFile);
            Assert.IsTrue(File.Exists(csvTestFile));

            //02. Create local copy
            csvTestFile = TestHelper.CreateLocalCopySingleFile(csvTestFile);
            Assert.IsTrue(File.Exists(csvTestFile));

            //03. Set expectations
            var testPropName = "PropertyWithCommasOnDescription";
            var expectedDescription = "Dummy description, with comma in it";
            var expectedUnit = "dummyUnit";

            //04. Load CSV into properties.
            ModelPropertySchema<WaterFlowFMPropertyDefinition> modelPropertySchema = new ModelSchemaCsvFile().ReadModelSchema<WaterFlowFMPropertyDefinition>(csvTestFile, "MduGroup");

            //05. Find property, and check description.
            KeyValuePair<string, WaterFlowFMPropertyDefinition> testProp = modelPropertySchema.PropertyDefinitions.FirstOrDefault(pd => pd.Value.MduPropertyName == testPropName);
            Assert.IsNotNull(testProp);
            Assert.AreEqual(expectedDescription, testProp.Value.Description);
            Assert.AreEqual(expectedUnit, testProp.Value.Unit);

            //Remove test data
            FileUtils.DeleteIfExists(csvTestFile);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void Read_FMModelSchema_DoesNotSplitMduNameWithCommas()
        {
            // Setup
            string propertiesFilePath = TestHelper.GetTestFilePath(@"CsvFile\dflowfm-properties_CommasInMduName.csv");
            using (var temporaryDirectory = new TemporaryDirectory())
            {
                string testFilePath = temporaryDirectory.CopyTestDataFileToTempDirectory(propertiesFilePath);
                var schemaReader = new ModelSchemaCsvFile();

                // Call
                ModelPropertySchema<WaterFlowFMPropertyDefinition> schema = schemaReader.ReadModelSchema<WaterFlowFMPropertyDefinition>(testFilePath, "MduGroup");

                // Assert
                WaterFlowFMPropertyDefinition readPropertyDefinition = schema.PropertyDefinitions.Single().Value;
                Assert.That(readPropertyDefinition.Caption, Is.EqualTo("This, that, here, there"));
                Assert.That(readPropertyDefinition.FilePropertyKey, Is.EqualTo("This, that, here, there"));
            }
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void Read_FMModelSchema_LoadsUnits()
        {
            //Load test file
            string csvTestFile = TestHelper.GetTestFilePath(@"CsvFile\properties_test.csv");
            Assert.IsNotNull(csvTestFile);
            Assert.IsTrue(File.Exists(csvTestFile));
            //Create local copy
            csvTestFile = TestHelper.CreateLocalCopySingleFile(csvTestFile);
            Assert.IsTrue(File.Exists(csvTestFile));

            //Load CSV into properties.
            ModelPropertySchema<WaterFlowFMPropertyDefinition> modelPropertySchema = new ModelSchemaCsvFile().ReadModelSchema<WaterFlowFMPropertyDefinition>(csvTestFile, "MduGroup");

            //Check units have been loaded
            Assert.IsTrue(modelPropertySchema.PropertyDefinitions.Any(pd => !string.IsNullOrEmpty(pd.Value.Unit)));
            Assert.IsTrue(modelPropertySchema.PropertyDefinitions.Any(pd => pd.Value.Unit.Equals("dummyUnit")));

            //Remove test data
            FileUtils.DeleteIfExists(csvTestFile);
        }

        [Test]
        public void LoadCharEnumsTestParser()
        {
            ModelPropertySchema<WaterFlowFMPropertyDefinition> modelPropertySchema = new ModelSchemaCsvFile().ReadModelSchema<WaterFlowFMPropertyDefinition>(dflowfmPropertiesCsvFilePath, "MduGroup");
            Assert.Greater(modelPropertySchema.PropertyDefinitions.Count, 50);

            Assert.Greater(modelPropertySchema.PropertyDefinitions.Count, 50);

            WaterFlowFMPropertyDefinition tunitProperty = modelPropertySchema.PropertyDefinitions[KnownProperties.Tunit];
            Type tunitEnum = tunitProperty.DataType;
            IList<Enum> values = GetEnumValues(tunitEnum);

            Enum hourValue = values[0];

            Assert.AreEqual("H", FMParser.ToString(hourValue, tunitEnum));
            Assert.AreEqual(hourValue, FMParser.FromString("H", tunitEnum));
        }

        [Test]
        public void LoadIntEnums()
        {
            ModelPropertySchema<WaterFlowFMPropertyDefinition> modelPropertySchema = new ModelSchemaCsvFile().ReadModelSchema<WaterFlowFMPropertyDefinition>(dflowfmPropertiesCsvFilePath, "MduGroup");
            Assert.Greater(modelPropertySchema.PropertyDefinitions.Count, 50);

            WaterFlowFMPropertyDefinition convProperty = modelPropertySchema.PropertyDefinitions[KnownProperties.Conveyance2d];
            Type convEnum = convProperty.DataType;
            IList<Enum> values = GetEnumValues(convEnum);

            Assert.AreEqual("-1", values[0].GetDisplayName());
            Assert.AreEqual("R=HU", values[0].GetDescription());
            Assert.AreEqual("-1", convProperty.DefaultValueAsString);
        }

        [Test]
        public void LoadConveyance2dEnumAndVerifyThatItHasNotChanged()
        {
            //if changed check fm validationrule 'WaterFlowFMModelDefinitionValidator'
            ModelPropertySchema<WaterFlowFMPropertyDefinition> modelPropertySchema = new ModelSchemaCsvFile().ReadModelSchema<WaterFlowFMPropertyDefinition>(dflowfmPropertiesCsvFilePath, "MduGroup");

            WaterFlowFMPropertyDefinition convProperty = modelPropertySchema.PropertyDefinitions[KnownProperties.Conveyance2d];
            Type convEnum = convProperty.DataType;
            IList<Enum> values = GetEnumValues(convEnum);
            Assert.AreEqual(5, values.Count, "The enum size of " + KnownProperties.Conveyance2d + " has changed!");
            Assert.AreEqual("R=HU", values[0].GetDescription());
            Assert.AreEqual(((int) Conveyance2DType.RisHU).ToString(), values[0].GetDisplayName());
            Assert.AreEqual("R=H", values[1].GetDescription());
            Assert.AreEqual(((int) Conveyance2DType.RisH).ToString(), values[1].GetDisplayName());
            Assert.AreEqual("R=A/P", values[2].GetDescription());
            Assert.AreEqual(((int) Conveyance2DType.RisAperP).ToString(), values[2].GetDisplayName());
            Assert.AreEqual("K=analytic-1D conv", values[3].GetDescription());
            Assert.AreEqual(((int) Conveyance2DType.Kisanalytic1Dconv).ToString(), values[3].GetDisplayName());
            Assert.AreEqual("K=analytic-2D conv", values[4].GetDescription());
            Assert.AreEqual(((int) Conveyance2DType.Kisanalytic2Dconv).ToString(), values[4].GetDisplayName());
        }

        [Test]
        public void LoadIntEnumsNotStartingAtZero()
        {
            ModelPropertySchema<WaterFlowFMPropertyDefinition> modelPropertySchema = new ModelSchemaCsvFile().ReadModelSchema<WaterFlowFMPropertyDefinition>(dflowfmPropertiesCsvFilePath, "MduGroup");
            Assert.Greater(modelPropertySchema.PropertyDefinitions.Count, 50);

            WaterFlowFMPropertyDefinition convProperty = modelPropertySchema.PropertyDefinitions["icgsolver"];
            Type convEnum = convProperty.DataType;
            IList<Enum> values = GetEnumValues(convEnum);

            Assert.AreEqual("1", values[0].GetDisplayName());
            Assert.AreEqual("sobekGS_OMP", values[0].GetDescription());
            Assert.AreEqual("4", convProperty.DefaultValueAsString);
        }

        [Test]
        public void FixedWeirScheme()
        {
            ModelPropertySchema<WaterFlowFMPropertyDefinition> modelPropertySchema = new ModelSchemaCsvFile().ReadModelSchema<WaterFlowFMPropertyDefinition>(dflowfmPropertiesCsvFilePath, "MduGroup");
            Assert.Greater(modelPropertySchema.PropertyDefinitions.Count, 50);

            WaterFlowFMPropertyDefinition convProperty = modelPropertySchema.PropertyDefinitions["fixedweirscheme"];

            Assert.AreEqual("9", convProperty.MaxValueAsString);
        }

        private static IList<Enum> GetEnumValues(Type enumType)
        {
            return Enum.GetValues(enumType).OfType<Enum>().OrderBy(o => o.ToString()).ToList();
        }
    }
}