using System;
using System.Drawing;
using System.Windows.Forms;
using DelftTools.Controls;
using DelftTools.Controls.Swf;
using DeltaShell.Plugins.FMSuite.Common.Gui.NodePresenters;
using DeltaShell.Plugins.FMSuite.Common.Gui.Properties;

namespace DeltaShell.Plugins.FMSuite.Wave.Gui.NodePresenters
{
    public class WaveBoundaryNodePresenter : FMSuiteNodePresenterBase<WaveBoundaryCondition>
    {
        private static readonly Bitmap BoundaryImage = Resources.boundary;

        // function to retrieve the WaveModel containing this WaveBoundaryCondition
        private readonly Func<WaveBoundaryCondition, WaveModel> modelFunc;

        public WaveBoundaryNodePresenter(Func<WaveBoundaryCondition, WaveModel> getModelFunc)
        {
            modelFunc = getModelFunc;
        }

        protected override string GetNodeText(WaveBoundaryCondition data)
        {
            return data.Name;
        }

        protected override Image GetNodeImage(WaveBoundaryCondition data)
        {
            return BoundaryImage;
        }

        protected override bool CanRemove(WaveBoundaryCondition nodeData)
        {
            return true;
        }

        protected override bool RemoveNodeData(object parentNodeData, WaveBoundaryCondition nodeData)
        {
            return DeleteBoundary(nodeData);
        }

        private bool DeleteBoundary(WaveBoundaryCondition nodeData)
        {
            // remove the boundary from the wave model
            WaveModel wm = modelFunc(nodeData);
            return wm.Boundaries.Remove(nodeData.Feature);
        }

        /// <summary>
        /// Override the context menu.
        /// <see cref="WaveBoundaryCondition" /> only has a delete button and no importer, exporter and properties.
        /// </summary>
        /// <param name="sender"> </param>
        /// <param name="nodeData"> </param>
        /// <returns> </returns>
        public override IMenuItem GetContextMenu(ITreeNode sender, object nodeData)
        {
            var boundaryCondition = nodeData as WaveBoundaryCondition;
            WaveModel model = modelFunc(boundaryCondition);
            if (model != null && boundaryCondition != null)
            {
                var contextMenu = new ContextMenuStrip();
                var item = new ClonableToolStripMenuItem
                {
                    Text = Properties.Resources.WaveBoundaryNodePresenter_GetContextMenu_Delete,
                    Tag = model
                };
                item.Click += (s, a) => DeleteBoundary(boundaryCondition);
                item.Image = Resources.DeleteHS;
                contextMenu.Items.Add(item);
                var domainMenu = new MenuItemContextMenuStripAdapter(contextMenu);
                return domainMenu;
            }
            else
            {
                return null;
            }
        }
    }
}