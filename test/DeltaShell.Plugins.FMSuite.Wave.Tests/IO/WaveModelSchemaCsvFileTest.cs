using System;
using System.Linq;
using DelftTools.TestUtils;
using DeltaShell.Plugins.FMSuite.Common.IO;
using DeltaShell.Plugins.FMSuite.Wave.ModelDefinition;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.IO
{
    [TestFixture]
    public class WaveModelSchemaCsvFileTest
    {
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
            var model = new WaveModel();

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