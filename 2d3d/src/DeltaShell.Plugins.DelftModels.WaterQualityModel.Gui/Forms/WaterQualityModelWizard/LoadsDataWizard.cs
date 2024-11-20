using DelftTools.Controls.Swf;
using DelftTools.Shell.Gui;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.Gui.Properties;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.IO;

namespace DeltaShell.Plugins.DelftModels.WaterQualityModel.Gui.Forms.WaterQualityModelWizard
{
    /// <summary>
    /// Wizard for importing loads data from a csv file.
    /// </summary>
    internal class LoadsDataWizard : WizardDialog, IConfigureDialog
    {
        private readonly LoadsDataWizardPage loadsDataWizardPage;

        /// <summary>
        /// Creates a new instance of <see cref="LoadsDataWizard"/>.
        /// </summary>
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

        public void Configure(object item)
        {
            var importer = (LoadsDataTableImporter) item;
            importer.FilePath = loadsDataWizardPage.CsvFilePath;
        }
    }
}