using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DelftTools.Controls.Swf;
using DelftTools.Shell.Gui;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.Gui.Properties;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.IO;

namespace DeltaShell.Plugins.DelftModels.WaterQualityModel.Gui.Forms.WaterQualityModelWizard
{
    class BoundaryDataWizard: WizardDialog, IConfigureDialog
    {
        private DataTableBoundaryImporter boundaryDataItemFileImporter;
        private readonly BoundaryDataWizardPage boundaryDataWizardPage;

        public BoundaryDataWizard()
        {
            Height = 700;
            Title = Resources.BoundaryDataWizard_Title;
            WelcomeMessage = Resources.BoundaryDataWizard_Welcome_message;
            FinishedPageMessage = Resources.BoundaryDataWizard_Finished_message;

            boundaryDataWizardPage = new BoundaryDataWizardPage();
            AddPage(boundaryDataWizardPage, Resources.BoundaryDataWizard_Title,
                    Resources.BoundaryDataWizardPage_Description);
        }

        public void Configure(object targetItemFileImporter)
        {
            boundaryDataItemFileImporter = (DataTableBoundaryImporter)targetItemFileImporter;
            boundaryDataItemFileImporter.FilePath = boundaryDataWizardPage.csvBoundaryPath;
            boundaryDataItemFileImporter.ImportItem(boundaryDataWizardPage.csvBoundaryPath,
                                                    WaterQualityModel.BoundaryDataManager);
        }

        public WaterQualityModel WaterQualityModel { get; set; }
    }
}
