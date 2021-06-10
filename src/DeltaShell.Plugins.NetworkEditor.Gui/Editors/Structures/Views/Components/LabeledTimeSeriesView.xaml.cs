using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using DelftTools.Controls.Wpf.Commands;
using DelftTools.Functions;
using DeltaShell.NGHS.Common.Gui.Components;
using DeltaShell.Plugins.CommonTools.Gui.Forms.Charting;
using DeltaShell.Plugins.CommonTools.Gui.Forms.Functions;
using UserControl = System.Windows.Controls.UserControl;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Editors.Structures.Views.Components
{
    /// <summary>
    /// Interaction logic for LabeledTimeSeriesView.xaml
    /// </summary>
    public partial class LabeledTimeSeriesView : UserControl
    {
        /// <summary>
        /// The structure name property
        /// </summary>
        public static readonly DependencyProperty StructureNameProperty =
            DependencyProperty.Register(nameof(StructureName),
                                        typeof(string),
                                        typeof(LabeledTimeSeriesView),
                                        new PropertyMetadata(default(string)));

        /// <summary>
        /// The label property
        /// </summary>
        public static readonly DependencyProperty LabelProperty =
            DependencyProperty.Register(nameof(Label),
                                        typeof(string),
                                        typeof(LabeledTimeSeriesView),
                                        new PropertyMetadata(default(string)));

        /// <summary>
        /// The unit property
        /// </summary>
        public static readonly DependencyProperty UnitProperty =
            DependencyProperty.Register(nameof(Unit),
                                        typeof(string),
                                        typeof(LabeledTimeSeriesView),
                                        new PropertyMetadata(default(string)));

        /// <summary>
        /// The value content property
        /// </summary>
        public static readonly DependencyProperty SingleValueContentProperty =
            DependencyProperty.Register(nameof(SingleValueContent),
                                        typeof(string),
                                        typeof(LabeledTimeSeriesView),
                                        new FrameworkPropertyMetadata(default(string),
                                                                      FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        /// <summary>
        /// The time series property
        /// </summary>
        public static readonly DependencyProperty TimeSeriesProperty =
            DependencyProperty.Register(nameof(TimeSeries),
                                        typeof(TimeSeries),
                                        typeof(LabeledTimeSeriesView),
                                        new PropertyMetadata(default(TimeSeries)));

        /// <summary>
        /// The is time series property
        /// </summary>
        public static readonly DependencyProperty IsTimeSeriesProperty =
            DependencyProperty.Register(nameof(IsTimeSeries),
                                        typeof(bool),
                                        typeof(LabeledTimeSeriesView),
                                        new FrameworkPropertyMetadata(default(bool),
                                                                      FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        /// <summary>
        /// Creates a new <see cref="LabeledTimeSeriesView"/>.
        /// </summary>
        public LabeledTimeSeriesView()
        {
            TimeSeriesCommand = new RelayCommand((_) => OnTimeSeriesClick());
            InitializeComponent();
        }

        /// <summary>
        /// Gets or sets the name of the structure.
        /// </summary>
        public string StructureName
        {
            get => (string) GetValue(StructureNameProperty);
            set => SetValue(StructureNameProperty, value);
        }

        /// <summary>
        /// Gets or sets the label of this <see cref="LabeledButtonRow"/>.
        /// </summary>
        public string Label
        {
            get => (string) GetValue(LabelProperty);
            set => SetValue(LabelProperty, value);
        }

        /// <summary>
        /// Gets or sets the single value of this <see cref="LabeledValueBoxRow"/>.
        /// </summary>
        public string SingleValueContent
        {
            get => (string) GetValue(SingleValueContentProperty);
            set => SetValue(SingleValueContentProperty, value);
        }

        /// <summary>
        /// Gets or sets the time series.
        /// </summary>
        public TimeSeries TimeSeries
        {
            get => (TimeSeries) GetValue(TimeSeriesProperty);
            set => SetValue(TimeSeriesProperty, value);
        }

        /// <summary>
        /// Gets or sets the unit of this <see cref="LabeledValueBoxRow"/>.
        /// </summary>
        public string Unit
        {
            get => (string) GetValue(UnitProperty);
            set => SetValue(UnitProperty, value);
        }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is rendered as a time series.
        /// </summary>
        public bool IsTimeSeries
        {
            get => (bool) GetValue(IsTimeSeriesProperty);
            set => SetValue(IsTimeSeriesProperty, value);
        }

        /// <summary>
        /// Gets the open time series editor command.
        /// </summary>
        public ICommand TimeSeriesCommand { get; }

        private void OnTimeSeriesClick()
        {
            var dialogData = (TimeSeries) TimeSeries.Clone(true);
            var editFunctionDialog = new EditFunctionDialog
            {
                Text = $@"{Label} time series for {StructureName}.",
                ColumnNames = new[]
                {
                    "Date time",
                    $"{Label} [{Unit}]"
                },
                ChartViewOption = ChartViewOptions.AllSeries,
                Data = dialogData,
                ShowOnlyFirstWordInColumnHeadersOnLoad = false
            };

            if (DialogResult.OK != editFunctionDialog.ShowDialog())
            {
                return;
            }

            TimeSeries.Time.Clear();
            TimeSeries.Components[0].Clear();

            TimeSeries.Time.SetValues(dialogData.Time.Values);
            TimeSeries.Components[0].SetValues(dialogData.Components[0].Values);
        }
    }
}