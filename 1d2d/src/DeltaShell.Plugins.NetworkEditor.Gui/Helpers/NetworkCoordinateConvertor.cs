using System.Windows.Forms;
using GeoAPI.CoordinateSystems.Transformations;
using GeoAPI.Extensions.CoordinateSystems;
using GeoAPI.Extensions.Networks;
using log4net;
using NetTopologySuite.Extensions.Networks;
using SharpMap;
using SharpMap.Extensions.CoordinateSystems;
using SharpMap.UI.Forms;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Helpers
{
    public static class NetworkCoordinateConvertor
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(NetworkCoordinateConvertor));

        public static bool Convert(INetwork network)
        {
            if (network.CoordinateSystem == null)
            {
                Log.Error("Can not convert the coordinate system when no coordinate system is set for the network.");
                return false;
            }

            var dialog = new SelectCoordinateSystemDialog(Map.CoordinateSystemFactory.SupportedCoordinateSystems, Map.CoordinateSystemFactory.CustomCoordinateSystems)
            {
                SelectedCoordinateSystem = network.CoordinateSystem
            };

            if (dialog.ShowDialog() != DialogResult.OK)
                return false;

            if (dialog.SelectedCoordinateSystem == null)
            {
                Log.Error("Can not convert the coordinate system when no target coordinate system is selected.");
                return false;
            }

            ICoordinateSystem targetCoordinateSystem = dialog.SelectedCoordinateSystem;
            ICoordinateTransformation transformation =
                new OgrCoordinateSystemFactory().CreateTransformation(network.CoordinateSystem, targetCoordinateSystem);

            return network.ToCoordinateSystem(transformation);
        }
    }
}
