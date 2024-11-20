using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows.Forms;
using DelftTools.Controls.Swf.Table;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Concepts;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Gui.Controls;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Gui.Concepts
{
    public partial class SacramentoUnitHydrographControl : UserControl
    {
        private SacramentoData data;

        private class UnitHydrographValueWrapper
        {
            private readonly int index;
            private readonly SacramentoData data;

            public UnitHydrographValueWrapper(SacramentoData d, int i)
            {
                data = d;
                index = i;
            }

            public double Value
            {
                get { return data.HydrographValues[index]; }
                set { data.HydrographValues[index] = value; }
            }
        }
        
        private readonly TableView tableView;

        public SacramentoUnitHydrographControl()
        {
            tableView = new TableView
                {
                    AllowAddNewRow = false,
                    AllowDeleteRow = false,
                    Dock = DockStyle.Fill,
                    ShowRowNumbers = true
                };

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
                    sacramentoBindingSource.DataSource = typeof(SacramentoData);
                    data.HydrographValues.CollectionChanged -= HydrographValuesChanged;
                }
                data = value;                
                if (data != null)
                {
                    sacramentoBindingSource.DataSource = data;
                    tableView.Data = CreateBindingList(data);
                    ConfigureColumn();
                    data.HydrographValues.CollectionChanged += HydrographValuesChanged;
                }
            }
        }

        private void HydrographValuesChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            tableView.Invalidate(true);
            tableView.RefreshData();
        }

        private void ConfigureColumn()
        {
            if (tableView.Columns.Count != 0)
            {
                tableView.AllowColumnFiltering = false;
                tableView.Columns[0].SortingAllowed = false;
            }
        }

        private static BindingList<UnitHydrographValueWrapper> CreateBindingList(SacramentoData sacramentoData)
        {
            var list = new BindingList<UnitHydrographValueWrapper>();
            for (var i = 0; i < sacramentoData.HydrographValues.Count; i++)
            {
                list.Add(new UnitHydrographValueWrapper(sacramentoData, i));
            }
            return list;
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
