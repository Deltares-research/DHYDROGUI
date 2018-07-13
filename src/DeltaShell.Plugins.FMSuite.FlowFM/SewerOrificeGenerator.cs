using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.Structures;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Importers;
using GeoAPI.Extensions.Networks;
using log4net;

namespace DeltaShell.Plugins.FMSuite.FlowFM
{
    public class SewerOrificeGenerator : ISewerNetworkFeatureGenerator
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(SewerOrificeGenerator));

        public INetworkFeature Generate(GwswElement gwswElement, IHydroNetwork network, object importHelper = null)
        {
            return gwswElement.IsValidGwswSewerConnection()
                ? CreateOrificeFromGwswSewerConnection(gwswElement, network)
                : CreateOrificeFromGwswStructure(gwswElement, network);
        }

        private static INetworkFeature CreateOrificeFromGwswStructure(GwswElement gwswElement, IHydroNetwork network)
        {
            if (network == null)
            {
                // log
                Log.ErrorFormat($"Orifices cannot be created without a network defined");
                return null;
            }

            var structureNameAttribute = gwswElement.GetAttributeFromList(SewerStructureMapping.PropertyKeys.UniqueId);
            if (!structureNameAttribute.IsValidAttribute()) return null;

            var structureName = structureNameAttribute.ValueAsString;
            var orificeFound = network.BranchFeatures.OfType<Orifice>().FirstOrDefault(bf => bf.Name.Equals(structureName));
            if (orificeFound == null)
            {
                orificeFound = new Orifice(structureName);
                var auxSewerConnection = new SewerConnection(structureName)
                {
                    Network = network,
                };

                network.Branches.Add(auxSewerConnection);
                auxSewerConnection.AddStructureToBranch(orificeFound);
            }

            ExtendOrificeAttributes(orificeFound, gwswElement);
            return orificeFound;
        }
        
        private static INetworkFeature CreateOrificeFromGwswSewerConnection(GwswElement gwswElement, IHydroNetwork network)
        {
            var sewerConnection = (SewerConnection) new SewerConnectionGenerator().Generate(gwswElement, network);
            AddAttributesToSewerConnection(sewerConnection, gwswElement);
            return sewerConnection;
        }

        private static void AddAttributesToSewerConnection(ISewerConnection sewerConnection, GwswElement gwswElement)
        {
            var orifice = FindOrCreateOrifice(sewerConnection);
            
            // Do we need some creation logic here? See SewerPumpGenerator or SewerWeirGenerator
            if (!sewerConnection.BranchFeatures.Contains(orifice))
                sewerConnection.AddStructureToBranch(orifice);
        }

        private static Orifice FindOrCreateOrifice(ISewerConnection sewerConnection)
        {
            var structureFound = sewerConnection.BranchFeatures.OfType<Orifice>().FirstOrDefault(bf => bf.Name.Equals(sewerConnection.Name));
            return structureFound ?? new Orifice(sewerConnection.Name);
        }

        private static void ExtendOrificeAttributes(Orifice orificeFound, GwswElement gwswElement)
        {
            double auxDouble;
            //Add Attributes
            var bottomLevel = gwswElement.GetAttributeFromList(SewerStructureMapping.PropertyKeys.BottomLevel);
            if (bottomLevel.TryGetValueAsDouble(out auxDouble))
                orificeFound.BottomLevel = auxDouble;

            var contractionCoefficient = gwswElement.GetAttributeFromList(SewerStructureMapping.PropertyKeys.ContractionCoefficient);
            if (contractionCoefficient.TryGetValueAsDouble(out auxDouble))
                orificeFound.ContractionCoefficent = auxDouble;

            var maxDischarge = gwswElement.GetAttributeFromList(SewerStructureMapping.PropertyKeys.MaxDischarge);
            if (maxDischarge.TryGetValueAsDouble(out auxDouble))
                orificeFound.MaxDischarge = auxDouble;
        }

    }
}