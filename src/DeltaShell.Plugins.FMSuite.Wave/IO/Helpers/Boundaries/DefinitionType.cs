using System.ComponentModel;

namespace DeltaShell.Plugins.FMSuite.Wave.IO.Helpers.Boundaries
{
    /// <summary>
    /// Definition type of the wave boundary.
    /// </summary>
    public enum DefinitionType
    {
        [Description("xy-coordinates")]
        Coordinates,

        [Description("orientation")]
        Oriented
    }
}
