using System.Windows;
using System.Windows.Controls;
using DeltaShell.NGHS.Common.Gui.Components;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Editors.Structures.Views.Components
{
    /// <summary>
    /// Interaction logic for LabeledTimeSeriesView.xaml
    /// </summary>
    public partial class LabeledTimeSeriesView : UserControl
    {
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
        /// The is time series property
        /// </summary>
        public static readonly DependencyProperty IsTimeSeriesProperty = 
            DependencyProperty.Register(nameof(IsTimeSeries),
                                        typeof(bool), 
                                        typeof(LabeledTimeSeriesView), 
                                        new PropertyMetadata(default(bool)));

        /// <summary>
        /// Creates a new <see cref="LabeledTimeSeriesView"/>.
        /// </summary>
        public LabeledTimeSeriesView()
        {
            InitializeComponent();
        }
        
        /// <summary>
        /// Gets or sets the label of this <see cref="LabeledButton"/>.
        /// </summary>
        public string Label
        {
            get => (string) GetValue(LabelProperty);
            set => SetValue(LabelProperty, value);
        }

        /// <summary>
        /// Gets or sets the single value of this <see cref="LabeledValueBox"/>.
        /// </summary>
        public string SingleValueContent
        {
            get => (string) GetValue(SingleValueContentProperty);
            set => SetValue(SingleValueContentProperty, value);
        }

        /// <summary>
        /// Gets or sets the unit of this <see cref="LabeledValueBox"/>.
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

    }
}
