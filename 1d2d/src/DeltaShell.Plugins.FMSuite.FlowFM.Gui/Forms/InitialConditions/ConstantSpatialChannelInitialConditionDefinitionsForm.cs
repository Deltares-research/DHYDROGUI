using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using DelftTools.Hydro;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.NGHS.IO.DataObjects.InitialConditions;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Gui.Forms.InitialConditions
{
    public partial class ConstantSpatialChannelInitialConditionDefinitionsForm : Form
    {
        private readonly IEventedList<ConstantSpatialChannelInitialConditionDefinition> originalInitialConditionDefinitions;

        private List<ConstantSpatialChannelInitialConditionDefinition> editableInitialConditionDefinitions;

        public ConstantSpatialChannelInitialConditionDefinitionsForm(
            IEventedList<ConstantSpatialChannelInitialConditionDefinition> constantSpatialChannelInitialConditionDefinitions,
            InitialConditionQuantity quantity,
            string channelName)
        {
            InitializeComponent();

            Text = $"{quantity} initial condition for branch {channelName}";

            originalInitialConditionDefinitions = constantSpatialChannelInitialConditionDefinitions;
            editableInitialConditionDefinitions = constantSpatialChannelInitialConditionDefinitions.Select(d => new ConstantSpatialChannelInitialConditionDefinition
            {
                Chainage = d.Chainage,
                Value = d.Value
            }).ToList();

            SetBindingList();

            tableView.AddColumn(nameof(ConstantSpatialChannelInitialConditionDefinition.Chainage), "Chainage");
            tableView.AddColumn(nameof(ConstantSpatialChannelInitialConditionDefinition.Value), "Value");
        }

        private void SetBindingList()
        {
            tableView.Data = new BindingList<ConstantSpatialChannelInitialConditionDefinition>(editableInitialConditionDefinitions);
        }

        private void OnOkButtonClick(object sender, EventArgs e)
        {
            originalInitialConditionDefinitions.Clear();
            originalInitialConditionDefinitions.AddRange(editableInitialConditionDefinitions);
        }

        private void OnCancelButtonClick(object sender, EventArgs e)
        {
            // Since we are using a copy, cancel does nothing
        }

        private void OnOrderByChainageButtonClick(object sender, EventArgs e)
        {
            editableInitialConditionDefinitions = editableInitialConditionDefinitions.OrderBy(d => d.Chainage).ToList();

            SetBindingList();
        }
    }
}
