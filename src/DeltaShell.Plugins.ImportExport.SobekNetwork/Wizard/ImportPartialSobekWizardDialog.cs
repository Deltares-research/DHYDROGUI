using DelftTools.Controls.Swf;
using DelftTools.Controls.Swf.WizardPages;
using DelftTools.Shell.Core;

namespace DeltaShell.Plugins.ImportExport.SobekNetwork.Wizard
{
    /// <summary>
    /// 
    /// </summary>
    public class ImportPartialSobekWizardDialog: WizardDialog
    {
        private SelectSobekPartsWizardPage selectHydroNetworkPartsWizardPage;
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
            selectHydroNetworkPartsWizardPage = new SelectSobekPartsWizardPage();

            AddPage(selectFileWizardPage, selectText, "");
            AddPage(selectHydroNetworkPartsWizardPage, "Select SOBEK model parts to import", "Use the checkboxes to select");

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
                selectHydroNetworkPartsWizardPage.PartialSobekImporter = importer;
            }
            base.OnPageCompleted(page);
        }
    }
}