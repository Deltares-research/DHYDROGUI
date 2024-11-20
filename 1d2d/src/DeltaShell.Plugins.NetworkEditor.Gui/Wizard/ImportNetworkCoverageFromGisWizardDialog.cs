using System.Linq;
using DelftTools.Controls.Swf;
using DeltaShell.Plugins.NetworkEditor.Import;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Wizard
{
    public class ImportNetworkCoverageFromGisWizardDialog : WizardDialog
    {
        private NetworkCoverageFromGisImporter importer;

        private SelectDataWizardPage selectDataPage;
        private DefineMappingTableWizardPage defineMappingTablePage;
        private ImportFromGisWizardPage importPage;

        public ImportNetworkCoverageFromGisWizardDialog()
        {
            Title = "Import network spatial data from GIS";
            WelcomeMessage = "This wizard is used to (partially) import network spatial data from GIS";
            FinishedPageMessage = "Press Finish to import the data from GIS onto the selected network spatial data.";
            Height = 700;

            selectDataPage = new SelectDataWizardPage();
            defineMappingTablePage = new DefineMappingTableWizardPage();
            importPage = new ImportFromGisWizardPage();

            AddPage(selectDataPage, "Select model features to import", "Select model features to import from GIS");
            AddPage(defineMappingTablePage, "Define a mapping table", "Define object mappings to set the GIS data into model features");
            AddPage(importPage, "Import properties", "Set the conditions for importing");

            Importer = new NetworkCoverageFromGisImporter();
        }

        public NetworkCoverageFromGisImporter Importer
        {
            get { return importer; }
            set
            {
                importer = value;

                var hydroRegionFromGisImporter = new HydroRegionFromGisImporter();
                hydroRegionFromGisImporter.AvailableFeatureFromGisImporters.Add("Network location and value", typeof(PointValuePairsFromGisImporter));

                selectDataPage.HydroRegionFromGisImporter = hydroRegionFromGisImporter;
                defineMappingTablePage.HydroRegionFromGisImporter = hydroRegionFromGisImporter;
                importPage.HydroRegionFromGisImporter = hydroRegionFromGisImporter;
            }
        }

        protected override void OnPageCompleted(IWizardPage page)
        {
            if (page == importPage)
            {
                var hydroRegionFromGisImporter = importPage.HydroRegionFromGisImporter;
                importer.PointValuePairsFromGisImporter = hydroRegionFromGisImporter.FeatureFromGisImporters.OfType<PointValuePairsFromGisImporter>().FirstOrDefault();
                
                if (importer.PointValuePairsFromGisImporter != null)
                {
                    importer.PointValuePairsFromGisImporter.SnappingPrecision = hydroRegionFromGisImporter.SnappingPrecision;
                }
            }

            base.OnPageCompleted(page);
        }
    }
}
