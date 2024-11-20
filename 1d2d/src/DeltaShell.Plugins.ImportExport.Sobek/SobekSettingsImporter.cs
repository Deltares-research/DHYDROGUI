using System;
using System.Globalization;
using System.IO;
using System.Linq;
using DeltaShell.Plugins.FMSuite.FlowFM;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using DeltaShell.Plugins.ImportExport.Sobek.PartialSobekImporter;
using DeltaShell.Sobek.Readers.Readers;
using DeltaShell.Sobek.Readers.SobekDataObjects;
using log4net;

namespace DeltaShell.Plugins.ImportExport.Sobek
{
    public class SobekSettingsImporter : PartialSobekImporterBase
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(SobekSettingsImporter));

        private SobekCaseSettings sobekCaseSettings;
        private WaterFlowFMModel waterFlowFMModel;
        
        public override string DisplayName
        {
            get { return "Model and case settings"; }
        }

        public override SobekImporterCategories Category { get; } = SobekImporterCategories.WaterFlow1D;

        protected override void PartialImport()
        {
            log.DebugFormat("Importing model settings ...");

            waterFlowFMModel = GetModel<WaterFlowFMModel>();
            
            if (SobekType == DeltaShell.Sobek.Readers.SobekType.Sobek212)
            {
                ImportSobek212Settings();
            }
            else
            {
                ImportSobekReSettings();
            }

            ImportCaseSettingsFile();
        }

        private void ImportSobek212Settings()
        {
            var path = GetFilePath(SobekFileNames.SobekCaseSettingFileName);

            try
            {
                if (!File.Exists(path))
                {
                    log.WarnFormat("Import of case settings skipped, file {0} does not exist.", path);
                    return;
                }

                sobekCaseSettings = new SobekCaseSettingsReader().GetSobekCaseSettings(path);

                SetModelParameters();
            }
            catch (Exception exception)
            {
                log.ErrorFormat("Error reading case settings {0}; reason {1}", path, exception.Message);
            }
        }

        private void ImportSobekReSettings()
        {
            var path = "";
            try
            {
                path = GetFilePath("DEFRUN.1");
                sobekCaseSettings = new SobekReDefRun1Reader().Read(path).First();
                waterFlowFMModel.StartTime = sobekCaseSettings.StartTime;
                waterFlowFMModel.StopTime = sobekCaseSettings.StopTime;
                waterFlowFMModel.TimeStep = sobekCaseSettings.TimeStep;
                waterFlowFMModel.OutputTimeStep = sobekCaseSettings.OutPutTimeStep;

                path = GetFilePath("DEFRUN.2");
                var sobekReDefRun2Reader = new SobekReDefRun2Reader {SobekCaseSettingsInstance = sobekCaseSettings};
                sobekReDefRun2Reader.Read(path).First();
                sobekCaseSettings = sobekReDefRun2Reader.SobekCaseSettingsInstance;
            }
            catch (Exception exception)
            {
                log.ErrorFormat("Error reading case settings {0}; reason {1}", path, exception.Message);
            }
        }

        private void ImportCaseSettingsFile()
        {
            if (CaseData.WindFile == null)
            {
                log.Error("No wind data available.");
            }
        }
        
        private void SetModelParameters()
        {
            waterFlowFMModel.StartTime = sobekCaseSettings.StartTime;
            waterFlowFMModel.ReferenceTime = sobekCaseSettings.StartTime;
            waterFlowFMModel.StopTime = sobekCaseSettings.StopTime;
            waterFlowFMModel.TimeStep = sobekCaseSettings.TimeStep;
            waterFlowFMModel.OutputTimeStep = sobekCaseSettings.OutPutTimeStep;
            waterFlowFMModel.ModelDefinition.GetModelProperty(KnownProperties.DtMax).Value = sobekCaseSettings.TimeStep.TotalSeconds;
            waterFlowFMModel.ModelDefinition.SetModelProperty(GuiProperties.HisOutputDeltaT, sobekCaseSettings.OutPutTimeStep.TotalSeconds.ToString(CultureInfo.InvariantCulture));
        }
    }
}
