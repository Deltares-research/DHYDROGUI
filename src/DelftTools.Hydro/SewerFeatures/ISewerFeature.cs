using System;
using System.Collections.Generic;
using DelftTools.Hydro.Structures;

namespace DelftTools.Hydro.SewerFeatures
{
    public class SewerImporterHelper{
        public IDictionary<string, IManhole> ManholesByManholeName { get; } = new Dictionary<string, IManhole>(StringComparer.InvariantCultureIgnoreCase);
        public IDictionary<string, IManhole> ManholesByCompartmentName { get; } = new Dictionary<string, IManhole>(StringComparer.InvariantCultureIgnoreCase);
        public IDictionary<string, ISewerConnection> SewerConnectionsByName { get; } = new Dictionary<string, ISewerConnection>(StringComparer.InvariantCultureIgnoreCase);
    }
    public interface ISewerFeature
    {
        void AddToHydroNetwork(IHydroNetwork network, SewerImporterHelper helper);
    }
}