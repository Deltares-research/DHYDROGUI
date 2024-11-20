using System;
using System.ComponentModel;
using NUnit.Framework;

namespace DeltaShell.NGHS.IO.TestUtils
{
    /// <summary>
    /// Test helper for testing objects implementing <see cref="INotifyPropertyChanged"/>
    /// </summary>
    public static class NotifyPropertyChangedTestHelper
    {
        /// <summary>
        /// Asserts the number of times <see cref="INotifyPropertyChanged.PropertyChanged"/> is fired when the specified
        /// <paramref name="action"/> is performed.
        /// </summary>
        /// <param name="npc">The notify property changed object.</param>
        /// <param name="action">The action to perform.</param>
        /// <param name="expectedCount">Expected number of times property changed is fired.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <param name="validateEventArgs">Action to validate the <see cref="PropertyChangedEventArgs"/>.</param>
        public static void AssertPropertyChangedFired(this INotifyPropertyChanged npc, Action action, int expectedCount, string propertyName = null, Action<PropertyChangedEventArgs> validateEventArgs = null)
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

                validateEventArgs?.Invoke(e);
            }

            Assert.That(propertyChangedCount, Is.EqualTo(expectedCount), "Property changed count");
        }
    }
}