using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.Structures;
using DelftTools.Hydro.Structures.WeirFormula;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Importers;
using DeltaShell.Plugins.FMSuite.FlowFM.Properties;
using GeoAPI.Extensions.Networks;
using log4net;

namespace DeltaShell.Plugins.FMSuite.FlowFM
{
    public class SewerWeirGenerator : ISewerNetworkFeatureGenerator
    {
        private static ILog Log = LogManager.GetLogger(typeof(SewerWeirGenerator));

        public INetworkFeature Generate(GwswElement gwswElement, IHydroNetwork network, object importHelper = null)
        {
            return gwswElement.IsValidGwswSewerConnection() 
                ? CreateWeirFromGwswSewerConnection(gwswElement, network) 
                : CreateWeirFromGwswStructure(gwswElement, network);
        }

        private static INetworkFeature CreateWeirFromGwswStructure(GwswElement gwswElement, IHydroNetwork network)
        {
            if (network == null)
            {
                Log.ErrorFormat(Resources.SewerWeirGenerator_CreateWeirFromGwswStructure_Weir_s__cannot_be_created_without_a_network_defined_);
                return null;
            }

            var structureNameAttribute = gwswElement.GetAttributeFromList(SewerStructureMapping.PropertyKeys.UniqueId);
            if (!structureNameAttribute.IsValidAttribute()) return null;
            var structureName = structureNameAttribute.ValueAsString;

            var weirFound = network.BranchFeatures.OfType<Weir>()
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

            ExtendWeirAttributes(weirFound, gwswElement);

            return weirFound;
        }

        private static void ExtendWeirAttributes(IWeir weir, GwswElement gwswElement)
        {
            var auxDouble = 0.0;
            //Add Attributes
            var crestWidth = gwswElement.GetAttributeFromList(SewerStructureMapping.PropertyKeys.CrestWidth);
            if (crestWidth.TryGetValueAsDouble(out auxDouble))
                weir.CrestWidth = auxDouble;

            var crestLevel = gwswElement.GetAttributeFromList(SewerStructureMapping.PropertyKeys.CrestLevel);
            if (crestLevel.TryGetValueAsDouble(out auxDouble))
                weir.CrestLevel = auxDouble;

            var dischargeCoefficient = gwswElement.GetAttributeFromList(SewerStructureMapping.PropertyKeys.DischargeCoefficient);
            if (dischargeCoefficient.TryGetValueAsDouble(out auxDouble))
            {
                var weirFormula = weir.WeirFormula as SimpleWeirFormula;
                if(weirFormula != null) weirFormula.DischargeCoefficient = auxDouble;
            }
        }

        private static INetworkFeature CreateWeirFromGwswSewerConnection(GwswElement gwswElement, IHydroNetwork network)
        {
            var sewerConnection = (SewerConnection)new SewerConnectionGenerator().Generate(gwswElement, network);
            AddAttributesToSewerConnection(sewerConnection, gwswElement);
            return sewerConnection;
        }

        private static void AddAttributesToSewerConnection(ISewerConnection sewerConnection, GwswElement gwswElement)
        {
            var sewerWeir = FindOrCreateWeir(sewerConnection);

            //Add Attributes
            var flowDirection = gwswElement.GetAttributeFromList(SewerConnectionMapping.PropertyKeys.FlowDirection);
            if (flowDirection.IsValidAttribute())
            {
                var directionValue = flowDirection.GetValueFromDescription<SewerConnectionMapping.FlowDirection>();
                switch (directionValue)
                {
                    case SewerConnectionMapping.FlowDirection.Open:
                        sewerWeir.FlowDirection = FlowDirection.Both;
                        break;
                    case SewerConnectionMapping.FlowDirection.Closed:
                        sewerWeir.FlowDirection = FlowDirection.None;
                        break;
                    case SewerConnectionMapping.FlowDirection.FromStartToEnd:
                        sewerWeir.FlowDirection = FlowDirection.Positive;
                        break;
                    case SewerConnectionMapping.FlowDirection.FromEndToStart:
                        sewerWeir.FlowDirection = FlowDirection.Negative;
                        break;
                }
            }

            if (!sewerConnection.BranchFeatures.Contains(sewerWeir))
                sewerConnection.AddStructureToBranch(sewerWeir);
        }

        private static Weir FindOrCreateWeir(ISewerConnection sewerConnection)
        {
            var structureFound = sewerConnection.BranchFeatures.OfType<Weir>().FirstOrDefault(bf => bf.Name.Equals(sewerConnection.Name));
            return structureFound ?? new Weir(sewerConnection.Name);
        }
    }
}
