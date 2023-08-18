using System;
using DelftTools.Utils;
using DelftTools.Utils.Guards;
using DHYDRO.Common.Extensions;

namespace DeltaShell.NGHS.IO.DelftIniObjects
{
    /// <summary>
    /// Representation of a property in a .ini file.
    /// </summary>
    public class DelftIniProperty : INameable
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DelftIniProperty"/> class.
        /// </summary>
        /// <param name="name"> The property name. </param>
        /// <param name="value"> The property value as a <see cref="string"/>. </param>
        /// <param name="comment"> The property comment. </param>
        public DelftIniProperty(string name, string value, string comment)
        {
            Id = name;
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
        /// Initializes a new instance of the <see cref="DelftIniProperty"/> class.
        /// </summary>
        /// <param name="other">The property to copy.</param>
        /// <exception cref="ArgumentNullException">When <paramref name="other"/> is <c>null</c>.</exception>
        public DelftIniProperty(DelftIniProperty other)
        {
            Ensure.NotNull(other, nameof(other));

            Id = other.Id;
            Name = other.Name;
            Value = other.Value;
            Comment = other.Comment;
            LineNumber = other.LineNumber;
        }

        /// <summary>
        /// The category identifier. The default value is the property name.
        /// This value is not written to the INI file.
        /// </summary>
        public string Id { get; set; }
        
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
        /// Determines whether the specified identifier is equal to the current identifier.
        /// </summary>
        /// <param name="id">The identifier to compare with the current identifier.</param>
        /// <returns><c>true</c> if the specified identifier  is equal to the current identifier; otherwise, <c>false</c>.</returns>
        /// <exception cref="ArgumentNullException">When <paramref name="id"/> is <c>null</c>.</exception>
        public bool IdEqualsTo(string id)
        {
            Ensure.NotNull(id, nameof(id));
            return Id.EqualsCaseInsensitive(id);
        }
        
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