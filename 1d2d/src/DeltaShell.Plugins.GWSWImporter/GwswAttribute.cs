namespace DeltaShell.Plugins.ImportExport.GWSW
{
    /// <summary>
    /// GwswAttribute
    /// </summary>
    public class GwswAttribute
    {
        private string valueAsString;

        /// <summary>
        /// Gets or sets the type of the GWSW attribute.
        /// </summary>
        /// <value>
        /// The type of the GWSW attribute.
        /// </value>
        public GwswAttributeType GwswAttributeType { get; set; }

        /// <summary>
        /// Gets or sets the line number.
        /// </summary>
        /// <value>
        /// The line number.
        /// </value>
        public int LineNumber { get; set; }

        /// <summary>
        /// Gets or sets the value as string with culture independent double conversion.
        /// </summary>
        /// <value>
        /// The value as string.
        /// </value>
        public string ValueAsString
        {
            get
            {
                return this.IsTypeOf(typeof(double)) ? ReplaceCommaWithPoint(valueAsString) : valueAsString;
            }
            set { valueAsString = value; }
        }

        private static string ReplaceCommaWithPoint(string doubleString)
        {
            return doubleString.Replace(',', '.');
        }

        public override string ToString()
        {
            return GwswAttributeType.Key + ": " + valueAsString;
        }
    }
}