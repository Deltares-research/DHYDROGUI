namespace DeltaShell.Plugins.DelftModels.RealTimeControl.ImportExport.DataAccess
{
    public interface ILeafNode : IExpressionNode
    {
        string Value { get; set; }

    }
}