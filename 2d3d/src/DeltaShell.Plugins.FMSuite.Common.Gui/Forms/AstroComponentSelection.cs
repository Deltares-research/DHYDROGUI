using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using DeltaShell.Plugins.FMSuite.Common.FeatureData;

namespace DeltaShell.Plugins.FMSuite.Common.Gui.Forms
{
    public partial class AstroComponentSelection : Form
    {
        private IDictionary<string, double> astroComponents;
        private bool periodRepresentation;

        public AstroComponentSelection()
        {
            InitializeComponent();
            const double factor = 360 / (2 * Math.PI);
            AstroComponents = HarmonicComponent.DefaultAstroComponentsRadPerHour.ToDictionary(kvp => kvp.Key,
                                                                                              kvp => factor * kvp.Value);
            dataGridView1.KeyDown += dataGridView1_KeyDown;
            dataGridView1.CellMouseClick += dataGridView1_CellMouseClick;
        }

        public AstroComponentSelection(IDictionary<string, double> astroComponents)
        {
            InitializeComponent();
            AstroComponents = astroComponents;
            dataGridView1.KeyDown += dataGridView1_KeyDown;
            dataGridView1.CellMouseClick += dataGridView1_CellMouseClick;
        }

        // Astronomical tidal modes, name mapped to frequency (in deg/h)
        public IDictionary<string, double> AstroComponents
        {
            get
            {
                return astroComponents;
            }
            set
            {
                ClearListView();
                astroComponents = value;
                FillListView();
            }
        }

        public IEnumerable<KeyValuePair<string, double>> SelectedComponents
        {
            get
            {
                foreach (DataGridViewRow row in dataGridView1.Rows.OfType<DataGridViewRow>())
                {
                    if ((bool) row.Cells[0].Value)
                    {
                        var name = (string) row.Cells[1].Value;
                        yield return new KeyValuePair<string, double>(name, AstroComponents[name]);
                    }
                }
            }
        }

        public void SelectComponents(IEnumerable<string> selection)
        {
            dataGridView1.ClearSelection();
            foreach (string component in selection)
            {
                DataGridViewRow row = dataGridView1.Rows.OfType<DataGridViewRow>().FirstOrDefault(r => Equals(r.Cells[1].Value, component));
                if (row != null)
                {
                    row.Cells[0].Value = true;
                    row.Selected = true;
                }
            }
        }

        private bool PeriodRepresentation
        {
            get
            {
                return periodRepresentation;
            }
            set
            {
                if (value != periodRepresentation)
                {
                    periodRepresentation = value;
                    SwitchToPeriodRepresentation();
                }
            }
        }

        private void dataGridView1_CellMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (dataGridView1.CurrentCell.ColumnIndex == 0)
            {
                dataGridView1.CurrentCell.Value = !(bool) dataGridView1.CurrentCell.Value;
            }
        }

        private void dataGridView1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Space)
            {
                foreach (DataGridViewRow selectedRow in dataGridView1.SelectedRows.OfType<DataGridViewRow>())
                {
                    selectedRow.Cells[0].Value = !(bool) selectedRow.Cells[0].Value;
                }
            }
            else if (e.KeyCode == Keys.Escape)
            {
                buttonCancel.PerformClick();
            }
            else if (e.KeyCode == Keys.Enter)
            {
                buttonOk.PerformClick();
            }
            else if (e.KeyCode == Keys.Tab)
            {
                if (dataGridView1.EditingControl != null)
                {
                    dataGridView1.CurrentCell.DetachEditingControl();
                }

                SelectNextControl(dataGridView1, true, true, true, true);
            }
        }

        private void ClearListView()
        {
            dataGridView1.SuspendLayout();
            dataGridView1.Rows.Clear();
            dataGridView1.ResumeLayout();
        }

        private void FillListView()
        {
            dataGridView1.SuspendLayout();
            Column3.HeaderText = PeriodRepresentation ? "Period [h]" : "Frequency [deg/h]";

            foreach (KeyValuePair<string, double> astroComponent in AstroComponents)
            {
                dataGridView1.Rows.Add(new object[]
                {
                    false,
                    astroComponent.Key,
                    FrequencyToString(astroComponent.Value)
                });
            }

            dataGridView1.ResumeLayout();
        }

        private void SwitchToPeriodRepresentation()
        {
            Column3.HeaderText = PeriodRepresentation ? "Period [h]" : "Frequency [deg/h]";
            foreach (object row in dataGridView1.Rows)
            {
                var astroComponent = ((DataGridViewRow) row).Cells[1].Value as string;
                ((DataGridViewRow) row).Cells[2].Value = FrequencyToString(AstroComponents[astroComponent]);
            }
        }

        private string FrequencyToString(double frequency)
        {
            return PeriodRepresentation
                       ? (360 / frequency).ToString("0.00")
                       : frequency.ToString("0.00");
        }

        private void buttonOk_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            Close();
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void periodCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            PeriodRepresentation = periodCheckBox.Checked;
        }
    }
}