namespace DeltaShell.Plugins.FMSuite.Wave.Boundaries
{
    public interface IBoundariesPerFile
    {
        /// <summary>
        /// Boolean property for using a .sp2 file for defining boundaries
        /// for the whole domain or creating boundaries by yourself by using
        /// the boundaries of <see cref="IBoundaryProvider"/>.
        /// </summary>
        bool DefinitionPerFileUsed { get; set; }

        /// <summary>
        /// If a .sp2 file will be used for defining boundaries, this is the file path.
        /// </summary>
        string FilePathForBoundariesPerFile { get; set; }
    }
}