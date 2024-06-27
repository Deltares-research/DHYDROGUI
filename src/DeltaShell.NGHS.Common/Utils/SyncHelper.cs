using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using Deltares.Infrastructure.API.Guards;

namespace DeltaShell.NGHS.Common.Utils
{
    /// <summary>
    /// A helper for synchronisation.
    /// </summary>
    public static class SyncHelper
    {
        /// <summary>
        /// Creates a <see cref="NotifyCollectionChangedEventHandler"/> which synchronizes the
        /// specified <paramref name="collection"/> with the source of the events.
        /// </summary>
        /// <param name="collection"> The collection to be synced. </param>
        /// <typeparam name="T"> The type of the items in the collection. </typeparam>
        /// <returns>The created <see cref="NotifyCollectionChangedEventHandler"/> </returns>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when <paramref name="collection"/> is <c>null</c>.
        /// </exception>
        /// <remarks>
        /// Remember to unsubscribe this event handler when needed, preventing the unwanted
        /// modification of the <paramref name="collection"/>.
        /// Note that to unsubscribe this event handler, a reference to the created instance is needed.
        /// </remarks>
        public static NotifyCollectionChangedEventHandler GetSyncNotifyCollectionChangedEventHandler<T>(ICollection<T> collection)
        {
            Ensure.NotNull(collection, nameof(collection));

            return (sender, e) =>
            {
                IEnumerable<T> itemsToRemove = e.OldItems?.OfType<T>() ?? Enumerable.Empty<T>();
                foreach (T data in itemsToRemove)
                {
                    collection.Remove(data);
                }

                IEnumerable<T> itemsToAdd = e.NewItems?.OfType<T>() ?? Enumerable.Empty<T>();
                foreach (T data in itemsToAdd)
                {
                    collection.Add(data);
                }
            };
        }
    }
}