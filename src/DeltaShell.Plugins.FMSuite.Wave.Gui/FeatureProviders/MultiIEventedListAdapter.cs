using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Windows.Markup;
using DelftTools.Utils.Collections;
using DelftTools.Utils.Collections.Extensions;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries;

namespace DeltaShell.Plugins.FMSuite.Wave.Gui.FeatureProviders
{
    /// <summary>
    /// <see cref="MultiIEventedListAdapter{TObserved, TDisplayed}" /> provides
    /// an adapter implementation to view multiple <see cref="IEventedList{TObserved}" />
    /// as a single <see cref="IEventedList{TDisplayed}" /> and <see cref="IList" />.
    /// </summary>
    /// <typeparam name="TObserved">The type of objects which are observed.</typeparam>
    /// <typeparam name="TDisplayed">The type of objects which are displayed.</typeparam>
    /// <seealso cref="IEventedList{TDisplayed}" />
    /// <seealso cref="IList" />
    /// <remarks>
    /// This class is required to properly map the data within the DataModel of the
    /// waves model on the required FeatureProviders. This is purely done, such that it can
    /// play nice with the expectations of the framework and sharpmap in particular.
    /// 
    /// The <see cref="MultiIEventedListAdapter{TObserved,TDisplayed}" /> will hold a copy
    /// of each <typeparamref name="TDisplayed" /> for each <typeparamref name="TObserved" />.
    ///
    /// This list does not respect ordering of elements, as this would lead to unnecessary
    /// overhead to figure this out for the different lists.
    /// </remarks>
    /// <invariant>
    /// | There are no null values within the <see cref="MultiIEventedListAdapter" />.
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
            if (obtainObservedValueFunc == null)
            {
                throw new ArgumentNullException(nameof(obtainObservedValueFunc));
            }

            if (createDisplayedValueFunc == null)
            {
                throw new ArgumentNullException(nameof(createDisplayedValueFunc));
            }

            this.obtainObservedValueFunc = obtainObservedValueFunc;
            this.createDisplayedValueFunc = createDisplayedValueFunc;

            values = new List<Tuple<TDisplayed, IEventedList<TObserved>>>();

            SyncRoot = new object();
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
            if (observedList == null)
            {
                throw new ArgumentNullException(nameof(observedList));
            }

            AddObservedListContents(observedList);
            SubscribeToObservedList(observedList);
        }

        public void DeregisterList(IEventedList<TObserved> observedList)
        {
            if (observedList == null)
            {
                throw new ArgumentNullException(nameof(observedList));
            }

            RemoveSourceListContents(observedList);
            UnsubscribeFromSourceList(observedList);
        }

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
                default:
                    break;
            }
        }

        private void HandleCollectionChangedReplaced(IEventedList<TObserved> observedList, NotifyCollectionChangedEventArgs e)
        {
            var removedItems = RemoveItems(observedList, e.OldItems);
            var addedItems = AddItems(observedList, e.NewItems);
            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(
                                          NotifyCollectionChangedAction.Replace,
                                          addedItems,
                                          removedItems));
        }

        private void HandleCollectionChangedAdd(IEventedList<TObserved> observedList, NotifyCollectionChangedEventArgs e)
        {
            var addedItems = AddItems(observedList, e.NewItems);
            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(
                                          NotifyCollectionChangedAction.Add,
                                          addedItems));
        }

        private void HandleCollectionChangedRemove(IEventedList<TObserved> observedList, NotifyCollectionChangedEventArgs e)
        {
            var removedItems = RemoveItems(observedList, e.OldItems);
            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(
                                          NotifyCollectionChangedAction.Remove,
                                          removedItems));
        }

        private IList AddItems(IEventedList<TObserved> observedList, IList addedItems)
        {
            var result = new List<TDisplayed>();

            foreach (var addedItem in addedItems)
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

        private TDisplayed Emplace(IEventedList<TObserved> container, TObserved observedFeature)
        {
            var newGoalValue = CreateDisplayedValue(observedFeature);
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

        public void AddRange(IEnumerable<TDisplayed> enumerable)
        {
            throw new NotImplementedException();
        }

        public IEnumerator<TDisplayed> GetEnumerator() => values.Select(x => x.Item1).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public void Add(TDisplayed item)
        {
            throw new NotImplementedException();
        }

        public int Add(object value)
        {
            throw new NotImplementedException();
        }

        public bool Contains(TDisplayed item) => values.Any(x => Equals(x.Item1, item));

        public bool Contains(object value)
        {
            if (!(value is TDisplayed displayedItem))
            {
                return false;
            }

            return this.Contains(displayedItem);
        }

        public int IndexOf(object value)
        {
            throw new NotImplementedException();
        }

        public void Insert(int index, object value)
        {
            throw new NotImplementedException();
        }

        public bool Remove(TDisplayed item)
        {
            throw new NotImplementedException();
        }

        public void Remove(object value)
        {
            throw new NotImplementedException();
        }

        void IList.RemoveAt(int index)
        {
            throw new NotImplementedException();
        }

        object IList.this[int index]
        {
            get => throw new NotImplementedException();
            set => throw new NotImplementedException();
        }

        public bool IsReadOnly => false;

        public bool IsFixedSize => false;

        public void Clear() => throw new NotSupportedException("This operation is currently not supported.");


        public void CopyTo(TDisplayed[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public void CopyTo(Array array, int index)
        {
            throw new NotImplementedException();
        }

        public int Count => values.Count;

        public object SyncRoot { get; }
        public bool IsSynchronized { get; }

        public int IndexOf(TDisplayed item)
        {
            throw new NotImplementedException();
        }

        public void Insert(int index, TDisplayed item)
        {
            throw new NotImplementedException();
        }

        void IList<TDisplayed>.RemoveAt(int index)
        {
            throw new NotImplementedException();
        }

        public TDisplayed this[int index]
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public event NotifyCollectionChangingEventHandler CollectionChanging;
        public event NotifyCollectionChangedEventHandler CollectionChanged;

        public bool SkipChildItemEventBubbling { get; set; }
    }
}