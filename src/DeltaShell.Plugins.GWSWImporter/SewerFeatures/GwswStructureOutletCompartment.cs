using DelftTools.Hydro.SewerFeatures;
using DelftTools.Hydro.Structures;
using DHYDRO.Common.Logging;

namespace DeltaShell.Plugins.ImportExport.GWSW.SewerFeatures
{
    public class GwswStructureOutletCompartment : OutletCompartment
    {
        public GwswStructureOutletCompartment(ILogHandler logHandler, string name) : base(logHandler, name)
        {
        }
        protected override void CopyToExistingCompartmentPropertyValues(ICompartment existingCompartment)
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
