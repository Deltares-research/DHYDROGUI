using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.Structures;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Importers;
using GeoAPI.Extensions.Networks;

namespace DeltaShell.Plugins.FMSuite.FlowFM
{
    public class SewerWeirGenerator : ISewerNetworkFeatureGenerator
    {
        public INetworkFeature Generate(GwswElement gwswElement, IHydroNetwork network)
        {
            if (gwswElement.IsValidGwswSewerConnection()) return CreateWeirFromGwswSewerConnection(gwswElement, network);
            return CreateWeirFromGwswStructure(gwswElement, network);
        }

        private INetworkFeature CreateWeirFromGwswStructure(GwswElement gwswElement, IHydroNetwork network)
        {
            var structureNameAttribute = gwswElement.GetAttributeFromList(SewerStructureMapping.PropertyKeys.UniqueId);
            if (!structureNameAttribute.IsValidAttribute()) return null;
            var structureName = structureNameAttribute.ValueAsString;

            var weirFound = network.BranchFeatures.OfType<IWeir>()
                .FirstOrDefault(p => p.Name.Equals(structureName));
            if (weirFound == null)
            {
                weirFound = new Weir(structureName);
                var auxSewerConnection = new SewerConnection(structureName)
                {
                    Network = network
                };
                network.Branches.Add(auxSewerConnection);
                auxSewerConnection.AddStructureToBranch(weirFound);
            }

            return weirFound;
        }

        private INetworkFeature CreateWeirFromGwswSewerConnection(GwswElement gwswElement, IHydroNetwork network)
        {
            var sewerConnection = (SewerConnection)new SewerConnectionGenerator().Generate(gwswElement, network);
            AddAttributesToSewerConnection(sewerConnection, gwswElement);
            return sewerConnection;
        }

        private void AddAttributesToSewerConnection(ISewerConnection sewerConnection, GwswElement gwswElement)
        {
            var sewerWeir = new Weir();
            sewerConnection.AddStructureToBranch(sewerWeir);
        }
    }
}
