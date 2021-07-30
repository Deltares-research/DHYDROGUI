using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using DelftTools.Hydro;
using DelftTools.Hydro.SewerFeatures;
using DelftTools.Hydro.Structures;
using DelftTools.Utils.Collections;
using GeoAPI.Geometries;
using log4net;
using NetTopologySuite.Geometries;
using SharpMap.UI.Tools;

namespace DeltaShell.Plugins.NetworkEditor.Gui.MapTools
{
    internal class AddNWRWCatchmentContextMenuMapTool : MapTool
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(AddNWRWCatchmentContextMenuMapTool));

        public override bool AlwaysActive
        {
            get { return true; }
        }

        public override IEnumerable<MapToolContextMenuItem> GetContextMenuItems(Coordinate worldPosition)
        {
            var mapToolContextMenuItems = base.GetContextMenuItems(worldPosition);

            var upgradeMenu = new ToolStripMenuItem("Add NWRW catchment");

            var selectedManholes = MapControl.SelectedFeatures.OfType<Manhole>().ToArray();
            
            ToolStripMenuItem allCatchments = new ToolStripMenuItem("Selected manholes", null, (s, e) => selectedManholes.ForEach(m => AddNwrwCatchment(m.Compartments.FirstOrDefault())))
            {
                ToolTipText = "Add NWRW catchment for all selected manholes using the first compartment"
            };

            ToolStripItem[] upgradeCompartmentToolStripItems = selectedManholes.Select(CreateManholeToolStripMenuItem).Take(10).ToArray();

            if (upgradeCompartmentToolStripItems.Any() && 
                (selectedManholes?.FirstOrDefault()?.Network as IHydroRegion)?.Parent != null)
            {
                upgradeMenu.DropDownItems.Add(allCatchments);
                upgradeMenu.DropDownItems.Add("-");
                upgradeMenu.DropDownItems.AddRange(upgradeCompartmentToolStripItems);
                if (selectedManholes.Length > 10)
                {
                    upgradeMenu.DropDownItems.Add("...");
                }
                mapToolContextMenuItems = mapToolContextMenuItems.Plus(new MapToolContextMenuItem
                {
                    Priority = 3,
                    MenuItem = upgradeMenu
                });
            }

            return mapToolContextMenuItems;
        }

        private ToolStripMenuItem CreateManholeToolStripMenuItem(Manhole m)
        {
            if (m.Compartments.Count > 1)
            {
                var menuItem = new ToolStripMenuItem($"{m.Name}");
                ToolStripItem[] toolStripItems = m.Compartments
                                                  .Select(c => new ToolStripMenuItem($"{c.Name}", null, (s, e) => AddNwrwCatchment(c)))
                                                  .ToArray();

                menuItem.DropDownItems.AddRange(toolStripItems);
                return menuItem;
            }

            var firstCompartment = m.Compartments[0];
            return new ToolStripMenuItem($"{firstCompartment.ParentManhole?.Name} ({firstCompartment.Name})", null, (s, e) => AddNwrwCatchment(firstCompartment));
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

            var branchAndDir = manhole.IncomingBranches
                .Select(b => new {branch = b, incoming = true})
                .Concat(manhole.OutgoingBranches.Select(b => new { branch = b, incoming = false }))
                .FirstOrDefault();

            if (branchAndDir == null) return;

            var branch = branchAndDir.branch;
            var chainage = branchAndDir.incoming ? branch.Length : 0;

            var existingLateral = branch.BranchFeatures.OfType<ILateralSource>().FirstOrDefault(l => Math.Abs(l.Chainage - chainage) < 1e-8);
            if (existingLateral != null)
            {
                log.Error($"An lateral ({existingLateral.Name}) already exists at branch {branch.Name} ({chainage})");
                return;
            }
                
            var lateral = new LateralSource
            {
                Branch = branch, 
                Chainage = chainage,
                Geometry = compartment.Geometry
            };

            basin.Catchments.Add(catchment);
            branch.BranchFeatures.Add(lateral);

            parentRegion.Links.Add(new HydroLink(catchment, lateral){Geometry = new LineString(new []{catchment.Geometry.Coordinate, lateral.Geometry.Coordinate})});
        }
    }
}