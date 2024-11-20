using DelftTools.TestUtils;
using DeltaShell.Plugins.DelftModels.HydroModel;
using DeltaShell.Plugins.ImportExport.Sobek.Wizard;
using NUnit.Framework;

namespace DeltaShell.Plugins.ImportExport.Sobek.Tests.Wizard
{
    [TestFixture]
    public class ImportSobekHydroModelWizardDialogTest
    {
        [Test]
        [Category(TestCategory.Slow)]
        public void Show()
        {
            WindowsFormsTestHelper.ShowModal(new ImportSobekHydroModelWizardDialog
                {
                    Data = new SobekHydroModelImporter
                        {
                            TargetObject = new HydroModel()
                        }
                });
        }
    }
}