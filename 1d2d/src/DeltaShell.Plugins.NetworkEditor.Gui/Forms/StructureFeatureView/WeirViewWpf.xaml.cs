using System.Drawing;
using System.Windows.Forms;
using DelftTools.Controls;
using DelftTools.Functions;
using DelftTools.Hydro.Structures;
using DeltaShell.Plugins.CommonTools.Gui.Forms.Charting;
using DeltaShell.Plugins.CommonTools.Gui.Forms.Functions;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.StructureFeatureView
{
    /// <summary>
    /// Interaction logic for FMWeirView.xaml
    /// </summary>
    public partial class WeirViewWpf : IView
    {
        public WeirViewWpf()
        {
            InitializeComponent();
        }

        public object Data
        {
            get { return WeirViewModel.Weir; }
            set
            {
                WeirViewModel.Weir = (IWeir) value;
                WeirViewModel.GetTimeSeriesEditor = TimeSeriesEditor;
            }
        }

        public string Text { get; set; }

        public Image Image { get; set; }

        public bool Visible { get; private set; }

        public ViewInfo ViewInfo { get; set; }

        private TimeSeries TimeSeriesEditor(IWeir weirData)
        {
            var weirName = weirData.Name;
            var dialogData = (TimeSeries)weirData.CrestLevelTimeSeries.Clone(true);

            var editFunctionDialog = new EditFunctionDialog
            {
                Text = string.Format("Crest level time series for {0}", weirName),
                ColumnNames = new[] { "Date time", "Crest level [m]" },
                ChartViewOption = ChartViewOptions.AllSeries,
                Data = dialogData
            };

            if (DialogResult.OK == editFunctionDialog.ShowDialog())
            {
                return dialogData;
            }
            return null;
        }

        public void EnsureVisible(object item)
        {

        }

        public void Dispose()
        {
        }
    }
}
