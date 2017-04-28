using System.Windows.Forms;
using DelftTools.Functions;
using DelftTools.Hydro;
using DeltaShell.Plugins.CommonTools.Gui.Forms.Functions;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.Gui.Forms
{
    public partial class RoughnessAsFunctionOfView : Form
    {
        private FunctionView FunctionView { get; set; }
        private string FunctionOfColumnTitle { get; set; }
        private RoughnessType RoughnessType { get; set; }
        private string RoughnessUnit { get; set; }

        public RoughnessAsFunctionOfView(string functionOfColumnTitle, string branchName, RoughnessType roughnessType, string roughnessUnit)
        {
            InitializeComponent();
            FunctionView = new FunctionView { Dock = DockStyle.Fill };
            FunctionOfColumnTitle = functionOfColumnTitle;
            functionGroupBox.Controls.Add(FunctionView);
            Text = "Roughness as function of " + functionOfColumnTitle + " for channel '" + branchName + "'";
            RoughnessType = roughnessType;
            RoughnessUnit = roughnessUnit;
        }

        private IFunction data;
        public IFunction Data
        {
            set
            {
                data = value;
                FunctionView.Data = RoughnessFunctionConvertor.ConvertFunctionOfToTableWithChainageColumns(data, FunctionOfColumnTitle);
                FunctionView.ChartView.Chart.LeftAxis.Title = string.Format("{0} [{1}]", RoughnessType, RoughnessUnit);
            }
        }

        private void BtnOkClick(object sender, System.EventArgs e)
        {
            // since we are using the clone, cancel does nothing
            RoughnessFunctionConvertor.ConvertTableWithChainageColumnsToFunctionOf((IFunction) FunctionView.Data, data);
        }
    }
}
