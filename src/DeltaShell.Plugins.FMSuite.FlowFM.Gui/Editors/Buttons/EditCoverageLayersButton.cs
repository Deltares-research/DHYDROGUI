using System;
using System.Drawing;
using System.Windows.Forms;
using DelftTools.Controls.Swf.DataEditorGenerator;
using DelftTools.Controls.Swf.DataEditorGenerator.Metadata;
using DelftTools.Utils.Editing;
using DeltaShell.Plugins.FMSuite.Common.Gui.Forms;
using DeltaShell.Plugins.FMSuite.FlowFM.CoverageDefinition;
using DeltaShell.Plugins.FMSuite.FlowFM.FeatureData;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Gui.Editors.Buttons
{
    public class EditCoverageLayersButton : ICustomControlHelper
    {
        private TextBox text;
        private Label label;
        private Control thePanel;

        public Control CreateControl()
        {
            const int labelWidth = DataEditorGeneratorSwf.LabelWidth;
            const int editorWidth = DataEditorGeneratorSwf.EditorWidth;
            const int height = DataEditorGeneratorSwf.DefaultHeight;
            const int buttonWidth = 26;

            var panel = new Panel
            {
                Width = labelWidth + editorWidth + buttonWidth + 5,
                Height = height
            };
            var button = new Button
            {
                Text = "",
                Width = buttonWidth,
                Height = height,
                Image = Common.Gui.Properties.Resources.waterLayers,
                TextImageRelation = TextImageRelation.ImageBeforeText,
                Dock = DockStyle.Left
            };

            var tooltip = new ToolTip();
            tooltip.SetToolTip(button, "Edit number of depth layers.");

            var marginPanel1 = new Panel {Width = 2, Dock = DockStyle.Left};

            label = new Label
            {
                Text = "Depth layers",
                TextAlign = ContentAlignment.MiddleLeft,
                Width = labelWidth,
                Dock = DockStyle.Left
            };
            text = new TextBox
            {
                Width = editorWidth,
                ReadOnly = true,
                Dock = DockStyle.Fill
            };
            var paddingPanel = new Panel
            {
                Dock = DockStyle.Left,
                Width = editorWidth,
                Padding = new Padding(0, 3, 0, 0)
            };
            paddingPanel.Controls.Add(text);

            var marginPanel2 = new Panel
            {
                Width = 3,
                Dock = DockStyle.Left
            };

            panel.Controls.Add(button);
            panel.Controls.Add(marginPanel2);
            panel.Controls.Add(paddingPanel);
            panel.Controls.Add(marginPanel1);
            panel.Controls.Add(label);
            button.Click += ButtonClick;

            this.thePanel = panel;
            return panel;
        }

        private void ButtonClick(object sender, EventArgs e)
        {
            var dialog = new VerticalProfileDialog();
            dialog.SetSupportedProfileTypes(SupportedVerticalProfileTypes.InitialConditionProfileTypes);
            dialog.ModelDepthLayerDefinition = waterFlowFMModel.DepthLayerDefinition;
            dialog.VerticalProfileDefinition = depthLayerDefinition.VerticalProfile;

            dialog.ShowDialog();
            if (dialog.DialogResult == DialogResult.OK)
            {
                waterFlowFMModel.BeginEdit(new DefaultEditAction("replacing salinity vertical profile definition"));
                depthLayerDefinition.VerticalProfile = dialog.VerticalProfileDefinition;
                waterFlowFMModel.EndEdit();
                UpdateLabelText();
            }
        }

        private WaterFlowFMModel waterFlowFMModel;

        private CoverageDepthLayersList depthLayerDefinition;

        public string DepthLayersPrefix { get; set; }

        public void SetData(Control control, object rootObject, object propertyValue)
        {
            waterFlowFMModel = (WaterFlowFMModel) rootObject;
            depthLayerDefinition = (CoverageDepthLayersList) propertyValue;
            UpdateLabelText();
        }

        private void UpdateLabelText()
        {
            label.Text = DepthLayersPrefix == null ? "" : DepthLayersPrefix + " depth layers";
            text.Text = DepthLayersToString;
        }

        private string DepthLayersToString
        {
            get { return depthLayerDefinition == null ? "" : depthLayerDefinition.VerticalProfile.Type.ToString(); }
        }

        public bool HideCaptionAndUnitLabel()
        {
            return true;
        }

        public bool Enabled
        {
            set
            {
                thePanel.Enabled = value;
            }
        }
    }
}
