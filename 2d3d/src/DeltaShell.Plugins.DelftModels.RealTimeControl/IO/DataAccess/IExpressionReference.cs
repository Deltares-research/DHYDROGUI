namespace DeltaShell.Plugins.DelftModels.RealTimeControl.IO.DataAccess
{
    /// <summary>
    /// Represents an expression parameter reference that is retrieved from the rtc tools config xml file.
    /// </summary>
    public interface IExpressionReference
    {
        /// <summary>
        /// Gets the value that was specified in the file.
        /// </summary>
        string Value { get; }
    }
}