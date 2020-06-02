using System.Windows;
using System.Windows.Controls;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.StructureFeatureView
{
    /// <summary>
    /// Interaction logic for DoubleTextBoxWithLabelAndUnitControl.xaml
    /// </summary>
    public partial class DoubleTextBoxWithLabelAndUnitControl : UserControl
    {
        public DoubleTextBoxWithLabelAndUnitControl()
        {
            InitializeComponent();
        }

        #region Label DP

        /// <summary>
        /// Gets or sets the Label which is displayed next to the field
        /// </summary>
        public string Label
        {
            get
            {
                return (string) GetValue(LabelProperty);
            }
            set
            {
                SetValue(LabelProperty, value);
            }
        }

        /// <summary>
        /// Identified the Label dependency property
        /// </summary>
        public static readonly DependencyProperty LabelProperty =
            DependencyProperty.Register("Label", typeof(string),
                                        typeof(DoubleTextBoxWithLabelAndUnitControl), new PropertyMetadata(""));

        #endregion

        #region Unit DP

        /// <summary>
        /// Gets or sets the Label which is displayed next to the field
        /// </summary>
        public string Unit1
        {
            get
            {
                return (string) GetValue(Unit1Property);
            }
            set
            {
                SetValue(Unit1Property, value);
            }
        }

        /// <summary>
        /// Identified the Unit dependency property
        /// </summary>
        public static readonly DependencyProperty Unit1Property =
            DependencyProperty.Register("Unit1", typeof(string),
                                        typeof(DoubleTextBoxWithLabelAndUnitControl), new PropertyMetadata(""));

        public string Unit2
        {
            get
            {
                return (string) GetValue(Unit2Property);
            }
            set
            {
                SetValue(Unit2Property, value);
            }
        }

        /// <summary>
        /// Identified the Unit dependency property
        /// </summary>
        public static readonly DependencyProperty Unit2Property =
            DependencyProperty.Register("Unit2", typeof(string),
                                        typeof(DoubleTextBoxWithLabelAndUnitControl), new PropertyMetadata(""));

        #endregion

        #region Value DP

        /// <summary>
        /// Gets or sets the Value which is being displayed
        /// </summary>
        public object Value1
        {
            get
            {
                return (object) GetValue(Value1Property);
            }
            set
            {
                SetValue(Value1Property, value);
            }
        }

        /// <summary>
        /// Identified the Label dependency property
        /// </summary>
        public static readonly DependencyProperty Value1Property =
            DependencyProperty.Register("Value1", typeof(object),
                                        typeof(DoubleTextBoxWithLabelAndUnitControl), new PropertyMetadata(null));

        /// <summary>
        /// Gets or sets the Value which is being displayed
        /// </summary>
        public object Value2
        {
            get
            {
                return (object) GetValue(Value2Property);
            }
            set
            {
                SetValue(Value2Property, value);
            }
        }

        /// <summary>
        /// Identified the Label dependency property
        /// </summary>
        public static readonly DependencyProperty Value2Property =
            DependencyProperty.Register("Value2", typeof(object),
                                        typeof(DoubleTextBoxWithLabelAndUnitControl), new PropertyMetadata(null));

        #endregion
    }
}