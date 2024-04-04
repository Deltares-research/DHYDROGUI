using DeltaShell.Plugins.DelftModels.WaterQualityModel.DataObjects.SubstanceProcessLibrary;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.Gui.Properties;
using log4net;

namespace DeltaShell.Plugins.DelftModels.WaterQualityModel.Gui.Forms.WaterQualityModelWizard
{
    public static class SubstanceProcessLibraryWizardPageExtensions
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(SubstanceProcessLibraryWizardPageExtensions));

        public static void SetProcessFilesToModel(
            this SubstanceProcessLibraryWizardPage substanceProcessLibraryWizardPage,
            SubstanceProcessLibrary substanceProcessLibrary)
        {
            switch (substanceProcessLibraryWizardPage.SubFileProcessType)
            {
                case WaterQualityProcessType.Sobek:
                {
                    substanceProcessLibrary.ProcessDllFilePath = "";
                    substanceProcessLibrary.ProcessDefinitionFilesPath = WaterQualityApiDataSet.DelWaqProcessDefinitionFilesDirectory;

                    break;
                }
                case WaterQualityProcessType.Duflow:
                {
                    substanceProcessLibrary.ProcessDllFilePath = WaterQualityApiDataSet.WaqDuflowDllPath;
                    substanceProcessLibrary.ProcessDefinitionFilesPath = WaterQualityApiDataSet.WaqDuflowProcessDefinitionFilesDirectory;

                    break;
                }
                case WaterQualityProcessType.Custom:
                {
                    if (substanceProcessLibraryWizardPage.UsingCustomProcessFiles)
                    {
                        substanceProcessLibrary.ProcessDllFilePath = substanceProcessLibraryWizardPage.CustomProcessDllFilePath;
                        substanceProcessLibrary.ProcessDefinitionFilesPath = substanceProcessLibraryWizardPage.CustomProcessDefinitionFilesPath;
                    }
                    else
                    {
                        substanceProcessLibrary.ProcessDllFilePath = "";
                        substanceProcessLibrary.ProcessDefinitionFilesPath = WaterQualityApiDataSet.DelWaqProcessDefinitionFilesDirectory;
                    }

                    break;
                }
            }

            Log.InfoFormat(
                Resources.SubstanceProcessLibraryWizardPageExtensions_The_process_definition_files_path_is_set_to_0_,
                string.IsNullOrEmpty(substanceProcessLibrary.ProcessDefinitionFilesPath)
                    ? Resources.SubstanceProcessLibraryWizardPageExtentsions_Empty_
                    : substanceProcessLibrary.ProcessDefinitionFilesPath);
        }
    }
}