using System.Drawing;
using System.Windows.Forms;
using DelftTools.Controls;
using DelftTools.Controls.Swf;
using DelftTools.Shell.Gui.Swf;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.NGHS.Common.Gui.NodePresenters;
using GeoAPI.Extensions.Feature;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Gui.NodePresenters
{
    internal class FeatureProjectTreeViewNodePresenter<T> : TreeViewNodePresenterBaseForPluginGui<IEventedList<T>>
        where T : IFeature
    {
        private readonly string name;
        private readonly Image image;

        public FeatureProjectTreeViewNodePresenter(string name, Image image)
        {
            this.name = name;
            this.image = image;
        }

        public override void UpdateNode(ITreeNode parentNode, ITreeNode node, IEventedList<T> nodeData)
        {
            node.Text = name;
            node.Image = image;
        }

        public override IMenuItem GetContextMenu(ITreeNode sender, object nodeData)
        {
            IMenuItem menuBase = base.GetContextMenu(sender, nodeData);
            IMenuItem menu = NodePresenterHelper.GetContextMenuFromPluginGuis(Gui, sender, nodeData);
            if (menuBase != null)
            {
                menu.Add(menuBase);
            }

            ContextMenuStrip contextMenuStrip = ContextMenuFactory.CreateMenuFor(nodeData, Gui, this, sender);
            menu.Add(new MenuItemContextMenuStripAdapter(contextMenuStrip));

            return menu;
        }
    }
}