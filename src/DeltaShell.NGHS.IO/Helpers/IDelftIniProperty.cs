namespace DeltaShell.NGHS.IO.Helpers
{
    public interface IDelftIniProperty
    {
        string Name { get; set; }
        string Value { get; set; }
        string Comment { get; set; }

        /// <summary>
        /// The line where this property was read in the file.
        /// </summary>
        int LineNumber { get; set; }
    }
}