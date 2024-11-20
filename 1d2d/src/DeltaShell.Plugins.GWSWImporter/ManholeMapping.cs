using System.ComponentModel;

namespace DeltaShell.Plugins.ImportExport.GWSW
{
    public static class ManholeMapping
    {
        public enum NodeType
        {
            [Description("CMP")] Compartment,
            [Description("ITP")] InfiltrationPoint /*Should be created as a pipe*/,
            [Description("INS")] InspectionPoint /*Should be created as a pipe*/,
            [Description("PMP")] Pump /*Should be created as a pipe*/,
            [Description("UIT")] Outlet,
            [Description("MAN")] Manhole /* Custom type, only for creating a manhole with no compartiments*/,
        }

        public enum GwswCompartmentStorageType
        {
            [Description("RES")]
            Reservoir,

            [Description("KNV")]
            Closed,

            [Description("VRL")]
            Loss
        }

        public static class PropertyKeys
        {
            public const string ManholeId = "MANHOLE_ID";
            public const string NodeType = "NODE_TYPE";
            public const string UniqueId = "UNIQUE_ID";
            public const string NodeLength = "NODE_LENGTH";
            public const string NodeWidth = "NODE_WIDTH";
            public const string NodeShape = "NODE_SHAPE";
            public const string FloodableArea = "FLOODABLE_AREA";
            public const string BottomLevel = "BOTTOM_LEVEL";
            public const string SurfaceLevel = "SURFACE_LEVEL";
            public const string XCoordinate = "X_COORDINATE";
            public const string YCoordinate = "Y_COORDINATE"; 
            public const string CompartmentStorageType = "SURFACE_SCHEMATISATION";
        }
    }
}