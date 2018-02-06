namespace DelftTools.Hydro
{
    public class OutletCompartment : Compartment
    {
        public OutletCompartment() : this("outletCompartment")
        {
        }

        public OutletCompartment(string uniqueId) : base(uniqueId)
        {
        }

        public double SurfaceWaterLevel { get; set; }
    }
}