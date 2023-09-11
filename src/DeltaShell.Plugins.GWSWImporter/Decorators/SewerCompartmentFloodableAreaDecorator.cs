using DelftTools.Hydro.SewerFeatures;
using DelftTools.Hydro.Structures;

namespace DeltaShell.Plugins.ImportExport.GWSW.Decorators
{
    /// <summary>
    /// Decorator for setting the compartment floodable area.
    /// </summary>
    public class SewerCompartmentFloodableAreaDecorator : SewerCompartmentDecorator
    {
        public SewerCompartmentFloodableAreaDecorator(ACompartment compartment) : base(compartment) {}

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
            
            SetFloodableArea(compartment, element);

            return aCompartment;
        }
        
        private void SetFloodableArea(ICompartment compartment, GwswElement gwswElement)
        {
            double defaultValue = 0;
            if (compartment.CompartmentStorageType == CompartmentStorageType.Reservoir)
            {
                defaultValue = Compartment.DefaultReservoirFloodableArea;
            }
            
            var logMessage = $"Missing floodable area value for '{compartment.Name}', using default value: {defaultValue}";
            
            double floodableArea = gwswElement.GetAttributeValueFromList<double>(ManholeMapping.PropertyKeys.FloodableArea, LogHandler, defaultValue, logMessage);
            
            compartment.FloodableArea = floodableArea;
        }
    }
}