using System.ComponentModel;

namespace DeltaShell.Plugins.FMSuite.Wave.IO.Helpers.Boundaries
{
    /// <summary>
    /// Definition import type of the wave boundary.
    /// </summary>
    public enum DefinitionImportType
    {
        [Description("xy-coordinates")]
        Coordinates,

        [Description("orientation")]
        Oriented
    }
}
