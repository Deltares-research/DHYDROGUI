using System.IO;
using DelftTools.TestUtils;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.Gui.Forms.WaterQualityModelWizard;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.WaterQualityModel.Tests.Forms.WaterQualityModel1DWizard
{
    [TestFixture]
    [Category(TestCategory.WindowsForms)]
    public class SubstanceProcessLibraryWizardPageTest
    {
        [Test]
        public void ShowSubstanceProcessLibraryWizardPageWithExistingDataDirectory()
        {
            WindowsFormsTestHelper.ShowModal(new SubstanceProcessLibraryWizardPage(Path.Combine(TestHelper.GetTestDataDirectory(), "Data")));
        }

        [Test]
        public void ShowSubstanceProcessLibraryWizardPageWithNonExistingDataDirectory()
        {
            WindowsFormsTestHelper.ShowModal(new SubstanceProcessLibraryWizardPage(Path.Combine(TestHelper.GetTestDataDirectory(), "NonExist")));
        }
    }
}