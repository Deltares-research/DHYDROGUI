using System.Drawing;
using DelftTools.Controls;
using DelftTools.Hydro;
using DelftTools.Shell.Core.Workflow.DataItems;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.Plugins.NetworkEditor.Gui.Properties;

namespace DeltaShell.Plugins.NetworkEditor.Gui.ProjectExplorer
{
    /// <summary>
    /// A custom TreeViewNodePresenter to distinguish between different (both are Feature2DPoints)
    /// </summary>
    internal class Feature2DPointTreeViewNodePresenter : FeatureProjectTreeViewNodePresenter<GroupableFeature2DPoint>
    {
        private static Bitmap observationPointImage;
        private static Bitmap GullyImage;
        
        public Feature2DPointTreeViewNodePresenter() : base("", null)
        {
            observationPointImage = Resources.observationcs2d;
            GullyImage = Resources.Gully;
        }

        /// <summary>
        /// Override UpdateNode to distinguish between observation points and Gullies
        /// </summary>
        /// <param name="parentNode"></param>
        /// <param name="node"></param>
        /// <param name="nodeData"></param>
        public override void UpdateNode(ITreeNode parentNode, ITreeNode node,
            IEventedList<GroupableFeature2DPoint> nodeData)
        {
            var nodeTag = parentNode.Tag as IDataItem;
            var area = (nodeTag != null ? nodeTag.Value : parentNode.Tag) as HydroArea;
            if (area == null) return;

            if (nodeData == area.ObservationPoints)
            {
                node.Text = HydroArea.ObservationPointsPluralName;
                node.Image = observationPointImage;
            }

            if (nodeData == area.Gullies)
            {
                node.Text = HydroArea.GullyName;
                node.Image = GullyImage;
            }
        }
    }
}

