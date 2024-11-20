using DeltaShell.Plugins.DelftModels.WaterQualityModel.Gui.Properties;

namespace DeltaShell.Plugins.DelftModels.WaterQualityModel.Gui.Forms.WaterQualityModelWizard
{
    /// <summary>
    /// Wizard page for importing loads data from a csv file.
    /// </summary>
    public class LoadsDataWizardPage : CsvDataWizardPage
    {
        /// <summary>
        /// Creates an instance of <see cref="LoadsDataWizardPage"/>.
        /// </summary>
        public LoadsDataWizardPage()
        {
            InitializeComponent(Resources.LoadsDataWizardPage_Open_Loads_Data_File);
        }
    }
}