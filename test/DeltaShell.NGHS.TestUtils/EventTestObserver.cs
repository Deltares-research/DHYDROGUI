using System;
using System.Collections.Generic;

namespace DeltaShell.NGHS.TestUtils
{
    /// <summary>
    /// <see cref="EventTestObserver{T}"/> provides functionality
    /// to observe an event, and collect the number of
    /// calls, senders, and event arguments.
    /// </summary>
    public class EventTestObserver<T> where T : EventArgs
    {
        /// <summary>
        /// Gets or sets the number of calls.
        /// </summary>
        public int NCalls { get; private set; }

        /// <summary>
        /// Gets the senders observed.
        /// </summary>
        public IList<object> Senders { get; } = new List<object>();

        /// <summary>
        /// Gets the event arguments observed.
        /// </summary>
        public IList<T> EventArgses { get; } = new List<T>();

        /// <summary>
        /// Method to be attached to the event.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="eventArgs">The <see cref="T"/> instance containing the event data.</param>
        public void OnEventFired(object sender, T eventArgs)
        {
            NCalls += 1;
            Senders.Add(sender);
            EventArgses.Add(eventArgs);
        }
    }
}