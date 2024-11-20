using System.Collections.Generic;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Concepts.Nwrw
{
    public static class NwrwSurfaceTypeHelper
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

        /// <summary>
        /// Dictionary of <see cref="NwrwSurfaceType"/> and the corresponding name
        /// as found in the GWSW files.
        /// </summary>
        public static readonly IDictionary<NwrwSurfaceType, string> SurfaceTypeDictionary =
            new Dictionary<NwrwSurfaceType, string>
            {
                {NwrwSurfaceType.ClosedPavedWithSlope, "GVH_HEL"},
                {NwrwSurfaceType.ClosedPavedFlat, "GVH_VLA"},
                {NwrwSurfaceType.ClosedPavedFlatStretch, "GVH_VLU"},
                {NwrwSurfaceType.OpenPavedWithSlope, "OVH_HEL"},
                {NwrwSurfaceType.OpenPavedFlat, "OVH_VLA"},
                {NwrwSurfaceType.OpenPavedFlatStretched, "OVH_VLU"},
                {NwrwSurfaceType.RoofWithSlope, "DAK_HEL"},
                {NwrwSurfaceType.RoofFlat, "DAK_VLA"},
                {NwrwSurfaceType.RoofFlatStretched, "DAK_VLU"},
                {NwrwSurfaceType.UnpavedWithSlope, "ONV_HEL"},
                {NwrwSurfaceType.UnpavedFlat, "ONV_VLA"},
                {NwrwSurfaceType.UnpavedFlatStretched, "ONV_VLU"}
            };
    }
}