using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using DelftTools.Controls;
using DelftTools.Functions;
using DelftTools.Hydro.Structures;
using DeltaShell.Plugins.CommonTools.Gui.Forms.Charting;
using DeltaShell.Plugins.CommonTools.Gui.Forms.Functions;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.StructureFeatureView
{
    public partial class PumpView : UserControl, IView
    {
        private IPump pump;
        private bool settingCheckBoxes;

        public PumpView()
        {
            InitializeComponent();
            radioButtonPositive.CheckedChanged += RadioButtonPositiveCheckedChanged;
        }

        public IPump Data
        {
            get
            {
                return pump;
            }
            set
            {
                if (pump != null)
                {
                    ((INotifyPropertyChanged) pump).PropertyChanged -= PumpPropertyChanged;
                }

                pump = value;

                if (pump == null)
                {
                    return;
                }

                ipumpBindingSource.DataSource = (object) pump ?? typeof(IPump);

                if (pump != null)
                {
                    SetSideCheckboxes(pump.ControlDirection);
                    ((INotifyPropertyChanged) pump).PropertyChanged += PumpPropertyChanged;
                }

                ConfigureTimeDependentControls();
                ConfigureVisibilityControls();
            }
        }

        object IView.Data
        {
            get
            {
                return Data;
            }
            set
            {
                Data = (Pump) value;
            }
        }

        public Image Image { get; set; }

        public ViewInfo ViewInfo { get; set; }

        public void EnsureVisible(object item) {}

        private void RadioButtonPositiveCheckedChanged(object sender, EventArgs e)
        {
            bool expectedForRadioButtonNegative = !radioButtonPositive.Checked;

            if (radioButtonNegative.Checked != expectedForRadioButtonNegative)
            {
                radioButtonNegative.Checked = expectedForRadioButtonNegative;
            }
        }

        private void SetSideCheckboxes(PumpControlDirection direction)
        {
            settingCheckBoxes = true;

            switch (direction)
            {
                case PumpControlDirection.SuctionSideControl:
                    checkBoxSuction.Checked = true;
                    checkBoxDelivery.Checked = false;
                    break;
                case PumpControlDirection.DeliverySideControl:
                    checkBoxSuction.Checked = false;
                    checkBoxDelivery.Checked = true;
                    break;
                case PumpControlDirection.SuctionAndDeliverySideControl:
                    checkBoxSuction.Checked = true;
                    checkBoxDelivery.Checked = true;
                    break;
                default:
                    throw new ArgumentOutOfRangeException("direction");
            }

            settingCheckBoxes = false;
        }

        private void ConfigureVisibilityControls()
        {
            if (pump != null && pump.Branch == null)
            {
                radioButtonNegative.Visible = false;
                radioButtonPositive.Visible = false;
                controlLevelsGroupBox.Visible = false;
                yOffsetLabel.Visible = false;
                yOffsetTextBox.Visible = false;
            }
            else
            {
                radioButtonNegative.Visible = true;
                radioButtonPositive.Visible = true;
            }
        }

        private void ConfigureTimeDependentControls()
        {
            if (pump != null && pump.CanBeTimedependent)
            {
                UseTimeDependentLabel.Visible = true;
                useTimeDependentCapacityCheckBox.Visible = true;
                useTimeDependentCapacityCheckBox.Checked = pump.UseCapacityTimeSeries;

                ConfigureUseCapacityTimeSeries();
            }
            else
            {
                UseTimeDependentLabel.Visible = false;
                OpenCapacityTimeSeriesButton.Visible = false;
                useTimeDependentCapacityCheckBox.Visible = false;

                textBoxCapacity.Visible = true;
                lblConvertedCapacaties.Visible = true;
            }
        }

        private void ConfigureUseCapacityTimeSeries()
        {
            if (pump.UseCapacityTimeSeries)
            {
                OpenCapacityTimeSeriesButton.Visible = true;
                textBoxCapacity.Visible = false;
                lblConvertedCapacaties.Visible = false;
            }
            else
            {
                OpenCapacityTimeSeriesButton.Visible = false;
                textBoxCapacity.Visible = true;
                lblConvertedCapacaties.Visible = true;
            }
        }

        private void PumpPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(pump.ControlDirection))
            {
                SetSideCheckboxes(pump.ControlDirection);
            }
        }

        private void CheckBoxCheckedChanged(object sender, EventArgs e)
        {
            if (settingCheckBoxes)
            {
                return;
            }

            bool suctionSide = checkBoxSuction.Checked;
            bool deliverySide = checkBoxDelivery.Checked;
            bool bothSides = suctionSide && deliverySide;

            if (!deliverySide && !suctionSide)
            {
                MessageBox.Show("At least one side needs to be enabled.");
                ((CheckBox) sender).Checked = true;
                return;
            }

            var pumpDirection = PumpControlDirection.SuctionAndDeliverySideControl;
            if (suctionSide && !bothSides)
            {
                pumpDirection = PumpControlDirection.SuctionSideControl;
            }

            if (deliverySide && !bothSides)
            {
                pumpDirection = PumpControlDirection.DeliverySideControl;
            }

            pump.ControlDirection = pumpDirection;

            SetSuctionSideControls(suctionSide);
            SetDeliverySideControls(deliverySide);
        }

        private void SetSuctionSideControls(bool enabled)
        {
            txtSuctionOn.Enabled = enabled;
            txtSuctionOff.Enabled = enabled;
        }

        private void SetDeliverySideControls(bool enabled)
        {
            txtDeliveryOn.Enabled = enabled;
            txtDeliveryOff.Enabled = enabled;
        }

        private void buttonReduction_Click(object sender, EventArgs e)
        {
            var dialogData = (IFunction) pump.ReductionTable.Clone();
            var editFunctionDialog = new EditFunctionDialog
            {
                Text = "Reduction curve for " + pump.Name,
                ColumnNames = new[]
                {
                    "Pump Head",
                    "Reduction"
                },
                Data = dialogData,
                ShowOnlyFirstWordInColumnHeadersOnLoad = false
            };

            if (DialogResult.OK == editFunctionDialog.ShowDialog())
            {
                pump.ReductionTable = dialogData;
            }
        }

        private void TextBoxCapacityTextChanged(object sender, EventArgs e)
        {
            double capacity;
            if (double.TryParse(textBoxCapacity.Text, out capacity))
            {
                lblConvertedCapacaties.Text = string.Format("(= {0:0.###} m3/min, {1:0.##} m3/h)", capacity * 60.0,
                                                            capacity * 3600.0);
            }
            else
            {
                lblConvertedCapacaties.Text = "(= ?)";
            }
        }

        private void OpenCapacityTimeSeriesButton_Click(object sender, EventArgs e)
        {
            var dialogData = (TimeSeries) pump.CapacityTimeSeries.Clone();
            var editFunctionDialog = new EditFunctionDialog
            {
                Text = "Capacity time series for " + pump.Name,
                ColumnNames = new[]
                {
                    "Date time",
                    $"Capacity [{capacityUnitLabel.Text}]"
                },
                ChartViewOption = ChartViewOptions.AllSeries,
                Data = dialogData,
                ShowOnlyFirstWordInColumnHeadersOnLoad = false
            };

            if (DialogResult.OK == editFunctionDialog.ShowDialog())
            {
                pump.CapacityTimeSeries.Time.Clear();
                pump.CapacityTimeSeries.Components[0].Clear();
                pump.CapacityTimeSeries.Time.SetValues(dialogData.Time.Values);
                pump.CapacityTimeSeries.Components[0].SetValues(dialogData.Components[0].Values);
            }
        }

        private void useTimeDependentCapacityCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            pump.UseCapacityTimeSeries = useTimeDependentCapacityCheckBox.Checked;
            ConfigureUseCapacityTimeSeries();
        }
    }
}