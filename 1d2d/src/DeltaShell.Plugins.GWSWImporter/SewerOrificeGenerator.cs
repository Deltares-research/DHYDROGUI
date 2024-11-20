using DelftTools.Hydro.SewerFeatures;
using DelftTools.Hydro.Structures.WeirFormula;
using Deltares.Infrastructure.API.Logging;
using DeltaShell.Plugins.ImportExport.GWSW.SewerFeatures;

namespace DeltaShell.Plugins.ImportExport.GWSW
{
    public class SewerOrificeGenerator : ASewerGenerator, IGwswFeatureGenerator<ISewerFeature>
    {
        public SewerOrificeGenerator(ILogHandler logHandler)
            : base(logHandler)
        {
        }
        public ISewerFeature Generate(GwswElement gwswElement)
        {
            var orifice = CreateNewOrifice(gwswElement);
            return orifice;
        }

        private Orifice CreateNewOrifice(GwswElement gwswElement)
        {
            var uniqueIdKey = gwswElement.IsValidGwswSewerConnection(logHandler) 
                ? SewerConnectionMapping.PropertyKeys.UniqueId 
                : SewerStructureMapping.PropertyKeys.UniqueId;

            string orificeId = gwswElement.GetAttributeValueFromList<string>(uniqueIdKey, logHandler);

            if (gwswElement.IsValidGwswSewerConnection(logHandler))
            {
                var gwswConnectionOrifice = new GwswConnectionOrifice(orificeId);
                AddSewerConnectionAttributesToOrifice(gwswConnectionOrifice, gwswElement);
                return gwswConnectionOrifice;
            }

            var gwswStructureOrifice = new Orifice(orificeId);
            AddStructureAttributesToOrifice(gwswStructureOrifice, gwswElement);
            return gwswStructureOrifice;
        }

        private void AddSewerConnectionAttributesToOrifice(GwswConnectionOrifice orifice, GwswElement gwswElement)
        {
            if (!gwswElement.IsValidGwswSewerConnection(logHandler)) return;

            orifice.SourceCompartmentName = gwswElement.GetAttributeValueFromList<string>(SewerConnectionMapping.PropertyKeys.SourceCompartmentId, logHandler);
            orifice.TargetCompartmentName = gwswElement.GetAttributeValueFromList<string>(SewerConnectionMapping.PropertyKeys.TargetCompartmentId, logHandler);

            double auxDouble;

            var levelStart = gwswElement.GetAttributeFromList(SewerConnectionMapping.PropertyKeys.LevelStart, logHandler);
            if (levelStart.TryGetValueAsDouble(logHandler, out auxDouble))
                orifice.LevelSource = auxDouble;

            var levelEnd = gwswElement.GetAttributeFromList(SewerConnectionMapping.PropertyKeys.LevelEnd, logHandler);
            if (levelEnd.TryGetValueAsDouble(logHandler, out auxDouble))
                orifice.LevelTarget = auxDouble;

            var length = gwswElement.GetAttributeFromList(SewerConnectionMapping.PropertyKeys.Length, logHandler);
            if (length.TryGetValueAsDouble(logHandler, out auxDouble))
                orifice.Length = auxDouble;
            
            var waterTypeString = gwswElement.GetAttributeValueFromList<string>(SewerConnectionMapping.PropertyKeys.WaterType, logHandler);
            if (waterTypeString != null)
            {
                orifice.WaterType = WaterTypeConverter.ConvertStringToSewerConnectionWaterType(waterTypeString, logHandler);
            }
            
            var flowDirection = gwswElement.GetAttributeFromList(SewerConnectionMapping.PropertyKeys.FlowDirection, logHandler);
            if (flowDirection.IsValidAttribute(logHandler))
            {
                var flowDirectionValue = flowDirection.GetValueFromDescription<SewerConnectionMapping.FlowDirection>(logHandler);
                switch (flowDirectionValue)
                {
                    case SewerConnectionMapping.FlowDirection.Closed://this is also the default....
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
                }
            }
            
            orifice.CrossSectionDefinitionName = gwswElement.GetAttributeValueFromList<string>(SewerConnectionMapping.PropertyKeys.CrossSectionDefinitionId, logHandler);
        }

        private void AddStructureAttributesToOrifice(IOrifice orifice, GwswElement gwswElement)
        {
            var gatedWeirFormula = orifice.WeirFormula as GatedWeirFormula;
            if (!gwswElement.IsValidGwswStructure(logHandler) || gatedWeirFormula == null) return;
            
            double auxDouble;
            //Add Attributes
            var bottomLevel = gwswElement.GetAttributeFromList(SewerStructureMapping.PropertyKeys.BottomLevel, logHandler);
            if (bottomLevel.TryGetValueAsDouble(logHandler, out auxDouble))
                orifice.CrestLevel = auxDouble;

            var contractionCoefficient = gwswElement.GetAttributeFromList(SewerStructureMapping.PropertyKeys.ContractionCoefficient, logHandler);
            if (contractionCoefficient.TryGetValueAsDouble(logHandler, out auxDouble))
                gatedWeirFormula.ContractionCoefficient = auxDouble;

            var maxDischarge = gwswElement.GetAttributeFromList(SewerStructureMapping.PropertyKeys.MaxDischarge, logHandler);
            
            if (maxDischarge.TryGetValueAsDouble(logHandler, out auxDouble))
            {
                gatedWeirFormula.UseMaxFlowPos = true;
                gatedWeirFormula.UseMaxFlowNeg = true;
                gatedWeirFormula.MaxFlowNeg = auxDouble;
                gatedWeirFormula.MaxFlowPos = auxDouble;
                orifice.MaxDischarge = auxDouble;
            }
        }
    }
}