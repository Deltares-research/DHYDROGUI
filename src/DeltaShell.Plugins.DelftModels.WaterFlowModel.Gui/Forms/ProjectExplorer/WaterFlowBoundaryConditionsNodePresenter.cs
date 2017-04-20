namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.Forms.ProjectExplorer
{
    // TODO: remove it, not used
/*
    public class WaterFlowBoundaryConditionsNodePresenter : DataItemSetNodePresenter
    {
        public WaterFlowBoundaryConditionsNodePresenter(GuiPlugin GuiPlugin) : base(GuiPlugin)
        {
        }

        public override IEnumerable GetChildNodeObjects(DataItemSet parentNodeData, ITreeNode node)
        {
            var childNodeObjects = base.GetChildNodeObjects(parentNodeData, node);
            return childNodeObjects.OfType<DataItem>().Where(SelectDataItem);
        }

        private static bool SelectDataItem(DataItem dataItem)
        {
            var boundaryData = dataItem.Value as WaterFlowModel1DBoundaryNodeData;
            return boundaryData == null || boundaryData.DataType != WaterFlowModel1DBoundaryNodeDataType.None;
        }

        public override bool IsPresenterForNode(ITreeNode parentNode)
        {
            return parentNode != null &&
                   parentNode.Tag is TreeFolder &&
                   parentNode.Parent != null &&
                   parentNode.Parent.Tag is WaterFlowModel1D;
        }
    }
*/
}