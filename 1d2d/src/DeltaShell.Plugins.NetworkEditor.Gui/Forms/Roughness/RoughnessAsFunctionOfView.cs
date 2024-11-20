using System.Windows.Forms;
using DelftTools.Controls.Swf.Table;
using DelftTools.Functions;
using DelftTools.Hydro;
using DeltaShell.Plugins.CommonTools.Gui.Forms.Functions;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.Roughness
{
    public partial class RoughnessAsFunctionOfView : Form
    {
        private readonly bool isEditable;
        private readonly FunctionView functionView;
        private readonly string leftAxisTitle;
        private readonly string functionOfColumnTitle;

        private IFunction data;

        public RoughnessAsFunctionOfView(string functionOfColumnTitle, string branchName, RoughnessType roughnessType, string roughnessUnit, bool isEditable = true)
        {
            InitializeComponent();

            this.isEditable = isEditable;

            functionView = new FunctionView { Dock = DockStyle.Fill };
            functionGroupBox.Controls.Add(functionView);

            var functionTableView = (TableView) functionView.TableView;
            functionTableView.ReadOnly = !isEditable;
            functionTableView.ShowImportExportToolbar = isEditable;

            btnCancel.Visible = isEditable;

            leftAxisTitle = $"{roughnessType} [{roughnessUnit}]";

            this.functionOfColumnTitle = functionOfColumnTitle;

            Text = RoughnessHelper.GetDialogTitle(roughnessType, branchName);
        }

        public IFunction Data
        {
            set
            {
                data = value;
                functionView.Data = RoughnessFunctionConvertor.ConvertFunctionOfToTableWithChainageColumns(data, functionOfColumnTitle);
                functionView.ChartView.Chart.LeftAxis.Title = leftAxisTitle;
            }
        }

        private void BtnOkClick(object sender, System.EventArgs e)
        {
            if (isEditable)
            {
                RoughnessFunctionConvertor.ConvertTableWithChainageColumnsToFunctionOf((IFunction) functionView.Data, data);
            }
        }

        private void BtnCancelClick(object sender, System.EventArgs e)
        {
            // Since we are using a clone, cancel does nothing
        }
    }
}
