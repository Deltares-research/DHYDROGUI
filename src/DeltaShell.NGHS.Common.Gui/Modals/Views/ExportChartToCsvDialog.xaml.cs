using System.Windows;
using System.Windows.Forms;
using DelftTools.Controls.Swf.Charting;

namespace DeltaShell.NGHS.Common.Gui.Modals.Views
{
    /// <summary>
    /// Interaction logic for SideViewExportToCsvDialog.xaml
    /// </summary>
    public partial class ExportChartToCsvDialog : Window
    {
        public ExportChartToCsvDialog()
        {
            InitializeComponent();
            ViewModel.GetFilePath = () =>
            {
                var dialog = new SaveFileDialog
                {
                    Filter = "Csv file|*.csv",
                    Title = "Select export file",
                    AddExtension = true,
                    DefaultExt = "csv",
                    RestoreDirectory = true,
                    OverwritePrompt = true,
                    ValidateNames = true
                };

                if(dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    return dialog.FileName;
                }

                return null;
            };
            ViewModel.CloseView = () => Close();
        }

        /// <summary>
        /// Initialize dialog for supplied chart
        /// </summary>
        /// <param name="chart">Chart to export</param>
        public void SetChart(IChart chart)
        {
            ViewModel?.SetChart(chart);
        }
    }
}
