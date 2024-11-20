using System.Collections.Generic;

namespace DeltaShell.Plugins.ImportExport.GWSW
{
    /// <summary>
    /// Element with placeholder for GWSWattributes
    /// </summary>
    public class GwswElement
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GwswElement"/> class.
        /// </summary>
        public GwswElement()
        {
            GwswAttributeList = new List<GwswAttribute>();
        }

        /// <summary>
        /// Gets or sets the name of the element type.
        /// </summary>
        /// <value>
        /// The name of the element type.
        /// </value>
        public string ElementTypeName { get; set; }

        /// <summary>
        /// Gets or sets the GWSW attribute list.
        /// </summary>
        /// <value>
        /// The GWSW attribute list.
        /// </value>
        public List<GwswAttribute> GwswAttributeList { get; set; }

    }
}