using System.Collections.Generic;

namespace DelftTools.Hydro.Structures.SteerableProperties
{
    /// <summary>
    /// <see cref="IHasSteerableProperties"/> defines whether an object has any
    /// <see cref="SteerableProperty"/> properties.
    /// </summary>
    public interface IHasSteerableProperties
    {
        /// <summary>
        /// Retrieves all <see cref="SteerableProperty"/> properties of this object.
        /// </summary>
        /// <returns>
        /// A collection of <see cref="SteerableProperty"/> properties of this object.
        /// </returns>
        IEnumerable<SteerableProperty> RetrieveSteerableProperties();
    }
}