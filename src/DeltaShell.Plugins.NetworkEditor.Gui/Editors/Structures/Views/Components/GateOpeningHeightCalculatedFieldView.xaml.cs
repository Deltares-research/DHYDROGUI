using System.Windows;
using System.Windows.Controls;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Editors.Structures.Views.Components
{
    /// <summary>
    /// Interaction logic for GateOpeningHeightCalculatedFieldControl.xaml
    /// </summary>
    public partial class GateOpeningHeightCalculatedFieldView : UserControl
    {
        /// <summary>
        /// The is using crest level time series property
        /// </summary>
        public static readonly DependencyProperty IsUsingCrestLevelTimeSeriesProperty =
            DependencyProperty.Register(nameof(IsUsingCrestLevelTimeSeries),
                                        typeof(bool),
                                        typeof(GateOpeningHeightCalculatedFieldView),
                                        new PropertyMetadata(default(bool),
                                                             OnDependentPropertyChanged));

        /// <summary>
        /// The is using gate lower edge level time series property
        /// </summary>
        public static readonly DependencyProperty IsUsingGateLowerEdgeLevelTimeSeriesProperty =
            DependencyProperty.Register(nameof(IsUsingGateLowerEdgeLevelTimeSeries),
                                        typeof(bool),
                                        typeof(GateOpeningHeightCalculatedFieldView),
                                        new PropertyMetadata(default(bool),
                                                             OnDependentPropertyChanged));

        /// <summary>
        /// The crest level property
        /// </summary>
        public static readonly DependencyProperty CrestLevelProperty =
            DependencyProperty.Register(nameof(CrestLevel),
                                        typeof(double),
                                        typeof(GateOpeningHeightCalculatedFieldView),
                                        new PropertyMetadata(default(double),
                                                             OnDependentPropertyChanged));

        /// <summary>
        /// The gate lower edge level property
        /// </summary>
        public static readonly DependencyProperty GateLowerEdgeLevelProperty =
            DependencyProperty.Register(nameof(GateLowerEdgeLevel),
                                        typeof(double),
                                        typeof(GateOpeningHeightCalculatedFieldView),
                                        new PropertyMetadata(default(double),
                                                             OnDependentPropertyChanged));

        /// <summary>
        /// The calculated value property
        /// </summary>
        public static readonly DependencyProperty CalculatedValueProperty =
            DependencyProperty.Register(nameof(CalculatedValue),
                                        typeof(double?),
                                        typeof(GateOpeningHeightCalculatedFieldView),
                                        new PropertyMetadata(default(double?)));

        /// <summary>
        /// Initializes a new instance of the <see cref="GateOpeningHeightCalculatedFieldView"/> class.
        /// </summary>
        public GateOpeningHeightCalculatedFieldView()
        {
            InitializeComponent();
            CalculatedValue = GateLowerEdgeLevel - CrestLevel;
        }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is using crest level time series.
        /// </summary>
        public bool IsUsingCrestLevelTimeSeries
        {
            get => (bool) GetValue(IsUsingCrestLevelTimeSeriesProperty);
            set => SetValue(IsUsingCrestLevelTimeSeriesProperty, value);
        }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is using gate lower edge level time series.
        /// </summary>
        public bool IsUsingGateLowerEdgeLevelTimeSeries
        {
            get => (bool) GetValue(IsUsingGateLowerEdgeLevelTimeSeriesProperty);
            set => SetValue(IsUsingGateLowerEdgeLevelTimeSeriesProperty, value);
        }

        /// <summary>
        /// Gets or sets the crest level.
        /// </summary>
        public double CrestLevel
        {
            get => (double) GetValue(CrestLevelProperty);
            set => SetValue(CrestLevelProperty, value);
        }

        /// <summary>
        /// Gets or sets the gate lower edge level.
        /// </summary>
        public double GateLowerEdgeLevel
        {
            get => (double) GetValue(GateLowerEdgeLevelProperty);
            set => SetValue(GateLowerEdgeLevelProperty, value);
        }

        /// <summary>
        /// Gets or sets the calculated value.
        /// </summary>
        public double? CalculatedValue
        {
            get => (double?) GetValue(CalculatedValueProperty);
            set => SetValue(CalculatedValueProperty, value);
        }

        private static void OnDependentPropertyChanged(DependencyObject dependencyObject,
                                                       DependencyPropertyChangedEventArgs eventArgs)
        {
            if (!(dependencyObject is GateOpeningHeightCalculatedFieldView control))
            {
                return;
            }

            if (control.IsUsingCrestLevelTimeSeries ||
                control.IsUsingGateLowerEdgeLevelTimeSeries)
            {
                control.CalculatedValue = null;
            }
            else
            {
                control.CalculatedValue = control.GateLowerEdgeLevel - control.CrestLevel;
            }
        }
    }
}