using System.Collections;
using System.Drawing;
using DelftTools.Controls;
using DelftTools.Shell.Core.Workflow;
using DelftTools.Shell.Gui;
using DelftTools.Shell.Gui.Swf;
using DelftTools.Utils.Drawing;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Gui.Properties;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.Gui.NodePresenters
{
    public class OutputTreeFolderNodePresenter : TreeViewNodePresenterBaseForPluginGui<OutputTreeFolder>
    {
        public OutputTreeFolderNodePresenter(GuiPlugin guiPlugin) : base(guiPlugin) { }

            public override void UpdateNode(ITreeNode parentNode, ITreeNode node, OutputTreeFolder data)
            {
                node.Text = data.Text;
                node.Tag = data;
                node.Image = GetImage(data);
            }

            public override IEnumerable GetChildNodeObjects(OutputTreeFolder parentNodeData, ITreeNode node)
            {
                return parentNodeData.ChildItems;
            }

            private static Image GetImage(OutputTreeFolder data)
            {
                Image image = data.Image;

                if (data.Parent is IModel model && model.OutputOutOfSync)
                {
                    image = image.AddOverlayImage(Resources.ExclamationOverlay, 5, 1, 10, 10);
                }

                return image;
            }
        }
}