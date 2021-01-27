using DelftTools.Hydro.Area.Objects;
using DelftTools.Hydro.Area.Objects.StructuresObjects;
using DelftTools.TestUtils;
using DeltaShell.Plugins.NetworkEditor.Gui.Forms.StructureFeatureView;
using NUnit.Framework;

namespace DeltaShell.Plugins.NetworkEditor.Tests.Forms.StructureFeatureView
{
    [TestFixture]
    public class PumpViewTest
    {
        [Test]
        [Category(TestCategory.WindowsForms)]
        public void ShowPumpView()
        {
            var pump = new Pump();
            var pumpView = new PumpView {Data = pump};
            WindowsFormsTestHelper.ShowModal(pumpView);
        }
    }
}