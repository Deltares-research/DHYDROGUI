using DelftTools.Utils.Reflection;
using DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.DomainSpecificDataEditor.ViewModels;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.Gui.Editors.DomainSpecificDataEditor.ViewModels
{
    [TestFixture]
    public class HydroDynamicsUseParameterTypeTest
    {
        [TestCase(HydroDynamicsUseParameterType.DoNotUse, "Do not use")]
        [TestCase(HydroDynamicsUseParameterType.Use, "Use")]
        [TestCase(HydroDynamicsUseParameterType.UseExtend, "Use extend")]
        public void WindInputType_GetDescription_ReturnsCorrectDescription(HydroDynamicsUseParameterType useParameterType, string expectedDescription)
        {
            Assert.That(useParameterType.GetDescription(), Is.EqualTo(expectedDescription));
        }
    }
}