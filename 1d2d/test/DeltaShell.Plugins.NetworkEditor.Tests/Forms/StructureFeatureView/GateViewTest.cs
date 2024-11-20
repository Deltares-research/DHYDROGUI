using DelftTools.Hydro.Structures;
using DelftTools.TestUtils;
using DeltaShell.Plugins.NetworkEditor.Gui.Forms.StructureFeatureView;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using NUnit.Framework;

namespace DeltaShell.Plugins.NetworkEditor.Tests.Forms.StructureFeatureView
{
    [TestFixture]
    public class GateViewTest
    {
        [Test]
        [Category(TestCategory.WindowsForms)]
        public void ShowEmpty()
        {
            var view = new GateView
                {
                    Data = null
                };
            WindowsFormsTestHelper.ShowModal(view);
        }

        [Test]
        [Category(TestCategory.WindowsForms)]
        public void ShowGateView()
        {
            var gate = new Gate("TestGate")
            {
                Geometry = new LineString(new[] {new Coordinate(0, 0), new Coordinate(1, 1)})
            };

            var gateView = new GateView
                {
                    Data = gate
                };

            WindowsFormsTestHelper.ShowModal(gateView);
        }

        [Test]
        [Category(TestCategory.WindowsForms)]
        public void ShowGateWithSillWidth()
        {
            var gate = new Gate("TestGate")
            {
                SillWidth = 100,
                Geometry = new LineString(new[] {new Coordinate(0, 0), new Coordinate(1, 1)})
            };

            var gateView = new GateView {Data = gate};

            WindowsFormsTestHelper.ShowModal(gateView);
        }
    }
}