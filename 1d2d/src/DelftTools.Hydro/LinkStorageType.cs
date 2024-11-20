using System.ComponentModel;

namespace DelftTools.Hydro
{
    public enum LinkStorageType
    {
        [Description("1D2D embedded")]
        Embedded = 3,

        [Description("Gully sewer")]
        GullySewer = 5
    }
}