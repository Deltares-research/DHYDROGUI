using DelftTools.Utils.Reflection;
using DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.DomainSpecificDataEditor.ViewModels;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.Gui.Editors.DomainSpecificDataEditor.ViewModels
{
    [TestFixture]
    public class VelocityTypeTest
    {
        [TestCase(VelocityType.WaveDependent, "Wave dependent")]
        [TestCase(VelocityType.SurfaceLevel, "Surface level")]
        [TestCase(VelocityType.DepthAveraged, "Depth averaged")]
        public void WindInputType_GetDescription_ReturnsCorrectDescription(VelocityType velocityType, string expectedDescription)
        {
            Assert.That(velocityType.GetDescription(), Is.EqualTo(expectedDescription));
        }
    }
}