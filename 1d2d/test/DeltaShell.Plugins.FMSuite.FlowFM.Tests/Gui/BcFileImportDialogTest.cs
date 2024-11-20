using DelftTools.TestUtils;
using DeltaShell.Plugins.FMSuite.FlowFM.Gui.Forms;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.Gui
{
    [TestFixture]
    [Category(TestCategory.WindowsForms)]
    public class BcFileImportDialogTest
    {
        [Test]
        public void ShowDialog()
        {
            var form = new BcFileImportDialog();
            WindowsFormsTestHelper.ShowModal(form);
        }
    }
}
