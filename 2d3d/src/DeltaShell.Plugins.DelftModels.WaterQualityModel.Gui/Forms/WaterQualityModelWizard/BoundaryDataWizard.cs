using DelftTools.Controls.Swf;
using DelftTools.Shell.Gui;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.Gui.Properties;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.IO;

namespace DeltaShell.Plugins.DelftModels.WaterQualityModel.Gui.Forms.WaterQualityModelWizard
{
    /// <summary>
    /// Wizard for importing boundary data from a csv file.
    /// </summary>
    internal class BoundaryDataWizard : WizardDialog, IConfigureDialog
    {
        private readonly BoundaryDataWizardPage boundaryDataWizardPage;

        /// <summary>
        /// Creates a new instance of <see cref="BoundaryDataWizard"/>.
        /// </summary>
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

        public void Configure(object item)
        {
            var importer = (BoundaryDataTableImporter) item;
            importer.FilePath = boundaryDataWizardPage.CsvFilePath;
        }
    }
}