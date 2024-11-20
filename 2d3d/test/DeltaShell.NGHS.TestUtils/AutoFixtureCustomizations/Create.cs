using AutoFixture;
using AutoFixture.AutoNSubstitute;

namespace DeltaShell.NGHS.TestUtils.AutoFixtureCustomizations
{
    /// <summary>
    /// <see cref="Create"/> provides methods to create configured objects.
    /// </summary>
    public static class Create
    {
        private static readonly IFixture fixture = new Fixture()
                                                   .Customize(new AutoNSubstituteCustomization {ConfigureMembers = true})
                                                   .Customize(new RandomBooleanSequenceCustomization())
                                                   .Customize(new RandomEnumCustomization());

        /// <summary>
        /// Created a configured instance of the requested type.
        /// </summary>
        /// <typeparam name="T">The type of object to create.</typeparam>
        /// <returns>
        /// The created object
        /// </returns>
        public static T For<T>() => fixture.Create<T>();
    }
}