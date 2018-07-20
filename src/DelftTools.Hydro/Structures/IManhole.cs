using DelftTools.Utils.Collections.Generic;
using GeoAPI.Extensions.Networks;


namespace DelftTools.Hydro.Structures
{
    public interface IManhole : INode, IHydroNetworkFeature
    {
        IEventedList<Compartment> Compartments { get; set; }
        Compartment GetCompartmentByName(string compartmentName);
        bool ContainsCompartmentWithName(string compartmentName);
    }
}