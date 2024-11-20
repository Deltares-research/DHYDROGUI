using DelftTools.Hydro.SewerFeatures;
using DelftTools.Hydro.Structures;

namespace DeltaShell.Plugins.ImportExport.GWSW.Decorators
{
    /// <summary>
    /// Decorator for setting the compartment surface level.
    /// </summary>
    public class SewerCompartmentSurfaceLevelDecorator : SewerCompartmentDecorator
    {
        public SewerCompartmentSurfaceLevelDecorator(ACompartment compartment) : base(compartment) {}

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
            
            SetSurfaceLevel(compartment, element);
            
            return aCompartment;
        }
        
        private void SetSurfaceLevel(ICompartment compartment, GwswElement gwswElement)
        {
            double defaultSurfaceLevel = compartment.SurfaceLevel;
            string logMessage = $"Missing surface level value for '{compartment.Name}', using default value: {defaultSurfaceLevel}";
            
            double surfaceLevel = gwswElement.GetAttributeValueFromList<double>(ManholeMapping.PropertyKeys.SurfaceLevel, LogHandler, defaultSurfaceLevel, logMessage);
            
            compartment.SurfaceLevel = surfaceLevel;
        }
    }
}