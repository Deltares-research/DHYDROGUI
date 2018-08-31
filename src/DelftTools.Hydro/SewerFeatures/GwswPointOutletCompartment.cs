namespace DelftTools.Hydro.SewerFeatures
{
    public class GwswPointOutletCompartment : OutletCompartment
    {
        protected override void CopyExistingCompartmentPropertyValuesToNewCompartment(Compartment existingCompartment)
        {
            if(existingCompartment is OutletCompartment)
            {
                SurfaceWaterLevel = ((OutletCompartment) existingCompartment).SurfaceWaterLevel;
            }
        }
    }
}
