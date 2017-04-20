using DelftTools.Hydro.Structures;
using DelftTools.TestUtils;
using DeltaShell.Plugins.NetworkEditor.Gui.Forms;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using NUnit.Framework;

namespace DeltaShell.Plugins.NetworkEditor.Tests.Forms.StructureFeatureView
{
    [TestFixture]
    public class FMWeirViewTest
    {
        [Test]
        [Category(TestCategory.WindowsForms)]
        public void ShowEmpty()
        {
            var view = new FMWeirView
            {
                Data = null
            };
            WindowsFormsTestHelper.ShowModal(view);
        }

        [Test]
        [Category(TestCategory.WindowsForms)]
        public void ShowFMWeirView()
        {
            var weir = new Weir("TestWeir")
            {
                Geometry = new LineString(new[] {new Coordinate(0, 0), new Coordinate(1, 1)})
            };

            var fmWeirView = new FMWeirView
            {
                Data = weir
            };

            WindowsFormsTestHelper.ShowModal(fmWeirView);
        }

        [Test]
        [Category(TestCategory.WindowsForms)]
        public void ShowWeirWithCrestWidth()
        {
            var weir = new Weir("TestWeir")
            {
                CrestWidth = 100,
                Geometry = new LineString(new[] { new Coordinate(0, 0), new Coordinate(1, 1) })
            };

            var fmWeirView = new FMWeirView { Data = weir };

            WindowsFormsTestHelper.ShowModal(fmWeirView);
        }
    }
}
