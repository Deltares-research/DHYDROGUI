namespace DeltaShell.NGHS.IO.Helpers
{
    /// <summary>
    /// Interface for representation of a property in a .ini file.
    /// </summary>
    public interface IDelftIniProperty
    {
        /// <summary>
        /// The property name.
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// The property value.
        /// </summary>
        string Value { get; set; }

        /// <summary>
        /// The property comment, describing the property.
        /// </summary>
        string Comment { get; set; }

        /// <summary>
        /// The line where this property was read in the file.
        /// </summary>
        int LineNumber { get; }
    }
}