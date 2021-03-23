using System;
using DelftTools.Hydro.SewerFeatures;
using DelftTools.Hydro.Structures.WeirFormula;
using DeltaShell.Plugins.ImportExport.GWSW.SewerFeatures;

namespace DeltaShell.Plugins.ImportExport.GWSW
{
    public class SewerOrificeGenerator : IGwswFeatureGenerator<ISewerFeature>
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
                orifice.WaterType = WaterTypeConverter.ConvertStringToSewerConnectionWaterType(waterType.GetValidStringValue());

            var flowDirection = gwswElement.GetAttributeFromList(SewerConnectionMapping.PropertyKeys.FlowDirection);
            if (flowDirection.IsValidAttribute())
            {
                var flowDirectionValue = flowDirection.GetValueFromDescription<SewerConnectionMapping.FlowDirection>();
                switch (flowDirectionValue)
                {
                    case SewerConnectionMapping.FlowDirection.Closed:
                        orifice.AllowNegativeFlow = false;
                        orifice.AllowPositiveFlow = false;
                        break;
                    case SewerConnectionMapping.FlowDirection.Open:
                        orifice.AllowNegativeFlow = true;
                        orifice.AllowPositiveFlow = true;
                        break;
                    case SewerConnectionMapping.FlowDirection.FromStartToEnd:
                        orifice.AllowNegativeFlow = false;
                        orifice.AllowPositiveFlow = true;
                        break;
                    case SewerConnectionMapping.FlowDirection.FromEndToStart:
                        orifice.AllowNegativeFlow = true;
                        orifice.AllowPositiveFlow = false;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException($"{flowDirectionValue} is not a valid flow direction.");
                }
            }
        }

        private static void AddStructureAttributesToOrifice(IOrifice orifice, GwswElement gwswElement)
        {
            var gatedWeirFormula = orifice.WeirFormula as GatedWeirFormula;
            if (!gwswElement.IsValidGwswStructure() || gatedWeirFormula == null) return;
            
            double auxDouble;
            //Add Attributes
            var bottomLevel = gwswElement.GetAttributeFromList(SewerStructureMapping.PropertyKeys.BottomLevel);
            if (bottomLevel.TryGetValueAsDouble(out auxDouble))
                orifice.CrestLevel = auxDouble;

            var contractionCoefficient = gwswElement.GetAttributeFromList(SewerStructureMapping.PropertyKeys.ContractionCoefficient);
            if (contractionCoefficient.TryGetValueAsDouble(out auxDouble))
                gatedWeirFormula.ContractionCoefficient = auxDouble;

            var maxDischarge = gwswElement.GetAttributeFromList(SewerStructureMapping.PropertyKeys.MaxDischarge);
            
            if (maxDischarge.TryGetValueAsDouble(out auxDouble))
            {
                gatedWeirFormula.MaxFlowNeg = auxDouble;
                gatedWeirFormula.MaxFlowPos = auxDouble;
                orifice.MaxDischarge = auxDouble;
            }
        }
    }
}