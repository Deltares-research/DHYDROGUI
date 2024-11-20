using System;
using System.Windows;
using System.Windows.Controls;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.StructureFeatureView
{
    /// <summary>
    /// Interaction logic for SingleTextBoxWithLabelAndUnitControl.xaml
    /// </summary>
    public partial class SingleTextBoxWithLabelAndUnitControl : UserControl
    {
        #region Label DP

        /// <summary>
        /// Gets or sets the Label which is displayed next to the field
        /// </summary>
        public String Label
        {
            get { return (String)GetValue(LabelProperty); }
            set { SetValue(LabelProperty, value); }
        }

        /// <summary>
        /// Identified the Label dependency property
        /// </summary>
        public static readonly DependencyProperty LabelProperty =
            DependencyProperty.Register("Label", typeof(string),
              typeof(SingleTextBoxWithLabelAndUnitControl), new PropertyMetadata(""));

        #endregion

        #region Unit DP

        /// <summary>
        /// Gets or sets the Label which is displayed next to the field
        /// </summary>
        public String Unit
        {
            get { return (String)GetValue(UnitProperty); }
            set { SetValue(UnitProperty, value); }
        }

        /// <summary>
        /// Identified the Unit dependency property
        /// </summary>
        public static readonly DependencyProperty UnitProperty =
            DependencyProperty.Register("Unit", typeof(string),
              typeof(SingleTextBoxWithLabelAndUnitControl), new PropertyMetadata(""));

        #endregion

        #region Value DP

        /// <summary>
        /// Gets or sets the Value which is being displayed
        /// </summary>
        public object Value
        {
            get { return (object)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        /// <summary>
        /// Identified the Label dependency property
        /// </summary>
        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register("Value", typeof(object),
              typeof(SingleTextBoxWithLabelAndUnitControl), new PropertyMetadata(null));

        #endregion

        public SingleTextBoxWithLabelAndUnitControl()
        {
            InitializeComponent();
        }
    }
}
