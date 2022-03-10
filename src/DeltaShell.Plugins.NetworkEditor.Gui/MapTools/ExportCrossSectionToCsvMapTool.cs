using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using DelftTools.Hydro;
using DelftTools.Hydro.CrossSections;
using DeltaShell.Plugins.NetworkEditor.MapLayers;
using GeoAPI.Geometries;
using log4net;
using SharpMap.Api.Layers;
using SharpMap.UI.Tools;

namespace DeltaShell.Plugins.NetworkEditor.Gui.MapTools
{
    public class ExportCrossSectionToCsvMapTool : MapTool
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(ExportCrossSectionToCsvMapTool));

        public ExportCrossSectionToCsvMapTool(Func<ILayer, bool> layerFilter)
        {
            LayerFilter = layerFilter;
        }

        HydroRegionMapLayer HydroNetworkMapLayer
        {
            get { return (HydroRegionMapLayer)Layers.FirstOrDefault(); }
        }
        
        public override bool Enabled
        {
            get
            {
                return (HydroNetworkMapLayer != null && MapControl.SelectedFeatures.OfType<ICrossSection>().Any());
            }
        }
        
        public override bool AlwaysActive
        {
            get { return Enabled; }
        }

        public override IEnumerable<MapToolContextMenuItem> GetContextMenuItems(Coordinate worldPosition)
        {
            if (!Enabled) yield break;

            yield return new MapToolContextMenuItem
                        {
                            Priority = 4,
                            MenuItem = new ToolStripMenuItem("Export all cross section(s) ...", null, ExportAllCrossSectionEventHandler)
                        };
            
            if (MapControl.SelectedFeatures.Any())
            {
                yield return new MapToolContextMenuItem
                        {
                            Priority = 3,
                            MenuItem = new ToolStripMenuItem("Export selected cross section(s) ...", null, ExportSelectedCrossSectionEventHandler)
                        };
            }
        }

        private void ExportAllCrossSectionEventHandler(object sender, EventArgs e)
        {
            Burp(false);
        }

        private void ExportSelectedCrossSectionEventHandler(object sender, EventArgs e)
        {
            Burp(true);
        }

        public override void Execute()
        {
            Burp(MapControl.SelectedFeatures.OfType<ICrossSection>().Any());
        }

        private void Burp(bool selection)
        {
            var cursor = MapControl.Cursor;
            MapControl.Cursor = Cursors.WaitCursor;
            var oldSelectioon = NetworkEditorGuiPlugin.Instance.Gui.Selection;
            try
            {
                IList<ICrossSection> list;
                if (selection)
                {
                    list = MapControl.SelectedFeatures.OfType<ICrossSection>().ToList();
                }
                else
                {
                    var network = (IHydroNetwork)HydroNetworkMapLayer.Region;
                    list = network.CrossSections.ToList();
                }
                NetworkEditorGuiPlugin.Instance.Gui.Selection = list;
                NetworkEditorGuiPlugin.Instance.Gui.CommandHandler.ExportSelectedItem();
            }
            catch (Exception exception)
            {
                Log.Error(exception.Message);
            }
            finally
            {
                MapControl.Cursor = cursor;
                NetworkEditorGuiPlugin.Instance.Gui.Selection = oldSelectioon;
            }
            MapControl.Refresh();
        }  
    }
}