using DelftTools.Hydro;
using DelftTools.Hydro.Structures;
using DeltaShell.Plugins.NetworkEditor.Gui.Forms.PropertyGrid;
using NUnit.Framework;

namespace DeltaShell.Plugins.NetworkEditor.Tests
{
    [TestFixture]
    public class CulvertPropertiesTest
    {
        [Test]
        public void AllowNegativeFlowIsReadOnlyForSiphons()
        {
            Assert.IsTrue(new CulvertProperties
                {
                    Data = new Culvert
                        {
                            CulvertType = CulvertType.Siphon
                    }
                }.DynamicReadOnlyValidationMethod(nameof(ICulvert.AllowNegativeFlow))); // When the culvert is a siphon the allownegativeflow property is readonly
        }
    }
}