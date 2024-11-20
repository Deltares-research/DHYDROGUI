using DelftTools.Hydro.SewerFeatures;
using DelftTools.Hydro.Structures;

namespace DeltaShell.Plugins.ImportExport.GWSW.Decorators
{
    /// <summary>
    /// Decorator for setting the compartment width.
    /// </summary>
    public class SewerCompartmentWidthDecorator : SewerCompartmentDecorator
    {
        public SewerCompartmentWidthDecorator(ACompartment compartment) : base(compartment) {}

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
            
            SetNodeWidth(compartment, element);
            
            return aCompartment;
        }
        
        private void SetNodeWidth(ICompartment compartment, GwswElement gwswElement)
        {
            double defaultWidth = 0.8d * 1000;
            var logMessage = $"Missing width value for '{compartment.Name}', using default value: {defaultWidth}";
            
            double manholeWidth = gwswElement.GetAttributeValueFromList<double>(ManholeMapping.PropertyKeys.NodeWidth, LogHandler, defaultWidth, logMessage);
            
            compartment.ManholeWidth = manholeWidth / 1000;
        }
    }
}