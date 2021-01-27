using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using DelftTools.Utils.Collections.Generic;
using DelftTools.Utils.Guards;

namespace DeltaShell.NGHS.Common.Utils
{
    /// <summary>
    /// A helper for synchronisation.
    /// </summary>
    public static class SyncHelper
    {
        /// <summary>
        /// Synchronizes the specified <paramref name="list"/> with the added or removed items.
        /// </summary>
        /// <param name="list"> The list to be synced. </param>
        /// <typeparam name="T"> The type of the items in the list. </typeparam>
        /// <returns>The created <see cref="NotifyCollectionChangedEventHandler"/> </returns>
        /// <remarks>
        /// The method returns when the sender is not of the same type as <paramref name="list"/>
        /// </remarks>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when <paramref name="list"/> is <c>null</c>.
        /// </exception>
        public static NotifyCollectionChangedEventHandler GetSyncNotifyCollectionChangedEventHandler<T>(IEventedList<T> list)
        {
            Ensure.NotNull(list, nameof(list));

            return (sender, e) =>
            {
                if (!(sender is IEventedList<T>))
                {
                    return;
                }

                IEnumerable<T> itemsToRemove = e.OldItems?.Cast<T>() ?? Enumerable.Empty<T>();
                foreach (T data in itemsToRemove)
                {
                    list.Remove(data);
                }

                IEnumerable<T> itemsToAdd = e.NewItems?.Cast<T>() ?? Enumerable.Empty<T>();
                list.AddRange(itemsToAdd);
            };
        }
    }
}