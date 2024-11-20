using DelftTools.Utils.Reflection;
using DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.DomainSpecificDataEditor.ViewModels;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.Gui.Editors.DomainSpecificDataEditor.ViewModels
{
    [TestFixture]
    public class WindInputTypeTest
    {
        [TestCase(WindInputType.SpiderWebGrid, "Spiderweb grid")]
        [TestCase(WindInputType.WindVector, "Wind vector")]
        [TestCase(WindInputType.XYComponents, "XY components")]
        public void WindInputType_GetDescription_ReturnsCorrectDescription(WindInputType windInputType, string expectedDescription)
        {
            Assert.That(windInputType.GetDescription(), Is.EqualTo(expectedDescription));
        }
    }
}