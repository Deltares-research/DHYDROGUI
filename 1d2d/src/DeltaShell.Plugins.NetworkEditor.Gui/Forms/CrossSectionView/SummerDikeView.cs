using System;
using System.Drawing;
using System.Windows.Forms;
using DelftTools.Controls;
using DelftTools.Hydro.CrossSections;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.CrossSectionView
{
    public partial class SummerDikeView : UserControl, IView
    {
        private SummerDike summerDike;

        public SummerDikeView()
        {
            InitializeComponent();
        }

        private void cbxHasSummerdike_CheckedChanged(object sender, EventArgs e)
        {
            UpdateVisibleState(cbxHasSummerdike.Checked);
        }

        private void UpdateVisibleState(bool show)
        {
            txtCrestLevel.Enabled = show;
            txtFloodplainLevel.Enabled = show;
            txtFloodSurface.Enabled = show;
            txtTotalSurface.Enabled = show;
        }
        
        private void Bind()
        {
            if (summerDike != null)
            {
                UpdateVisibleState(summerDike.Active);

                bindingSourceCrossSection.DataSource = summerDike;
                cbxHasSummerdike.DataBindings.Add("checked", bindingSourceCrossSection,nameof(summerDike.Active));

                txtCrestLevel.DataBindings.Add("Text", bindingSourceCrossSection, nameof(summerDike.CrestLevel), true,
                                               DataSourceUpdateMode.OnValidation, null, "N2");
                txtFloodplainLevel.DataBindings.Add("Text", bindingSourceCrossSection, nameof(summerDike.FloodPlainLevel), true,
                                                    DataSourceUpdateMode.OnValidation, null, "N2");
                txtFloodSurface.DataBindings.Add("Text", bindingSourceCrossSection, nameof(summerDike.FloodSurface), true,
                                                 DataSourceUpdateMode.OnValidation, null, "N2");
                txtTotalSurface.DataBindings.Add("Text", bindingSourceCrossSection, nameof(summerDike.TotalSurface), true,
                                                 DataSourceUpdateMode.OnValidation, null, "N2");
            }
        }
        
        private void Unbind()
        {
            cbxHasSummerdike.DataBindings.Clear();
            txtCrestLevel.DataBindings.Clear();
            txtFloodplainLevel.DataBindings.Clear();
            txtFloodSurface.DataBindings.Clear();
            txtTotalSurface.DataBindings.Clear();
        }

        public object Data
        {
            get { return summerDike; }
            set
            {
                Unbind();
                summerDike = (SummerDike) value;
                Bind();
            }
        }

        public Image Image
        {
            get; set;
        }

        public void EnsureVisible(object item) { }
        public ViewInfo ViewInfo { get; set; }
    }
}
