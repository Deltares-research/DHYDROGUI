using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using DelftTools.Controls;
using DeltaShell.Plugins.CommonTools.Gui.Forms.Functions;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Gui.Controls
{
    public partial class RRBoundarySeriesView : UserControl, IView
    {
        private readonly FunctionView functionView;

        private RainfallRunoffBoundaryData data;
        private bool updating;

        public RRBoundarySeriesView()
        {
            InitializeComponent();
            functionView = new FunctionView {Dock = DockStyle.Fill};
            Controls.Add(functionView);
            functionView.BringToFront();
        }

        #region IView<RainfallRunoffBoundaryData> Members

        public object Data
        {
            get { return data; }
            set
            {
                if (data != null)
                {
                    ((INotifyPropertyChanged) data).PropertyChanged += RainfallRunoffBoundaryDataViewPropertyChanged;
                }

                functionView.Data = null;
                data = (RainfallRunoffBoundaryData) value;

                if (data != null)
                {
                    functionView.Data = data.Data;
                    ((INotifyPropertyChanged) data).PropertyChanged += RainfallRunoffBoundaryDataViewPropertyChanged;
                }
                UpdateView();
            }
        }

        public Image Image { get; set; }

        public void EnsureVisible(object item)
        {
        }

        public ViewInfo ViewInfo { get; set; }

        #endregion

        private void RainfallRunoffBoundaryDataViewPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            UpdateView();
        }

        private void ConstantRadioButtonCheckedChanged(object sender, EventArgs e)
        {
            if (updating)
                return;

            if (Data != null)
            {
                data.IsConstant = constantRadioButton.Checked;
            }
            UpdateView();
        }

        private void UpdateView()
        {
            if (Data == null)
                return;

            updating = true;

            bool isConstant = data.IsConstant;
            constantRadioButton.Checked = isConstant;
            variableRadioButton.Checked = !isConstant;
            waterLevelConstantTextBox.Visible = isConstant;
            label1.Visible = isConstant;
            functionView.Visible = !isConstant;
            if (!isConstant)
            {
                functionView.SetBottomAxisAutomatic();
            }
            waterLevelConstantTextBox.Text = data.Value.ToString();

            updating = false;
        }

        private void WaterLevelConstantTextBoxValidated(object sender, EventArgs e)
        {
            if (Data == null)
                return;

            double val;
            if (Double.TryParse(waterLevelConstantTextBox.Text, out val))
            {
                data.Value = val;
            }
        }
    }
}