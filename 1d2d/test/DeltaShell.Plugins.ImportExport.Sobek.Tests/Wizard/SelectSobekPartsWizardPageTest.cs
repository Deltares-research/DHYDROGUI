using DelftTools.Hydro;
using DelftTools.TestUtils;
using DeltaShell.Plugins.ImportExport.Sobek.PartialSobekImporter;
using DeltaShell.Plugins.ImportExport.Sobek.Wizard;
using NUnit.Framework;

namespace DeltaShell.Plugins.ImportExport.Sobek.Tests.Wizard
{
    [TestFixture]
    public class SelectSobekPartsWizardPageTest
    {
        [Test]
        [Category(TestCategory.WindowsForms)]
        public void ShowPage()
        {
            var page = new SelectSobekPartsWizardPage();
            var importer = PartialSobekImporterBuilder.BuildPartialSobekImporter("C://network.tp", new HydroNetwork(), new IPartialSobekImporter[] { new SobekBranchesImporter(), new SobekCrossSectionsImporter() });

            page.PartialSobekImporter = importer;

            WindowsFormsTestHelper.ShowModal(page);
        }
    }
}
