using DelftTools.TestUtils;
using DeltaShell.Plugins.ImportExport.Sobek.Wizard;
using NUnit.Framework;

namespace DeltaShell.Plugins.ImportExport.Sobek.Tests.Wizard
{
    [TestFixture]
    public class SobekModelSelectFileWizardPageTest
    {
        [Test]
        [Category(TestCategory.WindowsForms)]
        public void Show()
        {
            var page = new SobekModelSelectFileWizardPage();
            WindowsFormsTestHelper.ShowModal(page);
        }
    }
}
