using DelftTools.Controls.Swf;
using DelftTools.Shell.Gui;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.Gui.Properties;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.IO;

namespace DeltaShell.Plugins.DelftModels.WaterQualityModel.Gui.Forms.WaterQualityModelWizard
{
    internal class BoundaryDataWizard : WizardDialog, IConfigureDialog
    {
        private readonly BoundaryDataWizardPage boundaryDataWizardPage;

        public BoundaryDataWizard()
        {
            Height = 700;
            Title = Resources.BoundaryDataWizard_Title;
            WelcomeMessage = Resources.BoundaryDataWizard_Welcome_message;
            FinishedPageMessage = Resources.BoundaryDataWizard_Finished_message;

            boundaryDataWizardPage = new BoundaryDataWizardPage();
            AddPage(boundaryDataWizardPage, Resources.BoundaryDataWizard_Title,
                    Resources.BoundaryDataWizardPage_Description);
        }

        public void Configure(object model)
        {
            var importer = (BoundaryDataTableImporter) model;
            importer.FilePath = boundaryDataWizardPage.CsvBoundaryPath;
        }
    }
}
