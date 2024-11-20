using System;
using System.Drawing;
using System.Windows.Forms;
using DelftTools.Controls;
using DelftTools.Functions;
using DelftTools.Hydro.Area.Objects.StructureObjects;
using DeltaShell.Plugins.CommonTools.Gui.Forms.Charting;
using DeltaShell.Plugins.CommonTools.Gui.Forms.Functions;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.StructureFeatureView
{
    public partial class PumpView : UserControl, IView
    {
        private IPump pump;

        public PumpView()
        {
            InitializeComponent();
        }

        public IPump Data
        {
            get => pump;
            set
            {
                pump = value;

                if (pump != null)
                {
                    ipumpBindingSource.DataSource = pump;

                    ConfigureTimeDependentControls();
                }
            }
        }

        object IView.Data
        {
            get => Data;
            set => Data = (Pump) value;
        }

        public Image Image { get; set; }

        public ViewInfo ViewInfo { get; set; }

        public void EnsureVisible(object item)
        {
            // Nothing to be done, enforced through IView
        }

        private void ConfigureTimeDependentControls()
        {
            if (pump != null)
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

        private void TextBoxCapacityTextChanged(object sender, EventArgs e)
        {
            if (double.TryParse(textBoxCapacity.Text, out double capacity))
            {
                lblConvertedCapacaties.Text = $"(= {capacity * 60.0:0.###} m3/min, {capacity * 3600.0:0.##} m3/h)";
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