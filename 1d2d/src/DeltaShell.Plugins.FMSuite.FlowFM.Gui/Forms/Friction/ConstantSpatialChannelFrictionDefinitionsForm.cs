using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using DelftTools.Hydro;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.NGHS.IO.DataObjects.Friction;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Gui.Forms.Friction
{
    public partial class ConstantSpatialChannelFrictionDefinitionsForm : Form
    {
        private readonly IEventedList<ConstantSpatialChannelFrictionDefinition> originalFrictionDefinitions;

        private List<ConstantSpatialChannelFrictionDefinition> editableFrictionDefinitions;

        public ConstantSpatialChannelFrictionDefinitionsForm(
            IEventedList<ConstantSpatialChannelFrictionDefinition> constantSpatialChannelFrictionDefinitions,
            RoughnessType roughnessType,
            string channelName)
        {
            InitializeComponent();

            Text = RoughnessHelper.GetDialogTitle(roughnessType, channelName);

            originalFrictionDefinitions = constantSpatialChannelFrictionDefinitions;
            editableFrictionDefinitions = constantSpatialChannelFrictionDefinitions.Select(d => new ConstantSpatialChannelFrictionDefinition
            {
                Chainage = d.Chainage,
                Value = d.Value
            }).ToList();

            SetBindingList();

            tableView.AddColumn(nameof(ConstantSpatialChannelFrictionDefinition.Chainage), "Chainage");
            tableView.AddColumn(nameof(ConstantSpatialChannelFrictionDefinition.Value), "Value");
        }

        private void SetBindingList()
        {
            tableView.Data = new BindingList<ConstantSpatialChannelFrictionDefinition>(editableFrictionDefinitions);
        }

        private void OnOkButtonClick(object sender, EventArgs e)
        {
            originalFrictionDefinitions.Clear();
            originalFrictionDefinitions.AddRange(editableFrictionDefinitions);
        }

        private void OnCancelButtonClick(object sender, EventArgs e)
        {
            // Since we are using a copy, cancel does nothing
        }

        private void OnOrderByChainageButtonClick(object sender, EventArgs e)
        {
            editableFrictionDefinitions = editableFrictionDefinitions.OrderBy(d => d.Chainage).ToList();

            SetBindingList();
        }
    }
}
