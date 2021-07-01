using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using DelftTools.Hydro;
using DelftTools.Hydro.SewerFeatures;
using DelftTools.Hydro.Structures;
using DelftTools.Utils.Collections;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using SharpMap.UI.Tools;

namespace DeltaShell.Plugins.NetworkEditor.Gui.MapTools
{
    internal class AddNWRWCatchmentContextMenuMapTool : MapTool
    {
        public override bool AlwaysActive
        {
            get { return true; }
        }

        public override IEnumerable<MapToolContextMenuItem> GetContextMenuItems(Coordinate worldPosition)
        {
            var mapToolContextMenuItems = base.GetContextMenuItems(worldPosition);

            var selectedManhole = MapControl.SelectedFeatures.OfType<Manhole>().FirstOrDefault();
            var upgradeMenu = new ToolStripMenuItem("Add NWRW catchment");

            ToolStripItem[] upgradeCompartmentToolStripItems = selectedManhole?.Compartments
                .Select(c => new ToolStripMenuItem(c.Name, null, (s, e) => AddNwrwCatchment(c)))
                .ToArray();

            if (upgradeCompartmentToolStripItems != null && 
                upgradeCompartmentToolStripItems.Any() && 
                (selectedManhole.Network as IHydroRegion)?.Parent != null)
            {
                upgradeMenu.DropDownItems.AddRange(upgradeCompartmentToolStripItems);
                mapToolContextMenuItems = mapToolContextMenuItems.Plus(new MapToolContextMenuItem
                {
                    Priority = 3,
                    MenuItem = upgradeMenu
                });
            }

            return mapToolContextMenuItems;
        }

        private void AddNwrwCatchment(ICompartment compartment)
        {
            var manhole = compartment?.ParentManhole;
            var network = manhole?.Network as IHydroNetwork;
            var parentRegion = network?.Parent as IHydroRegion;
            var basin = parentRegion?.SubRegions?.OfType<IDrainageBasin>().FirstOrDefault();
            if (basin == null) return;

            var catchment = new Catchment
            {
                Name = $"{compartment.Name}_catchment",
                CatchmentType = CatchmentType.NWRW,
                IsGeometryDerivedFromAreaSize = true,
                Geometry = compartment?.Geometry?.Centroid
            };

            var width = Map.PixelSize * 30;
            catchment.SetAreaSize(width* width);

            basin.Catchments.Add(catchment); 
            var branchAndDir = manhole.IncomingBranches
                .Select(b => new {branch = b, incoming = true})
                .Concat(manhole.OutgoingBranches.Select(b => new { branch = b, incoming = false }))
                .FirstOrDefault();

            if (branchAndDir == null) return;

            var branch = branchAndDir.branch;
            var lateral = new LateralSource
            {
                Branch = branch, 
                Chainage = branchAndDir.incoming ? branch.Length : 0,
                Geometry = compartment.Geometry
            };
            branch.BranchFeatures.Add(lateral);

            parentRegion.Links.Add(new HydroLink(catchment, lateral){Geometry = new LineString(new []{catchment.Geometry.Coordinate, lateral.Geometry.Coordinate})});
        }
    }
}