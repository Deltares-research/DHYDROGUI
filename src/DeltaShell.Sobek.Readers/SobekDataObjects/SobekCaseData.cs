using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Utils.Guards;

namespace DeltaShell.Sobek.Readers.SobekDataObjects
{
    /// <summary>
    /// Data found in the CASEDESC.CMT file of the Sobek case.
    /// </summary>
    public class SobekCaseData
    {
        private readonly FilePaths filePaths;

        /// <summary>
        /// Initializes a new instance of the <see cref="SobekCaseData"/> class.
        /// </summary>
        /// <param name="filePaths"> The sobek file paths. </param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when <paramref name="filePaths"/> is <c>null</c>.
        /// </exception>
        public SobekCaseData(IEnumerable<string> filePaths)
        {
            Ensure.NotNull(filePaths, nameof(filePaths));

            this.filePaths = new FilePaths(filePaths);
            IsEmpty = !filePaths.Any();
        }
        
        /// <summary>
        /// Gets a boolean indicating whether or not this instance contains any data.
        /// </summary>
        public bool IsEmpty { get; }

        /// <summary>
        /// Gets the bui file info.
        /// </summary>
        public FileInfo PrecipitationFile => filePaths.GetByExtension(".bui");

        /// <summary>
        /// Gets the RKS file.
        /// </summary>
        public FileInfo RksFile => filePaths.GetByExtension(".rks");

        /// <summary>
        /// Gets the wind file info.
        /// </summary>
        public FileInfo WindFile => filePaths.GetByExtension(".wdc", ".wnd");

        /// <summary>
        /// Gets the evaporation file.
        /// </summary>
        public FileInfo EvaporationFile => filePaths.GetByExtension(".evp", ".gem", ".plv") ??
                                           filePaths.GetByNameWithoutExtension("evapor");

        /// <summary>
        /// Gets the temperature file info.
        /// </summary>
        public FileInfo TemperatureFile => filePaths.GetByExtension(".tmp");

        /// <summary>
        /// Gets the boundary conditions file info.
        /// </summary>
        public FileInfo BoundaryConditionsFile => filePaths.GetByName("bound3b.3b");

        /// <summary>
        /// Gets the boundary conditions table file info.
        /// </summary>
        public FileInfo BoundaryConditionsTableFile => filePaths.GetByName("bound3b.tbl");
    }
}