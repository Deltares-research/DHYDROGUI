using DelftTools.Controls;
using DelftTools.Hydro.Structures;
using Image = System.Drawing.Image;

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
            set { WeirViewModel.Weir = (IWeir) value; }
        }

        public string Text { get; set; }

        public Image Image { get; set; }

        public bool Visible { get; private set; }

        public ViewInfo ViewInfo { get; set; }
        
        public void EnsureVisible(object item)
        {

        }

        public void Dispose()
        {
        }
    }
}
