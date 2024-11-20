using System;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Windows.Forms;
using DelftTools.Controls;
using DelftTools.Controls.Swf;
using DeltaShell.Plugins.FMSuite.Common.Gui.NodePresenters;
using DeltaShell.Plugins.FMSuite.Common.Gui.Properties;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries;

namespace DeltaShell.Plugins.FMSuite.Wave.Gui.NodePresenters
{
    /// <summary>
    /// <see cref="SpatiallyVariantBoundaryNodePresenter"/> implements the NodePresenter for
    /// <see cref="IWaveBoundary"/>, such that they can be viewed within the Node Tree.
    /// </summary>
    public class SpatiallyVariantBoundaryNodePresenter : FMSuiteNodePresenterBase<IWaveBoundary>
    {
        private static readonly Bitmap boundaryImage = Resources.boundary;
        private readonly Func<IWaveBoundary, IBoundaryContainer> getBoundaryContainerFunc;

        /// <summary>
        /// Creates a new <see cref="SpatiallyVariantBoundaryNodePresenter"/>.
        /// </summary>
        /// <param name="getBoundaryContainerFunc">
        /// The function to obtain the <see cref="IBoundaryContainer"/>
        /// of an <see cref="IWaveBoundary"/>.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="getBoundaryContainerFunc"/> is <c>null</c>.
        /// </exception>
        public SpatiallyVariantBoundaryNodePresenter(Func<IWaveBoundary, IBoundaryContainer> getBoundaryContainerFunc)
        {
            this.getBoundaryContainerFunc = getBoundaryContainerFunc ??
                                            throw new ArgumentNullException(nameof(getBoundaryContainerFunc));
        }

        public override IMenuItem GetContextMenu(ITreeNode sender, object nodeData)
        {
            if (!(nodeData is IWaveBoundary waveBoundary))
            {
                return null;
            }

            IBoundaryContainer boundaryContainer = GetBoundaryContainer(waveBoundary);

            ContextMenuStrip contextMenu = GetDeleteContextMenu(waveBoundary, boundaryContainer);
            return new MenuItemContextMenuStripAdapter(contextMenu);
        }

        protected override string GetNodeText(IWaveBoundary data) => data.Name;

        [ExcludeFromCodeCoverage]
        protected override Image GetNodeImage(IWaveBoundary data) => boundaryImage;

        protected override bool CanRemove(IWaveBoundary nodeData) => true;

        protected override bool RemoveNodeData(object parentNodeData, IWaveBoundary nodeData)
        {
            return OnDeleteBoundary(nodeData);
        }

        private IBoundaryContainer GetBoundaryContainer(IWaveBoundary waveBoundary)
        {
            return getBoundaryContainerFunc.Invoke(waveBoundary);
        }

        private ContextMenuStrip GetDeleteContextMenu(IWaveBoundary waveBoundary,
                                                      IBoundaryContainer boundaryContainer)
        {
            var contextMenu = new ContextMenuStrip();
            var item = new ClonableToolStripMenuItem
            {
                Text = Properties.Resources.WaveBoundaryNodePresenter_GetContextMenu_Delete,
                Tag = boundaryContainer
            };
            item.Click += (s, a) => OnDeleteBoundary(waveBoundary);
            item.Image = Resources.DeleteHS;

            contextMenu.Items.Add(item);
            return contextMenu;
        }

        private bool OnDeleteBoundary(IWaveBoundary waveBoundary)
        {
            IBoundaryContainer boundaryContainer = GetBoundaryContainer(waveBoundary);
            return boundaryContainer.Boundaries.Remove(waveBoundary);
        }
    }
}