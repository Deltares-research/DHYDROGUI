using System.ComponentModel;

namespace DeltaShell.NGHS.Common.Eventing
{
    /// <summary>
    /// Provides extended data for the <see cref="INotifyPropertyChanged.PropertyChanged"/> event.
    /// </summary>
    /// <seealso cref="PropertyChangedEventArgs"/>
    public class PropertyChangedExtendedEventArgs : PropertyChangedEventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyChangedExtendedEventArgs"/> class.
        /// </summary>
        /// <param name="propertyName"> Name of the property. </param>
        /// <param name="originalValue"> The original value of the property. </param>
        public PropertyChangedExtendedEventArgs(string propertyName, object originalValue)
            : base(propertyName)
        {
            OriginalValue = originalValue;
        }

        /// <summary>
        /// Gets the original property value.
        /// </summary>
        /// <value>
        /// The original property value.
        /// </value>
        public virtual object OriginalValue { get; }
    }
}