using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using DelftTools.Hydro;
using DelftTools.Shell.Gui;
using DeltaShell.Plugins.DelftModels.HydroModel.Gui.Forms.SettingsWpf;
using DeltaShell.Plugins.FMSuite.Common.Gui.Layers;
using DeltaShell.Plugins.FMSuite.Common.Gui.Properties;
using DeltaShell.Plugins.SharpMapGis.Gui.Forms;
using GeoAPI.Extensions.CoordinateSystems;
using SharpMap;
using SharpMap.UI.Forms;

namespace DeltaShell.Plugins.FMSuite.Common.Gui.Editors.Buttons
{
    public class SetCoordinateSystemButton : IButtonBehaviour
    {
        public const string ToolTip = "Set model coordinate system (does not adjust model coordinates, but can affect rendering and model results)";
        public const string Label = "Coordinate system";
        public static readonly Bitmap ButtonImage = Resources.set_coordinate_system;
        private readonly IGui gui;
        private readonly Func<ICoordinateSystem, bool> coordinateSystemFilter;

        /// <summary>
        /// Initialize a new instance of the <see cref="SetCoordinateSystemButton"/> class.
        /// </summary>
        /// <param name="gui"> The gui instance. </param>
        /// <param name="coordinateSystemFilter"> A filter function for the coordinate system. </param>
        public SetCoordinateSystemButton(IGui gui, Func<ICoordinateSystem, bool> coordinateSystemFilter)
        {
            this.gui = gui;
            this.coordinateSystemFilter = coordinateSystemFilter;
        }

        public void Execute(object inputObject)
        {
            var model = inputObject as IHasCoordinateSystem;
            if (model == null || Map.CoordinateSystemFactory == null)
            {
                return;
            }

            var control = new SelectCoordinateSystemDialog(Map.CoordinateSystemFactory.SupportedCoordinateSystems, Map.CoordinateSystemFactory.CustomCoordinateSystems)
            {
                Dock = DockStyle.Fill,
                SelectedCoordinateSystem = model.CoordinateSystem,
                CoordinateSystemFilter = coordinateSystemFilter
            };

            if (control.ShowDialog() != DialogResult.OK)
            {
                return;
            }

            ICoordinateSystem selectedCoordinateSystem = control.SelectedCoordinateSystem;

            if (selectedCoordinateSystem != null &&
                !model.CanSetCoordinateSystem(selectedCoordinateSystem)
                && MessageBox.Show(string.Format(Resources.SetCoordinateSystemButton_Coordinates_are_not_in_coordinate_system,
                                                 selectedCoordinateSystem, Environment.NewLine),
                                   Resources.SetCoordinateSystemButton_Coordinates_are_not_in_coordinate_system_Caption,
                                   MessageBoxButtons.YesNo,
                                   MessageBoxIcon.Warning) == DialogResult.No)
            {
                return;
            }

            ICoordinateSystem coordinateSystem = model.CoordinateSystem;

            model.CoordinateSystem = selectedCoordinateSystem;

            ProjectItemMapView view = gui.DocumentViews.OfType<ProjectItemMapView>().FirstOrDefault(v => Equals(v.Data, model));

            if (view == null)
            {
                return;
            }

            view.MapView.MapControl.Visible = false;

            ModelGroupLayer modelLayer = view.MapView.Map.GetAllLayers(true).OfType<ModelGroupLayer>().First();

            modelLayer.UpdateCoordinateSystem(coordinateSystem, selectedCoordinateSystem);

            view.MapView.MapControl.Visible = true;

            modelLayer.Map.ZoomToFit(modelLayer.Envelope);
        }

        public static string CoordinateSystemName(IHasCoordinateSystem model)
        {
            return model?.CoordinateSystem != null
                       ? model.CoordinateSystem.Name
                       : "<empty>";
        }
    }
}