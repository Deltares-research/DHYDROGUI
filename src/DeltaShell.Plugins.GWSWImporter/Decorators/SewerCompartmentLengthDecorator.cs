using DelftTools.Hydro;
using DelftTools.Hydro.SewerFeatures;
using DelftTools.Hydro.Structures;

namespace DeltaShell.Plugins.ImportExport.GWSW.Decorators
{
    /// <summary>
    /// Decorator for setting the compartment length.
    /// </summary>
    public class SewerCompartmentLengthDecorator : SewerCompartmentDecorator
    {
        public SewerCompartmentLengthDecorator(ACompartment compartment) : base(compartment) {}

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
            
            SetNodeLength(compartment, element);
            
            return aCompartment;
        }
        
        private void SetNodeLength(ICompartment compartment, GwswElement gwswElement)
        {
            double defaultLength = 0.8d * 1000;
            
            string logMessage = null;
            if (compartment.Shape == CompartmentShape.Rectangular)
            {
                logMessage = $"Missing length value for '{compartment.Name}', using default value: {defaultLength}";
            }
            
            double length = gwswElement.GetAttributeValueFromList<double>(ManholeMapping.PropertyKeys.NodeLength, LogHandler, defaultLength, logMessage);
            compartment.ManholeLength = length / 1000.0; // Conversion from mm to m
        }
    }
}