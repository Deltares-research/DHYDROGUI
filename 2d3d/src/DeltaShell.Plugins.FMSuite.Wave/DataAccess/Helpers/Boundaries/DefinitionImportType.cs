using System.ComponentModel;

namespace DeltaShell.Plugins.FMSuite.Wave.DataAccess.Helpers.Boundaries
{
    /// <summary>
    /// Definition import type of the wave boundary.
    /// </summary>
    public enum DefinitionImportType
    {
        [Description(KnownWaveBoundariesFileConstants.CoordinatesDefinitionType)]
        Coordinates,

        [Description(KnownWaveBoundariesFileConstants.OrientationDefinitionType)]
        Oriented,

        [Description(KnownWaveBoundariesFileConstants.SpectrumFileDefinitionType)]
        SpectrumFile
    }
}