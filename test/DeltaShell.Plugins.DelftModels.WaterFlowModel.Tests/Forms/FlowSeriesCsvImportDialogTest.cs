using DelftTools.TestUtils;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.Gui.Forms;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.Tests.Forms
{
    [TestFixture]
    public class FlowSeriesCsvImportDialogTest
    {
        [Test]
        [Category(TestCategory.WindowsForms)]
        public void ShowDialogTest()
        {
            var dialog = new FlowTimeSeriesCsvImportDialog();
            WindowsFormsTestHelper.ShowModal(dialog);
        }
    }
}
