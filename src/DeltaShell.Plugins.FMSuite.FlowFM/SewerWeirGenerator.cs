using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.Structures;
using DelftTools.Hydro.Structures.WeirFormula;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Importers;
using log4net;

namespace DeltaShell.Plugins.FMSuite.FlowFM
{
    public class SewerWeirGenerator : ISewerNetworkFeatureGenerator
    {
        private static ILog Log = LogManager.GetLogger(typeof(SewerWeirGenerator));

        public ISewerFeature Generate(GwswElement gwswElement)
        {
            var weir = CreateNewWeir(gwswElement);
            return weir;
        }

        private Weir CreateNewWeir(GwswElement gwswElement)
        {
            var uniqueIdKey = gwswElement.IsValidGwswSewerConnection()
                ? SewerConnectionMapping.PropertyKeys.UniqueId
                : SewerStructureMapping.PropertyKeys.UniqueId;

            var weirIdAttribute = gwswElement.GetAttributeFromList(uniqueIdKey);
            var weirId = weirIdAttribute.GetValidStringValue();

            if (gwswElement.IsValidGwswSewerConnection())
            {
                var gwswConnectionWeir = new GwswConnectionWeir(weirId);
                AddConnectionAttributesToWeir(gwswConnectionWeir, gwswElement);
                return gwswConnectionWeir;
            }

            var gwswStructureWeir = new GwswStructureWeir(weirId);
            AddStructureAttributesToWeir(gwswStructureWeir, gwswElement);
            return gwswStructureWeir;
        }
        
        private static void AddStructureAttributesToWeir(IWeir weir, GwswElement gwswElement)
        {
            double auxDouble;
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
        
        private static void AddConnectionAttributesToWeir(GwswConnectionWeir weir, GwswElement gwswElement)
        {
            var nodeIdStartAttribute = gwswElement.GetAttributeFromList(SewerConnectionMapping.PropertyKeys.SourceCompartmentId);
            weir.SourceCompartmentId = nodeIdStartAttribute.GetValidStringValue();

            var nodeIdEndAttribute = gwswElement.GetAttributeFromList(SewerConnectionMapping.PropertyKeys.TargetCompartmentId);
            weir.TargetCompartmentId = nodeIdEndAttribute.GetValidStringValue();

            var flowDirection = gwswElement.GetAttributeFromList(SewerConnectionMapping.PropertyKeys.FlowDirection);
            if (flowDirection.IsValidAttribute())
            {
                var directionValue = flowDirection.GetValueFromDescription<SewerConnectionMapping.FlowDirection>();
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

        private static Weir FindOrCreateWeir(ISewerConnection sewerConnection)
        {
            var structureFound = sewerConnection.BranchFeatures.OfType<Weir>().FirstOrDefault(bf => bf.Name.Equals(sewerConnection.Name));
            return structureFound ?? new Weir(sewerConnection.Name);
        }
    }
}
