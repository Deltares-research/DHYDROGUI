using DelftTools.Utils;
using DelftTools.Utils.Data;

namespace DeltaShell.Plugins.DelftModels.WaterQualityModel.DataObjects.BoundaryData
{
    /// <summary>
    /// Represents boundary data specified in delwaq format using files on disk.
    /// </summary>
    public class DataTable : Unique<long>, INameable
    {
        public DataTable()
        {
            Name = string.Empty;
            IsEnabled = true;
        }

        /// <summary>
        /// Indicates whether this instance is enabled and used or not.
        /// </summary>
        public virtual bool IsEnabled { get; set; }

        /// <summary>
        /// Gets or sets the datatable file in delwaq format.
        /// </summary>
        public virtual TextDocumentFromFile DataFile { get; set; }

        /// <summary>
        /// Gets or sets the substance usefors file.
        /// </summary>
        public virtual TextDocumentFromFile SubstanceUseforFile { get; set; }

        /// <summary>
        /// Name of this DataTable
        /// </summary>
        public virtual string Name { get; set; }
    }
}