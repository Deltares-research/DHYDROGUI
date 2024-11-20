using DelftTools.Controls.Swf;
using DelftTools.Shell.Gui;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.Gui.Properties;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.IO;

namespace DeltaShell.Plugins.DelftModels.WaterQualityModel.Gui.Forms.WaterQualityModelWizard
{
    public class SubstanceProcessLibraryWizard : WizardDialog, IConfigureDialog
    {
        private readonly SubstanceProcessLibraryWizardPage substanceProcessLibraryWizardPage;
        private SubFileImporter subFileItemFileImporter;

        public SubstanceProcessLibraryWizard()
        {
            Height = 700;
            Title = Resources.SubstanceProcessLibraryWizard_Title;
            WelcomeMessage = Resources.SubstanceProcessLibraryWizard_Welcome_message;
            FinishedPageMessage = Resources.SubstanceProcessLibraryWizard_Finished_message;

            substanceProcessLibraryWizardPage = new SubstanceProcessLibraryWizardPage();
            AddPage(substanceProcessLibraryWizardPage, Resources.SubstanceProcessLibraryWizard_Title,
                    Resources.SubstanceProcessLibraryWizardPage_Description);
        }

        public WaterQualityModel WaterQualityModel { get; set; }

        public void Configure(object item)
        {
            subFileItemFileImporter = (SubFileImporter) item;

            subFileItemFileImporter.DefaultFilePath = substanceProcessLibraryWizardPage.SubFilePath;

            if (WaterQualityModel != null)
            {
                substanceProcessLibraryWizardPage.SetProcessFilesToModel(WaterQualityModel.SubstanceProcessLibrary);
            }
        }
    }
}