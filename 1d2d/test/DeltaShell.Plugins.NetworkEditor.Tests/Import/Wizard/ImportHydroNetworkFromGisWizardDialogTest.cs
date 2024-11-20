using DelftTools.TestUtils;
using DeltaShell.Plugins.NetworkEditor.Gui.Wizard;
using DeltaShell.Plugins.NetworkEditor.Import;
using NUnit.Framework;

namespace DeltaShell.Plugins.NetworkEditor.Tests.Import.Wizard
{
    [TestFixture]
    public class ImportHydroNetworkFromGisWizardDialogTest
    {
        [Test]
        [Category(TestCategory.WindowsForms)]
        public void ShowImportHydroNetworkFromGisWizardDialog()
        {
            var importer = new HydroRegionFromGisImporter();

            var wizard = new ImportHydroNetworkFromGisWizardDialog();
            wizard.Importer = importer;

            //wizard.ShowModal();

            WindowsFormsTestHelper.ShowModal(wizard);
        }
    }
}
