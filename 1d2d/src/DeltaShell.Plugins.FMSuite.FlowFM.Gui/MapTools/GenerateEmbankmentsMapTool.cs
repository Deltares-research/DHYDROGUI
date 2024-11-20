using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using DelftTools.Hydro;
using DeltaShell.Plugins.FMSuite.FlowFM.Gui.Forms;
using DeltaShell.Plugins.FMSuite.FlowFM.Gui.Properties;
using GeoAPI.Geometries;
using SharpMap.Api.Layers;
using SharpMap.UI.Tools;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Gui.MapTools
{
    public class GenerateEmbankmentsMapTool : MapTool
    {
        public GenerateEmbankmentsMapTool()
        {
            Name = FlowFMMapViewDecorator.GenerateEmbankmentsToolName;
            LayerFilter = null;
        }

        public override bool AlwaysActive
        {
            get { return true; }
        }

        public override bool Enabled
        {
            get
            {
                // Only active when at least one 1D channel exists and when there is an Area layer. 
                var branchesLayer = Layers.FirstOrDefault(
                        l => l.DataSource != null && l.DataSource.FeatureType == typeof (Channel) && l.Name == "Branches");
                if (branchesLayer == null)
                {
                    return false; 
                }
                var branches = branchesLayer.DataSource.Features as IList<Channel>;

                return branches != null && branches.Count > 0; 
            }
        }

        public override void Execute()
        {
            var branches = GetBranches();
            var usingSelected = MapControl.SelectedFeatures.OfType<Channel>().Any();
            
            var generateEmbankmentsDialog = new GenerateEmbankmentsDialog{Text = string.Format("Generate embankments for {0} channels", (usingSelected ? "selected": "all"))};
            
            if (generateEmbankmentsDialog.ShowDialog() != DialogResult.OK) return;

            if (!generateEmbankmentsDialog.GenerateLeftEmbankments && !generateEmbankmentsDialog.GenerateRightEmbankments) return;

            var embankmentDefinitionsLayer = GetEmbankmentsLayer();
            if (embankmentDefinitionsLayer == null)
            {
                throw new InvalidOperationException("Embankment definition layer not found.");
            }

            var embankmentDefinitions = embankmentDefinitionsLayer.DataSource.Features as IList<Embankment>;
            if (embankmentDefinitions == null)
            {
                throw new InvalidOperationException("Invalid embankment definitions data in layer.");
            }

            if (!usingSelected)
            {
                embankmentDefinitions.Clear();
            }

            EmbankmentGenerator.GenerateEmbankments(branches, embankmentDefinitions, generateEmbankmentsDialog.CrossSectionBased,
                generateEmbankmentsDialog.ConstantDistance, generateEmbankmentsDialog.GenerateLeftEmbankments,
                generateEmbankmentsDialog.GenerateRightEmbankments, generateEmbankmentsDialog.MergeAutomatically);
        }

        public override IEnumerable<MapToolContextMenuItem> GetContextMenuItems(Coordinate worldPosition)
        {
            if (GetBranches().Count == 0) yield break;

            if (GetEmbankmentsLayer() == null)
            {
                yield break;
            }

            var generateEmbankmentsMenu = new ToolStripMenuItem("Generate embankments", Resources.guide, (s, e) => Execute());

            yield return new MapToolContextMenuItem
            {
                Priority = 4,
                MenuItem = generateEmbankmentsMenu
            };
        }

        private IList<Channel> GetBranches()
        {
            var selectedBranches = MapControl.SelectedFeatures.OfType<Channel>().ToList();
            if (selectedBranches.Count > 0)
            {
                return selectedBranches;
            }

            var branchesLayer = Layers.FirstOrDefault(l => l.DataSource != null && l.DataSource.FeatureType == typeof (Channel) && l.Name == "Branches");
            return branchesLayer != null
                ? branchesLayer.DataSource.Features as IList<Channel>
                : new List<Channel>();
        }

        private ILayer GetEmbankmentsLayer()
        {
            return Layers.FirstOrDefault(l =>
                l.DataSource != null &&
                l.DataSource.FeatureType == typeof (Embankment) &&
                l.Name == HydroArea.EmbankmentsPluralName);
        }
    }

}
