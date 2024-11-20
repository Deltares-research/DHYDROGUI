using System.Linq;
using System.Windows.Forms;
using DelftTools.Controls.Swf;
using DelftTools.Hydro;
using DelftTools.Shell.Gui;
using DeltaShell.Plugins.FMSuite.Common.Gui.Properties;
using DeltaShell.Plugins.FMSuite.Common.Layers;
using DeltaShell.Plugins.SharpMapGis.Gui.Forms;
using SharpMap;
using SharpMap.CoordinateSystems.Transformations;
using SharpMap.Extensions.CoordinateSystems;
using SharpMap.UI.Forms;
using MessageBox = DelftTools.Controls.Swf.MessageBox;

namespace DeltaShell.Plugins.FMSuite.Common.Gui
{
    public static class FMMenuItemHelper
    {
        public static ClonableToolStripMenuItem CreateCoordinateTransformItem(IHasCoordinateSystem modelWithCoordinates, IGui gui)
        {
            var item = new ClonableToolStripMenuItem
                {
                    Text = Resources.FMMenuItemHelper_CreateCoordinateTransformItem_Convert_All_Model_Coordinates___,
                    Tag = modelWithCoordinates,
                    Image = Resources.set_coordinate_system
                };
            item.Click += (s,a) => OnConversionClick((IHasCoordinateSystem)((ToolStripItem)s).Tag, gui);
            return item;
        }

        // add reset system item

        private static void OnConversionClick(IHasCoordinateSystem model, IGui gui)
        {
            if (model == null) return;

            if (model.CoordinateSystem == null)
            {
                MessageBox.Show("Cannot start conversion; set current coordinate system first",
                                "Current coordinate system not set");
                return;
            }

            var control = new SelectCoordinateSystemDialog(Map.CoordinateSystemFactory.SupportedCoordinateSystems,
                Map.CoordinateSystemFactory.CustomCoordinateSystems)
            {
                Dock = DockStyle.Fill,
                SelectedCoordinateSystem = model.CoordinateSystem,
                CoordinateSystemFilter = cs => !cs.IsGeographic || cs.Name == "WGS 84"
            };

            if (control.ShowDialog() != DialogResult.OK)
                return;

            var currentCS = model.CoordinateSystem;
            var targetCS = control.SelectedCoordinateSystem;

            if (targetCS == null)
            {
                MessageBox.Show("Cannot convert to empty coordinate system", "No coordinate system chosen");
                return;
            }

            var transformation = new OgrCoordinateSystemFactory().CreateTransformation(currentCS, targetCS);

            ProjectItemMapView view = null;

            try
            {
                gui.MainWindow.SetWaitCursorOn();

                model.TransformCoordinates(transformation);
                
                view = gui.DocumentViews.OfType<ProjectItemMapView>().FirstOrDefault(v => Equals(v.Data, model));

                if (view == null) return;

                view.MapView.MapControl.Visible = false;

                var modelLayer = view.MapView.Map.GetAllLayers(true).OfType<ModelGroupLayer>().First();

                modelLayer.UpdateCoordinateSystem(currentCS, targetCS);

                view.MapView.MapControl.Visible = true;

                modelLayer.Map.ZoomToExtents();
            }
            catch (CoordinateTransformException e)
            {
                MessageBox.Show("Failed to convert model coordinates to given coordinate system: " + e.Message,
                    "Coordinate conversion", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                if (view != null)
                {
                    view.MapView.MapControl.Visible = true;
                }
                gui.MainWindow.SetWaitCursorOff();
            }
        }

        public static ClonableToolStripMenuItem CreateResetCoordinateSystemItem(IHasCoordinateSystem model)
        {
            var item = new ClonableToolStripMenuItem
            {
                Text = Resources.FMMenuItemHelper_CreateResetCoordinateSystemItem_Reset_Coordinate_System___, 
                Tag = model
            };
            item.Click += (s, e) => model.CoordinateSystem = null;
            return item;
        }
    }
}
