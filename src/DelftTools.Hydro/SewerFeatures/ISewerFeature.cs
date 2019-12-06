using System.Collections.Generic;
using DelftTools.Hydro.Structures;

namespace DelftTools.Hydro.SewerFeatures
{
    public class SewerImporterHelper{
        public IDictionary<string, IManhole> ManholesByManholeName { get; } = new Dictionary<string, IManhole>();
        public IDictionary<string, IManhole> ManholesByCompartmentName { get; } = new Dictionary<string, IManhole>();
    }
    public interface ISewerFeature
    {
        void AddToHydroNetwork(IHydroNetwork network, SewerImporterHelper helper);
    }
}