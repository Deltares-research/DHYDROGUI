using DelftTools.Hydro.SewerFeatures;
using DelftTools.Hydro.Structures;
using Deltares.Infrastructure.API.Logging;

namespace DeltaShell.Plugins.ImportExport.GWSW.SewerFeatures
{
    public class GwswPointOutletCompartment : OutletCompartment
    {
        public GwswPointOutletCompartment(ILogHandler logHandler, string name):base(logHandler, name)
        {
        }
        protected override void CopyToExistingCompartmentPropertyValues(ICompartment existingCompartment)
        {
            if(existingCompartment is OutletCompartment compartment)
            {
                SurfaceWaterLevel = compartment.SurfaceWaterLevel;
            }
        }
    }
}
