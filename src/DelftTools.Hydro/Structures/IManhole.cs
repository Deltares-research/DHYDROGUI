using GeoAPI.Extensions.Networks;

namespace DelftTools.Hydro.Structures
{
    public interface IManhole : INode
    {
        Compartment GetCompartmentByName(string compartmentName);
        bool ContainsCompartment(string compartmentName);
    }
}