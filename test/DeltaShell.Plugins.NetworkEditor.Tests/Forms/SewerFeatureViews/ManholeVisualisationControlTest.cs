using System.Collections.Generic;
using DelftTools.Hydro;
using DelftTools.Hydro.Structures;
using DelftTools.TestUtils;
using DeltaShell.Plugins.NetworkEditor.Gui.Forms.SewerFeatureViews;
using NUnit.Framework;

namespace DeltaShell.Plugins.NetworkEditor.Tests.Forms.SewerFeatureViews
{
    [TestFixture]
    public class ManholeVisualisationControlTest
    {
        [Test]
        [Category(TestCategory.WindowsForms)]
        public void Test()
        {
            var compartment1 = new Compartment("Compartment 1") { BottomLevel = -10, SurfaceLevel = 20, ManholeWidth = 50 };
            var compartment2 = new Compartment("Compartment 2") { BottomLevel = -12, SurfaceLevel = 21, ManholeWidth = 31 };

            var manhole = new Manhole("manhole 1");

            manhole.Compartments.AddRange(new List<Compartment>
            {
                compartment1,
                compartment2,
            });
            var view = new ManholeVisualisationControl { Manhole = manhole };
            WpfTestHelper.ShowModal(view);
        }
    }
}