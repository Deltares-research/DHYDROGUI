using DelftTools.Hydro;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Importers;
using GeoAPI.Extensions.Networks;

namespace DeltaShell.Plugins.FMSuite.FlowFM
{
    public class SewerConnectionOrificeGenerator: SewerConnectionGenerator
    {
        public override INetworkFeature Generate(GwswElement gwswElement, IHydroNetwork network)
        {
            if (gwswElement.IsValidGwswSewerConnection()) return CreateSewerConnection<SewerConnectionOrifice>(gwswElement, network);

            var orifice = CreateSewerConnection<SewerConnectionOrifice>(gwswElement, network, ExtendOrificeAttributes);
            //Because it is read as a structure, it needs to be added in here (if it is not already)
            if (network != null && !network.Branches.Contains(orifice))
                network.Branches.Add(orifice);
            return orifice;
        }

        private void ExtendOrificeAttributes(ISewerConnection element, GwswElement gwswElement, IHydroNetwork network = null)
        {
            var connection = element as SewerConnectionOrifice;
            if (connection == null) return;

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