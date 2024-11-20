using DelftTools.Hydro.SewerFeatures;
using DeltaShell.Plugins.ImportExport.GWSW.Decorators;

namespace DeltaShell.Plugins.ImportExport.GWSW
{
    /// <summary>
    /// Factory for setting sewer compartment decorators.
    /// </summary>
    public class SewerCompartmentDecoratorFactory
    {
        /// <summary>
        /// Sets decorators for sewer compartments.
        /// </summary>
        /// <param name="compartment">The compartment to decorate.</param>
        /// <returns>The decorated compartment.</returns>
        public ACompartment SetDecorators(ACompartment compartment)
        {
            compartment = new SewerCompartmentParentManholeDecorator(compartment);
            compartment = new SewerCompartmentGeometryDecorator(compartment);
            compartment = new SewerCompartmentNodeShapeDecorator(compartment);
            compartment = new SewerCompartmentWidthDecorator(compartment);
            compartment = new SewerCompartmentLengthDecorator(compartment); // required if NodeShape is 'Rectangular'
            compartment = new SewerCompartmentBottomLevelDecorator(compartment);
            compartment = new SewerCompartmentSurfaceLevelDecorator(compartment);
            compartment = new SewerCompartmentStorageTypeDecorator(compartment);
            compartment = new SewerCompartmentFloodableAreaDecorator(compartment); // required if CompartmentStorageType is 'RES'

            return compartment;
        }
    }
}