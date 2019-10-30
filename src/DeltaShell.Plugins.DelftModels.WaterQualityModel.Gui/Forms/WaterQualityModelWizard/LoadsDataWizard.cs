using DelftTools.Controls.Swf;
using DelftTools.Shell.Gui;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.Gui.Properties;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.IO;

namespace DeltaShell.Plugins.DelftModels.WaterQualityModel.Gui.Forms.WaterQualityModelWizard
{
    internal class LoadsDataWizard : WizardDialog, IConfigureDialog
    {
        private readonly LoadsDataWizardPage loadsDataWizardPage;

        public LoadsDataWizard()
        {
            Height = 700;
            Title = Resources.LoadsDataWizard_Title;
            WelcomeMessage = Resources.LoadsDataWizard_Welcome_message;
            FinishedPageMessage = Resources.LoadsDataWizard_Finished_message;

            loadsDataWizardPage = new LoadsDataWizardPage();
            AddPage(loadsDataWizardPage, Resources.LoadsDataWizard_Title,
                    Resources.LoadsDataWizardPage_Description);
        }

        public void Configure(object model)
        {
            var importer = (LoadsDataTableImporter) model;
            importer.FilePath = loadsDataWizardPage.CsvLoadsPath;
        }
    }
}
