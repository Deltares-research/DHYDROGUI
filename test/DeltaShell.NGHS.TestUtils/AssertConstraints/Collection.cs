namespace DeltaShell.NGHS.TestUtils.AssertConstraints
{
    /// <summary>
    /// Contains constraints for collections
    /// </summary>
    public static class Collection
    {
        /// <summary>
        /// Provides a contains only constraint that can be applied to a collection
        /// to test whether it only contains the specified <paramref name="expectedItem"/>.
        /// </summary>
        /// <param name="expectedItem">The only expected item</param>
        /// <returns>A contains only constraint.</returns>
        public static ContainsOnlyConstraint OnlyContains(object expectedItem)
        {
            return new ContainsOnlyConstraint(expectedItem);
        }
    }
}