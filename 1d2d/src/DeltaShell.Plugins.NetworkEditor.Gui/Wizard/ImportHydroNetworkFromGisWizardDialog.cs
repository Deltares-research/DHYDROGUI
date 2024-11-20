using System.ComponentModel;
using System.Windows.Forms;
using DelftTools.Controls.Swf;
using DeltaShell.Plugins.NetworkEditor.Import;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Wizard
{
    /// <summary>
    /// 
    /// </summary>
    public class ImportHydroNetworkFromGisWizardDialog: WizardDialog
    {
        private HydroRegionFromGisImporter importer;
        private SelectDataWizardPage selectDataPage;
        private DefineMappingTableWizardPage defineMappingTablePage;
        private ImportFromGisWizardPage importPage;

        public ImportHydroNetworkFromGisWizardDialog()
        {
            Title = "Import from GIS";
            WelcomeMessage = "This wizard is used to import model features from GIS-data";
            FinishedPageMessage = "Press Finish to import features from GIS-data into the selected region.";
            Height = 700;

            selectDataPage = new SelectDataWizardPage();

            defineMappingTablePage = new DefineMappingTableWizardPage();

            importPage = new ImportFromGisWizardPage();

            AddPage(selectDataPage, "Select model features to import", "Select model features to import from GIS");
            AddPage(defineMappingTablePage, "Define a mapping table", "Define object mappings to set the GIS data into model features");
            AddPage(importPage, "Import properties", "Set the conditions for importing");

        }

        public override object Data
        {
            get { return Importer; }
            set
            {
                Importer = (HydroRegionFromGisImporter)value;
            }
        }

        public HydroRegionFromGisImporter Importer
        {
            get { return importer; }
            set
            {
                importer = value;
                selectDataPage.HydroRegionFromGisImporter = importer;
                defineMappingTablePage.HydroRegionFromGisImporter = importer;
                importPage.HydroRegionFromGisImporter = importer;
            }
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            if (DialogResult == DialogResult.Cancel)
            {
                importer.FeatureFromGisImporters.Clear();
            }
        }
    }
}
