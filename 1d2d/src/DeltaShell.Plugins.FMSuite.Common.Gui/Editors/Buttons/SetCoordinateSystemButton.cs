using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using DelftTools.Hydro;
using DelftTools.Shell.Gui;
using DeltaShell.Plugins.FMSuite.Common.Layers;
using DeltaShell.Plugins.SharpMapGis.Gui.Forms;
using GeoAPI.Extensions.CoordinateSystems;
using SharpMap;
using SharpMap.UI.Forms;

namespace DeltaShell.Plugins.FMSuite.Common.Gui.Editors.Buttons
{
    public static class SetCoordinateSystemButton
    {
        public static string ToolTip { get; } = "Set model coordinate system (does not adjust model coordinates, but can affect rendering and model results)";

        public static string Label { get; } = "Coordinate system";
        public static Bitmap ButtonImage { get; } = Properties.Resources.set_coordinate_system;

        public static void ButtonAction(object inputObject, IGui gui, Func<ICoordinateSystem, bool> CoordinateSystemFilter)
        {
            var model = inputObject as IHasCoordinateSystem;
            if (model == null || Map.CoordinateSystemFactory == null) return;

            var control = new SelectCoordinateSystemDialog(Map.CoordinateSystemFactory.SupportedCoordinateSystems, Map.CoordinateSystemFactory.CustomCoordinateSystems)
            {
                Dock = DockStyle.Fill,
                SelectedCoordinateSystem = model.CoordinateSystem,
                CoordinateSystemFilter = CoordinateSystemFilter
            };

            if (control.ShowDialog() != DialogResult.OK)
                return;

            var selectedCoordinateSystem = control.SelectedCoordinateSystem;

            var message = string.Format(
                "The model coordinates do not appear to be in '{0}', as they fall outside the expected range of values for this system. Please verify the selected " +
                "coordinate system is the system the coordinates were measured in. Continuing could lead to the map visualization failing and unexpected behaviour of spatial operations {1}{1}" +
                "Are you sure you want to continue?",
                selectedCoordinateSystem, Environment.NewLine);

            if (selectedCoordinateSystem != null &&
                !model.CanSetCoordinateSystem(selectedCoordinateSystem) && MessageBox.Show(message,
                    "Warning: model coordinates do not appear to be in the selected system",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning) == DialogResult.No)
                return;

            var coordinateSystem = model.CoordinateSystem;

            model.CoordinateSystem = selectedCoordinateSystem;

            var view = gui.DocumentViews.OfType<ProjectItemMapView>().FirstOrDefault(v => Equals(v.Data, model));

            if (view == null) return;

            view.MapView.MapControl.Visible = false;

            var modelLayer = view.MapView.Map.GetAllLayers(true).OfType<ModelGroupLayer>().First();

            modelLayer.UpdateCoordinateSystem(coordinateSystem, selectedCoordinateSystem);

            view.MapView.MapControl.Visible = true;

            modelLayer.Map.ZoomToFit(modelLayer.Envelope);
        }

        public static string CoordinateSystemName(IHasCoordinateSystem model)
        {
            return (model?.CoordinateSystem != null
                ? model.CoordinateSystem.Name
                : "<empty>");
        }
    }
}