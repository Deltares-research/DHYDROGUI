using DelftTools.TestUtils;
using DeltaShell.Plugins.FMSuite.FlowFM.Gui.Forms;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.Gui
{
    [TestFixture]
    [Category(TestCategory.WindowsForms)]
    public class BcFileExportDialogTest
    {
        [Test]
        public void ShowDialog()
        {
            var form = new BcFileExportDialog();
            WindowsFormsTestHelper.ShowModal(form);
        }
    }
}