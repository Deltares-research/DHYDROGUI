namespace DeltaShell.NGHS.Common.Eventing
{
    /// <summary>
    /// Represents the method that will handle the <see cref="INotifyPropertyChangedExtended.PropertyChanged" />
    /// event raised when a property is changed on a component.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">A <see cref="PropertyChangedExtendedEventArgs"/> that contains the event data. </param>
    public delegate void PropertyChangedExtendedEventHandler(object sender, PropertyChangedExtendedEventArgs e);
}
