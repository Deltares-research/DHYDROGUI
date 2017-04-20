/*using System.Diagnostics;
using DelftTools.Hydro;
using DeltaShell.Plugins.DeveloperTools.VisualStudioExtensions.DebuggerVisualizers;
using DeltaShell.Plugins.NetworkEditor.Gui.Helpers;
using DeltaShell.Plugins.NetworkEditor.Layers;
using DeltaShell.Plugins.SharpMapGis.Gui.Forms;
using Microsoft.VisualStudio.DebuggerVisualizers;

// TODO: how to pass only INetwork
[assembly: DebuggerVisualizer(typeof (HydroNetworkVisualizer), typeof (VisualizerObjectSource), Target = typeof (HydroNetwork), Description = "HydroNetwork Visualizer")]

namespace DeltaShell.Plugins.DeveloperTools.VisualStudioExtensions.DebuggerVisualizers
{
    public class HydroNetworkVisualizer : DialogDebuggerVisualizer
    {
        protected override void Show(IDialogVisualizerService windowService, IVisualizerObjectProvider objectProvider)
        {
            var o = objectProvider.GetObject();

            if (!(o is IHydroNetwork))
            {
                return;
            }

            Show((IHydroNetwork)o, windowService);
        }

        public void Show(IHydroNetwork network, IDialogVisualizerService windowService)
        {
            var networkMapLayer = new HydroNetworkMapLayer { Network = network };

            var mapView = new MapView { Width = 300, Height = 300};
            mapView.Map.Layers.Add(networkMapLayer);
            HydroRegionEditorHelper.AddHydroRegionEditorMapTool(mapView.MapControl);

            windowService.ShowDialog(mapView);
        }
    }
}*/
