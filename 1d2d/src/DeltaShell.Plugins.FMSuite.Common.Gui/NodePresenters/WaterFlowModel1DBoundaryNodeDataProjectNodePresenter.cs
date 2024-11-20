using System.Drawing;
using DelftTools.Controls;
using DelftTools.Shell.Gui.Swf;
using DeltaShell.NGHS.IO.DataObjects;
using DeltaShell.Plugins.FMSuite.Common.Gui.Properties;

namespace DeltaShell.Plugins.FMSuite.Common.Gui.NodePresenters
{
    public class Model1DBoundaryNodeDataProjectNodePresenter : TreeViewNodePresenterBaseForPluginGui<Model1DBoundaryNodeData>
    {
        public override bool CanRenameNode(ITreeNode node)
        {           
            return false;
        }

        public override void UpdateNode(ITreeNode parentNode, ITreeNode node, Model1DBoundaryNodeData nodeData)
        {
            node.Image = GetImageForType(nodeData.DataType);
            node.Text = nodeData.Name;
        }

        private static Image GetImageForType(Model1DBoundaryNodeDataType type)
        {
            switch (type)
            {
                case Model1DBoundaryNodeDataType.WaterLevelTimeSeries:
                    return Resources.HBoundary;
                case Model1DBoundaryNodeDataType.FlowTimeSeries:
                    return Resources.QBoundary;
                case Model1DBoundaryNodeDataType.FlowWaterLevelTable:
                    return Resources.QHBoundary;
                case Model1DBoundaryNodeDataType.FlowConstant:
                    return Resources.QConst;
                case Model1DBoundaryNodeDataType.WaterLevelConstant:
                    return Resources.HConst;
                case Model1DBoundaryNodeDataType.None:
                    return Resources.none;
            }

            return null;
        }

        public override IMenuItem GetContextMenu(ITreeNode sender, object nodeData)
        {
            return GuiPlugin == null ? null : GuiPlugin.GetContextMenu(null, nodeData);
        }
    }
}