using System;
using System.Drawing;
using System.Windows.Forms;
using DelftTools.Controls;
using DelftTools.Hydro.Structures;
using DelftTools.Hydro.Structures.WeirFormula;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.StructureFeatureView.WeirFormulaViews
{
    public partial class GeneralStructureWeirFormulaView : UserControl, IView
    {
        public GeneralStructureWeirFormulaView()
        {
            InitializeComponent();
            comboBoxGateOpeningDirection.DataSource = Enum.GetValues(typeof(GateOpeningDirection));
        }


        /// <summary>
        /// Sets or gets image set on the title of the view.
        /// </summary>
        public Image Image { get; set; }

        public void EnsureVisible(object item) { }
        public ViewInfo ViewInfo { get; set; }

        private GeneralStructureWeirFormula data;

        /// <summary>
        /// Gets or sets data shown by this view. Usually it is any object in the system which can be shown by some IView derived class.
        /// </summary>
        public object Data
        {
            get { return data; }
            set
            {
                data = (GeneralStructureWeirFormula) value;
                bindingSourceGeneralStructure.DataSource = data;
            }
        }
    }
}