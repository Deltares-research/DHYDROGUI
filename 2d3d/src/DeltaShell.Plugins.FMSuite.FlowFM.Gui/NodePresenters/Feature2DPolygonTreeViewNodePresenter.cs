using System.Drawing;
using DelftTools.Controls;
using DelftTools.Hydro;
using DelftTools.Hydro.GroupableFeatures;
using DelftTools.Shell.Core.Workflow.DataItems;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.Plugins.NetworkEditor.Gui.Properties;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Gui.NodePresenters
{
    /// <summary>
    /// A custom TreeViewNodePresenter to distinguish between DryAreas and Enclosures (both are Feature2DPolygons)
    /// </summary>
    internal class Feature2DPolygonTreeViewNodePresenter : FeatureProjectTreeViewNodePresenter<GroupableFeature2DPolygon>
    {
        private static Bitmap enclosureImage;
        private static Bitmap dryAreaImage;

        public Feature2DPolygonTreeViewNodePresenter() : base("", null)
        {
            enclosureImage = Resources.enclosure;
            dryAreaImage = Resources.dry_area;
        }

        /// <summary>
        /// Override UpdateNode to distinguish between Enclosures and DryAreas
        /// </summary>
        /// <param name="parentNode"></param>
        /// <param name="node"></param>
        /// <param name="nodeData"></param>
        public override void UpdateNode(ITreeNode parentNode, ITreeNode node, IEventedList<GroupableFeature2DPolygon> nodeData)
        {
            var nodeTag = parentNode.Tag as IDataItem;
            var area = (nodeTag != null ? nodeTag.Value : parentNode.Tag) as HydroArea;
            if (area == null)
            {
                return;
            }

            if (nodeData == area.Enclosures)
            {
                node.Text = HydroAreaLayerNames.EnclosureName;
                node.Image = enclosureImage;
            }

            if (nodeData == area.DryAreas)
            {
                node.Text = HydroAreaLayerNames.DryAreasPluralName;
                node.Image = dryAreaImage;
            }
        }
    }
}