using DelftTools.Controls.Swf;
using DelftTools.Controls.Swf.WizardPages;
using DelftTools.Shell.Core;
using DeltaShell.Plugins.ImportExport.Sobek.PartialSobekImporter;

namespace DeltaShell.Plugins.ImportExport.Sobek.Wizard
{
    /// <summary>
    /// 
    /// </summary>
    public class ImportPartialSobekWizardDialog: WizardDialog
    {
        private SelectSobekPartsWizardPage selectImportPartsWizardPage;
        private SelectFileWizardPage selectFileWizardPage;
        private IPartialSobekImporter importer;

        public ImportPartialSobekWizardDialog()
        {
            Title = "SOBEK network importer";
        }

        public override object Data
        {
            get { return Importer; }
            set
            {
                Importer = (IPartialSobekImporter)value;
            }
        }

        public IPartialSobekImporter Importer
        {
            get { return importer; }
            set
            {
                importer = value;
                ConfigureWizard();
            }
        }

        private void ConfigureWizard()
        {
            string selectText = "Select SOBEK case or network file";

            selectFileWizardPage = new SobekModelSelectFileWizardPage();
            selectImportPartsWizardPage = new SelectSobekPartsWizardPage();

            AddPage(selectFileWizardPage, selectText, "");
            AddPage(selectImportPartsWizardPage, "Select SOBEK model parts to import", "Use the checkboxes to select");

            if (importer is IFileImporter)
            {
                selectFileWizardPage.Filter = ((IFileImporter)importer).FileFilter;
            }

            WelcomePageVisible = false;
            CompletionPageVisible = false;

        }

        protected override void OnPageCompleted(IWizardPage page)
        {
            if (page == selectFileWizardPage)
            {
                importer.PathSobek = selectFileWizardPage.FileName;
                selectImportPartsWizardPage.PartialSobekImporter = importer;
                base.OnPageCompleted(page);
            }
        }
    }
}