using DelftTools.TestUtils;
using DeltaShell.Plugins.NetworkEditor.Gui.Wizard;
using DeltaShell.Plugins.NetworkEditor.Import;
using NUnit.Framework;

namespace DeltaShell.Plugins.NetworkEditor.Tests.Import.Wizard
{
    [TestFixture]
    public class SelectDataWizardPageTest
    {
        [Test]
        [Category(TestCategory.WindowsForms)]
        public void ShowSelectDataWizardPage()
        {
            var importer = new HydroRegionFromGisImporter();

            var page = new SelectDataWizardPage();
            page.HydroRegionFromGisImporter = importer;

            //var form = new Form();
            //form.Size = new Size(750, 500);
            //form.Controls.Add(page);
            //form.ShowDialog();

            WindowsFormsTestHelper.ShowModal(page);
        }
    }
}
