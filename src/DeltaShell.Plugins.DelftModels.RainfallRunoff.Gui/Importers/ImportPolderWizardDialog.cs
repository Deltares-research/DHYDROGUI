using System.Linq;
using DelftTools.Controls.Swf;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Importers;
using DeltaShell.Plugins.NetworkEditor.Gui.Wizard;
using DeltaShell.Plugins.NetworkEditor.Import;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Gui.Importers
{
    public class ImportPolderWizardDialog : WizardDialog
    {
        private readonly DefineMappingTableWizardPage defineMappingTablePage;
        private readonly SelectDataWizardPage selectCatchmentShapeFilePage;
        private readonly LandUsePolderMappingWizardPage selectLandUseShapeFilePage;

        private PolderFromGisImporter importer;

        public ImportPolderWizardDialog()
        {
            Title = "Import from GIS: Polder Concept";
            WelcomeMessage = "This wizard is used to import polder catchments from GIS-data.";
            FinishedPageMessage =
                "Press Finish to import catchments and initial schematization from GIS-data into your model.";
            Height = 700;

            selectCatchmentShapeFilePage = new SelectDataWizardPage();
            defineMappingTablePage = new DefineMappingTableWizardPage();
            selectLandUseShapeFilePage = new LandUsePolderMappingWizardPage();

            AddPage(selectCatchmentShapeFilePage, "Import catchment from GIS-data", "");
            AddPage(defineMappingTablePage, "Define a mapping table",
                    "Define object mappings to set the GIS data into catchments");
            AddPage(selectLandUseShapeFilePage, "Land-use shape file",
                    "Optionally select a shape file to import land-use from and define the mapping to Polder Concept subtypes");

            Importer = new PolderFromGisImporter();
        }

        public PolderFromGisImporter Importer
        {
            get { return importer; }
            set
            {
                importer = value;
                selectLandUseShapeFilePage.Importer = importer;
                
                var hydroRegionFromGisImporter = new HydroRegionFromGisImporter();
                hydroRegionFromGisImporter.AvailableFeatureFromGisImporters.Add("Polder Catchment", typeof(CatchmentFromGisImporter));
                selectCatchmentShapeFilePage.HydroRegionFromGisImporter = hydroRegionFromGisImporter;
                defineMappingTablePage.HydroRegionFromGisImporter = hydroRegionFromGisImporter;
            }
        }

        protected override void OnPageCompleted(IWizardPage page)
        {
            if (page == selectCatchmentShapeFilePage)
            {
                selectLandUseShapeFilePage.OnCatchmentDataSourceSelected();
            }
            if (page == defineMappingTablePage)
            {
                var hydroRegionFromGisImporter = defineMappingTablePage.HydroRegionFromGisImporter;
                importer.CatchmentImporter = hydroRegionFromGisImporter.FeatureFromGisImporters.OfType<CatchmentFromGisImporter>().FirstOrDefault();
            }

            base.OnPageCompleted(page);
        }
    }
}