using System.Drawing;
using DelftTools.Controls;
using DelftTools.Shell.Gui.Swf;
using DeltaShell.NGHS.IO.DataObjects;
using DeltaShell.Plugins.FMSuite.Common.Gui.Properties;

namespace DeltaShell.Plugins.FMSuite.Common.Gui.NodePresenters
{
    public class Model1DLateralDataProjectNodePresenter : TreeViewNodePresenterBaseForPluginGui<Model1DLateralSourceData>
    {
        public override bool CanRenameNode(ITreeNode node)
        {           
            return false;
        }

        public override void UpdateNode(ITreeNode parentNode, ITreeNode node, Model1DLateralSourceData nodeData)
        {
            node.Image = GetNodeImage(nodeData);

            var foregroundColor = nodeData.IsLinked ? Color.Gray : Color.Black;
            if (node.ForegroundColor != foregroundColor)
            {
                node.ForegroundColor = foregroundColor;
            }

            node.Text = nodeData.Name;
        }

        private static Image GetNodeImage(Model1DLateralSourceData item)
        {
            var image = GetImageForType(item.DataType);
            
            if (item.IsLinked)
            {
                // Draw link overlay on top of image
                AddLinkedOverlay(image);
            }
            
            return image;
        }

        private static void AddLinkedOverlay(Image image)
        {
            var graphics = Graphics.FromImage(image);
            var height = image.Height;
            var overlayDimension = height / 2;

            graphics.DrawImage(Resources.linkOverlay, 0, height - overlayDimension,
                               overlayDimension, overlayDimension);
        }

        private static Image GetImageForType(Model1DLateralDataType type)
        {
            switch (type)
            {
                case Model1DLateralDataType.FlowTimeSeries:
                    return Resources.QBoundary;
                case Model1DLateralDataType.FlowConstant:
                    return Resources.QConst;
                case Model1DLateralDataType.FlowWaterLevelTable:
                    return Resources.QHBoundary;
                case Model1DLateralDataType.FlowRealTime:
                    return Resources.realtime;
            }

            return null;
        }

        public override IMenuItem GetContextMenu(ITreeNode sender, object nodeData)
        {
            return GuiPlugin == null ? null : GuiPlugin.GetContextMenu(null, nodeData);
        }
    }
}
