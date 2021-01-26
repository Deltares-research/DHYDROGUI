using System;
using DeltaShell.Plugins.FMSuite.Wave.Validation;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.Validation
{
    [TestFixture]
    public class DomainSpecificValidationShortcutTest
    {
        [Test]
        public void Constructor_WaveModelNull_ThrowsArgumentNullException()
        {
            // Setup
            var domainData = Substitute.For<IWaveDomainData>();

            // Call
            TestDelegate call = () => new DomainSpecificValidationShortcut(null, domainData);

            // Assert
            Assert.That(call, Throws.TypeOf<ArgumentNullException>()
                                    .With.Property(nameof(ArgumentNullException.ParamName))
                                    .EqualTo("waveModel"));
        }

        [Test]
        public void Constructor_DomainDataNull_ThrowsArgumentNullException()
        {
            // Setup
            using (var model = new WaveModel())
            {
                // Call
                TestDelegate call = () => new DomainSpecificValidationShortcut(model, null);

                // Assert
                Assert.That(call, Throws.TypeOf<ArgumentNullException>()
                                        .With.Property(nameof(ArgumentNullException.ParamName))
                                        .EqualTo("selectedDomainData"));
            }
        }
        
        [Test]
        public void Constructor_WithArguments_ExpectedValues()
        {
            // Setup
            var domainData = Substitute.For<IWaveDomainData>();
            using (var model = new WaveModel())
            {
                // Call
                var shortcut = new DomainSpecificValidationShortcut(model, domainData);

                // Assert
                Assert.That(shortcut.WaveModel, Is.SameAs(model));
                Assert.That(shortcut.SelectedDomainData, Is.SameAs(domainData));
            }
        }

        [Test]
        public void TabName_Always_ReturnsExpectedValue()
        {
            // Setup
            var domainData = Substitute.For<IWaveDomainData>();
            using (var model = new WaveModel())
            {
                var shortcut = new DomainSpecificValidationShortcut(model, domainData);

                // Call
                string tabName = shortcut.TabName;

                // Assert
                const string expectedTabName = "Domain specific settings";
                Assert.That(tabName, Is.EqualTo(expectedTabName));
            }
        }
    }
}