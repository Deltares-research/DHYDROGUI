using System.Drawing;
using System.Windows.Forms;
using DelftTools.Controls;
using DelftTools.Hydro.CrossSections.StandardShapes;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.CrossSectionView.StandardCrossSections
{
    public partial class CrossSectionStandardShapeWidthHeightView : UserControl, IView
    {
        public CrossSectionStandardShapeWidthHeightView()
        {
            InitializeComponent();
        }

        public object Data
        {
            get
            {
                return crossSectionStandardShapeWidthHeightBindingSource.DataSource as CrossSectionStandardShapeWidthHeightBase;
            }
            set
            {
                crossSectionStandardShapeWidthHeightBindingSource.DataSource = value;
            }
        }

        public Image Image
        {
            get;set;
        }

        public void EnsureVisible(object item)
        {
        }

        public ViewInfo ViewInfo { get; set; }
    }
}
