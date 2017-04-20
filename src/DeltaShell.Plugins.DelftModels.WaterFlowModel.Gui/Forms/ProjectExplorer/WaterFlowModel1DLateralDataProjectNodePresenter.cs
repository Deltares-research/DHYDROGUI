using System.Drawing;
using DelftTools.Controls;
using DelftTools.Shell.Gui.Swf;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.DataObjects;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.Gui.Properties;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.Gui.Forms.ProjectExplorer
{
    public class WaterFlowModel1DLateralDataProjectNodePresenter : TreeViewNodePresenterBaseForPluginGui<WaterFlowModel1DLateralSourceData>
    {
        public override bool CanRenameNode(ITreeNode node)
        {           
            return false;
        }

        public override void UpdateNode(ITreeNode parentNode, ITreeNode node, WaterFlowModel1DLateralSourceData data)
        {
            node.Image = GetNodeImage(data);

            var foregroundColor = data.IsLinked ? Color.Gray : Color.Black;
            if (node.ForegroundColor != foregroundColor)
            {
                node.ForegroundColor = foregroundColor;
            }

            node.Text = data.Name;
        }

        private static Image GetNodeImage(WaterFlowModel1DLateralSourceData item)
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

        private static Image GetImageForType(WaterFlowModel1DLateralDataType type)
        {
            switch (type)
            {
                case WaterFlowModel1DLateralDataType.FlowTimeSeries:
                    return Resources.QBoundary;
                case WaterFlowModel1DLateralDataType.FlowConstant:
                    return Resources.QConst;
                case WaterFlowModel1DLateralDataType.FlowWaterLevelTable:
                    return Resources.QHBoundary;
            }

            return null;
        }

        public override IMenuItem GetContextMenu(ITreeNode sender, object nodeData)
        {
            return GuiPlugin == null ? null : GuiPlugin.GetContextMenu(null, nodeData);
        }
    }
}
