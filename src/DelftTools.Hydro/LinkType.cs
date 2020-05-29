using System.ComponentModel;

namespace DelftTools.Hydro
{
    public enum LinkType
    {
        [Description("1D2D embedded (1-to-1)")]
        EmbeddedOneToOne,

        [Description("1D2D embedded (1-to-n)")]
        EmbeddedOneToMany,

        [Description("1D2D lateral")]
        Lateral,

        [Description("Gully sewer")]
        GullySewer
    }
}