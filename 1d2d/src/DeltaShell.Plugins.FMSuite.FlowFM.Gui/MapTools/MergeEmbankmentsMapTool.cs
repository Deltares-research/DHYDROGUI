using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using DelftTools.Hydro;
using DeltaShell.Plugins.FMSuite.FlowFM.Gui.Properties;
using GeoAPI.Geometries;
using SharpMap.UI.Tools;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Gui.MapTools
{
    public class MergeEmbankmentsMapTool : MapTool
    {
        public MergeEmbankmentsMapTool()
        {
            Name = FlowFMMapViewDecorator.MergeEmbankmentsToolName;
        }

        public override bool Enabled
        {
            get { return GetSelectedEmbankments().Count == 2; }
        }

        public override void Execute()
        {
            var embankmentDefinitionsLayer = Layers.FirstOrDefault(l => l.DataSource != null && l.DataSource.FeatureType == typeof(Embankment) && l.Name == HydroArea.EmbankmentsPluralName);
            if (embankmentDefinitionsLayer == null)
            {
                throw new InvalidOperationException("Embankment definition layer not found.");
            }
            
            var embankmentDefinitions = embankmentDefinitionsLayer.DataSource.Features as IList<Embankment>;
            if (embankmentDefinitions == null)
            {
                throw new InvalidOperationException("Embankment features not of expected type.");
            }

            var selectedEmbankments = GetSelectedEmbankments();
            if (selectedEmbankments.Count() != 2)
            {
                throw new InvalidOperationException("Exactly two embankments need to be selected in order to use the Merge Embankments tool.");
            }

            var mergedEmbankment = EmbankmentMerger.MergeSelectedEmbankments(embankmentDefinitions, selectedEmbankments[0], selectedEmbankments[1]);
            if (mergedEmbankment != null)
            {
                // Replace the two embankments with the merged one. 
                embankmentDefinitions.Remove(selectedEmbankments[0]);
                embankmentDefinitions.Remove(selectedEmbankments[1]);
                embankmentDefinitions.Add(mergedEmbankment);
            }

            MapControl.SelectTool.Select(null); 
        }

        public override IEnumerable<MapToolContextMenuItem> GetContextMenuItems(Coordinate worldPosition)
        {
            var allEmbankments = GetAllEmbankments();

            if (allEmbankments.Count == 0) yield break;

            var generateEmbankmentsMenu = new ToolStripMenuItem("Merge embankments", Resources.wrenchPlus, (s, e) => Execute())
            {
                Enabled = GetSelectedEmbankments().Count == 2
            };

            yield return new MapToolContextMenuItem
            {
                Priority = 4,
                MenuItem = generateEmbankmentsMenu
            };
        }

        private List<Embankment> GetSelectedEmbankments()
        {
            return MapControl.SelectedFeatures.OfType<Embankment>().ToList();
        }

        private IList<Embankment> GetAllEmbankments()
        {
            var embankmentsLayer = Layers.FirstOrDefault(l => l.DataSource != null && l.DataSource.FeatureType == typeof (Embankment) && l.Name == HydroArea.EmbankmentsPluralName);
            return embankmentsLayer != null
                ? (IList<Embankment>) embankmentsLayer.DataSource.Features
                : new List<Embankment>();
        }

        public override bool AlwaysActive
        {
            get { return true; }
        }

    }
}
