using AutoFixture;
using Deltares.Infrastructure.API.Guards;

namespace DeltaShell.NGHS.TestUtils.AutoFixtureCustomizations
{
    /// <summary>
    /// A customization that changes how <see cref="System.Enum"/> values are generated.
    /// Uses <see cref="RandomEnumGenerator"/>.
    /// </summary>
    public class RandomEnumCustomization : ICustomization
    {
        /// <summary>
        /// Customizes the specified <paramref name="fixture"/> by adding <see cref="RandomEnumGenerator"/>
        /// as a default strategy for creating a new <see cref="System.Enum"/> value.
        /// </summary>
        /// <param name="fixture">The fixture to customize.</param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when <paramref name="fixture"/> is <c>null</c>.
        /// </exception>
        public void Customize(IFixture fixture)
        {
            Ensure.NotNull(fixture, nameof(fixture));

            fixture.Customizations.Add(new RandomEnumGenerator());
        }
    }
}