using DelftTools.Hydro.SewerFeatures;
using DelftTools.Utils;

namespace DelftTools.Hydro.Structures
{
    public interface ICompartment : ISewerFeature, INameable
    {
        string ParentManholeName { get; set; }
    }
}
