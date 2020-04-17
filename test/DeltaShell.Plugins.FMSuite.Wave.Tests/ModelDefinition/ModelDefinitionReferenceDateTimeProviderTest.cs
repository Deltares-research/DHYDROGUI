using System;
using DelftTools.TestUtils;
using DeltaShell.NGHS.TestUtils;
using DeltaShell.Plugins.FMSuite.Wave.ModelDefinition;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.ModelDefinition
{
    [TestFixture]
    public class ModelDefinitionReferenceDateTimeProviderTest
    {
        private readonly Random random = new Random();

        [Test]
        public void Constructor_ExpectedResults()
        {
            // Setup
            DateTime expectedReferenceTime = random.NextDateTime();

            var waveModelDefinition = new WaveModelDefinition();
            waveModelDefinition.ModelReferenceDateTime = expectedReferenceTime;

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
            DateTime initialDateTime = random.NextDateTime();
            var waveModelDefinition = new WaveModelDefinition {ModelReferenceDateTime = initialDateTime};
            var referenceTimeProvider = new ModelDefinitionReferenceDateTimeProvider(waveModelDefinition);

            DateTime newDateTime = random.NextDateTime();

            // Precondition
            Assert.That(referenceTimeProvider.ModelReferenceDateTime, Is.EqualTo(initialDateTime));

            // When
            waveModelDefinition.ModelReferenceDateTime = newDateTime;

            // Assert
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