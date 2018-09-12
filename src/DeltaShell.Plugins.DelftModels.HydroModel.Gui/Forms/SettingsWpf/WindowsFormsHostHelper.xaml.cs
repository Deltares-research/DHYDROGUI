using System.Windows.Controls;
using DelftTools.Controls.Swf.DataEditorGenerator;
using Control = System.Windows.Forms.Control;

namespace DeltaShell.Plugins.DelftModels.HydroModel.Gui.Forms.SettingsWpf
{
    /// <summary>
    /// Interaction logic for WindowsFormsHostHelper.xaml
    /// </summary>
    public partial class WindowsFormsHostHelper : UserControl
    {
        private Control _hostedControl;

        public WindowsFormsHostHelper()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Gets or sets the hosted control.
        /// </summary>
        /// <value>
        /// The hosted control.
        /// </value>
        public System.Windows.Forms.Control HostedControl
        {
            get { return _hostedControl; }
            set
            {
                _hostedControl = value;
                if (_hostedControl != null)
                {
                    FormsHost.Child = _hostedControl;
                }
            }
        }
    }
}
