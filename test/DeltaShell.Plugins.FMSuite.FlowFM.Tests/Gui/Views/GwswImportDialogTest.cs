using DelftTools.TestUtils;
using DeltaShell.Plugins.FMSuite.FlowFM.Gui.Forms;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Importers;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.Gui
{
    [TestFixture]
    public class GwswImportDialogTest
    {
        [Category(TestCategory.WindowsForms)]
        [Test]
        public void ShowUserControl()
        {
            var dialog = new GwswImportDialog();
            dialog.Data = new GwswFileImporter();
            WpfTestHelper.ShowModal(dialog);
        }
    }
}