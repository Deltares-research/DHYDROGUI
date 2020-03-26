using System;
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
            DateTime expectedReferenceTime = DateTime.Today - TimeSpan.FromDays(2);

            var waveModelDefinition = new WaveModelDefinition();
            waveModelDefinition.ModelReferenceDateTime = expectedReferenceTime;

            // Call
            var referenceTimeProvider = new ModelDefinitionReferenceDateTimeProvider(waveModelDefinition);

            // Assert
            Assert.That(referenceTimeProvider, Is.InstanceOf<IReferenceDateTimeProvider>());
            Assert.That(referenceTimeProvider.ModelReferenceDateTime, Is.EqualTo(expectedReferenceTime));
        }

        [Test]
        public void Constructor_WaveModelDefinitionNull_ThrowsArgumentNullException()
        {
            void Call() => new ModelDefinitionReferenceDateTimeProvider(null);
            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo("modelDefinition"));
        }
    }
}