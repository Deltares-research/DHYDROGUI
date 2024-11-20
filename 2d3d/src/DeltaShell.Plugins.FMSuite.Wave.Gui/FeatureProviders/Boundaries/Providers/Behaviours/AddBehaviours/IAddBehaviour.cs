using GeoAPI.Geometries;

namespace DeltaShell.Plugins.FMSuite.Wave.Gui.FeatureProviders.Boundaries.Providers.Behaviours.AddBehaviours
{
    /// <summary>
    /// <see cref="IAddBehaviour"/> defines the add behaviour to which
    /// <see cref="SharpMap.Api.IFeatureProvider"/> of <see cref="Providers"/>
    /// can delegate their add methods.
    /// </summary>
    public interface IAddBehaviour
    {
        /// <summary>
        /// Execute this <see cref="IAddBehaviour"/> for the given
        /// <paramref name="geometry"/>.
        /// </summary>
        /// <param name="geometry">The geometry of the Feature to add.</param>
        /// <remarks>
        /// The actual behaviour and potential exceptions are left up to the
        /// implementing classes.
        /// </remarks>
        void Execute(IGeometry geometry);
    }
}