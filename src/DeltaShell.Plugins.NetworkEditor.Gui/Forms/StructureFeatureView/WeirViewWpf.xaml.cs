using System.Drawing;
using System.Windows.Forms;
using DelftTools.Controls;
using DelftTools.Functions;
using DelftTools.Hydro.Structures;
using DelftTools.Hydro.Structures.WeirFormula;
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
                WeirViewModel.GetTimeSeriesEditorForCrestLevel = CrestLevelTimeSeriesEditor;
                WeirViewModel.GetTimeSeriesEditorForEdgeLevel = LowerEdgeLevelTimeSeriesEditor;
                WeirViewModel.GetTimeSeriesEditorForHorizontalDoorOpeningWidth = HorizontalDoorOpeningWidthTimeSeriesEditor;
            }
        }

        public string Text { get; set; }

        public Image Image { get; set; }

        public bool Visible { get; private set; }

        public ViewInfo ViewInfo { get; set; }

        private TimeSeries CrestLevelTimeSeriesEditor(IWeir weirData)
        {
            var weirName = weirData.Name;
            var dialogData = (TimeSeries)weirData.CrestLevelTimeSeries.Clone(true);

            var editFunctionDialog = new EditFunctionDialog
            {
                Text = $@"{GuiParameterNames.CrestLevel} time series for {weirName}",
                ColumnNames = new[] { "Date time", $"{GuiParameterNames.CrestLevel} [m]" },
                ChartViewOption = ChartViewOptions.AllSeries,
                Data = dialogData
            };

            if (DialogResult.OK == editFunctionDialog.ShowDialog())
            {
                return dialogData;
            }
            return null;
        }
        private TimeSeries LowerEdgeLevelTimeSeriesEditor(IGatedWeirFormula gatedWeirData)
        {
            var weirName = gatedWeirData.Name;
            var dialogData = (TimeSeries)gatedWeirData.LowerEdgeLevelTimeSeries.Clone(true);

            var editFunctionDialog = new EditFunctionDialog
            {
                Text = $@"{GuiParameterNames.GateLowerEdgeLevel} time series for {weirName}",
                ColumnNames = new[] { "Date time", $"{GuiParameterNames.GateLowerEdgeLevel} [m]" },
                ChartViewOption = ChartViewOptions.AllSeries,
                Data = dialogData
            };

            if (DialogResult.OK == editFunctionDialog.ShowDialog())
            {
                return dialogData;
            }
            return null;
        }
        private TimeSeries HorizontalDoorOpeningWidthTimeSeriesEditor(IGatedWeirFormula gatedWeirData)
        {
            var weirName = gatedWeirData.Name;
            var dialogData = (TimeSeries)gatedWeirData.HorizontalDoorOpeningWidthTimeSeries.Clone(true);

            var editFunctionDialog = new EditFunctionDialog
            {
                Text = $@"{GuiParameterNames.HorizontalOpeningWidth} time series for {weirName}",
                ColumnNames = new[] { "Date time", GuiParameterNames.HorizontalOpeningWidth},
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
