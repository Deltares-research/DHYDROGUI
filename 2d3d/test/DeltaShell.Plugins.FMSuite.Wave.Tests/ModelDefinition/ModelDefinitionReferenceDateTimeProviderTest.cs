using System;
using DelftTools.TestUtils;
using DeltaShell.Plugins.FMSuite.Wave.ModelDefinition;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.ModelDefinition
{
    [TestFixture]
    public class ModelDefinitionReferenceDateTimeProviderTest
    {
        [Test]
        public void Constructor_ExpectedResults()
        {
            // Setup
            var expectedReferenceTime = new DateTime(2023, 6, 30);

            var waveModelDefinition = new WaveModelDefinition { ModelReferenceDateTime = expectedReferenceTime };

            // Call
            var referenceTimeProvider = new ModelDefinitionReferenceDateTimeProvider(waveModelDefinition);

            // Assert
            Assert.That(referenceTimeProvider, Is.InstanceOf<IReferenceDateTimeProvider>());
            Assert.That(referenceTimeProvider.ModelReferenceDateTime, Is.EqualTo(expectedReferenceTime));
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void GivenAModelDefinitionReferenceDateTimeProvider_WhenTheDateIsChangedInTheModelDefinition_ThenTheTimeProviderReturnsTheNewValue()
        {
            // Given
            var initialDateTime = new DateTime(2023, 6, 29);
            var waveModelDefinition = new WaveModelDefinition {ModelReferenceDateTime = initialDateTime};
            var referenceTimeProvider = new ModelDefinitionReferenceDateTimeProvider(waveModelDefinition);

            var newDateTime = new DateTime(2023, 6, 30);

            // Precondition
            Assert.That(referenceTimeProvider.ModelReferenceDateTime, Is.EqualTo(initialDateTime));

            // When
            waveModelDefinition.ModelReferenceDateTime = newDateTime;

            // Then
            Assert.That(referenceTimeProvider.ModelReferenceDateTime, Is.EqualTo(newDateTime));
        }

        [Test]
        public void Constructor_WaveModelDefinitionNull_ThrowsArgumentNullException()
        {
            // Call | Assert
            void Call() => new ModelDefinitionReferenceDateTimeProvider(null);
            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo("modelDefinition"));
        }
    }
}