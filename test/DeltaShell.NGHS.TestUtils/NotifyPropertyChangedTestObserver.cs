using System.Collections.Generic;
using System.ComponentModel;

namespace DeltaShell.NGHS.TestUtils
{
    /// <summary>
    /// <see cref="NotifyPropertyChangedTestObserver"/> provides functionality
    /// to observe a NotifyPropertyChanged event, and collect the number of
    /// calls, senders, and event arguments.
    /// </summary>
    public class NotifyPropertyChangedTestObserver
    {
        /// <summary>
        /// Gets or sets the number of calls.
        /// </summary>
        public int NCalls { get; set; } = 0;

        /// <summary>
        /// Gets the senders observed.
        /// </summary>
        public IList<object> Senders { get; } = new List<object>();

        /// <summary>
        /// Gets the event arguments observed.
        /// </summary>
        public IList<PropertyChangedEventArgs> EventArgses { get; } = new List<PropertyChangedEventArgs>();

        /// <summary>
        /// Method to be attached to the property changed event.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="eventArgs">The <see cref="PropertyChangedEventArgs"/> instance containing the event data.</param>
        public void OnPropertyChanged(object sender, PropertyChangedEventArgs eventArgs)
        {
            NCalls += 1;
            Senders.Add(sender);
            EventArgses.Add(eventArgs);
        }
    }
}