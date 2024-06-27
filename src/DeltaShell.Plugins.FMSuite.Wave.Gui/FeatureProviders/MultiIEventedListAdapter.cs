using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using DelftTools.Utils.Collections;
using DelftTools.Utils.Collections.Generic;
using Deltares.Infrastructure.API.Guards;

namespace DeltaShell.Plugins.FMSuite.Wave.Gui.FeatureProviders
{
    /// <summary>
    /// <see cref="MultiIEventedListAdapter{TObserved, TDisplayed}"/> provides
    /// an adapter implementation to view multiple <see cref="IEventedList{T}"/>
    /// as a single <see cref="IEventedList{TDisplayed}"/> and <see cref="IList"/>.
    /// </summary>
    /// <typeparam name="TObserved">The type of objects which are observed.</typeparam>
    /// <typeparam name="TDisplayed">The type of objects which are displayed.</typeparam>
    /// <seealso cref="IEventedList{TDisplayed}"/>
    /// <seealso cref="IList"/>
    /// <remarks>
    /// This class is required to properly map the data within the DataModel of the
    /// waves model on the required FeatureProviders. This is purely done, such that it can
    /// play nice with the expectations of the framework and sharpmap in particular.
    /// The <see cref="MultiIEventedListAdapter{TObserved,TDisplayed}"/> will hold a copy
    /// of each <typeparamref name="TDisplayed"/> for each <typeparamref name="TObserved"/>.
    /// This list does not respect ordering of elements, as this would lead to unnecessary
    /// overhead to figure this out for the different lists.
    /// </remarks>
    /// <invariant>
    /// | There are no null values within the <see cref="MultiIEventedListAdapter{TObserved, TDisplayed}"/>.
    /// </invariant>
    public class MultiIEventedListAdapter<TObserved, TDisplayed> : IEventedList<TDisplayed>, IList
    {
        /// <summary>
        /// The values of this <see cref="MultiIEventedListAdapter{TObserved,TDisplayed}"/>
        /// and the respective <see cref="IEventedList{TObserved}"/> they belong too.
        /// </summary>
        private readonly IList<Tuple<TDisplayed, IEventedList<TObserved>>> values;

        private readonly Func<TDisplayed, Tuple<TObserved, IEventedList<TObserved>>> obtainObservedValueFunc;
        private readonly Func<TObserved, TDisplayed> createDisplayedValueFunc;

        private int? nextAdd = null;

        public event NotifyCollectionChangingEventHandler CollectionChanging;
        public event NotifyCollectionChangedEventHandler CollectionChanged;

        /// <summary>
        /// Create a new <see cref="MultiIEventedListAdapter{TObserved, TDisplayed}"/>.
        /// </summary>
        /// <param name="obtainObservedValueFunc">
        /// The function to obtain a <typeparamref name="TObserved"/> and its
        /// containing list from a <typeparamref name="TDisplayed"/>.
        /// </param>
        /// <param name="createDisplayedValueFunc">
        /// The function to create a <typeparamref name="TDisplayed"/> from a
        /// <typeparamref name="TObserved"/>.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when any parameter is <c>null</c>.
        /// </exception>
        public MultiIEventedListAdapter(Func<TDisplayed, Tuple<TObserved, IEventedList<TObserved>>> obtainObservedValueFunc,
                                        Func<TObserved, TDisplayed> createDisplayedValueFunc)
        {
            Ensure.NotNull(obtainObservedValueFunc, nameof(obtainObservedValueFunc));
            Ensure.NotNull(createDisplayedValueFunc, nameof(createDisplayedValueFunc));

            this.obtainObservedValueFunc = obtainObservedValueFunc;
            this.createDisplayedValueFunc = createDisplayedValueFunc;

            values = new List<Tuple<TDisplayed, IEventedList<TObserved>>>();

            SyncRoot = new object();
        }

        public TDisplayed this[int index]
        {
            get => values[index].Item1;
            set => throw new NotSupportedException("Currently not supported, implement when needed");
        }

        public bool IsReadOnly => false;

        public int Count => values.Count;

        public bool SkipChildItemEventBubbling { get; set; }

        public bool IsFixedSize => false;

        public object SyncRoot { get; }
        public bool IsSynchronized { get; }

        /// <summary>
        /// Register <paramref name="observedList"/> to the observed lists of this
        /// <see cref="MultiIEventedListAdapter{TObserved,TDisplayed}"/>.
        /// </summary>
        /// <param name="observedList">The list to be observed.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="observedList"/> is <c>null</c>.
        /// </exception>
        public void RegisterList(IEventedList<TObserved> observedList)
        {
            Ensure.NotNull(observedList, nameof(observedList));

            AddObservedListContents(observedList);
            SubscribeToObservedList(observedList);
        }

        public void DeregisterList(IEventedList<TObserved> observedList)
        {
            Ensure.NotNull(observedList, nameof(observedList));

            RemoveSourceListContents(observedList);
            UnsubscribeFromSourceList(observedList);
        }

