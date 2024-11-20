using System.IO;

namespace DeltaShell.NGHS.Common
{
    /// <summary>
    /// This class contains default model settings.
    /// </summary>
    public static class DefaultModelSettings
    {
        /// <summary>
        /// Gets the default working directory.
        /// </summary>
        public static string DefaultDeltaShellWorkingDirectory =>
            Path.Combine(Path.GetTempPath(), "DeltaShell_Working_Directory");
    }
}