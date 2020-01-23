using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DelftTools.Controls.Swf;
using DelftTools.Controls.Swf.WizardPages;

namespace DeltaShell.Plugins.ImportExport.Sobek.Wizard
{
    public class ImportSobekWaterFlowFMWizardDialog : WizardDialog
    { 
        private SelectFileWizardPage selectFileWizardPage;
        private SelectSobekPartsWizardPage selectSobekPartsWizardPage;
        private SobekModelToWaterFlowFMImporter importer;

        public ImportSobekWaterFlowFMWizardDialog()
        {
            Title = "SOBEK WaterFlow FM importer";
        }

        public override object Data
        {
            get { return Importer; }
            set
            {
                Importer = (SobekModelToWaterFlowFMImporter)value;
            }
        }

        public SobekModelToWaterFlowFMImporter Importer
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
            selectSobekPartsWizardPage = new SelectSobekPartsWizardPage();

            AddPage(selectFileWizardPage, selectText, "");
            AddPage(selectSobekPartsWizardPage, "Select parts to import", "Use the checkboxes to select");

            WelcomePageVisible = false;
            CompletionPageVisible = false;
        }

        protected override void OnPageCompleted(IWizardPage page)
        {
            if (page == selectFileWizardPage)
            {
                importer.PathSobek = selectFileWizardPage.FileName;
                selectSobekPartsWizardPage.PartialSobekImporter = importer;

                if (selectFileWizardPage.FileName.ToLower().EndsWith("network.tp"))
                {
                    //string dir = Path.GetDirectoryName(selectFileWizardPage.FileName);
                    //string pathSettingsDat = Path.Combine(dir, "settings.dat");
                    //var settingsDat = File.ReadAllText(pathSettingsDat).ToLower();
                    //var indexRestart = settingsDat.IndexOf("[restart]");
                    //settingsDat = settingsDat.Substring(0, indexRestart);
                }
                else
                {
                    throw new ArgumentException("Not a valid file to import.");
                }
            }
            base.OnPageCompleted(page);
        }

    }
}