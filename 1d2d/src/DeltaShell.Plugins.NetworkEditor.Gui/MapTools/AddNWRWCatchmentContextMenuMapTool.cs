using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using DelftTools.Hydro;
using DelftTools.Hydro.SewerFeatures;
using DelftTools.Hydro.Structures;
using DelftTools.Utils.Collections;
using GeoAPI.Extensions.Networks;
using GeoAPI.Geometries;
using log4net;
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
                                                  .Select(c => CreateCompartmentToolStripMenuItem($"{c.Name}", c))
                                                  .ToArray();

                menuItem.DropDownItems.AddRange(toolStripItems);
                return menuItem;
            }

            ICompartment firstCompartment = m.Compartments[0];
            return CreateCompartmentToolStripMenuItem($"{firstCompartment.ParentManhole?.Name} ({firstCompartment.Name})", firstCompartment);
        }

        private ToolStripMenuItem CreateCompartmentToolStripMenuItem(string text, ICompartment compartment)
        {
            return new ToolStripMenuItem(text, null, (s, e) => AddNwrwCatchment(compartment));
        }

        private void AddNwrwCatchment(ICompartment compartment)
        {
            IManhole manhole = compartment.ParentManhole;
            var network = (IHydroNetwork)manhole.Network;
            var parentRegion = (IHydroRegion)network.Parent;
            IDrainageBasin basin = parentRegion.SubRegions.OfType<IDrainageBasin>().FirstOrDefault() ??
                                   throw new InvalidOperationException("No basin is available to add the NWRW catchment to.");

            IEnumerable<BranchWithChainage> branchesWithChainages = GetBranchesWithChainages(manhole);
            ILateralSource existingLateralSource = GetExistingLateralSource(branchesWithChainages);
            if (existingLateralSource != null)
            {
                log.Error($"A lateral source already exists at branch {existingLateralSource.Branch.Name} ({existingLateralSource.Chainage})");
                return;
            }

            BranchWithChainage branchWithChainage = branchesWithChainages.First();
            Catchment catchment = CreateNwrwCatchment(compartment);
            LateralSource lateral = CreateLateralSource(compartment, branchWithChainage.Branch, branchWithChainage.Chainage);

            basin.Catchments.Add(catchment);
            branchWithChainage.Branch.BranchFeatures.Add(lateral);
            catchment.LinkTo(lateral);
        }

        private static LateralSource CreateLateralSource(ICompartment compartment, IBranch branch, double chainage)
        {
            return new LateralSource
            {
                Branch = branch,
                Chainage = chainage,
                Name = $"{compartment.Name}",
                Geometry = compartment.Geometry
            };
        }

        private Catchment CreateNwrwCatchment(ICompartment compartment)
        {
            var catchment = new Catchment
            {
                Name = $"{compartment.Name}",
                CatchmentType = CatchmentType.NWRW,
                IsGeometryDerivedFromAreaSize = true,
                Geometry = compartment.Geometry?.Centroid
            };

            double width = Map.PixelSize * 30;
            catchment.SetAreaSize(width * width);

            return catchment;
        }

        private static IEnumerable<BranchWithChainage> GetBranchesWithChainages(INode node)
        {
            foreach (IBranch incomingBranch in node.IncomingBranches)
            {
                yield return new BranchWithChainage(incomingBranch, incomingBranch.Length);
            }

            foreach (IBranch outgoingBranch in node.OutgoingBranches)
            {
                yield return new BranchWithChainage(outgoingBranch, 0);
            }
        }

        private static ILateralSource GetExistingLateralSource(IEnumerable<BranchWithChainage> branchesWithChainages)
        {
            foreach (BranchWithChainage branchWithChainage in branchesWithChainages)
            {
                foreach (ILateralSource lateralSource in branchWithChainage.Branch.BranchFeatures.OfType<ILateralSource>())
                {
                    if (lateralSource.Chainage.IsEqualTo(branchWithChainage.Chainage, 1e-8))
                    {
                        return lateralSource;
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Helper struct to couple a branch with a chainage for that branch.
        /// </summary>
        private struct BranchWithChainage
        {
            public BranchWithChainage(IBranch branch, double chainage)
            {
                Branch = branch;
                Chainage = chainage;
            }

            public IBranch Branch { get; }

            public double Chainage { get; }
        }
    }
}