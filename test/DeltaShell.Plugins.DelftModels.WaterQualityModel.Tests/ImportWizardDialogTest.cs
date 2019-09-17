using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DelftTools.TestUtils;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.Gui.Forms.WaterQualityModelWizard;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.WaterQualityModel.Tests
{
    [TestFixture]
    [Category(TestCategory.WindowsForms)]
    class ImportWizardDialogTest
    {

        [TestFixture]
        [Category(TestCategory.WindowsForms)]
        public class SubstanceProcessLibraryWizardPageTest
        {
            [Test]
            public void ShowBoundaryWizardPageWithExistingDataDirectory()
            {
                WindowsFormsTestHelper.ShowModal(new BoundaryDataWizardPage(Path.Combine(TestHelper.GetTestDataDirectory(), "Data")));
            }

            [Test]
            public void ShowBoundaryWizardPageWithNonExistingDataDirectory()
            {
                WindowsFormsTestHelper.ShowModal(new BoundaryDataWizardPage(Path.Combine(TestHelper.GetTestDataDirectory(), "NonExist")));
            }
        }
    }
}
