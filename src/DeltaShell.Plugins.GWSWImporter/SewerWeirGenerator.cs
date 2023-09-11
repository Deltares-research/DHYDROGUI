using DelftTools.Hydro;
using DelftTools.Hydro.SewerFeatures;
using DelftTools.Hydro.Structures;
using DelftTools.Hydro.Structures.WeirFormula;
using DeltaShell.Plugins.ImportExport.GWSW.SewerFeatures;
using DHYDRO.Common.Logging;

namespace DeltaShell.Plugins.ImportExport.GWSW
{
    public class SewerWeirGenerator : ASewerGenerator, IGwswFeatureGenerator<ISewerFeature>
    {
        public SewerWeirGenerator(ILogHandler logHandler)
            : base(logHandler)
        {
        }
        public ISewerFeature Generate(GwswElement gwswElement)
        {
            var weir = CreateNewWeir(gwswElement);
            return weir;
        }

        private Weir CreateNewWeir(GwswElement gwswElement)
        {
            var uniqueIdKey = gwswElement.IsValidGwswSewerConnection(logHandler)
                ? SewerConnectionMapping.PropertyKeys.UniqueId
                : SewerStructureMapping.PropertyKeys.UniqueId;
            
            var weirId = gwswElement.GetAttributeValueFromList<string>(uniqueIdKey, logHandler);

            if (gwswElement.IsValidGwswSewerConnection(logHandler))
            {
                var gwswConnectionWeir = new GwswConnectionWeir(weirId);
                AddConnectionAttributesToWeir(gwswConnectionWeir, gwswElement);
                return gwswConnectionWeir;
            }

            var gwswStructureWeir = new GwswStructureWeir(weirId);
            AddStructureAttributesToWeir(gwswStructureWeir, gwswElement);
            return gwswStructureWeir;
        }
        
        private void AddStructureAttributesToWeir(IWeir weir, GwswElement gwswElement)
        {
            double auxDouble;
            //Add Attributes
            var crestWidth = gwswElement.GetAttributeFromList(SewerStructureMapping.PropertyKeys.CrestWidth, logHandler);
            if (crestWidth.TryGetValueAsDouble(logHandler, out auxDouble))
                weir.CrestWidth = auxDouble;

            var crestLevel = gwswElement.GetAttributeFromList(SewerStructureMapping.PropertyKeys.CrestLevel, logHandler);
            if (crestLevel.TryGetValueAsDouble(logHandler, out auxDouble))
                weir.CrestLevel = auxDouble;

            var dischargeCoefficient = gwswElement.GetAttributeFromList(SewerStructureMapping.PropertyKeys.DischargeCoefficient, logHandler);
            if (dischargeCoefficient.TryGetValueAsDouble(logHandler, out auxDouble))
            {
                var weirFormula = weir.WeirFormula as SimpleWeirFormula;
                if(weirFormula != null) weirFormula.CorrectionCoefficient = auxDouble;
            }
        }
        
        private void AddConnectionAttributesToWeir(GwswConnectionWeir weir, GwswElement gwswElement)
        {
            weir.SourceCompartmentName = gwswElement.GetAttributeValueFromList<string>(SewerConnectionMapping.PropertyKeys.SourceCompartmentId, logHandler);
            weir.TargetCompartmentName = gwswElement.GetAttributeValueFromList<string>(SewerConnectionMapping.PropertyKeys.TargetCompartmentId, logHandler);

            double auxDouble;
            var length = gwswElement.GetAttributeFromList(SewerConnectionMapping.PropertyKeys.Length, logHandler);
            if (length.TryGetValueAsDouble(logHandler, out auxDouble))
                weir.Length = auxDouble;

            var flowDirection = gwswElement.GetAttributeFromList(SewerConnectionMapping.PropertyKeys.FlowDirection, logHandler);
            if (flowDirection.IsValidAttribute(logHandler))
            {
                var directionValue = flowDirection.GetValueFromDescription<SewerConnectionMapping.FlowDirection>(logHandler);
                switch (directionValue)
                {
                    case SewerConnectionMapping.FlowDirection.Open:
                        weir.FlowDirection = FlowDirection.Both;
                        break;
                    case SewerConnectionMapping.FlowDirection.Closed:
                        weir.FlowDirection = FlowDirection.None;
                        break;
                    case SewerConnectionMapping.FlowDirection.FromStartToEnd:
                        weir.FlowDirection = FlowDirection.Positive;
                        break;
                    case SewerConnectionMapping.FlowDirection.FromEndToStart:
                        weir.FlowDirection = FlowDirection.Negative;
                        break;
                }
            }
        }
    }
}
