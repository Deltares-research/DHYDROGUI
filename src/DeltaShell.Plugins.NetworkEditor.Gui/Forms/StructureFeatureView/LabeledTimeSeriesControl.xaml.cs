using System;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using DelftTools.Controls.Wpf.Commands;
using DelftTools.Functions;
using DeltaShell.Plugins.CommonTools.Gui.Forms.Charting;
using DeltaShell.Plugins.CommonTools.Gui.Forms.Functions;
using UserControl = System.Windows.Controls.UserControl;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.StructureFeatureView
{
    /// <summary>
    /// Interaction logic for LabeledTimeSeriesControl.xaml
    /// </summary>
    public partial class LabeledTimeSeriesControl : UserControl
    {
        /// <summary>
        /// Gets or sets the Label which is displayed next to the field
        /// </summary>
        public string Label
        {
            get => (string)GetValue(LabelProperty);
            set => SetValue(LabelProperty, value);
        }

        /// <summary>
        /// Identified the Label dependency property
        /// </summary>
        public static readonly DependencyProperty LabelProperty =
            DependencyProperty.Register(nameof(Label), 
                                        typeof(string), 
                                        typeof(LabeledTimeSeriesControl), 
                                        new PropertyMetadata(""));

        /// <summary>
        /// Gets or sets the Label which is displayed next to the field
        /// </summary>
        public string Unit
        {
            get => (string)GetValue(UnitProperty);
            set => SetValue(UnitProperty, value);
        }

        /// <summary>
        /// Identified the Unit dependency property
        /// </summary>
        public static readonly DependencyProperty UnitProperty =
            DependencyProperty.Register(nameof(Unit), 
                                        typeof(string), 
                                        typeof(LabeledTimeSeriesControl), 
                                        new PropertyMetadata(""));

        /// <summary>
        /// Gets or sets the Value which is being displayed
        /// </summary>
        public string Value
        {
            get => (string)GetValue(ValueProperty);
            set => SetValue(ValueProperty, value);
        }

        /// <summary>
        /// Identified the Label dependency property
        /// </summary>
        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register(nameof(Value), 
                                        typeof(string), 
                                        typeof(LabeledTimeSeriesControl), 
                                        new FrameworkPropertyMetadata(default(string),
                                                                      FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        /// <summary>
        /// Gets or sets the time series.
        /// </summary>
        public TimeSeries TimeSeries
        {
            get => (TimeSeries) GetValue(TimeSeriesProperty);
            set => SetValue(TimeSeriesProperty, value);
        }

        /// <summary>
        /// The time series property
        /// </summary>
        public static readonly DependencyProperty TimeSeriesProperty =
            DependencyProperty.Register(nameof(TimeSeries),
                                        typeof(TimeSeries),
                                        typeof(LabeledTimeSeriesControl),
                                        new PropertyMetadata(default(TimeSeries))); 

        /// <summary>
        /// Gets or sets a value indicating whether this instance is rendered as a time series.
        /// </summary>
        public bool IsTimeSeries
        {
            get => (bool) GetValue(IsTimeSeriesProperty);
            set => SetValue(IsTimeSeriesProperty, value);
        }

        /// <summary>
        /// The is time series property
        /// </summary>
        public static readonly DependencyProperty IsTimeSeriesProperty =
            DependencyProperty.Register(nameof(IsTimeSeries),
                                        typeof(bool),
                                        typeof(LabeledTimeSeriesControl),
                                        new FrameworkPropertyMetadata(default(bool),
                                                                      FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        /// <summary>
        /// Gets or sets the time series button label.
        /// </summary>
        public string TimeSeriesButtonLabel
        {
            get => (string)GetValue(TimeSeriesButtonLabelProperty);
            set => SetValue(TimeSeriesButtonLabelProperty, value);
        }

        /// <summary>
        /// The time series button label.
        /// </summary>
        public static readonly DependencyProperty TimeSeriesButtonLabelProperty =
            DependencyProperty.Register(nameof(TimeSeriesButtonLabel),
                                        typeof(string),
                                        typeof(LabeledTimeSeriesControl),
                                        new PropertyMetadata("Time Series"));

        public LabeledTimeSeriesControl()
        {
            TimeSeriesCommand = new RelayCommand((_) => OnTimeSeriesClick());
            InitializeComponent();
        }

        public ICommand TimeSeriesCommand { get; }

        private void OnTimeSeriesClick()
        {
            var dialogData = (TimeSeries) TimeSeries.Clone(true);
            
            var editFunctionDialog = new EditFunctionDialog
            {
                Text = $@"{Label} time series",
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
