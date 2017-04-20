using System.Collections.Generic;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain;
using SharpMap.Api;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Importers
{
    public class LandUseMapping
    {
        public LandUseMapping()
        {
            Mapping = new Dictionary<object, PolderSubTypes>();
        }

        public bool Use { get; set; }
        public string Column { get; set; }
        public IDictionary<object, PolderSubTypes> Mapping { get; set; }
        public IFeatureProvider LandUseFeatureProvider { get; set; }
    }
}