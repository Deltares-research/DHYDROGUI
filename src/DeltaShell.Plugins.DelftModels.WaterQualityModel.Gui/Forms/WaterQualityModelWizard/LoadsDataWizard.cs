using DelftTools.Controls.Swf;
using DelftTools.Shell.Gui;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.Gui.Properties;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.IO;

namespace DeltaShell.Plugins.DelftModels.WaterQualityModel.Gui.Forms.WaterQualityModelWizard
{
    class LoadsDataWizard: WizardDialog, IConfigureDialog
    {
        private LoadsDataTableImporter loadsDataItemFileImporter;
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

        public void Configure(object targetItemFileImporter)
        {
            loadsDataItemFileImporter = (LoadsDataTableImporter)targetItemFileImporter;
            loadsDataItemFileImporter.FilePath = loadsDataWizardPage.CsvLoadsPath;
        }
    }
}
