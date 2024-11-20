using DelftTools.TestUtils;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.Gui.Forms.WaterQualityModelWizard;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.WaterQualityModel.Tests
{
    [TestFixture]
    [Category(TestCategory.WindowsForms)]
    internal class BoundaryWizardPageTest
    {
        [Test]
        public void BoundaryWizardConstructorSetCorrectValues()
        {
            using (var wizard = new BoundaryDataWizard())
            {
                Assert.AreEqual(wizard.Height, 700);
                Assert.AreEqual(wizard.Title, "Import a boundary data file (in CSV format)");
                Assert.AreEqual(wizard.WelcomeMessage, "This wizard will import a Boundary File.");
                Assert.AreEqual(wizard.FinishedPageMessage, "Press Finish to import your boundary data file.");
            }
        }

        [Test]
        public void CanDoMethodsReturnTrue()
        {
            using (var wizardPage = new BoundaryDataWizardPage())
            {
                Assert.IsTrue(wizardPage.CanDoNext());
                Assert.IsTrue(wizardPage.CanDoPrevious());
                Assert.IsTrue(wizardPage.CanFinish());
            }
        }
    }
}