namespace DeltaShell.NGHS.Common.Eventing
{
    /// <summary>
    /// Notifies clients that a property value has changed.
    /// </summary>
    public interface INotifyPropertyChangedExtended
    {
        /// <summary>
        /// Occurs when a property value has changed.
        /// </summary>
        event PropertyChangedExtendedEventHandler PropertyChanged;
    }
}