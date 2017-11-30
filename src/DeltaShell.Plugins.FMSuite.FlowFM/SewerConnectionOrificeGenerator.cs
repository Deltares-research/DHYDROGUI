using DelftTools.Hydro;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Importers;
using GeoAPI.Extensions.Networks;

namespace DeltaShell.Plugins.FMSuite.FlowFM
{
    public class SewerConnectionOrificeGenerator: SewerConnectionGenerator, ISewerNetworkFeatureGenerator
    {
        public new INetworkFeature Generate(GwswElement gwswElement, IHydroNetwork network)
        {
            if (gwswElement.IsValidGwswSewerConnection()) return CreateSewerConnection<SewerConnectionOrifice>(gwswElement, network);
            return CreateOrificeFromGwswStructure(gwswElement, network);
        }

        private INetworkFeature CreateOrificeFromGwswStructure(GwswElement gwswElement, IHydroNetwork network)
        {
            if (network == null)
                return null;

            var orifice = FindOrGetNewConnection<SewerConnectionOrifice>(gwswElement, network);
            ExtendOrificeAttributes(gwswElement, orifice);

            //Because it is read as a structure, it needs to be added in here.
            network.Branches.Add(orifice);

            return orifice;
        }

        private void ExtendOrificeAttributes(GwswElement gwswElement, SewerConnectionOrifice connection)
        {
            var auxDouble = 0.0;
            //Add Attributes
            var bottomLevel = gwswElement.GetAttributeFromList(SewerStructureMapping.PropertyKeys.BottomLevel);
            if( bottomLevel.TryGetValueAsDouble(out auxDouble))
                connection.Bottom_Level = auxDouble;

            var contractionCoefficient = gwswElement.GetAttributeFromList(SewerStructureMapping.PropertyKeys.ContractionCoefficient);
            if (contractionCoefficient.TryGetValueAsDouble(out auxDouble))
                connection.Contraction_Coefficent = auxDouble;

            var maxDischarge = gwswElement.GetAttributeFromList(SewerStructureMapping.PropertyKeys.MaxDischarge);
            if (maxDischarge.TryGetValueAsDouble(out auxDouble))
                connection.Max_Discharge = auxDouble;
        }
    }
}