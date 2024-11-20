using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using DelftTools.Hydro.SewerFeatures;
using DelftTools.Utils.Collections;
using GeoAPI.Geometries;
using SharpMap.UI.Tools;

namespace DeltaShell.Plugins.NetworkEditor.Gui.MapTools
{
    internal class OutletCompartmentContextMenuMapTool : MapTool
    {
        public override bool AlwaysActive
        {
            get { return true; }
        }

        public override IEnumerable<MapToolContextMenuItem> GetContextMenuItems(Coordinate worldPosition)
        {
            var mapToolContextMenuItems = base.GetContextMenuItems(worldPosition);

            var selectedManhole = MapControl.SelectedFeatures.OfType<Manhole>().FirstOrDefault();
            var upgradeMenu = new ToolStripMenuItem("Upgrade to outlet compartment");
            ToolStripItem[] upgradeCompartmentToolStripItems = selectedManhole?.Compartments.Where(c => !(c is OutletCompartment)).Select(c =>
                new ToolStripMenuItem(c.Name, null, (s, e) => selectedManhole.UpdateCompartmentToOutletCompartment(c))).ToArray();
            if (upgradeCompartmentToolStripItems != null && upgradeCompartmentToolStripItems.Any())
            {
                upgradeMenu.DropDownItems.AddRange(upgradeCompartmentToolStripItems);
                mapToolContextMenuItems = mapToolContextMenuItems.Plus(new MapToolContextMenuItem
                {
                    Priority = 3,
                    MenuItem = upgradeMenu
                });
            }
            var downgradeMenu = new ToolStripMenuItem("Downgrade to compartment");
            ToolStripItem[] downgradeOutletCompartmentToolStripItems = selectedManhole?.Compartments.OfType<OutletCompartment>().Select(c =>
                new ToolStripMenuItem(c.Name, null, (s, e) => selectedManhole.DowngradeOutletCompartmentToCompartment(c))).ToArray();
            if (downgradeOutletCompartmentToolStripItems != null && downgradeOutletCompartmentToolStripItems.Any())
            {
                downgradeMenu.DropDownItems.AddRange(downgradeOutletCompartmentToolStripItems);
                mapToolContextMenuItems = mapToolContextMenuItems.Plus(new MapToolContextMenuItem
                {
                    Priority = 3,
                    MenuItem = downgradeMenu
                });
            }
            return mapToolContextMenuItems;
        }
    }
}