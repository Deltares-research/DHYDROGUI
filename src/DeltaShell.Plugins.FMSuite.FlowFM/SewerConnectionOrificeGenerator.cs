using System.Linq;
using DelftTools.Hydro;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Importers;
using GeoAPI.Extensions.Networks;

namespace DeltaShell.Plugins.FMSuite.FlowFM
{
    public class SewerConnectionOrificeGenerator: SewerConnectionGenerator, ISewerNetworkFeatureGenerator
    {
        public new INetworkFeature Generate(GwswElement gwswElement, IHydroNetwork network)
        {
            if (IsValidGwswSewerConnection(gwswElement)) return CreateSewerConnection<SewerConnectionOrifice>(gwswElement, network);
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
            //Add Attributes
            var newDoubleValue = 0.0;
            var bottomLevel = GetAttributeFromList(gwswElement, SewerStructureMapping.PropertyKeys.BottomLevel);
            if (bottomLevel != null && bottomLevel.ValueAsString != string.Empty)
            {
                var valueType = bottomLevel.GwswAttributeType.AttributeType;
                if (valueType == connection.Bottom_Level.GetType() &&
                    TryParseDoubleElseLogError(bottomLevel, valueType, out newDoubleValue))
                {
                    connection.Bottom_Level = newDoubleValue;
                }
            }

            var contractionCoefficient = GetAttributeFromList(gwswElement, SewerStructureMapping.PropertyKeys.ContractionCoefficient);
            if (contractionCoefficient != null && contractionCoefficient.ValueAsString != string.Empty)
            {
                var valueType = contractionCoefficient.GwswAttributeType.AttributeType;
                if (valueType == connection.Contraction_Coefficent.GetType() &&
                    TryParseDoubleElseLogError(contractionCoefficient, valueType, out newDoubleValue))
                {
                    connection.Contraction_Coefficent = newDoubleValue;
                }
            }

            var maxDischarge = GetAttributeFromList(gwswElement, SewerStructureMapping.PropertyKeys.MaxDischarge);
            if (maxDischarge != null && maxDischarge.ValueAsString != string.Empty)
            {
                var valueType = maxDischarge.GwswAttributeType.AttributeType;
                if (valueType == connection.Max_Discharge.GetType() &&
                    TryParseDoubleElseLogError(maxDischarge, valueType, out newDoubleValue))
                {
                    connection.Max_Discharge = newDoubleValue;
                }
            }
        }
    }
}