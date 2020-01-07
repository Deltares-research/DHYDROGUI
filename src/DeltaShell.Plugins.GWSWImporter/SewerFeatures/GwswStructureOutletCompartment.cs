using DelftTools.Hydro.SewerFeatures;
using DelftTools.Hydro.Structures;

namespace DeltaShell.Plugins.ImportExport.GWSW.SewerFeatures
{
    public class GwswStructureOutletCompartment : OutletCompartment
    {
        protected override void CopyExistingCompartmentPropertyValuesToNewCompartment(ICompartment existingCompartment)
        {
            ManholeLength = existingCompartment.ManholeLength;
            ManholeWidth = existingCompartment.ManholeWidth;
            Shape = existingCompartment.Shape;
            BottomLevel = existingCompartment.BottomLevel;
            SurfaceLevel = existingCompartment.SurfaceLevel;
            FloodableArea = existingCompartment.FloodableArea;
            Geometry = existingCompartment.Geometry;
        }
    }
}
