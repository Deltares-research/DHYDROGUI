using System.ComponentModel;

namespace DelftTools.Hydro
{
    public enum LinkType
    {
        [Description("1D2D embedded (1-to-1)")]
        EmbeddedOneToOne = 3,

        [Description("1D2D embedded (1-to-n)")]
        EmbeddedOneToMany = -1,

        [Description("1D2D lateral")]
        Lateral = -2,

        [Description("Gully sewer")]
        GullySewer = 5
    }
}