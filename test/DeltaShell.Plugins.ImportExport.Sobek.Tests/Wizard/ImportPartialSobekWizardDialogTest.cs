using System;
using DelftTools.TestUtils;
using DeltaShell.Plugins.ImportExport.Sobek.Wizard;
using NUnit.Framework;

namespace DeltaShell.Plugins.ImportExport.Sobek.Tests.Wizard
{
    [TestFixture]
    public class ImportPartialSobekWizardDialogTest
    {
        [Test]
        [Category(TestCategory.WindowsForms)]
        [STAThread]
        public void ShowDialogForSobekNetworkImporter()
        {
            var dialog = new ImportPartialSobekWizardDialog();
            dialog.Data = new SobekNetworkImporter();
            WindowsFormsTestHelper.ShowModal(dialog);
        }

        [Test]
        [Category(TestCategory.WindowsForms)]
        [STAThread]
        public void ShowDialogForSobekWaterFlowModel1DImporter()
        {
            var dialog = new ImportPartialSobekWizardDialog();
            dialog.Data = new SobekModelToIntegratedModelImporter();
            WindowsFormsTestHelper.ShowModal(dialog);
        }
    }
}
