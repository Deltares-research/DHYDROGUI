using DelftTools.Utils.Validation.NameValidation;
using GeoAPI.Extensions.Feature;

namespace DelftTools.Hydro
{
    /// <summary>
    /// Represents a link between two instances of an <see cref="IHydroObject"/>.
    /// </summary>
    public interface IHydroLink : IFeature, IHasNameValidation
    {
        /// <summary>
        /// Gets or sets the source hydro object of this hydro link.
        /// </summary>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when this property is set with <c>null</c>.
        /// </exception>
        IHydroObject Source { get; set; }
        
        /// <summary>
        /// Gets or sets the target hydro object of this hydro link.
        /// </summary>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when this property is set with <c>null</c>.
        /// </exception>
        IHydroObject Target { get; set; }
    }
}