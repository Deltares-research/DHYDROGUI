using System.Drawing;
using System.Windows.Forms;
using DelftTools.Controls;
using DelftTools.Hydro.CrossSections.StandardShapes;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.CrossSectionView.StandardCrossSections
{
    public partial class CrossSectionStandardShapeRoundView : UserControl,IView
    {
        public CrossSectionStandardShapeRoundView()
        {
            InitializeComponent();
        }

        public object Data
        {
            get
            {
                return bindingSourceShape.DataSource as CrossSectionStandardShapeCircle;
            }
            set
            {
                bindingSourceShape.DataSource = value;
            }
        }

        public Image Image
        {
            get; set;
        }

        public void EnsureVisible(object item)
        {
            
        }

        public ViewInfo ViewInfo { get; set; }
    }
}
