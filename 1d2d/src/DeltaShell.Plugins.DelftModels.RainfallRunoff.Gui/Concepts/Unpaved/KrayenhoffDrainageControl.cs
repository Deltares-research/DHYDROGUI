using System.Windows.Forms;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Concepts.Unpaved;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Gui.Concepts.Unpaved
{
    public partial class KrayenhoffDrainageControl : UserControl
    {
        private KrayenhoffVanDeLeurDrainageFormula data;

        public KrayenhoffDrainageControl()
        {
            InitializeComponent();
        }

        public KrayenhoffVanDeLeurDrainageFormula Data
        {
            get { return data; }
            set
            {
                data = value;

                if (data != null)
                {
                    krayenhoffVanDeLeurDrainageFormulaBindingSource.DataSource = data;
                }
            }
        }
    }
}