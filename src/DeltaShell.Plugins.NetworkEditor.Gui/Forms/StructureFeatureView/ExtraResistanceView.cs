using System.Drawing;
using System.Windows.Forms;
using DelftTools.Controls;
using DelftTools.Functions;
using DelftTools.Hydro.Structures;
using DeltaShell.Plugins.CommonTools.Gui.Forms.Functions;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.StructureFeatureView
{
    public partial class ExtraResistanceView : UserControl, IView
    {
        private IExtraResistance extraResistance;

        public ExtraResistanceView()
        {
            InitializeComponent();
        }

        public object Data
        {
            get { return extraResistance; }
            set
            {
                extraResistance = (IExtraResistance) value;
                // binding via bindingsource caused problem

                if (extraResistance == null)
                {
                    return;
                }
            }
        }

        public Image Image { get; set; }
        public void EnsureVisible(object item) { }
        public ViewInfo ViewInfo { get; set; }

        private void buttonTable_Click(object sender, System.EventArgs e)
        {
            var dialogData = (IFunction)extraResistance.FrictionTable.Clone();
            var editFunctionDialog = new EditFunctionDialog
            {
                Text = "Extra resistance table",
                Data = dialogData,
                ColumnNames = new[]
                {
                    "Water Level [m above datum]",
                    "KSI [s2/m5]"
                },
                ShowOnlyFirstWordInColumnHeadersOnLoad = false
            };

            if (DialogResult.OK == editFunctionDialog.ShowDialog())
            {
                extraResistance.FrictionTable = dialogData;
            }
        }

    }
}
