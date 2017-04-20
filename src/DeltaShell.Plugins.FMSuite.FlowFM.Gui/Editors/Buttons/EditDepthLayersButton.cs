using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using DelftTools.Controls.Swf.DataEditorGenerator;
using DelftTools.Controls.Swf.DataEditorGenerator.Metadata;
using DeltaShell.Plugins.FMSuite.Common.DepthLayers;
using DeltaShell.Plugins.FMSuite.Common.Gui.Forms;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Gui.Editors.Buttons
{
    public class EditDepthLayersButton : ICustomControlHelper
    {
        private TextBox text;
        private WaterFlowFMModel model;

        public Control CreateControl()
        {
            const int labelWidth = DataEditorGeneratorSwf.LabelWidth;
            const int editorWidth = DataEditorGeneratorSwf.EditorWidth;
            const int height = DataEditorGeneratorSwf.DefaultHeight;
            const int buttonWidth = 26;

            var panel = new Panel {Width = labelWidth + editorWidth + buttonWidth + 5, Height = height};
            var button = new Button
                {
                    Text = "",
                    Width = buttonWidth,
                    Height = height,
                    Image = Properties.Resources.waterLayers,
                    TextImageRelation = TextImageRelation.ImageBeforeText,
                    Dock = DockStyle.Left,
                };

            var tooltip = new ToolTip();
            tooltip.SetToolTip(button, "Adjust depth layers");
            
            var marginPanel1 = new Panel { Width = 2, Dock = DockStyle.Left };
            
            var label = new Label { Text = "Depth layers", Width = labelWidth, TextAlign = ContentAlignment.MiddleLeft, Dock = DockStyle.Left };
            text = new TextBox {Width = editorWidth, ReadOnly = true, Dock = DockStyle.Fill};
            var paddingPanel = new Panel {Dock = DockStyle.Left, Width = editorWidth, Padding = new Padding(0, 3, 0, 0)};
            paddingPanel.Controls.Add(text);

            var marginPanel2 = new Panel { Width = 3, Dock = DockStyle.Left };

            panel.Controls.Add(button);
            panel.Controls.Add(marginPanel2);
            panel.Controls.Add(paddingPanel);
            panel.Controls.Add(marginPanel1);
            panel.Controls.Add(label);
            button.Click += ButtonClick;

            return panel;
        }

        private void ButtonClick(object sender, EventArgs e)
        {
            var view = new DepthLayerDialog(WaterFlowFMModelDefinition.SupportedDepthLayerTypes)
                {
                    CanSpecifyLayerThicknesses = WaterFlowFMModelDefinition.CanSpecifyLayerThicknesses,
                    DepthLayerDefinition = model.DepthLayerDefinition.Clone() as DepthLayerDefinition
                };

            if (view.ShowDialog() == DialogResult.OK)
            {
                model.DepthLayerDefinition = view.DepthLayerDefinition;
            }
            UpdateLabel();
        }

        public void SetData(Control control, object rootObject, object propertyValue)
        {
            model = (WaterFlowFMModel)rootObject;

            if (model != null)
                UpdateLabel();
        }

        private void UpdateLabel()
        {
            text.Text = model.DepthLayerDefinition.Description;
        }

        public bool HideCaptionAndUnitLabel()
        {
            return true;
        }
    }
}