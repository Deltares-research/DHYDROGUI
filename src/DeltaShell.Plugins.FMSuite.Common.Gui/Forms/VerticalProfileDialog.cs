using System;
using System.Collections.Generic;
using System.Windows.Forms;
using DeltaShell.Plugins.FMSuite.Common.DepthLayers;
using DeltaShell.Plugins.FMSuite.Common.FeatureData;

namespace DeltaShell.Plugins.FMSuite.Common.Gui.Forms
{
    public partial class VerticalProfileDialog : Form
    {
        public VerticalProfileDialog()
        {
            InitializeComponent();
        }

        public VerticalProfileDefinition VerticalProfileDefinition
        {
            get
            {
                return verticalProfileControl.VerticalProfileDefinition;
            }
            set
            {
                verticalProfileControl.VerticalProfileDefinition = value;
            }
        }

        public DepthLayerDefinition ModelDepthLayerDefinition
        {
            get
            {
                return verticalProfileControl.ModelDepthLayerDefinition;
            }
            set
            {
                verticalProfileControl.ModelDepthLayerDefinition = value;
            }
        }

        public void SetSupportedProfileTypes(IEnumerable<VerticalProfileType> verticalProfileTypes)
        {
            verticalProfileControl.SetSupportedProfileTypes(verticalProfileTypes);
        }

        private void ButtonOkClick(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            Close();
        }

        private void ButtonCancelClick(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }
    }
}