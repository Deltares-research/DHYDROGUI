using DelftTools.Utils.Collections.Generic;
using GeoAPI.Extensions.Networks;

namespace DelftTools.Hydro.Structures
{
    public interface IManhole : INode
    {
        IEventedList<Compartment> Compartments { get; set; }
        Compartment GetCompartmentByName(string compartmentName);
        bool ContainsCompartment(string compartmentName);
    }
}