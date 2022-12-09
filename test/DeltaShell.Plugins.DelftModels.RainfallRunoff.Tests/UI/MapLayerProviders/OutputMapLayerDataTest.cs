using System;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Gui.MapLayerProviders;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Tests.UI.MapLayerProviders
{
    [TestFixture]
    public class OutputMapLayerDataTest
    {
        [Test]
        public void Constructor_ModelNull_ThrowsArgumentNullException()
        {
            // Call
            void Call() => _ = new OutputMapLayerData(null);

            // Assert
            Assert.That(Call, Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("model"));
        }

        [Test]
        public void Constructor_InitializesInstanceCorrectly()
        {
            // Setup
            var model = new RainfallRunoffModel();

            // Call
            var data = new OutputMapLayerData(model);

            // Assert
            Assert.That(data.Model, Is.SameAs(model));
            Assert.That(data.Name, Is.EqualTo("Output"));
        }
    }
}