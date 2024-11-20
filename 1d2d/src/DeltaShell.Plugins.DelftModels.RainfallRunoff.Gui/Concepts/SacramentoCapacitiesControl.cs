using System.Windows.Forms;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Concepts;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Gui.Controls;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Gui.Concepts
{
    public partial class SacramentoCapacitiesControl : UserControl
    {
        private SacramentoData data;

        public SacramentoCapacitiesControl()
        {
            InitializeComponent();
            RainfallRunoffFormsHelper.ApplyRealNumberFormatToDataBinding(this);
        }

        public SacramentoData Data
        {
            get { return data; }
            set
            {
                if (data != null)
                {
                    sacramentoBindingSource.DataSource = typeof (SacramentoData);
                }
                data = value;
                if (data != null)
                {
                    sacramentoBindingSource.DataSource = data;
                }
            }
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.Enter)
            {
                var box = ActiveControl as TextBoxBase;
                if (box == null || !box.Multiline)
                {
                    Validate();
                    return true;
                }
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }
    }
}
