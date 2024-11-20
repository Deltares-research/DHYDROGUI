using DelftTools.Hydro;
using DeltaShell.Plugins.NetworkEditor.Gui.Layers;
using NUnit.Framework;

namespace DeltaShell.Plugins.NetworkEditor.Tests
{
    [TestFixture]
    public class AreaLayerTest
    {
        [Test]
        public void GivenAreaLayer_WhenHydroAreaIsSet_ThenLayerIsNotEnabledInLegend()
        {
            //Given/When
            var areaLayer = new HydroAreaLayer {HydroArea = new HydroArea()};

            //Then
            Assert.That(areaLayer.ShowInLegend, Is.EqualTo(false));
        }
    }
}