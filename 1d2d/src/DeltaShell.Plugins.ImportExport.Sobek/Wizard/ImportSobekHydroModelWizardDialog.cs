using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Controls.Swf;
using DelftTools.Controls.Swf.WizardPages;
using DelftTools.Hydro.Roughness;
using DelftTools.Shell.Core.Extensions;
using DelftTools.Utils.Reflection;
using DeltaShell.Plugins.DelftModels.HydroModel;
using DeltaShell.Plugins.DelftModels.RainfallRunoff;
using DeltaShell.Plugins.DelftModels.RealTimeControl;
using DeltaShell.Plugins.FMSuite.FlowFM;
using DeltaShell.Plugins.ImportExport.Sobek.PartialSobekImporter;

namespace DeltaShell.Plugins.ImportExport.Sobek.Wizard
{
    public class ImportSobekHydroModelWizardDialog : WizardDialog
    {
        private SelectSobekModelsWizardPage selectSobekModelsWizardPage;
        private SelectSobekPartsWizardPage selectSobekPartsWizardPage;
        private SelectFileWizardPage selectFileWizardPage;
        private SobekHydroModelImporter importer;

        public ImportSobekHydroModelWizardDialog()
        {
            Title = "SOBEK Hydro model importer";
        }

        public override object Data
        {
            get { return Importer; }
            set
            {
                Importer = (SobekHydroModelImporter)value;
            }
        }

        public SobekHydroModelImporter Importer
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
            selectSobekModelsWizardPage = new SelectSobekModelsWizardPage();
            selectSobekPartsWizardPage = new SelectSobekPartsWizardPage();

            AddPage(selectFileWizardPage, selectText, "");
            AddPage(selectSobekModelsWizardPage, "Select models to import", "Use the checkboxes to select");
            AddPage(selectSobekPartsWizardPage, "Select parts to import", "Use the checkboxes to select");

            WelcomePageVisible = false;
            CompletionPageVisible = false;
        }

        protected override void OnPageCompleted(IWizardPage page)
        {
            if (page == selectFileWizardPage)
            {
                importer.PathSobek = selectFileWizardPage.FileName;
                
                if (selectFileWizardPage.FileName.ToLower().EndsWith("deftop.1"))
                {
                    // This is a SobekRE model. 
                    selectSobekModelsWizardPage.ImportRR = false;
                    selectSobekModelsWizardPage.ImportRREnabled = false; 
                    selectSobekModelsWizardPage.ImportFlow = true;
                    selectSobekModelsWizardPage.ImportRtc = true;                    
                }
                else if (selectFileWizardPage.FileName.ToLower().EndsWith("network.tp"))
                {
                    string dir = Path.GetDirectoryName(selectFileWizardPage.FileName);
                    string pathSettingsDat = Path.Combine(dir, "settings.dat");
                    var settingsDat = File.ReadAllText(pathSettingsDat).ToLower();
                    var indexRestart = settingsDat.IndexOf("[restart]");
                    settingsDat = settingsDat.Substring(0, indexRestart);

                    // Add RTC in case that a flow model is detected. In case no controls are detected afterwards, this will be deleted. 
                    selectSobekModelsWizardPage.ImportFlow = settingsDat.Contains("channel=-1") || settingsDat.Contains("river=-1") || settingsDat.Contains("sewer=-1");
                    selectSobekModelsWizardPage.ImportRtc = selectSobekModelsWizardPage.ImportFlow;
                    selectSobekModelsWizardPage.ImportRR = settingsDat.Contains("3b=-1");
                    
                    selectSobekModelsWizardPage.ImportFlowEnabled = settingsDat.Contains("channel=-1") || settingsDat.Contains("river=-1") || settingsDat.Contains("sewer=-1");
                    selectSobekModelsWizardPage.ImportRtcEnabled = selectSobekModelsWizardPage.ImportFlow;
                    selectSobekModelsWizardPage.ImportRREnabled = settingsDat.Contains("3b=-1");
                }
                else
                {
                    throw new ArgumentException("Not a valid file to import.");
                }
            }
            else if (page == selectSobekModelsWizardPage)
            {
                importer.UseFm = selectSobekModelsWizardPage.ImportFlow;
                importer.UseRR = selectSobekModelsWizardPage.ImportRR;
                importer.UseRTC = selectSobekModelsWizardPage.ImportRtc;
                selectSobekPartsWizardPage.PartialSobekImporter = importer;
            }
            else if (page == selectSobekPartsWizardPage)
            {
                //do something?
            }
            base.OnPageCompleted(page);
        }
        private IEnumerable<IPartialSobekImporter> GetImporters(IPartialSobekImporter partialImporter)
        {
            while (partialImporter != null)
            {
                yield return partialImporter;
                partialImporter = partialImporter.PartialSobekImporter;
            }
        }
        protected override void OnDialogFinished()
        {
            importer.UseRTC = GetImporters(importer).Where(i => PartialSobekImporterBuilder.GetRealTimeControlModelImporters().Any(rtcImp => i.GetType().Implements(rtcImp.GetType()))).Any(imp => imp.IsActive);
            importer.UseRR = GetImporters(importer).Where(i => PartialSobekImporterBuilder.GetRainfallRunoffModelImporters().Any(rrImp => i.GetType().Implements(rrImp.GetType()))).Any(imp => imp.IsActive);
            importer.UseFm = GetImporters(importer).Where(i => PartialSobekImporterBuilder.GetWaterFlowFMModelImporters().Any(fmImp => i.GetType().Implements(fmImp.GetType()))).Any(imp => imp.IsActive);
            ConfigureIntegratedModel(importer);
            base.OnDialogFinished();
        }

        private void ConfigureIntegratedModel(SobekHydroModelImporter sobekHydroModelImporter)
        {
            if (sobekHydroModelImporter.TargetObject is HydroModel hydroModel)
            {
                if (!hydroModel.GetAllActivitiesRecursive<IModelWithNetwork>().Any() && sobekHydroModelImporter.UseFm)
                {
                    //seems like an integrated model without fm
                    var fmModel = new WaterFlowFMModel();
                    fmModel.MoveModelIntoIntegratedModel(hydroModel.GetFolderContainer(), hydroModel);
                }

                if (!hydroModel.GetAllActivitiesRecursive<RainfallRunoffModel>().Any() && sobekHydroModelImporter.UseRR)
                {
                    //seems like an integrated model without rr
                    var rrModel = new RainfallRunoffModel();
                    rrModel.MoveModelIntoIntegratedModel(hydroModel.GetFolderContainer(), hydroModel);
                }

                if (!hydroModel.GetAllActivitiesRecursive<RealTimeControlModel>().Any() && sobekHydroModelImporter.UseRTC)
                {
                    //seems like an integrated model without rtc
                    var rtcModel = new RealTimeControlModel();
                    hydroModel.Activities.Add(rtcModel);
                }
                hydroModel.RefreshDefaultModelWorkflows();
            }

        }
    }
}