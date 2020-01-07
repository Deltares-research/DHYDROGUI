using DelftTools.Hydro.SewerFeatures;
using DelftTools.Hydro.Structures;

namespace DeltaShell.Plugins.ImportExport.GWSW.Hydro.SewerFeatures
{
    public class GwswPointOutletCompartment : OutletCompartment
    {
        protected override void CopyExistingCompartmentPropertyValuesToNewCompartment(ICompartment existingCompartment)
        {
            if(existingCompartment is OutletCompartment)
            {
                SurfaceWaterLevel = ((OutletCompartment) existingCompartment).SurfaceWaterLevel;
            }
        }
    }
}
