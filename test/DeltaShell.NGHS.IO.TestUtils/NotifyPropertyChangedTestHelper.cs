using System;
using System.ComponentModel;

namespace DeltaShell.NGHS.IO.TestUtils
{
    /// <summary>
    /// Test helper for testing objects implementing <see cref="INotifyPropertyChanged"/>
    /// </summary>
    public static class NotifyPropertyChangedTestHelper
    {
        /// <summary>
        /// Counts the number of times <see cref="INotifyPropertyChanged.PropertyChanged"/> is fired when the specified
        /// <paramref name="action"/> is performed.
        /// </summary>
        /// <param name="npc">The notify property changed object.</param>
        /// <param name="action">The action to perform.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <returns>
        /// The number of times <see cref="INotifyPropertyChanged.PropertyChanged"/> is fired with the specified
        /// <paramref name="propertyName"/>
        /// </returns>
        public static int CountPropertyChangedFired(this INotifyPropertyChanged npc, Action action, string propertyName = null)
        {
            var propertyChangedCount = 0;

            npc.PropertyChanged += CountPropertyChanged;

            action.Invoke();

            void CountPropertyChanged(object sender, PropertyChangedEventArgs e)
            {
                if (e.PropertyName == propertyName || propertyName == null)
                {
                    propertyChangedCount++;
                }
            }

            return propertyChangedCount;
        }
    }
}