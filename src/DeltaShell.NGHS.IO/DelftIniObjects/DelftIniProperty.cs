using DelftTools.Utils;

namespace DeltaShell.NGHS.IO.DelftIniObjects
{
    /// <summary>
    /// Representation of a property in a .ini file.
    /// </summary>
    public class DelftIniProperty : INameable
    {
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

        /// <summary>
        /// Creates a new instance of <see cref="DelftIniProperty"/>.
        /// </summary>
        /// <param name="name"> The property name. </param>
        /// <param name="value"> The property value as a <see cref="string"/>. </param>
        /// <param name="comment"> The property comment. </param>
        /// <param name="lineNumber"> The line number. </param>
        public DelftIniProperty(string name, string value, string comment, int lineNumber)
            : this(name, value, comment)
        {
            LineNumber = lineNumber;
        }

        /// <summary>
        /// The property name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The property value.
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// The property comment, describing the property.
        /// </summary>
        public string Comment { get; set; }

        /// <summary>
        /// The line where this property was read in the file.
        /// </summary>
        public int LineNumber { get; }
        
        /// <summary>
        /// Override to add the <seealso cref="Name"/>.
        /// </summary>
        /// <returns>Base.ToString and a the <seealso cref="Name"/> of the <seealso cref="DelftIniProperty"/> </returns>
        public override string ToString()
        {
            return base.ToString() + $" ( {Name} )";
        }
    }
}