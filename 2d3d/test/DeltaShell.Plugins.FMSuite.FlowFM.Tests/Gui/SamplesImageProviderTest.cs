using System.Collections.Generic;
using System.Drawing;
using DelftTools.Utils.Drawing;
using DeltaShell.Plugins.FMSuite.FlowFM.Gui;
using DeltaShell.Plugins.FMSuite.FlowFM.Gui.Properties;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.Gui
{
    [TestFixture]
    public class SamplesImageProviderTest
    {
        [Test]
        public void GetImage_SamplesNull_ThrowsException()
        {
            // Setup
            var provider = new SamplesImageProvider();

            // Call
            void Call() => provider.GetImage(null);

            // Assert
            Assert.That(Call, Throws.ArgumentNullException);
        }

        [Test]
        public void GetImage_UnknownSamplesName_ThrowsException()
        {
            // Setup
            var provider = new SamplesImageProvider();
            var samples = new Samples("Unknown Samples Name");

            // Call
            void Call() => provider.GetImage(samples);

            // Assert
            Assert.That(Call, Throws.ArgumentException);
        }

        [Test]
        [TestCaseSource(nameof(GetGetImageTestCases))]
        public void GetImage_ReturnsExpectedImage(string samplesName, Image expectedImage)
        {
            // Setup
            var provider = new SamplesImageProvider();
            var samples = new Samples(samplesName);

            // Call
            Image image = provider.GetImage(samples);

            // Assert
            Assert.That(image.PixelsEqual(expectedImage), Is.True);
        }

        private static IEnumerable<TestCaseData> GetGetImageTestCases()
        {
            yield return new TestCaseData(WaterFlowFMModelDefinition.InitialVelocityXName, Resources.velocity_x);
            yield return new TestCaseData(WaterFlowFMModelDefinition.InitialVelocityYName, Resources.velocity_y);
        }
    }
}