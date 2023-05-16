using DelftTools.Hydro.CrossSections;

namespace DelftTools.Hydro.Structures
{
    /// <summary>
    /// Provides extension methods for <see cref="IBridge"/>.
    /// </summary>
    public static class BridgeExtensions
    {
        /// <summary>
        /// Returns a shifted clone of the cross section definition.
        /// </summary>
        /// <param name="bridge">The bridge for which to get the cross section definition.</param>
        /// <returns>A new cross section definition instance with the bridge's shift value applied.</returns>
        public static ICrossSectionDefinition GetShiftedCrossSectionDefinition(this IBridge bridge)
        {
            return bridge.CrossSectionDefinition.AddLevel(bridge.Shift);
        }
    }
}