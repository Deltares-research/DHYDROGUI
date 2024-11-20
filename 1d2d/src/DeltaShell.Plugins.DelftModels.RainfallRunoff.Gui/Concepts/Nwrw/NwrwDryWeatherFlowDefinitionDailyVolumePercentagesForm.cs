using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Gui.Concepts.Nwrw
{
    public partial class NwrwDryWeatherFlowDefinitionDailyVolumePercentagesForm : Form
    {
        private double[] originalHourlyPercentages;
        private List<HourlyPercentageOfVolume> editablePercentages;

        public NwrwDryWeatherFlowDefinitionDailyVolumePercentagesForm(double[] hourlyPercentages, string definitionName)
        {
            InitializeComponent();

            Text = definitionName;

            originalHourlyPercentages = hourlyPercentages;
            editablePercentages = hourlyPercentages.Select((value, index) => new HourlyPercentageOfVolume
            {
                Hour = index,
                Percentage = value
            }).ToList();


            SetBindingList();

            tableView.AddColumn(nameof(HourlyPercentageOfVolume.Hour), "Hour", readOnly:true, width: 75, displayFormat:@"00\:\0\0");
            tableView.AddColumn(nameof(HourlyPercentageOfVolume.Percentage), "Percentage", readOnly:false, width:100);
        }

        private void SetBindingList()
        {
            tableView.Data = new BindingList<HourlyPercentageOfVolume>(editablePercentages);
        }

        private void OnCancelButtonClick(object sender, EventArgs e)
        {
            // Since we are using a copy, cancel does nothing
        }

        private void OnOkButtonClick(object sender, EventArgs e)
        {
            var editableHourlyPercentagesArray = editablePercentages
                .OrderBy(hpov => hpov.Hour)
                .Select(hpov => hpov.Percentage)
                .ToArray();

            Array.Clear(originalHourlyPercentages, 0, originalHourlyPercentages.Length);
            Array.Copy(editableHourlyPercentagesArray, 0, 
                originalHourlyPercentages, 0, 
                originalHourlyPercentages.Length);
        }

        private class HourlyPercentageOfVolume
        {
            public int Hour { get; set; }
            public double Percentage { get; set; }
        }
    }

    
}
