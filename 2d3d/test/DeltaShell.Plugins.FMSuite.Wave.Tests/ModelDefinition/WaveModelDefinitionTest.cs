using System.Globalization;
using DelftTools.TestUtils;
using DeltaShell.Plugins.FMSuite.Common.ModelSchema;
using DeltaShell.Plugins.FMSuite.Wave.DataAccess;
using DeltaShell.Plugins.FMSuite.Wave.ModelDefinition;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.ModelDefinition
{
    [TestFixture]
    public class WaveModelDefinitionTest
    {
        [Test]
        [Category(TestCategory.DataAccess)]
        public void ReadModelDefinitionFromMdw()
        {
            string mdwPath = TestHelper.GetTestFilePath(@"wave_timespacevarbnd\tst.mdw");
            MdwFileDTO dto = new MdwFile().Load(mdwPath);

            WaveModelDefinition modelDefinition = dto.WaveModelDefinition;

            Assert.AreEqual(6, modelDefinition.ModelSchema.GuiPropertyGroups.Count);
            Assert.AreEqual(6, modelDefinition.ModelSchema.ModelDefinitionCategory.Count);

            Assert.AreEqual("nautical",
                            modelDefinition.GetModelProperty(KnownWaveSections.GeneralSection, "DirConvention")
                                           .GetValueAsString());
        }

        [Test]
        public void GivenACsvFileWithMultipleDefaultValuesForBedFricCoefBasedOnBedFriction_WhenCreatingTheModelDefinition_ThenTheCorrectDefaultValueShouldBeSet()
        {
            var modelDefinition = new WaveModelDefinition();
            ModelPropertySchema<WaveModelPropertyDefinition> schema = modelDefinition.ModelSchema;
            WaveModelPropertyDefinition propertyDefinitionBedFrictionCoef = schema.PropertyDefinitions[KnownWaveProperties.BedFrictionCoef.ToLower()];

            WaveModelProperty propertyBedFriction = modelDefinition.GetModelProperty(KnownWaveSections.ProcessesSection, KnownWaveProperties.BedFriction);
            Assert.IsNotNull(propertyBedFriction);

            //Check DefaultValueAsString
            var valueBedFriction = (int)propertyBedFriction.Value;
            string expectedDefaultValue = propertyDefinitionBedFrictionCoef.MultipleDefaultValues[valueBedFriction];
            Assert.AreEqual(expectedDefaultValue, propertyDefinitionBedFrictionCoef.DefaultValueAsString);

            // Check value of property
            WaveModelProperty propertyBedFrictionCoef = modelDefinition.GetModelProperty(KnownWaveSections.ProcessesSection, KnownWaveProperties.BedFrictionCoef);
            Assert.IsNotNull(propertyBedFriction);
            string retrievedBedFrictionCoef = string.Format(CultureInfo.InvariantCulture, "{0}", propertyBedFrictionCoef.Value);
            Assert.AreEqual(expectedDefaultValue, retrievedBedFrictionCoef);
        }

        [Test]
        public void GivenACsvFileWithMultipleDefaultValuesForMaxIterBasedOnSimMode_WhenCreatingTheModelDefinition_ThenTheCorrectDefaultValueShouldBeSet()
        {
            var modelDefinition = new WaveModelDefinition();
            ModelPropertySchema<WaveModelPropertyDefinition> schema = modelDefinition.ModelSchema;
            WaveModelPropertyDefinition propertyDefinitionMaxIter = schema.PropertyDefinitions[KnownWaveProperties.MaxIter.ToLower()];

            WaveModelProperty propertySimMode = modelDefinition.GetModelProperty(KnownWaveSections.GeneralSection, KnownWaveProperties.SimulationMode);
            Assert.IsNotNull(propertySimMode);

            //Check DefaultValueAsString
            var valueBedFriction = (int)propertySimMode.Value;
            string expectedDefaultValue = propertyDefinitionMaxIter.MultipleDefaultValues[valueBedFriction];
            Assert.AreEqual(expectedDefaultValue, propertyDefinitionMaxIter.DefaultValueAsString);

            // Check value of property
            WaveModelProperty propertyMaxIter = modelDefinition.GetModelProperty(KnownWaveSections.NumericsSection, KnownWaveProperties.MaxIter);
            Assert.IsNotNull(propertySimMode);
            string retrievedMaxIter = string.Format(CultureInfo.InvariantCulture, "{0}", propertyMaxIter.Value);
            Assert.AreEqual(expectedDefaultValue, retrievedMaxIter);
        }

        [Test]
        public void GivenWaveModelDefinition_WhenConstructorFinished_ThenPropertiesCorrectlySet()
        {
            // Given
            WaveModelDefinition Call() => new WaveModelDefinition();

            // When
            WaveModelDefinition waveModelDefinition = Call();

            // Then
            Assert.That(waveModelDefinition.Properties, Is.Not.Empty);
            Assert.That(waveModelDefinition.FeatureContainer, Is.Not.Null);
            Assert.That(waveModelDefinition.BoundaryContainer, Is.Not.Null);
        }

        [Test]
        public void SetInputTemplateFilePath_SetsTheValueOfTheInputTemplateFileProperty()
        {
            // Setup
            var waveModelDefinition = new WaveModelDefinition();
            const string value = "some_path";

            // Call
            waveModelDefinition.InputTemplateFilePath = value;

            // Assert
            WaveModelProperty property = waveModelDefinition.GetModelProperty("General", "INPUTTemplateFile");
            Assert.That(property.Value, Is.EqualTo(value));
        }

        [Test]
        public void GetInputTemplateFilePath_GetsTheValueOfTheInputTemplateFileProperty()
        {
            // Setup
            var waveModelDefinition = new WaveModelDefinition();
            const string value = "some_path";
            WaveModelProperty property = waveModelDefinition.GetModelProperty("General", "INPUTTemplateFile");
            property.Value = value;
            
            // Call
            string result = waveModelDefinition.InputTemplateFilePath;

            // Assert
            Assert.That(result, Is.EqualTo(value));
        }
    }
}