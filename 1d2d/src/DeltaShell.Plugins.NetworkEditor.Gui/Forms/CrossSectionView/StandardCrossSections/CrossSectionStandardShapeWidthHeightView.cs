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
                RefreshView();
            }
        }
        private void RefreshView()
        {
            if (Data is ICrossSectionStandardShapeOpenClosed shape)
            {
                tableLayoutPanel1.RowStyles[2].Height = 31F;
                checkboxIsClosedProfile.Checked = shape.Closed;
                checkboxIsClosedProfile.Visible = true;
            }
            else
            {
                tableLayoutPanel1.RowStyles[2].Height = 0;
                checkboxIsClosedProfile.Visible = false;
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