        public void AddRange(IEnumerable<TDisplayed> enumerable) => enumerable.ForEach(Add);

        public IEnumerator<TDisplayed> GetEnumerator() => values.Select(x => x.Item1).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public void Add(TDisplayed item)
        {
            throw new NotSupportedException("Currently not supported, implement when needed");
        }

        public bool Contains(TDisplayed item) => values.Any(x => Equals(x.Item1, item));

        public int IndexOf(TDisplayed item)
        {
            for (var i = 0; i < values.Count; i++)
            {
                if (Equals(item, values[i].Item1))
                {
                    return i;
                }
            }

            return -1;
        }

        public void Insert(int index, TDisplayed item)
        {
            throw new NotSupportedException("Currently not supported, implement when needed");
        }

        public bool Remove(TDisplayed item)
        {
            // We do not delete the item directly from the MultiIEventedListAdapter, 
            // instead we delete the item in the underlying EventedList, and wait for the
            // remove event to bubble up, at which time we will remove the actual item from
            // this values.
            Tuple<TDisplayed, IEventedList<TObserved>> itemInValues =
                values.FirstOrDefault(valueInList => Equals(valueInList.Item1, item));

            if (itemInValues == null)
            {
                return false;
            }

            Tuple<TObserved, IEventedList<TObserved>> observedValue = ObtainObservedValue(itemInValues.Item1);
            return itemInValues.Item2.Remove(observedValue.Item1);
        }

        public void RemoveAt(int index)
        {
            if (index < 0 || index >= Count)
            {
                return;
            }

            Tuple<TDisplayed, IEventedList<TObserved>> removalCandidate = values[index];
            Tuple<TObserved, IEventedList<TObserved>> observedValue = ObtainObservedValue(removalCandidate.Item1);
            removalCandidate.Item2.Remove(observedValue.Item1);
        }

        public void Clear() => throw new NotSupportedException("This operation is currently not supported.");

        public void CopyTo(TDisplayed[] array, int arrayIndex)
        {
            throw new NotSupportedException("Currently not supported, implement when needed");
        }

        public int Add(object value)
        {
            throw new NotSupportedException("Currently not supported, implement when needed");
        }

        public bool Contains(object value)
        {
            if (!(value is TDisplayed displayedItem))
            {
                return false;
            }

            return Contains(displayedItem);
        }

        public int IndexOf(object value)
        {
            if (!(value is TDisplayed goalValue))
            {
                return -1;
            }

            return IndexOf(goalValue);
        }

        public void Insert(int index, object value)
        {
            throw new NotSupportedException("Currently not supported, implement when needed");
        }

        public void Remove(object value)
        {
            if (!(value is TDisplayed valueAsDisplayed))
            {
                return;
            }

            Remove(valueAsDisplayed);
        }

        public void CopyTo(Array array, int index)
        {
            throw new NotSupportedException("Currently not supported, implement when needed");
        }

        /// <summary>
        /// Obtain the observed value and the <see cref="IEventedList{TObserved}"/>
        /// it belongs to, from the provided <paramref name="val"/>.
        /// </summary>
        /// <param name="val">
        /// The <typeparamref name="TDisplayed"/> of which the corresponding
        /// <typeparamref name="TObserved"/> should be retrieved.
        /// </param>
        /// <returns>
        /// A tuple consisting of a <typeparamref name="TObserved"/>
        /// corresponding with the provided <paramref name="val"/> and the
        /// <see cref="IEventedList{TObserved}"/> the <typeparamref name="TObserved"/>
        /// belongs to.
        /// </returns>
        private Tuple<TObserved, IEventedList<TObserved>> ObtainObservedValue(TDisplayed val) =>
            obtainObservedValueFunc.Invoke(val);

        /// <summary>
        /// Create a <typeparamref name="TDisplayed"/> corresponding with the
        /// provided <paramref name="val"/>.
        /// </summary>
        /// <param name="val">
        /// The value from which the <typeparamref name="TDisplayed"/> should
        /// be created.
        /// </param>
        /// <returns>
        /// The <typeparamref name="TDisplayed"/> based on the <paramref name="val"/>.
        /// </returns>
        private TDisplayed CreateDisplayedValue(TObserved val) =>
            createDisplayedValueFunc.Invoke(val);

        private void AddObservedListContents(IEventedList<TObserved> observedList)
        {
            if (!observedList.Any())
            {
                return;
            }

            IList addedItems = AddItems(observedList, observedList.ToList());
            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(
                                          NotifyCollectionChangedAction.Add,
                                          addedItems));
        }

