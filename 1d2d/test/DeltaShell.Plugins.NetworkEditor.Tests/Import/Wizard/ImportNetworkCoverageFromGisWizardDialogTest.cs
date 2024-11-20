using DelftTools.TestUtils;
using DeltaShell.Plugins.NetworkEditor.Gui.Wizard;
using NUnit.Framework;

namespace DeltaShell.Plugins.NetworkEditor.Tests.Import.Wizard
{
    [TestFixture]
    public class ImportNetworkCoverageFromGisWizardDialogTest
    {
        [Test]
        [Category(TestCategory.WindowsForms)]
        public void ShowDialog()
        {
            var dialog = new ImportNetworkCoverageFromGisWizardDialog();
            WindowsFormsTestHelper.ShowModal(dialog);
        }
    }
}
