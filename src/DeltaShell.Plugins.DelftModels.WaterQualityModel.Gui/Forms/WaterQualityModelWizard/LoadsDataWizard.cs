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
    class LoadsDataWizard: WizardDialog, IConfigureDialog
    {
        private DataTableLoadsImporter loadsDataItemFileImporter;
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
            loadsDataItemFileImporter = (DataTableLoadsImporter)targetItemFileImporter;
            loadsDataItemFileImporter.FilePath = loadsDataWizardPage.csvLoadsPath;
            loadsDataItemFileImporter.ImportItem(loadsDataWizardPage.csvLoadsPath,
                                                    WaterQualityModel.LoadsDataManager);
        }
        public WaterQualityModel WaterQualityModel { get; set; }
    }
}
