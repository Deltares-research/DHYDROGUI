namespace DeltaShell.NGHS.IO.Helpers
{
    /// <summary>
    /// Representation of a property in an .ini file.
    /// </summary>
    public class DelftIniProperty : IDelftIniProperty
    {
        /// <inheritdoc />
        public string Name { get; set; }

        /// <inheritdoc />
        public string Value { get; set; }

        /// <inheritdoc />
        public string Comment { get; set; }

        /// <inheritdoc />
        public int LineNumber { get; set; }

        /// <summary>
        /// Creates a new instance of <see cref="DelftIniProperty"/>.
        /// </summary>
        /// <param name="name"> The property name. </param>
        /// <param name="value"> The property value as a <see cref="string"/>. </param>
        /// <param name="comment"> The property comment. </param>
        public DelftIniProperty(string name, string value, string comment)
        {
            Name = name;
            Value = value;
            Comment = comment;
        }
    }
}