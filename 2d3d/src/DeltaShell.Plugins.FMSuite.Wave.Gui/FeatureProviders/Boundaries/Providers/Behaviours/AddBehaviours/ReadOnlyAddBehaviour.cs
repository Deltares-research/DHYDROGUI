using System;
using GeoAPI.Geometries;

namespace DeltaShell.Plugins.FMSuite.Wave.Gui.FeatureProviders.Boundaries.Providers.Behaviours.AddBehaviours
{
    /// <summary>
    /// <see cref="ReadOnlyAddBehaviour"/> defines read-only add behaviour,
    /// where executing the add behaviour results in a <see cref="System.NotSupportedException"/>.
    /// </summary>
    /// <seealso cref="IAddBehaviour"/>
    public sealed class ReadOnlyAddBehaviour : IAddBehaviour
    {
        /// <summary>
        /// Throws a <see cref="System.NotSupportedException"/>.
        /// </summary>
        /// <param name="geometry">The geometry of the Feature to add.</param>
        /// <exception cref="System.NotSupportedException">
        /// Add behaviour is not supported when the provider is considered
        /// read-only.
        /// </exception>
        public void Execute(IGeometry geometry)
        {
            throw new NotSupportedException("Add behaviour is not supported when the provider is considered read-only.");
        }
    }
}