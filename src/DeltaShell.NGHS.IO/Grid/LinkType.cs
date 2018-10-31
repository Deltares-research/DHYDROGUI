using System.ComponentModel;

namespace DeltaShell.NGHS.IO.Grid
{
    public static partial class GridApiDataSet
    {
        public enum LinkType
        {
            [Description("1D2D embedded")]
            Embedded = 3,

            [Description("1D2D lateral")]
            Lateral = -1,

            [Description("Roof sewer")]
            RoofSewer = 7,

            [Description("Inhabitants sewer")]
            InhabitantsSewer = -2,

            [Description("Gully sewer")]
            GullySewer = 5
        }
    }
}