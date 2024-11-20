using System;
using System.Collections.Generic;
using System.Linq;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Gui.MapLayerProviders;
using GeoAPI.Extensions.Coverages;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Tests.UI.MapLayerProviders
{
    [TestFixture]
    public class OutputCoverageGroupMapLayerDataTest
    {
        [Test]
        [TestCase(null)]
        [TestCase("")]
        [TestCase(" ")]
        public void Constructor_NameNullOrWhiteSpace_ThrowsArgumentException(string name)
        {
            // Call
            void Call() => _ = new OutputCoverageGroupMapLayerData(name, Enumerable.Empty<ICoverage>());

            // Assert
            Assert.That(Call, Throws.ArgumentException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("name"));
        }

        [Test]
        public void Constructor_CoveragesNull_ThrowsArgumentNullException()
        {
            // Call
            void Call() => _ = new OutputCoverageGroupMapLayerData("some_name", null);

            // Assert
            Assert.That(Call, Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("coverages"));
        }

        [Test]
        public void Constructor_InitializesInstanceCorrectly()
        {
            // Setup
            var coverages = new List<ICoverage>
            {
                Substitute.For<ICoverage>(),
                Substitute.For<ICoverage>(),
                Substitute.For<ICoverage>()
            };

            // Call
            var data = new OutputCoverageGroupMapLayerData("some_name", coverages);

            // Assert
            Assert.That(data.Name, Is.EqualTo("some_name"));
            Assert.That(data.Coverages, Is.EqualTo(coverages));
        }
    }
}