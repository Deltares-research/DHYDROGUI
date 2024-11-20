using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace DeltaShell.NGHS.TestUtils
{
    /// <summary>
    /// Assertion helper method to check if a collection only contains a certain element.
    /// </summary>
    public static class CollectionContainsOnlyAssert
    {
        /// <summary>
        /// Asserts if the given <paramref name="collection"/> only contains <paramref name="item"/>.
        /// </summary>
        /// <typeparam name="T">Tje type of the collection to assert.</typeparam>
        /// <param name="collection">The collection to assert.</param>
        /// <param name="item">The item that should be in the collection.</param>
        public static void AssertContainsOnly<T>(IEnumerable<T> collection, T item)
        {
            Assert.That(collection.SingleOrDefault(x => ReferenceEquals(x, item)), Is.Not.Null, 
                        "Collection does not contain given item");
        }
    }
}