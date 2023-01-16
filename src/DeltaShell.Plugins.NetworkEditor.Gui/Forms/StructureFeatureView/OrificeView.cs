using System.Windows.Forms;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.StructureFeatureView
{
    public partial class OrificeView : WeirView
    {
        public OrificeView() : base()
        {
            groupBox1.SuspendLayout();
            tableLayoutPanel2.SuspendLayout();
            SuspendLayout();
            groupBox1.Text = "Orifice Properties";

            label1.Hide();
            var removeLineHeight = comboBoxWeirFormula.Height;
            comboBoxWeirFormula.Hide();

            groupBox1.Controls.Remove(label1);
            groupBox1.Controls.Remove(comboBoxWeirFormula);

            GateHeightLabel.Hide();
            GateHeightUnitLabel.Hide();
            textBoxGateHeight.Hide();
            
            tableLayoutPanel2.Controls.Remove(GateHeightLabel);
            tableLayoutPanel2.Controls.Remove(textBoxGateHeight);
            tableLayoutPanel2.Controls.Remove(GateHeightUnitLabel);
            tableLayoutPanel2.RowCount = 2;

            labelCrestShape.Hide();
            comboBoxCrestShape.Hide();
            groupBox1.Controls.Remove(labelCrestShape);
            groupBox1.Controls.Remove(comboBoxCrestShape);
            foreach (Control control in groupBox1.Controls)
            {
                control.Location = new System.Drawing.Point(control.Location.X, control.Location.Y - removeLineHeight);
            }
            OpenGateOpeningTimeSeriesButton.Location = new System.Drawing.Point(OpenGateOpeningTimeSeriesButton.Location.X, OpenGateOpeningTimeSeriesButton.Location.Y + removeLineHeight);
            groupBox1.ResumeLayout(false);
            groupBox1.PerformLayout();
            tableLayoutPanel2.ResumeLayout(false);
            tableLayoutPanel2.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

    }
}