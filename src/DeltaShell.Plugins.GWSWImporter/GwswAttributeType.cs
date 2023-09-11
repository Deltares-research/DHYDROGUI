using System;
using System.IO;
using DeltaShell.NGHS.IO;
using DeltaShell.Plugins.ImportExport.GWSW.Properties;
using DHYDRO.Common.Logging;

namespace DeltaShell.Plugins.ImportExport.GWSW
{
    /// <summary>
    /// GwswAttributeType with attributes to organize the GWSW import
    /// </summary>
    public class GwswAttributeType
    {
        private readonly ILogHandler logHandler;

        public GwswAttributeType(ILogHandler logHandler)
        {
            this.logHandler = logHandler;
        }
        private string elementName;

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the name of the element.
        /// </summary>
        /// <value>
        /// The name of the element.
        /// </value>
        public string ElementName
        {
            get
            {
                if (elementName == null)
                {
                    return elementName;
                }
                return Path.GetFileNameWithoutExtension(elementName); /*The element names might contain extensions*/
            }
            set { elementName = value; }
        }

        /// <summary>
        /// Gets or sets the key.
        /// </summary>
        /// <value>
        /// The key.
        /// </value>
        public string Key { get; set; }

        /// <summary>
        /// Gets or sets the local key.
        /// </summary>
        /// <value>
        /// The local key.
        /// </value>
        public string LocalKey { get; set; }

        /// <summary>
        /// Gets or sets the definition.
        /// </summary>
        /// <value>
        /// The definition.
        /// </value>
        public string Definition { get; set; }

        /// <summary>
        /// Gets or sets the mandatory.
        /// </summary>
        /// <value>
        /// The mandatory.
        /// </value>
        public string Mandatory { get; set; }

        /// <summary>
        /// Gets or sets the remarks.
        /// </summary>
        /// <value>
        /// The remarks.
        /// </value>
        public string Remarks { get; set; }

        /// <summary>
        /// Gets or sets the name of the file.
        /// </summary>
        /// <value>
        /// The name of the file.
        /// </value>
        public string FileName { get; set; }

        /// <summary>
        /// Gets or sets the type of the attribute.
        /// </summary>
        /// <value>
        /// The type of the attribute.
        /// </value>
        public Type AttributeType { get; set; }

        /// <summary>
        /// Gets or sets the default value.
        /// </summary>
        /// <value>
        /// The default value.
        /// </value>
        public string DefaultValue { get; set; }

        /// <summary>
        /// Tries the type of the get parsed value.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="typeField">The type field.</param>
        /// <param name="definition">The definition.</param>
        /// <param name="fileName">Name of the file.</param>
        /// <param name="lineNumber">The line number.</param>
        /// <returns></returns>
        public Type TryGetParsedValueType(string name, string typeField, string definition, string fileName, int lineNumber)
        {
            try
            {
                return DataTypeValueParser.GetClrType(name, typeField, ref definition, fileName, lineNumber);
            }
            catch (Exception)
            {
                logHandler?.ReportErrorFormat(Resources
                                           .GwswAttributeType_TryGetParsedValueType_The_type_value__0__on_line__1__file__2___could_not_be_parsed__Please_check_it_is_correctly_written_,
                                       name, lineNumber, fileName);
            }

            return null;
        }

        public override string ToString()
        {
            return Key;
        }
    }
}