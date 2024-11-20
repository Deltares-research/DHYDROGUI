using System.Linq;
using DelftTools.Controls;
using DelftTools.Controls.Wpf.Commands;
using DeltaShell.NGHS.Common.Gui.MapLayers;
using DeltaShell.Plugins.SharpMapGis.Gui;
using DeltaShell.Plugins.SharpMapGis.Gui.Forms;
using ICommand = System.Windows.Input.ICommand;

namespace DeltaShell.Plugins.NetworkEditor.Gui
{
    public static class NetworkEditorRibbonCommands
    {
        /// <summary>
        /// Adds a WMTS layer to the current map view
        /// </summary>
        public static readonly ICommand AddWmtsLayerCommand = AddWmtsLayerCommand = new RelayCommand(url =>
        {
            var mapView = SharpMapGisGuiPlugin.Instance.Gui.DocumentViews.ActiveView?.GetViewsOfType<MapView>().FirstOrDefault();
            if (mapView == null) return;

            mapView.Map.Layers.Add(new WmtsGroupLayer { Url = url as string});
        }, url => !string.IsNullOrEmpty(url as string));
    }
}