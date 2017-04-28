using System.Linq;
using DelftTools.Controls;
using DelftTools.Hydro;
using DelftTools.Shell.Core.Extensions;
using DelftTools.Shell.Core.Workflow.DataItems;
using DelftTools.Shell.Gui;
using DeltaShell.Plugins.NetworkEditor;
using DeltaShell.Plugins.SharpMapGis;
using SharpMap;

namespace DeltaShell.Plugins.DeveloperTools.Commands
{
    public class CreateTestNetworkAndMapCommand : Command
    {
        /// <summary> 
        /// Method to be called when command will be executed.
        /// </summary>
        /// <param name="arguments">arguments to the command</param>
        protected override void OnExecute(params object[] arguments)
        {
            // TODO: DelftTools API should allow to do operations below in a simpler way - refactor, write tests which will be able to do the same in much simpler way

            var gui = DeveloperToolsGuiPlugin.Instance.Gui;
            var app = gui.Application;

            // Add new network
            var networkEditorPlugin = app.GetPluginForType(typeof(NetworkEditorApplicationPlugin));
            var networkDataItemInfo = networkEditorPlugin.GetDataItemInfos().FirstOrDefault(dii => dii.ValueType == typeof(HydroNetwork));
            if (networkDataItemInfo == null) return;

            if (app.Project == null)
            {
                app.CreateNewProject();
            }

            var newNetwork = networkDataItemInfo.CreateData(app.Project.RootFolder) as HydroNetwork;
            if (newNetwork == null) return;

            newNetwork.Name = app.Project.GetUniqueProjectItemName<HydroNetwork>();
            var networkDataItem = new DataItem(newNetwork);
            var network = (IHydroNetwork) networkDataItem.Value;

            app.Project.RootFolder.Add(networkDataItem);

            // Add new map
            var gisPlugin = app.GetPluginForType(typeof(SharpMapGisApplicationPlugin));
            var mapDataItemInfo = gisPlugin.GetDataItemInfos().FirstOrDefault(dii => dii.ValueType == typeof(Map));
            if (mapDataItemInfo == null) return;

            var newMap = mapDataItemInfo.CreateData(app.Project.RootFolder) as Map;
            if (newMap == null) return;

            newMap.Name = app.Project.GetUniqueProjectItemName<Map>();

            var mapDataItem = new DataItem(newMap);

            app.Project.RootFolder.Add(mapDataItem);

            // add network to map and open map view
            var hydroNetworkLayer = MapLayerProviderHelper.CreateLayersRecursive(network, null, gui.Plugins.Select(p => p.MapLayerProvider).ToList());
            newMap.Layers.Add(hydroNetworkLayer);

            gui.Selection = mapDataItem;
            gui.CommandHandler.OpenViewForSelection();
        }

        /// <summary>
        /// Commands can be disabled when they should not be used.
        /// </summary>
        public override bool Enabled
        {
            get { return true; }
        }
    }
}