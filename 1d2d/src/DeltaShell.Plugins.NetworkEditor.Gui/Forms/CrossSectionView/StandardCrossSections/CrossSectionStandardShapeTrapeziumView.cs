using System.Drawing;
using System.Windows.Forms;
using DelftTools.Controls;
using DelftTools.Hydro.CrossSections.StandardShapes;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.CrossSectionView.StandardCrossSections
{
    public partial class CrossSectionStandardShapeTrapeziumView : UserControl,IView
    {
        public CrossSectionStandardShapeTrapeziumView()
        {
            InitializeComponent();
        }

        public object Data
        {
            get
            {
                return bindingSourceTrapezium.DataSource as CrossSectionStandardShapeTrapezium;
            }
            set
            {
                bindingSourceTrapezium.DataSource = value;
                trapeziumErrorProvider.DataSource = bindingSourceTrapezium;
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
