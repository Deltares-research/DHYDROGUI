using DelftTools.TestUtils;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Gui.Importers;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Tests.Domain.Concepts.Polder.Wizard
{
    [TestFixture]
    [Category(TestCategory.WindowsForms)]
    public class ImportPolderWizardDialogTest
    {
        [Test]
        public void ShowWizard()
        {
            var wizard = new ImportPolderWizardDialog();

            WindowsFormsTestHelper.ShowModal(wizard);
        }
    }
}
