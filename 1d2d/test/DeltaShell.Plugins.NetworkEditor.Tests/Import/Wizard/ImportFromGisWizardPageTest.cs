using DelftTools.TestUtils;
using DeltaShell.Plugins.NetworkEditor.Gui.Wizard;
using DeltaShell.Plugins.NetworkEditor.Import;
using NUnit.Framework;

namespace DeltaShell.Plugins.NetworkEditor.Tests.Import.Wizard
{
    [TestFixture]
    public class ImportFromGisWizardPageTest
    {
        [Test]
        [Category(TestCategory.WindowsForms)]
        public void ShowImportFromGisWizardPage()
        {
            var importer = new HydroRegionFromGisImporter();

            var page = new ImportFromGisWizardPage();
            page.HydroRegionFromGisImporter = importer;

            //var form = new Form();
            //form.Size = new Size(750,500);
            //form.Controls.Add(page);
            //form.ShowDialog();

            WindowsFormsTestHelper.ShowModal(page);
        }
    }
}
