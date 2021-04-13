using DelftTools.Hydro.SewerFeatures;
using DelftTools.Hydro.Structures;
using log4net;

namespace DeltaShell.Plugins.ImportExport.GWSW.Decorators
{
    /// <summary>
    /// Decorator for setting the compartment storage type.
    /// </summary>
    public class SewerCompartmentStorageTypeDecorator : SewerCompartmentDecorator
    {
        private static ILog Log = LogManager.GetLogger(typeof(SewerCompartmentStorageTypeDecorator));
        
        public SewerCompartmentStorageTypeDecorator(ACompartment compartment) : base(compartment) {}

        public override ACompartment ProcessInput(object gwswElement)
        {
            if (!(gwswElement is GwswElement element) )
            {
                return base.ProcessInput(gwswElement);
            }
            
            ACompartment aCompartment = base.ProcessInput(gwswElement);
            if (!(aCompartment is ICompartment compartment))
            {
                return aCompartment;
            }
            
            SetCompartmentStorageType(compartment, element);
            
            return aCompartment;
        }
        
        private static void SetCompartmentStorageType(ICompartment compartment, GwswElement gwswElement)
        {
            GwswAttribute compartmentStorageTypeAttribute = gwswElement.GetAttributeFromList(ManholeMapping.PropertyKeys.CompartmentStorageType);
            var compartmentStorageType = compartmentStorageTypeAttribute?.GetValueFromDescription<ManholeMapping.GwswCompartmentStorageType>();
            switch (compartmentStorageType)
            {
                case ManholeMapping.GwswCompartmentStorageType.Reservoir:
                    compartment.CompartmentStorageType = CompartmentStorageType.Reservoir;
                    break;
                case ManholeMapping.GwswCompartmentStorageType.Closed:
                    compartment.FloodableArea = 0;
                    compartment.CompartmentStorageType = CompartmentStorageType.Closed;
                    break;
                case ManholeMapping.GwswCompartmentStorageType.Loss:
                    Log.Warn($"Compartment {compartment.Name} has an unsupported compartment storage type 'VRL'. " +
                             $"Setting the default compartment storage type 'RES' instead.");
                    compartment.CompartmentStorageType = CompartmentStorageType.Reservoir;
                    break;
                default:
                    Log.Warn($"Compartment {compartment.Name} has an unsupported compartment storage type. Setting default 'Reservoir'.");
                    compartment.CompartmentStorageType = CompartmentStorageType.Reservoir;
                    break;
            }
        }
    }
}