        private void RemoveSourceListContents(IEventedList<TObserved> observedList)
        {
            if (!observedList.Any())
            {
                return;
            }

            IList removeItems = RemoveItems(observedList, observedList.ToList());
            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(
                                          NotifyCollectionChangedAction.Remove,
                                          removeItems));
        }

        private void UnsubscribeFromSourceList(IEventedList<TObserved> observedList)
        {
            observedList.CollectionChanged -= HandleCollectionChanged;
        }

        private void SubscribeToObservedList(IEventedList<TObserved> observedList)
        {
            observedList.CollectionChanged += HandleCollectionChanged;
        }

        private void HandleCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (!(sender is IEventedList<TObserved> observedList))
            {
                return;
            }

            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Replace:
                    HandleCollectionChangedReplaced(observedList, e);
                    break;
                case NotifyCollectionChangedAction.Add:
                    HandleCollectionChangedAdd(observedList, e);
                    break;
                case NotifyCollectionChangedAction.Remove:
                case NotifyCollectionChangedAction.Reset:
                    HandleCollectionChangedRemove(observedList, e);
                    break;
                case NotifyCollectionChangedAction.Move:
                // The MultiEventedListAdapter ordering is explicitly not dependent on the underlying lists.
                // As such, we only need to handle collection changed events that modify the contents of the
                // underlying lists, and thus the MultiEventedListAdapter. Since the underlying lists' content
                // does not change with a move action, we do not need to do anything. And thus there is no action
                // associated with a NotifyCollectionChangedAction.Move.
                default:
                    break;
            }
        }

        private void HandleCollectionChangedReplaced(IEventedList<TObserved> observedList, NotifyCollectionChangedEventArgs e)
        {
            IList removedItems = RemoveItems(observedList, e.OldItems);
            IList addedItems = AddItems(observedList, e.NewItems);
            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(
                                          NotifyCollectionChangedAction.Replace,
                                          addedItems,
                                          removedItems));
        }

        private void HandleCollectionChangedAdd(IEventedList<TObserved> observedList, NotifyCollectionChangedEventArgs e)
        {
            IList addedItems = AddItems(observedList, e.NewItems);
            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(
                                          NotifyCollectionChangedAction.Add,
                                          addedItems));
        }

        private void HandleCollectionChangedRemove(IEventedList<TObserved> observedList, NotifyCollectionChangedEventArgs e)
        {
            IList removedItems = RemoveItems(observedList, e.OldItems);
            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(
                                          NotifyCollectionChangedAction.Remove,
                                          removedItems));
        }

        private IList AddItems(IEventedList<TObserved> observedList, IList addedItems)
        {
            var result = new List<TDisplayed>();

            foreach (object addedItem in addedItems)
            {
                if (!(addedItem is TObserved addedObservedItem))
                {
                    continue;
                }

                result.Add(Emplace(observedList, addedObservedItem));
            }

            return result;
        }

        private IList RemoveItems(IEventedList<TObserved> observedlist, IList removedItems)
        {
            var result = new List<TDisplayed>();

            foreach (object removedItem in removedItems)
            {
                if (!(removedItem is TObserved removedSourceItem) ||
                    !values.Any(x => IsValueEqualToObservedFeature(x, removedSourceItem, observedlist)))
                {
                    continue;
                }

                result.Add(PopItem(observedlist, removedSourceItem));
            }

            return result;
        }

        /// <summary>
        /// Create a new <typeparamref name="TDisplayed"/> in place from the provided parameters.
        /// </summary>
        /// <param name="container">The container.</param>
        /// <param name="observedFeature">The observed feature.</param>
        /// <returns> The created <typeparamref name="TDisplayed"/> value. </returns>
        /// <remarks>
        /// If <see cref="nextAdd"/> is set, the new value will be inserted
        /// at the specified <see cref="nextAdd"/> location, otherwise it will
        /// be appended to the internal list.
        /// </remarks>
        private TDisplayed Emplace(IEventedList<TObserved> container, TObserved observedFeature)
        {
            TDisplayed newGoalValue = CreateDisplayedValue(observedFeature);
            var newElement = new Tuple<TDisplayed, IEventedList<TObserved>>(newGoalValue, container);

            if (nextAdd != null)
            {
                values.Insert(nextAdd.Value, newElement);
                nextAdd = null;
            }
            else
            {
                values.Add(newElement);
            }

            return newGoalValue;
        }

        private bool IsValueEqualToObservedFeature(Tuple<TDisplayed, IEventedList<TObserved>> value,
                                                   TObserved sourceFeature,
                                                   IEventedList<TObserved> container)
        {
            return Equals(ObtainObservedValue(value.Item1).Item1, sourceFeature) &&
                   Equals(value.Item2, container);
        }

        private TDisplayed PopItem(IEventedList<TObserved> container, TObserved sourceFeature)
        {
            Tuple<TDisplayed, IEventedList<TObserved>> correspondingItem =
                values.First(x => IsValueEqualToObservedFeature(x, sourceFeature, container));

            values.Remove(correspondingItem);
            return correspondingItem.Item1;
        }

        object IList.this[int index]
        {
            get => this[index];
            set => throw new NotSupportedException("Currently not supported, implement when needed");
        }
    }
}