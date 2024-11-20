namespace DeltaShell.NGHS.IO.FileWriters.Structure.StructureFileNameGenerator
{
    /// <summary>
    /// <see cref="IStructureFileNameGenerator"/> generates names for files.
    /// </summary>
    public interface IStructureFileNameGenerator
    {
        /// <summary>
        /// Generate a valid file name with extension.
        /// </summary>
        /// <returns>A valid file name with extension.</returns>
        string Generate();
    }
}