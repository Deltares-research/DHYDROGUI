using System.Windows.Controls;
using DelftTools.Controls;
using DelftTools.Hydro.Structures;
using Image = System.Drawing.Image;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.SewerFeatureViews
{
    /// <summary>
    /// Interaction logic for ManholeView.xaml
    /// </summary>
    public partial class ManholeView : UserControl, IView
    {
        public ManholeView()
        {
            InitializeComponent();
            ViewModel.DeselectItem = () => ManholeVisualisationControl.DeselectItem();
        }

        #region IView implementation

        public object Data
        {
            get { return ViewModel.Manhole; }
            set { ViewModel.Manhole = (Manhole)value; }
        }

        public void Dispose()
        {
        }

        public void EnsureVisible(object item)
        {
        }

        public string Text { get; set; }

        public Image Image { get; set; }

        public bool Visible { get; }

        public ViewInfo ViewInfo { get; set; }

        #endregion
    }
}
