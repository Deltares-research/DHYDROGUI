using DelftTools.TestUtils;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.Gui.Forms.WaterQualityModelWizard;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.WaterQualityModel.Tests
{
    [TestFixture]
    [Category(TestCategory.WindowsForms)]
    class LoadsWizardPageTest
    {
        [Test]
        public void BoundaryWizardConstructorSetCorrectValues()
        {
            using (LoadsDataWizard wizard = new LoadsDataWizard())
            {
                Assert.AreEqual(wizard.Height, 700);
                Assert.AreEqual(wizard.Title, "Import a loads data file (in CSV format)");
                Assert.AreEqual(wizard.WelcomeMessage, "This wizard will import a Loads File.");
                Assert.AreEqual(wizard.FinishedPageMessage,
                                "Press Finish to import your loads data file.");
            }
        }

        [Test]
        public void CanDoMethodsReturnTrue()
        {
            using (LoadsDataWizardPage wizardPage = new LoadsDataWizardPage())
            {
                Assert.IsTrue(wizardPage.CanDoNext());
                Assert.IsTrue(wizardPage.CanDoPrevious());
                Assert.IsTrue(wizardPage.CanFinish());
            }
        }

    }
}

