using System;
using System.Drawing;
using System.Windows.Forms;
using DeltaShell.Plugins.FMSuite.Common.FeatureData;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Gui.Forms
{
    /// <summary>
    /// Class responsible for creating the selection dialog when adding a new meteo item.
    /// </summary>
    /// <seealso cref="System.Windows.Forms.Form" />
    internal partial class FmMeteoSelectionDialog : Form
    {

        public FmMeteoSelectionDialog()
        {
            InitializeComponent();
            PrecipitationRadioButton.Checked = true;
            PrecipitationTypeComboBox.SelectedIndex = 0;
        }

        public IFmMeteoField FmMeteoField { get; private set; }

        private void OkButtonClick(object sender, EventArgs e)
        {
            FmMeteoField = CreateMeteoField();
            if (FmMeteoField != null)
            {
                DialogResult = DialogResult.OK;
                Close();
            }
        }

        private IFmMeteoField CreateMeteoField()
        {
            if (PrecipitationRadioButton.Checked)
            {
                var precipitationType = PrecipitationTypeComboBox.SelectedItem is FmMeteoLocationType ? (FmMeteoLocationType)PrecipitationTypeComboBox.SelectedItem : FmMeteoLocationType.Global;

                return Common.FeatureData.FmMeteoField.CreateMeteoPrecipitationSeries(precipitationType);
            }

            return null;
        }

        private void CancelButtonClick(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }
        
        private void PrecipitationTypeComboBox_DrawItem(object sender, DrawItemEventArgs e)
        {
            var comboBox = (ComboBox)sender;
            if (e.Index >= 0)
            {
                if (IsItemDisabled(e.Index))
                {
                    e.Graphics.FillRectangle(SystemBrushes.Window, e.Bounds);
                    e.Graphics.DrawString(comboBox.Items[e.Index].ToString(), comboBox.Font, Brushes.LightGray, e.Bounds);
                }
                else
                {
                    e.DrawBackground();

                    // Set the brush according to whether the item is selected or not
                    var brush = ((e.State & DrawItemState.Selected) > 0) ? SystemBrushes.HighlightText : SystemBrushes.ControlText;

                    e.Graphics.DrawString(comboBox.Items[e.Index].ToString(), comboBox.Font, brush, e.Bounds);
                    e.DrawFocusRectangle();
                }
            }
        }
        void PrecipitationTypeComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (IsItemDisabled(PrecipitationTypeComboBox.SelectedIndex))
                PrecipitationTypeComboBox.SelectedIndex = 0;
        }

        bool IsItemDisabled(int index)
        {
            return index >= 1;
        }
    }
}
