using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using DelftTools.Controls.Swf.DataEditorGenerator;
using DelftTools.Controls.Swf.DataEditorGenerator.Metadata;
using DelftTools.Hydro;
using DelftTools.Shell.Gui;
using DeltaShell.Plugins.FMSuite.Common.Layers;
using DeltaShell.Plugins.SharpMapGis.Gui.Forms;
using GeoAPI.Extensions.CoordinateSystems;
using SharpMap;
using SharpMap.UI.Forms;

namespace DeltaShell.Plugins.FMSuite.Common.Gui.Editors.Buttons
{
    public class SetCoordinateSystemButton : ICustomControlHelper
    {
        private TextBox text;

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
                    Image = Properties.Resources.set_coordinate_system,
                    TextImageRelation = TextImageRelation.ImageBeforeText,
                    Dock = DockStyle.Left
                };

            var tooltip = new ToolTip();
            tooltip.SetToolTip(button, "Set model coordinate system (does not adjust model coordinates, but can affect rendering and model results)");

            var marginPanel1 = new Panel { Width = 2, Dock = DockStyle.Left };
            
            var label = new Label { Text = "Coordinate system", TextAlign = ContentAlignment.MiddleLeft, Width = labelWidth, Dock = DockStyle.Left };
            text = new TextBox {Width = editorWidth, ReadOnly = true, Dock = DockStyle.Fill};
            var paddingPanel = new Panel {Dock = DockStyle.Left, Width = editorWidth, Padding = new Padding(0, 3, 0, 0)};
            paddingPanel.Controls.Add(text);

            var marginPanel2 = new Panel {Width = 3, Dock = DockStyle.Left};

            panel.Controls.Add(button);
            panel.Controls.Add(marginPanel2);
            panel.Controls.Add(paddingPanel);
            panel.Controls.Add(marginPanel1);
            panel.Controls.Add(label);
            button.Click += ButtonClick;

            return panel;
        }

        public Func<ICoordinateSystem, bool> CoordinateSystemFilter { get; set; } 

        void ButtonClick(object sender, EventArgs e)
        {
            var control = new SelectCoordinateSystemDialog(Map.CoordinateSystemFactory.SupportedCoordinateSystems, Map.CoordinateSystemFactory.CustomCoordinateSystems)
            {
                Dock = DockStyle.Fill,
                SelectedCoordinateSystem = ModelWithCoordinateSystem.CoordinateSystem,
                CoordinateSystemFilter = CoordinateSystemFilter
            };

            if (control.ShowDialog() != DialogResult.OK) 
                return;

            var selectedCoordinateSystem = control.SelectedCoordinateSystem;

            if (selectedCoordinateSystem != null &&
                !ModelWithCoordinateSystem.CanSetCoordinateSystem(selectedCoordinateSystem))
            {
                if (MessageBox.Show(string.Format(
                    "The model coordinates do not appear to be in '{0}', as they fall outside the expected range of values for this system. Please verify the selected " +
                    "coordinate system is the system the coordinates were measured in. Continuing could lead to the map visualization failing and unexpected behaviour of spatial operations {1}{1}" +
                    "Are you sure you want to continue?",
                    selectedCoordinateSystem, Environment.NewLine),
                    "Warning: model coordinates do not appear to be in the selected system",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning) == DialogResult.No)
                    return;
            }

            var coordinateSystem = ModelWithCoordinateSystem.CoordinateSystem;

            ModelWithCoordinateSystem.CoordinateSystem = selectedCoordinateSystem;

            var button = sender as Button;
            if (button != null)
            {
                UpdateLabelText();
            }

            var view = Gui.DocumentViews.OfType<ProjectItemMapView>().FirstOrDefault(v => Equals(v.Data, ModelWithCoordinateSystem));

            if (view == null) return;

            view.MapView.MapControl.Visible = false;

            var modelLayer = view.MapView.Map.GetAllLayers(true).OfType<ModelGroupLayer>().First();

            modelLayer.UpdateCoordinateSystem(coordinateSystem, selectedCoordinateSystem);

            view.MapView.MapControl.Visible = true;

            modelLayer.Map.ZoomToFit(modelLayer.Envelope);
        }

        public IGui Gui { get; set; }

        private IHasCoordinateSystem ModelWithCoordinateSystem;
        public void SetData(Control control, object rootObject, object propertyValue)
        {
            ModelWithCoordinateSystem = (IHasCoordinateSystem)rootObject;
            control.Tag = propertyValue;
            UpdateLabelText();
        }

        public bool HideCaptionAndUnitLabel()
        {
            return true;
        }

        public void UpdateLabelText()
        {
            text.Text = CoordinateSystemName;
        }

        private string CoordinateSystemName
        {
            get
            {
                return (ModelWithCoordinateSystem != null && ModelWithCoordinateSystem.CoordinateSystem != null
                            ? ModelWithCoordinateSystem.CoordinateSystem.Name
                            : "<empty>");
            }
        }
    }
}