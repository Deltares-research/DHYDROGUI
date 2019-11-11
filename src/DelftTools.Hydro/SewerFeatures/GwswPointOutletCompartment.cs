using DelftTools.Hydro.Structures;

namespace DelftTools.Hydro.SewerFeatures
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
