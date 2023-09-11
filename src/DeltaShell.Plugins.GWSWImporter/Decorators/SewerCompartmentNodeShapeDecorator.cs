using DelftTools.Hydro.SewerFeatures;
using DelftTools.Hydro.Structures;

namespace DeltaShell.Plugins.ImportExport.GWSW.Decorators
{
    /// <summary>
    /// Decorator for setting the compartment shape.
    /// </summary>
    public class SewerCompartmentNodeShapeDecorator : SewerCompartmentDecorator
    {
        public SewerCompartmentNodeShapeDecorator(ACompartment compartment) : base(compartment) {}

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
            
            SetNodeShape(compartment, element);
            
            return aCompartment;
        }

        private void SetNodeShape(ICompartment compartment, GwswElement gwswElement)
        {
            string nodeShapeString = gwswElement.GetAttributeValueFromList<string>(ManholeMapping.PropertyKeys.NodeShape, LogHandler);
            compartment.Shape = CompartmentShapeConverter.ConvertStringToCompartmentShape(nodeShapeString, LogHandler);
        }
    }
}