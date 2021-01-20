using System.ComponentModel;

namespace DelftTools.Hydro.Structures
{
    /// <summary>
    /// <see cref="CrestShape"/> defines the possible crest shapes of a weir.
    /// </summary>
    public enum CrestShape
    {
        Sharp,
        Round,
        Triangular,
        Broad,

        [Description("User defined")]
        UserDefined
    }
}