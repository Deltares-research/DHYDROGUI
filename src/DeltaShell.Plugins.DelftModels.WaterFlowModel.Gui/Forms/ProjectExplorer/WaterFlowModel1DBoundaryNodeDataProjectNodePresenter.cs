using System.Drawing;
using DelftTools.Controls;
using DelftTools.Shell.Gui.Swf;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.DataObjects;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.Gui.Properties;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.Gui.Forms.ProjectExplorer
{
    public class WaterFlowModel1DBoundaryNodeDataProjectNodePresenter : TreeViewNodePresenterBaseForPluginGui<WaterFlowModel1DBoundaryNodeData>
    {
        public override bool CanRenameNode(ITreeNode node)
        {           
            return false;
        }

        public override void UpdateNode(ITreeNode parentNode, ITreeNode node, WaterFlowModel1DBoundaryNodeData data)
        {
            node.Image = GetImageForType(data.DataType);
            node.Text = data.Name;
        }

        private static Image GetImageForType(WaterFlowModel1DBoundaryNodeDataType type)
        {
            switch (type)
            {
                case WaterFlowModel1DBoundaryNodeDataType.WaterLevelTimeSeries:
                    return Resources.HBoundary;
                case WaterFlowModel1DBoundaryNodeDataType.FlowTimeSeries:
                    return Resources.QBoundary;
                case WaterFlowModel1DBoundaryNodeDataType.FlowWaterLevelTable:
                    return Resources.QHBoundary;
                case WaterFlowModel1DBoundaryNodeDataType.FlowConstant:
                    return Resources.QConst;
                case WaterFlowModel1DBoundaryNodeDataType.WaterLevelConstant:
                    return Resources.HConst;
                case WaterFlowModel1DBoundaryNodeDataType.None:
                    return Resources.None;
            }

            return null;
        }

        public override IMenuItem GetContextMenu(ITreeNode sender, object nodeData)
        {
            return GuiPlugin == null ? null : GuiPlugin.GetContextMenu(null, nodeData);
        }
    }
}