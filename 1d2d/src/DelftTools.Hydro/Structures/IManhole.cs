using DelftTools.Utils.Collections.Generic;
using GeoAPI.Extensions.Networks;

namespace DelftTools.Hydro.Structures
{
    public interface IManhole : INode, IHydroNetworkFeature, ICompositeNetworkPointFeature
    {
        /// <summary>
        /// Compartments inside the manhole
        /// </summary>
        IEventedList<ICompartment> Compartments { get; }

        /// <summary>
        /// Gets compartment by name
        /// </summary>
        /// <param name="compartmentName">Name to search for</param>
        /// <returns></returns>
        ICompartment GetCompartmentByName(string compartmentName);

        /// <summary>
        /// Check if a compartment name is present in the compartments
        /// </summary>
        /// <param name="compartmentName"></param>
        /// <returns></returns>
        bool ContainsCompartmentWithName(string compartmentName);
    }
}