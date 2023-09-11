using DelftTools.Hydro.SewerFeatures;
using DelftTools.Hydro.Structures;
using NetTopologySuite.Geometries;

namespace DeltaShell.Plugins.ImportExport.GWSW.Decorators
{
    /// <summary>
    /// Decorator for setting the compartment geometry.
    /// </summary>
    public class SewerCompartmentGeometryDecorator : SewerCompartmentDecorator
    {
        public SewerCompartmentGeometryDecorator(ACompartment compartment) : base(compartment) { }

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
            
            SetGeometry(compartment, element);
            
            return aCompartment;
        }
        
        private void SetGeometry(ICompartment compartment, GwswElement gwswElement)
        {
            double defaultX = 0;
            string logMessageX = $"Missing xCoordinate value for compartment '{compartment.Name}', using default value: {defaultX}";
            double xCoordinate = gwswElement.GetAttributeValueFromList<double>(ManholeMapping.PropertyKeys.XCoordinate, LogHandler, defaultX, logMessageX);
            
            double defaultY = 0;
            string logMessageY = $"Missing yCoordinate value for compartment '{compartment.Name}', using default value: {defaultY}";
            double yCoordinate = gwswElement.GetAttributeValueFromList<double>(ManholeMapping.PropertyKeys.YCoordinate, LogHandler, defaultY, logMessageY);
            
            compartment.Geometry = new Point(xCoordinate, yCoordinate);
        }
    }
}