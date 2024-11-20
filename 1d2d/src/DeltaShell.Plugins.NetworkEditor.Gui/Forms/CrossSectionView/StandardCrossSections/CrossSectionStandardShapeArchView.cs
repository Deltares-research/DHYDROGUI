using System.Drawing;
using System.Windows.Forms;
using DelftTools.Controls;
using DelftTools.Hydro.CrossSections.StandardShapes;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.CrossSectionView.StandardCrossSections
{
    public partial class CrossSectionStandardShapeArchView : UserControl, IView
    {
        public CrossSectionStandardShapeArchView()
        {
            InitializeComponent();
        }

        public object Data
        {
            get
            {
                return crossSectionStandardShapeArchBindingSource.DataSource as CrossSectionStandardShapeArch;
            }
            set
            {
                crossSectionStandardShapeArchBindingSource.DataSource = value;
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
