using System.Drawing;
using System.Windows.Forms;
using DelftTools.Controls;
using DelftTools.Hydro.Structures.WeirFormula;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.StructureFeatureView.WeirFormulaViews
{
    public partial class PierWeirFormulaView : UserControl, IView
    {
        private Image image;

        private PierWeirFormula data;

        public PierWeirFormulaView()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Gets or sets data shown by this view. Usually it is any object in the system which can be shown by some IView derived class.
        /// </summary>
        object IView.Data
        {
            get { return Data; }
            set { Data = (PierWeirFormula) value; }
        }

        /// <summary>
        /// Sets or gets image set on the title of the view.
        /// </summary>
        public Image Image
        {
            get { return image; }
            set { image = value; }
        }

        public void EnsureVisible(object item) { }
        public ViewInfo ViewInfo { get; set; }

        /// <summary>
        /// Gets or sets data shown by this view. Usually it is any object in the system which can be shown by some IView derived class.
        /// </summary>
        public PierWeirFormula Data
        {
            get { return data; }
            set
            {
                data = value;
                bindingSourcePierWeirFormula.DataSource = data;
            }
        }
    }
}