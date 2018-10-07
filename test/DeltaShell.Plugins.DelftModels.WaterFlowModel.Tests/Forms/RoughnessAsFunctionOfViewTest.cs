using DelftTools.Hydro;
using DelftTools.Hydro.Roughness;
using DelftTools.TestUtils;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.Gui.Forms;
using DeltaShell.Plugins.NetworkEditor.Gui.Forms.Roughness;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.Tests.Forms
{
    [TestFixture]
    public class RoughnessAsFunctionOfViewTest
    {
        
        [Test]
        [Category(TestCategory.WindowsForms)]
        public void Show()
        {
            var function = RoughnessSection.DefineFunctionOfQ();
            function[0.0, 0.0] = 1.1;
            function[0.0, 1000.0] = 2.1;
            function[0.0, 5000.0] = 3.1;
            function[0.0, 10000.0] = 2.1;

            function[2500.0, 0.0] = 11.1;
            function[2500.0, 8000.0] = 13.1;
            function[2500.0, 10000.0] = 12.1;
        
            var form = new RoughnessAsFunctionOfView("Q", "Rijn", RoughnessType.WhiteColebrook, "") {Data = function};

            WindowsFormsTestHelper.ShowModal(form);
        }

    }
}
