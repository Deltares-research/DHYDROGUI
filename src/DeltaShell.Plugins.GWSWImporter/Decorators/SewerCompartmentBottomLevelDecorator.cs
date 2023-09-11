using DelftTools.Hydro.SewerFeatures;
using DelftTools.Hydro.Structures;

namespace DeltaShell.Plugins.ImportExport.GWSW.Decorators
{
    /// <summary>
    /// Decorator for setting the compartment bottom level.
    /// </summary>
    public class SewerCompartmentBottomLevelDecorator : SewerCompartmentDecorator
    {
        public SewerCompartmentBottomLevelDecorator(ACompartment compartment) : base(compartment) {}

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
            
            SetBottomLevel(compartment, element);
            
            return aCompartment;
        }
        
        private void SetBottomLevel(ICompartment compartment, GwswElement gwswElement)
        {
            double defaultValue = compartment.BottomLevel;
            var logMessage = $"Missing bottom level value for '{compartment.Name}', using default value: {defaultValue}";
            
            double bottomLevel = gwswElement.GetAttributeValueFromList<double>(ManholeMapping.PropertyKeys.BottomLevel, LogHandler, defaultValue, logMessage);
            
            compartment.BottomLevel = bottomLevel;
        }
    }
}