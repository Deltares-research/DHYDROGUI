using DeltaShell.Plugins.DelftModels.WaterQualityModel.Gui.Properties;

namespace DeltaShell.Plugins.DelftModels.WaterQualityModel.Gui.Forms.WaterQualityModelWizard
{
    /// <summary>
    /// Wizard page for importing boundary data from a csv file.
    /// </summary>
    public class BoundaryDataWizardPage : CsvDataWizardPage
    {
        /// <summary>
        /// Creates an instance of <see cref="BoundaryDataWizardPage"/>.
        /// </summary>
        public BoundaryDataWizardPage()
        {
            InitializeComponent(Resources.BoundaryDataWizardPage_Open_Boundary_Data_File);
        }
    }
}