using System;
using System.Collections.Generic;
using DelftTools.Utils.Collections.Generic;

namespace DeltaShell.Plugins.FMSuite.Wave.Gui.FeatureProviders
{
    /// <summary>
    /// <see cref="MultiIEventedListAdapter{TObserved, TDisplayed}" /> provides an adapter implementation
    /// to view multiple lists of an arbitrary type into an <see cref="IEventedList{TDisplayed}" />
    /// and <see cref="IList" /> of another type.
    /// </summary>
    /// <typeparam name="TObserved">The type of objects which are observed.</typeparam>
    /// <typeparam name="TDisplayed">The type of objects which are displayed.</typeparam>
    /// <remarks>
    /// This class is required to properly map the data within the DataModel of the
    /// waves model on the required FeatureProviders.
    ///
    /// The <see cref="MultiIEventedListAdapter{TObserved,TDisplayed}"/> will hold a copy
    /// of each <typeparamref name="TDisplayed"/> for each <typeparamref name="TObserved"/>.
    /// </remarks>
    /// <invariant>
    /// | There are no null values within the <see cref="MultiIEventedListAdapter"/>.
    /// </invariant>
    public class MultiIEventedListAdapter<TObserved, TDisplayed>
    {
        /// <summary>
        /// The values of this <see cref="MultiIEventedListAdapter{TObserved,TDisplayed}"/>
        /// and the respective <see cref="IEventedList{TObserved}"/> they belong too.
        /// </summary>
        private readonly IList<Tuple<TObserved, IEventedList<TObserved>>> values;

        private readonly Func<TDisplayed, Tuple<TObserved, IEventedList<TObserved>>> obtainObservedValueFunc;
        private readonly Func<TObserved, TDisplayed> createDisplayedValueFunc;

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

            values = new List<Tuple<TObserved, IEventedList<TObserved>>>();

        }

    }
}