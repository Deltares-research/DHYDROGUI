using System.Collections.Generic;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Concepts.Nwrw
{
    public static class NwrwFileHelper
    {
        /// <summary>
        /// IEnumerable with surface types in the order they are written
        /// and read in NWRW files.
        /// </summary>
        public static readonly IEnumerable<NwrwSurfaceType> SurfaceTypesInCorrectOrder  = new[]
        {
            NwrwSurfaceType.ClosedPavedWithSlope, // a1
            NwrwSurfaceType.ClosedPavedFlat, // a2
            NwrwSurfaceType.ClosedPavedFlatStretch, // a3
            NwrwSurfaceType.OpenPavedWithSlope, // a4
            NwrwSurfaceType.OpenPavedFlat, // a5
            NwrwSurfaceType.OpenPavedFlatStretched, // a6
            NwrwSurfaceType.RoofWithSlope, // a7
            NwrwSurfaceType.RoofFlat, // a8
            NwrwSurfaceType.RoofFlatStretched, // a9
            NwrwSurfaceType.UnpavedWithSlope, // a10
            NwrwSurfaceType.UnpavedFlat, // a11
            NwrwSurfaceType.UnpavedFlatStretched // a12
        };
    }
}