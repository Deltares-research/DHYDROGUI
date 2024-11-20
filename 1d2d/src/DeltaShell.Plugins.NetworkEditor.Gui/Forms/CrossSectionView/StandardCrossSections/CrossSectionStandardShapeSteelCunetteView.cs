using System.Drawing;
using System.Windows.Forms;
using DelftTools.Controls;
using DelftTools.Hydro.CrossSections.StandardShapes;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.CrossSectionView.StandardCrossSections
{
    public partial class CrossSectionStandardShapeSteelCunetteView : UserControl, IView
    {
        public CrossSectionStandardShapeSteelCunetteView()
        {
            InitializeComponent();
        }
        
        public object Data
        {
            get
            {
                return crossSectionStandardShapeSteelCunetteBindingSource.DataSource as CrossSectionStandardShapeSteelCunette;
            }
            set
            {
                crossSectionStandardShapeSteelCunetteBindingSource.DataSource = value;
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
