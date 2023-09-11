using DelftTools.Hydro.SewerFeatures;

namespace DeltaShell.Plugins.ImportExport.GWSW.Decorators
{
    /// <summary>
    /// Decorator for setting the compartment parent manhole.
    /// </summary>
    public class SewerCompartmentParentManholeDecorator : SewerCompartmentDecorator
    { 
        public SewerCompartmentParentManholeDecorator(ACompartment compartment) : base(compartment) {}

        public override ACompartment ProcessInput(object gwswElement)
        {
            if (!(gwswElement is GwswElement element) )
            {
                return base.ProcessInput(gwswElement);
            }
            
            ACompartment aCompartment = base.ProcessInput(gwswElement);
            if (!(aCompartment is Compartment compartment))
            {
                return aCompartment;
            }
            
            compartment.ParentManholeName = element.GetAttributeValueFromList<string>(ManholeMapping.PropertyKeys.ManholeId, LogHandler);
            
            return aCompartment;
        }
    }
}