using System.Linq;
using System.Windows.Forms;
using DelftTools.Controls;
using DelftTools.Hydro;
using DelftTools.Shell.Core;
using DelftTools.Shell.Core.Extensions;
using DelftTools.Shell.Core.Workflow.DataItems;
using DelftTools.Shell.Gui;
using DeltaShell.Plugins.ImportExport.Sobek;
using DeltaShell.Plugins.SharpMapGis;
using SharpMap;

namespace DeltaShell.Plugins.DeveloperTools.Commands
{
    class ImportNetworkAndAddToMapCommand : Command
    {
        /// <summary> 
        /// Method to be called when command will be executed.
        /// </summary>
        /// <param name="arguments">arguments to the command</param>
        protected override void OnExecute(params object[] arguments)
        {
            // TODO: DelftTools API should allow to do operations below in a simpler way - refactor, write tests which will be able to do the same in much simpler way

            IGui gui = DeveloperToolsGuiPlugin.Instance.Gui;
            IApplication app = gui.Application;

            // add new network

            var importer = new SobekNetworkImporter();

            var dlg = new OpenFileDialog
                {
                    Filter = importer.FileFilter,
                    Title = "Select a file to import from"
                };

            IHydroNetwork network = null;
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                network = importer.ImportItem(dlg.FileName) as IHydroNetwork;
            }

            if (network == null)
            {
                return;
            }

            network.Name = app.Project.GetUniqueProjectItemName(network.GetType());

            if (app.Project == null)
            {
                app.CreateNewProject();
            }

            app.Project.RootFolder.Add(new DataItem(network));

            // Add new map
            var gisPlugin = app.GetPluginForType(typeof(SharpMapGisApplicationPlugin));
            var mapDataItemInfo = gisPlugin.GetDataItemInfos().FirstOrDefault(dii => dii.ValueType == typeof(Map));
            if (mapDataItemInfo == null) return;

            var map = mapDataItemInfo.CreateData(app.Project.RootFolder) as Map;
            if (map == null) return;

            map.Name = app.Project.GetUniqueProjectItemName<Map>();

            IDataItem mapDataItem = new DataItem(map);

            app.Project.RootFolder.Add(mapDataItem);

            // add network to map and open map view
            var hydroNetworkLayer = MapLayerProviderHelper.CreateLayersRecursive(network, null, gui.Plugins.Select(p => p.MapLayerProvider).ToList());
            map.Layers.Add(hydroNetworkLayer);

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