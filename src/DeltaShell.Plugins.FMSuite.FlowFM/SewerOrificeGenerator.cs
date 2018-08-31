using DelftTools.Hydro;
using DelftTools.Hydro.SewerFeatures;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Importers;

namespace DeltaShell.Plugins.FMSuite.FlowFM
{
    public class SewerOrificeGenerator : ISewerFeatureGenerator
    {
        public ISewerFeature Generate(GwswElement gwswElement)
        {
            var orifice = CreateNewOrifice(gwswElement);
            return orifice;
        }

        private static Orifice CreateNewOrifice(GwswElement gwswElement)
        {
            var uniqueIdKey = gwswElement.IsValidGwswSewerConnection() 
                ? SewerConnectionMapping.PropertyKeys.UniqueId 
                : SewerStructureMapping.PropertyKeys.UniqueId;

            var orificeIdAttribute = gwswElement.GetAttributeFromList(uniqueIdKey);
            var orificeId = orificeIdAttribute.GetValidStringValue();

            if (gwswElement.IsValidGwswSewerConnection())
            {
                var gwswConnectionOrifice = new GwswConnectionOrifice(orificeId);
                AddSewerConnectionAttributesToOrifice(gwswConnectionOrifice, gwswElement);
                return gwswConnectionOrifice;
            }

            var gwswStructureOrifice = new Orifice(orificeId);
            AddStructureAttributesToOrifice(gwswStructureOrifice, gwswElement);
            return gwswStructureOrifice;
        }

        private static void AddSewerConnectionAttributesToOrifice(GwswConnectionOrifice orifice, GwswElement gwswElement)
        {
            if (!gwswElement.IsValidGwswSewerConnection()) return;

            var nodeIdStartAttribute = gwswElement.GetAttributeFromList(SewerConnectionMapping.PropertyKeys.SourceCompartmentId);
            orifice.SourceCompartmentName = nodeIdStartAttribute.GetValidStringValue();

            var nodeIdEndAttribute = gwswElement.GetAttributeFromList(SewerConnectionMapping.PropertyKeys.TargetCompartmentId);
            orifice.TargetCompartmentName = nodeIdEndAttribute.GetValidStringValue();

            double auxDouble;

            var levelStart = gwswElement.GetAttributeFromList(SewerConnectionMapping.PropertyKeys.LevelStart);
            if (levelStart.TryGetValueAsDouble(out auxDouble))
                orifice.LevelSource = auxDouble;

            var levelEnd = gwswElement.GetAttributeFromList(SewerConnectionMapping.PropertyKeys.LevelEnd);
            if (levelEnd.TryGetValueAsDouble(out auxDouble))
                orifice.LevelTarget = auxDouble;

            var length = gwswElement.GetAttributeFromList(SewerConnectionMapping.PropertyKeys.Length);
            if (length.TryGetValueAsDouble(out auxDouble))
                orifice.Length = auxDouble;

            var waterType = gwswElement.GetAttributeFromList(SewerConnectionMapping.PropertyKeys.WaterType);
            if (waterType.IsValidAttribute())
                orifice.WaterType = waterType.GetValueFromDescription<SewerConnectionWaterType>();
        }

        private static void AddStructureAttributesToOrifice(IOrifice orifice, GwswElement gwswElement)
        {
            if (!gwswElement.IsValidGwswStructure()) return;

            double auxDouble;
            //Add Attributes
            var bottomLevel = gwswElement.GetAttributeFromList(SewerStructureMapping.PropertyKeys.BottomLevel);
            if (bottomLevel.TryGetValueAsDouble(out auxDouble))
                orifice.BottomLevel = auxDouble;

            var contractionCoefficient = gwswElement.GetAttributeFromList(SewerStructureMapping.PropertyKeys.ContractionCoefficient);
            if (contractionCoefficient.TryGetValueAsDouble(out auxDouble))
                orifice.ContractionCoefficent = auxDouble;

            var maxDischarge = gwswElement.GetAttributeFromList(SewerStructureMapping.PropertyKeys.MaxDischarge);
            if (maxDischarge.TryGetValueAsDouble(out auxDouble))
                orifice.MaxDischarge = auxDouble;
        }

    }
}