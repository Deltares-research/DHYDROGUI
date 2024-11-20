using System.Windows.Forms;
using GeoAPI.CoordinateSystems.Transformations;
using GeoAPI.Extensions.CoordinateSystems;
using log4net;
using SharpMap;
using SharpMap.Extensions.CoordinateSystems;
using SharpMap.UI.Forms;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.Gui.Helpers
{
    public static class RTCModelCoordinateConvertor
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(RTCModelCoordinateConvertor));

        public static bool Convert(IRealTimeControlModel realTimeControlModel)
        {
            if (realTimeControlModel.CoordinateSystem == null)
            {
                Log.Error("Can not convert the coordinate system when no coordinate system is set for the network.");
                return false;
            }

            var dialog = new SelectCoordinateSystemDialog(Map.CoordinateSystemFactory.SupportedCoordinateSystems, Map.CoordinateSystemFactory.CustomCoordinateSystems) {SelectedCoordinateSystem = realTimeControlModel.CoordinateSystem};

            if (dialog.ShowDialog() != DialogResult.OK)
            {
                return false;
            }

            if (dialog.SelectedCoordinateSystem == null)
            {
                Log.Error("Can not convert the coordinate system when no target coordinate system is selected.");
                return false;
            }

            ICoordinateSystem targetCoordinateSystem = dialog.SelectedCoordinateSystem;
            ICoordinateTransformation transformation =
                new OgrCoordinateSystemFactory().CreateTransformation(realTimeControlModel.CoordinateSystem, targetCoordinateSystem);

            return realTimeControlModel.ToCoordinateSystem(transformation);
        }
    }
}