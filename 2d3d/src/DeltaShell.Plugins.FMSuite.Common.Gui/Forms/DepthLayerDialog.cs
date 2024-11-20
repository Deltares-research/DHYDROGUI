using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using DeltaShell.Plugins.FMSuite.Common.DepthLayers;

namespace DeltaShell.Plugins.FMSuite.Common.Gui.Forms
{
    public partial class DepthLayerDialog : Form
    {
        private readonly IList<DepthLayerType> supportedDepthLayerTypes;

        public DepthLayerDialog()
            : this(Enum.GetValues(typeof(DepthLayerType)).Cast<DepthLayerType>()) {}

        public DepthLayerDialog(IEnumerable<DepthLayerType> supportedDepthLayerTypes)
        {
            this.supportedDepthLayerTypes = supportedDepthLayerTypes.ToList();
            InitializeComponent();
            buttonCancel.MouseUp += OnMouseUp;
        }

        public bool CanSpecifyLayerThicknesses
        {
            get
            {
                return depthLayerControl.CanSpecifyThicknesses;
            }
            set
            {
                depthLayerControl.CanSpecifyThicknesses = value;
            }
        }

        public DepthLayerDefinition DepthLayerDefinition
        {
            get
            {
                return depthLayerControl.DepthLayerDefinition;
            }
            set
            {
                depthLayerControl.DepthLayerDefinition = value;
            }
        }

        protected override bool ProcessDialogKey(Keys keyData)
        {
            if (keyData == Keys.Escape)
            {
                AutoValidate = AutoValidate.Disable;
                ButtonCancelClick(buttonCancel, new EventArgs());
                return true;
            }

            return base.ProcessDialogKey(keyData);
        }

        private void ButtonOkClick(object sender, EventArgs e)
        {
            DialogResult = ValidateChildren() ? DialogResult.OK : DialogResult.None;
            Close();
        }

        private void ButtonCancelClick(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void OnMouseUp(object sender, MouseEventArgs mouseEventArgs)
        {
            AutoValidate = AutoValidate.Disable;
            ButtonCancelClick(buttonCancel, new EventArgs());
        }
    }
}