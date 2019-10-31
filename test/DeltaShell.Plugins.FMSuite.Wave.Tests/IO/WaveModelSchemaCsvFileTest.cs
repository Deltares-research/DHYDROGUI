using System;
using System.IO;
using System.Linq;
using DelftTools.TestUtils;
using DelftTools.Utils.IO;
using DeltaShell.Plugins.FMSuite.Common.IO.Files;
using DeltaShell.Plugins.FMSuite.Wave.ModelDefinition;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.IO
{
    [TestFixture]
    public class WaveModelSchemaCsvFileTest
    {
        [Test]
        [Category(TestCategory.DataAccess)]
        public void Read_WaveModelSchema_DoesNotSplitCommentWithCommas()
        {
            //01. Load test file
            var csvTestFile = TestHelper.GetTestFilePath(@"CsvFile\properties_test.csv");
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
            var modelPropertySchema = new ModelSchemaCsvFile().ReadModelSchema<WaveModelPropertyDefinition>(csvTestFile, "MdwGroup");

            //05. Find property, and check description.
            var testProp = modelPropertySchema.PropertyDefinitions.FirstOrDefault(pd => pd.Value.FilePropertyName == testPropName);
            Assert.IsNotNull(testProp);
            Assert.AreEqual(expectedDescription, testProp.Value.Description);
            Assert.AreEqual(expectedUnit, testProp.Value.Unit);

            //Remove test data
            FileUtils.DeleteIfExists(csvTestFile);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void Read_WaveModelSchema_LoadsUnits()
        {
            //01. Load test file
            var csvTestFile = TestHelper.GetTestFilePath(@"CsvFile\properties_test.csv");
            Assert.IsNotNull(csvTestFile);
            Assert.IsTrue(File.Exists(csvTestFile));
            //02. Create local copy
            csvTestFile = TestHelper.CreateLocalCopySingleFile(csvTestFile);
            Assert.IsTrue(File.Exists(csvTestFile));

            //03. Load CSV into properties.
            var modelPropertySchema = new ModelSchemaCsvFile().ReadModelSchema<WaveModelPropertyDefinition>(csvTestFile, "MdwGroup");

            //04. Check units have been loaded
            Assert.IsTrue(modelPropertySchema.PropertyDefinitions.Any(pd => !string.IsNullOrEmpty(pd.Value.Unit)));
            Assert.IsTrue(modelPropertySchema.PropertyDefinitions.Any(pd => pd.Value.Unit.Equals("dummyUnit")));

            //Remove test data
            FileUtils.DeleteIfExists(csvTestFile);
        }


        [Test]
        [Category(TestCategory.DataAccess)]
        public void GivenADwavePropertiesCsvWithPropertiesUsingMultipleDefaultValues_WhenReadingThisFile_ThenThePropertyDefinitionShouldBeSetCorrectly()
        {
            var modelPropertySchema =
                new ModelSchemaCsvFile().ReadModelSchema<WaveModelPropertyDefinition>(
                    "plugins\\DeltaShell.Plugins.FMSuite.Wave\\dwave-properties.csv", "MdwGroup");
            Assert.AreEqual(75, modelPropertySchema.PropertyDefinitions.Count);
            Assert.AreEqual(6, modelPropertySchema.ModelDefinitionCategory.Count);

            var propertyDefinitionBedFrictionCoef =
                modelPropertySchema.PropertyDefinitions.FirstOrDefault(pd =>
                    pd.Value.FilePropertyName == KnownWaveProperties.BedFrictionCoef);
            
            Assert.IsTrue(propertyDefinitionBedFrictionCoef.Value.MultipleDefaultValuesAvailable);
            Assert.AreEqual(KnownWaveProperties.BedFriction, propertyDefinitionBedFrictionCoef.Value.DefaultValueDependentOn);
            var expectedDefaultValues = new String[] {"0","0.038","0.015","0.05"};
            Assert.AreEqual(expectedDefaultValues, propertyDefinitionBedFrictionCoef.Value.MultipleDefaultValues);
            Assert.AreEqual("BedFriction:0|0.038|0.015|0.05", propertyDefinitionBedFrictionCoef.Value.DefaultValueAsString);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void GivenADwavePropertiesCsvWithPropertiesUsingOneDefaultValue_WhenReadingThisFile_ThenThePropertyDefinitionShouldBeSetCorrectly()
        {
            var modelPropertySchema =
                new ModelSchemaCsvFile().ReadModelSchema<WaveModelPropertyDefinition>(
                    "plugins\\DeltaShell.Plugins.FMSuite.Wave\\dwave-properties.csv", "MdwGroup");
            Assert.AreEqual(75, modelPropertySchema.PropertyDefinitions.Count);
            Assert.AreEqual(6, modelPropertySchema.ModelDefinitionCategory.Count);

            var propertyDefinitionBedFriction =
                modelPropertySchema.PropertyDefinitions.FirstOrDefault(pd =>
                    pd.Value.FilePropertyName == KnownWaveProperties.BedFriction);

            Assert.IsFalse(propertyDefinitionBedFriction.Value.MultipleDefaultValuesAvailable);
            Assert.IsNull(propertyDefinitionBedFriction.Value.DefaultValueDependentOn);
            Assert.IsNull(propertyDefinitionBedFriction.Value.MultipleDefaultValues);
            Assert.AreEqual("jonswap", propertyDefinitionBedFriction.Value.DefaultValueAsString);
        }

    }
}