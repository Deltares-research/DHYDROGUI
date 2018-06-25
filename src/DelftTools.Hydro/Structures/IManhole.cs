using DelftTools.Utils.Collections.Generic;
using GeoAPI.Extensions.Feature;
using GeoAPI.Extensions.Networks;


namespace DelftTools.Hydro.Structures
{
    public interface IManhole : INode
    {
        IEventedList<Compartment> Compartments { get; set; }
        Compartment GetCompartmentByName(string compartmentName);
        bool ContainsCompartmentWithName(string compartmentName);
    }
}