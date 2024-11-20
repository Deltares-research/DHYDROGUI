using System.Windows.Controls;
using DelftTools.Controls;
using DelftTools.Hydro.Structures;
using Image = System.Drawing.Image;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.SewerFeatureViews
{
    /// <summary>
    /// Interaction logic for DamBreakView.xaml
    /// </summary>
    public partial class LeveeBreachView : UserControl, IView
    {
        public LeveeBreachView()
        {
            InitializeComponent();
        }

        #region Implementation of IView

        public void Dispose()
        {
        }

        public void EnsureVisible(object item)
        {
        }

        public object Data
        {
            get { return ViewModel.LeveeBreach; }
            set { ViewModel.LeveeBreach = (LeveeBreach) value; }
        }

        public string Text { get; set; }
        public Image Image { get; set; }
        public bool Visible { get; }
        public ViewInfo ViewInfo { get; set; }

        #endregion
    }
}
